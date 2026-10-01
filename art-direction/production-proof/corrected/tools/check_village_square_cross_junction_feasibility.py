#!/usr/bin/env python3
"""Build or validate the proof-only Village Square cross-junction review."""

from __future__ import annotations

import argparse
import hashlib
import io
import json
import math
import re
from pathlib import Path

from PIL import Image, ImageDraw


ROOT = Path(__file__).resolve().parents[4]
PROOF = ROOT / "art-direction/production-proof/corrected"
REVIEW = PROOF / "review"
LAYOUT = ROOT / "game/OpeningRegionLayoutGeometry.cs"
SQUARE_ID = "village-square"
NEIGHBOUR_IDS = ("sickhouse", "healer-garden", "southern-road", "village-edge")
VIEW_SPECS = (
    ("wide", 1280, 720),
    ("tall", 1280, 827),
)
ZOOMS = (0.8, 1.0, 2.0)
EXPECTED_REVIEW_SHA256 = {
    "village-square-cross-junction-0.8x-wide.png":
        "56d6c26a524688e36ea90c401892925ed68de1c1c42ed96618b081e57de173ad",
    "village-square-cross-junction-0.8x-tall.png":
        "f434731d3693683231821bdb0b9aebd98e45c6b97f4233bcceecdffc90330a38",
    "village-square-cross-junction-1.0x-wide.png":
        "ec7e61049fddff52d4c6496b267e0a72055bf38879a450fe67dc76b12c4b5a1d",
    "village-square-cross-junction-1.0x-tall.png":
        "b2b39ba86ef05ce2c1b9a3a928d44d4b47f4e2c8aa626aa46bbffbd0a162bf0a",
    "village-square-cross-junction-2.0x-wide.png":
        "61d5fc22034290eb5e4e9ef76a1488a0a4138932e29a8f52ab61da1f6c373826",
    "village-square-cross-junction-2.0x-tall.png":
        "279b5eeeae28e730a45d336bb24f06ab216ef4d2e20f8722c4a55567cef11a6c",
}
EXPECTED_PROOF_REVIEW_SHA256 = {
    "village-square-junction-0.8x-wide.png":
        "fe1d1c26c217a5325a124c9017d9a36eb8de9ed66c1a990180723b62bb2e1de1",
    "village-square-junction-0.8x-tall.png":
        "ca9714dde492dc0b03db510380942e4638a6a957399d20eebf13beaa5fbe7b18",
    "village-square-junction-1.0x-wide.png":
        "d2e176564d7cb02e4a43988c5133c5158f95782780f0478a725f7f9df49f10ad",
    "village-square-junction-1.0x-tall.png":
        "480601e38c122964e248737722265d08ae881551267d98d317e5dcdcef70fa0b",
    "village-square-junction-2.0x-wide.png":
        "2093647a72744478e5f2b25907fb90aef814888e79768ec339937aed6389f29e",
    "village-square-junction-2.0x-tall.png":
        "5738eea59f42ca20ef4d262be3e32a2d1c54d7339f8a1d902c03e569e841db16",
}
ANCHOR_PATTERN = re.compile(
    r'\["(?P<id>[^"]+)"\]\s*=\s*new\('
    r'(?P<x>[\d.]+)f,\s*(?P<y>[\d.]+)f\)'
)


def require(condition: bool, message: str) -> None:
    """Reject source or review drift with one actionable message."""

    if not condition:
        raise AssertionError(message)


def sha256_bytes(data: bytes) -> str:
    """Return the lowercase SHA-256 digest for an in-memory review."""

    return hashlib.sha256(data).hexdigest()


def heading(first: tuple[float, float], second: tuple[float, float]) -> float:
    """Return one screen-space route heading in degrees."""

    return math.degrees(math.atan2(second[1] - first[1], second[0] - first[0])) % 360.0


def read_geometry() -> tuple[dict[str, tuple[float, float]], float]:
    """Bind the proof to the exact live and approved layout geometry."""

    source = LAYOUT.read_text(encoding="utf-8")
    anchors = {
        match["id"]: (float(match["x"]), float(match["y"]))
        for match in ANCHOR_PATTERN.finditer(source)
    }
    required = (SQUARE_ID, *NEIGHBOUR_IDS)
    require(all(anchor_id in anchors for anchor_id in required), "Village Square anchor missing")
    require(
        re.search(r"PathWidthPixels\s*=\s*96\.0f", source) is not None,
        "live path width changed",
    )
    connection_source = source.split("internal static IReadOnlyList", 1)[1]
    connections = re.findall(r'\("([^"]+)", "([^"]+)"\)', connection_source)
    square_neighbours = {
        second if first == SQUARE_ID else first
        for first, second in connections
        if SQUARE_ID in (first, second)
    }
    require(
        square_neighbours == set(NEIGHBOUR_IDS),
        "live Village Square connections changed",
    )

    proposal = json.loads(
        (ROOT / "art-direction/world-layout-proposal/layout-proposal.json").read_text(
            encoding="utf-8"
        )
    )
    proposal_anchors = {
        entry["id"]: (float(entry["x"]), float(entry["y"]))
        for entry in proposal["anchors"]
    }
    require(
        all(anchors[anchor_id] == proposal_anchors[anchor_id] for anchor_id in required),
        "live Village Square geometry differs from the approved proposal",
    )
    require(proposal["world"]["path_width"] == 96, "approved path width changed")
    return anchors, float(proposal["world"]["path_width"])


def read_cross_junction() -> Image.Image:
    """Load and authenticate the exact manifest-bound cross-junction export."""

    manifest = json.loads((PROOF / "asset-manifest.json").read_text(encoding="utf-8"))
    asset = next(
        entry for entry in manifest["assets"] if entry["id"] == "terrain.path_cross_junction"
    )
    path = PROOF / asset["export"]
    require(
        hashlib.sha256(path.read_bytes()).hexdigest() == asset["sha256"],
        "cross hash drifted",
    )
    image = Image.open(path).convert("RGBA")
    require(image.size == (256, 256), "cross-junction dimensions changed")
    require(asset["pivot"] == {"x": 128, "y": 128}, "cross-junction pivot changed")
    return image


def read_village_square_junction() -> tuple[Image.Image, dict]:
    """Load the exact tailored proof and its manifest-bound geometry."""

    manifest = json.loads((PROOF / "asset-manifest.json").read_text(encoding="utf-8"))
    proof = manifest["village_square_junction_proof"]
    asset = next(entry for entry in manifest["assets"] if entry["id"] == proof["asset"])
    path = PROOF / asset["export"]
    require(
        hashlib.sha256(path.read_bytes()).hexdigest() == asset["sha256"],
        "tailored junction hash drifted",
    )
    image = Image.open(path).convert("RGBA")
    require(image.size == (512, 512), "tailored junction dimensions changed")
    require(asset["pivot"] == {"x": 256, "y": 256}, "tailored junction pivot changed")
    return image, proof


def solve_rotation(
    anchors: dict[str, tuple[float, float]],
) -> tuple[list[float], float, list[float]]:
    """Return the minimax rigid rotation and residual port mismatches."""

    square = anchors[SQUARE_ID]
    route_headings = sorted(heading(square, anchors[anchor_id]) for anchor_id in NEIGHBOUR_IDS)
    port_headings = (0.0, 90.0, 180.0, 270.0)
    offsets = [route - port for route, port in zip(route_headings, port_headings)]
    rotation = (max(offsets) + min(offsets)) / 2.0
    residuals = [route - (port + rotation) for route, port in zip(route_headings, port_headings)]
    require(max(abs(residual) for residual in residuals) > 70.0, "cross junction now fits")
    return route_headings, rotation, residuals


def world_to_view(
    point: tuple[float, float],
    center: tuple[float, float],
    zoom: float,
    size: tuple[int, int],
) -> tuple[float, float]:
    """Project one world point into a square-centred external review view."""

    return (
        ((point[0] - center[0]) * zoom) + (size[0] / 2.0),
        ((point[1] - center[1]) * zoom) + (size[1] / 2.0),
    )


def tile_background(size: tuple[int, int], zoom: float, quiet_moss: Image.Image) -> Image.Image:
    """Fill one review with the exact proof moss at the selected display scale."""

    tile_size = max(1, round(quiet_moss.width * zoom))
    tile = quiet_moss.resize((tile_size, tile_size), Image.Resampling.LANCZOS)
    canvas = Image.new("RGBA", size, (71, 100, 67, 255))
    for x in range(0, size[0], tile_size):
        for y in range(0, size[1], tile_size):
            canvas.alpha_composite(tile, (x, y))
    return canvas


def render_review(
    anchors: dict[str, tuple[float, float]],
    path_width: float,
    cross: Image.Image,
    rotation: float,
    zoom: float,
    size: tuple[int, int],
) -> bytes:
    """Render actual route bands beneath the best rigid cross placement."""

    quiet_moss = Image.open(PROOF / "exports/terrain/quiet-moss.png").convert("RGBA")
    canvas = tile_background(size, zoom, quiet_moss)
    draw = ImageDraw.Draw(canvas, "RGBA")
    center = anchors[SQUARE_ID]
    view_center = (size[0] / 2.0, size[1] / 2.0)
    route_width = max(1, round(path_width * zoom))
    for neighbour_id in NEIGHBOUR_IDS:
        endpoint = world_to_view(anchors[neighbour_id], center, zoom, size)
        draw.line((view_center, endpoint), fill=(244, 199, 89, 190), width=route_width)
        draw.line((view_center, endpoint), fill=(92, 55, 34, 220), width=max(2, round(3 * zoom)))

    rotated = cross.rotate(-rotation, resample=Image.Resampling.BICUBIC, expand=True)
    scaled_size = tuple(
        max(1, round(dimension * zoom)) for dimension in rotated.size
    )
    scaled = rotated.resize(scaled_size, Image.Resampling.LANCZOS)
    canvas.alpha_composite(
        scaled,
        (round(view_center[0] - scaled.width / 2), round(view_center[1] - scaled.height / 2)),
    )

    port_length = 220.0 * zoom
    for port in (0.0, 90.0, 180.0, 270.0):
        angle = math.radians(port + rotation)
        endpoint = (
            view_center[0] + (math.cos(angle) * port_length),
            view_center[1] + (math.sin(angle) * port_length),
        )
        draw.line((view_center, endpoint), fill=(64, 220, 232, 230), width=max(2, round(4 * zoom)))
    radius = max(4, round(7 * zoom))
    draw.ellipse(
        (
            view_center[0] - radius,
            view_center[1] - radius,
            view_center[0] + radius,
            view_center[1] + radius,
        ),
        fill=(255, 255, 255, 255),
        outline=(24, 32, 30, 255),
        width=max(1, round(2 * zoom)),
    )
    output = io.BytesIO()
    canvas.convert("RGB").save(output, format="PNG", optimize=False, compress_level=9)
    return output.getvalue()


def render_proof_review(
    anchors: dict[str, tuple[float, float]],
    path_width: float,
    junction: Image.Image,
    zoom: float,
    size: tuple[int, int],
) -> bytes:
    """Render the tailored junction over the four exact live route bands."""

    quiet_moss = Image.open(PROOF / "exports/terrain/quiet-moss.png").convert("RGBA")
    canvas = tile_background(size, zoom, quiet_moss)
    draw = ImageDraw.Draw(canvas, "RGBA")
    center = anchors[SQUARE_ID]
    view_center = (size[0] / 2.0, size[1] / 2.0)
    route_width = max(1, round(path_width * zoom))
    for neighbour_id in NEIGHBOUR_IDS:
        endpoint = world_to_view(anchors[neighbour_id], center, zoom, size)
        draw.line((view_center, endpoint), fill=(244, 199, 89, 190), width=route_width)
        draw.line(
            (view_center, endpoint),
            fill=(92, 55, 34, 220),
            width=max(2, round(3 * zoom)),
        )

    straight_path = Image.open(PROOF / "exports/terrain/straight-path.png").convert("RGBA")
    scaled_path = straight_path.resize(
        (
            max(1, round(straight_path.width * zoom)),
            max(1, round(straight_path.height * zoom)),
        ),
        Image.Resampling.LANCZOS,
    )
    for heading_value in sorted(
        heading(center, anchors[neighbour_id]) for neighbour_id in NEIGHBOUR_IDS
    ):
        continuation = render_path_leg_reference(
            scaled_path,
            size,
            view_center,
            heading_value,
            max(size) * 1.5,
        )
        canvas.alpha_composite(continuation)

    scaled_size = tuple(max(1, round(dimension * zoom)) for dimension in junction.size)
    scaled = junction.resize(scaled_size, Image.Resampling.LANCZOS)
    canvas.alpha_composite(
        scaled,
        (round(view_center[0] - scaled.width / 2), round(view_center[1] - scaled.height / 2)),
    )
    radius = max(4, round(7 * zoom))
    draw.ellipse(
        (
            view_center[0] - radius,
            view_center[1] - radius,
            view_center[0] + radius,
            view_center[1] + radius,
        ),
        fill=(255, 255, 255, 255),
        outline=(24, 32, 30, 255),
        width=max(1, round(2 * zoom)),
    )
    output = io.BytesIO()
    canvas.convert("RGB").save(output, format="PNG", optimize=False, compress_level=9)
    return output.getvalue()


def ray_exit(
    pivot: tuple[float, float], heading_degrees: float, canvas_size: int
) -> tuple[float, tuple[float, float]]:
    """Return distance and point where one forward ray exits a square canvas."""

    angle = math.radians(heading_degrees)
    direction = (math.cos(angle), math.sin(angle))
    candidates = []
    if direction[0] > 0.0:
        candidates.append((canvas_size - pivot[0]) / direction[0])
    elif direction[0] < 0.0:
        candidates.append((0.0 - pivot[0]) / direction[0])
    if direction[1] > 0.0:
        candidates.append((canvas_size - pivot[1]) / direction[1])
    elif direction[1] < 0.0:
        candidates.append((0.0 - pivot[1]) / direction[1])
    distance = min(candidate for candidate in candidates if candidate >= 0.0)
    return distance, (
        pivot[0] + (direction[0] * distance),
        pivot[1] + (direction[1] * distance),
    )


def render_path_leg_reference(
    path: Image.Image,
    canvas_size: tuple[int, int],
    start: tuple[float, float],
    heading_degrees: float,
    length: float,
) -> Image.Image:
    """Independently reconstruct one repeated straight-path leg."""

    source_height = max(1, math.ceil(length) + 2)
    source = Image.new("RGBA", (path.width, source_height), (0, 0, 0, 0))
    for y in range(0, source_height, path.height):
        source.alpha_composite(path, (0, y))
    angle = math.radians(heading_degrees)
    direction = (math.cos(angle), math.sin(angle))
    right = (math.sin(angle), -math.cos(angle))
    offset_x = (path.width / 2.0) - (right[0] * start[0]) - (right[1] * start[1])
    offset_y = -(direction[0] * start[0]) - (direction[1] * start[1])
    return source.transform(
        canvas_size,
        Image.Transform.AFFINE,
        (right[0], right[1], offset_x, direction[0], direction[1], offset_y),
        resample=Image.Resampling.BICUBIC,
    )


def validate_tailored_geometry(
    junction: Image.Image,
    proof: dict,
    route_headings: list[float],
) -> None:
    """Prove exact connectors and containment inside the four live route bands."""

    require(
        len(proof["headings_degrees"]) == len(route_headings)
        and all(
            math.isclose(expected, actual, abs_tol=1e-6)
            for expected, actual in zip(proof["headings_degrees"], route_headings)
        ),
        "tailored headings drifted",
    )
    require(
        proof["path_width_pixels"] == 96 and proof["port_depth_pixels"] == 32,
        "tailored connector geometry drifted",
    )
    pivot = (256.0, 256.0)
    path = Image.open(PROOF / "exports/terrain/straight-path.png").convert("RGBA")
    references = [
        render_path_leg_reference(path, junction.size, pivot, heading_value, 768.0)
        for heading_value in route_headings
    ]
    junction_pixels = junction.load()
    alpha = junction.getchannel("A")
    visible_bounds = alpha.getbbox()
    require(visible_bounds is not None, "tailored junction is empty")
    exits = [ray_exit(pivot, heading_value, junction.width) for heading_value in route_headings]
    for y in range(visible_bounds[1], visible_bounds[3]):
        for x in range(visible_bounds[0], visible_bounds[2]):
            if junction_pixels[x, y][3] == 0:
                continue
            sample = (x + 0.5, y + 0.5)
            within_route = False
            relative = (sample[0] - pivot[0], sample[1] - pivot[1])
            for heading_value in route_headings:
                angle = math.radians(heading_value)
                direction = (math.cos(angle), math.sin(angle))
                right = (direction[1], -direction[0])
                forward = (
                    (relative[0] * direction[0])
                    + (relative[1] * direction[1])
                )
                lateral = abs(
                    (relative[0] * right[0]) + (relative[1] * right[1])
                )
                within_route = within_route or (forward >= -1.0 and lateral <= 50.0)
            require(
                within_route,
                f"tailored junction escaped retained route bands at {(x, y)}",
            )

    for reference, (exit_distance, _), heading_value in zip(
        references, exits, route_headings
    ):
        angle = math.radians(heading_value)
        direction = (math.cos(angle), math.sin(angle))
        right = (direction[1], -direction[0])
        reference_pixels = reference.load()
        compared = 0
        for y in range(junction.height):
            for x in range(junction.width):
                relative = ((x + 0.5) - pivot[0], (y + 0.5) - pivot[1])
                forward = (relative[0] * direction[0]) + (relative[1] * direction[1])
                lateral = abs((relative[0] * right[0]) + (relative[1] * right[1]))
                if forward < exit_distance - 32.0 or lateral > 50.0:
                    continue
                require(
                    junction_pixels[x, y] == reference_pixels[x, y],
                    f"tailored {heading_value:.6f}-degree connector drifted at {(x, y)}",
                )
                compared += 1
        require(compared > 2500, f"tailored {heading_value:.6f}-degree port is undersized")


def main() -> None:
    """Write requested views or validate their exact source-bound bytes."""

    parser = argparse.ArgumentParser()
    parser.add_argument("--write", action="store_true", help="regenerate the six review PNGs")
    parser.add_argument(
        "--write-proof",
        action="store_true",
        help="regenerate the six tailored-junction review PNGs",
    )
    args = parser.parse_args()
    anchors, path_width = read_geometry()
    cross = read_cross_junction()
    headings, rotation, residuals = solve_rotation(anchors)
    junction, proof = read_village_square_junction()
    validate_tailored_geometry(junction, proof, headings)
    rendered: dict[str, bytes] = {}
    rendered_proof: dict[str, bytes] = {}
    for zoom in ZOOMS:
        for label, width, height in VIEW_SPECS:
            name = f"village-square-cross-junction-{zoom:.1f}x-{label}.png"
            rendered[name] = render_review(
                anchors,
                path_width,
                cross,
                rotation,
                zoom,
                (width, height),
            )
            proof_name = f"village-square-junction-{zoom:.1f}x-{label}.png"
            rendered_proof[proof_name] = render_proof_review(
                anchors,
                path_width,
                junction,
                zoom,
                (width, height),
            )

    if args.write:
        REVIEW.mkdir(parents=True, exist_ok=True)
        for name, data in rendered.items():
            (REVIEW / name).write_bytes(data)
            print(f'{name}: {sha256_bytes(data)}')
    elif not args.write_proof:
        require(all(EXPECTED_REVIEW_SHA256.values()), "review hashes are not recorded")
        actual_reviews = {
            path.name
            for path in REVIEW.glob("village-square-cross-junction-*.png")
        }
        require(
            actual_reviews == set(EXPECTED_REVIEW_SHA256),
            "Village Square review set drifted",
        )
        for name, expected_digest in EXPECTED_REVIEW_SHA256.items():
            path = REVIEW / name
            require(path.exists(), f"missing review: {name}")
            require(path.read_bytes() == rendered[name], f"non-deterministic review drift: {name}")
            require(
                sha256_bytes(rendered[name]) == expected_digest,
                f"review hash drifted: {name}",
            )

    if args.write_proof:
        REVIEW.mkdir(parents=True, exist_ok=True)
        for name, data in rendered_proof.items():
            (REVIEW / name).write_bytes(data)
            print(f"{name}: {sha256_bytes(data)}")
    else:
        require(
            all(EXPECTED_PROOF_REVIEW_SHA256.values()),
            "tailored review hashes are not recorded",
        )
        actual_proof_reviews = {
            path.name for path in REVIEW.glob("village-square-junction-*.png")
        }
        require(
            actual_proof_reviews == set(EXPECTED_PROOF_REVIEW_SHA256),
            "tailored Village Square review set drifted",
        )
        for name, expected_digest in EXPECTED_PROOF_REVIEW_SHA256.items():
            path = REVIEW / name
            require(path.exists(), f"missing tailored review: {name}")
            require(
                path.read_bytes() == rendered_proof[name],
                f"non-deterministic tailored review drift: {name}",
            )
            require(
                sha256_bytes(rendered_proof[name]) == expected_digest,
                f"tailored review hash drifted: {name}",
            )

    print(
        "Village Square route headings: "
        + ", ".join(f"{value:.6f}" for value in headings)
    )
    print(f"Best rigid minimax rotation: {rotation:.6f} degrees")
    print("Port residuals: " + ", ".join(f"{value:.6f}" for value in residuals))
    print(
        "Minimum worst port mismatch: "
        f"{max(abs(value) for value in residuals):.6f} degrees"
    )
    print("Result: the exact unscaled cross-junction cannot match the four live route headings.")
    print(
        "Tailored proof: four exact straight-path connectors match the live headings and remain "
        "inside the retained 96px route bands."
    )


if __name__ == "__main__":
    main()
