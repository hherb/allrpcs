#!/usr/bin/env python3
"""Validate exact terrain colour-vision review pixels and measurement evidence."""

from __future__ import annotations

import json
from pathlib import Path

from PIL import Image

import terrain_accessibility_review as review


def images_equal(first: Image.Image, second: Image.Image) -> bool:
    """Return whether two equal-sized RGBA images match in every channel."""

    if first.size != second.size or first.mode != second.mode:
        return False
    return first.tobytes() == second.tobytes()


def main() -> None:
    """Recompute every view and reject source, pixel, file-set, or report drift."""

    manifest, assets = review.load_manifest()
    samples = review.build_samples(assets)
    rendered = {}
    expected_pngs: set[Path] = set()
    for sample in samples:
        views = review.render_sample_views(sample)
        rendered[sample.sample_id] = views
        for view_name, expected in views.items():
            path = review.VIEWS_ROOT / sample.sample_id / f"{view_name}.png"
            expected_pngs.add(path)
            review.require(path.is_file(), f"missing accessibility view: {path}")
            actual = Image.open(path).convert("RGBA")
            review.require(
                images_equal(actual, expected),
                f"accessibility view drifted from current exports: {path}",
            )
        sheet_path = review.SHEETS_ROOT / f"{sample.sample_id}.png"
        expected_pngs.add(sheet_path)
        review.require(sheet_path.is_file(), f"missing side-by-side sheet: {sheet_path}")
        actual_sheet = Image.open(sheet_path).convert("RGBA")
        review.require(
            images_equal(actual_sheet, review.make_sheet(views)),
            f"side-by-side sheet order or pixels drifted: {sheet_path}",
        )

    actual_pngs = set(review.REVIEW_ROOT.rglob("*.png"))
    review.require(
        actual_pngs == expected_pngs,
        "terrain accessibility PNG set diverged from the exact bounded review",
    )
    report_path = review.REVIEW_ROOT / "report.json"
    review.require(report_path.is_file(), "terrain accessibility report is missing")
    actual_report = json.loads(report_path.read_text(encoding="utf-8"))
    expected_report = review.build_report(manifest, assets, samples, rendered)
    review.require(
        actual_report == expected_report,
        "terrain accessibility source bindings or measurements drifted",
    )

    print("terrain accessibility review validation passed")
    print(f"samples: {len(samples)}")
    print(f"views: {len(expected_pngs) - len(samples)} exact recomputed images")
    print("method: Machado-Oliveira-Fernandes 2009, full severity, linear sRGB")
    print("measurements: descriptive luminance and texture statistics; no acceptance gate")


if __name__ == "__main__":
    main()
