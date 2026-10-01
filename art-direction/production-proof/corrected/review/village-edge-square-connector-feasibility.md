# Village Edge–Village Square connector feasibility result

Status: failed feasibility, not a runtime candidate. The product owner approved building the
unchanged-module proof; no acceptance of these new views or correction authority is recorded.

The temporary scene adds only a centered, clockwise-rotated `96x124` region of the unchanged
`terrain.straight_path` at `(1882,900)`. Its world footprint is exactly `x=1820..1944`,
`y=852..948`, with no scale or gap to the accepted junction. It uses the existing art layer
(`z=-70`), below opening markers and labels. The capture-only Seeker is hidden and its physics
disabled so it does not obscure the strip; the camera is centered without smoothing. No scene,
runtime asset, source export/master, physical opening, collision, navigation, or gameplay file
was changed. The original Main, route presenter, layout, and village-transition file hashes pass
the optional baseline check in the evidence validator.

## Findings

- The retained greybox route beneath the strip is visible through the path's irregular alpha
  edges, producing brown rectangular fringes. This is conspicuous in both 2.0x views.
- The west end abruptly meets the retained Village Edge greybox at `x=1820`; the yellow opening
  marker remains visible above the handoff. This is not a finished visual transition.
- At the east port, 83 of 96 RGBA pixel pairs differ from the accepted junction's west edge.
  Maximum channel differences are `(26,30,35,1)`; only 13 pairs match exactly. Comparison uses
  the rotated strip's final column against junction column zero, rows `208..303`.
  An exhaustive check of the 133 valid source-region offsets finds no exact port match; offset
  zero is already the minimum absolute RGBA difference (365 over all 384 channel samples).

These are recorded failures, not issues silently fixed outside the approved scope. Labels and
markers remain in their ordinary layers; the wide framing supplies context and 2.0x deliberately
clips distant labels. No pacing, usability, accessibility, or product-owner acceptance is inferred.

## Evidence and reproduction

All six images were inspected. Two independent Compatibility capture runs produced identical
SHA-256 hashes at exact 1280x720 and 1280x827 sizes:

| Zoom | Wide | Tall |
| --- | --- | --- |
| 0.8x | [view](village-edge-square-connector-0.8x-wide.png) | [view](village-edge-square-connector-0.8x-tall.png) |
| 1.0x | [view](village-edge-square-connector-1.0x-wide.png) | [view](village-edge-square-connector-1.0x-tall.png) |
| 2.0x | [view](village-edge-square-connector-2.0x-wide.png) | [view](village-edge-square-connector-2.0x-tall.png) |

From the repository root, regenerate using a graphical Compatibility renderer (not headless):

```sh
godot --path game --script "$PWD/scripts/capture-village-edge-square-connector-proof.gd"
python3 art-direction/production-proof/corrected/tools/check_village_edge_square_connector_feasibility.py --check-live-baseline
```

The validator checks asset hashes/dimensions, exact crop, recorded seam mismatch, and all six
capture hashes. Exit zero means the failed-proof evidence is intact, not visual feasibility.
Omit `--check-live-baseline` when validating archived evidence after future authorized scene edits.

The focused validator and negative dimension/hash/recipe-drift checks passed. The full
`./scripts/quality.sh` passed on the host: validators, formatting, zero-warning build, 5,033
tests, scenario validation, all Godot smokes, and Compatibility launch. The sandbox run passed
through tests/scenario but hit the known Godot user-log permission crash; the identical host
retry completed. Session review added a missing-image/frame-size guard to the Stream capture
script; regenerating its six accepted views preserved every original hash. No unresolved
implementation finding remains; the connector's recorded visual failure is intentionally not
fixed under the unchanged-module approval.

Stop at the [external correction proposal](village-edge-square-connector-correction-proposal.md).
