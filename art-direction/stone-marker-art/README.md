# Stone marker standing tablet

30 October 2026 (Australia/Sydney). After closing the depth-order slice (PR #482), the owner asked
for the greybox markers to be replaced, starting with the stone marker. They also asked for the
stone marker to join the depth order. The three replies are in [authority.json](authority.json).

## Runtime

![Seeker north of (behind) the tablet](captures/stone-marker-seeker-north.png)
![Seeker south of (in front of) the tablet](captures/stone-marker-seeker-south.png)

In `game/Main.tscn`, the `Marker` Polygon2D under `InteractionCandidates/StoneMarker` is now a
Sprite2D. It shows `game/art/runtime/objects/inscriptions-01.png`, a byte copy of the approved
[objects-batch](../objects-batch/README.md) export "Standing tablet" (bound by hash in its
`owner-acceptance.json`). No new artwork was made. The sprite sits at (0, −17), so the export's proposed
foot pivot (41, 75) stands on the old polygon's base line (y +18), just above the unchanged
`STONE MARKER` label. The node position (1080, 350), which the interaction ranges and perception
measure from, is unchanged.

The 80 px tablet is twice the height of the old polygon, so a Seeker standing just behind it was
drawn over it and looked as if it stood on the stone. `CharacterDepthOrderPresentation` now gives
the marker z `1 + DepthDrawOrder.CountNorthOf(...)`, the number of ranked figures whose feet are
north of the tablet's foot. The Sage, water cask, Healer and Seeker keep their accepted ranks
z 1..4, so the accepted depth-order check passes unchanged. When the marker ties with the first
figure south of it, Godot draws in tree order. The cask, Healer and Seeker all come after the
marker in `Main.tscn`, so they draw in front. The fixed Sage comes before it, but a tie with the
Sage (possible only once the cask has been moved south of it) never shows, because the two are
far apart.

## Checks

- `scripts/validate-stone-marker-art.py` checks the objects-batch acceptance binding, that the
  runtime PNG is byte-identical to the approved export and that its import is lossless. It also checks
  the captures and their art and script inputs against [capture-evidence.json](capture-evidence.json).
  It checks only those bytes, never the whole tree (#471).
- `scripts/check-stone-marker-art.gd` checks the texture, the foot placement and the unchanged
  interaction point. It checks that the cask, Healer and Seeker come after the marker in the tree.
  It also checks that a Seeker level with the tablet's foot draws in front of it and one 1 px north
  of it draws behind it, which ties the presentation's foot offset to the sprite's foot.
- `DepthDrawOrderTests` unit-tests `CountNorthOf`, including that it splits the `Ranks` exactly.
- `scripts/capture-stone-marker-art.gd` writes the two `captures/` and
  [capture-evidence.json](capture-evidence.json).

Results are in [verification.json](verification.json).
