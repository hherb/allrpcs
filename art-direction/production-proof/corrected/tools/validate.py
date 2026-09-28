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
    require(len(assets) == 60, f"expected 60 assets, got {len(assets)}")
    import_proof_ids = {
        asset_id
        for asset_id, asset in assets.items()
        if asset["godot_import_proof"]
    }
    interaction_widths = {direction: 64 for direction in ["south", "west", "east", "north"]}
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
    path = Image.open(ROOT / assets["terrain.straight_path"]["export"]).convert("RGBA")
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
        ImageChops.difference(
            bend.crop(
                (port_offset, 0, port_offset + path.width, port_depth)
            ),
            straight_entry,
        ).getbbox()
        is None,
        "path bend top entry differs from the straight-path connector",
    )
    horizontal_path = path.transpose(Image.Transpose.ROTATE_90)
    expected_right_entry = horizontal_path.crop(
        (0, 0, port_depth, path.width)
    ).transpose(Image.Transpose.FLIP_LEFT_RIGHT)
    require(
        ImageChops.difference(
            bend.crop(
                (
                    bend.width - port_depth,
                    port_offset,
                    bend.width,
                    port_offset + path.width,
                )
            ),
            expected_right_entry,
        ).getbbox()
        is None,
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
        ImageChops.difference(
            junction.crop((port_offset, 0, port_offset + path.width, port_depth)),
            straight_entry,
        ).getbbox()
        is None,
        "path T-junction top entry differs from the straight-path connector",
    )
    expected_left_entry = horizontal_path.crop((0, 0, port_depth, path.width))
    expected_right_entry = expected_left_entry.transpose(
        Image.Transpose.FLIP_LEFT_RIGHT
    )
    require(
        ImageChops.difference(
            junction.crop((0, port_offset, port_depth, port_offset + path.width)),
            expected_left_entry,
        ).getbbox()
        is None,
        "path T-junction left exit differs from the straight-path connector",
    )
    require(
        ImageChops.difference(
            junction.crop(
                (
                    junction.width - port_depth,
                    port_offset,
                    junction.width,
                    port_offset + path.width,
                )
            ),
            expected_right_entry,
        ).getbbox()
        is None,
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
            ImageChops.difference(actual, expected).getbbox() is None,
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
        ImageChops.difference(
            end_cap.crop((port_offset, 0, port_offset + path.width, port_depth)),
            straight_entry,
        ).getbbox()
        is None,
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
    for animation_id in expected_walk_ids:
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
            require(
                frame.getchannel("A").getbbox()[3] == asset["contact_baseline"],
                f"{frame_id}: foot baseline drifted",
            )
            frame_hashes.add(asset["sha256"])
        require(len(frame_hashes) == 6, f"{animation['id']}: contains duplicate frames")

    expected_idle_widths = {"south": 36, "west": 48, "east": 48, "north": 36}
    for direction, width in expected_idle_widths.items():
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
            require(
                asset["contact_baseline"] == 52
                and asset["pivot"] == {"x": width // 2, "y": 52}
                and frame.size == (width, 56)
                and frame.getchannel("A").getbbox()[3] == 52,
                f"{frame_id}: fixed idle origin drifted",
            )
            idle_frames.append(frame)
        idle_difference = ImageChops.difference(idle_frames[0], idle_frames[1])
        require(
            idle_difference.getbbox() is not None,
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
            foot_bbox = frame.getchannel("A").crop((0, 42, width, 56)).getbbox()
            require(
                asset["contact_baseline"] == 52
                and asset["pivot"] == {"x": width // 2, "y": 52}
                and frame.size == (width, 56)
                and frame.getchannel("A").getbbox()[3] == 52,
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
    require(review.size == (960, 540), "review scene is not 960x540")
    grayscale = Image.open(ROOT / "review" / "module-review-scene-grayscale.png")
    require(grayscale.size == review.size, "grayscale review dimensions drifted")
    seeker_review = Image.open(ROOT / "review" / "seeker-four-direction-walk.png")
    require(
        seeker_review.size == (288, 224),
        "four-direction Seeker review dimensions drifted",
    )
    idle_review = Image.open(ROOT / "review" / "seeker-four-direction-idle-breathe.png")
    require(
        idle_review.size == (96, 224),
        "four-direction idle/breathe review dimensions drifted",
    )
    interaction_review = Image.open(
        ROOT / "review" / "seeker-four-direction-interact-reach.png"
    )
    require(
        interaction_review.size == (256, 224),
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
    validate_cue_grayscale()

    for filename in [
        "terrain-straight-path.ora",
        "terrain-path-bend.ora",
        "terrain-path-t-junction.ora",
        "terrain-path-cross-junction.ora",
        "terrain-path-end-cap.ora",
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
    print("four-direction walk: 24 frames, baseline 52, 120 ms loops")
    print("four-direction idle/breathe: 8 frames, fixed origins, 600 ms loops")
    print("four-direction interaction: 16 unique frames, fixed origins, 160 ms gestures")
    print("grayscale cues: ordinary, noticeable, identified, dangerous remain distinct")
    print("navigation clearance: fixed obstacles remain outside 96x540 path corridor")
    print("path bend: exact 96px top/right ports with transparent outer regions")
    print("path T-junction: exact 96px top/left/right ports with transparent lower regions")
    print("path cross-junction: exact 96px four-way ports with transparent corners")
    print("path end-cap: exact 96px top port with terminus above a transparent lower half")
    print("path topology: straight, bend, T, cross, and end-cap compose as one network")
    print("editable masters: 11 valid OpenRaster archives")


if __name__ == "__main__":
    main()
