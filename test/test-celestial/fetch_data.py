"""
fetch_data.py
Fetch Earth ephemeris and asteroid 2015 XF261 ephemeris from ASTROX WebAPI,
and save the raw JSON responses to local files.
"""

import json
import sys
import requests

BASE_URL = "http://astrox.cn:8765"
OUTPUT_DIR = "."


def fetch_earth_ephemeris():
    """Fetch Earth heliocentric ephemeris at 2-day intervals from 2028-06-01 to 2029-02-01."""
    url = f"{BASE_URL}/celestial/ephemeris"
    payload = {
        "TargetName": "Earth",
        "ObserverName": "Sun",
        "ObserverFrame": "MEANECLPJ2000",
        "Start": "2028-06-01T00:00:00Z",
        "Stop": "2029-02-01T00:00:00Z",
        "Step": 172800,
    }
    print(f"[1/2] Fetching Earth ephemeris  {payload['Start']} → {payload['Stop']}  step={payload['Step']}s ...")
    resp = requests.post(url, json=payload, timeout=120)
    resp.raise_for_status()
    data = resp.json()
    out_path = f"{OUTPUT_DIR}/earth_ephemeris.json"
    with open(out_path, "w", encoding="utf-8") as f:
        json.dump(data, f, ensure_ascii=False, indent=2)
    print(f"    Saved → {out_path}")
    return data


def fetch_asteroid_ephemeris():
    """Fetch asteroid 2015 XF261 ephemeris up to 2029-04-11 (covers April 10 point)."""
    url = f"{BASE_URL}/celestial/mpc"
    payload = {
        "TargetName": "2015 XF261",
        "ObserverName": "Sun",
        "ObserverFrame": "MEANECLPJ2000",
        "Start": "",
        "Stop": "2029-04-11T00:00:00Z",
    }
    print(f"[2/2] Fetching asteroid 2015 XF261 ephemeris  Stop={payload['Stop']} ...")
    resp = requests.post(url, json=payload, timeout=180)
    resp.raise_for_status()
    data = resp.json()
    if not data.get("IsSuccess", False):
        print(f"    WARNING: API returned IsSuccess=False: {data.get('Message', '')}")
    out_path = f"{OUTPUT_DIR}/asteroid_2015xf261.json"
    with open(out_path, "w", encoding="utf-8") as f:
        json.dump(data, f, ensure_ascii=False, indent=2)
    print(f"    Saved → {out_path}")
    return data


if __name__ == "__main__":
    try:
        fetch_earth_ephemeris()
        fetch_asteroid_ephemeris()
        print("\nAll data fetched successfully.")
    except requests.exceptions.ConnectionError as e:
        print(f"\nERROR: Cannot connect to {BASE_URL}. Server may be offline.\n{e}", file=sys.stderr)
        sys.exit(1)
    except requests.exceptions.HTTPError as e:
        print(f"\nERROR: HTTP error: {e}", file=sys.stderr)
        sys.exit(1)
