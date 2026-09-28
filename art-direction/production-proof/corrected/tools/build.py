#!/usr/bin/env python3
"""Build the bounded Direction A corrected-pipeline proof deterministically."""

from __future__ import annotations

import hashlib
import json
import math
import tempfile
import zipfile
from dataclasses import dataclass
from pathlib import Path
from xml.etree.ElementTree import Element, SubElement, tostring

from PIL import Image, ImageDraw, ImageOps


ROOT = Path(__file__).resolve().parents[1]
REPOSITORY_ROOT = ROOT.parents[2]
RAW = ROOT / "sources" / "imagegen"
EXPORTS = ROOT / "exports"
MASTERS = ROOT / "masters"
REVIEW = ROOT / "review"
LEGACY_SEEKER = (
    REPOSITORY_ROOT
    / "art-direction"
    / "production-proof"
    / "sources"
    / "seeker-four-direction-walk.png"
)

MASTER_SCALE = 2
ALPHA_CUTOFF = 96
GROUND_CONNECTOR_DEPTH = 32
GROUND_CORNER_FEATHER_MASTER = 48.0
GROUND_CORNER_IRREGULARITY_MASTER = 14.0
RESAMPLE = Image.Resampling.LANCZOS


@dataclass(frozen=True)
class Asset:
    """One deterministic 1x export and its 2x editable-master geometry."""

    asset_id: str
    category: str
    export_path: Path
    master_size: tuple[int, int]
    pivot_master: tuple[int, int]
    baseline_master: int
    image_master: Image.Image
    source: str
    godot_import_proof: bool = True


def alpha_clean(image: Image.Image, cutoff: int = ALPHA_CUTOFF) -> Image.Image:
    """Remove low-alpha generator haze and normalize retained alpha to the full range."""

    rgba = image.convert("RGBA")
    pixels = []
    denominator = max(1, 255 - cutoff)
    for red, green, blue, alpha in rgba.getdata():
        if alpha <= cutoff:
            pixels.append((0, 0, 0, 0))
            continue
        normalized = min(255, round((alpha - cutoff) * 255 / denominator))
        pixels.append((red, green, blue, normalized))
    rgba.putdata(pixels)
    return rgba


def crop_alpha(image: Image.Image) -> Image.Image:
    """Crop to nontransparent content, rejecting an unexpectedly empty source."""

    bbox = image.getchannel("A").getbbox()
    if bbox is None:
        raise ValueError("source contains no retained alpha")
    return image.crop(bbox)


def contain_on_canvas(
    source: Image.Image,
    canvas_size: tuple[int, int],
    content_size: tuple[int, int],
    baseline: int,
) -> Image.Image:
    """Scale one cutout into a fixed canvas and align it to a fixed baseline."""

    source = crop_alpha(alpha_clean(source))
    target_width, target_height = content_size
    scale = min(target_width / source.width, target_height / source.height)
    resized = source.resize(
        (max(1, round(source.width * scale)), max(1, round(source.height * scale))),
        RESAMPLE,
    )
    resized = alpha_clean(resized, cutoff=24)
    canvas = Image.new("RGBA", canvas_size, (0, 0, 0, 0))
    x = (canvas.width - resized.width) // 2
    y = baseline - resized.height
    canvas.alpha_composite(resized, (x, y))
    return canvas


def enforce_opposite_edges(
    image: Image.Image, horizontal: bool, vertical: bool
) -> Image.Image:
    """Make requested opposite edge pixels identical without altering the interior."""

    result = image.copy().convert("RGBA")
    if horizontal:
        left = result.crop((0, 0, 1, result.height))
        right = result.crop((result.width - 1, 0, result.width, result.height))
        average = Image.blend(left, right, 0.5)
        result.paste(average, (0, 0))
        result.paste(average, (result.width - 1, 0))
    if vertical:
        top = result.crop((0, 0, result.width, 1))
        bottom = result.crop((0, result.height - 1, result.width, result.height))
        average = Image.blend(top, bottom, 0.5)
        result.paste(average, (0, 0))
        result.paste(average, (0, result.height - 1))
    return result


def make_ground() -> Image.Image:
    """Create the 512px 2x master for one repeatable 256px logical ground tile."""

    source = Image.open(RAW / "quiet-moss-ground.png").convert("RGBA")
    square = ImageOps.fit(source, (512, 512), method=RESAMPLE, centering=(0.5, 0.5))
    return enforce_opposite_edges(square, horizontal=True, vertical=True)


def make_leaf_litter() -> tuple[Image.Image, Image.Image]:
    """Create the generated reference and repeatable 512px leaf-litter master."""

    source = Image.open(RAW / "woodland-leaf-litter-ground.png").convert("RGBA")
    reference = ImageOps.fit(source, (512, 512), method=RESAMPLE, centering=(0.5, 0.5))
    repeatable = enforce_opposite_edges(reference, horizontal=True, vertical=True)
    return reference, repeatable


def make_leaf_litter_transition(
    quiet_moss: Image.Image, leaf_litter: Image.Image
) -> Image.Image:
    """Blend exact ground masters across one repeatable irregular vertical edge."""

    if quiet_moss.size != leaf_litter.size:
        raise ValueError("ground-transition sources must share exact master geometry")
    mask = Image.new("L", quiet_moss.size, 0)
    pixels = []
    feather = 48.0
    for y in range(mask.height):
        phase = y / (mask.height - 1)
        boundary = (
            mask.width / 2
            + 24 * math.sin(2 * math.pi * phase)
            + 10 * math.sin(6 * math.pi * phase)
        )
        for x in range(mask.width):
            blend = (x - (boundary - feather / 2)) / feather
            blend = max(0.0, min(1.0, blend))
            smooth = blend * blend * (3.0 - 2.0 * blend)
            pixels.append(round(smooth * 255))
    mask.putdata(pixels)
    return Image.composite(leaf_litter, quiet_moss, mask).convert("RGBA")


def enforce_ground_transition_connectors(
    transition: Image.Image,
    quiet_moss: Image.Image,
    leaf_litter: Image.Image,
) -> Image.Image:
    """Install exact 32px ground connectors after 1x resampling."""

    result = enforce_opposite_edges(transition, horizontal=False, vertical=True)
    result.paste(
        quiet_moss.crop((0, 0, GROUND_CONNECTOR_DEPTH, quiet_moss.height)),
        (0, 0),
    )
    result.paste(
        leaf_litter.crop(
            (
                leaf_litter.width - GROUND_CONNECTOR_DEPTH,
                0,
                leaf_litter.width,
                leaf_litter.height,
            )
        ),
        (result.width - GROUND_CONNECTOR_DEPTH, 0),
    )
    return result


def make_ground_corner_mask(
    size: tuple[int, int], turns_left: bool
) -> Image.Image:
    """Create one deterministic feathered diagonal mask for a ground corner."""

    if size[0] != size[1]:
        raise ValueError("ground-corner masks require square geometry")
    mask = Image.new("L", size, 0)
    pixels = []
    for y in range(mask.height):
        for x in range(mask.width):
            if turns_left:
                progress = (x - y + mask.width / 2) / mask.width
                distance = mask.width / 2 - x - y
            else:
                progress = (x + y - mask.width / 2) / mask.width
                distance = x - y - mask.width / 2
            irregularity = GROUND_CORNER_IRREGULARITY_MASTER * math.sin(
                2 * math.pi * progress
            )
            blend = (
                distance
                + irregularity
                + GROUND_CORNER_FEATHER_MASTER / 2
            ) / GROUND_CORNER_FEATHER_MASTER
            blend = max(0.0, min(1.0, blend))
            smooth = blend * blend * (3.0 - 2.0 * blend)
            pixels.append(round(smooth * 255))
    mask.putdata(pixels)
    return mask


def make_leaf_litter_corner_transition(
    quiet_moss: Image.Image,
    leaf_litter: Image.Image,
    straight_transition: Image.Image,
) -> Image.Image:
    """Turn the straight moss/leaf boundary through one deterministic 90-degree corner."""

    if quiet_moss.size != leaf_litter.size or quiet_moss.size != straight_transition.size:
        raise ValueError("ground-corner sources must share exact master geometry")
    mask = make_ground_corner_mask(quiet_moss.size, turns_left=False)
    corner = Image.composite(leaf_litter, quiet_moss, mask).convert("RGBA")
    return enforce_ground_corner_connectors(
        corner, quiet_moss, straight_transition
    )


def enforce_ground_corner_connectors(
    corner: Image.Image,
    quiet_moss: Image.Image,
    straight_transition: Image.Image,
) -> Image.Image:
    """Restore exact 1x top/right transition and unused moss edge connectors."""

    result = corner.copy().convert("RGBA")
    rotated_transition = straight_transition.transpose(Image.Transpose.ROTATE_90)
    result.paste(straight_transition.crop((0, 0, result.width, 1)), (0, 0))
    result.paste(
        rotated_transition.crop(
            (rotated_transition.width - 1, 0, rotated_transition.width, result.height)
        ),
        (result.width - 1, 0),
    )
    result.paste(quiet_moss.crop((0, 0, 1, result.height)), (0, 0))
    result.paste(
        quiet_moss.crop((0, quiet_moss.height - 1, result.width, quiet_moss.height)),
        (0, result.height - 1),
    )
    return result


def make_leaf_litter_left_corner_transition(
    quiet_moss: Image.Image,
    leaf_litter: Image.Image,
    straight_transition: Image.Image,
) -> Image.Image:
    """Turn the straight moss/leaf boundary toward the left in one corner."""

    if quiet_moss.size != leaf_litter.size or quiet_moss.size != straight_transition.size:
        raise ValueError("left ground-corner sources must share exact master geometry")
    mask = make_ground_corner_mask(quiet_moss.size, turns_left=True)
    corner = Image.composite(leaf_litter, quiet_moss, mask).convert("RGBA")
    return enforce_left_ground_corner_connectors(
        corner, quiet_moss, straight_transition
    )


def enforce_left_ground_corner_connectors(
    corner: Image.Image,
    quiet_moss: Image.Image,
    straight_transition: Image.Image,
) -> Image.Image:
    """Restore exact top/left transition and unused moss edge connectors."""

    result = corner.copy().convert("RGBA")
    flipped_transition = straight_transition.transpose(Image.Transpose.FLIP_LEFT_RIGHT)
    left_transition = straight_transition.transpose(Image.Transpose.ROTATE_90)
    result.paste(flipped_transition.crop((0, 0, result.width, 1)), (0, 0))
    result.paste(left_transition.crop((0, 0, 1, result.height)), (0, 0))
    result.paste(
        quiet_moss.crop((quiet_moss.width - 1, 0, quiet_moss.width, result.height)),
        (result.width - 1, 0),
    )
    result.paste(
        quiet_moss.crop((0, quiet_moss.height - 1, result.width, quiet_moss.height)),
        (0, result.height - 1),
    )
    return result


def make_leaf_litter_inset_corner_transition(
    quiet_moss: Image.Image,
    leaf_litter: Image.Image,
    straight_transition: Image.Image,
) -> Image.Image:
    """Place quiet moss inside one leaf-litter top-right inset corner."""

    if quiet_moss.size != leaf_litter.size or quiet_moss.size != straight_transition.size:
        raise ValueError("inset ground-corner sources must share exact master geometry")
    mask = make_ground_corner_mask(quiet_moss.size, turns_left=False)
    corner = Image.composite(quiet_moss, leaf_litter, mask).convert("RGBA")
    return enforce_inset_ground_corner_connectors(
        corner, leaf_litter, straight_transition
    )


def enforce_inset_ground_corner_connectors(
    corner: Image.Image,
    leaf_litter: Image.Image,
    straight_transition: Image.Image,
) -> Image.Image:
    """Restore exact top/right transition and unused leaf-litter edges."""

    result = corner.copy().convert("RGBA")
    top_transition = straight_transition.transpose(Image.Transpose.FLIP_LEFT_RIGHT)
    right_transition = straight_transition.transpose(Image.Transpose.ROTATE_270)
    result.paste(top_transition.crop((0, 0, result.width, 1)), (0, 0))
    result.paste(
        right_transition.crop(
            (right_transition.width - 1, 0, right_transition.width, result.height)
        ),
        (result.width - 1, 0),
    )
    result.paste(leaf_litter.crop((0, 0, 1, result.height)), (0, 0))
    result.paste(
        leaf_litter.crop(
            (0, leaf_litter.height - 1, result.width, leaf_litter.height)
        ),
        (0, result.height - 1),
    )
    return result


def make_leaf_litter_left_inset_corner_transition(
    quiet_moss: Image.Image,
    leaf_litter: Image.Image,
    straight_transition: Image.Image,
) -> Image.Image:
    """Place quiet moss inside one leaf-litter top-left inset corner."""

    if quiet_moss.size != leaf_litter.size or quiet_moss.size != straight_transition.size:
        raise ValueError("left inset ground-corner sources must share exact master geometry")
    mask = make_ground_corner_mask(quiet_moss.size, turns_left=True)
    corner = Image.composite(quiet_moss, leaf_litter, mask).convert("RGBA")
    return enforce_left_inset_ground_corner_connectors(
        corner, leaf_litter, straight_transition
    )


def enforce_left_inset_ground_corner_connectors(
    corner: Image.Image,
    leaf_litter: Image.Image,
    straight_transition: Image.Image,
) -> Image.Image:
    """Restore exact top/left transition and unused leaf-litter edges."""

    result = corner.copy().convert("RGBA")
    left_transition = straight_transition.transpose(Image.Transpose.ROTATE_270)
    result.paste(
        straight_transition.crop(
            (0, straight_transition.height - 1, result.width, straight_transition.height)
        ),
        (0, 0),
    )
    result.paste(
        left_transition.crop(
            (left_transition.width - 1, 0, left_transition.width, result.height)
        ),
        (0, 0),
    )
    result.paste(
        leaf_litter.crop((0, 0, 1, result.height)),
        (result.width - 1, 0),
    )
    result.paste(
        leaf_litter.crop((0, 0, result.width, 1)),
        (0, result.height - 1),
    )
    return result


def make_path() -> Image.Image:
    """Create the 192x512 2x straight-path overlay with repeatable vertical edges."""

    source = crop_alpha(alpha_clean(Image.open(RAW / "straight-path-overlay.png")))
    resized = ImageOps.fit(source, (192, 512), method=RESAMPLE, centering=(0.5, 0.5))
    resized = alpha_clean(resized, cutoff=24)
    return enforce_opposite_edges(resized, horizontal=False, vertical=True)


def enforce_path_bend_ports(
    bend: Image.Image, straight_path: Image.Image
) -> Image.Image:
    """Install exact top/right ports and transparent unused outer edges."""

    bend = bend.copy().convert("RGBA")
    port_width = straight_path.width
    port_offset = (bend.width - port_width) // 2
    port_depth = straight_path.height // 8
    straight_entry = straight_path.crop((0, 0, port_width, port_depth))
    bend.paste(straight_entry, (port_offset, 0))
    horizontal_path = straight_path.transpose(Image.Transpose.ROTATE_90)
    right_entry = horizontal_path.crop(
        (0, 0, port_depth, port_width)
    ).transpose(Image.Transpose.FLIP_LEFT_RIGHT)
    bend.paste(right_entry, (bend.width - port_depth, port_offset))
    straight_edge = straight_path.crop((0, 0, port_width, 1))

    top_edge = Image.new("RGBA", (bend.width, 1), (0, 0, 0, 0))
    top_edge.paste(straight_edge, (port_offset, 0))
    bend.paste(top_edge, (0, 0))

    right_edge = Image.new("RGBA", (1, bend.height), (0, 0, 0, 0))
    right_edge.paste(
        straight_edge.transpose(Image.Transpose.ROTATE_90), (0, port_offset)
    )
    bend.paste(right_edge, (bend.width - 1, 0))
    bend.paste(Image.new("RGBA", (1, bend.height), (0, 0, 0, 0)), (0, 0))
    bend.paste(Image.new("RGBA", (bend.width, 1), (0, 0, 0, 0)), (0, bend.height - 1))
    return bend


def make_path_bend(path_master: Image.Image) -> Image.Image:
    """Create one 512px bend with exact straight-path top and right ports."""

    source = Image.open(RAW / "path-bend-overlay.png").convert("RGBA")
    bend = ImageOps.fit(source, (512, 512), method=RESAMPLE, centering=(0.5, 0.5))
    bend = alpha_clean(bend, cutoff=24)
    return enforce_path_bend_ports(bend, path_master)


def enforce_path_t_junction_ports(
    junction: Image.Image, straight_path: Image.Image
) -> Image.Image:
    """Install exact top/left/right ports and transparent lower outer edges."""

    junction = junction.copy().convert("RGBA")
    port_width = straight_path.width
    port_offset = (junction.width - port_width) // 2
    port_depth = straight_path.height // 8
    straight_entry = straight_path.crop((0, 0, port_width, port_depth))
    junction.paste(straight_entry, (port_offset, 0))

    horizontal_path = straight_path.transpose(Image.Transpose.ROTATE_90)
    left_entry = horizontal_path.crop((0, 0, port_depth, port_width))
    right_entry = left_entry.transpose(Image.Transpose.FLIP_LEFT_RIGHT)
    junction.paste(left_entry, (0, port_offset))
    junction.paste(right_entry, (junction.width - port_depth, port_offset))

    straight_edge = straight_path.crop((0, 0, port_width, 1))
    top_edge = Image.new("RGBA", (junction.width, 1), (0, 0, 0, 0))
    top_edge.paste(straight_edge, (port_offset, 0))
    junction.paste(top_edge, (0, 0))

    horizontal_edge = straight_edge.transpose(Image.Transpose.ROTATE_90)
    left_edge = Image.new("RGBA", (1, junction.height), (0, 0, 0, 0))
    left_edge.paste(horizontal_edge, (0, port_offset))
    junction.paste(left_edge, (0, 0))
    right_edge = Image.new("RGBA", (1, junction.height), (0, 0, 0, 0))
    right_edge.paste(horizontal_edge, (0, port_offset))
    junction.paste(right_edge, (junction.width - 1, 0))
    junction.paste(
        Image.new("RGBA", (junction.width, 1), (0, 0, 0, 0)),
        (0, junction.height - 1),
    )
    return junction


def make_path_t_junction(path_master: Image.Image) -> Image.Image:
    """Create one 512px T-junction with exact straight-path ports."""

    source = Image.open(RAW / "path-t-junction-overlay.png").convert("RGBA")
    junction = ImageOps.fit(source, (512, 512), method=RESAMPLE, centering=(0.5, 0.5))
    junction = alpha_clean(junction, cutoff=24)
    return enforce_path_t_junction_ports(junction, path_master)


def enforce_path_cross_junction_ports(
    junction: Image.Image, straight_path: Image.Image
) -> Image.Image:
    """Install exact four-way ports and transparent outer edge regions."""

    junction = junction.copy().convert("RGBA")
    port_width = straight_path.width
    port_offset = (junction.width - port_width) // 2
    port_depth = straight_path.height // 8
    straight_entry = straight_path.crop((0, 0, port_width, port_depth))
    junction.paste(straight_entry, (port_offset, 0))
    junction.paste(
        straight_entry.transpose(Image.Transpose.FLIP_TOP_BOTTOM),
        (port_offset, junction.height - port_depth),
    )

    horizontal_path = straight_path.transpose(Image.Transpose.ROTATE_90)
    left_entry = horizontal_path.crop((0, 0, port_depth, port_width))
    junction.paste(left_entry, (0, port_offset))
    junction.paste(
        left_entry.transpose(Image.Transpose.FLIP_LEFT_RIGHT),
        (junction.width - port_depth, port_offset),
    )

    straight_edge = straight_path.crop((0, 0, port_width, 1))
    top_edge = Image.new("RGBA", (junction.width, 1), (0, 0, 0, 0))
    top_edge.paste(straight_edge, (port_offset, 0))
    junction.paste(top_edge, (0, 0))
    junction.paste(top_edge, (0, junction.height - 1))

    horizontal_edge = straight_edge.transpose(Image.Transpose.ROTATE_90)
    side_edge = Image.new("RGBA", (1, junction.height), (0, 0, 0, 0))
    side_edge.paste(horizontal_edge, (0, port_offset))
    junction.paste(side_edge, (0, 0))
    junction.paste(side_edge, (junction.width - 1, 0))
    return junction


def make_path_cross_junction(path_master: Image.Image) -> Image.Image:
    """Create one 512px cross-junction with exact straight-path ports."""

    source = Image.open(RAW / "path-cross-junction-overlay.png").convert("RGBA")
    junction = ImageOps.fit(source, (512, 512), method=RESAMPLE, centering=(0.5, 0.5))
    junction = alpha_clean(junction, cutoff=24)
    return enforce_path_cross_junction_ports(junction, path_master)


def enforce_path_end_cap_port(
    end_cap: Image.Image, straight_path: Image.Image
) -> Image.Image:
    """Install one exact top port and keep the lower half fully transparent."""

    end_cap = end_cap.copy().convert("RGBA")
    port_width = straight_path.width
    port_offset = (end_cap.width - port_width) // 2
    port_depth = straight_path.height // 8
    straight_entry = straight_path.crop((0, 0, port_width, port_depth))
    end_cap.paste(straight_entry, (port_offset, 0))

    top_edge = Image.new("RGBA", (end_cap.width, 1), (0, 0, 0, 0))
    top_edge.paste(straight_path.crop((0, 0, port_width, 1)), (port_offset, 0))
    end_cap.paste(top_edge, (0, 0))
    end_cap.paste(
        Image.new("RGBA", (end_cap.width, end_cap.height // 2), (0, 0, 0, 0)),
        (0, end_cap.height // 2),
    )
    end_cap.paste(Image.new("RGBA", (1, end_cap.height), (0, 0, 0, 0)), (0, 0))
    end_cap.paste(
        Image.new("RGBA", (1, end_cap.height), (0, 0, 0, 0)),
        (end_cap.width - 1, 0),
    )
    return end_cap


def make_path_end_cap(path_master: Image.Image) -> Image.Image:
    """Create one 512px end-cap with an exact top port and rounded terminus."""

    source = crop_alpha(
        alpha_clean(Image.open(RAW / "path-end-cap-overlay.png"), cutoff=24)
    )
    terminus_source = source.crop(
        (0, round(source.height * 0.42), source.width, source.height)
    )
    terminus = terminus_source.resize((224, 164), RESAMPLE)
    terminus = alpha_clean(terminus, cutoff=24)
    end_cap = Image.new("RGBA", (512, 512), (0, 0, 0, 0))
    end_cap.alpha_composite(path_master.crop((0, 0, path_master.width, 120)), (160, 0))
    end_cap.alpha_composite(terminus, (144, 76))
    return enforce_path_end_cap_port(end_cap, path_master)


def make_cues() -> dict[str, Image.Image]:
    """Create geometry-only state cues on the exact 160x160 table canvas."""

    size = (160, 160)
    noticeable = Image.new("RGBA", size, (0, 0, 0, 0))
    draw = ImageDraw.Draw(noticeable)
    tick = (247, 227, 161, 255)
    outline = (93, 73, 39, 255)
    draw.polygon([(12, 132), (30, 125), (36, 132), (18, 142)], fill=outline)
    draw.polygon([(15, 132), (29, 127), (32, 131), (18, 138)], fill=tick)

    identified = Image.new("RGBA", size, (0, 0, 0, 0))
    draw = ImageDraw.Draw(identified)
    contour = [
        (20, 62),
        (54, 28),
        (111, 25),
        (145, 61),
        (143, 127),
        (112, 151),
        (50, 151),
        (18, 126),
    ]
    draw.line(contour + [contour[0]], fill=(93, 73, 39, 255), width=10, joint="curve")
    draw.line(contour + [contour[0]], fill=(247, 227, 161, 255), width=6, joint="curve")
    inner = [
        (28, 65),
        (58, 36),
        (107, 33),
        (137, 64),
        (135, 122),
        (108, 143),
        (54, 143),
        (26, 121),
    ]
    draw.line(inner + [inner[0]], fill=(247, 227, 161, 255), width=3, joint="curve")
    draw.polygon([(80, 3), (94, 17), (80, 31), (66, 17)], fill=(93, 73, 39, 255))
    draw.polygon([(80, 7), (90, 17), (80, 27), (70, 17)], fill=(247, 227, 161, 255))

    dangerous = Image.new("RGBA", size, (0, 0, 0, 0))
    draw = ImageDraw.Draw(dangerous)
    brick = (168, 68, 50, 220)
    dark = (74, 45, 35, 255)
    draw.polygon([(80, 2), (100, 31), (60, 31)], fill=dark)
    draw.polygon([(80, 7), (95, 28), (65, 28)], fill=(168, 68, 50, 255))
    hatch_bounds = (28, 55, 138, 137)
    mask = Image.new("L", size, 0)
    ImageDraw.Draw(mask).rounded_rectangle(hatch_bounds, radius=12, fill=150)
    hatch = Image.new("RGBA", size, (0, 0, 0, 0))
    hatch_draw = ImageDraw.Draw(hatch)
    for offset in range(-100, 220, 18):
        hatch_draw.line((offset, 145, offset + 100, 45), fill=brick, width=7)
        hatch_draw.line((offset + 5, 145, offset + 105, 45), fill=dark, width=2)
    dangerous.alpha_composite(Image.composite(hatch, Image.new("RGBA", size), mask))
    return {"noticeable": noticeable, "identified": identified, "dangerous": dangerous}


def seeker_frame_sets() -> dict[str, list[Image.Image]]:
    """Extract and normalize the four approved six-frame direction rows."""

    directions = [("south", 0, 72), ("west", 1, 96), ("east", 2, 96), ("north", 3, 72)]
    sheet = Image.open(LEGACY_SEEKER).convert("RGBA")
    row_height = sheet.height / 4
    frame_sets: dict[str, list[Image.Image]] = {}
    for direction, row, canvas_width in directions:
        cells: list[Image.Image] = []
        top = round(row * row_height)
        bottom = round((row + 1) * row_height)
        for index in range(6):
            left = round(index * sheet.width / 6)
            right = round((index + 1) * sheet.width / 6)
            cell = sheet.crop((left, top, right, bottom))
            cells.append(crop_alpha(alpha_clean(cell, cutoff=24)))

        common_scale = min(
            (canvas_width - 8) / max(cell.width for cell in cells),
            100 / max(cell.height for cell in cells),
        )
        frames: list[Image.Image] = []
        for cell in cells:
            resized = cell.resize(
                (
                    max(1, round(cell.width * common_scale)),
                    max(1, round(cell.height * common_scale)),
                ),
                RESAMPLE,
            )
            resized = alpha_clean(resized, cutoff=16)
            frame = Image.new("RGBA", (canvas_width, 112), (0, 0, 0, 0))
            frame.alpha_composite(
                resized,
                ((canvas_width - resized.width) // 2, 104 - resized.height),
            )
            frames.append(frame)
        frame_sets[direction] = frames
    return frame_sets


def seeker_idle_frame_sets() -> dict[str, list[Image.Image]]:
    """Normalize each generated direction's neutral and inhale proof frames."""

    frame_sets = {}
    for direction, canvas_width in [
        ("south", 72),
        ("west", 96),
        ("east", 96),
        ("north", 72),
    ]:
        sheet = Image.open(RAW / f"seeker-{direction}-idle-breathe.png").convert(
            "RGBA"
        )
        cells = []
        for index in range(2):
            left = round(index * sheet.width / 2)
            right = round((index + 1) * sheet.width / 2)
            cells.append(
                crop_alpha(alpha_clean(sheet.crop((left, 0, right, sheet.height))))
            )

        common_scale = min(
            (canvas_width - 12) / max(cell.width for cell in cells),
            94 / max(cell.height for cell in cells),
        )
        frames = []
        for cell in cells:
            resized = cell.resize(
                (
                    max(1, round(cell.width * common_scale)),
                    max(1, round(cell.height * common_scale)),
                ),
                RESAMPLE,
            )
            resized = alpha_clean(resized, cutoff=16)
            frame = Image.new("RGBA", (canvas_width, 112), (0, 0, 0, 0))
            frame.alpha_composite(
                resized,
                ((canvas_width - resized.width) // 2, 104 - resized.height),
            )
            frames.append(frame)
        frame_sets[direction] = frames
    return frame_sets


def seeker_interaction_frame_sets() -> dict[str, list[Image.Image]]:
    """Normalize each generated direction's four-frame reach-and-inspect proof."""

    frame_sets = {}
    for direction, canvas_width in [
        ("south", 128),
        ("west", 128),
        ("east", 128),
        ("north", 128),
    ]:
        sheet = Image.open(RAW / f"seeker-{direction}-interact-reach.png").convert(
            "RGBA"
        )
        cells = []
        for index in range(4):
            left = round(index * sheet.width / 4)
            right = round((index + 1) * sheet.width / 4)
            cells.append(
                crop_alpha(alpha_clean(sheet.crop((left, 0, right, sheet.height))))
            )

        common_scale = min(
            84 / max(cell.width for cell in cells),
            94 / max(cell.height for cell in cells),
        )
        frames = []
        for cell in cells:
            resized = cell.resize(
                (
                    max(1, round(cell.width * common_scale)),
                    max(1, round(cell.height * common_scale)),
                ),
                RESAMPLE,
            )
            resized = alpha_clean(resized, cutoff=16)
            frame = Image.new("RGBA", (canvas_width, 112), (0, 0, 0, 0))
            foot_band_top = max(0, resized.height - 20)
            foot_bbox = resized.getchannel("A").crop(
                (0, foot_band_top, resized.width, resized.height)
            ).getbbox()
            if foot_bbox is None:
                raise ValueError(f"{direction} interaction frame has no visible feet")
            anchored_x = round((canvas_width - foot_bbox[0] - foot_bbox[2]) / 2)
            anchored_x = max(0, min(anchored_x, canvas_width - resized.width))
            frame.alpha_composite(
                resized,
                (anchored_x, 104 - resized.height),
            )
            frames.append(frame)
        frame_sets[direction] = frames
    return frame_sets


def downsample(master: Image.Image) -> Image.Image:
    """Export one exact half-scale PNG from a 2x master."""

    resized = master.resize(
        (master.width // MASTER_SCALE, master.height // MASTER_SCALE), RESAMPLE
    )
    return alpha_clean(resized, cutoff=8)


def save_png(image: Image.Image, path: Path) -> None:
    """Save an RGBA PNG with deterministic encoder settings."""

    path.parent.mkdir(parents=True, exist_ok=True)
    image.convert("RGBA").save(path, format="PNG", optimize=False, compress_level=9)


def align_alpha_baseline(image: Image.Image, baseline: int) -> Image.Image:
    """Move retained alpha vertically so its last occupied row equals one baseline."""

    bbox = image.getchannel("A").getbbox()
    if bbox is None:
        raise ValueError("cannot align an empty frame")
    offset = baseline - bbox[3]
    if offset == 0:
        return image
    aligned = Image.new("RGBA", image.size, (0, 0, 0, 0))
    aligned.alpha_composite(image, (0, offset))
    return aligned


def write_zip_entry(
    archive: zipfile.ZipFile,
    name: str,
    data: bytes,
    compress_type: int,
) -> None:
    """Write one reproducible ZIP member with a fixed timestamp and permissions."""

    info = zipfile.ZipInfo(name, date_time=(1980, 1, 1, 0, 0, 0))
    info.compress_type = compress_type
    info.external_attr = 0o100644 << 16
    archive.writestr(info, data)


def create_ora(
    path: Path,
    size: tuple[int, int],
    layers: list[tuple[str, Image.Image, int, int, bool]],
) -> None:
    """Write a minimal standards-based editable OpenRaster master."""

    path.parent.mkdir(parents=True, exist_ok=True)
    merged = Image.new("RGBA", size, (0, 0, 0, 0))
    for _, layer, x, y, visible in layers:
        if visible:
            merged.alpha_composite(layer, (x, y))

    with tempfile.TemporaryDirectory() as temporary:
        temporary_root = Path(temporary)
        data_root = temporary_root / "data"
        thumbnail_root = temporary_root / "Thumbnails"
        data_root.mkdir()
        thumbnail_root.mkdir()

        image_node = Element(
            "image", version="0.0.1", w=str(size[0]), h=str(size[1]), name=path.stem
        )
        stack = SubElement(image_node, "stack", name="root")
        for index, (name, layer, x, y, visible) in enumerate(reversed(layers)):
            filename = f"layer{index:03d}.png"
            layer.save(data_root / filename, format="PNG", compress_level=9)
            SubElement(
                stack,
                "layer",
                name=name,
                src=f"data/{filename}",
                x=str(x),
                y=str(y),
                visibility="visible" if visible else "hidden",
                opacity="1.0",
                composite_op="svg:src-over",
            )
        (temporary_root / "stack.xml").write_bytes(
            tostring(image_node, encoding="utf-8", xml_declaration=True)
        )
        merged.save(temporary_root / "mergedimage.png", format="PNG", compress_level=9)
        thumbnail = merged.copy()
        thumbnail.thumbnail((256, 256), RESAMPLE)
        thumbnail.save(thumbnail_root / "thumbnail.png", format="PNG", compress_level=9)

        with zipfile.ZipFile(path, "w") as archive:
            write_zip_entry(
                archive,
                "mimetype",
                b"image/openraster",
                zipfile.ZIP_STORED,
            )
            for relative in [
                "stack.xml",
                "mergedimage.png",
                "Thumbnails/thumbnail.png",
            ]:
                write_zip_entry(
                    archive,
                    relative,
                    (temporary_root / relative).read_bytes(),
                    zipfile.ZIP_DEFLATED,
                )
            for layer_path in sorted(data_root.glob("*.png")):
                write_zip_entry(
                    archive,
                    f"data/{layer_path.name}",
                    layer_path.read_bytes(),
                    zipfile.ZIP_DEFLATED,
                )


def hash_file(path: Path) -> str:
    """Return the lowercase SHA-256 digest for one file."""

    digest = hashlib.sha256()
    with path.open("rb") as stream:
        for chunk in iter(lambda: stream.read(1024 * 1024), b""):
            digest.update(chunk)
    return digest.hexdigest()


def make_review_scene(
    assets: dict[str, Image.Image],
) -> tuple[Image.Image, list[tuple[str, Image.Image, int, int, bool]]]:
    """Recompose one 960x540 review scene exclusively from corrected exports."""

    scene_size = (960, 540)
    ground = assets["terrain.quiet_moss"]
    ground_layer = Image.new("RGBA", scene_size, (0, 0, 0, 0))
    for y in range(0, scene_size[1], ground.height):
        for x in range(0, scene_size[0], ground.width):
            ground_layer.alpha_composite(ground, (x, y))

    path = assets["terrain.straight_path"]
    path_layer = Image.new("RGBA", scene_size, (0, 0, 0, 0))
    path_x = (scene_size[0] - path.width) // 2
    for y in range(0, scene_size[1], path.height):
        path_layer.alpha_composite(path, (path_x, y))

    props_layer = Image.new("RGBA", scene_size, (0, 0, 0, 0))
    bed = assets["plants.herb_bed_empty"]
    foliage = assets["plants.foliage_broadleaf"]
    table = assets["objects.preparation_table"]
    props_layer.alpha_composite(bed, (102, 128))
    props_layer.alpha_composite(bed, (706, 128))
    props_layer.alpha_composite(foliage, (718, 126))
    props_layer.alpha_composite(foliage, (128, 348))
    props_layer.alpha_composite(foliage, (742, 350))
    props_layer.alpha_composite(table, (650, 330))

    cue_layer = Image.new("RGBA", scene_size, (0, 0, 0, 0))
    cue_layer.alpha_composite(assets["cues.preparation_table_identified"], (650, 330))

    character_layer = Image.new("RGBA", scene_size, (0, 0, 0, 0))
    character_layer.alpha_composite(
        assets["characters.seeker.south.walk_01"], (462, 250)
    )

    layers = [
        ("ground", ground_layer, 0, 0, True),
        ("straight path", path_layer, 0, 0, True),
        ("herb beds and foliage", props_layer, 0, 0, True),
        ("identified cue", cue_layer, 0, 0, True),
        ("Seeker south frame 01", character_layer, 0, 0, True),
    ]
    scene = Image.new("RGBA", scene_size, (0, 0, 0, 0))
    for _, layer, _, _, visible in layers:
        if visible:
            scene.alpha_composite(layer)
    return scene, layers


def make_path_bend_review(assets: dict[str, Image.Image]) -> Image.Image:
    """Compose the external bend between exact straight-path connectors."""

    scene = Image.new("RGBA", (768, 768), (0, 0, 0, 0))
    ground = assets["terrain.quiet_moss"]
    for y in range(0, scene.height, ground.height):
        for x in range(0, scene.width, ground.width):
            scene.alpha_composite(ground, (x, y))

    straight = assets["terrain.straight_path"]
    bend = assets["terrain.path_bend"]
    scene.alpha_composite(straight, (336, 0))
    scene.alpha_composite(bend, (256, 256))
    scene.alpha_composite(straight.transpose(Image.Transpose.ROTATE_90), (512, 336))
    return scene


def make_path_t_junction_review(assets: dict[str, Image.Image]) -> Image.Image:
    """Compose the external T-junction between exact straight connectors."""

    scene = Image.new("RGBA", (768, 768), (0, 0, 0, 0))
    ground = assets["terrain.quiet_moss"]
    for y in range(0, scene.height, ground.height):
        for x in range(0, scene.width, ground.width):
            scene.alpha_composite(ground, (x, y))

    straight = assets["terrain.straight_path"]
    junction = assets["terrain.path_t_junction"]
    scene.alpha_composite(straight, (336, 0))
    scene.alpha_composite(straight.transpose(Image.Transpose.ROTATE_90), (0, 336))
    scene.alpha_composite(junction, (256, 256))
    scene.alpha_composite(straight.transpose(Image.Transpose.ROTATE_90), (512, 336))
    return scene


def make_path_cross_junction_review(assets: dict[str, Image.Image]) -> Image.Image:
    """Compose the external cross-junction between four exact connectors."""

    scene = Image.new("RGBA", (768, 768), (0, 0, 0, 0))
    ground = assets["terrain.quiet_moss"]
    for y in range(0, scene.height, ground.height):
        for x in range(0, scene.width, ground.width):
            scene.alpha_composite(ground, (x, y))

    straight = assets["terrain.straight_path"]
    horizontal = straight.transpose(Image.Transpose.ROTATE_90)
    junction = assets["terrain.path_cross_junction"]
    scene.alpha_composite(straight, (336, 0))
    scene.alpha_composite(horizontal, (0, 336))
    scene.alpha_composite(junction, (256, 256))
    scene.alpha_composite(horizontal, (512, 336))
    scene.alpha_composite(straight, (336, 512))
    return scene


def make_path_end_cap_review(assets: dict[str, Image.Image]) -> Image.Image:
    """Compose the external end-cap below one exact straight connector."""

    scene = Image.new("RGBA", (768, 768), (0, 0, 0, 0))
    ground = assets["terrain.quiet_moss"]
    for y in range(0, scene.height, ground.height):
        for x in range(0, scene.width, ground.width):
            scene.alpha_composite(ground, (x, y))

    scene.alpha_composite(assets["terrain.straight_path"], (336, 0))
    scene.alpha_composite(assets["terrain.path_end_cap"], (256, 256))
    return scene


def make_path_module_topology_review(assets: dict[str, Image.Image]) -> Image.Image:
    """Compose every path topology into one connected external review network."""

    scene = Image.new("RGBA", (1024, 768), (0, 0, 0, 0))
    ground = assets["terrain.quiet_moss"]
    for y in range(0, scene.height, ground.height):
        for x in range(0, scene.width, ground.width):
            scene.alpha_composite(ground, (x, y))

    straight = assets["terrain.straight_path"]
    horizontal = straight.transpose(Image.Transpose.ROTATE_90)
    scene.alpha_composite(
        assets["terrain.path_t_junction"].transpose(Image.Transpose.ROTATE_270),
        (0, 256),
    )
    scene.alpha_composite(straight, (80, 0))
    scene.alpha_composite(straight, (80, 512))
    scene.alpha_composite(horizontal, (256, 336))
    scene.alpha_composite(assets["terrain.path_cross_junction"], (512, 256))
    scene.alpha_composite(straight, (592, 0))
    scene.alpha_composite(straight, (592, 512))
    scene.alpha_composite(
        assets["terrain.path_bend"].transpose(Image.Transpose.ROTATE_180),
        (768, 256),
    )
    scene.alpha_composite(assets["terrain.path_end_cap"], (768, 512))
    return scene


def make_leaf_litter_review(assets: dict[str, Image.Image]) -> Image.Image:
    """Show four exact leaf-litter seams beside quiet moss and packed earth."""

    scene = Image.new("RGBA", (1024, 512), (0, 0, 0, 0))
    leaf_litter = assets["terrain.woodland_leaf_litter"]
    for y in (0, leaf_litter.height):
        for x in (0, leaf_litter.width):
            scene.alpha_composite(leaf_litter, (x, y))

    ground = assets["terrain.quiet_moss"]
    for y in (0, ground.height):
        for x in (512, 512 + ground.width):
            scene.alpha_composite(ground, (x, y))

    straight = assets["terrain.straight_path"]
    horizontal = straight.transpose(Image.Transpose.ROTATE_90)
    scene.alpha_composite(straight, (720, 0))
    scene.alpha_composite(horizontal, (512, 208))
    scene.alpha_composite(assets["terrain.path_cross_junction"], (640, 128))
    scene.alpha_composite(horizontal, (768, 208))
    scene.alpha_composite(straight, (720, 256))
    return scene


def make_leaf_litter_transition_review(assets: dict[str, Image.Image]) -> Image.Image:
    """Repeat the exact moss-to-leaf transition in two connected rows."""

    scene = Image.new("RGBA", (768, 512), (0, 0, 0, 0))
    modules = [
        assets["terrain.quiet_moss"],
        assets["terrain.woodland_leaf_litter_transition"],
        assets["terrain.woodland_leaf_litter"],
    ]
    for y in (0, 256):
        for index, module in enumerate(modules):
            scene.alpha_composite(module, (index * 256, y))
    return scene


def make_leaf_litter_corner_transition_review(
    assets: dict[str, Image.Image],
) -> Image.Image:
    """Connect the corner to exact ground and straight-transition neighbours."""

    moss = assets["terrain.quiet_moss"]
    leaf = assets["terrain.woodland_leaf_litter"]
    transition = assets["terrain.woodland_leaf_litter_transition"]
    corner = assets["terrain.woodland_leaf_litter_corner_transition"]
    rotated_transition = transition.transpose(Image.Transpose.ROTATE_90)
    modules = [
        [moss, transition, leaf],
        [moss, corner, rotated_transition],
        [moss, moss, moss],
    ]
    scene = Image.new("RGBA", (768, 768), (0, 0, 0, 0))
    for row, row_modules in enumerate(modules):
        for column, module in enumerate(row_modules):
            scene.alpha_composite(module, (column * 256, row * 256))
    return scene


def make_leaf_litter_left_corner_transition_review(
    assets: dict[str, Image.Image],
) -> Image.Image:
    """Connect the left corner to exact ground and transition neighbours."""

    moss = assets["terrain.quiet_moss"]
    leaf = assets["terrain.woodland_leaf_litter"]
    transition = assets["terrain.woodland_leaf_litter_transition"]
    corner = assets["terrain.woodland_leaf_litter_corner_transition_left"]
    flipped_transition = transition.transpose(Image.Transpose.FLIP_LEFT_RIGHT)
    left_transition = transition.transpose(Image.Transpose.ROTATE_90)
    modules = [
        [leaf, flipped_transition, moss],
        [left_transition, corner, moss],
        [moss, moss, moss],
    ]
    scene = Image.new("RGBA", (768, 768), (0, 0, 0, 0))
    for row, row_modules in enumerate(modules):
        for column, module in enumerate(row_modules):
            scene.alpha_composite(module, (column * 256, row * 256))
    return scene


def make_leaf_litter_inset_corner_transition_review(
    assets: dict[str, Image.Image],
) -> Image.Image:
    """Connect one moss inset corner to exact leaf and transition neighbours."""

    moss = assets["terrain.quiet_moss"]
    leaf = assets["terrain.woodland_leaf_litter"]
    transition = assets["terrain.woodland_leaf_litter_transition"]
    corner = assets["terrain.woodland_leaf_litter_inset_corner_transition"]
    top_transition = transition.transpose(Image.Transpose.FLIP_LEFT_RIGHT)
    right_transition = transition.transpose(Image.Transpose.ROTATE_270)
    modules = [
        [leaf, top_transition, moss],
        [leaf, corner, right_transition],
        [leaf, leaf, leaf],
    ]
    scene = Image.new("RGBA", (768, 768), (0, 0, 0, 0))
    for row, row_modules in enumerate(modules):
        for column, module in enumerate(row_modules):
            scene.alpha_composite(module, (column * 256, row * 256))
    return scene


def make_leaf_litter_left_inset_corner_transition_review(
    assets: dict[str, Image.Image],
) -> Image.Image:
    """Connect the complementary moss inset to exact leaf and transition neighbours."""

    moss = assets["terrain.quiet_moss"]
    leaf = assets["terrain.woodland_leaf_litter"]
    transition = assets["terrain.woodland_leaf_litter_transition"]
    corner = assets[
        "terrain.woodland_leaf_litter_inset_corner_transition_left"
    ]
    left_transition = transition.transpose(Image.Transpose.ROTATE_270)
    modules = [
        [moss, transition, leaf],
        [left_transition, corner, leaf],
        [leaf, leaf, leaf],
    ]
    scene = Image.new("RGBA", (768, 768), (0, 0, 0, 0))
    for row, row_modules in enumerate(modules):
        for column, module in enumerate(row_modules):
            scene.alpha_composite(module, (column * 256, row * 256))
    return scene


def state_cue_check(assets: dict[str, Image.Image]) -> Image.Image:
    """Render ordinary, noticeable, identified, and dangerous variants in grayscale."""

    master = Image.new("RGBA", (4 * 176, 176), (232, 224, 203, 255))
    cue_order: list[str | None] = [None, "noticeable", "identified", "dangerous"]
    for index, cue_name in enumerate(cue_order):
        cell = Image.new("RGBA", (160, 160), (82, 95, 65, 255))
        table = assets["objects.preparation_table"].resize((160, 160), RESAMPLE)
        cell.alpha_composite(table)
        if cue_name is not None:
            cue = assets[f"cues.preparation_table_{cue_name}"].resize(
                (160, 160), RESAMPLE
            )
            cell.alpha_composite(cue)
        gray = ImageOps.grayscale(cell).convert("RGBA")
        master.alpha_composite(gray, (index * 176 + 8, 8))
    return downsample(master)


def main() -> None:
    """Build exports, editable masters, review evidence, and the asset manifest."""

    for directory in [EXPORTS, MASTERS, REVIEW]:
        directory.mkdir(parents=True, exist_ok=True)

    ground_master = make_ground()
    leaf_litter_reference_master, leaf_litter_master = make_leaf_litter()
    leaf_litter_transition_master = make_leaf_litter_transition(
        ground_master, leaf_litter_master
    )
    leaf_litter_corner_transition_master = make_leaf_litter_corner_transition(
        ground_master, leaf_litter_master, leaf_litter_transition_master
    )
    leaf_litter_left_corner_transition_master = (
        make_leaf_litter_left_corner_transition(
            ground_master, leaf_litter_master, leaf_litter_transition_master
        )
    )
    leaf_litter_inset_corner_transition_master = (
        make_leaf_litter_inset_corner_transition(
            ground_master, leaf_litter_master, leaf_litter_transition_master
        )
    )
    leaf_litter_left_inset_corner_transition_master = (
        make_leaf_litter_left_inset_corner_transition(
            ground_master, leaf_litter_master, leaf_litter_transition_master
        )
    )
    path_master = make_path()
    path_bend_master = make_path_bend(path_master)
    path_t_junction_master = make_path_t_junction(path_master)
    path_cross_junction_master = make_path_cross_junction(path_master)
    path_end_cap_master = make_path_end_cap(path_master)
    bed_master = contain_on_canvas(
        Image.open(RAW / "herb-bed-empty.png"), (192, 160), (180, 148), 154
    )
    foliage_master = contain_on_canvas(
        Image.open(RAW / "foliage-broadleaf.png"), (144, 112), (136, 100), 106
    )
    table_master = contain_on_canvas(
        Image.open(RAW / "preparation-table.png"), (160, 160), (146, 146), 154
    )
    cues = make_cues()
    frame_sets = seeker_frame_sets()
    idle_frame_sets = seeker_idle_frame_sets()
    interaction_frame_sets = seeker_interaction_frame_sets()

    assets = [
        Asset(
            "terrain.quiet_moss",
            "terrain",
            Path("exports/terrain/quiet-moss.png"),
            (512, 512),
            (256, 512),
            512,
            ground_master,
            "sources/imagegen/quiet-moss-ground.png",
        ),
        Asset(
            "terrain.woodland_leaf_litter",
            "terrain",
            Path("exports/terrain/woodland-leaf-litter.png"),
            (512, 512),
            (256, 512),
            512,
            leaf_litter_master,
            "sources/imagegen/woodland-leaf-litter-ground.png",
        ),
        Asset(
            "terrain.woodland_leaf_litter_transition",
            "terrain",
            Path("exports/terrain/woodland-leaf-litter-transition.png"),
            (512, 512),
            (256, 512),
            512,
            leaf_litter_transition_master,
            "tools/build.py",
        ),
        Asset(
            "terrain.woodland_leaf_litter_corner_transition",
            "terrain",
            Path("exports/terrain/woodland-leaf-litter-corner-transition.png"),
            (512, 512),
            (256, 512),
            512,
            leaf_litter_corner_transition_master,
            "tools/build.py",
        ),
        Asset(
            "terrain.woodland_leaf_litter_corner_transition_left",
            "terrain",
            Path("exports/terrain/woodland-leaf-litter-corner-transition-left.png"),
            (512, 512),
            (256, 512),
            512,
            leaf_litter_left_corner_transition_master,
            "tools/build.py",
        ),
        Asset(
            "terrain.woodland_leaf_litter_inset_corner_transition",
            "terrain",
            Path("exports/terrain/woodland-leaf-litter-inset-corner-transition.png"),
            (512, 512),
            (256, 512),
            512,
            leaf_litter_inset_corner_transition_master,
            "tools/build.py",
        ),
        Asset(
            "terrain.woodland_leaf_litter_inset_corner_transition_left",
            "terrain",
            Path(
                "exports/terrain/"
                "woodland-leaf-litter-inset-corner-transition-left.png"
            ),
            (512, 512),
            (256, 512),
            512,
            leaf_litter_left_inset_corner_transition_master,
            "tools/build.py",
        ),
        Asset(
            "terrain.straight_path",
            "terrain",
            Path("exports/terrain/straight-path.png"),
            (192, 512),
            (96, 512),
            512,
            path_master,
            "sources/imagegen/straight-path-overlay.png",
        ),
        Asset(
            "terrain.path_bend",
            "terrain",
            Path("exports/terrain/path-bend.png"),
            (512, 512),
            (256, 256),
            512,
            path_bend_master,
            "sources/imagegen/path-bend-overlay.png",
        ),
        Asset(
            "terrain.path_t_junction",
            "terrain",
            Path("exports/terrain/path-t-junction.png"),
            (512, 512),
            (256, 256),
            512,
            path_t_junction_master,
            "sources/imagegen/path-t-junction-overlay.png",
        ),
        Asset(
            "terrain.path_cross_junction",
            "terrain",
            Path("exports/terrain/path-cross-junction.png"),
            (512, 512),
            (256, 256),
            512,
            path_cross_junction_master,
            "sources/imagegen/path-cross-junction-overlay.png",
        ),
        Asset(
            "terrain.path_end_cap",
            "terrain",
            Path("exports/terrain/path-end-cap.png"),
            (512, 512),
            (256, 256),
            512,
            path_end_cap_master,
            "sources/imagegen/path-end-cap-overlay.png",
        ),
        Asset(
            "plants.herb_bed_empty",
            "plants",
            Path("exports/plants/herb-bed-empty.png"),
            (192, 160),
            (96, 154),
            154,
            bed_master,
            "sources/imagegen/herb-bed-empty.png",
        ),
        Asset(
            "plants.foliage_broadleaf",
            "plants",
            Path("exports/plants/foliage-broadleaf.png"),
            (144, 112),
            (72, 106),
            106,
            foliage_master,
            "sources/imagegen/foliage-broadleaf.png",
        ),
        Asset(
            "objects.preparation_table",
            "objects",
            Path("exports/objects/preparation-table.png"),
            (160, 160),
            (80, 154),
            154,
            table_master,
            "sources/imagegen/preparation-table.png",
        ),
    ]
    for cue_name, cue in cues.items():
        assets.append(
            Asset(
                f"cues.preparation_table_{cue_name}",
                "cues",
                Path(f"exports/cues/preparation-table-{cue_name}.png"),
                (160, 160),
                (80, 154),
                154,
                cue,
                "tools/build.py",
            )
        )
    for direction, direction_frames in frame_sets.items():
        for index, frame in enumerate(direction_frames, start=1):
            assets.append(
                Asset(
                    f"characters.seeker.{direction}.walk_{index:02d}",
                    "characters",
                    Path(f"exports/characters/seeker/{direction}/walk-{index:02d}.png"),
                    frame.size,
                    (frame.width // 2, 104),
                    104,
                    frame,
                    "../sources/seeker-four-direction-walk.png",
                )
            )
    for direction, direction_frames in idle_frame_sets.items():
        for index, frame in enumerate(direction_frames, start=1):
            assets.append(
                Asset(
                    f"characters.seeker.{direction}.idle_breathe_{index:02d}",
                    "characters",
                    Path(
                        "exports/characters/seeker/"
                        f"{direction}/idle-breathe-{index:02d}.png"
                    ),
                    frame.size,
                    (frame.width // 2, 104),
                    104,
                    frame,
                    f"sources/imagegen/seeker-{direction}-idle-breathe.png",
                )
            )
    for direction, direction_frames in interaction_frame_sets.items():
        for index, frame in enumerate(direction_frames, start=1):
            assets.append(
                Asset(
                    f"characters.seeker.{direction}.interact_reach_{index:02d}",
                    "characters",
                    Path(
                        "exports/characters/seeker/"
                        f"{direction}/interact-reach-{index:02d}.png"
                    ),
                    frame.size,
                    (frame.width // 2, 104),
                    104,
                    frame,
                    f"sources/imagegen/seeker-{direction}-interact-reach.png",
                )
            )

    export_images: dict[str, Image.Image] = {}
    for asset in assets:
        export = downsample(asset.image_master)
        if asset.category == "characters":
            export = align_alpha_baseline(export, asset.baseline_master // MASTER_SCALE)
        if asset.asset_id in {
            "terrain.quiet_moss",
            "terrain.woodland_leaf_litter",
        }:
            export = enforce_opposite_edges(export, horizontal=True, vertical=True)
        elif asset.asset_id == "terrain.woodland_leaf_litter_transition":
            export = enforce_ground_transition_connectors(
                export,
                export_images["terrain.quiet_moss"],
                export_images["terrain.woodland_leaf_litter"],
            )
        elif asset.asset_id == "terrain.woodland_leaf_litter_corner_transition":
            export = enforce_ground_corner_connectors(
                export,
                export_images["terrain.quiet_moss"],
                export_images["terrain.woodland_leaf_litter_transition"],
            )
        elif asset.asset_id == "terrain.woodland_leaf_litter_corner_transition_left":
            export = enforce_left_ground_corner_connectors(
                export,
                export_images["terrain.quiet_moss"],
                export_images["terrain.woodland_leaf_litter_transition"],
            )
        elif asset.asset_id == "terrain.woodland_leaf_litter_inset_corner_transition":
            export = enforce_inset_ground_corner_connectors(
                export,
                export_images["terrain.woodland_leaf_litter"],
                export_images["terrain.woodland_leaf_litter_transition"],
            )
        elif (
            asset.asset_id
            == "terrain.woodland_leaf_litter_inset_corner_transition_left"
        ):
            export = enforce_left_inset_ground_corner_connectors(
                export,
                export_images["terrain.woodland_leaf_litter"],
                export_images["terrain.woodland_leaf_litter_transition"],
            )
        elif asset.asset_id == "terrain.straight_path":
            export = alpha_clean(
                enforce_opposite_edges(export, horizontal=False, vertical=True),
                cutoff=8,
            )
        elif asset.asset_id == "terrain.path_bend":
            export = enforce_path_bend_ports(
                alpha_clean(export, cutoff=8),
                export_images["terrain.straight_path"],
            )
        elif asset.asset_id == "terrain.path_t_junction":
            export = enforce_path_t_junction_ports(
                alpha_clean(export, cutoff=8),
                export_images["terrain.straight_path"],
            )
        elif asset.asset_id == "terrain.path_cross_junction":
            export = enforce_path_cross_junction_ports(
                alpha_clean(export, cutoff=8),
                export_images["terrain.straight_path"],
            )
        elif asset.asset_id == "terrain.path_end_cap":
            export = enforce_path_end_cap_port(
                alpha_clean(export, cutoff=8),
                export_images["terrain.straight_path"],
            )
        save_png(export, ROOT / asset.export_path)
        export_images[asset.asset_id] = export

    create_ora(
        MASTERS / "terrain-straight-path.ora",
        ground_master.size,
        [
            ("quiet moss ground", ground_master, 0, 0, True),
            ("straight path", path_master, 160, 0, True),
        ],
    )
    create_ora(
        MASTERS / "terrain-woodland-leaf-litter.ora",
        leaf_litter_master.size,
        [
            ("generated reference", leaf_litter_reference_master, 0, 0, False),
            ("repeatable woodland leaf litter", leaf_litter_master, 0, 0, True),
        ],
    )
    create_ora(
        MASTERS / "terrain-woodland-leaf-litter-transition.ora",
        leaf_litter_transition_master.size,
        [
            ("quiet moss source", ground_master, 0, 0, False),
            ("woodland leaf-litter source", leaf_litter_master, 0, 0, False),
            (
                "repeatable quiet-moss to leaf-litter transition",
                leaf_litter_transition_master,
                0,
                0,
                True,
            ),
        ],
    )
    create_ora(
        MASTERS / "terrain-woodland-leaf-litter-corner-transition.ora",
        leaf_litter_corner_transition_master.size,
        [
            ("quiet moss source", ground_master, 0, 0, False),
            ("woodland leaf-litter source", leaf_litter_master, 0, 0, False),
            ("straight transition source", leaf_litter_transition_master, 0, 0, False),
            (
                "quiet-moss to leaf-litter corner transition",
                leaf_litter_corner_transition_master,
                0,
                0,
                True,
            ),
        ],
    )
    create_ora(
        MASTERS / "terrain-woodland-leaf-litter-corner-transition-left.ora",
        leaf_litter_left_corner_transition_master.size,
        [
            ("quiet moss source", ground_master, 0, 0, False),
            ("woodland leaf-litter source", leaf_litter_master, 0, 0, False),
            ("straight transition source", leaf_litter_transition_master, 0, 0, False),
            (
                "quiet-moss to leaf-litter left corner transition",
                leaf_litter_left_corner_transition_master,
                0,
                0,
                True,
            ),
        ],
    )
    create_ora(
        MASTERS / "terrain-woodland-leaf-litter-inset-corner-transition.ora",
        leaf_litter_inset_corner_transition_master.size,
        [
            ("quiet moss source", ground_master, 0, 0, False),
            ("woodland leaf-litter source", leaf_litter_master, 0, 0, False),
            ("straight transition source", leaf_litter_transition_master, 0, 0, False),
            (
                "quiet-moss inset corner in woodland leaf litter",
                leaf_litter_inset_corner_transition_master,
                0,
                0,
                True,
            ),
        ],
    )
    create_ora(
        MASTERS / "terrain-woodland-leaf-litter-inset-corner-transition-left.ora",
        leaf_litter_left_inset_corner_transition_master.size,
        [
            ("quiet moss source", ground_master, 0, 0, False),
            ("woodland leaf-litter source", leaf_litter_master, 0, 0, False),
            ("straight transition source", leaf_litter_transition_master, 0, 0, False),
            (
                "quiet-moss left inset corner in woodland leaf litter",
                leaf_litter_left_inset_corner_transition_master,
                0,
                0,
                True,
            ),
        ],
    )
    create_ora(
        MASTERS / "terrain-path-bend.ora",
        path_bend_master.size,
        [("packed-earth path bend", path_bend_master, 0, 0, True)],
    )
    create_ora(
        MASTERS / "terrain-path-t-junction.ora",
        path_t_junction_master.size,
        [("packed-earth path T-junction", path_t_junction_master, 0, 0, True)],
    )
    create_ora(
        MASTERS / "terrain-path-cross-junction.ora",
        path_cross_junction_master.size,
        [("packed-earth path cross-junction", path_cross_junction_master, 0, 0, True)],
    )
    create_ora(
        MASTERS / "terrain-path-end-cap.ora",
        path_end_cap_master.size,
        [("packed-earth path end-cap", path_end_cap_master, 0, 0, True)],
    )
    create_ora(
        MASTERS / "planted-herb-bed.ora",
        bed_master.size,
        [
            ("empty herb bed", bed_master, 0, 0, True),
            ("broadleaf foliage", foliage_master, 24, 34, True),
        ],
    )
    create_ora(
        MASTERS / "preparation-table-states.ora",
        table_master.size,
        [
            ("ordinary table", table_master, 0, 0, True),
            ("noticeable cue", cues["noticeable"], 0, 0, False),
            ("identified cue", cues["identified"], 0, 0, True),
            ("dangerous cue", cues["dangerous"], 0, 0, False),
        ],
    )
    obsolete_south_master = MASTERS / "seeker-south-walk.ora"
    if obsolete_south_master.exists():
        obsolete_south_master.unlink()
    seeker_layers = []
    direction_order = ["south", "west", "east", "north"]
    for row, direction in enumerate(direction_order):
        for index, frame in enumerate(frame_sets[direction], start=1):
            x = (index - 1) * 96 + (96 - frame.width) // 2
            seeker_layers.append(
                (f"{direction} walk {index:02d}", frame, x, row * 112, True)
            )
    create_ora(
        MASTERS / "seeker-four-direction-walk.ora", (6 * 96, 4 * 112), seeker_layers
    )
    seeker_review = Image.new("RGBA", (6 * 96, 4 * 112), (0, 0, 0, 0))
    for _, frame, x, y, _ in seeker_layers:
        seeker_review.alpha_composite(frame, (x, y))
    save_png(downsample(seeker_review), REVIEW / "seeker-four-direction-walk.png")

    obsolete_idle_master = MASTERS / "seeker-south-idle-breathe.ora"
    if obsolete_idle_master.exists():
        obsolete_idle_master.unlink()
    obsolete_idle_review = REVIEW / "seeker-south-idle-breathe.png"
    if obsolete_idle_review.exists():
        obsolete_idle_review.unlink()
    idle_layers = []
    for row, direction in enumerate(direction_order):
        for index, frame in enumerate(idle_frame_sets[direction], start=1):
            x = (index - 1) * 96 + (96 - frame.width) // 2
            idle_layers.append(
                (f"{direction} idle breathe {index:02d}", frame, x, row * 112, True)
            )
    create_ora(
        MASTERS / "seeker-four-direction-idle-breathe.ora",
        (2 * 96, 4 * 112),
        idle_layers,
    )
    idle_review = Image.new("RGBA", (2 * 96, 4 * 112), (0, 0, 0, 0))
    for _, frame, x, y, _ in idle_layers:
        idle_review.alpha_composite(frame, (x, y))
    save_png(downsample(idle_review), REVIEW / "seeker-four-direction-idle-breathe.png")

    obsolete_interaction_master = MASTERS / "seeker-south-interact-reach.ora"
    if obsolete_interaction_master.exists():
        obsolete_interaction_master.unlink()
    obsolete_interaction_review = REVIEW / "seeker-south-interact-reach.png"
    if obsolete_interaction_review.exists():
        obsolete_interaction_review.unlink()
    interaction_cell_width = 128
    interaction_layers = []
    for row, direction in enumerate(direction_order):
        for index, frame in enumerate(interaction_frame_sets[direction], start=1):
            x = (index - 1) * interaction_cell_width + (
                interaction_cell_width - frame.width
            ) // 2
            interaction_layers.append(
                (f"{direction} interact reach {index:02d}", frame, x, row * 112, True)
            )
    create_ora(
        MASTERS / "seeker-four-direction-interact-reach.ora",
        (4 * interaction_cell_width, 4 * 112),
        interaction_layers,
    )
    interaction_review = Image.new(
        "RGBA", (4 * interaction_cell_width, 4 * 112), (0, 0, 0, 0)
    )
    for _, frame, x, y, _ in interaction_layers:
        interaction_review.alpha_composite(frame, (x, y))
    save_png(
        downsample(interaction_review),
        REVIEW / "seeker-four-direction-interact-reach.png",
    )

    scene, scene_layers = make_review_scene(export_images)
    save_png(scene, REVIEW / "module-review-scene.png")
    save_png(
        ImageOps.grayscale(scene).convert("RGBA"),
        REVIEW / "module-review-scene-grayscale.png",
    )
    create_ora(MASTERS / "module-review-scene.ora", scene.size, scene_layers)
    save_png(state_cue_check(export_images), REVIEW / "state-cues-grayscale.png")
    save_png(make_path_bend_review(export_images), REVIEW / "path-bend-review.png")
    save_png(
        make_path_t_junction_review(export_images),
        REVIEW / "path-t-junction-review.png",
    )
    save_png(
        make_path_cross_junction_review(export_images),
        REVIEW / "path-cross-junction-review.png",
    )
    save_png(
        make_path_end_cap_review(export_images),
        REVIEW / "path-end-cap-review.png",
    )
    save_png(
        make_path_module_topology_review(export_images),
        REVIEW / "path-module-topology-review.png",
    )
    save_png(
        make_leaf_litter_review(export_images),
        REVIEW / "woodland-leaf-litter-review.png",
    )
    save_png(
        make_leaf_litter_transition_review(export_images),
        REVIEW / "woodland-leaf-litter-transition-review.png",
    )
    save_png(
        make_leaf_litter_corner_transition_review(export_images),
        REVIEW / "woodland-leaf-litter-corner-transition-review.png",
    )
    save_png(
        make_leaf_litter_left_corner_transition_review(export_images),
        REVIEW / "woodland-leaf-litter-corner-transition-left-review.png",
    )
    save_png(
        make_leaf_litter_inset_corner_transition_review(export_images),
        REVIEW / "woodland-leaf-litter-inset-corner-transition-review.png",
    )
    save_png(
        make_leaf_litter_left_inset_corner_transition_review(export_images),
        REVIEW / "woodland-leaf-litter-inset-corner-transition-left-review.png",
    )

    manifest_assets = []
    for asset in assets:
        export_path = ROOT / asset.export_path
        export_width = asset.master_size[0] // MASTER_SCALE
        export_height = asset.master_size[1] // MASTER_SCALE
        manifest_assets.append(
            {
                "id": asset.asset_id,
                "category": asset.category,
                "source": asset.source,
                "export": asset.export_path.as_posix(),
                "logical_rect": {
                    "x": 0,
                    "y": 0,
                    "width": export_width,
                    "height": export_height,
                },
                "export_rect_pixels": {
                    "x": 0,
                    "y": 0,
                    "width": export_width,
                    "height": export_height,
                },
                "master_dimensions_pixels": {
                    "width": asset.master_size[0],
                    "height": asset.master_size[1],
                    "scale": MASTER_SCALE,
                },
                "contact_baseline": asset.baseline_master // MASTER_SCALE,
                "pivot": {
                    "x": asset.pivot_master[0] // MASTER_SCALE,
                    "y": asset.pivot_master[1] // MASTER_SCALE,
                },
                "godot_import_proof": asset.godot_import_proof,
                "sha256": hash_file(export_path),
            }
        )

    manifest = {
        "schema_version": 1,
        "status": "corrected production-pipeline proof with isolated Godot import parity",
        "logical_viewport": {"width": 960, "height": 540},
        "master_scale": MASTER_SCALE,
        "navigation_clearance": {
            "scene": "review/module-review-scene.png",
            "x": 432,
            "y": 0,
            "width": 96,
            "height": 540,
        },
        "review_composition": [
            {
                "asset": "plants.herb_bed_empty",
                "x": 102,
                "y": 128,
                "fixed_obstacle": True,
            },
            {
                "asset": "plants.herb_bed_empty",
                "x": 706,
                "y": 128,
                "fixed_obstacle": True,
            },
            {
                "asset": "plants.foliage_broadleaf",
                "x": 718,
                "y": 126,
                "fixed_obstacle": True,
            },
            {
                "asset": "plants.foliage_broadleaf",
                "x": 128,
                "y": 348,
                "fixed_obstacle": True,
            },
            {
                "asset": "plants.foliage_broadleaf",
                "x": 742,
                "y": 350,
                "fixed_obstacle": True,
            },
            {
                "asset": "objects.preparation_table",
                "x": 650,
                "y": 330,
                "fixed_obstacle": True,
            },
            {
                "asset": "cues.preparation_table_identified",
                "x": 650,
                "y": 330,
                "fixed_obstacle": False,
            },
            {
                "asset": "characters.seeker.south.walk_01",
                "x": 462,
                "y": 250,
                "fixed_obstacle": False,
            },
        ],
        "animations": [
            {
                "id": f"characters.seeker.{direction}.walk",
                "frame_order": [
                    f"characters.seeker.{direction}.walk_{index:02d}"
                    for index in range(1, 7)
                ],
                "frame_duration_ms": 120,
                "loop": True,
            }
            for direction in direction_order
        ]
        + [
            {
                "id": f"characters.seeker.{direction}.idle_breathe",
                "frame_order": [
                    f"characters.seeker.{direction}.idle_breathe_{index:02d}"
                    for index in range(1, 3)
                ],
                "frame_duration_ms": 600,
                "loop": True,
            }
            for direction in direction_order
        ]
        + [
            {
                "id": f"characters.seeker.{direction}.interact_reach",
                "frame_order": [
                    f"characters.seeker.{direction}.interact_reach_{index:02d}"
                    for index in range(1, 5)
                ],
                "frame_duration_ms": 160,
                "loop": False,
            }
            for direction in direction_order
        ],
        "assets": manifest_assets,
    }
    (ROOT / "asset-manifest.json").write_text(
        json.dumps(manifest, indent=2) + "\n", encoding="utf-8"
    )


if __name__ == "__main__":
    main()
