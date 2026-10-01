# Village Edge–Village Square connector correction result

Status: external proof complete; west transition remains visually unsuitable at 2.0x.
This is not an accepted runtime candidate. The owner authorized the handover's correction
slice and removed the requirement for separate approval before starting prescribed next slices.
Visual acceptance and ordinary-scene promotion remain separate boundaries.

## Exact composition and findings

The review-only strip occupies `x=1820..1944`, `y=852..948`, centered at `(1882,900)`.
It is a clockwise rotation of the original `96x124` straight-path crop, with only the first
and last 32 columns blended. Columns `32..91` remain byte-identical to the source crop.
For each RGBA channel, the deterministic blend is
`(source * (31 - weight) + target * weight + 15) // 31`.
West weights fall from 31 to 0 across columns `0..31`; east weights rise from 0 to 31
across columns `92..123`. The west target is the retained opaque greybox route colour
`(114,100,77,255)`. The east target is junction column zero, rows `208..303`.
All 96 east pixel pairs now match exactly. The west boundary matches the route colour.

The capture modifies exactly one presentation line in a temporary `Main.tscn` instance:
its Square-side endpoint moves from `(1944,900)` to `(1820,900)`, retaining the Village Edge
endpoint `(1700,900)`. This removes the exposed brown rectangular fringe beneath the strip.
The review sprite stays at `z=-70`, above the route line and below opening markers and labels.
The Seeker is hidden and its physics disabled only for capture, with the camera centered and
smoothing disabled. Existing markers, labels, junction, Stream, and other approaches remain.

All six views were inspected. The east join and irregular central edges are improved, but the
west 32-column fade forms a conspicuous dark band beside the yellow opening marker, especially
in both 2.0x views. The fully opaque west boundary also gives the fade rectangular shoulders.
Exact colour matching is insufficient visual evidence for a finished transition. Following the
proposal's failure gate, the footprint was not widened and no substitute asset was introduced.
No human visual, pacing, usability, or accessibility acceptance is inferred.

## Evidence and reproduction

| Zoom | Wide | Tall |
| --- | --- | --- |
| 0.8x | [view](village-edge-square-connector-corrected-0.8x-wide.png) | [view](village-edge-square-connector-corrected-0.8x-tall.png) |
| 1.0x | [view](village-edge-square-connector-corrected-1.0x-wide.png) | [view](village-edge-square-connector-corrected-1.0x-tall.png) |
| 2.0x | [view](village-edge-square-connector-corrected-2.0x-wide.png) | [view](village-edge-square-connector-corrected-2.0x-tall.png) |

```sh
python3 art-direction/production-proof/corrected/tools/build_village_edge_square_connector_correction.py
godot --path game --script "$PWD/scripts/capture-village-edge-square-connector-correction.gd"
python3 art-direction/production-proof/corrected/tools/check_village_edge_square_connector_correction.py --check-live-baseline
```

Two graphical Compatibility runs produced identical hashes at 1280x720/827. The evidence JSON
binds the recipe, strip, six views, original exports/masters/imports/runtime copies, and live
scene/geometry files. Omit `--check-live-baseline` for archived validation after future authorized
runtime edits. Exit zero confirms evidence integrity, not visual acceptance. The previous
failed unchanged-strip evidence and capture recipe remain intact.

Session code review added generator source-hash/dimension guards before writing the strip.
Focused validation and negative central/west/east pixel-corruption checks passed. No game file,
production master/export, import, physical/gameplay authority, topology, collision, navigation,
camera configuration, scenario, provider, UI, audio, release, or signing file changed.

## Follow-on diagnostic slice

The external-only west-handoff diagnosis is complete in
[village-edge-square-west-handoff-diagnosis.md](village-edge-square-west-handoff-diagnosis.md).
It attributes the new rectangular shoulders to raised alpha and records one exact next
alpha-preserving, eight-column RGB fade comparison. No alternative image has been built yet.

Full host-local `./scripts/quality.sh` passed validators, formatting, zero-warning build,
5,033 tests, scenario validation, all credential-free Godot smokes, and Compatibility launch.
The identical sandbox run reached Godot but failed on the known user-log access restriction;
its host retry completed. The final focused evidence checks also passed after both repeated
capture runs.
