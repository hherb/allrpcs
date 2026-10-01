#!/usr/bin/env python3
"""Validate evidence for the failed, external-only connector feasibility proof."""

from __future__ import annotations

import argparse
import hashlib
from pathlib import Path

from PIL import Image


ROOT = Path(__file__).resolve().parents[4]
PROOF = ROOT / "art-direction/production-proof/corrected"
REVIEW = PROOF / "review"
STRAIGHT_HASH = "3d12ffdfb32fbbda2e9d0c2029803691a4eb4f8484a4ee5245a729f2fc8278bd"
JUNCTION_HASH = "0ba0b2cfa16466936710bb801409ac4c790058449e7d609b0df2cc96d772e2e3"
CAPTURE_HASH = "a37dfb54dd528173d707a615b5b262a491292195006a7298b0078fed071b3d8e"
VIEW_HASHES = {
    "0.8x-tall": "9fa26d8ef0136b5cc27eaf2fd19fb4608b931d0f1df4acf7a2cb95c44ca8d7eb",
    "0.8x-wide": "294ad13585b55b9f779cdf39c292c1a62cc39a04366494932014d2b2235824c1",
    "1.0x-tall": "6709e77e783b2c917da635251996d4ddf08928e327886d504ed19a8f6820bf14",
    "1.0x-wide": "1f25c5a62bd4240b5af1b5098af246313d4696b0b533e01cf210c353289d87ec",
    "2.0x-tall": "5ea842656ad9689db65a0a2ec481f012dcbcd3c9fb93fe0040698d865c378fc7",
    "2.0x-wide": "8cfbfd3537a16cbe357849a929241eaf7a1d9d6d7a72b5cbd8c2236095821a12",
}
BASELINE_HASHES = {
    "game/Main.tscn": "ab322e2a91acbadf029831893836c4e14e316235cdcbfce0494176f6262c5127",
    "game/GreyboxNavigation.cs": "169bd2b2d47f67c05bb312caf39d93fa20cc0957604d624140d18e3d24f1f4f0",
    "game/OpeningRegionVillageTransitionGeometry.cs":
        "ce235645a5c33c60809e74ec4f3d9b705bef09d1f8114d4cc7e0cc7ab876299f",
    "game/OpeningRegionLayoutGeometry.cs":
        "92524c8084675dd76de3105a2e4ed92f7b80b097c8d344593089428ebf277e57",
}


def require(condition: bool, message: str) -> None:
    """Reject evidence drift without confusing it with the expected visual failure."""
    if not condition:
        raise AssertionError(message)


def check_image(path: Path, size: tuple[int, int], digest: str) -> Image.Image:
    """Bind one decoded RGBA image to its original dimensions and byte digest."""
    require(hashlib.sha256(path.read_bytes()).hexdigest() == digest, f"hash changed: {path}")
    with Image.open(path) as image:
        require(image.size == size, f"dimensions changed: {path}")
        return image.convert("RGBA")


def validate(check_live_baseline: bool = False) -> None:
    """Check the exact crop, port mismatch, captures, and optional original runtime baseline."""
    terrain = ROOT / "game/art/runtime/terrain"
    straight = check_image(terrain / "straight-path.png", (96, 256), STRAIGHT_HASH)
    junction = check_image(terrain / "village-square-junction.png", (512, 512), JUNCTION_HASH)
    # Sprite rotation PI/2 is clockwise in screen coordinates. The centered 96x124
    # source region therefore occupies exactly x=1820..1944, y=852..948 at (1882,900).
    strip = straight.crop((0, 0, 96, 124)).transpose(Image.Transpose.ROTATE_270)
    require(strip.size == (124, 96), "connector footprint changed")
    left = list(strip.crop((123, 0, 124, 96)).getdata())
    right = list(junction.crop((0, 208, 1, 304)).getdata())
    equal = sum(a == b for a, b in zip(left, right))
    maximum = [max(abs(a[channel] - b[channel]) for a, b in zip(left, right)) for channel in range(4)]
    require(equal == 13 and maximum == [26, 30, 35, 1], "recorded seam mismatch changed")
    phase_scores = []
    for offset in range(straight.height - 124 + 1):
        candidate = straight.crop((0, offset, 96, offset + 124)).transpose(Image.Transpose.ROTATE_270)
        edge = list(candidate.crop((123, 0, 124, 96)).getdata())
        score = sum(sum(abs(x - y) for x, y in zip(a, b)) for a, b in zip(edge, right))
        phase_scores.append((score, offset))
    require(min(phase_scores) == (365, 0), "best crop phase changed")
    require(all(score > 0 for score, _ in phase_scores), "an exact crop phase now exists")
    capture = ROOT / "scripts/capture-village-edge-square-connector-proof.gd"
    require(hashlib.sha256(capture.read_bytes()).hexdigest() == CAPTURE_HASH, "capture recipe changed")
    for spec, digest in VIEW_HASHES.items():
        size = (1280, 827 if spec.endswith("tall") else 720)
        check_image(REVIEW / f"village-edge-square-connector-{spec}.png", size, digest)
    if check_live_baseline:
        for relative, digest in BASELINE_HASHES.items():
            actual = hashlib.sha256((ROOT / relative).read_bytes()).hexdigest()
            require(actual == digest, f"original runtime baseline changed: {relative}")
    print("Connector evidence valid: six captures; exact 124x96 crop; 83/96 port pixels differ.")
    print("Feasibility FAILED: retained greybox fringes and unmatched port; runtime promotion not authorized.")


def main() -> None:
    """Validate archived evidence; opt into original-baseline checks only at this slice."""
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--check-live-baseline", action="store_true")
    args = parser.parse_args()
    validate(args.check_live_baseline)


if __name__ == "__main__":
    main()
