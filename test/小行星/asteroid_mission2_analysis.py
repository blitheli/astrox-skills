import json
import math
from datetime import datetime
from pathlib import Path


OUT_DIR = Path(__file__).resolve().parent
MU_EARTH_KM3_S2 = 398600.4418
EARTH_RADIUS_KM = 6378.137
PERIGEE_ALTITUDE_KM = 200.0
REFERENCE_SPEED_KM_S = 10.96
ARRIVAL_GAP_TARGET_DAYS = 90
ARRIVAL_GAP_TOLERANCE_DAYS = 5


def vector_norm(vector: list[float]) -> float:
    return math.sqrt(sum(value * value for value in vector))


def vector_angle_deg(a: list[float], b: list[float]) -> float:
    norm_a = vector_norm(a)
    norm_b = vector_norm(b)
    cos_value = sum(x * y for x, y in zip(a, b)) / (norm_a * norm_b)
    cos_value = max(-1.0, min(1.0, cos_value))
    return math.degrees(math.acos(cos_value))


def parse_time(value: str) -> datetime:
    return datetime.fromisoformat(value.replace("Z", "+00:00"))


def earth_departure_dv_km_s(dv1_mag_m_s: float) -> float:
    vinf_km_s = dv1_mag_m_s / 1000.0
    rp_km = EARTH_RADIUS_KM + PERIGEE_ALTITUDE_KM
    perigee_speed_km_s = math.sqrt(vinf_km_s * vinf_km_s + 2.0 * MU_EARTH_KM3_S2 / rp_km)
    return perigee_speed_km_s - REFERENCE_SPEED_KM_S


def enrich_transfer(item: dict) -> dict:
    enriched = dict(item)
    enriched["DepartureDate"] = enriched["DepartureTime"][:10]
    enriched["ArrivalDate"] = enriched["ArrivalTime"][:10]
    enriched["arrival_sunlight_angle_deg"] = vector_angle_deg(enriched["RV2"][:3], enriched["DeltaV2"])
    enriched["earth_departure_dv_km_s"] = earth_departure_dv_km_s(enriched["DV1_Mag"])
    enriched["dv2_km_s"] = enriched["DV2_Mag"] / 1000.0
    enriched["observer_total_dv_km_s"] = enriched["earth_departure_dv_km_s"] + enriched["dv2_km_s"]
    enriched["_arrival_dt"] = parse_time(enriched["ArrivalTime"])
    return enriched


def public_transfer(item: dict) -> dict:
    item = dict(item)
    item.pop("_arrival_dt", None)
    return item


def load_transfers(path: Path) -> list[dict]:
    data = json.loads(path.read_text(encoding="utf-8"))
    return [enrich_transfer(item) for item in data.get("TransferResults", [])]


def main() -> None:
    impactor_transfers = load_transfers(OUT_DIR / "2025-WN6-transfer-window-1-output.json")
    observer_transfers = load_transfers(OUT_DIR / "2025-WN6-transfer-observer-early-output.json")

    impactor_feasible = [
        item
        for item in impactor_transfers
        if item["arrival_sunlight_angle_deg"] < 80.0 and item["DV2_Mag"] > 5000.0
    ]
    observer_feasible = [
        item for item in observer_transfers if item["observer_total_dv_km_s"] <= 2.0
    ]

    pairs = []
    lower_gap = ARRIVAL_GAP_TARGET_DAYS - ARRIVAL_GAP_TOLERANCE_DAYS
    upper_gap = ARRIVAL_GAP_TARGET_DAYS + ARRIVAL_GAP_TOLERANCE_DAYS
    for impactor in impactor_feasible:
        for observer in observer_feasible:
            gap_days = (impactor["_arrival_dt"] - observer["_arrival_dt"]).total_seconds() / 86400.0
            if lower_gap <= gap_days <= upper_gap:
                pairs.append(
                    {
                        "arrival_gap_days": gap_days,
                        "impactor": public_transfer(impactor),
                        "observer": public_transfer(observer),
                        "combined_metric_km_s": impactor["earth_departure_dv_km_s"]
                        + observer["observer_total_dv_km_s"],
                    }
                )

    for pair in pairs:
        pair["impactor"].pop("observer_total_dv_km_s", None)

    summary = {
        "asteroid": "2025 WN6",
        "arrival_gap_target_days": ARRIVAL_GAP_TARGET_DAYS,
        "arrival_gap_tolerance_days": ARRIVAL_GAP_TOLERANCE_DAYS,
        "impactor_constraints": {
            "arrival_sunlight_angle_deg": "<80",
            "DV2_Mag_km_s": ">5",
        },
        "observer_constraints": {
            "earth_departure_dv_plus_DV2_Mag_km_s": "<=2",
        },
        "impact_feasible_count": len(impactor_feasible),
        "observer_feasible_count": len(observer_feasible),
        "pair_count": len(pairs),
        "paired_impactor_departure_range": [
            min(pair["impactor"]["DepartureTime"] for pair in pairs),
            max(pair["impactor"]["DepartureTime"] for pair in pairs),
        ]
        if pairs
        else None,
        "paired_impactor_arrival_range": [
            min(pair["impactor"]["ArrivalTime"] for pair in pairs),
            max(pair["impactor"]["ArrivalTime"] for pair in pairs),
        ]
        if pairs
        else None,
        "paired_observer_departure_range": [
            min(pair["observer"]["DepartureTime"] for pair in pairs),
            max(pair["observer"]["DepartureTime"] for pair in pairs),
        ]
        if pairs
        else None,
        "paired_observer_arrival_range": [
            min(pair["observer"]["ArrivalTime"] for pair in pairs),
            max(pair["observer"]["ArrivalTime"] for pair in pairs),
        ]
        if pairs
        else None,
        "best_pair_by_combined_metric": min(pairs, key=lambda pair: pair["combined_metric_km_s"])
        if pairs
        else None,
        "best_pair_by_impactor_earth_departure_dv": min(
            pairs, key=lambda pair: pair["impactor"]["earth_departure_dv_km_s"]
        )
        if pairs
        else None,
        "best_pair_closest_to_90_then_combined_metric": min(
            pairs,
            key=lambda pair: (
                abs(pair["arrival_gap_days"] - ARRIVAL_GAP_TARGET_DAYS),
                pair["combined_metric_km_s"],
            ),
        )
        if pairs
        else None,
        "top_pairs_by_combined_metric": sorted(
            pairs, key=lambda pair: pair["combined_metric_km_s"]
        )[:10],
        "top_pairs_closest_to_90": sorted(
            pairs,
            key=lambda pair: (
                abs(pair["arrival_gap_days"] - ARRIVAL_GAP_TARGET_DAYS),
                pair["combined_metric_km_s"],
            ),
        )[:10],
    }

    (OUT_DIR / "2025-WN6-mission2-summary.json").write_text(
        json.dumps(summary, ensure_ascii=False, indent=2), encoding="utf-8"
    )
    print(f"impactor feasible: {len(impactor_feasible)}")
    print(f"observer feasible: {len(observer_feasible)}")
    print(f"paired solutions: {len(pairs)}")


if __name__ == "__main__":
    main()
