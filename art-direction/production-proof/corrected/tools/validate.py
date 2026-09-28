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


def images_equal(first: Image.Image, second: Image.Image) -> bool:
    """Return whether two equal-sized images match in every channel."""

    if first.size != second.size or first.mode != second.mode:
        return False
    extrema = ImageChops.difference(first, second).getextrema()
    if isinstance(extrema[0], tuple):
        return all(channel_max == 0 for _, channel_max in extrema)
    return extrema[1] == 0


def rectangles_overlap(first: dict[str, int], second: dict[str, int]) -> bool:
    """Return whether two axis-aligned logical rectangles overlap."""

    return not (
        first["x"] + first["width"] <= second["x"]
        or second["x"] + second["width"] <= first["x"]
        or first["y"] + first["height"] <= second["y"]
        or second["y"] + second["height"] <= first["y"]
    )


def mean_rgb(image: Image.Image) -> tuple[float, float, float]:
    """Return the mean visible color for one opaque terrain proof."""

    return tuple(ImageStat.Stat(image.convert("RGB")).mean)


def color_distance(
    first: tuple[float, float, float], second: tuple[float, float, float]
) -> float:
    """Return Euclidean distance between two mean RGB colors."""

    return sum((left - right) ** 2 for left, right in zip(first, second)) ** 0.5


def maximum_corridor_contrast(image: Image.Image, width: int) -> float:
    """Measure the strongest full-span band against its surrounding ground."""

    grayscale = ImageOps.grayscale(image)
    pixels = list(grayscale.getdata())
    image_mean = sum(pixels) / len(pixels)
    strongest = 0.0
    for transpose in (False, True):
        candidate = grayscale.transpose(Image.Transpose.TRANSPOSE) if transpose else grayscale
        for offset in range(0, candidate.width - width + 1, 8):
            band_mean = ImageStat.Stat(
                candidate.crop((offset, 0, offset + width, candidate.height))
            ).mean[0]
            outside_count = candidate.width - width
            outside_mean = (
                image_mean * candidate.width - band_mean * width
            ) / outside_count
            strongest = max(strongest, abs(band_mean - outside_mean))
    return strongest


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
    require(manifest["schema_version"] == 2, "unsupported manifest schema")
    require(
        manifest["logical_viewport"] == {"width": 1280, "height": 720},
        "logical viewport changed",
    )
    require(
        manifest["fullscreen_reference_logical_viewport"]
        == {"width": 1280, "height": 827},
        "fullscreen reference viewport changed",
    )
    require(
        manifest["minimum_visible_character_height"] == 144,
        "minimum visible character height changed",
    )
    require(manifest["master_scale"] == 2, "masters must remain exactly 2x")

    assets = {asset["id"]: asset for asset in manifest["assets"]}
    require(len(assets) == 66, f"expected 66 assets, got {len(assets)}")
    import_proof_ids = {
        asset_id
        for asset_id, asset in assets.items()
        if asset["godot_import_proof"]
    }
    interaction_widths = {
        direction: 192 for direction in ["south", "west", "east", "north"]
    }
    corner_id = "terrain.woodland_leaf_litter_corner_transition"
    opposite_corner_id = "terrain.woodland_leaf_litter_corner_transition_left"
    inset_corner_id = "terrain.woodland_leaf_litter_inset_corner_transition"
    left_inset_corner_id = (
        "terrain.woodland_leaf_litter_inset_corner_transition_left"
    )
    require(
        import_proof_ids == set(assets),
        "not every corrected proof asset is included in the isolated Godot import set",
    )
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
    leaf_asset = assets["terrain.woodland_leaf_litter"]
    require(
        leaf_asset["source"]
        == "sources/imagegen/woodland-leaf-litter-ground.png"
        and leaf_asset["logical_rect"]
        == {"x": 0, "y": 0, "width": 256, "height": 256}
        and leaf_asset["master_dimensions_pixels"]
        == {"width": 512, "height": 512, "scale": 2}
        and leaf_asset["godot_import_proof"] is True,
        "leaf-litter manifest geometry, provenance, or import status drifted",
    )
    leaf_litter = Image.open(ROOT / leaf_asset["export"]).convert("RGBA")
    leaf_alpha = leaf_litter.getchannel("A")
    require(
        leaf_alpha.getextrema() == (255, 255),
        "leaf-litter tile is not fully opaque",
    )
    require(
        edge_delta(
            leaf_litter,
            (0, 0, 1, leaf_litter.height),
            (leaf_litter.width - 1, 0, leaf_litter.width, leaf_litter.height),
        )
        == 0,
        "leaf-litter left/right seam differs",
    )
    require(
        edge_delta(
            leaf_litter,
            (0, 0, leaf_litter.width, 1),
            (0, leaf_litter.height - 1, leaf_litter.width, leaf_litter.height),
        )
        == 0,
        "leaf-litter top/bottom seam differs",
    )
    require(
        ImageStat.Stat(ImageOps.grayscale(leaf_litter)).stddev[0] >= 18,
        "leaf-litter tile lacks useful tonal texture",
    )
    require(
        maximum_corridor_contrast(leaf_litter, 96) < 18,
        "leaf-litter tile contains a path-like full-span corridor",
    )
    path = Image.open(ROOT / assets["terrain.straight_path"]["export"]).convert("RGBA")
    path_on_ground = ground.copy()
    path_on_ground.alpha_composite(path, ((ground.width - path.width) // 2, 0))
    leaf_mean = mean_rgb(leaf_litter)
    ground_mean = mean_rgb(ground)
    require(
        color_distance(leaf_mean, ground_mean) >= 35,
        "leaf litter is not distinct enough from quiet moss",
    )
    require(
        color_distance(leaf_mean, mean_rgb(path_on_ground)) >= 25,
        "leaf litter is not distinct enough from the packed-earth path treatment",
    )
    transition_asset = assets["terrain.woodland_leaf_litter_transition"]
    require(
        transition_asset["source"] == "tools/build.py"
        and transition_asset["logical_rect"]
        == {"x": 0, "y": 0, "width": 256, "height": 256}
        and transition_asset["master_dimensions_pixels"]
        == {"width": 512, "height": 512, "scale": 2}
        and transition_asset["godot_import_proof"] is True,
        "ground-transition geometry, provenance, or import status drifted",
    )
    transition = Image.open(ROOT / transition_asset["export"]).convert("RGBA")
    require(
        transition.getchannel("A").getextrema() == (255, 255),
        "ground transition is not fully opaque",
    )
    connector_depth = 32
    require(
        images_equal(
            transition.crop((0, 0, connector_depth, transition.height)),
            ground.crop((0, 0, connector_depth, ground.height)),
        ),
        "ground transition lost its exact quiet-moss connector",
    )
    require(
        images_equal(
            transition.crop(
                (
                    transition.width - connector_depth,
                    0,
                    transition.width,
                    transition.height,
                )
            ),
            leaf_litter.crop(
                (
                    leaf_litter.width - connector_depth,
                    0,
                    leaf_litter.width,
                    leaf_litter.height,
                )
            ),
        ),
        "ground transition lost its exact leaf-litter connector",
    )
    require(
        edge_delta(
            transition,
            (0, 0, transition.width, 1),
            (0, transition.height - 1, transition.width, transition.height),
        )
        == 0,
        "ground-transition top/bottom seam differs",
    )
    corner_asset = assets[corner_id]
    require(
        corner_asset["source"] == "tools/build.py"
        and corner_asset["logical_rect"]
        == {"x": 0, "y": 0, "width": 256, "height": 256}
        and corner_asset["master_dimensions_pixels"]
        == {"width": 512, "height": 512, "scale": 2}
        and corner_asset["godot_import_proof"] is True,
        "ground-corner geometry, provenance, or import status drifted",
    )
    corner = Image.open(ROOT / corner_asset["export"]).convert("RGBA")
    rotated_transition = transition.transpose(Image.Transpose.ROTATE_90)
    require(
        corner.getchannel("A").getextrema() == (255, 255),
        "ground corner is not fully opaque",
    )
    require(
        images_equal(
            corner.crop((0, 0, corner.width, 1)),
            transition.crop(
                (0, transition.height - 1, transition.width, transition.height)
            ),
        ),
        "ground corner lost its exact top transition connector",
    )
    require(
        images_equal(
            corner.crop((corner.width - 1, 0, corner.width, corner.height)),
            rotated_transition.crop(
                (
                    0,
                    0,
                    1,
                    rotated_transition.height,
                )
            ),
        ),
        "ground corner lost its rotated right transition connector",
    )
    require(
        images_equal(
            corner.crop((0, 0, 1, corner.height)),
            ground.crop((ground.width - 1, 0, ground.width, ground.height)),
        ),
        "ground corner lost quiet moss on its unused left edge",
    )
    require(
        images_equal(
            corner.crop((0, corner.height - 1, corner.width, corner.height)),
            ground.crop((0, 0, ground.width, 1)),
        ),
        "ground corner lost quiet moss on its unused bottom edge",
    )
    corner_leaf_mean = mean_rgb(corner.crop((192, 0, 256, 64)))
    corner_moss_mean = mean_rgb(corner.crop((0, 192, 64, 256)))
    require(
        color_distance(corner_leaf_mean, leaf_mean)
        < color_distance(corner_leaf_mean, ground_mean)
        and color_distance(corner_moss_mean, ground_mean)
        < color_distance(corner_moss_mean, leaf_mean),
        "ground corner materials are not leaf upper-right and moss lower-left",
    )
    opposite_corner_asset = assets[opposite_corner_id]
    require(
        opposite_corner_asset["source"] == "tools/build.py"
        and opposite_corner_asset["logical_rect"]
        == {"x": 0, "y": 0, "width": 256, "height": 256}
        and opposite_corner_asset["master_dimensions_pixels"]
        == {"width": 512, "height": 512, "scale": 2}
        and opposite_corner_asset["godot_import_proof"] is True,
        "left ground-corner geometry, provenance, or import status drifted",
    )
    opposite_corner = Image.open(ROOT / opposite_corner_asset["export"]).convert(
        "RGBA"
    )
    flipped_transition = transition.transpose(Image.Transpose.FLIP_LEFT_RIGHT)
    left_transition = transition.transpose(Image.Transpose.ROTATE_90)
    require(
        opposite_corner.getchannel("A").getextrema() == (255, 255),
        "left ground corner is not fully opaque",
    )
    require(
        images_equal(
            opposite_corner.crop((0, 0, opposite_corner.width, 1)),
            flipped_transition.crop(
                (
                    0,
                    flipped_transition.height - 1,
                    flipped_transition.width,
                    flipped_transition.height,
                )
            ),
        ),
        "left ground corner lost its exact top transition connector",
    )
    require(
        images_equal(
            opposite_corner.crop((0, 0, 1, opposite_corner.height)),
            left_transition.crop(
                (
                    left_transition.width - 1,
                    0,
                    left_transition.width,
                    left_transition.height,
                )
            ),
        ),
        "left ground corner lost its rotated left transition connector",
    )
    require(
        images_equal(
            opposite_corner.crop(
                (
                    opposite_corner.width - 1,
                    0,
                    opposite_corner.width,
                    opposite_corner.height,
                )
            ),
            ground.crop((0, 0, 1, ground.height)),
        ),
        "left ground corner lost quiet moss on its unused right edge",
    )
    require(
        images_equal(
            opposite_corner.crop(
                (0, opposite_corner.height - 1, opposite_corner.width, opposite_corner.height)
            ),
            ground.crop((0, 0, ground.width, 1)),
        ),
        "left ground corner lost quiet moss on its unused bottom edge",
    )
    opposite_leaf_mean = mean_rgb(opposite_corner.crop((0, 0, 64, 64)))
    opposite_moss_mean = mean_rgb(opposite_corner.crop((192, 192, 256, 256)))
    require(
        color_distance(opposite_leaf_mean, leaf_mean)
        < color_distance(opposite_leaf_mean, ground_mean)
        and color_distance(opposite_moss_mean, ground_mean)
        < color_distance(opposite_moss_mean, leaf_mean),
        "left ground corner materials are not leaf upper-left and moss lower-right",
    )
    inset_corner_asset = assets[inset_corner_id]
    require(
        inset_corner_asset["source"] == "tools/build.py"
        and inset_corner_asset["logical_rect"]
        == {"x": 0, "y": 0, "width": 256, "height": 256}
        and inset_corner_asset["master_dimensions_pixels"]
        == {"width": 512, "height": 512, "scale": 2}
        and inset_corner_asset["godot_import_proof"] is True,
        "inset ground-corner geometry, provenance, or import status drifted",
    )
    inset_corner = Image.open(ROOT / inset_corner_asset["export"]).convert("RGBA")
    inset_top_transition = transition.transpose(Image.Transpose.FLIP_LEFT_RIGHT)
    inset_right_transition = transition.transpose(Image.Transpose.ROTATE_270)
    require(
        inset_corner.getchannel("A").getextrema() == (255, 255),
        "inset ground corner is not fully opaque",
    )
    require(
        images_equal(
            inset_corner.crop((0, 0, inset_corner.width, 1)),
            inset_top_transition.crop(
                (
                    0,
                    inset_top_transition.height - 1,
                    inset_top_transition.width,
                    inset_top_transition.height,
                )
            ),
        ),
        "inset ground corner lost its exact top transition connector",
    )
    require(
        images_equal(
            inset_corner.crop(
                (inset_corner.width - 1, 0, inset_corner.width, inset_corner.height)
            ),
            inset_right_transition.crop(
                (
                    0,
                    0,
                    1,
                    inset_right_transition.height,
                )
            ),
        ),
        "inset ground corner lost its rotated right transition connector",
    )
    require(
        images_equal(
            inset_corner.crop((0, 0, 1, inset_corner.height)),
            leaf_litter.crop(
                (leaf_litter.width - 1, 0, leaf_litter.width, leaf_litter.height)
            ),
        ),
        "inset ground corner lost leaf litter on its unused left edge",
    )
    require(
        images_equal(
            inset_corner.crop(
                (0, inset_corner.height - 1, inset_corner.width, inset_corner.height)
            ),
            leaf_litter.crop((0, 0, leaf_litter.width, 1)),
        ),
        "inset ground corner lost leaf litter on its unused bottom edge",
    )
    inset_moss_mean = mean_rgb(inset_corner.crop((192, 0, 256, 64)))
    inset_leaf_mean = mean_rgb(inset_corner.crop((0, 192, 64, 256)))
    require(
        color_distance(inset_moss_mean, ground_mean)
        < color_distance(inset_moss_mean, leaf_mean)
        and color_distance(inset_leaf_mean, leaf_mean)
        < color_distance(inset_leaf_mean, ground_mean),
        "inset ground corner materials are not moss upper-right and leaf lower-left",
    )
    left_inset_corner_asset = assets[left_inset_corner_id]
    require(
        left_inset_corner_asset["source"] == "tools/build.py"
        and left_inset_corner_asset["logical_rect"]
        == {"x": 0, "y": 0, "width": 256, "height": 256}
        and left_inset_corner_asset["master_dimensions_pixels"]
        == {"width": 512, "height": 512, "scale": 2}
        and left_inset_corner_asset["godot_import_proof"] is True,
        "left inset ground-corner geometry, provenance, or import status drifted",
    )
    left_inset_corner = Image.open(
        ROOT / left_inset_corner_asset["export"]
    ).convert("RGBA")
    left_inset_transition = transition.transpose(Image.Transpose.ROTATE_270)
    require(
        left_inset_corner.getchannel("A").getextrema() == (255, 255),
        "left inset ground corner is not fully opaque",
    )
    require(
        images_equal(
            left_inset_corner.crop((0, 0, left_inset_corner.width, 1)),
            transition.crop(
                (0, transition.height - 1, transition.width, transition.height)
            ),
        ),
        "left inset ground corner lost its exact top transition connector",
    )
    require(
        images_equal(
            left_inset_corner.crop((0, 0, 1, left_inset_corner.height)),
            left_inset_transition.crop(
                (
                    left_inset_transition.width - 1,
                    0,
                    left_inset_transition.width,
                    left_inset_transition.height,
                )
            ),
        ),
        "left inset ground corner lost its rotated left transition connector",
    )
    require(
        images_equal(
            left_inset_corner.crop(
                (
                    left_inset_corner.width - 1,
                    0,
                    left_inset_corner.width,
                    left_inset_corner.height,
                )
            ),
            leaf_litter.crop((0, 0, 1, leaf_litter.height)),
        ),
        "left inset ground corner lost leaf litter on its unused right edge",
    )
    require(
        images_equal(
            left_inset_corner.crop(
                (
                    0,
                    left_inset_corner.height - 1,
                    left_inset_corner.width,
                    left_inset_corner.height,
                )
            ),
            leaf_litter.crop((0, 0, leaf_litter.width, 1)),
        ),
        "left inset ground corner lost leaf litter on its unused bottom edge",
    )
    left_inset_moss_mean = mean_rgb(left_inset_corner.crop((0, 0, 64, 64)))
    left_inset_leaf_mean = mean_rgb(
        left_inset_corner.crop((192, 192, 256, 256))
    )
    require(
        color_distance(left_inset_moss_mean, ground_mean)
        < color_distance(left_inset_moss_mean, leaf_mean)
        and color_distance(left_inset_leaf_mean, leaf_mean)
        < color_distance(left_inset_leaf_mean, ground_mean),
        "left inset ground corner materials are not moss upper-left and leaf lower-right",
    )
    require(
        edge_delta(
            path, (0, 0, path.width, 1), (0, path.height - 1, path.width, path.height)
        )
        == 0,
        "path top/bottom seam differs",
    )
    bend_asset = assets["terrain.path_bend"]
    require(
        bend_asset["source"] == "sources/imagegen/path-bend-overlay.png"
        and bend_asset["logical_rect"]
        == {"x": 0, "y": 0, "width": 256, "height": 256}
        and bend_asset["master_dimensions_pixels"]
        == {"width": 512, "height": 512, "scale": 2}
        and bend_asset["godot_import_proof"] is True,
        "path bend manifest geometry, provenance, or import status drifted",
    )
    bend = Image.open(ROOT / bend_asset["export"]).convert("RGBA")
    port_offset = (bend.width - path.width) // 2
    port_depth = path.height // 8
    straight_entry = path.crop((0, 0, path.width, port_depth))
    require(
        images_equal(
            bend.crop(
                (port_offset, 0, port_offset + path.width, port_depth)
            ),
            straight_entry,
        ),
        "path bend top entry differs from the straight-path connector",
    )
    horizontal_path = path.transpose(Image.Transpose.ROTATE_90)
    expected_right_entry = horizontal_path.crop(
        (0, 0, port_depth, path.width)
    ).transpose(Image.Transpose.FLIP_LEFT_RIGHT)
    require(
        images_equal(
            bend.crop(
                (
                    bend.width - port_depth,
                    port_offset,
                    bend.width,
                    port_offset + path.width,
                )
            ),
            expected_right_entry,
        ),
        "path bend right exit differs from the straight-path connector",
    )
    alpha = bend.getchannel("A")
    for label, box in [
        ("top-left edge", (0, 0, port_offset, 1)),
        ("top-right edge", (port_offset + path.width, 0, bend.width, 1)),
        ("right-upper edge", (bend.width - 1, 0, bend.width, port_offset)),
        (
            "right-lower edge",
            (bend.width - 1, port_offset + path.width, bend.width, bend.height),
        ),
        ("left edge", (0, 0, 1, bend.height)),
        ("bottom edge", (0, bend.height - 1, bend.width, bend.height)),
        ("top-right outer region", (192, 0, 256, 64)),
        ("bottom-left outer region", (0, 192, 64, 256)),
    ]:
        require(alpha.crop(box).getbbox() is None, f"path bend {label} is not transparent")

    junction_asset = assets["terrain.path_t_junction"]
    require(
        junction_asset["source"]
        == "sources/imagegen/path-t-junction-overlay.png"
        and junction_asset["logical_rect"]
        == {"x": 0, "y": 0, "width": 256, "height": 256}
        and junction_asset["master_dimensions_pixels"]
        == {"width": 512, "height": 512, "scale": 2}
        and junction_asset["godot_import_proof"] is True,
        "path T-junction manifest geometry, provenance, or import status drifted",
    )
    junction = Image.open(ROOT / junction_asset["export"]).convert("RGBA")
    require(
        images_equal(
            junction.crop((port_offset, 0, port_offset + path.width, port_depth)),
            straight_entry,
        ),
        "path T-junction top entry differs from the straight-path connector",
    )
    expected_left_entry = horizontal_path.crop((0, 0, port_depth, path.width))
    expected_right_entry = expected_left_entry.transpose(
        Image.Transpose.FLIP_LEFT_RIGHT
    )
    require(
        images_equal(
            junction.crop((0, port_offset, port_depth, port_offset + path.width)),
            expected_left_entry,
        ),
        "path T-junction left exit differs from the straight-path connector",
    )
    require(
        images_equal(
            junction.crop(
                (
                    junction.width - port_depth,
                    port_offset,
                    junction.width,
                    port_offset + path.width,
                )
            ),
            expected_right_entry,
        ),
        "path T-junction right exit differs from the straight-path connector",
    )
    junction_alpha = junction.getchannel("A")
    for label, box in [
        ("top-left edge", (0, 0, port_offset, 1)),
        ("top-right edge", (port_offset + path.width, 0, junction.width, 1)),
        ("left-upper edge", (0, 0, 1, port_offset)),
        ("left-lower edge", (0, port_offset + path.width, 1, junction.height)),
        ("right-upper edge", (junction.width - 1, 0, junction.width, port_offset)),
        (
            "right-lower edge",
            (
                junction.width - 1,
                port_offset + path.width,
                junction.width,
                junction.height,
            ),
        ),
        ("bottom edge", (0, junction.height - 1, junction.width, junction.height)),
        ("lower band", (0, 192, 256, 256)),
        ("lower-left outer region", (0, 192, 64, 256)),
        ("lower-right outer region", (192, 192, 256, 256)),
    ]:
        require(
            junction_alpha.crop(box).getbbox() is None,
            f"path T-junction {label} is not transparent",
        )

    cross_asset = assets["terrain.path_cross_junction"]
    require(
        cross_asset["source"]
        == "sources/imagegen/path-cross-junction-overlay.png"
        and cross_asset["logical_rect"]
        == {"x": 0, "y": 0, "width": 256, "height": 256}
        and cross_asset["master_dimensions_pixels"]
        == {"width": 512, "height": 512, "scale": 2}
        and cross_asset["godot_import_proof"] is True,
        "path cross-junction manifest geometry, provenance, or import status drifted",
    )
    cross = Image.open(ROOT / cross_asset["export"]).convert("RGBA")
    expected_bottom_entry = straight_entry.transpose(Image.Transpose.FLIP_TOP_BOTTOM)
    for label, actual, expected in [
        (
            "top",
            cross.crop((port_offset, 0, port_offset + path.width, port_depth)),
            straight_entry,
        ),
        (
            "bottom",
            cross.crop(
                (
                    port_offset,
                    cross.height - port_depth,
                    port_offset + path.width,
                    cross.height,
                )
            ),
            expected_bottom_entry,
        ),
        (
            "left",
            cross.crop((0, port_offset, port_depth, port_offset + path.width)),
            expected_left_entry,
        ),
        (
            "right",
            cross.crop(
                (
                    cross.width - port_depth,
                    port_offset,
                    cross.width,
                    port_offset + path.width,
                )
            ),
            expected_right_entry,
        ),
    ]:
        require(
            images_equal(actual, expected),
            f"path cross-junction {label} port differs from the straight path",
        )
    cross_alpha = cross.getchannel("A")
    for label, box in [
        ("top-left corner", (0, 0, 64, 64)),
        ("top-right corner", (192, 0, 256, 64)),
        ("bottom-left corner", (0, 192, 64, 256)),
        ("bottom-right corner", (192, 192, 256, 256)),
    ]:
        require(
            cross_alpha.crop(box).getbbox() is None,
            f"path cross-junction {label} is not transparent",
        )

    end_cap_asset = assets["terrain.path_end_cap"]
    require(
        end_cap_asset["source"] == "sources/imagegen/path-end-cap-overlay.png"
        and end_cap_asset["logical_rect"]
        == {"x": 0, "y": 0, "width": 256, "height": 256}
        and end_cap_asset["master_dimensions_pixels"]
        == {"width": 512, "height": 512, "scale": 2}
        and end_cap_asset["godot_import_proof"] is True,
        "path end-cap manifest geometry, provenance, or import status drifted",
    )
    end_cap = Image.open(ROOT / end_cap_asset["export"]).convert("RGBA")
    require(
        images_equal(
            end_cap.crop((port_offset, 0, port_offset + path.width, port_depth)),
            straight_entry,
        ),
        "path end-cap top entry differs from the straight-path connector",
    )
    end_cap_alpha = end_cap.getchannel("A")
    require(
        end_cap_alpha.getbbox() is not None and end_cap_alpha.getbbox()[3] <= 128,
        "path end-cap terminus does not finish before the lower half",
    )
    for label, box in [
        ("left edge", (0, 0, 1, end_cap.height)),
        ("right edge", (end_cap.width - 1, 0, end_cap.width, end_cap.height)),
        ("bottom edge", (0, end_cap.height - 1, end_cap.width, end_cap.height)),
        ("lower 64-pixel band", (0, 192, end_cap.width, end_cap.height)),
    ]:
        require(
            end_cap_alpha.crop(box).getbbox() is None,
            f"path end-cap {label} is not transparent",
        )

    animations = {animation["id"]: animation for animation in manifest["animations"]}
    expected_walk_ids = {
        f"characters.seeker.{direction}.walk"
        for direction in ["south", "west", "east", "north"]
    }
    expected_idle_ids = {
        f"characters.seeker.{direction}.idle_breathe"
        for direction in ["south", "west", "east", "north"]
    }
    interaction_ids = {
        f"characters.seeker.{direction}.interact_reach"
        for direction in interaction_widths
    }
    require(
        set(animations) == expected_walk_ids | expected_idle_ids | interaction_ids,
        "animation set drifted",
    )
    expected_direction_widths = {
        "south": 108,
        "west": 144,
        "east": 144,
        "north": 108,
    }
    for direction, width in expected_direction_widths.items():
        animation_id = f"characters.seeker.{direction}.walk"
        animation = animations[animation_id]
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
            alpha_bbox = frame.getchannel("A").getbbox()
            require(
                alpha_bbox is not None
                and alpha_bbox[3] == asset["contact_baseline"]
                and alpha_bbox[3] - alpha_bbox[1] >= 144,
                f"{frame_id}: foot baseline drifted",
            )
            require(
                asset["contact_baseline"] == 156
                and asset["pivot"] == {"x": width // 2, "y": 156}
                and frame.size == (width, 168),
                f"{frame_id}: fixed walk origin drifted",
            )
            frame_hashes.add(asset["sha256"])
        require(len(frame_hashes) == 6, f"{animation['id']}: contains duplicate frames")

    for direction, width in expected_direction_widths.items():
        idle = animations[f"characters.seeker.{direction}.idle_breathe"]
        require(
            len(idle["frame_order"]) == 2,
            f"{direction} idle/breathe proof must contain exactly two frames",
        )
        require(
            idle["loop"] is True and idle["frame_duration_ms"] == 600,
            f"{direction} idle/breathe timing drifted",
        )
        idle_frames = []
        for frame_id in idle["frame_order"]:
            asset = assets[frame_id]
            frame = Image.open(ROOT / asset["export"]).convert("RGBA")
            alpha_bbox = frame.getchannel("A").getbbox()
            require(
                asset["contact_baseline"] == 156
                and asset["pivot"] == {"x": width // 2, "y": 156}
                and frame.size == (width, 168)
                and alpha_bbox is not None
                and alpha_bbox[3] == 156
                and alpha_bbox[3] - alpha_bbox[1] >= 144,
                f"{frame_id}: fixed idle origin drifted",
            )
            idle_frames.append(frame)
        require(
            not images_equal(idle_frames[0], idle_frames[1]),
            f"{direction} idle/breathe frames are duplicates",
        )

    interaction_hashes = set()
    for direction, width in interaction_widths.items():
        interaction = animations[f"characters.seeker.{direction}.interact_reach"]
        require(
            len(interaction["frame_order"]) == 4,
            f"{direction} interaction proof must contain exactly four frames",
        )
        require(
            interaction["loop"] is False and interaction["frame_duration_ms"] == 160,
            f"{direction} interaction timing drifted",
        )
        direction_hashes = set()
        for frame_id in interaction["frame_order"]:
            asset = assets[frame_id]
            frame = Image.open(ROOT / asset["export"]).convert("RGBA")
            alpha_bbox = frame.getchannel("A").getbbox()
            foot_bbox = frame.getchannel("A").crop((0, 126, width, 168)).getbbox()
            require(
                asset["contact_baseline"] == 156
                and asset["pivot"] == {"x": width // 2, "y": 156}
                and frame.size == (width, 168)
                and alpha_bbox is not None
                and alpha_bbox[3] == 156,
                f"{frame_id}: fixed interaction origin drifted",
            )
            require(
                foot_bbox is not None
                and abs((foot_bbox[0] + foot_bbox[2]) / 2 - width / 2) <= 1,
                f"{frame_id}: horizontal foot origin drifted",
            )
            direction_hashes.add(asset["sha256"])
            interaction_hashes.add(asset["sha256"])
        require(
            len(direction_hashes) == 4,
            f"{direction} interaction proof contains duplicate frames",
        )
    require(
        len(interaction_hashes) == 16,
        "four-direction interaction proof contains duplicate frames",
    )

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
    require(review.size == (1280, 720), "review scene is not 1280x720")
    grayscale = Image.open(ROOT / "review" / "module-review-scene-grayscale.png")
    require(grayscale.size == review.size, "grayscale review dimensions drifted")
    seeker_review = Image.open(ROOT / "review" / "seeker-four-direction-walk.png")
    require(
        seeker_review.size == (864, 672),
        "four-direction Seeker review dimensions drifted",
    )
    idle_review = Image.open(ROOT / "review" / "seeker-four-direction-idle-breathe.png")
    require(
        idle_review.size == (288, 672),
        "four-direction idle/breathe review dimensions drifted",
    )
    interaction_review = Image.open(
        ROOT / "review" / "seeker-four-direction-interact-reach.png"
    )
    require(
        interaction_review.size == (768, 672),
        "four-direction interaction review dimensions drifted",
    )
    bend_review = Image.open(ROOT / "review" / "path-bend-review.png")
    require(bend_review.size == (768, 768), "path-bend review dimensions drifted")
    junction_review = Image.open(ROOT / "review" / "path-t-junction-review.png")
    require(
        junction_review.size == (768, 768),
        "path-T-junction review dimensions drifted",
    )
    cross_review = Image.open(ROOT / "review" / "path-cross-junction-review.png")
    require(
        cross_review.size == (768, 768),
        "path cross-junction review dimensions drifted",
    )
    end_cap_review = Image.open(ROOT / "review" / "path-end-cap-review.png")
    require(
        end_cap_review.size == (768, 768),
        "path end-cap review dimensions drifted",
    )
    topology_review = Image.open(ROOT / "review" / "path-module-topology-review.png")
    require(
        topology_review.size == (1024, 768),
        "path module topology review dimensions drifted",
    )
    leaf_review = Image.open(ROOT / "review" / "woodland-leaf-litter-review.png")
    require(
        leaf_review.size == (1024, 512),
        "leaf-litter seam and contrast review dimensions drifted",
    )
    transition_review = Image.open(
        ROOT / "review" / "woodland-leaf-litter-transition-review.png"
    )
    require(
        transition_review.size == (768, 512),
        "ground-transition review dimensions drifted",
    )
    corner_review = Image.open(
        ROOT / "review" / "woodland-leaf-litter-corner-transition-review.png"
    )
    require(
        corner_review.size == (768, 768),
        "ground-corner repeatability review dimensions drifted",
    )
    opposite_corner_review = Image.open(
        ROOT / "review" / "woodland-leaf-litter-corner-transition-left-review.png"
    )
    require(
        opposite_corner_review.size == (768, 768),
        "left ground-corner repeatability review dimensions drifted",
    )
    inset_corner_review = Image.open(
        ROOT / "review" / "woodland-leaf-litter-inset-corner-transition-review.png"
    )
    require(
        inset_corner_review.size == (768, 768),
        "inset ground-corner repeatability review dimensions drifted",
    )
    left_inset_corner_review = Image.open(
        ROOT
        / "review"
        / "woodland-leaf-litter-inset-corner-transition-left-review.png"
    )
    require(
        left_inset_corner_review.size == (768, 768),
        "left inset ground-corner repeatability review dimensions drifted",
    )
    require(
        images_equal(
            left_inset_corner_review.crop((256, 0, 512, 256)),
            transition,
        )
        and images_equal(
            left_inset_corner_review.crop((0, 256, 256, 512)),
            left_inset_transition,
        )
        and images_equal(
            left_inset_corner_review.crop((256, 256, 512, 512)),
            left_inset_corner,
        )
        and images_equal(
            left_inset_corner_review.crop((512, 256, 768, 512)),
            leaf_litter,
        )
        and images_equal(
            left_inset_corner_review.crop((256, 512, 512, 768)),
            leaf_litter,
        ),
        "left inset ground-corner review lost exact connected source tiles",
    )
    validate_cue_grayscale()

    for filename in [
        "terrain-straight-path.ora",
        "terrain-path-bend.ora",
        "terrain-path-t-junction.ora",
        "terrain-path-cross-junction.ora",
        "terrain-path-end-cap.ora",
        "terrain-woodland-leaf-litter.ora",
        "terrain-woodland-leaf-litter-transition.ora",
        "terrain-woodland-leaf-litter-corner-transition.ora",
        "terrain-woodland-leaf-litter-corner-transition-left.ora",
        "terrain-woodland-leaf-litter-inset-corner-transition.ora",
        "terrain-woodland-leaf-litter-inset-corner-transition-left.ora",
        "planted-herb-bed.ora",
        "preparation-table-states.ora",
        "seeker-four-direction-walk.ora",
        "seeker-four-direction-idle-breathe.ora",
        "seeker-four-direction-interact-reach.ora",
        "module-review-scene.ora",
    ]:
        validate_ora(ROOT / "masters" / filename)

    print("corrected proof validation passed")
    print(f"assets: {len(assets)}")
    print("terrain seams: exact opposite-edge match")
    print("four-direction walk: 24 frames, at least 144px visible, baseline 156, 120 ms loops")
    print("four-direction idle/breathe: 8 frames, at least 144px visible, 600 ms loops")
    print("four-direction interaction: 16 unique frames, fixed origins, 160 ms gestures")
    print("grayscale cues: ordinary, noticeable, identified, dangerous remain distinct")
    print("navigation clearance: fixed obstacles remain outside 96x720 path corridor")
    print("path bend: exact 96px top/right ports with transparent outer regions")
    print("path T-junction: exact 96px top/left/right ports with transparent lower regions")
    print("path cross-junction: exact 96px four-way ports with transparent corners")
    print("path end-cap: exact 96px top port with terminus above a transparent lower half")
    print("path topology: straight, bend, T, cross, and end-cap compose as one network")
    print("leaf litter: opaque, exact seams, useful texture and palette contrast, no corridor")
    print("ground transition: exact moss/leaf connectors and repeatable top/bottom seam")
    print("ground corner: exact top/right transition and unused moss edge connectors")
    print("left ground corner: exact top/left transition and unused moss edge connectors")
    print("inset ground corner: exact top/right transition and unused leaf edge connectors")
    print("left inset ground corner: exact top/left transition and unused leaf edge connectors")
    print("editable masters: 17 valid OpenRaster archives")


if __name__ == "__main__":
    main()
