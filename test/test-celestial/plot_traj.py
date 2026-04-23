"""
plot_traj.py
Plot Earth and asteroid 2015 XF261 trajectories in the heliocentric XY plane,
plus the Lambert transfer orbit (two-body RK4 propagation) for the min-DV1 case.
"""

import json
import math
from datetime import datetime, timedelta, timezone

import matplotlib.pyplot as plt
import matplotlib.patches as mpatches
import numpy as np

# ── shared with lambert_analysis ──────────────────────────────────────────────
DATA_DIR = "."
SUN_GM = 1.3271244004193938e20          # m^3/s^2
AU = 1.495978707e11                     # m → AU conversion
ARRIVAL_DATE = datetime(2029, 4, 10, 0, 0, 0, tzinfo=timezone.utc)
DEPART_START = datetime(2028, 6,  1, 0, 0, 0, tzinfo=timezone.utc)
DEPART_END   = datetime(2029, 1, 31, 0, 0, 0, tzinfo=timezone.utc)
DEPART_STEP_DAYS = 2


# ── helpers ───────────────────────────────────────────────────────────────────

def parse_iso(s: str) -> datetime:
    return datetime.fromisoformat(s.rstrip("Z")).replace(tzinfo=timezone.utc)


def parse_position_dict(pos: dict) -> dict:
    """Return {datetime_utc: [x,y,z,vx,vy,vz]} from a Position dict."""
    epoch = parse_iso(pos["epoch"])
    cv = pos["cartesianVelocity"]
    assert len(cv) % 7 == 0
    states = {}
    for i in range(0, len(cv), 7):
        t = float(cv[i])
        key = (epoch + timedelta(seconds=t)).replace(microsecond=0)
        states[key] = cv[i+1: i+7]
    return states


def load_states(path: str) -> dict:
    with open(path, encoding="utf-8") as f:
        d = json.load(f)
    pos = d.get("Position") or d.get("position")
    if isinstance(pos, dict):
        return parse_position_dict(pos)
    raise ValueError(f"No Position in {path}")


def nearest_state(states: dict, target: datetime):
    return states[min(states, key=lambda k: abs((k - target).total_seconds()))]


def build_departure_dates():
    dates, d = [], DEPART_START
    while d <= DEPART_END:
        dates.append(d); d += timedelta(days=DEPART_STEP_DAYS)
    return dates


# ── two-body RK4 propagator ───────────────────────────────────────────────────

def twobody_deriv(s, gm):
    r3 = (s[0]**2 + s[1]**2 + s[2]**2) ** 1.5
    a  = -gm / r3
    return np.array([s[3], s[4], s[5], a*s[0], a*s[1], a*s[2]])


def rk4_step(s, dt, gm):
    k1 = twobody_deriv(s, gm)
    k2 = twobody_deriv(s + 0.5*dt*k1, gm)
    k3 = twobody_deriv(s + 0.5*dt*k2, gm)
    k4 = twobody_deriv(s +    dt*k3, gm)
    return s + (dt/6)*(k1 + 2*k2 + 2*k3 + k4)


def propagate(state0, tof_s, gm=SUN_GM, dt=3600.0):
    """Propagate state0 for tof_s seconds; return array of (x,y,z) in AU."""
    s = np.array(state0, dtype=float)
    n_steps = max(int(tof_s / dt), 1)
    dt_act  = tof_s / n_steps
    xs, ys  = [s[0]/AU], [s[1]/AU]
    for _ in range(n_steps):
        s = rk4_step(s, dt_act, gm)
        xs.append(s[0]/AU); ys.append(s[1]/AU)
    return xs, ys


# ── main ──────────────────────────────────────────────────────────────────────

def main():
    # ── load data ─────────────────────────────────────────────────────────────
    print("Loading ephemeris data ...")
    earth_states    = load_states(f"{DATA_DIR}/earth_ephemeris.json")
    asteroid_states = load_states(f"{DATA_DIR}/asteroid_2015xf261.json")

    with open(f"{DATA_DIR}/lambert_results.json", encoding="utf-8") as f:
        lambert = json.load(f)
    dv1_raw = lambert["DV1"]
    dv2_raw = lambert["DV2"]

    # ── find min-DV1 case ─────────────────────────────────────────────────────
    depart_dates = build_departure_dates()
    n = len(depart_dates)
    dv1_mag = [
        math.sqrt(sum(v**2 for v in dv1_raw[i*3:(i+1)*3]))
        for i in range(n)
    ]
    idx_min  = int(np.nanargmin(dv1_mag))
    dep_date = depart_dates[idx_min]
    dv1_vec  = dv1_raw[idx_min*3: idx_min*3+3]   # m/s
    tof_s    = (ARRIVAL_DATE - dep_date).total_seconds()

    print(f"Min |DV1| = {dv1_mag[idx_min]/1000:.3f} km/s  on {dep_date.strftime('%Y-%m-%d')}")
    print(f"TOF = {tof_s/86400:.1f} days")

    # ── Earth departure state & transfer initial state ─────────────────────────
    earth_dep  = nearest_state(earth_states, dep_date)        # [x,y,z,vx,vy,vz] m / m/s
    xfer_state = [
        earth_dep[0], earth_dep[1], earth_dep[2],
        earth_dep[3] + dv1_vec[0],
        earth_dep[4] + dv1_vec[1],
        earth_dep[5] + dv1_vec[2],
    ]

    # ── propagate transfer orbit ──────────────────────────────────────────────
    print("Propagating Lambert transfer orbit ...")
    xfer_x, xfer_y = propagate(xfer_state, tof_s)

    # ── collect trajectory XY arrays ─────────────────────────────────────────
    # Earth: use all loaded points
    earth_sorted = sorted(earth_states.items())
    ex = [v[0]/AU for _, v in earth_sorted]
    ey = [v[1]/AU for _, v in earth_sorted]

    # Asteroid: filter to 2028-06-01 → 2029-04-10 for clarity
    t_start = datetime(2028, 6, 1, tzinfo=timezone.utc)
    ast_sorted = sorted(
        [(k, v) for k, v in asteroid_states.items() if t_start <= k <= ARRIVAL_DATE],
        key=lambda x: x[0]
    )
    ax_pts = [v[0]/AU for _, v in ast_sorted]
    ay_pts = [v[1]/AU for _, v in ast_sorted]

    # arrival / departure markers
    ast_arr  = nearest_state(asteroid_states, ARRIVAL_DATE)
    dep_xy   = (earth_dep[0]/AU, earth_dep[1]/AU)
    arr_xy   = (ast_arr[0]/AU,   ast_arr[1]/AU)

    # ── plot ──────────────────────────────────────────────────────────────────
    fig, ax = plt.subplots(figsize=(10, 10))

    # Sun
    ax.plot(0, 0, "o", color="gold", markersize=14, zorder=5, label="Sun")

    # Earth orbit arc
    ax.plot(ex, ey, color="#1f77b4", linewidth=1.5, label="Earth orbit (ephemeris)")

    # Asteroid trajectory
    ax.plot(ax_pts, ay_pts, color="#ff7f0e", linewidth=1.5,
            label="Asteroid 2015 XF261 (2028-06 → 2029-04)")

    # Lambert transfer orbit
    ax.plot(xfer_x, xfer_y, color="#2ca02c", linewidth=2, linestyle="--",
            label=f"Lambert transfer ({dep_date.strftime('%Y-%m-%d')} → {ARRIVAL_DATE.strftime('%Y-%m-%d')})")

    # Departure / arrival markers
    ax.plot(*dep_xy, "^", color="#1f77b4", markersize=10, zorder=6,
            label=f"Earth departure  {dep_date.strftime('%Y-%m-%d')}")
    ax.plot(*arr_xy, "s", color="#ff7f0e", markersize=10, zorder=6,
            label=f"Asteroid arrival {ARRIVAL_DATE.strftime('%Y-%m-%d')}")

    # Annotations
    ax.annotate(dep_date.strftime("%Y-%m-%d"),
                xy=dep_xy, xytext=(dep_xy[0]+0.05, dep_xy[1]+0.08),
                fontsize=8, color="#1f77b4",
                arrowprops=dict(arrowstyle="->", color="#1f77b4", lw=0.8))
    ax.annotate(ARRIVAL_DATE.strftime("%Y-%m-%d"),
                xy=arr_xy, xytext=(arr_xy[0]-0.25, arr_xy[1]-0.12),
                fontsize=8, color="#ff7f0e",
                arrowprops=dict(arrowstyle="->", color="#ff7f0e", lw=0.8))

    ax.set_xlabel("X (AU)", fontsize=12)
    ax.set_ylabel("Y (AU)", fontsize=12)
    ax.set_title(
        "Heliocentric XY Trajectories\n"
        f"Earth → Asteroid 2015 XF261  |  Min DV1 = {dv1_mag[idx_min]:.1f} m/s",
        fontsize=13,
    )
    ax.set_aspect("equal")
    ax.grid(True, alpha=0.25)
    ax.legend(fontsize=9, loc="upper right")

    out = f"{DATA_DIR}/traj_plot.png"
    plt.tight_layout()
    plt.savefig(out, dpi=150)
    print(f"Plot saved → {out}")
    plt.close()


if __name__ == "__main__":
    main()
