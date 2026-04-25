#!/usr/bin/env python3
import csv
import json
from datetime import datetime
from pathlib import Path
from statistics import mean


ROOT = Path(__file__).resolve().parents[2]
RESULT_DIR = Path(__file__).resolve().parent
RESPONSE_PATH = RESULT_DIR / "transfer-response.json"
CSV_PATH = RESULT_DIR / "earth2xf262-dv1.csv"
SVG_PATH = RESULT_DIR / "earth2xf262-dv1.svg"
Y_MIN = 0.0
Y_MAX = 1000.0


def parse_time(value):
    return datetime.fromisoformat(value.replace("Z", "+00:00"))


def svg_escape(value):
    return (
        str(value)
        .replace("&", "&amp;")
        .replace("<", "&lt;")
        .replace(">", "&gt;")
        .replace('"', "&quot;")
    )


def build_svg(rows):
    width = 1200
    height = 720
    left = 95
    right = 45
    top = 62
    bottom = 95
    plot_w = width - left - right
    plot_h = height - top - bottom

    times = [parse_time(row["DepartureTime"]) for row in rows]
    values = [float(row["DV1_Mag"]) for row in rows]
    t0 = min(times)
    t1 = max(times)
    span_seconds = max((t1 - t0).total_seconds(), 1.0)

    def x_for(t):
        return left + ((t - t0).total_seconds() / span_seconds) * plot_w

    def y_for(v):
        clipped = min(max(v, Y_MIN), Y_MAX)
        return top + (1 - ((clipped - Y_MIN) / (Y_MAX - Y_MIN))) * plot_h

    points = " ".join(
        f"{x_for(t):.2f},{y_for(v):.2f}" for t, v in zip(times, values)
    )
    best_idx = min(range(len(rows)), key=lambda idx: values[idx])
    best_x = x_for(times[best_idx])
    best_y = y_for(values[best_idx])
    clipped_count = sum(1 for value in values if value > Y_MAX)

    y_ticks = []
    for idx in range(6):
        value = Y_MIN + (Y_MAX - Y_MIN) * idx / 5
        y_ticks.append((value, y_for(value)))

    month_ticks = []
    last_label = None
    for t in times:
        label = t.strftime("%Y-%m")
        if label != last_label:
            month_ticks.append((label, x_for(t)))
            last_label = label

    title = "Earth to 2015 XF261 Lambert Transfer"
    subtitle = "Departure DV1 magnitude, arrival fixed at 2029-04-10, y-axis 0-1000 m/s"
    avg = mean(values)

    parts = [
        f'<svg xmlns="http://www.w3.org/2000/svg" width="{width}" height="{height}" viewBox="0 0 {width} {height}">',
        "<style>",
        "text{font-family:Arial,Helvetica,sans-serif;fill:#1f2937}",
        ".grid{stroke:#e5e7eb;stroke-width:1}",
        ".axis{stroke:#374151;stroke-width:1.5}",
        ".curve{fill:none;stroke:#2563eb;stroke-width:3}",
        ".best{fill:#dc2626;stroke:white;stroke-width:2}",
        "</style>",
        '<rect width="100%" height="100%" fill="white"/>',
        f'<text x="{width/2}" y="28" text-anchor="middle" font-size="22" font-weight="700">{svg_escape(title)}</text>',
        f'<text x="{width/2}" y="52" text-anchor="middle" font-size="14">{svg_escape(subtitle)}</text>',
    ]

    for value, y in y_ticks:
        parts.append(f'<line class="grid" x1="{left}" y1="{y:.2f}" x2="{width-right}" y2="{y:.2f}"/>')
        parts.append(f'<text x="{left-12}" y="{y+4:.2f}" text-anchor="end" font-size="12">{value:.0f}</text>')

    for label, x in month_ticks:
        parts.append(f'<line class="grid" x1="{x:.2f}" y1="{top}" x2="{x:.2f}" y2="{height-bottom}"/>')
        parts.append(f'<text x="{x:.2f}" y="{height-bottom+24}" text-anchor="middle" font-size="12">{label}</text>')

    parts.extend(
        [
            f'<line class="axis" x1="{left}" y1="{height-bottom}" x2="{width-right}" y2="{height-bottom}"/>',
            f'<line class="axis" x1="{left}" y1="{top}" x2="{left}" y2="{height-bottom}"/>',
            f'<polyline class="curve" points="{points}"/>',
            f'<circle class="best" cx="{best_x:.2f}" cy="{best_y:.2f}" r="6"/>',
            f'<text x="{best_x+12:.2f}" y="{best_y-10:.2f}" font-size="13" font-weight="700">min {values[best_idx]:.3f} m/s</text>',
            f'<text x="{best_x+12:.2f}" y="{best_y+8:.2f}" font-size="12">{times[best_idx].strftime("%Y-%m-%d")}</text>',
            f'<text x="{width/2}" y="{height-20}" text-anchor="middle" font-size="14">Earth departure date (UTC)</text>',
            f'<text transform="translate(24 {height/2}) rotate(-90)" text-anchor="middle" font-size="14">DV1 magnitude (m/s)</text>',
            f'<text x="{width-right}" y="{top+18}" text-anchor="end" font-size="12">points: {len(rows)}, mean: {avg:.3f} m/s, clipped above 1000: {clipped_count}</text>',
            "</svg>",
        ]
    )
    return "\n".join(parts)


def main():
    with RESPONSE_PATH.open("r", encoding="utf-8") as handle:
        payload = json.load(handle)

    if not payload.get("IsSuccess"):
        raise SystemExit(f"API response failed: {payload.get('Message')}")

    rows = sorted(payload.get("TransferResults", []), key=lambda row: row["DepartureTime"])
    if not rows:
        raise SystemExit("API response did not contain TransferResults")

    with CSV_PATH.open("w", encoding="utf-8", newline="") as handle:
        writer = csv.DictWriter(
            handle,
            fieldnames=["DepartureTime", "ArrivalTime", "DV1_Mag", "DV2_Mag"],
        )
        writer.writeheader()
        for row in rows:
            writer.writerow(
                {
                    "DepartureTime": row["DepartureTime"],
                    "ArrivalTime": row["ArrivalTime"],
                    "DV1_Mag": f'{float(row["DV1_Mag"]):.9f}',
                    "DV2_Mag": f'{float(row["DV2_Mag"]):.9f}',
                }
            )

    SVG_PATH.write_text(build_svg(rows), encoding="utf-8")

    best = min(rows, key=lambda row: float(row["DV1_Mag"]))
    worst = max(rows, key=lambda row: float(row["DV1_Mag"]))
    clipped_count = sum(1 for row in rows if float(row["DV1_Mag"]) > Y_MAX)
    print(f"rows={len(rows)}")
    print(f"departure_start={rows[0]['DepartureTime']}")
    print(f"departure_stop={rows[-1]['DepartureTime']}")
    print(f"arrival_time={rows[0]['ArrivalTime']}")
    print(f"y_axis_min_mps={Y_MIN:.0f}")
    print(f"y_axis_max_mps={Y_MAX:.0f}")
    print(f"clipped_above_1000={clipped_count}")
    print(f"min_dv1_mps={float(best['DV1_Mag']):.6f}")
    print(f"min_dv1_departure={best['DepartureTime']}")
    print(f"max_dv1_mps={float(worst['DV1_Mag']):.6f}")
    print(f"max_dv1_departure={worst['DepartureTime']}")
    print(f"csv={CSV_PATH.relative_to(ROOT)}")
    print(f"svg={SVG_PATH.relative_to(ROOT)}")


if __name__ == "__main__":
    if not RESPONSE_PATH.exists():
        raise SystemExit(f"Missing API response: {RESPONSE_PATH}")
    main()
