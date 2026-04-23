"""
lambert_analysis.py
Parse Earth and asteroid ephemeris JSON, build batch Lambert requests
(heliocentric, Sun Gm), call ASTROX WebAPI, save results, and plot DV1/DV2.

Run after fetch_data.py has been executed.
"""

import json
import math
import sys
from datetime import datetime, timedelta, timezone

import matplotlib.pyplot as plt
import matplotlib.dates as mdates
import numpy as np
import requests

BASE_URL = "http://astrox.cn:8765"
DATA_DIR = "."
SUN_GM = 1.3271244004193938e20  # m^3/s^2

ARRIVAL_DATE = datetime(2029, 4, 10, 0, 0, 0, tzinfo=timezone.utc)
DEPART_START = datetime(2028, 6, 1, 0, 0, 0, tzinfo=timezone.utc)
DEPART_END = datetime(2029, 1, 31, 0, 0, 0, tzinfo=timezone.utc)
DEPART_STEP_DAYS = 2


# ---------------------------------------------------------------------------
# CZML parsing helpers
# ---------------------------------------------------------------------------

def parse_iso(s: str) -> datetime:
    """Parse ISO8601 UTC string to timezone-aware datetime."""
    s = s.rstrip("Z")
    dt = datetime.fromisoformat(s)
    return dt.replace(tzinfo=timezone.utc)


def extract_czml_states(api_response) -> dict:
    """
    Parse ASTROX API response and return a dict:
        {datetime_utc: [x, y, z, vx, vy, vz]}

    Supported shapes:
      1. {"IsSuccess": ..., "Position": {"epoch": "...", "cartesianVelocity": [...]}, ...}
      2. CZML array: [{"id":"document",...}, {"position": {"epoch":..., "cartesianVelocity":[...]}}]

    cartesianVelocity layout (relative epoch seconds):
        [t0, x0, y0, z0, vx0, vy0, vz0,  t1, x1, y1, z1, vx1, vy1, vz1, ...]
    """

    def _parse_pos_dict(pos_dict: dict) -> dict:
        epoch_str = pos_dict.get("epoch") or pos_dict.get("Epoch")
        cv = pos_dict.get("cartesianVelocity") or pos_dict.get("CartesianVelocity")
        if not epoch_str or not cv:
            return {}
        epoch_dt = parse_iso(epoch_str)
        assert len(cv) % 7 == 0, f"cartesianVelocity length {len(cv)} not divisible by 7"
        result = {}
        for i in range(0, len(cv), 7):
            t_sec = float(cv[i])
            state = cv[i + 1: i + 7]
            dt_key = (epoch_dt + timedelta(seconds=t_sec)).replace(microsecond=0)
            result[dt_key] = state
        return result

    # Shape 1: top-level dict with "Position" key (ephemeris and MPC responses)
    if isinstance(api_response, dict):
        pos = api_response.get("Position") or api_response.get("position")
        if isinstance(pos, dict):
            return _parse_pos_dict(pos)
        # Fallback: api_response itself might be the position dict
        if "epoch" in api_response or "cartesianVelocity" in api_response:
            return _parse_pos_dict(api_response)
        raise ValueError(f"Cannot find Position in dict with keys: {list(api_response.keys())}")

    # Shape 2: CZML array
    if isinstance(api_response, list):
        states = {}
        for item in api_response:
            if not isinstance(item, dict):
                continue
            pos = item.get("position") or item.get("Position")
            if isinstance(pos, dict):
                states.update(_parse_pos_dict(pos))
        return states

    raise ValueError(f"Unsupported api_response type: {type(api_response)}")


def find_nearest_state(states: dict, target: datetime):
    """Find the state vector closest in time to target."""
    target = target.replace(tzinfo=timezone.utc)
    best_key = min(states.keys(), key=lambda k: abs((k - target).total_seconds()))
    delta_s = abs((best_key - target).total_seconds())
    if delta_s > 86400:
        print(f"    WARNING: nearest state is {delta_s/3600:.1f} h away from target {target.isoformat()}")
    return states[best_key], best_key


# ---------------------------------------------------------------------------
# Main
# ---------------------------------------------------------------------------

def load_json(path: str):
    with open(path, "r", encoding="utf-8") as f:
        return json.load(f)


def build_departure_dates():
    dates = []
    d = DEPART_START
    while d <= DEPART_END:
        dates.append(d)
        d += timedelta(days=DEPART_STEP_DAYS)
    return dates


def compute_lambert():
    # --- Load raw data ---
    print("Loading Earth ephemeris ...")
    earth_raw = load_json(f"{DATA_DIR}/earth_ephemeris.json")
    print("Loading asteroid ephemeris ...")
    asteroid_raw = load_json(f"{DATA_DIR}/asteroid_2015xf261.json")

    # --- Parse CZML ---
    print("Parsing Earth CZML ...")
    earth_states = extract_czml_states(earth_raw)
    print(f"    {len(earth_states)} Earth state vectors loaded.")

    print("Parsing asteroid CZML ...")
    asteroid_states = extract_czml_states(asteroid_raw)
    print(f"    {len(asteroid_states)} asteroid state vectors loaded.")

    # --- Asteroid state at arrival date ---
    rv2_single, arrived_at = find_nearest_state(asteroid_states, ARRIVAL_DATE)
    print(f"    Asteroid arrival state taken at {arrived_at.isoformat()}")
    print(f"    Asteroid pos [AU]: {[x/1.496e11 for x in rv2_single[:3]]}")

    # --- Build batch arrays ---
    depart_dates = build_departure_dates()
    RV1_flat = []
    RV2_flat = []
    TOF_list = []
    valid_dates = []
    missing = []

    for d in depart_dates:
        if d not in earth_states:
            # Try nearest within 1 hour tolerance
            nearest_state, nearest_key = find_nearest_state(earth_states, d)
            delta = abs((nearest_key - d).total_seconds())
            if delta > 3600:
                missing.append(d)
                continue
            rv1 = nearest_state
        else:
            rv1 = earth_states[d]

        tof = (ARRIVAL_DATE - d).total_seconds()
        if tof <= 0:
            missing.append(d)
            continue

        RV1_flat.extend(rv1)
        RV2_flat.extend(rv2_single)
        TOF_list.append(tof)
        valid_dates.append(d)

    if missing:
        print(f"    Skipped {len(missing)} departure dates (no Earth state or non-positive TOF).")
    print(f"    {len(valid_dates)} valid departure dates ready for Lambert.")

    # --- Call Lambert API ---
    print(f"\nCalling Lambert API ({len(valid_dates)} cases) ...")
    lambert_payload = {
        "RV1": RV1_flat,
        "RV2": RV2_flat,
        "TOF": TOF_list,
        "Gm": SUN_GM,
    }
    resp = requests.post(f"{BASE_URL}/orbit/lambert", json=lambert_payload, timeout=300)
    resp.raise_for_status()
    lambert_data = resp.json()

    if not lambert_data.get("IsSuccess", False):
        print(f"Lambert API error: {lambert_data.get('Message', '')}")
        sys.exit(1)

    # Save raw Lambert results
    result_path = f"{DATA_DIR}/lambert_results.json"
    with open(result_path, "w", encoding="utf-8") as f:
        json.dump(lambert_data, f, ensure_ascii=False, indent=2)
    print(f"Lambert results saved → {result_path}")

    return valid_dates, lambert_data


def extract_dv(lambert_data, n: int):
    """Extract |DV1| and |DV2| (km/s) for each of the n cases."""
    dv1_raw = lambert_data.get("DV1", [])
    dv2_raw = lambert_data.get("DV2", [])

    dv1_mag = []
    dv2_mag = []
    for i in range(n):
        base = i * 3
        if base + 2 < len(dv1_raw):
            mag1 = math.sqrt(sum(v ** 2 for v in dv1_raw[base: base + 3])) / 1000.0
        else:
            mag1 = float("nan")
        if base + 2 < len(dv2_raw):
            mag2 = math.sqrt(sum(v ** 2 for v in dv2_raw[base: base + 3])) / 1000.0
        else:
            mag2 = float("nan")
        dv1_mag.append(mag1)
        dv2_mag.append(mag2)
    return dv1_mag, dv2_mag


def plot_results(dates, dv1, dv2):
    fig, ax = plt.subplots(figsize=(12, 6))

    color1 = "#1f77b4"

    ax.set_xlabel("Earth Departure Date", fontsize=12)
    ax.set_ylabel("|DV1| (km/s)", fontsize=12)
    ax.plot(dates, dv1, color=color1, linewidth=2, marker="o", markersize=3, label="|DV1|")
    ax.xaxis.set_major_formatter(mdates.DateFormatter("%Y-%m"))
    ax.xaxis.set_major_locator(mdates.MonthLocator())
    plt.setp(ax.xaxis.get_majorticklabels(), rotation=30, ha="right")

    ax.set_title(
        "Lambert Transfer DV1: Earth → Asteroid 2015 XF261\n"
        f"Arrival fixed at {ARRIVAL_DATE.strftime('%Y-%m-%d')}, "
        f"Departure: {DEPART_START.strftime('%Y-%m-%d')} – {DEPART_END.strftime('%Y-%m-%d')}",
        fontsize=13,
    )
    ax.set_ylim(0, 1.0)
    ax.yaxis.set_major_formatter(plt.FuncFormatter(lambda v, _: f"{v*1000:.0f}"))
    ax.set_ylabel("|DV1| (m/s)", fontsize=12)
    ax.grid(True, alpha=0.3)

    # Annotate min-DV1 point
    idx_min = int(np.nanargmin(dv1))
    ax.axvline(x=dates[idx_min], color="green", linestyle=":", linewidth=1.5, alpha=0.7)
    ax.annotate(
        f"Min |DV1|\n{dates[idx_min].strftime('%Y-%m-%d')}\n{dv1[idx_min]*1000:.1f} m/s",
        xy=(dates[idx_min], dv1[idx_min]),
        xytext=(20, 30),
        textcoords="offset points",
        fontsize=9,
        arrowprops=dict(arrowstyle="->", color="green"),
        color="green",
    )

    fig.tight_layout()
    out_path = f"{DATA_DIR}/dv_plot.png"
    plt.savefig(out_path, dpi=150)
    print(f"Plot saved → {out_path}")
    plt.close()


if __name__ == "__main__":
    try:
        dates, lambert_data = compute_lambert()
    except requests.exceptions.ConnectionError as e:
        print(f"\nERROR: Cannot connect to {BASE_URL}. Server may be offline.\n{e}", file=sys.stderr)
        sys.exit(1)
    except FileNotFoundError as e:
        print(f"\nERROR: {e}\nRun fetch_data.py first.", file=sys.stderr)
        sys.exit(1)

    dv1, dv2 = extract_dv(lambert_data, len(dates))

    print("\n--- DV Summary (km/s) ---")
    print(f"{'Departure':<22}  {'|DV1|':>8}  {'|DV2|':>8}  {'Total':>8}")
    print("-" * 54)
    for d, v1, v2 in zip(dates, dv1, dv2):
        total = v1 + v2 if not (math.isnan(v1) or math.isnan(v2)) else float("nan")
        print(f"{d.strftime('%Y-%m-%d %H:%M'):<22}  {v1:>8.3f}  {v2:>8.3f}  {total:>8.3f}")

    plot_results(dates, dv1, dv2)
    print("\nDone.")
