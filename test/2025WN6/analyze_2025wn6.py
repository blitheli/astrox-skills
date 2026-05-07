# -*- coding: utf-8 -*-
"""Post-process /celestial/transfer results for 2025 WN6 mission constraints."""
from __future__ import annotations

import json
import math
from datetime import datetime
from pathlib import Path

MU_EARTH = 3.986004418e14
RP_M = 6378137.0 + 200_000.0
VREF_MPS = 10_960.0
# 撞击器：任务书「地球出发 dV 不超过 1.1 km/s」（第 6 节换算量，单位 m/s）
IMPACTOR_MAX_EARTH_DEPARTURE_DV_MPS = 1100.0


def earth_departure_dv_mps(dv1_mag_mps: float) -> float:
    v_inf = dv1_mag_mps
    v_p = math.sqrt(v_inf * v_inf + 2.0 * MU_EARTH / RP_M)
    return v_p - VREF_MPS


def angle_deg_r_dv(r: tuple[float, float, float], dv: list[float]) -> float:
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
    return datetime.fromisoformat(s.replace("Z", "+00:00"))


def parse_cartesian_samples(
    position_obj: dict,
) -> tuple[float, list[tuple[float, tuple[float, float, float]]]]:
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


def earth_asteroid_distance_km(
    arrival_iso: str,
    earth_epoch_u: float,
    earth_samples: list[tuple[float, tuple[float, float, float]]],
    ast_epoch_u: float,
    ast_samples: list[tuple[float, tuple[float, float, float]]],
) -> float:
    """Sun-frame MEANECLPJ2000: |r_Earth - r_asteroid| at arrival UTC."""
    abs_u = iso_to_dt(arrival_iso).timestamp()
    dt_e = abs_u - earth_epoch_u
    dt_a = abs_u - ast_epoch_u
    pe = interp_pos(earth_samples, dt_e)
    pa = interp_pos(ast_samples, dt_a)
    if pe is None or pa is None:
        return float("nan")
    dx = pe[0] - pa[0]
    dy = pe[1] - pa[1]
    dz = pe[2] - pa[2]
    return math.sqrt(dx * dx + dy * dy + dz * dz) / 1000.0


def main() -> None:
    base = Path(__file__).resolve().parent
    tr_path = base / "transfer-earth-2025WN6-response.json"
    mpc_path = base / "mpc-2025WN6-response.json"
    eph_path = base / "ephemeris-earth-202806-202912-response.json"

    with tr_path.open(encoding="utf-8-sig") as f:
        tr = json.load(f)
    with mpc_path.open(encoding="utf-8-sig") as f:
        mpc = json.load(f)
    with eph_path.open(encoding="utf-8-sig") as f:
        eph = json.load(f)

    ast_epoch_u, ast_samples = parse_cartesian_samples(mpc["Position"])
    earth_epoch_u, earth_samples = parse_cartesian_samples(eph["Position"])

    results = tr["TransferResults"]
    enriched = []
    for row in results:
        r = row["RV2"]
        pos = (r[0], r[1], r[2])
        illum = angle_deg_r_dv(pos, row["DeltaV2"])
        d1 = float(row["DV1_Mag"])
        d2 = float(row["DV2_Mag"])
        dv_earth = earth_departure_dv_mps(d1)
        dist_km = earth_asteroid_distance_km(
            row["ArrivalTime"], earth_epoch_u, earth_samples, ast_epoch_u, ast_samples
        )
        enriched.append(
            {
                **row,
                "IlluminationAngleDeg": illum,
                "EarthDepartureDv_mps": dv_earth,
                "DvTotal_mps": dv_earth + d2,
                "EarthAsteroidDistanceAtArrival_km": dist_km,
            }
        )

    min_dv1 = min(enriched, key=lambda x: x["DV1_Mag"])
    min_dv_earth = min(enriched, key=lambda x: x["EarthDepartureDv_mps"])
    min_tot = min(enriched, key=lambda x: x["DvTotal_mps"])

    impactor_cand_loose = [
        x
        for x in enriched
        if x["IlluminationAngleDeg"] < 80.0 and x["DV2_Mag"] > 5000.0
    ]
    impactor_cand = [
        x
        for x in impactor_cand_loose
        if x["EarthDepartureDv_mps"] <= IMPACTOR_MAX_EARTH_DEPARTURE_DV_MPS
    ]
    # 撞击器：距离尽量小（主排序），地球出发 Δv 次之
    impactor_cand.sort(
        key=lambda x: (x["EarthAsteroidDistanceAtArrival_km"], x["EarthDepartureDv_mps"])
    )

    obs_2k = [x for x in enriched if x["DvTotal_mps"] <= 2000.0]
    obs_20k = [x for x in enriched if x["DvTotal_mps"] <= 20000.0]

    min_dist_imp = None
    if impactor_cand:
        min_dist_imp = min(impactor_cand, key=lambda x: x["EarthAsteroidDistanceAtArrival_km"])

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

    pairs_relaxed.sort(
        key=lambda p: (
            p["impactor"]["EarthAsteroidDistanceAtArrival_km"],
            p["observer"]["DvTotal_mps"],
        )
    )

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

    pairs_strict.sort(
        key=lambda p: (
            p["impactor"]["EarthAsteroidDistanceAtArrival_km"],
            p["observer"]["DvTotal_mps"],
        )
    )

    out_summary = {
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
                "EarthAsteroidDistanceAtArrival_km": min_dv_earth[
                    "EarthAsteroidDistanceAtArrival_km"
                ],
            },
            "global_minimum_DvTotal_mps": min_tot["DvTotal_mps"],
            "at_global_minimum_DvTotal": {
                "DepartureTime": min_tot["DepartureTime"],
                "ArrivalTime": min_tot["ArrivalTime"],
                "DV1_Mag": min_tot["DV1_Mag"],
                "DV2_Mag": min_tot["DV2_Mag"],
                "IlluminationAngleDeg": min_tot["IlluminationAngleDeg"],
                "EarthAsteroidDistanceAtArrival_km": min_tot[
                    "EarthAsteroidDistanceAtArrival_km"
                ],
            },
            "impactor_candidates_illum80_DV2gt5000_only": len(impactor_cand_loose),
            "impactor_candidates_including_earth_dv_le_1100_mps": len(impactor_cand),
            "impactor_max_earth_departure_dv_mps": IMPACTOR_MAX_EARTH_DEPARTURE_DV_MPS,
            "impactor_min_EarthAsteroidDistanceAtArrival_km": (
                min_dist_imp["EarthAsteroidDistanceAtArrival_km"] if min_dist_imp else None
            ),
            "at_impactor_min_arrival_distance": (
                {
                    "DepartureTime": min_dist_imp["DepartureTime"],
                    "ArrivalTime": min_dist_imp["ArrivalTime"],
                    "DV1_Mag": min_dist_imp["DV1_Mag"],
                    "DV2_Mag": min_dist_imp["DV2_Mag"],
                    "IlluminationAngleDeg": min_dist_imp["IlluminationAngleDeg"],
                    "EarthDepartureDv_mps": min_dist_imp["EarthDepartureDv_mps"],
                    "EarthAsteroidDistanceAtArrival_km": min_dist_imp[
                        "EarthAsteroidDistanceAtArrival_km"
                    ],
                }
                if min_dist_imp
                else None
            ),
            "observer_dVtotal_le_2000_mps": len(obs_2k),
            "observer_dVtotal_le_20000_mps": len(obs_20k),
            "pairs_impactor_observer_90d_dVtotal_le_20000_mps": len(pairs_relaxed),
            "pairs_impactor_observer_90d_dVtotal_le_2000_mps": len(pairs_strict),
        },
    }

    with (base / "analysis-summary.json").open("w", encoding="utf-8") as f:
        json.dump(out_summary, f, indent=2, ensure_ascii=False)

    top_imp = impactor_cand[:20]
    with (base / "impactor-candidates-top20.json").open("w", encoding="utf-8") as f:
        json.dump(top_imp, f, indent=2, ensure_ascii=False)

    with (base / "pairs-observer-20kms.json").open("w", encoding="utf-8") as f:
        json.dump(pairs_relaxed[:30], f, indent=2, ensure_ascii=False)

    with (base / "pairs-observer-2kms.json").open("w", encoding="utf-8") as f:
        json.dump(pairs_strict, f, indent=2, ensure_ascii=False)

    print(json.dumps(out_summary, indent=2, ensure_ascii=False))


if __name__ == "__main__":
    main()
