#!/usr/bin/env python3
"""Build deterministic SVG review artifacts for the external opening-region proposal."""

from __future__ import annotations

import argparse
import html
import json
import math
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
SOURCE = ROOT / "layout-proposal.json"
FULL_REGION_NAME = "opening-region-3200x1800.svg"


def load_source() -> dict:
    with SOURCE.open(encoding="utf-8") as source_file:
        return json.load(source_file)


def escape(value: object) -> str:
    return html.escape(str(value), quote=True)


def point(anchor: dict) -> tuple[float, float]:
    return float(anchor["x"]), float(anchor["y"])


def route_length(first: dict, second: dict) -> float:
    first_x, first_y = point(first)
    second_x, second_y = point(second)
    return math.hypot(second_x - first_x, second_y - first_y)


def svg_header(
    view_box: tuple[float, float, float, float],
    width: int,
    height: int,
    title: str,
) -> list[str]:
    x, y, view_width, view_height = view_box
    return [
        '<?xml version="1.0" encoding="UTF-8"?>',
        (
            f'<svg xmlns="http://www.w3.org/2000/svg" width="{width}" height="{height}" '
            f'viewBox="{x:g} {y:g} {view_width:g} {view_height:g}" role="img">'
        ),
        f"  <title>{escape(title)}</title>",
        "  <defs>",
        "    <linearGradient id=\"ground\" x1=\"0\" y1=\"0\" x2=\"1\" y2=\"1\">",
        "      <stop offset=\"0\" stop-color=\"#233d35\"/>",
        "      <stop offset=\"1\" stop-color=\"#152b29\"/>",
        "    </linearGradient>",
        "    <pattern id=\"grid\" width=\"160\" height=\"160\" patternUnits=\"userSpaceOnUse\">",
        (
            "      <path d=\"M 160 0 L 0 0 0 160\" fill=\"none\" stroke=\"#d8ead8\" "
            "stroke-opacity=\"0.08\" stroke-width=\"3\"/>"
        ),
        "    </pattern>",
        "    <filter id=\"shadow\" x=\"-20%\" y=\"-20%\" width=\"140%\" height=\"140%\">",
        "      <feDropShadow dx=\"0\" dy=\"8\" stdDeviation=\"10\" flood-color=\"#07100e\" flood-opacity=\"0.45\"/>",
        "    </filter>",
        "  </defs>",
    ]


def add_world_base(lines: list[str], data: dict) -> None:
    world = data["world"]
    width = world["width"]
    height = world["height"]
    lines.extend(
        [
            f'  <rect x="0" y="0" width="{width}" height="{height}" fill="url(#ground)"/>',
            f'  <rect x="0" y="0" width="{width}" height="{height}" fill="url(#grid)"/>',
            (
                f'  <rect x="16" y="16" width="{width - 32}" height="{height - 32}" '
                'fill="none" stroke="#d7c791" stroke-width="12"/>'
            ),
        ]
    )


def add_content_zones(lines: list[str], data: dict) -> None:
    for zone in data["content_zones"]:
        lines.append(
            (
                f'  <rect x="{zone["x"]}" y="{zone["y"]}" width="{zone["width"]}" '
                f'height="{zone["height"]}" rx="32" fill="#d7c791" fill-opacity="0.045" '
                'stroke="#d7c791" stroke-opacity="0.38" stroke-width="8" stroke-dasharray="26 20"/>'
            )
        )
        lines.append(
            (
                f'  <text x="{zone["x"] + 28}" y="{zone["y"] + 48}" '
                'font-family="Arial, sans-serif" font-size="34" font-weight="700" '
                f'fill="#d7c791" fill-opacity="0.78">{escape(zone["label"])}</text>'
            )
        )


def add_routes(lines: list[str], data: dict, anchors: dict[str, dict]) -> None:
    speed = data["world"]["movement_speed_pixels_per_second"]
    path_width = data["world"]["path_width"]
    for connection in data["connections"]:
        first = anchors[connection["first"]]
        second = anchors[connection["second"]]
        first_x, first_y = point(first)
        second_x, second_y = point(second)
        length = route_length(first, second)
        seconds = length / speed
        midpoint_x = (first_x + second_x) / 2
        midpoint_y = (first_y + second_y) / 2
        lines.extend(
            [
                (
                    f'  <line x1="{first_x:g}" y1="{first_y:g}" x2="{second_x:g}" y2="{second_y:g}" '
                    f'stroke="#b78b55" stroke-opacity="0.28" stroke-width="{path_width}" '
                    'stroke-linecap="round"/>'
                ),
                (
                    f'  <line x1="{first_x:g}" y1="{first_y:g}" x2="{second_x:g}" y2="{second_y:g}" '
                    'stroke="#efcf8f" stroke-width="12" stroke-linecap="round"/>'
                ),
                (
                    f'  <rect x="{midpoint_x - 72:g}" y="{midpoint_y - 23:g}" width="144" height="46" '
                    'rx="23" fill="#102420" stroke="#efcf8f" stroke-opacity="0.55" stroke-width="3"/>'
                ),
                (
                    f'  <text x="{midpoint_x:g}" y="{midpoint_y + 11:g}" text-anchor="middle" '
                    'font-family="Arial, sans-serif" font-size="27" font-weight="700" fill="#f8e7bd">'
                    f'{length:.0f}px / {seconds:.1f}s</text>'
                ),
            ]
        )


def add_anchors(lines: list[str], data: dict) -> None:
    for index, anchor in enumerate(data["anchors"], start=1):
        x, y = point(anchor)
        label_x = x + float(anchor["label_dx"])
        label_y = y + float(anchor["label_dy"])
        lines.extend(
            [
                (
                    f'  <circle cx="{x:g}" cy="{y:g}" r="29" fill="#f3cf76" '
                    'stroke="#102420" stroke-width="10" filter="url(#shadow)"/>'
                ),
                (
                    f'  <text x="{x:g}" y="{y + 10:g}" text-anchor="middle" '
                    'font-family="Arial, sans-serif" font-size="28" font-weight="800" fill="#102420">'
                    f'{index}</text>'
                ),
                (
                    f'  <text x="{label_x:g}" y="{label_y:g}" font-family="Arial, sans-serif" '
                    f'font-size="36" font-weight="700" fill="#f6f0d8">{escape(anchor["label"])}</text>'
                ),
                (
                    f'  <text x="{label_x:g}" y="{label_y + 34:g}" font-family="Arial, sans-serif" '
                    'font-size="25" fill="#b8d4c3">'
                    f'({x:g}, {y:g})</text>'
                ),
            ]
        )


def add_title(lines: list[str], data: dict) -> None:
    lines.extend(
        [
            (
                '  <rect x="72" y="-154" width="1500" height="126" rx="28" '
                'fill="#102420" fill-opacity="0.94" stroke="#d7c791" stroke-width="4"/>'
            ),
            (
                '  <text x="112" y="-100" font-family="Arial, sans-serif" font-size="45" '
                f'font-weight="800" fill="#f6f0d8">{escape(data["title"])}</text>'
            ),
            (
                '  <text x="112" y="-54" font-family="Arial, sans-serif" font-size="28" '
                'fill="#b8d4c3">External planning artifact · 96px corridors · '
                'route labels show distance / time at 240px/s</text>'
            ),
        ]
    )


def render_full_region(data: dict) -> str:
    world = data["world"]
    lines = svg_header(
        (0, -180, world["width"], world["height"] + 180),
        1600,
        990,
        "Labelled 3200 by 1800 opening-region proposal",
    )
    add_world_base(lines, data)
    add_content_zones(lines, data)
    anchors = {anchor["id"]: anchor for anchor in data["anchors"]}
    add_routes(lines, data, anchors)
    add_anchors(lines, data)
    add_title(lines, data)
    lines.append("</svg>")
    return "\n".join(lines) + "\n"


def render_camera_frame(data: dict, frame: dict) -> str:
    frame_x = float(frame["center_x"]) - float(frame["width"]) / 2
    frame_y = float(frame["center_y"]) - float(frame["height"]) / 2
    lines = svg_header(
        (frame_x, frame_y, frame["width"], frame["height"]),
        int(frame["width"]),
        int(frame["height"]),
        frame["label"],
    )
    add_world_base(lines, data)
    add_content_zones(lines, data)
    anchors = {anchor["id"]: anchor for anchor in data["anchors"]}
    add_routes(lines, data, anchors)
    add_anchors(lines, data)
    lines.append("</svg>")
    return "\n".join(lines) + "\n"


def write_outputs(output_dir: Path) -> None:
    data = load_source()
    output_dir.mkdir(parents=True, exist_ok=True)
    (output_dir / FULL_REGION_NAME).write_text(render_full_region(data), encoding="utf-8")
    for frame in data["camera_frames"]:
        name = f'camera-{frame["width"]}x{frame["height"]}.svg'
        (output_dir / name).write_text(render_camera_frame(data, frame), encoding="utf-8")


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("--output-dir", type=Path, default=ROOT)
    args = parser.parse_args()
    write_outputs(args.output_dir)


if __name__ == "__main__":
    main()
