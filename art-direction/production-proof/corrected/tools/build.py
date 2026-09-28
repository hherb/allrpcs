#!/usr/bin/env python3
"""Build the bounded Direction A corrected-pipeline proof deterministically."""

from __future__ import annotations

import hashlib
import json
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


def make_path() -> Image.Image:
    """Create the 192x512 2x straight-path overlay with repeatable vertical edges."""

    source = crop_alpha(alpha_clean(Image.open(RAW / "straight-path-overlay.png")))
    resized = ImageOps.fit(source, (192, 512), method=RESAMPLE, centering=(0.5, 0.5))
    resized = alpha_clean(resized, cutoff=24)
    return enforce_opposite_edges(resized, horizontal=False, vertical=True)


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
    path_master = make_path()
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

    export_images: dict[str, Image.Image] = {}
    for asset in assets:
        export = downsample(asset.image_master)
        if asset.category == "characters":
            export = align_alpha_baseline(export, asset.baseline_master // MASTER_SCALE)
        if asset.asset_id == "terrain.quiet_moss":
            export = enforce_opposite_edges(export, horizontal=True, vertical=True)
        elif asset.asset_id == "terrain.straight_path":
            export = alpha_clean(
                enforce_opposite_edges(export, horizontal=False, vertical=True),
                cutoff=8,
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

    scene, scene_layers = make_review_scene(export_images)
    save_png(scene, REVIEW / "module-review-scene.png")
    save_png(
        ImageOps.grayscale(scene).convert("RGBA"),
        REVIEW / "module-review-scene-grayscale.png",
    )
    create_ora(MASTERS / "module-review-scene.ora", scene.size, scene_layers)
    save_png(state_cue_check(export_images), REVIEW / "state-cues-grayscale.png")

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
                "godot_import_proof": ".idle_breathe_" not in asset.asset_id,
                "sha256": hash_file(export_path),
            }
        )

    manifest = {
        "schema_version": 1,
        "status": "external production-pipeline proof; not imported into Godot",
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
        ],
        "assets": manifest_assets,
    }
    (ROOT / "asset-manifest.json").write_text(
        json.dumps(manifest, indent=2) + "\n", encoding="utf-8"
    )


if __name__ == "__main__":
    main()
