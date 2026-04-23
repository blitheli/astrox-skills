#!/usr/bin/env python3
import json
import math
import sys
from dataclasses import dataclass
from datetime import datetime, timezone
from pathlib import Path
from typing import Any, Dict, List, Optional, Tuple
from urllib.error import HTTPError, URLError
from urllib.parse import urlencode
from urllib.request import Request, urlopen

BASE_URL = "http://astrox.cn:8765"
START_UTC = "2026-04-23T00:00:00Z"
STOP_UTC = "2026-04-26T00:00:00Z"

WORK_DIR = Path(__file__).resolve().parent
INPUTS_DIR = WORK_DIR / "inputs"
OUTPUTS_DIR = WORK_DIR / "outputs"


@dataclass
class Interval:
    start: datetime
    stop: datetime

    @property
    def duration_seconds(self) -> float:
        return max(0.0, (self.stop - self.start).total_seconds())

    def to_json(self) -> Dict[str, Any]:
        return {
            "start": iso_z(self.start),
            "stop": iso_z(self.stop),
            "duration_seconds": self.duration_seconds,
        }


def iso_z(dt: datetime) -> str:
    return dt.astimezone(timezone.utc).replace(microsecond=0).isoformat().replace("+00:00", "Z")


def parse_utc(text: str) -> datetime:
    normalized = text.replace("Z", "+00:00")
    return datetime.fromisoformat(normalized).astimezone(timezone.utc)


def ensure_dirs() -> None:
    INPUTS_DIR.mkdir(parents=True, exist_ok=True)
    OUTPUTS_DIR.mkdir(parents=True, exist_ok=True)


def save_json(path: Path, data: Any) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    with path.open("w", encoding="utf-8") as f:
        json.dump(data, f, ensure_ascii=False, indent=2)


def http_get(endpoint: str, params: Dict[str, Any], tag: str) -> Dict[str, Any]:
    query = urlencode({k: v for k, v in params.items() if v is not None})
    url = f"{BASE_URL}{endpoint}"
    if query:
        url = f"{url}?{query}"

    request_record = {"method": "GET", "url": url, "params": params}
    save_json(OUTPUTS_DIR / f"request_{tag}.json", request_record)

    req = Request(url=url, method="GET", headers={"Content-Type": "application/json"})
    try:
        with urlopen(req, timeout=30) as resp:
            body = resp.read().decode("utf-8")
            payload = json.loads(body)
            record = {"ok": True, "status": resp.status, "json": payload}
            save_json(OUTPUTS_DIR / f"response_{tag}.json", record)
            return record
    except HTTPError as exc:
        body = exc.read().decode("utf-8", errors="replace")
        record = {"ok": False, "status": exc.code, "error": body}
        save_json(OUTPUTS_DIR / f"response_{tag}.json", record)
        return record
    except (URLError, TimeoutError) as exc:
        record = {"ok": False, "status": None, "error": str(exc)}
        save_json(OUTPUTS_DIR / f"response_{tag}.json", record)
        return record


def http_post(endpoint: str, payload: Dict[str, Any], tag: str) -> Dict[str, Any]:
    url = f"{BASE_URL}{endpoint}"
    save_json(OUTPUTS_DIR / f"request_{tag}.json", {"method": "POST", "url": url, "json": payload})
    data = json.dumps(payload, ensure_ascii=False).encode("utf-8")

    req = Request(
        url=url,
        data=data,
        method="POST",
        headers={"Content-Type": "application/json"},
    )
    try:
        with urlopen(req, timeout=60) as resp:
            body = resp.read().decode("utf-8")
            parsed = json.loads(body)
            record = {"ok": True, "status": resp.status, "json": parsed}
            save_json(OUTPUTS_DIR / f"response_{tag}.json", record)
            return record
    except HTTPError as exc:
        body = exc.read().decode("utf-8", errors="replace")
        record = {"ok": False, "status": exc.code, "error": body}
        save_json(OUTPUTS_DIR / f"response_{tag}.json", record)
        return record
    except (URLError, TimeoutError) as exc:
        record = {"ok": False, "status": None, "error": str(exc)}
        save_json(OUTPUTS_DIR / f"response_{tag}.json", record)
        return record


def expect_api_success(record: Dict[str, Any], context: str) -> Dict[str, Any]:
    if not record.get("ok"):
        raise RuntimeError(f"{context} HTTP failed: {record.get('status')} {record.get('error')}")
    body = record.get("json", {})
    if not body.get("IsSuccess", False):
        raise RuntimeError(f"{context} API failed: {body.get('Message')}")
    return body


def city_to_site_position(city_item: Dict[str, Any]) -> Tuple[Dict[str, Any], Dict[str, Any]]:
    lat_rad = city_item["Latitude"]
    lon_rad = city_item["Longitude"]
    lat_deg = math.degrees(lat_rad)
    lon_deg = math.degrees(lon_rad)
    source = {
        "mode": "query-city",
        "city_name": city_item.get("CityName"),
        "country_name": city_item.get("CountryName"),
        "latitude_rad": lat_rad,
        "longitude_rad": lon_rad,
        "latitude_deg": lat_deg,
        "longitude_deg": lon_deg,
    }
    position = {
        "$type": "SitePosition",
        "CentralBody": city_item.get("CentralBodyName") or "Earth",
        "cartographicDegrees": [lon_deg, lat_deg, 0],
        "clampToGround": False,
    }
    return position, source


def fallback_shanghai_position() -> Tuple[Dict[str, Any], Dict[str, Any]]:
    lon_deg, lat_deg, height_m = 121.4737, 31.2304, 0
    source = {
        "mode": "fallback",
        "city_name": "Shanghai",
        "latitude_deg": lat_deg,
        "longitude_deg": lon_deg,
        "height_m": height_m,
    }
    position = {
        "$type": "SitePosition",
        "CentralBody": "Earth",
        "cartographicDegrees": [lon_deg, lat_deg, height_m],
        "clampToGround": False,
    }
    return position, source


def pick_tiangong_tle() -> Tuple[Dict[str, Any], Dict[str, Any]]:
    query_names = ["TIANGONG", "TIANHE", "CSS", "CHINA SPACE STATION"]
    for i, name in enumerate(query_names, start=1):
        record = http_get("/ssc", {"sscName": name}, f"query_tle_{i}")
        if not record.get("ok"):
            continue
        data = record.get("json", {})
        for item in data.get("TLEs") or []:
            line1 = item.get("TLE_Line1")
            line2 = item.get("TLE_Line2")
            sat_num = item.get("SatelliteNumber")
            if line1 and line2 and sat_num:
                sgp4 = {"$type": "SGP4", "SatelliteNumber": str(sat_num), "TLEs": [line1, line2]}
                source = {
                    "mode": "query-tle",
                    "query_name": name,
                    "official_name": item.get("OfficialName"),
                    "common_name": item.get("CommonName"),
                    "satellite_number": sat_num,
                    "tle_epoch": item.get("TleEpoch"),
                }
                return sgp4, source
    raise RuntimeError("Unable to find Tiangong TLE from /ssc queries.")


def to_intervals_from_access(body: Dict[str, Any]) -> List[Interval]:
    intervals: List[Interval] = []
    for item in body.get("Passes") or []:
        start = item.get("AccessStart")
        stop = item.get("AccessStop")
        if start and stop:
            intervals.append(Interval(parse_utc(start), parse_utc(stop)))
    return intervals


def to_intervals_from_lighting(body: Dict[str, Any], key: str) -> List[Interval]:
    intervals: List[Interval] = []
    block = body.get(key) or {}
    for item in block.get("Intervals") or []:
        start = item.get("Start")
        stop = item.get("Stop")
        if start and stop:
            intervals.append(Interval(parse_utc(start), parse_utc(stop)))
    return intervals


def intersect_two(a: List[Interval], b: List[Interval]) -> List[Interval]:
    i, j = 0, 0
    result: List[Interval] = []
    a_sorted = sorted(a, key=lambda x: x.start)
    b_sorted = sorted(b, key=lambda x: x.start)
    while i < len(a_sorted) and j < len(b_sorted):
        left = max(a_sorted[i].start, b_sorted[j].start)
        right = min(a_sorted[i].stop, b_sorted[j].stop)
        if left < right:
            result.append(Interval(left, right))
        if a_sorted[i].stop <= b_sorted[j].stop:
            i += 1
        else:
            j += 1
    return result


def summarize(intervals: List[Interval]) -> Dict[str, Any]:
    if not intervals:
        return {"count": 0, "total_duration_seconds": 0, "min_duration_seconds": 0, "max_duration_seconds": 0}
    durations = [x.duration_seconds for x in intervals]
    return {
        "count": len(intervals),
        "total_duration_seconds": sum(durations),
        "min_duration_seconds": min(durations),
        "max_duration_seconds": max(durations),
    }


def main() -> int:
    ensure_dirs()

    city_record = http_get("/city", {"cityName": "Shanghai"}, "query_city_shanghai")
    shanghai_position: Dict[str, Any]
    city_source: Dict[str, Any]

    if city_record.get("ok") and city_record.get("json", {}).get("IsSuccess") and (city_record.get("json", {}).get("Cities") or []):
        shanghai_position, city_source = city_to_site_position(city_record["json"]["Cities"][0])
    else:
        shanghai_position, city_source = fallback_shanghai_position()

    tiangong_position, tle_source = pick_tiangong_tle()

    access_payload = {
        "Description": "Shanghai to Tiangong access with fixed window",
        "Start": START_UTC,
        "Stop": STOP_UTC,
        "OutStep": 60,
        "ComputeAER": False,
        "UseLightTimeDelay": False,
        "FromObjectPath": {"Name": "Facility/Shanghai", "Position": shanghai_position},
        "ToObjectPath": {"Name": "Satellite/Tiangong", "Position": tiangong_position},
    }
    lighting_sh_payload = {
        "Description": "Shanghai lighting for night(Umbra) intervals",
        "Start": START_UTC,
        "Stop": STOP_UTC,
        "Position": shanghai_position,
    }
    lighting_tg_payload = {
        "Description": "Tiangong lighting for SunLight intervals",
        "Start": START_UTC,
        "Stop": STOP_UTC,
        "Position": tiangong_position,
    }

    save_json(INPUTS_DIR / "access_request.json", access_payload)
    save_json(INPUTS_DIR / "lighting_shanghai_request.json", lighting_sh_payload)
    save_json(INPUTS_DIR / "lighting_tiangong_request.json", lighting_tg_payload)

    access_body = expect_api_success(http_post("/access/AccessComputeV2", access_payload, "access_compute_v2"), "AccessComputeV2")
    light_sh_body = expect_api_success(http_post("/Lighting/LightingTimes", lighting_sh_payload, "lighting_shanghai"), "LightingTimes(Shanghai)")
    light_tg_body = expect_api_success(http_post("/Lighting/LightingTimes", lighting_tg_payload, "lighting_tiangong"), "LightingTimes(Tiangong)")

    access_intervals = to_intervals_from_access(access_body)
    sh_night_intervals = to_intervals_from_lighting(light_sh_body, "Umbra")
    tg_sunlight_intervals = to_intervals_from_lighting(light_tg_body, "SunLight")

    constrained = intersect_two(intersect_two(access_intervals, sh_night_intervals), tg_sunlight_intervals)
    result = {
        "window": {"start": START_UTC, "stop": STOP_UTC},
        "input_sources": {"shanghai": city_source, "tiangong": tle_source},
        "raw_counts": {
            "access_passes": len(access_intervals),
            "shanghai_umbra_intervals": len(sh_night_intervals),
            "tiangong_sunlight_intervals": len(tg_sunlight_intervals),
        },
        "constrained_passes": [x.to_json() for x in constrained],
        "summary": summarize(constrained),
    }
    save_json(OUTPUTS_DIR / "final_access_with_light_constraints.json", result)
    print(json.dumps(result, ensure_ascii=False, indent=2))
    return 0


if __name__ == "__main__":
    try:
        raise SystemExit(main())
    except Exception as exc:
        print(f"ERROR: {exc}", file=sys.stderr)
        raise
