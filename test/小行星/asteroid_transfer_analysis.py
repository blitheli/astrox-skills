import json
import math
import urllib.request
from datetime import datetime, timezone
from pathlib import Path


BASE_URL = "http://astrox.cn:8765"
ASTEROIDS = ["2024 SJ3", "2025 WH5", "2025 WN6", "2024 QM"]
OUT_DIR = Path(__file__).resolve().parent

EARTH_START = "2029-01-01T00:00:00Z"
EARTH_STOP = "2029-12-31T00:00:00Z"
DEPARTURE_INTERVAL = "2028-03-01T00:00:00Z/2028-12-31T00:00:00Z"
DISTANCE_LIMIT_KM = 18_000_000
EPHEMERIS_STEP_SECONDS = 86_400
TRANSFER_STEP_DAYS = 5
MIN_TOF_DAYS = 10


def slug(name: str) -> str:
    return name.replace(" ", "-")


def save_json(path: Path, data) -> None:
    path.write_text(json.dumps(data, ensure_ascii=False, indent=2), encoding="utf-8")


def post_json(endpoint: str, payload: dict, timeout: int = 120) -> dict:
    req = urllib.request.Request(
        f"{BASE_URL}{endpoint}",
        data=json.dumps(payload).encode("utf-8"),
        headers={"Content-Type": "application/json"},
        method="POST",
    )
    with urllib.request.urlopen(req, timeout=timeout) as response:
        return json.loads(response.read().decode("utf-8"))


def parse_time(value: str) -> datetime:
    return datetime.fromisoformat(value.replace("Z", "+00:00"))


def iso_from_epoch(epoch: str, offset_seconds: float) -> str:
    dt = parse_time(epoch) + timedelta_seconds(offset_seconds)
    return dt.strftime("%Y-%m-%dT%H:%M:%SZ")


def timedelta_seconds(seconds: float):
    from datetime import timedelta

    return timedelta(seconds=float(seconds))


def state_series(position: dict) -> list[dict]:
    epoch = position["epoch"]
    data = position["cartesianVelocity"]
    states = []
    for i in range(0, len(data), 7):
        offset = data[i]
        states.append(
            {
                "time": iso_from_epoch(epoch, offset),
                "rv": data[i + 1 : i + 7],
            }
        )
    return states


def vector_norm(vec: list[float]) -> float:
    return math.sqrt(sum(x * x for x in vec))


def vector_angle_deg(a: list[float], b: list[float]) -> float:
    norm_a = vector_norm(a)
    norm_b = vector_norm(b)
    if norm_a == 0 or norm_b == 0:
        return float("nan")
    cos_value = sum(x * y for x, y in zip(a, b)) / (norm_a * norm_b)
    cos_value = max(-1.0, min(1.0, cos_value))
    return math.degrees(math.acos(cos_value))


def distance_km(rv_a: list[float], rv_b: list[float]) -> float:
    return vector_norm([rv_a[i] - rv_b[i] for i in range(3)]) / 1000.0


def contiguous_windows(samples: list[dict], max_distance_km: float) -> list[dict]:
    windows = []
    current = []
    for sample in samples:
        if sample["distance_km"] <= max_distance_km:
            current.append(sample)
        elif current:
            windows.append(window_summary(current))
            current = []
    if current:
        windows.append(window_summary(current))
    return windows


def window_summary(samples: list[dict]) -> dict:
    minimum = min(samples, key=lambda item: item["distance_km"])
    return {
        "start": samples[0]["time"],
        "stop": samples[-1]["time"],
        "days": len(samples),
        "min_distance_time": minimum["time"],
        "min_distance_km": minimum["distance_km"],
    }


def transfer_interval(window: dict) -> str:
    return f"{window['start']}/{window['stop']}"


def write_result_markdown(asteroid: str, result: dict) -> None:
    path = OUT_DIR / f"任务分析结果-{asteroid}.md"
    lines = [
        f"# 任务分析结果-{asteroid}",
        "",
        "## 距离筛选",
        "",
        f"- 2029 年最小地球距离: {result['min_distance_km']:,.0f} km",
        f"- 最小距离时刻: {result['min_distance_time']}",
        f"- 1800 万 km 内窗口数量: {len(result['arrival_windows'])}",
        "",
    ]

    if result["arrival_windows"]:
        lines.extend(["| 开始 | 结束 | 天数 | 窗口内最小距离(km) | 最小距离时刻 |", "| --- | --- | ---: | ---: | --- |"])
        for window in result["arrival_windows"]:
            lines.append(
                f"| {window['start']} | {window['stop']} | {window['days']} | "
                f"{window['min_distance_km']:,.0f} | {window['min_distance_time']} |"
            )
        lines.append("")
    else:
        lines.extend(["未发现距离小于等于 1800 万 km 的到达窗口,未执行转移搜索。", ""])

    lines.extend(["## 转移搜索", ""])
    best = result.get("best_transfer")
    if best:
        lines.extend(
            [
                f"- 地球出发搜索范围: {DEPARTURE_INTERVAL}",
                f"- 最优到达搜索范围: {best['arrival_interval']}",
                f"- 最优出发时刻: {best['DepartureTime']}",
                f"- 最优到达时刻: {best['ArrivalTime']}",
                f"- 出发速度增量 DV1: {best['DV1_Mag']:,.3f} m/s",
                f"- 到达速度增量 DV2: {best['DV2_Mag']:,.3f} m/s",
                f"- 到达光照角: {best['arrival_sunlight_angle_deg']:.3f} deg",
                "",
                "### 最优转移矢量",
                "",
                f"- DeltaV1: `{best['DeltaV1']}` m/s",
                f"- DeltaV2: `{best['DeltaV2']}` m/s",
                f"- RV2 位置: `{best['RV2'][:3]}` m",
            ]
        )
    else:
        lines.append("无可用转移结果。")

    path.write_text("\n".join(lines) + "\n", encoding="utf-8")


def main() -> None:
    earth_input = {
        "TargetName": "Earth",
        "ObserverName": "Sun",
        "ObserverFrame": "MEANECLPJ2000",
        "Start": EARTH_START,
        "Stop": EARTH_STOP,
        "Step": EPHEMERIS_STEP_SECONDS,
    }
    save_json(OUT_DIR / "earth-ephemeris-2029-input.json", earth_input)
    earth_output = post_json("/celestial/ephemeris", earth_input)
    save_json(OUT_DIR / "earth-ephemeris-2029-output.json", earth_output)
    earth_by_time = {item["time"]: item["rv"] for item in state_series(earth_output["Position"])}

    summary = []

    for asteroid in ASTEROIDS:
        asteroid_slug = slug(asteroid)
        mpc_input = {
            "TargetName": asteroid,
            "ObserverName": "Sun",
            "ObserverFrame": "MEANECLPJ2000",
            "Start": "",
            "Stop": EARTH_STOP,
        }
        save_json(OUT_DIR / f"{asteroid_slug}-mpc-input.json", mpc_input)
        mpc_output = post_json("/celestial/mpc", mpc_input)
        save_json(OUT_DIR / f"{asteroid_slug}-mpc-output.json", mpc_output)

        asteroid_states = state_series(mpc_output["Position"])
        distance_samples = []
        for state in asteroid_states:
            if state["time"] in earth_by_time:
                distance_samples.append(
                    {
                        "time": state["time"],
                        "distance_km": distance_km(state["rv"], earth_by_time[state["time"]]),
                    }
                )

        distance_output = {
            "asteroid": asteroid,
            "distance_limit_km": DISTANCE_LIMIT_KM,
            "samples": distance_samples,
        }
        save_json(OUT_DIR / f"{asteroid_slug}-distance-2029-output.json", distance_output)

        min_distance = min(distance_samples, key=lambda item: item["distance_km"])
        arrival_windows = contiguous_windows(distance_samples, DISTANCE_LIMIT_KM)
        result = {
            "asteroid": asteroid,
            "min_distance_km": min_distance["distance_km"],
            "min_distance_time": min_distance["time"],
            "arrival_windows": arrival_windows,
            "best_transfer": None,
        }

        all_transfers = []
        for index, window in enumerate(arrival_windows, start=1):
            transfer_input = {
                "SunFrameName": "MeanEclpJ2000",
                "DepartureCbName": "Earth",
                "ArrivalCbName": asteroid,
                "DepartureInterval": DEPARTURE_INTERVAL,
                "ArrivalInterval": transfer_interval(window),
                "MinTofDays": MIN_TOF_DAYS,
                "DepartureStepDay": TRANSFER_STEP_DAYS,
                "ArrivalStepDay": TRANSFER_STEP_DAYS,
                "ArrivalElements": mpc_output["OrbitElements"],
            }
            save_json(OUT_DIR / f"{asteroid_slug}-transfer-window-{index}-input.json", transfer_input)
            transfer_output = post_json("/celestial/transfer", transfer_input, timeout=300)
            save_json(OUT_DIR / f"{asteroid_slug}-transfer-window-{index}-output.json", transfer_output)

            for item in transfer_output.get("TransferResults", []):
                item["arrival_interval"] = transfer_input["ArrivalInterval"]
                all_transfers.append(item)

        if all_transfers:
            best = min(all_transfers, key=lambda item: item["DV1_Mag"])
            best["arrival_sunlight_angle_deg"] = vector_angle_deg(best["RV2"][:3], best["DeltaV2"])
            result["best_transfer"] = best

        save_json(OUT_DIR / f"{asteroid_slug}-analysis-summary.json", result)
        write_result_markdown(asteroid, result)
        summary.append(result)

    save_json(OUT_DIR / "asteroid-transfer-summary.json", summary)


if __name__ == "__main__":
    main()
