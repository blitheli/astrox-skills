# -*- coding: utf-8 -*-
"""Parse WebAPI JSON outputs for 2016 HO3 mission analysis."""
from __future__ import annotations

import json
import math
from datetime import datetime, timezone
from pathlib import Path

MU_EARTH = 3.986004418e14  # m^3/s^2
RP_M = 6378137.0 + 200_000.0  # 200 km altitude perigee
VREF_MPS = 10_960.0  # 10.96 km/s circular reference subtraction per task


def parse_cartesian_samples(
    position_obj: dict,
) -> tuple[float, list[tuple[float, tuple[float, float, float]]]]:
    """Return (offset0_unix, [(dt_seconds_from_epoch, (x,y,z)), ...])."""
    epoch_s = position_obj["epoch"].replace("Z", "+00:00")
    epoch_unix = datetime.fromisoformat(epoch_s).timestamp()
    arr = position_obj["cartesianVelocity"]
    out: list[tuple[float, tuple[float, float, float]]] = []
    i = 0
    n = len(arr)
    while i < n:
        dt = arr[i]
        x, y, z = arr[i + 1], arr[i + 2], arr[i + 3]
        out.append((float(dt), (float(x), float(y), float(z))))
        i += 7
    return float(epoch_unix), out


def interp_pos(
    samples: list[tuple[float, tuple[float, float, float]]], t_query: float
) -> tuple[float, float, float] | None:
    """Linear interpolation of position at offset t_query (seconds from series epoch)."""
    if not samples:
        return None
    if t_query <= samples[0][0]:
        return samples[0][1]
    if t_query >= samples[-1][0]:
        return samples[-1][1]
    j = 0
    while j + 1 < len(samples) and samples[j + 1][0] < t_query:
        j += 1
    t0, p0 = samples[j]
    t1, p1 = samples[j + 1]
    if t1 <= t0:
        return p0
    w = (t_query - t0) / (t1 - t0)
    return (
        p0[0] + w * (p1[0] - p0[0]),
        p0[1] + w * (p1[1] - p0[1]),
        p0[2] + w * (p1[2] - p0[2]),
    )


def earth_departure_dv_mps(dv1_mag_mps: float) -> float:
    """Hyperbolic excess = DV1_Mag -> perigee speed at 200 km, minus 10.96 km/s."""
    v_inf = dv1_mag_mps
    v_p = math.sqrt(v_inf * v_inf + 2.0 * MU_EARTH / RP_M)
    return v_p - VREF_MPS


def angle_deg_r_dv(r: tuple[float, float, float], dv: list[float]) -> float:
    """Angle between position vector r and DeltaV2 (arrival impulse), degrees."""
    rx, ry, rz = r
    vx, vy, vz = float(dv[0]), float(dv[1]), float(dv[2])
    cr = math.sqrt(rx * rx + ry * ry + rz * rz)
    cv = math.sqrt(vx * vx + vy * vy + vz * vz)
    if cr < 1e-6 or cv < 1e-6:
        return float("nan")
    c = (rx * vx + ry * vy + rz * vz) / (cr * cv)
    c = max(-1.0, min(1.0, c))
    return math.degrees(math.acos(c))


def iso_to_dt(s: str) -> datetime:
    s = s.replace("Z", "+00:00")
    return datetime.fromisoformat(s)


def main() -> None:
    base = Path(__file__).resolve().parent
    mpc_path = base / "mpc-2016HO3-response.json"
    eph_path = base / "ephemeris-earth-2029-response.json"
    tr_path = base / "transfer-earth-2016HO3-response.json"

    with mpc_path.open(encoding="utf-8-sig") as f:
        mpc = json.load(f)
    with eph_path.open(encoding="utf-8-sig") as f:
        eph = json.load(f)
    with tr_path.open(encoding="utf-8-sig") as f:
        tr = json.load(f)

    ast_epoch_u, ast_samples = parse_cartesian_samples(mpc["Position"])
    earth_epoch_u, earth_samples = parse_cartesian_samples(eph["Position"])

    dist_rows: list[tuple[str, float]] = []
    for dt_e, ep in earth_samples:
        if dt_e < 0:
            continue
        abs_u = earth_epoch_u + dt_e
        dt_a = abs_u - ast_epoch_u
        ap = interp_pos(ast_samples, dt_a)
        if ap is None:
            continue
        dx = ep[0] - ap[0]
        dy = ep[1] - ap[1]
        dz = ep[2] - ap[2]
        dist_km = math.sqrt(dx * dx + dy * dy + dz * dz) / 1000.0
        dist_rows.append((datetime.fromtimestamp(abs_u, tz=timezone.utc).strftime("%Y-%m-%d"), dist_km))

    dmin = min(dist_rows, key=lambda x: x[1])
    dmax = max(dist_rows, key=lambda x: x[1])

    results = tr["TransferResults"]
    enriched = []
    for row in results:
        r = row["RV2"]
        pos = (r[0], r[1], r[2])
        illum = angle_deg_r_dv(pos, row["DeltaV2"])
        d1 = float(row["DV1_Mag"])
        d2 = float(row["DV2_Mag"])
        dv_earth = earth_departure_dv_mps(d1)
        dtot = dv_earth + d2
        enriched.append(
            {
                **row,
                "IlluminationAngleDeg": illum,
                "EarthDepartureDv_mps": dv_earth,
                "DvTotal_mps": dtot,
            }
        )

    min_dv1 = min(enriched, key=lambda x: x["DV1_Mag"])
    min_dv_earth = min(enriched, key=lambda x: x["EarthDepartureDv_mps"])

    # Impactor: illum < 80, DV2 > 5000 m/s
    impactor_cand = [
        x
        for x in enriched
        if x["IlluminationAngleDeg"] < 80.0 and x["DV2_Mag"] > 5000.0
    ]

    # Observer: dV_total <= 2000 m/s (task); likely empty — also record <= 20000
    obs_2k = [x for x in enriched if x["DvTotal_mps"] <= 2000.0]
    obs_20k = [x for x in enriched if x["DvTotal_mps"] <= 20000.0]

    # Pair: pick best impactor by min EarthDepartureDv, then observer 90+ days earlier
    impactor_cand.sort(key=lambda x: x["EarthDepartureDv_mps"])
    pairs_relaxed = []
    for imp in impactor_cand:
        t_imp = iso_to_dt(imp["ArrivalTime"])
        best_ob = None
        for ob in obs_20k:
            t_ob = iso_to_dt(ob["ArrivalTime"])
            if (t_imp - t_ob).total_seconds() >= 90 * 86400:
                if best_ob is None or ob["DvTotal_mps"] < best_ob["DvTotal_mps"]:
                    best_ob = ob
        if best_ob is not None:
            pairs_relaxed.append({"impactor": imp, "observer": best_ob})

    pairs_relaxed.sort(key=lambda p: p["impactor"]["EarthDepartureDv_mps"])

    pairs_strict = []
    for imp in impactor_cand:
        t_imp = iso_to_dt(imp["ArrivalTime"])
        best_ob = None
        for ob in obs_2k:
            t_ob = iso_to_dt(ob["ArrivalTime"])
            if (t_imp - t_ob).total_seconds() >= 90 * 86400:
                if best_ob is None or ob["DvTotal_mps"] < best_ob["DvTotal_mps"]:
                    best_ob = ob
        if best_ob is not None:
            pairs_strict.append({"impactor": imp, "observer": best_ob})

    out_summary = {
        "distance_2029_au_summary_km": {
            "min_km": dmin[1],
            "min_epoch_seconds_tag": dmin[0],
            "max_km": dmax[1],
            "max_epoch_seconds_tag": dmax[0],
            "num_earth_samples": len(dist_rows),
        },
        "transfer_grid": {
            "count": len(enriched),
            "min_DV1_Mag_mps": min_dv1["DV1_Mag"],
            "at_min_DV1": {
                "DepartureTime": min_dv1["DepartureTime"],
                "ArrivalTime": min_dv1["ArrivalTime"],
            },
            "min_EarthDepartureDv_mps": min_dv_earth["EarthDepartureDv_mps"],
            "at_min_EarthDepartureDv": {
                "DepartureTime": min_dv_earth["DepartureTime"],
                "ArrivalTime": min_dv_earth["ArrivalTime"],
                "DV1_Mag": min_dv_earth["DV1_Mag"],
                "DV2_Mag": min_dv_earth["DV2_Mag"],
                "IlluminationAngleDeg": min_dv_earth["IlluminationAngleDeg"],
            },
            "impactor_candidates_illum80_DV2gt5000": len(impactor_cand),
            "observer_dVtotal_le_2000_mps": len(obs_2k),
            "observer_dVtotal_le_20000_mps": len(obs_20k),
            "pairs_impactor_observer_90d_dVtotal_le_20000_mps": len(pairs_relaxed),
            "pairs_impactor_observer_90d_dVtotal_le_2000_mps": len(pairs_strict),
        },
    }

    with (base / "earth-asteroid-distance-2029.csv").open("w", encoding="utf-8") as f:
        f.write("date_utc,distance_km\n")
        for d, km in sorted(dist_rows, key=lambda x: x[0]):
            f.write(f"{d},{km:.6f}\n")

    with (base / "analysis-summary.json").open("w", encoding="utf-8") as f:
        json.dump(out_summary, f, indent=2, ensure_ascii=False)

    # Top 20 impactor candidates
    top_imp = sorted(impactor_cand, key=lambda x: x["EarthDepartureDv_mps"])[:20]
    with (base / "impactor-candidates-top20.json").open("w", encoding="utf-8") as f:
        json.dump(top_imp, f, indent=2, ensure_ascii=False)

    min_tot = min(enriched, key=lambda x: x["DvTotal_mps"])
    with (base / "observer-min-dvtotal-context.json").open("w", encoding="utf-8") as f:
        json.dump(
            {
                "global_minimum_DvTotal_mps": min_tot["DvTotal_mps"],
                "case": min_tot,
            },
            f,
            indent=2,
            ensure_ascii=False,
        )

    with (base / "pairs-observer-20kms.json").open("w", encoding="utf-8") as f:
        json.dump(pairs_relaxed[:30], f, indent=2, ensure_ascii=False)

    with (base / "pairs-observer-2kms.json").open("w", encoding="utf-8") as f:
        json.dump(pairs_strict, f, indent=2, ensure_ascii=False)

    print(json.dumps(out_summary, indent=2, ensure_ascii=False))


if __name__ == "__main__":
    main()
