#!/usr/bin/env python3
"""Validate the bounded Direction A corrected-pipeline proof."""

from __future__ import annotations

import hashlib
import json
import zipfile
from pathlib import Path

from PIL import Image, ImageChops, ImageOps, ImageStat


ROOT = Path(__file__).resolve().parents[1]
MANIFEST = ROOT / "asset-manifest.json"


def require(condition: bool, message: str) -> None:
    """Raise one concise validation error when a proof invariant is false."""

    if not condition:
        raise AssertionError(message)


def sha256(path: Path) -> str:
    """Return one file's SHA-256 digest."""

    digest = hashlib.sha256()
    with path.open("rb") as stream:
        for chunk in iter(lambda: stream.read(1024 * 1024), b""):
            digest.update(chunk)
    return digest.hexdigest()


def validate_alpha(path: Path, image: Image.Image, category: str) -> None:
    """Require an RGBA export with no hidden RGB and useful alpha coverage."""

    require(image.mode == "RGBA", f"{path}: expected RGBA, got {image.mode}")
    alpha = image.getchannel("A")
    histogram = alpha.histogram()
    require(sum(histogram[1:]) > 0, f"{path}: no visible pixels")
    require(histogram[255] > 0, f"{path}: no fully opaque pixels")

    red, green, blue, _ = image.split()
    hidden = ImageOps.invert(alpha)
    fully_hidden = hidden.point(lambda value: 255 if value == 255 else 0)
    hidden_rgb = ImageChops.lighter(ImageChops.lighter(red, green), blue)
    hidden_rgb = ImageChops.multiply(hidden_rgb, fully_hidden)
    require(
        hidden_rgb.getbbox() is None,
        f"{path}: RGB data remains beneath fully transparent pixels",
    )

    if category not in {"terrain"}:
        border = Image.new("L", image.size, 0)
        border.paste(alpha.crop((0, 0, image.width, 1)), (0, 0))
        border.paste(
            alpha.crop((0, image.height - 1, image.width, image.height)),
            (0, image.height - 1),
        )
        border.paste(alpha.crop((0, 0, 1, image.height)), (0, 0))
        border.paste(
            alpha.crop((image.width - 1, 0, image.width, image.height)),
            (image.width - 1, 0),
        )
        require(border.getbbox() is None, f"{path}: cutout touches its export boundary")


def edge_delta(
    image: Image.Image,
    first_box: tuple[int, int, int, int],
    second_box: tuple[int, int, int, int],
) -> int:
    """Return the maximum channel delta between equal-sized edge strips."""

    difference = ImageChops.difference(image.crop(first_box), image.crop(second_box))
    extrema = difference.getextrema()
    return max(channel[1] for channel in extrema)


def rectangles_overlap(first: dict[str, int], second: dict[str, int]) -> bool:
    """Return whether two axis-aligned logical rectangles overlap."""

    return not (
        first["x"] + first["width"] <= second["x"]
        or second["x"] + second["width"] <= first["x"]
        or first["y"] + first["height"] <= second["y"]
        or second["y"] + second["height"] <= first["y"]
    )


def validate_ora(path: Path) -> None:
    """Require a minimally valid editable OpenRaster archive."""

    require(path.exists(), f"missing editable master: {path}")
    with zipfile.ZipFile(path) as archive:
        names = archive.namelist()
        require(names[0] == "mimetype", f"{path}: mimetype must be first")
        require(
            archive.read("mimetype") == b"image/openraster", f"{path}: invalid mimetype"
        )
        require("stack.xml" in names, f"{path}: missing stack.xml")
        require("mergedimage.png" in names, f"{path}: missing mergedimage.png")
        require(
            any(name.startswith("data/") and name.endswith(".png") for name in names),
            f"{path}: missing editable layers",
        )


def validate_cue_grayscale() -> None:
    """Require each table-state cell to remain measurably distinct in grayscale."""

    image = Image.open(ROOT / "review" / "state-cues-grayscale.png").convert("L")
    require(image.size == (352, 88), "state-cues-grayscale.png: unexpected dimensions")
    cells = [image.crop((index * 88, 0, (index + 1) * 88, 88)) for index in range(4)]
    for left_index, left in enumerate(cells):
        for right_index, right in enumerate(
            cells[left_index + 1 :], start=left_index + 1
        ):
            mean_delta = ImageStat.Stat(ImageChops.difference(left, right)).mean[0]
            changed_pixels = sum(ImageChops.difference(left, right).histogram()[1:])
            require(
                mean_delta >= 0.4 and changed_pixels >= 100,
                f"grayscale cue cells {left_index} and {right_index} are not distinct",
            )


def main() -> None:
    """Validate manifest geometry, exports, masters, seams, animation, and review evidence."""

    manifest = json.loads(MANIFEST.read_text(encoding="utf-8"))
    require(manifest["schema_version"] == 1, "unsupported manifest schema")
    require(
        manifest["logical_viewport"] == {"width": 960, "height": 540},
        "logical viewport changed",
    )
    require(manifest["master_scale"] == 2, "masters must remain exactly 2x")

    assets = {asset["id"]: asset for asset in manifest["assets"]}
    require(len(assets) == 32, f"expected 32 assets, got {len(assets)}")
    for asset_id, asset in assets.items():
        path = ROOT / asset["export"]
        require(path.exists(), f"{asset_id}: missing export {path}")
        image = Image.open(path)
        export_rect = asset["export_rect_pixels"]
        master = asset["master_dimensions_pixels"]
        require(
            image.size == (export_rect["width"], export_rect["height"]),
            f"{asset_id}: export dimensions drifted",
        )
        require(
            master["width"] == image.width * 2 and master["height"] == image.height * 2,
            f"{asset_id}: master is not 2x",
        )
        require(
            asset["logical_rect"] == export_rect,
            f"{asset_id}: logical/export rectangles diverged",
        )
        require(
            0 <= asset["pivot"]["x"] <= image.width
            and 0 <= asset["pivot"]["y"] <= image.height,
            f"{asset_id}: pivot outside export",
        )
        require(
            0 <= asset["contact_baseline"] <= image.height,
            f"{asset_id}: baseline outside export",
        )
        require(sha256(path) == asset["sha256"], f"{asset_id}: hash mismatch")
        validate_alpha(path, image.convert("RGBA"), asset["category"])

    ground = Image.open(ROOT / assets["terrain.quiet_moss"]["export"]).convert("RGBA")
    require(
        edge_delta(
            ground,
            (0, 0, 1, ground.height),
            (ground.width - 1, 0, ground.width, ground.height),
        )
        == 0,
        "ground left/right seam differs",
    )
    require(
        edge_delta(
            ground,
            (0, 0, ground.width, 1),
            (0, ground.height - 1, ground.width, ground.height),
        )
        == 0,
        "ground top/bottom seam differs",
    )
    path = Image.open(ROOT / assets["terrain.straight_path"]["export"]).convert("RGBA")
    require(
        edge_delta(
            path, (0, 0, path.width, 1), (0, path.height - 1, path.width, path.height)
        )
        == 0,
        "path top/bottom seam differs",
    )

    animations = manifest["animations"]
    require(len(animations) == 4, "expected south, west, east, and north walk loops")
    expected_directions = {"south", "west", "east", "north"}
    actual_directions = {animation["id"].split(".")[-2] for animation in animations}
    require(actual_directions == expected_directions, "four-direction walk set drifted")
    for animation in animations:
        require(
            len(animation["frame_order"]) == 6,
            f"{animation['id']}: expected six frames",
        )
        require(
            animation["loop"] is True and animation["frame_duration_ms"] == 120,
            f"{animation['id']}: timing drifted",
        )
        frame_hashes = set()
        for frame_id in animation["frame_order"]:
            asset = assets[frame_id]
            frame = Image.open(ROOT / asset["export"]).convert("RGBA")
            require(
                frame.getchannel("A").getbbox()[3] == asset["contact_baseline"],
                f"{frame_id}: foot baseline drifted",
            )
            frame_hashes.add(asset["sha256"])
        require(len(frame_hashes) == 6, f"{animation['id']}: contains duplicate frames")

    clearance = manifest["navigation_clearance"]
    for placement in manifest["review_composition"]:
        if not placement["fixed_obstacle"]:
            continue
        asset_rect = assets[placement["asset"]]["logical_rect"]
        placed = {
            "x": placement["x"],
            "y": placement["y"],
            "width": asset_rect["width"],
            "height": asset_rect["height"],
        }
        require(
            not rectangles_overlap(clearance, placed),
            f"{placement['asset']}: fixed obstacle enters navigation clearance",
        )

    review = Image.open(ROOT / manifest["navigation_clearance"]["scene"])
    require(review.size == (960, 540), "review scene is not 960x540")
    grayscale = Image.open(ROOT / "review" / "module-review-scene-grayscale.png")
    require(grayscale.size == review.size, "grayscale review dimensions drifted")
    seeker_review = Image.open(ROOT / "review" / "seeker-four-direction-walk.png")
    require(
        seeker_review.size == (288, 224),
        "four-direction Seeker review dimensions drifted",
    )
    validate_cue_grayscale()

    for filename in [
        "terrain-straight-path.ora",
        "planted-herb-bed.ora",
        "preparation-table-states.ora",
        "seeker-four-direction-walk.ora",
        "module-review-scene.ora",
    ]:
        validate_ora(ROOT / "masters" / filename)

    print("corrected proof validation passed")
    print(f"assets: {len(assets)}")
    print("terrain seams: exact opposite-edge match")
    print("four-direction walk: 24 frames, baseline 52, 120 ms loops")
    print("grayscale cues: ordinary, noticeable, identified, dangerous remain distinct")
    print("navigation clearance: fixed obstacles remain outside 96x540 path corridor")
    print("editable masters: 5 valid OpenRaster archives")


if __name__ == "__main__":
    main()
