"""Earth return capture analysis: hyperbolic arrival + lunar gravity assist."""
import numpy as np

MU_E = 398600.4418  # km^3/s^2
MU_M = 4902.800
R_E = 6378.137
R_M = 1737.4
A_EM = 384400.0
V_INF = 0.6  # km/s Earth arrival v_inf


def as_vec(x):
    return np.asarray(x, dtype=float).reshape(3)


def orbit_elements(r, v, mu=MU_E):
    r, v = as_vec(r), as_vec(v)
    rm, vm = np.linalg.norm(r), np.linalg.norm(v)
    eps = vm**2 / 2 - mu / rm
    h = np.cross(r, v)
    hn = np.linalg.norm(h)
    e_vec = np.cross(v, h) / mu - r / rm
    e = np.linalg.norm(e_vec)
    a = -mu / (2 * eps) if abs(eps) > 1e-15 else np.inf
    if e < 1.0:
        rp, ra = a * (1 - e), a * (1 + e)
    else:
        rp, ra = a * (e - 1), np.nan
    return dict(eps=eps, a=a, e=e, rp=rp, ra=ra, h=h, hn=hn)


def flyby_delta(v_inf, rp_m):
    s = np.clip(1.0 / (1.0 + rp_m * v_inf**2 / MU_M), -1.0, 1.0)
    return 2 * np.arcsin(s)


def rodrigues(v, axis, angle):
    axis = axis / np.linalg.norm(axis)
    c, s = np.cos(angle), np.sin(angle)
    return v * c + np.cross(axis, v) * s + axis * np.dot(axis, v) * (1 - c)


def moon_flyby(v_sc_ecl, r_sc_ecl, r_moon_ecl, v_moon_ecl, flyby_alt_km):
    r_rel = as_vec(r_sc_ecl) - as_vec(r_moon_ecl)
    v_rel = as_vec(v_sc_ecl) - as_vec(v_moon_ecl)
    v_inf_m = np.linalg.norm(v_rel)
    if v_inf_m < 1e-6:
        return []
    rp_m = R_M + flyby_alt_km
    delta = flyby_delta(v_inf_m, rp_m)
    axis = np.cross(r_rel, v_rel)
    if np.linalg.norm(axis) < 1e-6:
        return []
    results = []
    for sign in (1, -1):
        v_rel_out = rodrigues(v_rel, axis, sign * delta)
        v_out = v_rel_out + as_vec(v_moon_ecl)
        oe = orbit_elements(r_sc_ecl, v_out)
        vinf_after = np.sqrt(max(0.0, 2 * oe["eps"])) if oe["eps"] > 0 else 0.0
        results.append(
            dict(
                v_out=v_out,
                v_inf_m=v_inf_m,
                delta_deg=np.degrees(delta),
                sign=sign,
                vinf_after=vinf_after,
                C3_after=max(0.0, 2 * oe["eps"]),
                bound=oe["eps"] < 0,
                **oe,
            )
        )
    return results


def main():
    v_moon = np.sqrt(MU_E / A_EM)
    print(f"Earth arrival v_inf = {V_INF*1000:.0f} m/s, C3 = {V_INF**2:.2f} km^2/s^2\n")

    # Part 1: velocity at lunar distance
    v_lunar = np.sqrt(V_INF**2 + 2 * MU_E / A_EM)
    print("=== 1. 地心速度 at r = 384400 km (月球轨道距离) ===")
    print(f"    v = {v_lunar:.4f} km/s = {v_lunar*1000:.1f} m/s")
    print("    与近地点高度(1000~30000 km)无关，同一双曲线能量\n")

    print("=== 近地点速度 (随近地点高度) ===")
    for h in [1000, 5000, 10000, 20000, 30000]:
        rp = R_E + h
        vp = np.sqrt(V_INF**2 + 2 * MU_E / rp)
        print(f"    h_p = {h:5d} km  ->  v_p = {vp*1000:7.1f} m/s")

    # Part 2: lunar flyby scan - 3D encounter search
    print("\n=== 2. 月球引力辅助减速 (patched conics, 共面) ===")
    print("    场景: 双曲线到达，在月球附近飞越实现地心俘获\n")

    v_moon_spd = v_moon
    best_bound = []

    flyby_alts = [100, 300, 500, 1000, 2000, 3000, 5000, 8000, 10000,
                  15000, 20000, 30000, 50000, 80000, 100000, 200000]

    for alt in flyby_alts:
        for theta_m in np.linspace(0, 2 * np.pi, 72, endpoint=False):
            r_moon = as_vec([A_EM * np.cos(theta_m), A_EM * np.sin(theta_m), 0])
            v_moon = as_vec([-v_moon_spd * np.sin(theta_m), v_moon_spd * np.cos(theta_m), 0])

            for phi in np.linspace(0, 2 * np.pi, 36, endpoint=False):
                # SC at lunar distance, inbound hyperbola
                r_sc = as_vec([A_EM * np.cos(phi), A_EM * np.sin(phi), 0])
                rmag = np.linalg.norm(r_sc)
                vmag = np.sqrt(V_INF**2 + 2 * MU_E / rmag)
                # v_inf direction: combine radial (inward) + tangential component
                r_hat = r_sc / rmag
                t_hat = as_vec([-r_hat[1], r_hat[0], 0])
                for mix in [0.0, 0.15, 0.3, 0.5]:
                    v_sc = (-np.sqrt(1 - mix**2) * r_hat + mix * t_hat) * vmag
                    for res in moon_flyby(v_sc, r_sc, r_moon, v_moon, alt):
                        if res["bound"]:
                            best_bound.append(
                                dict(
                                    flyby_alt=alt,
                                    theta_m=np.degrees(theta_m),
                                    phi=np.degrees(phi),
                                    mix=mix,
                                    v_inf_m=res["v_inf_m"],
                                    delta_deg=res["delta_deg"],
                                    a=res["a"],
                                    e=res["e"],
                                    hp=res["rp"] - R_E,
                                    ha=res["ra"] - R_E,
                                    eps=res["eps"],
                                )
                            )

    # dedupe similar
    best_bound.sort(key=lambda x: (-x["ha"], x["flyby_alt"]))
    print(f"{'飞越高度km':>10} {'月心v_inf km/s':>14} {'转向角deg':>10} "
          f"{'近地点km':>10} {'远地点km':>12} {'偏心率e':>10}")
    seen = set()
    count = 0
    for c in best_bound:
        key = (c["flyby_alt"], round(c["hp"], -2), round(c["ha"], -3))
        if key in seen:
            continue
        seen.add(key)
        print(f"{c['flyby_alt']:>10} {c['v_inf_m']:>14.3f} {c['delta_deg']:>10.2f} "
              f"{c['hp']:>10.0f} {c['ha']:>12.0f} {c['e']:>10.4f}")
        count += 1
        if count >= 20:
            break

    # Representative case: target high ellipse with apogee ~ lunar distance
    print("\n=== 3. 典型大椭圆俘获轨道 (远地点 ~ 月球轨道高度) ===")
    target = [c for c in best_bound if 300000 < c["ha"] < 450000]
    target.sort(key=lambda x: x["flyby_alt"])
    if target:
        c = target[len(target) // 2]
        print(f"    月球飞越高度: {c['flyby_alt']:.0f} km (距月面)")
        print(f"    月心相对 v_inf: {c['v_inf_m']*1000:.1f} m/s")
        print(f"    飞越转向角: {c['delta_deg']:.2f} deg")
        print(f"    俘获后近地点高度: {c['hp']:.0f} km")
        print(f"    俘获后远地点高度: {c['ha']:.0f} km")
        print(f"    半长轴 a = {c['a']:.0f} km")
        print(f"    偏心率 e = {c['e']:.4f}")

    # Analytical: if braking at lunar distance with impulsive burn instead
    print("\n=== 4. 解析参考: 在月球轨道距离处直接制动 ===")
    v_circ = np.sqrt(MU_E / A_EM)
    dv_brake = v_lunar - v_circ
    eps_circ = v_circ**2 / 2 - MU_E / A_EM
    a_circ = -MU_E / (2 * eps_circ)
    print(f"    圆轨道速度(384400km): {v_circ*1000:.1f} m/s")
    print(f"    所需制动 ΔV: {(v_lunar - v_circ)*1000:.1f} m/s")
    print(f"    -> 圆轨道 r = 384400 km (不可行大椭圆)")

    # High ellipse: brake to v such that r=384400 is apogee
    # At apogee: v_a = sqrt(mu*(1-e)/(a*(1+e))) and r_a = a(1+e)
    # Choose r_p = R_E + 1000
    rp_tgt = R_E + 1000
    ra_tgt = A_EM
    e_tgt = (ra_tgt - rp_tgt) / (ra_tgt + rp_tgt)
    a_tgt = (ra_tgt + rp_tgt) / 2
    va_tgt = np.sqrt(MU_E * (1 - e_tgt) / (a_tgt * (1 + e_tgt)))
    dv_to_apogee = v_lunar - va_tgt  # approximate if radial inbound
    print(f"\n    目标: 近地点 {rp_tgt-R_E:.0f} km, 远地点 {ra_tgt-R_E:.0f} km")
    print(f"    偏心率 e = {e_tgt:.4f}, 半长轴 a = {a_tgt:.0f} km")
    print(f"    远地点速度 v_a = {va_tgt*1000:.1f} m/s")
    print(f"    在 r=384400 km 处需制动 ΔV ≈ {dv_to_apogee*1000:.0f} m/s (径向入射近似)")


if __name__ == "__main__":
    main()
