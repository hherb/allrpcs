# Village Edge–Village Square alpha-preserving comparison

Status: the owner accepted the six supplied previews and approved the scoped runtime promotion
after the checkpoint. The [runtime result](village-edge-square-connector-runtime-result.md)
records that implementation. The failed original and opaque-fade proofs remain independent evidence.

The strip uses the same `124x96` footprint at `(1882,900)` with no scale or rotation in the
preview. Its west 32 columns retain the original rotated crop's alpha byte-for-byte. Only RGB
in columns `0..7` blends toward `(114,100,77)`, using weight `7-x` and the integer formula
`(source * (7 - weight) + target * weight + 3) // 7`. Original source pixels are retained
throughout columns `8..91`. The corrected proof's east 32-column blend is retained exactly,
including all 96 matching east-port RGBA pairs. The same one-line shortening exists only in
the temporary preview instance; all runtime and production assets remain unchanged.

All six views were inspected alongside the opaque fade. The west dark band is substantially
reduced and the new opaque rectangular shoulders are removed. No brown underlying route fringe
is visible. The yellow opening marker still separates the textured path from the retained grey
Village Edge room. This is a reviewer recommendation for a bounded visual checkpoint, not a
claim that the surrounding greybox presentation is finished. Labels and markers remain above
the artwork; 2.0x deliberately clips distant labels, consistent with the previous proof frames.
No human pacing, usability, or accessibility acceptance is inferred.

| Zoom | Wide | Tall |
| --- | --- | --- |
| 0.8x | [view](village-edge-square-connector-alpha-0.8x-wide.png) | [view](village-edge-square-connector-alpha-0.8x-tall.png) |
| 1.0x | [view](village-edge-square-connector-alpha-1.0x-wide.png) | [view](village-edge-square-connector-alpha-1.0x-tall.png) |
| 2.0x | [view](village-edge-square-connector-alpha-2.0x-wide.png) | [view](village-edge-square-connector-alpha-2.0x-tall.png) |

```sh
python3 art-direction/production-proof/corrected/tools/build_village_edge_square_connector_alpha.py
godot --path game --script "$PWD/scripts/capture-village-edge-square-connector-alpha.gd"
python3 art-direction/production-proof/corrected/tools/check_village_edge_square_connector_alpha.py
```

Two graphical Compatibility runs are repeat-run identical at exact 1280x720/827 dimensions.
Focused validation binds recipe, strip, frames, retained correction, and original proof evidence;
`--check-live-baseline` checked the unchanged external-slice scene/geometry and source files;
that scene baseline is now superseded by the approved promotion. Negative west-alpha, original-pixel, and east-blend corruption
checks each fail closed. The retained diagnostic JSON exactly matches regenerated measurements.

Session review found and fixed missing approved-source guards in the first correction generator;
the alpha builder inherits those guards. Capture recipes reject missing/wrong-sized images and
missing/ambiguous route matches. No unresolved implementation finding remains.

The [connector promotion proposal](village-edge-square-connector-runtime-promotion-proposal.md)
was approved after the six views were shown. Ordinary-scene owner feedback is recorded separately
from that external acceptance and automated runtime comparison.

Full host-local `./scripts/quality.sh` passed validators, formatting, zero-warning build,
5,033 tests, scenario validation, all credential-free Godot smokes, and Compatibility launch.
The identical sandbox run reached Godot but failed on the known user-log access restriction;
its host retry completed. The final focused evidence checks also passed after both repeated
capture runs.
