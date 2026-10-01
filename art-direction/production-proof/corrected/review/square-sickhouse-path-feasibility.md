# Village Square–Sickhouse external feasibility

The 2026-10-02 proof tests the existing straight-path texture on the retained Square–Sickhouse
continuation. It changes no production export, editable master, manifest, import, runtime scene,
or physical/gameplay authority. The owner accepted all six corrected external views with "accepted" on 2026-10-02 after
their display. This is external visual acceptance; no promotion is authorized.

The Square anchor is `(2200,900)`, Sickhouse anchor `(2640,1450)`, and the 512-pixel junction
footprint exit is `(2404.8,1156)`. The retained continuation is `376.503705` pixels long along
heading `51.340192°` and 96 pixels wide. A centered straight strip at `(2522.4,1303)` rotated
`-38.659808°` repeats the 96x256 module without scaling. Its flat entry leaves a triangular
gap at the horizontal junction boundary, and the retained greybox line shows at its alpha edges.
All six failed `square-sickhouse-path-proof-*` views remain archived.

The correction samples the module from the Square pivot with the accepted junction's exact
heading and existing Pillow bicubic recipe. The world-aligned `380x360` external overlay begins
at `(2320,1140)`, overlaps 16 rows beneath the junction, and clips pixel centres at the unchanged
perpendicular Sickhouse endpoint. All 1,713 visible pixels within the junction intersection
match its RGBA exactly; transparent intersection pixels match too. Zero-alpha pixels have clean
RGB, and the canvas does not clip either path side or its far endpoint. The external sprite sits
at `(2510,1320)`, `z=-71`, without rotation or scale, beneath the junction at `-70`.

Only the matching greybox presentation line is hidden in the corrected disposable scene instance.
The Sickhouse structure and entrance marker remain above the path, so they obscure its far
terminus as in the ordinary scene. The patient marker, Garden preparation table/herb bed,
world labels, and other routes retain their ownership and draw order. The pre-existing Garden
label extends over the Sickhouse at high zoom. At 2.0x wide, the bottom Sickhouse label is partly
outside the frame; the tall view shows it. Neither framing limitation authorizes a runtime fix.

The Seeker and screen CanvasLayer overlays are hidden only in the disposable capture instance;
the first inspection caught command panels obscuring the seam, so the final capture recipe
removes that obstruction. The camera is temporarily centered on the retained continuation.
No scene is saved. Captures are external presentation evidence, with no ordinary-play pacing,
usability, UI, or human accessibility acceptance.

| Zoom | Failed wide | Failed tall | Corrected wide | Corrected tall |
| --- | --- | --- | --- | --- |
| 0.8x | [view](square-sickhouse-path-proof-0.8x-wide.png) | [view](square-sickhouse-path-proof-0.8x-tall.png) | [view](square-sickhouse-path-correction-0.8x-wide.png) | [view](square-sickhouse-path-correction-0.8x-tall.png) |
| 1.0x | [view](square-sickhouse-path-proof-1.0x-wide.png) | [view](square-sickhouse-path-proof-1.0x-tall.png) | [view](square-sickhouse-path-correction-1.0x-wide.png) | [view](square-sickhouse-path-correction-1.0x-tall.png) |
| 2.0x | [view](square-sickhouse-path-proof-2.0x-wide.png) | [view](square-sickhouse-path-proof-2.0x-tall.png) | [view](square-sickhouse-path-correction-2.0x-wide.png) | [view](square-sickhouse-path-correction-2.0x-tall.png) |

Reproduce from `bf54cd5` plus this proof's recipes, using Pillow-enabled Python and the configured
Godot .NET Compatibility wrapper:

```sh
/opt/homebrew/bin/python3 art-direction/production-proof/corrected/tools/build_square_sickhouse_path_correction.py
godot --path game --script "$PWD/scripts/capture-square-sickhouse-path-proof.gd"
godot --path game --script "$PWD/scripts/capture-square-sickhouse-path-proof.gd" -- --corrected
/opt/homebrew/bin/python3 art-direction/production-proof/corrected/tools/check_square_sickhouse_path_feasibility.py --check-live-baseline
```

The pinned evidence record binds the capture and sampling recipes, source textures, thirteen
images, and optional frozen ordinary scene/code baseline. Regular quality validates archived
evidence; `--check-live-baseline` additionally checks this slice's unchanged runtime inputs.
PNG bytes are renderer-specific: these use Godot 4.6.3 .NET Compatibility on this macOS host.
The validator rebuilds pixels and checks the entire junction intersection, clean alpha,
unclipped canvas, exact image inventory/dimensions/hashes, and endpoint containment.

Next: obtain separate approval of the [named promotion proposal](square-sickhouse-path-promotion-proposal.md)
before changing production assets or runtime. The external checkpoint covers the seam, repeated
texture, apparent width, Sickhouse entrance, and marker/label layering only.

Session code review inspected the sampling, capture, validator, all six corrected views, and
complete change scope. Screen-overlay obstruction was fixed before final captures; no unresolved
finding remains. Frozen-baseline validation, deterministic pixel rebuild, Python lint, and
negative corrupt-evidence/sampler checks passed. All quality validators, formatting, zero-warning
build, 5,033 tests, scenario validation, full host Godot smokes, and Compatibility launch passed.
The sandbox Godot launch/smoke could not access user logs and required the identical host retry.
No product workaround was introduced; no tracked runtime, production asset/master, or manifest
changed. The new archived-evidence validator is included in regular quality.
