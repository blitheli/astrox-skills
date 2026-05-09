"""
2015 MB54 双飞行器约束分析（依据 test/2015MB54/2015MB54要求.md）。

- POST /celestial/mpc
- POST /celestial/transfer
- POST /celestial/ephemeris（地球，用于到达日地-星距离）
"""

from __future__ import annotations

import json
import math
import urllib.request
from dataclasses import dataclass
from datetime import datetime, timedelta, timezone
from pathlib import Path

BASE_URL = "http://astrox.cn:8765"
OUT_DIR = Path(__file__).resolve().parent

ASTEROID = "2015 MB54"
FILE_SLUG = "2015-MB54"

DEPARTURE_INTERVAL = "2028-10-01T00:00:00Z/2033-05-31T00:00:00Z"
ARRIVAL_INTERVAL = "2029-05-01T00:00:00Z/2033-08-31T00:00:00Z"
STEP_DAYS = 5
MIN_TOF_DAYS = 10

MU_EARTH_M3_S2 = 3.986004415e14
RP_EARTH_200KM_M = 6_378_137.0 + 200_000.0
V_REF_MPS = 10_960.0  # 10.96 km/s

IMPACTOR_ANGLE_MAX_DEG = 80.0
IMPACTOR_DV2_MIN_MPS = 5_000.0  # 5 km/s
IMPACTOR_DIST_MAX_KM = 20_000_000.0  # 2000 万 km（文档）
IMPACTOR_DEP_DV_MAX_MPS = 1_100.0  # 1.1 km/s

OBSERVER_DV_TOTAL_MAX_MPS = 2_000.0  # |dV| + DV2_Mag ≤ 2 km/s
MIN_GAP_DAYS = 90

SUN_FRAME = "MeanEclpJ2000"

EARTH_EPHEM_START = "2029-05-01T00:00:00Z"
EARTH_EPHEM_STOP = "2033-08-31T00:00:00Z"
EARTH_EPHEM_STEP = 86400


def save_json(path: Path, data: object) -> None:
    path.write_text(json.dumps(data, ensure_ascii=False, indent=2), encoding="utf-8")


def post_json(endpoint: str, payload: dict, timeout: int = 600) -> dict:
    req = urllib.request.Request(
        f"{BASE_URL}{endpoint}",
        data=json.dumps(payload).encode("utf-8"),
        headers={"Content-Type": "application/json"},
        method="POST",
    )
    with urllib.request.urlopen(req, timeout=timeout) as response:
        return json.loads(response.read().decode("utf-8"))


def parse_utc(s: str) -> datetime:
    return datetime.fromisoformat(s.replace("Z", "+00:00"))


def iso_utc(dt: datetime) -> str:
    if dt.tzinfo is None:
        dt = dt.replace(tzinfo=timezone.utc)
    return dt.astimezone(timezone.utc).strftime("%Y-%m-%dT%H:%M:%SZ")


def norm3(v: list[float]) -> float:
    return math.sqrt(v[0] ** 2 + v[1] ** 2 + v[2] ** 2)


def angle_deg_vec(a: list[float], b: list[float]) -> float:
    na, nb = norm3(a), norm3(b)
    if na < 1e-30 or nb < 1e-30:
        return float("nan")
    c = sum(a[i] * b[i] for i in range(3)) / (na * nb)
    c = max(-1.0, min(1.0, c))
    return math.degrees(math.acos(c))


def departure_dv_metric_mps(dv1_mag_mps: float) -> float:
    v_inf = dv1_mag_mps
    v_peri = math.sqrt(max(0.0, v_inf * v_inf + 2.0 * MU_EARTH_M3_S2 / RP_EARTH_200KM_M))
    return v_peri - V_REF_MPS


def mpc_orbit_to_transfer_elements(orbit: dict) -> dict:
    keys = [
        "EpochMjdTdt",
        "SemimajorAxis",
        "Eccentricity",
        "Inclination",
        "Raan",
        "ArgOfPeriapsis",
        "MeanAnomaly",
        "PeriTimeMjdTdt",
        "Q",
    ]
    el = {k: orbit[k] for k in keys if k in orbit}
    au = 149597870700.0
    if "SemimajorAxis" in el and float(el["SemimajorAxis"]) > 1e7:
        el["SemimajorAxis"] = float(el["SemimajorAxis"]) / au
    return el


def state_series(position: dict) -> list[dict]:
    epoch_dt = parse_utc(position["epoch"])
    data = position["cartesianVelocity"]
    out: list[dict] = []
    for i in range(0, len(data), 7):
        off = float(data[i])
        rv = data[i + 1 : i + 7]
        t = epoch_dt + timedelta(seconds=off)
        out.append({"time": iso_utc(t), "time_dt": t, "rv": rv})
    return out


def nearest_earth_r(
    earth_by_day: dict[str, list[float]], target: datetime
) -> list[float] | None:
    key = target.strftime("%Y-%m-%d")
    if key in earth_by_day:
        return earth_by_day[key][:3]
    best_key = None
    best_d = float("inf")
    for k in earth_by_day:
        kt = parse_utc(k + "T12:00:00Z")
        d = abs((kt - target).total_seconds())
        if d < best_d:
            best_d = d
            best_key = k
    if best_key is None or best_d > 86400 * 1.5:
        return None
    return earth_by_day[best_key][:3]


@dataclass
class Cand:
    raw: dict
    departure: datetime
    arrival: datetime
    illum_deg: float
    dep_dv_mps: float
    dist_km: float
    dv2_mag: float


def parse_transfer_row(row: dict, earth_by_day: dict[str, list[float]]) -> Cand | None:
    dep = parse_utc(row["DepartureTime"])
    arr = parse_utc(row["ArrivalTime"])
    rv2 = row["RV2"]
    r_ast = rv2[:3]
    r_e = nearest_earth_r(earth_by_day, arr)
    if r_e is None:
        return None
    dist_km = norm3([r_e[i] - r_ast[i] for i in range(3)]) / 1000.0
    illum = angle_deg_vec(r_ast, row["DeltaV2"])
    dep_dv = departure_dv_metric_mps(float(row["DV1_Mag"]))
    return Cand(
        raw=row,
        departure=dep,
        arrival=arr,
        illum_deg=illum,
        dep_dv_mps=dep_dv,
        dist_km=dist_km,
        dv2_mag=float(row["DV2_Mag"]),
    )


def main() -> None:
    mpc_in = {
        "TargetName": ASTEROID,
        "ObserverName": "Sun",
        "ObserverFrame": "MEANECLPJ2000",
        "Start": "",
        "Stop": ARRIVAL_INTERVAL.split("/")[1],
    }
    save_json(OUT_DIR / f"mpc-{FILE_SLUG}-input.json", mpc_in)
    mpc_out = post_json("/celestial/mpc", mpc_in, timeout=120)
    save_json(OUT_DIR / f"mpc-{FILE_SLUG}-output.json", mpc_out)
    if not mpc_out.get("IsSuccess"):
        raise RuntimeError(mpc_out.get("Message", "MPC failed"))

    arrival_el = mpc_orbit_to_transfer_elements(mpc_out["OrbitElements"])

    xfer_in = {
        "SunFrameName": SUN_FRAME,
        "DepartureCbName": "Earth",
        "ArrivalCbName": ASTEROID,
        "DepartureInterval": DEPARTURE_INTERVAL,
        "ArrivalInterval": ARRIVAL_INTERVAL,
        "MinTofDays": MIN_TOF_DAYS,
        "DepartureStepDay": STEP_DAYS,
        "ArrivalStepDay": STEP_DAYS,
        "ArrivalElements": arrival_el,
    }
    save_json(OUT_DIR / "celestial-transfer-input.json", xfer_in)
    xfer_out = post_json("/celestial/transfer", xfer_in, timeout=900)
    save_json(OUT_DIR / "celestial-transfer-output.json", xfer_out)
    if not xfer_out.get("IsSuccess"):
        raise RuntimeError(xfer_out.get("Message", "transfer failed"))

    earth_in = {
        "TargetName": "Earth",
        "ObserverName": "Sun",
        "ObserverFrame": "MEANECLPJ2000",
        "Start": EARTH_EPHEM_START,
        "Stop": EARTH_EPHEM_STOP,
        "Step": EARTH_EPHEM_STEP,
    }
    save_json(OUT_DIR / "earth-ephemeris-input.json", earth_in)
    earth_out = post_json("/celestial/ephemeris", earth_in, timeout=300)
    save_json(OUT_DIR / "earth-ephemeris-output.json", earth_out)
    if not earth_out.get("IsSuccess"):
        raise RuntimeError(earth_out.get("Message", "Earth ephemeris failed"))

    earth_by_day: dict[str, list[float]] = {}
    for s in state_series(earth_out["Position"]):
        earth_by_day[s["time"][:10]] = s["rv"]

    results = xfer_out.get("TransferResults") or []
    enriched: list[Cand] = []
    for row in results:
        c = parse_transfer_row(row, earth_by_day)
        if c:
            enriched.append(c)

    save_json(
        OUT_DIR / "analysis-enriched-count.json",
        {"transfer_rows": len(results), "enriched_ok": len(enriched)},
    )

    impactors: list[Cand] = []
    observers: list[Cand] = []
    n_imp_illum = n_imp_dv2 = n_imp_dist = n_imp_dep = 0
    min_dep_among_triple: float | None = None
    for c in enriched:
        dd = abs(c.dep_dv_mps)
        ok_illum = c.illum_deg < IMPACTOR_ANGLE_MAX_DEG
        ok_dv2 = ok_illum and c.dv2_mag > IMPACTOR_DV2_MIN_MPS
        ok_dist = ok_dv2 and c.dist_km < IMPACTOR_DIST_MAX_KM
        if ok_illum:
            n_imp_illum += 1
        if ok_dv2:
            n_imp_dv2 += 1
        if ok_dist:
            n_imp_dist += 1
            if min_dep_among_triple is None or dd < min_dep_among_triple:
                min_dep_among_triple = dd
        if ok_dist and dd <= IMPACTOR_DEP_DV_MAX_MPS:
            n_imp_dep += 1
            impactors.append(c)
        obs_cost = dd + c.dv2_mag
        if obs_cost <= OBSERVER_DV_TOTAL_MAX_MPS:
            observers.append(c)

    pairs: list[tuple[Cand, Cand]] = []
    for obs in observers:
        for imp in impactors:
            if obs.arrival <= imp.arrival - timedelta(days=MIN_GAP_DAYS):
                pairs.append((obs, imp))

    diag: dict = {}
    if impactors and observers:
        imp_ts = sorted(c.arrival for c in impactors)
        obs_ts = sorted(c.arrival for c in observers)
        pairs_gap0 = sum(
            1 for o in observers for i in impactors if o.arrival < i.arrival
        )
        diag = {
            "impactor_arrival_utc_min": iso_utc(imp_ts[0]),
            "impactor_arrival_utc_max": iso_utc(imp_ts[-1]),
            "observer_arrival_utc_min": iso_utc(obs_ts[0]),
            "observer_arrival_utc_max": iso_utc(obs_ts[-1]),
            "pairs_observer_before_impactor_gap0_days": pairs_gap0,
            "note": "若要求观测器早于撞击器 ≥90 天，需 obs.arrival ≤ imp.arrival - 90d",
        }
        save_json(OUT_DIR / "analysis-timing-diagnostic.json", diag)
    else:
        save_json(OUT_DIR / "analysis-timing-diagnostic.json", diag)

    summary = {
        "asteroid": ASTEROID,
        "requirements_doc": "2015MB54要求.md",
        "impactor_dist_max_km": IMPACTOR_DIST_MAX_KM,
        "impactor_candidates": len(impactors),
        "observer_candidates": len(observers),
        "pairs_observer_before_impactor_90d": len(pairs),
        "impactor_breakdown": {
            "illum_lt_80_deg": n_imp_illum,
            "plus_DV2_gt_5000_mps": n_imp_dv2,
            "plus_dist_under_threshold_km": n_imp_dist,
            "plus_dep_abs_le_1100_mps": n_imp_dep,
            "min_abs_dep_mps_among_last_three_only": min_dep_among_triple,
        },
    }
    save_json(OUT_DIR / "analysis-filter-summary.json", summary)

    pair_records = []
    for obs, imp in pairs:
        pair_records.append(
            {
                "observer_departure": iso_utc(obs.departure),
                "observer_arrival": iso_utc(obs.arrival),
                "observer_DV1_Mag": obs.raw["DV1_Mag"],
                "observer_DV2_Mag": obs.raw["DV2_Mag"],
                "observer_dep_dv_metric_mps": obs.dep_dv_mps,
                "observer_illum_deg": obs.illum_deg,
                "observer_dist_km": obs.dist_km,
                "observer_dV_total_mps": abs(obs.dep_dv_mps) + obs.dv2_mag,
                "impactor_departure": iso_utc(imp.departure),
                "impactor_arrival": iso_utc(imp.arrival),
                "impactor_DV1_Mag": imp.raw["DV1_Mag"],
                "impactor_DV2_Mag": imp.raw["DV2_Mag"],
                "impactor_dep_dv_metric_mps": imp.dep_dv_mps,
                "impactor_illum_deg": imp.illum_deg,
                "impactor_dist_km": imp.dist_km,
                "gap_days": (imp.arrival - obs.arrival).total_seconds() / 86400.0,
            }
        )
    save_json(OUT_DIR / "analysis-pairs.json", pair_records)

    lines = [
        f"# 任务分析结果-{ASTEROID}",
        "",
        "## 1. 输入与接口",
        "",
        "- 约束依据: `2015MB54要求.md`。地球出发至 **2033-05**，小行星到达至 **2033-08**；撞击器地-星距离 **2000 万 km**（`IMPACTOR_DIST_MAX_KM`）。",
        "",
        f"- 地球出发搜索区间: `{DEPARTURE_INTERVAL}`",
        f"- 小行星到达搜索区间: `{ARRIVAL_INTERVAL}`",
        f"- Lambert 网格步长: {STEP_DAYS} 天（出发/到达相同）",
        "- 转移请求与响应: `celestial-transfer-input.json`, `celestial-transfer-output.json`",
        f"- MPC: `mpc-{FILE_SLUG}-input.json`, `mpc-{FILE_SLUG}-output.json`",
        "- 地球星历（到达日地-星距离）: `earth-ephemeris-input.json`, `earth-ephemeris-output.json`",
        "",
        "## 2. 物理量定义",
        "",
        "- **到达光照角**: `RV2` 前 3 元（小行星日心位置）与 `DeltaV2` 的夹角（0°–180°）。",
        "- **地球出发 dV**: `DV1_Mag` 视为双曲超速 v∞，近地点半径 200 km，",
        "  vp = sqrt(v∞² + 2μ/rp)，再令 `dV = vp - 10.96 km/s`（单位 m/s）。",
        "- **到达时地-星距离**: 地球与小行星日心位置矢量差（地球星历按到达日历日对齐）。",
        "",
        "## 3. 飞行器约束",
        "",
        "**撞击器**",
        f"- 光照角 < {IMPACTOR_ANGLE_MAX_DEG:g}°",
        f"- `DV2_Mag` > {IMPACTOR_DV2_MIN_MPS/1000:g} km/s",
        f"- 到达时地-星距离 < {IMPACTOR_DIST_MAX_KM/1e4:.0f} 万 km",
        f"- `|dV|` ≤ {IMPACTOR_DEP_DV_MAX_MPS/1000:g} km/s",
        "",
        "**观测器**",
        f"- `|dV| + DV2_Mag` ≤ {OBSERVER_DV_TOTAL_MAX_MPS/1000:g} km/s",
        f"- 到达比撞击器早 ≥ {MIN_GAP_DAYS} 天",
        "",
        "## 4. 筛选统计",
        "",
        json.dumps(summary, ensure_ascii=False, indent=2),
        "",
        "## 5. 同时满足双飞行器与间隔约束的解",
        "",
    ]

    if pair_records:
        lines.extend(
            [
                "| 观测器出发 | 观测器到达 | 观测器光照角(°) | 观测器 dV_total(m/s) | 撞击器出发 | 撞击器到达 | 撞击器光照角(°) | 间隔(天) |",
                "| --- | --- | ---: | ---: | --- | --- | ---: | ---: |",
            ]
        )
        for p in sorted(pair_records, key=lambda x: x["observer_arrival"]):
            lines.append(
                f"| {p['observer_departure']} | {p['observer_arrival']} | {p['observer_illum_deg']:.3f} | "
                f"{p['observer_dV_total_mps']:.3f} | {p['impactor_departure']} | {p['impactor_arrival']} | "
                f"{p['impactor_illum_deg']:.3f} | {p['gap_days']:.1f} |"
            )
    else:
        lines.extend(
            [
                "在给定搜索网格下 **未找到** 同时满足撞击器、观测器及 90 天间隔的轨迹对。",
                "",
                "### 5.1 原因分析（中间过程）",
                "",
                "对 `celestial/transfer` 返回的全部转移样本计算光照角、`dV`、地-星距离后套入约束。",
                "",
                "| 累计条件 | 仍满足条数 |",
                "| --- | ---: |",
                f"| 仅 光照角 < 80° | {n_imp_illum} |",
                f"| 再 + `DV2_Mag` > 5 km/s | {n_imp_dv2} |",
                f"| 再 + 地-星距离 < {IMPACTOR_DIST_MAX_KM/1e4:.0f} 万 km | {n_imp_dist} |",
                f"| 再 + `|dV|` ≤ 1.1 km/s | {n_imp_dep} |",
                "",
            ]
        )
        if min_dep_among_triple is not None:
            lines.append(
                f"在已满足前三条撞击器条件的样本中，`|dV|` 最小约 **{min_dep_among_triple/1000:.3f} km/s**。"
            )
        lines.extend(
            [
                "",
                f"- 观测器可行样本数: {len(observers)}",
                f"- 撞击器（四条件全满足）可行样本数: {len(impactors)}",
                "",
                "若无解，可尝试加密时间网格或复核任务指标含义后重算。",
            ]
        )
        if diag:
            lines.extend(
                [
                    "",
                    "### 5.2 到达时刻诊断（配对为 0 的原因）",
                    "",
                    "```json",
                    json.dumps(diag, ensure_ascii=False, indent=2),
                    "```",
                    "",
                    "- 观测器须满足：`观测器到达时刻 ≤ 撞击器到达时刻 − 90 天`。",
                    "- 若 `pairs_observer_before_impactor_gap0_days` 为 0，说明两类可行样本的到达窗口几乎互不相交（观测器普遍晚于撞击器）。",
                    "",
                ]
            )

    out_md = OUT_DIR / f"任务分析结果-{ASTEROID}.md"
    out_md.write_text("\n".join(lines) + "\n", encoding="utf-8")


if __name__ == "__main__":
    main()
