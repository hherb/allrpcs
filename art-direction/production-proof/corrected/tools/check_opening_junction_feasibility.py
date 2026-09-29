#!/usr/bin/env python3
"""Check exact existing path-module headings at the approved laundry junction."""

from __future__ import annotations

import hashlib
import json
import math
import re
import struct
from pathlib import Path


ROOT = Path(__file__).resolve().parents[4]
PROOF = ROOT / "art-direction/production-proof/corrected"
ANCHOR_IDS = ("stream", "abandoned-laundry", "woodland-clearing")
ANCHOR_PATTERN = re.compile(
    r'\["(?P<id>[^"]+)"\]\s*=\s*new\('
    r'(?P<x>[\d.]+)f,\s*(?P<y>[\d.]+)f\)'
)


def require(condition: bool, message: str) -> None:
    """Reject input drift rather than silently reporting stale geometry."""
    if not condition:
        raise ValueError(message)


def png_size(path: Path) -> tuple[int, int]:
    """Read the dimensions of one manifest-bound PNG without changing it."""
    with path.open("rb") as image:
        header = image.read(24)
    require(header[:8] == b"\x89PNG\r\n\x1a\n", f"invalid PNG: {path}")
    require(header[12:16] == b"IHDR", f"missing PNG IHDR: {path}")
    return struct.unpack(">II", header[16:24])


def heading(first: tuple[float, float], second: tuple[float, float]) -> float:
    """Return the route's world-space heading in degrees."""
    return math.degrees(math.atan2(second[1] - first[1], second[0] - first[0]))


def main() -> None:
    """Bind the live anchors and exact exports, then report bend feasibility."""
    layout = (ROOT / "game/OpeningRegionLayoutGeometry.cs").read_text(encoding="utf-8")
    anchors = {
        match["id"]: (float(match["x"]), float(match["y"]))
        for match in ANCHOR_PATTERN.finditer(layout)
    }
    require(all(anchor_id in anchors for anchor_id in ANCHOR_IDS), "junction anchor missing")
    require(
        re.search(r"PathWidthPixels\s*=\s*96\.0f", layout) is not None,
        "live path width changed",
    )
    live_connections = layout.split("internal static IReadOnlyList", 1)[1]
    require(
        all(
            f'("{first}", "{second}")' in live_connections
            for first, second in zip(ANCHOR_IDS, ANCHOR_IDS[1:])
        ),
        "live junction connections changed",
    )
    proposal = json.loads(
        (ROOT / "art-direction/world-layout-proposal/layout-proposal.json").read_text(
            encoding="utf-8"
        )
    )
    proposal_anchors = {entry["id"]: (entry["x"], entry["y"]) for entry in proposal["anchors"]}
    require(
        all(anchors[anchor_id] == proposal_anchors[anchor_id] for anchor_id in ANCHOR_IDS),
        "live junction anchors differ from the approved proposal",
    )
    connections = {(entry["first"], entry["second"]) for entry in proposal["connections"]}
    require(
        ((ANCHOR_IDS[0], ANCHOR_IDS[1]) in connections)
        and ((ANCHOR_IDS[1], ANCHOR_IDS[2]) in connections),
        "approved junction connections changed",
    )

    manifest = json.loads((PROOF / "asset-manifest.json").read_text(encoding="utf-8"))
    assets = {asset["id"]: asset for asset in manifest["assets"]}
    for asset_id, size in (("terrain.straight_path", (96, 256)), ("terrain.path_bend", (256, 256))):
        asset = assets[asset_id]
        path = PROOF / asset["export"]
        require(png_size(path) == size, f"{asset_id} export dimensions changed")
        require(
            hashlib.sha256(path.read_bytes()).hexdigest() == asset["sha256"],
            f"{asset_id} export digest changed",
        )

    incoming = heading(anchors[ANCHOR_IDS[0]], anchors[ANCHOR_IDS[1]])
    outgoing = heading(anchors[ANCHOR_IDS[1]], anchors[ANCHOR_IDS[2]])
    turn = (outgoing - incoming + 180.0) % 360.0 - 180.0
    mismatch = abs(abs(turn) - 90.0)
    require(mismatch > 1e-6, "the existing bend now fits; reassess the handover")
    half_bend = 128.0
    lateral_drift = half_bend * math.sin(math.radians(mismatch))

    proof = manifest["opening_junction_proof"]
    proof_asset = assets[proof["asset"]]
    proof_path = PROOF / proof_asset["export"]
    require(proof["anchor_ids"] == list(ANCHOR_IDS), "junction proof anchors drifted")
    require(
        proof["path_width_pixels"] == 96
        and proof["port_depth_pixels"] == 32,
        "junction proof port geometry drifted",
    )
    require(
        math.isclose(proof["turn_degrees"], turn, abs_tol=1e-6),
        "junction proof turn no longer matches the live routes",
    )
    canonical_outgoing = 90.0 + turn
    require(
        math.isclose(
            proof["canonical_outgoing_heading_degrees"],
            canonical_outgoing,
            abs_tol=1e-6,
        ),
        "junction proof outgoing heading drifted",
    )
    world_rotation = (incoming - 90.0 + 180.0) % 360.0 - 180.0
    projected_outgoing = (
        proof["canonical_outgoing_heading_degrees"] + world_rotation + 180.0
    ) % 360.0 - 180.0
    require(
        math.isclose(projected_outgoing, outgoing, abs_tol=1e-6),
        "junction proof cannot align both live route headings by rigid rotation",
    )
    require(
        png_size(proof_path) == (512, 512),
        "junction proof export dimensions changed",
    )
    require(
        hashlib.sha256(proof_path.read_bytes()).hexdigest() == proof_asset["sha256"],
        "junction proof export digest changed",
    )
    print(f"Stream -> Laundry heading: {incoming:.6f} degrees")
    print(f"Laundry -> Woodland heading: {outgoing:.6f} degrees")
    print(f"Required turn: {turn:.6f} degrees; exact bend turn: 90.000000 degrees")
    print(f"Port mismatch: {mismatch:.6f} degrees")
    print(f"Lateral drift over {half_bend:.0f} pixels: {lateral_drift:.3f} pixels")
    print("Result: the exact unscaled 90-degree bend cannot join both route headings.")
    print(
        "Approved proof: terrain.opening_junction has exact 96px ports at "
        f"90.000000 and {canonical_outgoing:.6f} degrees on a 512x512 canvas."
    )
    print(
        f"Rigid world rotation: {world_rotation:.6f} degrees; projected outgoing heading: "
        f"{projected_outgoing:.6f} degrees."
    )
    print(
        "Footprint: "
        f"{proof['visible_bounds']['width']}x{proof['visible_bounds']['height']} visible pixels; "
        "the corrected proof validator checks exact port pixels and 96px route containment."
    )


if __name__ == "__main__":
    main()
