#!/usr/bin/env python3
"""Build deterministic external colour-vision review evidence for terrain exports."""

from __future__ import annotations

import hashlib
import json
import math
from dataclasses import dataclass
from pathlib import Path
from typing import Iterable

from PIL import Image


ROOT = Path(__file__).resolve().parents[1]
MANIFEST = ROOT / "asset-manifest.json"
REVIEW_ROOT = ROOT / "review" / "terrain-accessibility"
VIEWS_ROOT = REVIEW_ROOT / "views"
SHEETS_ROOT = REVIEW_ROOT / "sheets"

VIEW_ORDER = ("original", "grayscale", "protanopia", "deuteranopia", "tritanopia")
GROUND_ASSET_IDS = (
    "terrain.quiet_moss",
    "terrain.woodland_leaf_litter",
    "terrain.woodland_leaf_litter_transition",
    "terrain.woodland_leaf_litter_corner_transition",
    "terrain.woodland_leaf_litter_corner_transition_left",
    "terrain.woodland_leaf_litter_inset_corner_transition",
    "terrain.woodland_leaf_litter_inset_corner_transition_left",
)
PATH_ASSET_ID = "terrain.straight_path"
SAMPLE_SIZE = (256, 256)
PATH_X = 80

LUMINANCE = (0.2126, 0.7152, 0.0722)
CVD_MATRICES = {
    "protanopia": (
        (0.152286, 1.052583, -0.204868),
        (0.114503, 0.786281, 0.099216),
        (-0.003882, -0.048116, 1.051998),
    ),
    "deuteranopia": (
        (0.367322, 0.860646, -0.227968),
        (0.280085, 0.672501, 0.047413),
        (-0.011820, 0.042940, 0.968881),
    ),
    "tritanopia": (
        (1.255528, -0.076749, -0.178779),
        (-0.078411, 0.930809, 0.147602),
        (0.004733, 0.691367, 0.303900),
    ),
}


@dataclass(frozen=True)
class ReviewSample:
    """One exact terrain sample and the pixels used for its measurements."""

    sample_id: str
    label: str
    input_asset_ids: tuple[str, ...]
    image: Image.Image
    measurement_mask: tuple[bool, ...]
    measurement_region: str


def require(condition: bool, message: str) -> None:
    """Raise one concise error when a review invariant fails."""

    if not condition:
        raise AssertionError(message)


def sha256(path: Path) -> str:
    """Return one file's lowercase SHA-256 digest."""

    digest = hashlib.sha256()
    with path.open("rb") as stream:
        for chunk in iter(lambda: stream.read(1024 * 1024), b""):
            digest.update(chunk)
    return digest.hexdigest()


def load_manifest() -> tuple[dict[str, object], dict[str, dict[str, object]]]:
    """Load the current manifest and bind every selected asset to its exact export."""

    manifest = json.loads(MANIFEST.read_text(encoding="utf-8"))
    assets = {asset["id"]: asset for asset in manifest["assets"]}
    for asset_id in (*GROUND_ASSET_IDS, PATH_ASSET_ID):
        require(asset_id in assets, f"manifest is missing review input {asset_id}")
        asset = assets[asset_id]
        path = ROOT / str(asset["export"])
        require(path.is_file(), f"review input is missing: {path}")
        require(
            sha256(path) == asset["sha256"],
            f"review input hash drifted: {asset_id}",
        )
    return manifest, assets


def load_rgba(asset: dict[str, object]) -> Image.Image:
    """Load one manifest asset as RGBA without changing its pixels."""

    return Image.open(ROOT / str(asset["export"])).convert("RGBA")


def build_samples(assets: dict[str, dict[str, object]]) -> list[ReviewSample]:
    """Compose the bounded terrain-family samples from exact manifest exports."""

    opaque_mask = (True,) * (SAMPLE_SIZE[0] * SAMPLE_SIZE[1])
    labels = {
        "terrain.quiet_moss": "Quiet moss",
        "terrain.woodland_leaf_litter": "Woodland leaf litter",
        "terrain.woodland_leaf_litter_transition": "Straight transition",
        "terrain.woodland_leaf_litter_corner_transition": "Outer corner right",
        "terrain.woodland_leaf_litter_corner_transition_left": "Outer corner left",
        "terrain.woodland_leaf_litter_inset_corner_transition": "Inset corner right",
        "terrain.woodland_leaf_litter_inset_corner_transition_left": "Inset corner left",
    }
    samples = []
    for asset_id in GROUND_ASSET_IDS:
        image = load_rgba(assets[asset_id])
        require(image.size == SAMPLE_SIZE, f"{asset_id}: expected a 256x256 export")
        require(
            image.getchannel("A").getextrema() == (255, 255),
            f"{asset_id}: accessibility sample must be fully opaque",
        )
        samples.append(
            ReviewSample(
                sample_id=asset_id.removeprefix("terrain."),
                label=labels[asset_id],
                input_asset_ids=(asset_id,),
                image=image,
                measurement_mask=opaque_mask,
                measurement_region="all 256x256 pixels",
            )
        )

    moss = load_rgba(assets["terrain.quiet_moss"])
    path = load_rgba(assets[PATH_ASSET_ID])
    require(path.size == (96, 256), "terrain.straight_path: expected a 96x256 export")
    packed_earth = moss.copy()
    packed_earth.alpha_composite(path, (PATH_X, 0))
    path_alpha = list(path.getchannel("A").getdata())
    path_mask = tuple(
        PATH_X <= x < PATH_X + path.width and path_alpha[y * path.width + x - PATH_X] > 0
        for y in range(SAMPLE_SIZE[1])
        for x in range(SAMPLE_SIZE[0])
    )
    require(any(path_mask), "terrain.straight_path: no visible packed-earth pixels")
    samples.insert(
        2,
        ReviewSample(
            sample_id="packed_earth_on_quiet_moss",
            label="Packed earth on quiet moss",
            input_asset_ids=("terrain.quiet_moss", PATH_ASSET_ID),
            image=packed_earth,
            measurement_mask=path_mask,
            measurement_region="visible straight-path pixels after exact alpha composition",
        ),
    )
    return samples


def srgb_to_linear(value: int) -> float:
    """Decode one 8-bit sRGB channel to linear light."""

    encoded = value / 255.0
    if encoded <= 0.04045:
        return encoded / 12.92
    return ((encoded + 0.055) / 1.055) ** 2.4


def linear_to_srgb(value: float) -> int:
    """Encode one clipped linear-light channel as an 8-bit sRGB channel."""

    linear = max(0.0, min(1.0, value))
    if linear <= 0.0031308:
        encoded = 12.92 * linear
    else:
        encoded = 1.055 * linear ** (1.0 / 2.4) - 0.055
    return round(encoded * 255)


def transform_pixel(
    pixel: tuple[int, int, int, int], view_name: str
) -> tuple[int, int, int, int]:
    """Transform one RGBA pixel in linear sRGB while preserving alpha."""

    if view_name == "original":
        return pixel
    red, green, blue, alpha = pixel
    linear = tuple(srgb_to_linear(channel) for channel in (red, green, blue))
    if view_name == "grayscale":
        output = (sum(weight * channel for weight, channel in zip(LUMINANCE, linear)),) * 3
    else:
        matrix = CVD_MATRICES[view_name]
        output = tuple(
            sum(weight * channel for weight, channel in zip(row, linear))
            for row in matrix
        )
    return (*[linear_to_srgb(channel) for channel in output], alpha)


def render_view(image: Image.Image, view_name: str) -> Image.Image:
    """Render one named original, grayscale, or full-severity CVD view."""

    require(view_name in VIEW_ORDER, f"unknown accessibility view: {view_name}")
    rgba = image.convert("RGBA")
    if view_name == "original":
        return rgba.copy()
    result = Image.new("RGBA", rgba.size)
    result.putdata([transform_pixel(pixel, view_name) for pixel in rgba.getdata()])
    return result


def pixel_luminances(image: Image.Image) -> list[float]:
    """Return linear relative luminance for every pixel in row-major order."""

    values = []
    for red, green, blue, _ in image.convert("RGBA").getdata():
        linear = tuple(srgb_to_linear(channel) for channel in (red, green, blue))
        values.append(sum(weight * channel for weight, channel in zip(LUMINANCE, linear)))
    return values


def percentile(sorted_values: list[float], proportion: float) -> float:
    """Return one linearly interpolated percentile from sorted values."""

    position = proportion * (len(sorted_values) - 1)
    lower = math.floor(position)
    upper = math.ceil(position)
    if lower == upper:
        return sorted_values[lower]
    fraction = position - lower
    return sorted_values[lower] * (1.0 - fraction) + sorted_values[upper] * fraction


def luminance_metrics(image: Image.Image, mask: Iterable[bool]) -> dict[str, float | int]:
    """Record descriptive luminance and neighbouring-pixel texture measurements."""

    mask_values = tuple(mask)
    require(
        len(mask_values) == image.width * image.height,
        "luminance mask dimensions do not match the review image",
    )
    all_values = pixel_luminances(image)
    values = [value for selected, value in zip(mask_values, all_values) if selected]
    require(bool(values), "luminance measurement selected no pixels")
    mean = sum(values) / len(values)
    variance = sum((value - mean) ** 2 for value in values) / len(values)
    ordered = sorted(values)
    neighbour_squared_deltas = []
    for y in range(image.height):
        for x in range(image.width):
            index = y * image.width + x
            if not mask_values[index]:
                continue
            if x + 1 < image.width and mask_values[index + 1]:
                neighbour_squared_deltas.append(
                    (all_values[index] - all_values[index + 1]) ** 2
                )
            if y + 1 < image.height and mask_values[index + image.width]:
                neighbour_squared_deltas.append(
                    (all_values[index] - all_values[index + image.width]) ** 2
                )
    require(bool(neighbour_squared_deltas), "texture measurement selected no pixel pairs")
    return {
        "pixel_count": len(values),
        "mean_relative_luminance": round(mean, 6),
        "luminance_standard_deviation": round(math.sqrt(variance), 6),
        "luminance_p10_p90_span": round(
            percentile(ordered, 0.9) - percentile(ordered, 0.1), 6
        ),
        "neighbor_rms_luminance_delta": round(
            math.sqrt(
                sum(neighbour_squared_deltas) / len(neighbour_squared_deltas)
            ),
            6,
        ),
    }


def save_png(image: Image.Image, path: Path) -> None:
    """Save one deterministic lossless review PNG."""

    path.parent.mkdir(parents=True, exist_ok=True)
    image.save(path, format="PNG", compress_level=9)


def render_sample_views(sample: ReviewSample) -> dict[str, Image.Image]:
    """Render all five ordered views for one terrain sample."""

    return {name: render_view(sample.image, name) for name in VIEW_ORDER}


def make_sheet(views: dict[str, Image.Image]) -> Image.Image:
    """Place the five fixed-order views side by side without resampling."""

    sheet = Image.new("RGBA", (SAMPLE_SIZE[0] * len(VIEW_ORDER), SAMPLE_SIZE[1]))
    for index, view_name in enumerate(VIEW_ORDER):
        sheet.alpha_composite(views[view_name], (index * SAMPLE_SIZE[0], 0))
    return sheet


def material_separations(
    samples: list[ReviewSample], rendered: dict[str, dict[str, Image.Image]]
) -> dict[str, list[dict[str, float | str]]]:
    """Describe luminance and texture differences among the three base materials."""

    by_id = {sample.sample_id: sample for sample in samples}
    pairs = (
        ("quiet_moss", "woodland_leaf_litter"),
        ("quiet_moss", "packed_earth_on_quiet_moss"),
        ("woodland_leaf_litter", "packed_earth_on_quiet_moss"),
    )
    result = {}
    for view_name in VIEW_ORDER:
        comparisons = []
        for first_id, second_id in pairs:
            first_sample = by_id[first_id]
            second_sample = by_id[second_id]
            first = luminance_metrics(
                rendered[first_id][view_name], first_sample.measurement_mask
            )
            second = luminance_metrics(
                rendered[second_id][view_name], second_sample.measurement_mask
            )
            comparisons.append(
                {
                    "first": first_id,
                    "second": second_id,
                    "absolute_mean_luminance_difference": round(
                        abs(
                            float(first["mean_relative_luminance"])
                            - float(second["mean_relative_luminance"])
                        ),
                        6,
                    ),
                    "absolute_texture_standard_deviation_difference": round(
                        abs(
                            float(first["luminance_standard_deviation"])
                            - float(second["luminance_standard_deviation"])
                        ),
                        6,
                    ),
                    "absolute_p10_p90_span_difference": round(
                        abs(
                            float(first["luminance_p10_p90_span"])
                            - float(second["luminance_p10_p90_span"])
                        ),
                        6,
                    ),
                    "absolute_neighbor_rms_texture_difference": round(
                        abs(
                            float(first["neighbor_rms_luminance_delta"])
                            - float(second["neighbor_rms_luminance_delta"])
                        ),
                        6,
                    ),
                }
            )
        result[view_name] = comparisons
    return result


def build_report(
    manifest: dict[str, object],
    assets: dict[str, dict[str, object]],
    samples: list[ReviewSample],
    rendered: dict[str, dict[str, Image.Image]],
) -> dict[str, object]:
    """Build the reproducible source binding and descriptive measurement record."""

    sample_records = []
    for sample in samples:
        views = []
        for view_name in VIEW_ORDER:
            view_path = VIEWS_ROOT / sample.sample_id / f"{view_name}.png"
            views.append(
                {
                    "name": view_name,
                    "path": view_path.relative_to(ROOT).as_posix(),
                    "sha256": sha256(view_path),
                    "measurements": luminance_metrics(
                        rendered[sample.sample_id][view_name], sample.measurement_mask
                    ),
                }
            )
        sheet_path = SHEETS_ROOT / f"{sample.sample_id}.png"
        sample_records.append(
            {
                "id": sample.sample_id,
                "label": sample.label,
                "inputs": [
                    {
                        "asset_id": asset_id,
                        "export": assets[asset_id]["export"],
                        "sha256": assets[asset_id]["sha256"],
                    }
                    for asset_id in sample.input_asset_ids
                ],
                "measurement_region": sample.measurement_region,
                "views": views,
                "side_by_side_sheet": {
                    "path": sheet_path.relative_to(ROOT).as_posix(),
                    "sha256": sha256(sheet_path),
                    "view_order": list(VIEW_ORDER),
                },
            }
        )
    return {
        "schema_version": 1,
        "status": "descriptive external review; not human accessibility acceptance",
        "manifest": {
            "path": MANIFEST.relative_to(ROOT).as_posix(),
            "sha256": sha256(MANIFEST),
            "schema_version": manifest["schema_version"],
        },
        "method": {
            "name": "Machado-Oliveira-Fernandes 2009 full-severity simulation",
            "colour_space": "sRGB IEC 61966-2-1 decoded to linear light",
            "severity": 1.0,
            "channel_handling": "matrix multiply in linear RGB; clip; encode to 8-bit sRGB",
            "alpha_handling": "preserved unchanged",
            "grayscale": "linear relative luminance Y = 0.2126 R + 0.7152 G + 0.0722 B",
            "matrices": {
                name: [list(row) for row in matrix]
                for name, matrix in CVD_MATRICES.items()
            },
        },
        "render": {
            "sample_size_pixels": {"width": SAMPLE_SIZE[0], "height": SAMPLE_SIZE[1]},
            "view_order": list(VIEW_ORDER),
            "packed_earth_composition": {
                "background": "terrain.quiet_moss",
                "foreground": PATH_ASSET_ID,
                "foreground_x": PATH_X,
                "foreground_y": 0,
            },
        },
        "samples": sample_records,
        "material_separation": material_separations(samples, rendered),
        "interpretation": {
            "measurements_are": "descriptive exact-pixel luminance and texture statistics",
            "measurements_are_not": (
                "thresholds, a pass/fail gate, or human accessibility acceptance"
            ),
        },
    }


def build() -> None:
    """Generate all views, side-by-side sheets, and the exact measurement report."""

    manifest, assets = load_manifest()
    samples = build_samples(assets)
    rendered = {}
    for sample in samples:
        views = render_sample_views(sample)
        rendered[sample.sample_id] = views
        for view_name, image in views.items():
            save_png(image, VIEWS_ROOT / sample.sample_id / f"{view_name}.png")
        save_png(make_sheet(views), SHEETS_ROOT / f"{sample.sample_id}.png")
    report = build_report(manifest, assets, samples, rendered)
    REVIEW_ROOT.mkdir(parents=True, exist_ok=True)
    (REVIEW_ROOT / "report.json").write_text(
        json.dumps(report, indent=2) + "\n", encoding="utf-8"
    )
    print("terrain accessibility review built")
    print(f"samples: {len(samples)}")
    print(f"views: {len(samples) * len(VIEW_ORDER)} exact manifest-derived PNGs")


if __name__ == "__main__":
    build()
