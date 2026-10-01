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

from PIL import Image, ImageChops, ImageDraw, ImageOps


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
TABLE_MASTER_SIZE = (192, 192)
TABLE_MASTER_PIVOT = (96, 184)
TABLE_MASTER_BASELINE = 184
TABLE_MASTER_CONTENT_SIZE = (176, 176)
ALPHA_CUTOFF = 96
GROUND_CONNECTOR_DEPTH = 32
GROUND_CORNER_FEATHER_MASTER = 48.0
GROUND_CORNER_IRREGULARITY_MASTER = 14.0
OPENING_JUNCTION_CANVAS_SIZE = 512
OPENING_JUNCTION_PIVOT = (224, 224)
OPENING_JUNCTION_PATH_WIDTH = 96
OPENING_JUNCTION_PORT_DEPTH = 32
OPENING_JUNCTION_TURN_DEGREES = -57.599282
OPENING_JUNCTION_OUTGOING_HEADING_DEGREES = 90.0 + OPENING_JUNCTION_TURN_DEGREES
OPENING_JUNCTION_TEXTURE_BLEND_LENGTH = 72
OPENING_JUNCTION_CURVE_TANGENT_DISTANCE = 80
OPENING_JUNCTION_CURVE_EDGE_INSET = 2
VILLAGE_SQUARE_JUNCTION_CANVAS_SIZE = 512
VILLAGE_SQUARE_JUNCTION_PIVOT = (256, 256)
VILLAGE_SQUARE_JUNCTION_PATH_WIDTH = 96
VILLAGE_SQUARE_JUNCTION_PORT_DEPTH = 32
VILLAGE_SQUARE_JUNCTION_HEADINGS = (51.340192, 75.963757, 144.462322, 180.0)
VILLAGE_SQUARE_JUNCTION_CENTER_BLEND_RADIUS = 112
VILLAGE_EDGE_TRANSITION_CANVAS_SIZE = (1024, 512)
VILLAGE_EDGE_TRANSITION_WORLD_ORIGIN = (536, 136)
VILLAGE_EDGE_TRANSITION_OPENING = (592, 314)
VILLAGE_EDGE_TRANSITION_CLEARING_BOUNDS = (64, 64, 712, 314)
VILLAGE_EDGE_TRANSITION_FEATHER = 64
VILLAGE_EDGE_TRANSITION_ROUTE_LENGTH = 180
VILLAGE_EDGE_TRANSITION_PATH_WIDTH = 96
RESAMPLE = Image.Resampling.LANCZOS
LOGICAL_VIEWPORT_SIZE = (1280, 720)
FULLSCREEN_REFERENCE_LOGICAL_SIZE = (1280, 827)
SEEKER_MINIMUM_VISIBLE_HEIGHT = 144
SEEKER_MASTER_CANVAS_HEIGHT = 336
SEEKER_MASTER_BASELINE = 312
SEEKER_MASTER_CONTENT_HEIGHT = 300
SEEKER_MASTER_IDLE_CONTENT_HEIGHT = 304
SEEKER_MASTER_DIRECTION_WIDTHS = {
    "south": 216,
    "west": 288,
    "east": 288,
    "north": 216,
}
SEEKER_MASTER_REVIEW_CELL_WIDTH = 288
SEEKER_MASTER_INTERACTION_WIDTH = 384
REVIEW_PATH_X = (LOGICAL_VIEWPORT_SIZE[0] - 96) // 2
REVIEW_POSITIONS = {
    "left_bed": (146, 170),
    "right_bed": (980, 170),
    "right_bed_foliage": (990, 168),
    "left_foliage": (180, 480),
    "right_foliage": (1050, 490),
    "preparation_table": (930, 450),
    "seeker": (586, 276),
}


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


def make_village_edge_transition(
    leaf_litter: Image.Image,
) -> tuple[Image.Image, Image.Image]:
    """Create one world-aligned leaf overlay that feathers the clearing and Village route."""

    canvas_width = VILLAGE_EDGE_TRANSITION_CANVAS_SIZE[0] * MASTER_SCALE
    canvas_height = VILLAGE_EDGE_TRANSITION_CANVAS_SIZE[1] * MASTER_SCALE
    origin_x = VILLAGE_EDGE_TRANSITION_WORLD_ORIGIN[0] * MASTER_SCALE
    origin_y = VILLAGE_EDGE_TRANSITION_WORLD_ORIGIN[1] * MASTER_SCALE
    left, top, right, bottom = (
        coordinate * MASTER_SCALE
        for coordinate in VILLAGE_EDGE_TRANSITION_CLEARING_BOUNDS
    )
    feather = VILLAGE_EDGE_TRANSITION_FEATHER * MASTER_SCALE
    opening_x = VILLAGE_EDGE_TRANSITION_OPENING[0] * MASTER_SCALE
    opening_y = VILLAGE_EDGE_TRANSITION_OPENING[1] * MASTER_SCALE
    route_length = VILLAGE_EDGE_TRANSITION_ROUTE_LENGTH * MASTER_SCALE
    half_path = VILLAGE_EDGE_TRANSITION_PATH_WIDTH * MASTER_SCALE / 2.0
    direction_length = math.hypot(572.0, 450.0)
    direction_x = 572.0 / direction_length
    direction_y = 450.0 / direction_length
    route_end_x = opening_x + direction_x * route_length
    route_end_y = opening_y + direction_y * route_length

    overlay = Image.new("RGBA", (canvas_width, canvas_height), (0, 0, 0, 0))
    mask = Image.new("L", (canvas_width, canvas_height), 0)
    overlay_pixels = []
    mask_pixels = []
    for y in range(canvas_height):
        for x in range(canvas_width):
            outside_x = max(left - x, 0.0, x - right)
            outside_y = max(top - y, 0.0, y - bottom)
            rectangle_distance = math.hypot(outside_x, outside_y)
            irregularity = 12.0 * MASTER_SCALE * math.sin(
                (x + (0.73 * y)) * math.pi / (96.0 * MASTER_SCALE)
            )
            rectangle_blend = max(
                0.0,
                min(
                    1.0,
                    1.0
                    - (max(0.0, rectangle_distance + irregularity) / feather),
                ),
            )
            rectangle_blend = rectangle_blend * rectangle_blend * (
                3.0 - (2.0 * rectangle_blend)
            )
            segment_x = route_end_x - opening_x
            segment_y = route_end_y - opening_y
            segment_squared = (segment_x * segment_x) + (segment_y * segment_y)
            projection = (
                ((x - opening_x) * segment_x) + ((y - opening_y) * segment_y)
            ) / segment_squared
            projection = max(0.0, min(1.0, projection))
            closest_x = opening_x + projection * segment_x
            closest_y = opening_y + projection * segment_y
            route_distance = math.hypot(x - closest_x, y - closest_y) - half_path
            route_blend = max(
                0.0,
                min(1.0, 1.0 - (max(0.0, route_distance) / feather)),
            )
            route_blend = route_blend * route_blend * (3.0 - (2.0 * route_blend))
            alpha = round(max(rectangle_blend, route_blend) * 255)
            mask_pixels.append(alpha)
            if alpha == 0:
                overlay_pixels.append((0, 0, 0, 0))
                continue
            source = leaf_litter.getpixel(
                ((origin_x + x) % leaf_litter.width, (origin_y + y) % leaf_litter.height)
            )
            overlay_pixels.append((source[0], source[1], source[2], alpha))
    overlay.putdata(overlay_pixels)
    mask.putdata(mask_pixels)
    return overlay, Image.merge("RGBA", (mask, mask, mask, Image.new("L", mask.size, 255)))


def make_path() -> Image.Image:
    """Create the 192x512 2x straight-path overlay with repeatable vertical edges."""

    source = crop_alpha(alpha_clean(Image.open(RAW / "straight-path-overlay.png")))
    resized = ImageOps.fit(source, (192, 512), method=RESAMPLE, centering=(0.5, 0.5))
    resized = alpha_clean(resized, cutoff=24)
    return enforce_opposite_edges(resized, horizontal=False, vertical=True)


def render_path_leg(
    path: Image.Image,
    canvas_size: tuple[int, int],
    start: tuple[float, float],
    heading_degrees: float,
    length: float,
) -> Image.Image:
    """Render one finite repeatable path leg into a shared pixel coordinate system."""

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
        (
            right[0],
            right[1],
            offset_x,
            direction[0],
            direction[1],
            offset_y,
        ),
        resample=Image.Resampling.BICUBIC,
    )


def blend_opening_junction_legs(
    incoming: Image.Image,
    outgoing: Image.Image,
    pivot: tuple[float, float],
    path_width: int,
) -> Image.Image:
    """Blend two opaque legs through the turn without a hard texture overlap."""

    incoming_pixels = incoming.convert("RGBA").load()
    outgoing_pixels = outgoing.convert("RGBA").load()
    result = Image.new("RGBA", incoming.size, (0, 0, 0, 0))
    result_pixels = result.load()
    outgoing_angle = math.radians(OPENING_JUNCTION_OUTGOING_HEADING_DEGREES)
    outgoing_direction = (math.cos(outgoing_angle), math.sin(outgoing_angle))
    transition_direction = (
        outgoing_direction[0],
        1.0 + outgoing_direction[1],
    )
    transition_length = math.hypot(*transition_direction)
    transition_direction = (
        transition_direction[0] / transition_length,
        transition_direction[1] / transition_length,
    )
    blend_extent = OPENING_JUNCTION_TEXTURE_BLEND_LENGTH * (
        path_width / OPENING_JUNCTION_PATH_WIDTH
    )

    for y in range(result.height):
        for x in range(result.width):
            incoming_pixel = incoming_pixels[x, y]
            outgoing_pixel = outgoing_pixels[x, y]
            if incoming_pixel[3] == 0:
                result_pixels[x, y] = outgoing_pixel
                continue
            if outgoing_pixel[3] == 0:
                result_pixels[x, y] = incoming_pixel
                continue

            projection = (
                ((x + 0.5) - pivot[0]) * transition_direction[0]
                + ((y + 0.5) - pivot[1]) * transition_direction[1]
            )
            outgoing_weight = min(
                1.0,
                max(0.0, (projection + blend_extent) / (2.0 * blend_extent)),
            )
            outgoing_weight = outgoing_weight * outgoing_weight * (
                3.0 - (2.0 * outgoing_weight)
            )
            incoming_weight = (1.0 - outgoing_weight) * incoming_pixel[3]
            outgoing_weight *= outgoing_pixel[3]
            weight_sum = incoming_weight + outgoing_weight
            result_pixels[x, y] = (
                round(
                    (
                        incoming_pixel[0] * incoming_weight
                        + outgoing_pixel[0] * outgoing_weight
                    )
                    / weight_sum
                ),
                round(
                    (
                        incoming_pixel[1] * incoming_weight
                        + outgoing_pixel[1] * outgoing_weight
                    )
                    / weight_sum
                ),
                round(
                    (
                        incoming_pixel[2] * incoming_weight
                        + outgoing_pixel[2] * outgoing_weight
                    )
                    / weight_sum
                ),
                max(incoming_pixel[3], outgoing_pixel[3]),
            )

    return result


def opening_junction_curve_points(
    pivot: tuple[float, float], scale: float
) -> list[tuple[float, float]]:
    """Return a tangent-continuous centerline through the exact opening turn."""

    incoming_direction = (0.0, 1.0)
    outgoing_angle = math.radians(OPENING_JUNCTION_OUTGOING_HEADING_DEGREES)
    outgoing_direction = (math.cos(outgoing_angle), math.sin(outgoing_angle))
    turn_radians = math.radians(abs(OPENING_JUNCTION_TURN_DEGREES))
    tangent_distance = OPENING_JUNCTION_CURVE_TANGENT_DISTANCE * scale
    curve_radius = tangent_distance / math.tan(turn_radians / 2.0)
    control_distance = (
        (4.0 / 3.0) * math.tan(turn_radians / 4.0) * curve_radius
    )
    start = (
        pivot[0] - (incoming_direction[0] * tangent_distance),
        pivot[1] - (incoming_direction[1] * tangent_distance),
    )
    end = (
        pivot[0] + (outgoing_direction[0] * tangent_distance),
        pivot[1] + (outgoing_direction[1] * tangent_distance),
    )
    first_control = (
        start[0] + (incoming_direction[0] * control_distance),
        start[1] + (incoming_direction[1] * control_distance),
    )
    second_control = (
        end[0] - (outgoing_direction[0] * control_distance),
        end[1] - (outgoing_direction[1] * control_distance),
    )

    points = []
    for step in range(65):
        parameter = step / 64.0
        inverse = 1.0 - parameter
        points.append(
            (
                (inverse**3 * start[0])
                + (3.0 * inverse * inverse * parameter * first_control[0])
                + (3.0 * inverse * parameter * parameter * second_control[0])
                + (parameter**3 * end[0]),
                (inverse**3 * start[1])
                + (3.0 * inverse * inverse * parameter * first_control[1])
                + (3.0 * inverse * parameter * parameter * second_control[1])
                + (parameter**3 * end[1]),
            )
        )
    return points


def sample_wrapped_path(path: Image.Image, x: float, y: float) -> tuple[int, int, int, int]:
    """Bilinearly sample a path strip with vertical repetition."""

    if x < 0.0 or x > path.width - 1:
        return (0, 0, 0, 0)
    left = math.floor(x)
    right = min(path.width - 1, left + 1)
    top = math.floor(y) % path.height
    bottom = (top + 1) % path.height
    horizontal = x - left
    vertical = y - math.floor(y)
    pixels = path.load()
    samples = (
        (pixels[left, top], (1.0 - horizontal) * (1.0 - vertical)),
        (pixels[right, top], horizontal * (1.0 - vertical)),
        (pixels[left, bottom], (1.0 - horizontal) * vertical),
        (pixels[right, bottom], horizontal * vertical),
    )
    return tuple(
        round(sum(pixel[channel] * weight for pixel, weight in samples))
        for channel in range(4)
    )


def render_opening_junction_curve(
    path: Image.Image,
    canvas_size: tuple[int, int],
    curve_points: list[tuple[float, float]],
) -> tuple[Image.Image, Image.Image]:
    """Warp the repeatable path texture along the tangent-continuous turn."""

    arc_lengths = [0.0]
    for first, second in zip(curve_points, curve_points[1:]):
        arc_lengths.append(
            arc_lengths[-1] + math.hypot(second[0] - first[0], second[1] - first[1])
        )
    tangents = []
    for index, point in enumerate(curve_points):
        before = curve_points[max(0, index - 1)]
        after = curve_points[min(len(curve_points) - 1, index + 1)]
        tangent = (after[0] - before[0], after[1] - before[1])
        tangent_length = math.hypot(*tangent)
        tangents.append((tangent[0] / tangent_length, tangent[1] / tangent_length))

    radius = path.width / 2.0
    bounds = (
        max(0, math.floor(min(point[0] for point in curve_points) - radius - 1)),
        max(0, math.floor(min(point[1] for point in curve_points) - radius - 1)),
        min(
            canvas_size[0],
            math.ceil(max(point[0] for point in curve_points) + radius + 1),
        ),
        min(
            canvas_size[1],
            math.ceil(max(point[1] for point in curve_points) + radius + 1),
        ),
    )
    result = Image.new("RGBA", canvas_size, (0, 0, 0, 0))
    weights = Image.new("L", canvas_size, 0)
    result_pixels = result.load()
    weight_pixels = weights.load()
    phase = curve_points[0][1]
    last_index = len(curve_points) - 1

    for y in range(bounds[1], bounds[3]):
        for x in range(bounds[0], bounds[2]):
            sample = (x + 0.5, y + 0.5)
            nearest = min(
                range(len(curve_points)),
                key=lambda index: (
                    (sample[0] - curve_points[index][0]) ** 2
                    + (sample[1] - curve_points[index][1]) ** 2
                ),
            )
            point = curve_points[nearest]
            tangent = tangents[nearest]
            right = (tangent[1], -tangent[0])
            lateral = (
                (sample[0] - point[0]) * right[0]
                + (sample[1] - point[1]) * right[1]
            )
            source_x = (path.width / 2.0) + lateral
            if source_x < 0.0 or source_x > path.width - 1:
                continue
            result_pixels[x, y] = sample_wrapped_path(
                path,
                source_x,
                phase + arc_lengths[nearest],
            )
            if (
                abs(lateral)
                <= radius
                - OPENING_JUNCTION_CURVE_EDGE_INSET
                * (path.width / OPENING_JUNCTION_PATH_WIDTH)
                and result_pixels[x, y][3] < 255
            ):
                interior_x = min(
                    path.width
                    - 1
                    - OPENING_JUNCTION_CURVE_EDGE_INSET
                    * (path.width / OPENING_JUNCTION_PATH_WIDTH),
                    max(
                        OPENING_JUNCTION_CURVE_EDGE_INSET
                        * (path.width / OPENING_JUNCTION_PATH_WIDTH),
                        source_x,
                    ),
                )
                interior = sample_wrapped_path(
                    path,
                    interior_x,
                    phase + arc_lengths[nearest],
                )
                result_pixels[x, y] = (*interior[:3], 255)
            parameter = nearest / last_index
            weight_pixels[x, y] = round(255.0 * math.sin(math.pi * parameter))

    return result, weights


def blend_rgba_with_mask(
    base: Image.Image, overlay: Image.Image, mask: Image.Image
) -> Image.Image:
    """Blend RGBA texture while retaining the strongest contributing alpha."""

    result = Image.new("RGBA", base.size, (0, 0, 0, 0))
    result_pixels = []
    for base_pixel, overlay_pixel, weight in zip(
        base.convert("RGBA").getdata(),
        overlay.convert("RGBA").getdata(),
        mask.convert("L").getdata(),
    ):
        if overlay_pixel[3] == 0 or weight == 0:
            result_pixels.append(base_pixel)
            continue
        if base_pixel[3] == 0:
            result_pixels.append(overlay_pixel)
            continue
        overlay_weight = weight * overlay_pixel[3]
        base_weight = (255 - weight) * base_pixel[3]
        weight_sum = base_weight + overlay_weight
        result_pixels.append(
            (
                round(
                    (base_pixel[0] * base_weight + overlay_pixel[0] * overlay_weight)
                    / weight_sum
                ),
                round(
                    (base_pixel[1] * base_weight + overlay_pixel[1] * overlay_weight)
                    / weight_sum
                ),
                round(
                    (base_pixel[2] * base_weight + overlay_pixel[2] * overlay_weight)
                    / weight_sum
                ),
                max(base_pixel[3], overlay_pixel[3]),
            )
        )
    result.putdata(result_pixels)
    return result


def remove_opening_junction_dark_fringe(
    junction: Image.Image, curve_points: list[tuple[float, float]], path_width: int
) -> Image.Image:
    """Replace opaque resampling fringe pixels without touching either exact port."""

    result = junction.copy().convert("RGBA")
    source = result.copy()
    result_pixels = result.load()
    source_pixels = source.load()
    radius = path_width / 2.0
    bounds = (
        max(0, math.floor(min(point[0] for point in curve_points) - radius)),
        max(0, math.floor(min(point[1] for point in curve_points) - radius)),
        min(result.width, math.ceil(max(point[0] for point in curve_points) + radius)),
        min(result.height, math.ceil(max(point[1] for point in curve_points) + radius)),
    )
    for y in range(bounds[1], bounds[3]):
        for x in range(bounds[0], bounds[2]):
            pixel = source_pixels[x, y]
            if pixel[3] < 240 or sum(pixel[:3]) >= 150:
                continue
            replacement = None
            for search_radius in range(1, 13):
                candidates = []
                for candidate_y in range(
                    max(bounds[1], y - search_radius),
                    min(bounds[3], y + search_radius + 1),
                ):
                    for candidate_x in range(
                        max(bounds[0], x - search_radius),
                        min(bounds[2], x + search_radius + 1),
                    ):
                        if (
                            abs(candidate_x - x) != search_radius
                            and abs(candidate_y - y) != search_radius
                        ):
                            continue
                        candidate = source_pixels[candidate_x, candidate_y]
                        if candidate[3] >= 240 and sum(candidate[:3]) >= 300:
                            candidates.append(candidate)
                if candidates:
                    replacement = candidates[0]
                    break
            if replacement is not None:
                result_pixels[x, y] = (*replacement[:3], pixel[3])
    return result


def make_opening_junction(path: Image.Image) -> Image.Image:
    """Create the exact Stream-Laundry-Woodland junction at the path's scale."""

    scale = path.width / OPENING_JUNCTION_PATH_WIDTH
    canvas_size = round(OPENING_JUNCTION_CANVAS_SIZE * scale)
    pivot = tuple(coordinate * scale for coordinate in OPENING_JUNCTION_PIVOT)
    overlap = (OPENING_JUNCTION_PATH_WIDTH / 2.0) * scale
    incoming = render_path_leg(
        path,
        (canvas_size, canvas_size),
        (pivot[0], 0.0),
        90.0,
        pivot[1] + overlap,
    )
    outgoing_angle = math.radians(OPENING_JUNCTION_OUTGOING_HEADING_DEGREES)
    outgoing_direction = (math.cos(outgoing_angle), math.sin(outgoing_angle))
    outgoing_start = (
        pivot[0] - (outgoing_direction[0] * overlap),
        pivot[1] - (outgoing_direction[1] * overlap),
    )
    outgoing = render_path_leg(
        path,
        (canvas_size, canvas_size),
        outgoing_start,
        OPENING_JUNCTION_OUTGOING_HEADING_DEGREES,
        canvas_size * 1.5,
    )
    junction = blend_opening_junction_legs(incoming, outgoing, pivot, path.width)
    outgoing_end = (
        pivot[0] + (outgoing_direction[0] * canvas_size * 1.5),
        pivot[1] + (outgoing_direction[1] * canvas_size * 1.5),
    )
    curve_points = opening_junction_curve_points(pivot, scale)
    curved_route = Image.new("L", junction.size, 0)
    curved_draw = ImageDraw.Draw(curved_route)
    curved_draw.line(
        [(pivot[0], -path.height), curve_points[0]],
        fill=255,
        width=path.width,
    )
    curved_draw.line(curve_points, fill=255, width=path.width, joint="curve")
    curve_radius = path.width / 2.0
    for point in curve_points:
        curved_draw.ellipse(
            (
                point[0] - curve_radius,
                point[1] - curve_radius,
                point[0] + curve_radius,
                point[1] + curve_radius,
            ),
            fill=255,
        )
    curved_draw.line(
        [curve_points[-1], outgoing_end],
        fill=255,
        width=path.width,
    )
    curve_texture, curve_weights = render_opening_junction_curve(
        path, junction.size, curve_points
    )
    junction = blend_rgba_with_mask(junction, curve_texture, curve_weights)
    junction.putalpha(ImageChops.multiply(junction.getchannel("A"), curved_route))
    junction = alpha_clean(junction, cutoff=0)
    junction = remove_opening_junction_dark_fringe(
        junction, curve_points, path.width
    )
    port_depth = path.height // 8
    port_offset = round(pivot[0] - (path.width / 2.0))
    junction.paste(path.crop((0, 0, path.width, port_depth)), (port_offset, 0))
    top_edge = Image.new("RGBA", (canvas_size, 1), (0, 0, 0, 0))
    top_edge.paste(path.crop((0, 0, path.width, 1)), (port_offset, 0))
    junction.paste(top_edge, (0, 0))
    return junction


def blend_path_legs(legs: list[Image.Image]) -> Image.Image:
    """Average overlapping path texture without accumulating alpha or dark seams."""

    if not legs or any(leg.size != legs[0].size for leg in legs):
        raise ValueError("path legs must share one non-empty canvas")
    result = Image.new("RGBA", legs[0].size, (0, 0, 0, 0))
    leg_pixels = [leg.convert("RGBA").load() for leg in legs]
    result_pixels = result.load()
    for y in range(result.height):
        for x in range(result.width):
            visible = [pixels[x, y] for pixels in leg_pixels if pixels[x, y][3] > 0]
            if not visible:
                continue
            weight = sum(pixel[3] for pixel in visible)
            result_pixels[x, y] = (
                round(sum(pixel[0] * pixel[3] for pixel in visible) / weight),
                round(sum(pixel[1] * pixel[3] for pixel in visible) / weight),
                round(sum(pixel[2] * pixel[3] for pixel in visible) / weight),
                max(pixel[3] for pixel in visible),
            )
    return result


def visible_rgb_mean(image: Image.Image) -> tuple[float, float, float]:
    """Return alpha-weighted mean RGB for one non-empty transparent texture."""

    pixels = [pixel for pixel in image.convert("RGBA").getdata() if pixel[3] > 0]
    if not pixels:
        raise ValueError("texture source contains no visible pixels")
    weight = sum(pixel[3] for pixel in pixels)
    return tuple(
        sum(pixel[channel] * pixel[3] for pixel in pixels) / weight
        for channel in range(3)
    )


def blend_generated_junction_center(
    junction: Image.Image,
    generated_source: Image.Image,
    path: Image.Image,
    pivot: tuple[float, float],
    blend_radius: float,
) -> Image.Image:
    """Use the generated painterly center while retaining deterministic path alpha."""

    generated = ImageOps.fit(
        alpha_clean(generated_source, cutoff=24),
        junction.size,
        method=RESAMPLE,
        centering=(0.5, 0.5),
    )
    generated_mean = visible_rgb_mean(generated)
    path_mean = visible_rgb_mean(path)
    channel_offsets = tuple(
        path_mean[channel] - generated_mean[channel] for channel in range(3)
    )
    result = junction.copy().convert("RGBA")
    result_pixels = result.load()
    generated_pixels = generated.load()
    for y in range(result.height):
        for x in range(result.width):
            base = result_pixels[x, y]
            source = generated_pixels[x, y]
            if base[3] == 0 or source[3] == 0:
                continue
            distance = math.hypot((x + 0.5) - pivot[0], (y + 0.5) - pivot[1])
            progress = max(0.0, min(1.0, 1.0 - (distance / blend_radius)))
            weight = 0.45 * progress * progress * (3.0 - (2.0 * progress))
            if weight <= 0.0:
                continue
            normalized = tuple(
                max(0, min(255, round(source[channel] + channel_offsets[channel])))
                for channel in range(3)
            )
            result_pixels[x, y] = (
                *(round(base[channel] * (1.0 - weight) + normalized[channel] * weight)
                  for channel in range(3)),
                base[3],
            )
    return result


def make_village_square_junction(
    path: Image.Image, generated_source: Image.Image
) -> Image.Image:
    """Create one four-port junction matching the exact live Village Square headings."""

    scale = path.width / VILLAGE_SQUARE_JUNCTION_PATH_WIDTH
    canvas_size = round(VILLAGE_SQUARE_JUNCTION_CANVAS_SIZE * scale)
    pivot = tuple(coordinate * scale for coordinate in VILLAGE_SQUARE_JUNCTION_PIVOT)
    headings = VILLAGE_SQUARE_JUNCTION_HEADINGS
    legs = [
        render_path_leg(
            path,
            (canvas_size, canvas_size),
            pivot,
            heading_degrees,
            canvas_size * 1.5,
        )
        for heading_degrees in headings
    ]
    junction = blend_path_legs(legs)
    junction = blend_generated_junction_center(
        junction,
        generated_source,
        path,
        pivot,
        VILLAGE_SQUARE_JUNCTION_CENTER_BLEND_RADIUS * scale,
    )
    return alpha_clean(junction, cutoff=0)


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
    """Scale the retained geometry-only cues onto the exact 192x192 table canvas."""

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
    return {
        name: alpha_clean(cue.resize(TABLE_MASTER_SIZE, RESAMPLE), cutoff=8)
        for name, cue in {
            "noticeable": noticeable,
            "identified": identified,
            "dangerous": dangerous,
        }.items()
    }


def seeker_frame_sets() -> dict[str, list[Image.Image]]:
    """Extract and normalize the four approved six-frame direction rows."""

    directions = [
        ("south", 0, SEEKER_MASTER_DIRECTION_WIDTHS["south"]),
        ("west", 1, SEEKER_MASTER_DIRECTION_WIDTHS["west"]),
        ("east", 2, SEEKER_MASTER_DIRECTION_WIDTHS["east"]),
        ("north", 3, SEEKER_MASTER_DIRECTION_WIDTHS["north"]),
    ]
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
            (canvas_width - 24) / max(cell.width for cell in cells),
            SEEKER_MASTER_CONTENT_HEIGHT / max(cell.height for cell in cells),
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
            frame = Image.new(
                "RGBA", (canvas_width, SEEKER_MASTER_CANVAS_HEIGHT), (0, 0, 0, 0)
            )
            frame.alpha_composite(
                resized,
                (
                    (canvas_width - resized.width) // 2,
                    SEEKER_MASTER_BASELINE - resized.height,
                ),
            )
            frames.append(frame)
        frame_sets[direction] = frames
    return frame_sets


def seeker_idle_frame_sets() -> dict[str, list[Image.Image]]:
    """Normalize each generated direction's neutral and inhale proof frames."""

    frame_sets = {}
    for direction, canvas_width in SEEKER_MASTER_DIRECTION_WIDTHS.items():
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
            (canvas_width - 32) / max(cell.width for cell in cells),
            SEEKER_MASTER_IDLE_CONTENT_HEIGHT / max(cell.height for cell in cells),
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
            frame = Image.new(
                "RGBA", (canvas_width, SEEKER_MASTER_CANVAS_HEIGHT), (0, 0, 0, 0)
            )
            frame.alpha_composite(
                resized,
                (
                    (canvas_width - resized.width) // 2,
                    SEEKER_MASTER_BASELINE - resized.height,
                ),
            )
            frames.append(frame)
        frame_sets[direction] = frames
    return frame_sets


def seeker_interaction_frame_sets() -> dict[str, list[Image.Image]]:
    """Normalize each generated direction's four-frame reach-and-inspect proof."""

    frame_sets = {}
    for direction in SEEKER_MASTER_DIRECTION_WIDTHS:
        canvas_width = SEEKER_MASTER_INTERACTION_WIDTH
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
            252 / max(cell.width for cell in cells),
            SEEKER_MASTER_CONTENT_HEIGHT / max(cell.height for cell in cells),
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
            frame = Image.new(
                "RGBA", (canvas_width, SEEKER_MASTER_CANVAS_HEIGHT), (0, 0, 0, 0)
            )
            foot_band_top = max(0, resized.height - 60)
            foot_bbox = resized.getchannel("A").crop(
                (0, foot_band_top, resized.width, resized.height)
            ).getbbox()
            if foot_bbox is None:
                raise ValueError(f"{direction} interaction frame has no visible feet")
            anchored_x = round((canvas_width - foot_bbox[0] - foot_bbox[2]) / 2)
            anchored_x = max(0, min(anchored_x, canvas_width - resized.width))
            frame.alpha_composite(
                resized,
                (anchored_x, SEEKER_MASTER_BASELINE - resized.height),
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
    """Recompose one 1280x720 review scene exclusively from corrected exports."""

    scene_size = LOGICAL_VIEWPORT_SIZE
    ground = assets["terrain.quiet_moss"]
    ground_layer = Image.new("RGBA", scene_size, (0, 0, 0, 0))
    for y in range(0, scene_size[1], ground.height):
        for x in range(0, scene_size[0], ground.width):
            ground_layer.alpha_composite(ground, (x, y))

    path = assets["terrain.straight_path"]
    path_layer = Image.new("RGBA", scene_size, (0, 0, 0, 0))
    path_x = REVIEW_PATH_X
    for y in range(0, scene_size[1], path.height):
        path_layer.alpha_composite(path, (path_x, y))

    props_layer = Image.new("RGBA", scene_size, (0, 0, 0, 0))
    bed = assets["plants.herb_bed_empty"]
    foliage = assets["plants.foliage_broadleaf"]
    table = assets["objects.preparation_table"]
    props_layer.alpha_composite(bed, REVIEW_POSITIONS["left_bed"])
    props_layer.alpha_composite(bed, REVIEW_POSITIONS["right_bed"])
    props_layer.alpha_composite(foliage, REVIEW_POSITIONS["right_bed_foliage"])
    props_layer.alpha_composite(foliage, REVIEW_POSITIONS["left_foliage"])
    props_layer.alpha_composite(foliage, REVIEW_POSITIONS["right_foliage"])
    props_layer.alpha_composite(table, REVIEW_POSITIONS["preparation_table"])

    cue_layer = Image.new("RGBA", scene_size, (0, 0, 0, 0))
    cue_layer.alpha_composite(
        assets["cues.preparation_table_identified"],
        REVIEW_POSITIONS["preparation_table"],
    )

    character_layer = Image.new("RGBA", scene_size, (0, 0, 0, 0))
    character_layer.alpha_composite(
        assets["characters.seeker.south.walk_01"], REVIEW_POSITIONS["seeker"]
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


def make_opening_junction_review(assets: dict[str, Image.Image]) -> Image.Image:
    """Show exact live-angle continuations and the module's route-band containment."""

    panel_size = (768, 768)
    scene = Image.new("RGBA", panel_size, (0, 0, 0, 0))
    ground = assets["terrain.quiet_moss"]
    for y in range(0, scene.height, ground.height):
        for x in range(0, scene.width, ground.width):
            scene.alpha_composite(ground, (x, y))

    straight = assets["terrain.straight_path"]
    module = assets["terrain.opening_junction"]
    module_position = (128, 128)
    pivot = (
        module_position[0] + OPENING_JUNCTION_PIVOT[0],
        module_position[1] + OPENING_JUNCTION_PIVOT[1],
    )
    overlap = OPENING_JUNCTION_PATH_WIDTH / 2.0
    incoming_start = (pivot[0], module_position[1] - straight.height)
    incoming = render_path_leg(
        straight,
        panel_size,
        incoming_start,
        90.0,
        pivot[1] - incoming_start[1],
    )
    outgoing_angle = math.radians(OPENING_JUNCTION_OUTGOING_HEADING_DEGREES)
    outgoing_direction = (math.cos(outgoing_angle), math.sin(outgoing_angle))
    outgoing_start = (
        pivot[0] - (outgoing_direction[0] * overlap),
        pivot[1] - (outgoing_direction[1] * overlap),
    )
    outgoing = render_path_leg(
        straight,
        panel_size,
        outgoing_start,
        OPENING_JUNCTION_OUTGOING_HEADING_DEGREES,
        768.0,
    )
    continuations = Image.alpha_composite(incoming, outgoing)
    continuation_draw = ImageDraw.Draw(continuations)
    continuation_draw.rectangle(
        (
            module_position[0],
            module_position[1],
            module_position[0] + module.width - 1,
            module_position[1] + module.height - 1,
        ),
        fill=(0, 0, 0, 0),
    )
    scene.alpha_composite(continuations)
    scene.alpha_composite(module, module_position)

    containment = Image.new("RGBA", panel_size, (30, 35, 31, 255))
    route_mask = Image.new("L", panel_size, 0)
    route_draw = ImageDraw.Draw(route_mask)
    curve_points = opening_junction_curve_points(pivot, 1.0)
    route_draw.line(
        [(pivot[0], 0), curve_points[0]],
        fill=180,
        width=OPENING_JUNCTION_PATH_WIDTH,
    )
    outgoing_end = (
        pivot[0] + (outgoing_direction[0] * 768.0),
        pivot[1] + (outgoing_direction[1] * 768.0),
    )
    route_draw.line(
        curve_points,
        fill=180,
        width=OPENING_JUNCTION_PATH_WIDTH,
        joint="curve",
    )
    for point in curve_points:
        route_draw.ellipse(
            (
                point[0] - overlap,
                point[1] - overlap,
                point[0] + overlap,
                point[1] + overlap,
            ),
            fill=180,
        )
    route_draw.line(
        [curve_points[-1], outgoing_end],
        fill=180,
        width=OPENING_JUNCTION_PATH_WIDTH,
    )
    route_tint = Image.new("RGBA", panel_size, (70, 139, 88, 0))
    route_tint.putalpha(route_mask)
    containment.alpha_composite(route_tint)
    module_layer = Image.new("RGBA", panel_size, (0, 0, 0, 0))
    module_layer.alpha_composite(module, module_position)
    module_alpha = module_layer.getchannel("A")
    module_tint = Image.new("RGBA", panel_size, (245, 215, 132, 0))
    module_tint.putalpha(module_alpha)
    containment.alpha_composite(module_tint)
    overflow = Image.new("L", panel_size, 0)
    overflow.putdata(
        [
            alpha if alpha > 0 and route == 0 else 0
            for alpha, route in zip(module_alpha.getdata(), route_mask.getdata())
        ]
    )
    overflow_tint = Image.new("RGBA", panel_size, (220, 60, 50, 0))
    overflow_tint.putalpha(overflow)
    containment.alpha_composite(overflow_tint)

    review = Image.new("RGBA", (1536, 768), (0, 0, 0, 0))
    review.alpha_composite(scene, (0, 0))
    review.alpha_composite(containment, (768, 0))
    return review


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


def make_village_edge_transition_review(
    assets: dict[str, Image.Image],
) -> Image.Image:
    """Show the exact feather overlay over quiet moss beside its alpha coverage."""

    moss = assets["terrain.quiet_moss"]
    overlay = assets["terrain.woodland_village_edge_transition"]
    composite = Image.new("RGBA", overlay.size, (0, 0, 0, 0))
    for y in range(0, composite.height, moss.height):
        for x in range(0, composite.width, moss.width):
            composite.alpha_composite(moss, (x, y))
    composite.alpha_composite(overlay)
    alpha = overlay.getchannel("A")
    alpha_view = Image.merge("RGBA", (alpha, alpha, alpha, Image.new("L", alpha.size, 255)))
    review = Image.new("RGBA", (overlay.width * 2, overlay.height), (0, 0, 0, 255))
    review.alpha_composite(composite, (0, 0))
    review.alpha_composite(alpha_view, (overlay.width, 0))
    return review


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
    village_edge_transition_master, village_edge_transition_mask_master = (
        make_village_edge_transition(leaf_litter_master)
    )
    path_master = make_path()
    path_bend_master = make_path_bend(path_master)
    path_t_junction_master = make_path_t_junction(path_master)
    path_cross_junction_master = make_path_cross_junction(path_master)
    path_end_cap_master = make_path_end_cap(path_master)
    opening_junction_master = make_opening_junction(path_master)
    village_square_generated_source = Image.open(
        RAW / "village-square-junction-source.png"
    ).convert("RGBA")
    village_square_junction_master = make_village_square_junction(
        path_master,
        village_square_generated_source,
    )
    bed_master = contain_on_canvas(
        Image.open(RAW / "herb-bed-empty.png"), (192, 160), (180, 148), 154
    )
    foliage_master = contain_on_canvas(
        Image.open(RAW / "foliage-broadleaf.png"), (144, 112), (136, 100), 106
    )
    table_master = contain_on_canvas(
        Image.open(RAW / "preparation-table.png"),
        TABLE_MASTER_SIZE,
        TABLE_MASTER_CONTENT_SIZE,
        TABLE_MASTER_BASELINE,
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
            "terrain.woodland_village_edge_transition",
            "terrain",
            Path("exports/terrain/woodland-village-edge-transition.png"),
            (2048, 1024),
            (1184, 628),
            1024,
            village_edge_transition_master,
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
            "terrain.opening_junction",
            "terrain",
            Path("exports/terrain/opening-junction.png"),
            (1024, 1024),
            (448, 448),
            1024,
            opening_junction_master,
            "tools/build.py",
        ),
        Asset(
            "terrain.village_square_junction",
            "terrain",
            Path("exports/terrain/village-square-junction.png"),
            (1024, 1024),
            (512, 512),
            1024,
            village_square_junction_master,
            "sources/imagegen/village-square-junction-source.png",
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
            TABLE_MASTER_SIZE,
            TABLE_MASTER_PIVOT,
            TABLE_MASTER_BASELINE,
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
                TABLE_MASTER_SIZE,
                TABLE_MASTER_PIVOT,
                TABLE_MASTER_BASELINE,
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
                    (frame.width // 2, SEEKER_MASTER_BASELINE),
                    SEEKER_MASTER_BASELINE,
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
                    (frame.width // 2, SEEKER_MASTER_BASELINE),
                    SEEKER_MASTER_BASELINE,
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
                    (frame.width // 2, SEEKER_MASTER_BASELINE),
                    SEEKER_MASTER_BASELINE,
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
        elif asset.asset_id == "terrain.opening_junction":
            export = make_opening_junction(export_images["terrain.straight_path"])
        elif asset.asset_id == "terrain.village_square_junction":
            export = make_village_square_junction(
                export_images["terrain.straight_path"],
                village_square_generated_source,
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
        MASTERS / "terrain-woodland-village-edge-transition.ora",
        village_edge_transition_master.size,
        [
            ("woodland leaf-litter source", leaf_litter_master, 0, 0, False),
            ("feather coverage mask", village_edge_transition_mask_master, 0, 0, False),
            (
                "world-aligned Village Edge feather overlay",
                village_edge_transition_master,
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
        MASTERS / "terrain-opening-junction.ora",
        opening_junction_master.size,
        [
            ("straight packed-earth source", path_master, 0, 0, False),
            (
                "Stream-Laundry-Woodland junction",
                opening_junction_master,
                0,
                0,
                True,
            ),
        ],
    )
    create_ora(
        MASTERS / "terrain-village-square-junction.ora",
        village_square_junction_master.size,
        [
            (
                "generated painterly source",
                ImageOps.fit(
                    alpha_clean(village_square_generated_source, cutoff=24),
                    village_square_junction_master.size,
                    method=RESAMPLE,
                    centering=(0.5, 0.5),
                ),
                0,
                0,
                False,
            ),
            ("straight packed-earth source", path_master, 0, 0, False),
            (
                "Village Square four-route junction",
                village_square_junction_master,
                0,
                0,
                True,
            ),
        ],
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
            x = (index - 1) * SEEKER_MASTER_REVIEW_CELL_WIDTH + (
                SEEKER_MASTER_REVIEW_CELL_WIDTH - frame.width
            ) // 2
            seeker_layers.append(
                (
                    f"{direction} walk {index:02d}",
                    frame,
                    x,
                    row * SEEKER_MASTER_CANVAS_HEIGHT,
                    True,
                )
            )
    create_ora(
        MASTERS / "seeker-four-direction-walk.ora",
        (
            6 * SEEKER_MASTER_REVIEW_CELL_WIDTH,
            4 * SEEKER_MASTER_CANVAS_HEIGHT,
        ),
        seeker_layers,
    )
    seeker_review = Image.new(
        "RGBA",
        (
            6 * SEEKER_MASTER_REVIEW_CELL_WIDTH,
            4 * SEEKER_MASTER_CANVAS_HEIGHT,
        ),
        (0, 0, 0, 0),
    )
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
            x = (index - 1) * SEEKER_MASTER_REVIEW_CELL_WIDTH + (
                SEEKER_MASTER_REVIEW_CELL_WIDTH - frame.width
            ) // 2
            idle_layers.append(
                (
                    f"{direction} idle breathe {index:02d}",
                    frame,
                    x,
                    row * SEEKER_MASTER_CANVAS_HEIGHT,
                    True,
                )
            )
    create_ora(
        MASTERS / "seeker-four-direction-idle-breathe.ora",
        (
            2 * SEEKER_MASTER_REVIEW_CELL_WIDTH,
            4 * SEEKER_MASTER_CANVAS_HEIGHT,
        ),
        idle_layers,
    )
    idle_review = Image.new(
        "RGBA",
        (
            2 * SEEKER_MASTER_REVIEW_CELL_WIDTH,
            4 * SEEKER_MASTER_CANVAS_HEIGHT,
        ),
        (0, 0, 0, 0),
    )
    for _, frame, x, y, _ in idle_layers:
        idle_review.alpha_composite(frame, (x, y))
    save_png(downsample(idle_review), REVIEW / "seeker-four-direction-idle-breathe.png")

    obsolete_interaction_master = MASTERS / "seeker-south-interact-reach.ora"
    if obsolete_interaction_master.exists():
        obsolete_interaction_master.unlink()
    obsolete_interaction_review = REVIEW / "seeker-south-interact-reach.png"
    if obsolete_interaction_review.exists():
        obsolete_interaction_review.unlink()
    interaction_cell_width = SEEKER_MASTER_INTERACTION_WIDTH
    interaction_layers = []
    for row, direction in enumerate(direction_order):
        for index, frame in enumerate(interaction_frame_sets[direction], start=1):
            x = (index - 1) * interaction_cell_width + (
                interaction_cell_width - frame.width
            ) // 2
            interaction_layers.append(
                (
                    f"{direction} interact reach {index:02d}",
                    frame,
                    x,
                    row * SEEKER_MASTER_CANVAS_HEIGHT,
                    True,
                )
            )
    create_ora(
        MASTERS / "seeker-four-direction-interact-reach.ora",
        (4 * interaction_cell_width, 4 * SEEKER_MASTER_CANVAS_HEIGHT),
        interaction_layers,
    )
    interaction_review = Image.new(
        "RGBA",
        (4 * interaction_cell_width, 4 * SEEKER_MASTER_CANVAS_HEIGHT),
        (0, 0, 0, 0),
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
        make_opening_junction_review(export_images),
        REVIEW / "opening-junction-review.png",
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
    save_png(
        make_village_edge_transition_review(export_images),
        REVIEW / "woodland-village-edge-transition-review.png",
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

    opening_junction_bounds = (
        export_images["terrain.opening_junction"].getchannel("A").getbbox()
    )
    if opening_junction_bounds is None:
        raise ValueError("opening junction export unexpectedly has no visible footprint")
    village_square_junction_bounds = (
        export_images["terrain.village_square_junction"].getchannel("A").getbbox()
    )
    if village_square_junction_bounds is None:
        raise ValueError("Village Square junction export has no visible footprint")
    manifest = {
        "schema_version": 2,
        "status": "scale-rebaselined production-pipeline proof with isolated Godot import parity",
        "logical_viewport": {
            "width": LOGICAL_VIEWPORT_SIZE[0],
            "height": LOGICAL_VIEWPORT_SIZE[1],
        },
        "fullscreen_reference_logical_viewport": {
            "width": FULLSCREEN_REFERENCE_LOGICAL_SIZE[0],
            "height": FULLSCREEN_REFERENCE_LOGICAL_SIZE[1],
        },
        "minimum_visible_character_height": SEEKER_MINIMUM_VISIBLE_HEIGHT,
        "master_scale": MASTER_SCALE,
        "opening_junction_proof": {
            "asset": "terrain.opening_junction",
            "anchor_ids": ["stream", "abandoned-laundry", "woodland-clearing"],
            "path_width_pixels": OPENING_JUNCTION_PATH_WIDTH,
            "port_depth_pixels": OPENING_JUNCTION_PORT_DEPTH,
            "turn_degrees": OPENING_JUNCTION_TURN_DEGREES,
            "canonical_incoming_heading_degrees": 90.0,
            "canonical_outgoing_heading_degrees": (
                OPENING_JUNCTION_OUTGOING_HEADING_DEGREES
            ),
            "curve_tangent_distance_pixels": (
                OPENING_JUNCTION_CURVE_TANGENT_DISTANCE
            ),
            "texture_transition": "alpha_weighted_tangent_blend",
            "pivot": {"x": OPENING_JUNCTION_PIVOT[0], "y": OPENING_JUNCTION_PIVOT[1]},
            "visible_bounds": {
                "x": opening_junction_bounds[0],
                "y": opening_junction_bounds[1],
                "width": opening_junction_bounds[2] - opening_junction_bounds[0],
                "height": opening_junction_bounds[3] - opening_junction_bounds[1],
            },
            "review": "review/opening-junction-review.png",
        },
        "village_square_junction_proof": {
            "asset": "terrain.village_square_junction",
            "anchor_ids": [
                "village-square",
                "sickhouse",
                "healer-garden",
                "southern-road",
                "village-edge",
            ],
            "path_width_pixels": VILLAGE_SQUARE_JUNCTION_PATH_WIDTH,
            "port_depth_pixels": VILLAGE_SQUARE_JUNCTION_PORT_DEPTH,
            "headings_degrees": list(VILLAGE_SQUARE_JUNCTION_HEADINGS),
            "pivot": {
                "x": VILLAGE_SQUARE_JUNCTION_PIVOT[0],
                "y": VILLAGE_SQUARE_JUNCTION_PIVOT[1],
            },
            "visible_bounds": {
                "x": village_square_junction_bounds[0],
                "y": village_square_junction_bounds[1],
                "width": (
                    village_square_junction_bounds[2]
                    - village_square_junction_bounds[0]
                ),
                "height": (
                    village_square_junction_bounds[3]
                    - village_square_junction_bounds[1]
                ),
            },
            "reviews": [
                f"review/village-square-junction-{zoom:.1f}x-{view}.png"
                for zoom in (0.8, 1.0, 2.0)
                for view in ("wide", "tall")
            ],
        },
        "village_edge_transition_proof": {
            "asset": "terrain.woodland_village_edge_transition",
            "opening": {
                "x": VILLAGE_EDGE_TRANSITION_OPENING[0],
                "y": VILLAGE_EDGE_TRANSITION_OPENING[1],
            },
            "world_origin": {
                "x": VILLAGE_EDGE_TRANSITION_WORLD_ORIGIN[0],
                "y": VILLAGE_EDGE_TRANSITION_WORLD_ORIGIN[1],
            },
            "clearing_bounds": {
                "left": VILLAGE_EDGE_TRANSITION_CLEARING_BOUNDS[0],
                "top": VILLAGE_EDGE_TRANSITION_CLEARING_BOUNDS[1],
                "right": VILLAGE_EDGE_TRANSITION_CLEARING_BOUNDS[2],
                "bottom": VILLAGE_EDGE_TRANSITION_CLEARING_BOUNDS[3],
            },
            "path_width_pixels": VILLAGE_EDGE_TRANSITION_PATH_WIDTH,
            "feather_pixels": VILLAGE_EDGE_TRANSITION_FEATHER,
            "route_length_pixels": VILLAGE_EDGE_TRANSITION_ROUTE_LENGTH,
            "review": "review/woodland-village-edge-transition-review.png",
        },
        "navigation_clearance": {
            "scene": "review/module-review-scene.png",
            "x": REVIEW_PATH_X,
            "y": 0,
            "width": 96,
            "height": LOGICAL_VIEWPORT_SIZE[1],
        },
        "review_composition": [
            {
                "asset": "plants.herb_bed_empty",
                "x": REVIEW_POSITIONS["left_bed"][0],
                "y": REVIEW_POSITIONS["left_bed"][1],
                "fixed_obstacle": True,
            },
            {
                "asset": "plants.herb_bed_empty",
                "x": REVIEW_POSITIONS["right_bed"][0],
                "y": REVIEW_POSITIONS["right_bed"][1],
                "fixed_obstacle": True,
            },
            {
                "asset": "plants.foliage_broadleaf",
                "x": REVIEW_POSITIONS["right_bed_foliage"][0],
                "y": REVIEW_POSITIONS["right_bed_foliage"][1],
                "fixed_obstacle": True,
            },
            {
                "asset": "plants.foliage_broadleaf",
                "x": REVIEW_POSITIONS["left_foliage"][0],
                "y": REVIEW_POSITIONS["left_foliage"][1],
                "fixed_obstacle": True,
            },
            {
                "asset": "plants.foliage_broadleaf",
                "x": REVIEW_POSITIONS["right_foliage"][0],
                "y": REVIEW_POSITIONS["right_foliage"][1],
                "fixed_obstacle": True,
            },
            {
                "asset": "objects.preparation_table",
                "x": REVIEW_POSITIONS["preparation_table"][0],
                "y": REVIEW_POSITIONS["preparation_table"][1],
                "fixed_obstacle": True,
            },
            {
                "asset": "cues.preparation_table_identified",
                "x": REVIEW_POSITIONS["preparation_table"][0],
                "y": REVIEW_POSITIONS["preparation_table"][1],
                "fixed_obstacle": False,
            },
            {
                "asset": "characters.seeker.south.walk_01",
                "x": REVIEW_POSITIONS["seeker"][0],
                "y": REVIEW_POSITIONS["seeker"][1],
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
