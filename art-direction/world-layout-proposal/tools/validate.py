#!/usr/bin/env python3
"""Validate the bounded external opening-region layout proposal."""

from __future__ import annotations

import hashlib
import json
import math
import subprocess
import sys
import tempfile
import xml.etree.ElementTree as element_tree
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
SOURCE = ROOT / "layout-proposal.json"
BUILD = ROOT / "tools" / "build.py"
OUTPUTS = (
    "opening-region-3200x1800.svg",
    "camera-1280x720.svg",
    "camera-1280x827.svg",
)
EXPECTED_LOCATION_IDS = (
    "sickhouse",
    "healer-garden",
    "village-square",
    "village-edge",
    "stream",
    "abandoned-laundry",
    "woodland-clearing",
    "old-shrine",
    "southern-road",
    "abandoned-wagon",
    "closed-deeper-forest-path",
)
EXPECTED_PROPOSED_COORDINATES = (
    (2640, 1450),
    (2300, 1300),
    (2200, 900),
    (1700, 900),
    (1800, 500),
    (1500, 220),
    (1000, 350),
    (380, 300),
    (1500, 1400),
    (1900, 1640),
    (650, 150),
)
EXPECTED_CONNECTIONS = {
    frozenset(("village-square", "sickhouse")),
    frozenset(("village-square", "healer-garden")),
    frozenset(("village-square", "village-edge")),
    frozenset(("village-square", "southern-road")),
    frozenset(("village-edge", "stream")),
    frozenset(("stream", "abandoned-laundry")),
    frozenset(("village-edge", "woodland-clearing")),
    frozenset(("abandoned-laundry", "woodland-clearing")),
    frozenset(("woodland-clearing", "old-shrine")),
    frozenset(("old-shrine", "southern-road")),
    frozenset(("southern-road", "abandoned-wagon")),
    frozenset(("woodland-clearing", "closed-deeper-forest-path")),
}


def fail(message: str) -> None:
    raise ValueError(message)


def digest(path: Path) -> str:
    return hashlib.sha256(path.read_bytes()).hexdigest()


def validate_source(data: dict) -> None:
    if data.get("schema_version") != 1:
        fail("The proposal must use schema version 1.")

    world = data["world"]
    if (world["width"], world["height"]) != (3200, 1800):
        fail("The proposal must retain exact 3200x1800 authored-world bounds.")
    if world["movement_speed_pixels_per_second"] != 240:
        fail("The proposal must retain the exact 240px/s Seeker speed.")
    if world["path_width"] != 96:
        fail("The proposal must retain the exact 96px path width.")
    camera = world["camera"]
    if (camera["minimum_zoom"], camera["starting_zoom"], camera["maximum_zoom"]) != (
        0.8,
        1.0,
        2.0,
    ):
        fail("The proposal must retain the exact 0.8/1.0/2.0 zoom tuning.")

    anchors = data["anchors"]
    ids = tuple(anchor["id"] for anchor in anchors)
    if ids != EXPECTED_LOCATION_IDS:
        fail("Anchor order and identity must exactly match the current opening topology.")
    if len(set(ids)) != len(ids):
        fail("Anchor identifiers must be unique.")
    for anchor, expected in zip(anchors, EXPECTED_PROPOSED_COORDINATES, strict=True):
        actual = (anchor["x"], anchor["y"])
        if actual != expected:
            fail(f'{anchor["id"]} must remain at the reviewed proposal coordinate {expected}.')
        if not 24 <= actual[0] <= world["width"] - 24:
            fail(f'{anchor["id"]} lacks horizontal endpoint clearance.')
        if not 24 <= actual[1] <= world["height"] - 24:
            fail(f'{anchor["id"]} lacks vertical endpoint clearance.')

    for index, first in enumerate(anchors):
        for second in anchors[index + 1 :]:
            separation = math.hypot(first["x"] - second["x"], first["y"] - second["y"])
            if separation < 120:
                fail(f'{first["id"]} and {second["id"]} violate 120px landmark clearance.')

    connections = data["connections"]
    connection_ids = [connection["id"] for connection in connections]
    if len(connection_ids) != len(set(connection_ids)):
        fail("Connection identifiers must be unique.")
    endpoint_sets = {
        frozenset((connection["first"], connection["second"])) for connection in connections
    }
    if endpoint_sets != EXPECTED_CONNECTIONS or len(connections) != len(EXPECTED_CONNECTIONS):
        fail("Connections must exactly preserve the current twelve-edge route topology.")
    if any(
        connection["first"] not in ids or connection["second"] not in ids
        for connection in connections
    ):
        fail("Every connection endpoint must name one exact retained anchor.")
    anchors_by_id = {anchor["id"]: anchor for anchor in anchors}
    minimum_independent_route_separation = world["path_width"]
    for index, first_connection in enumerate(connections):
        first_endpoints = {first_connection["first"], first_connection["second"]}
        for second_connection in connections[index + 1 :]:
            second_endpoints = {second_connection["first"], second_connection["second"]}
            if first_endpoints & second_endpoints:
                continue
            first_start = anchors_by_id[first_connection["first"]]
            first_end = anchors_by_id[first_connection["second"]]
            second_start = anchors_by_id[second_connection["first"]]
            second_end = anchors_by_id[second_connection["second"]]
            separation = segment_distance(
                first_start,
                first_end,
                second_start,
                second_end,
            )
            if route_bands_overlap(
                first_start,
                first_end,
                second_start,
                second_end,
                minimum_independent_route_separation,
            ):
                fail(
                    f'{first_connection["id"]} and {second_connection["id"]} create an '
                    "unauthorized route-band overlap "
                    f"({separation:.3f}px separation for "
                    f"{minimum_independent_route_separation}px bands)."
                )

    zones = data["content_zones"]
    zone_ids = [zone["id"] for zone in zones]
    if len(zones) != 5 or len(zone_ids) != len(set(zone_ids)):
        fail("The proposal must retain five unique explicit content-budget zones.")
    for zone in zones:
        if zone["width"] <= 0 or zone["height"] <= 0:
            fail(f'{zone["id"]} must have positive dimensions.')
        if (
            zone["x"] < 0
            or zone["y"] < 0
            or zone["x"] + zone["width"] > world["width"]
            or zone["y"] + zone["height"] > world["height"]
        ):
            fail(f'{zone["id"]} must remain inside the proposed world.')

    frames = data["camera_frames"]
    if [(frame["width"], frame["height"]) for frame in frames] != [(1280, 720), (1280, 827)]:
        fail("Camera review frames must be exact 1280x720 and 1280x827 views.")
    for frame in frames:
        left = frame["center_x"] - frame["width"] / 2
        top = frame["center_y"] - frame["height"] / 2
        right = left + frame["width"]
        bottom = top + frame["height"]
        if left < 0 or top < 0 or right > world["width"] or bottom > world["height"]:
            fail(f'{frame["id"]} camera frame must remain inside the proposed world.')
        minimum_covering_zoom = max(frame["width"] / world["width"], frame["height"] / world["height"])
        if camera["minimum_zoom"] < minimum_covering_zoom:
            fail(f'{frame["id"]} cannot be contained at the retained minimum zoom.')


def validate_outputs() -> None:
    for name in OUTPUTS:
        path = ROOT / name
        if not path.is_file():
            fail(f"Missing generated review artifact: {name}")
        root = element_tree.parse(path).getroot()
        if root.tag != "{http://www.w3.org/2000/svg}svg":
            fail(f"{name} must be an SVG document.")
        if not root.findall("{http://www.w3.org/2000/svg}title"):
            fail(f"{name} must include an accessible title.")

    with tempfile.TemporaryDirectory(prefix="allrpcs-layout-") as temporary_directory:
        temporary = Path(temporary_directory)
        subprocess.run(
            [sys.executable, str(BUILD), "--output-dir", str(temporary)],
            check=True,
        )
        for name in OUTPUTS:
            if digest(ROOT / name) != digest(temporary / name):
                fail(f"{name} is stale or the build is not byte-deterministic.")


def segments_intersect(first: dict, second: dict, third: dict, fourth: dict) -> bool:
    def cross(start: dict, middle: dict, end: dict) -> float:
        return (middle["x"] - start["x"]) * (end["y"] - start["y"]) - (
            middle["y"] - start["y"]
        ) * (end["x"] - start["x"])

    def on_segment(start: dict, middle: dict, end: dict) -> bool:
        return (
            min(start["x"], end["x"]) <= middle["x"] <= max(start["x"], end["x"])
            and min(start["y"], end["y"]) <= middle["y"] <= max(start["y"], end["y"])
        )

    first_third = cross(first, second, third)
    first_fourth = cross(first, second, fourth)
    third_first = cross(third, fourth, first)
    third_second = cross(third, fourth, second)
    if (first_third > 0) != (first_fourth > 0) and (third_first > 0) != (third_second > 0):
        return True
    return (
        first_third == 0 and on_segment(first, third, second)
        or first_fourth == 0 and on_segment(first, fourth, second)
        or third_first == 0 and on_segment(third, first, fourth)
        or third_second == 0 and on_segment(third, second, fourth)
    )


def segment_distance(first: dict, second: dict, third: dict, fourth: dict) -> float:
    if segments_intersect(first, second, third, fourth):
        return 0.0

    return min(
        point_to_segment_distance(first, third, fourth),
        point_to_segment_distance(second, third, fourth),
        point_to_segment_distance(third, first, second),
        point_to_segment_distance(fourth, first, second),
    )


def route_bands_overlap(
    first: dict,
    second: dict,
    third: dict,
    fourth: dict,
    route_width: float,
) -> bool:
    return segment_distance(first, second, third, fourth) < route_width


def point_to_segment_distance(point: dict, start: dict, end: dict) -> float:
    delta_x = end["x"] - start["x"]
    delta_y = end["y"] - start["y"]
    length_squared = delta_x * delta_x + delta_y * delta_y
    if length_squared == 0:
        return math.hypot(point["x"] - start["x"], point["y"] - start["y"])

    projection = (
        (point["x"] - start["x"]) * delta_x
        + (point["y"] - start["y"]) * delta_y
    ) / length_squared
    clamped_projection = max(0.0, min(1.0, projection))
    closest_x = start["x"] + clamped_projection * delta_x
    closest_y = start["y"] + clamped_projection * delta_y
    return math.hypot(point["x"] - closest_x, point["y"] - closest_y)


def validate_geometry_regressions() -> None:
    horizontal_start = {"x": 0, "y": 0}
    horizontal_end = {"x": 100, "y": 0}
    cases = (
        (
            "ordinary crossing",
            {"x": 50, "y": -50},
            {"x": 50, "y": 50},
            0.0,
            True,
        ),
        (
            "collinear overlap",
            {"x": 50, "y": 0},
            {"x": 150, "y": 0},
            0.0,
            True,
        ),
        (
            "non-intersecting route-band overlap",
            {"x": 0, "y": 95},
            {"x": 100, "y": 95},
            95.0,
            True,
        ),
        (
            "separated parallel routes",
            {"x": 0, "y": 97},
            {"x": 100, "y": 97},
            97.0,
            False,
        ),
    )
    for description, third, fourth, expected_distance, expected_overlap in cases:
        actual = segment_distance(horizontal_start, horizontal_end, third, fourth)
        if not math.isclose(actual, expected_distance, abs_tol=1e-9):
            fail(
                f"Segment-distance regression failed for {description}: "
                f"expected {expected_distance}, received {actual}."
            )
        overlap = route_bands_overlap(
            horizontal_start,
            horizontal_end,
            third,
            fourth,
            route_width=96.0,
        )
        if overlap != expected_overlap:
            fail(
                f"Route-band regression failed for {description}: "
                f"expected overlap={expected_overlap}, received overlap={overlap}."
            )


def main() -> None:
    validate_geometry_regressions()
    with SOURCE.open(encoding="utf-8") as source_file:
        data = json.load(source_file)
    validate_source(data)
    validate_outputs()
    print(
        "Opening-region proposal validation passed: 11 anchors, 12 topology edges, "
        "96px independent-route separation, 5 content zones, 2 camera frames, "
        "deterministic SVG output."
    )


if __name__ == "__main__":
    main()
