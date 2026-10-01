# Village Square–Southern Road external feasibility

The owner approved all six Square–Sickhouse runtime views with "approved" on 2026-10-02 after
display. The following slice tests only the existing straight-path texture on the retained
Square–Southern Road continuation. It changes no runtime/production asset, master, manifest,
import, ordinary scene, or physical/gameplay authority. External owner review is pending;
no Southern Road promotion is authorized.

Square is anchored at `(2200,900)`, Southern Road at `(1500,1400)`, and the 512-pixel junction
footprint exit is `(1944,1082.857143)`. The remaining route is `545.633203` pixels along heading
`144.462322°`, retaining 96-pixel width. The failed strip repeats the existing 96x256 module
without scaling at `(1722,1241.428571)`, rotated `54.462322°`. Its flat entry leaves a triangular
junction gap and the retained greybox line shows through its alpha edges. All six failed views
remain archived under `square-southern-road-path-proof-*`.

The correction uses the accepted junction's exact sampling heading and existing Pillow bicubic
recipe, sampled from the Square pivot. A world-aligned `520x416` external overlay starts at
`(1440,1024)`, draws at `(1700,1232)`, `z=-71`, with no rotation or scale, and overlaps 16 columns
beneath the junction's west edge. Within the complete junction intersection, all 1,662 visible
pixels and all transparent pixels match its RGBA exactly. Pixel centres are clipped at the
unchanged perpendicular Southern Road endpoint. Zero-alpha RGB is clean. The first canvas
clipped a shoulder outside the junction; adding 16 top-padding rows fixed the coverage check.
Top/right clipping now occurs only inside the matching junction overlap.

Only the exact matching presentation line is hidden in the corrected disposable scene instance.
The correction explicitly disables texture repeat on its finite overlay; the failed strip uses
repeat to cover its longer span. The Seeker and screen overlays are hidden and the camera is
temporarily centered on the retained continuation. No scene is saved. Other routes, the road
presentation and outline, wagon marker/label, Square/Sage markers, and Garden objects/labels
retain their ownership and draw order.

The correction resolves the junction gap. Its far end still forms a flat textured patch across
the existing untextured Southern Road junction; the road presentation partly covers it. This
is an explicit visual limitation for owner review, not a claim that the endpoint is naturally
finished. At 2.0x, nearby world labels largely fall outside the frame; 0.8x/1.0x show their
context. These external views establish no pacing, usability, UI, or human accessibility acceptance.

| Zoom | Failed wide | Failed tall | Corrected wide | Corrected tall |
| --- | --- | --- | --- | --- |
| 0.8x | [view](square-southern-road-path-proof-0.8x-wide.png) | [view](square-southern-road-path-proof-0.8x-tall.png) | [view](square-southern-road-path-correction-0.8x-wide.png) | [view](square-southern-road-path-correction-0.8x-tall.png) |
| 1.0x | [view](square-southern-road-path-proof-1.0x-wide.png) | [view](square-southern-road-path-proof-1.0x-tall.png) | [view](square-southern-road-path-correction-1.0x-wide.png) | [view](square-southern-road-path-correction-1.0x-tall.png) |
| 2.0x | [view](square-southern-road-path-proof-2.0x-wide.png) | [view](square-southern-road-path-proof-2.0x-tall.png) | [view](square-southern-road-path-correction-2.0x-wide.png) | [view](square-southern-road-path-correction-2.0x-tall.png) |

Reproduce from `3962515` plus these proof recipes with Pillow-enabled Python and Godot .NET:

```sh
/opt/homebrew/bin/python3 art-direction/production-proof/corrected/tools/build_square_southern_road_path_correction.py
godot --path game --script "$PWD/scripts/capture-square-southern-road-path-proof.gd"
godot --path game --script "$PWD/scripts/capture-square-southern-road-path-proof.gd" -- --corrected
/opt/homebrew/bin/python3 art-direction/production-proof/corrected/tools/check_square_southern_road_path_feasibility.py --check-live-baseline
```

The pinned evidence record binds all thirteen images, sampling/capture recipes, source textures,
and optional frozen ordinary scene/code baseline. Regular quality validates archived evidence;
`--check-live-baseline` additionally proves this slice's runtime inputs unchanged. PNG bytes
depend on the renderer; these use Godot 4.6.3 .NET Compatibility on this macOS host.

Session review inspected the generator, capture, validator, and all six final views. It fixed
the cropped shoulder by expanding the canvas. The validator checks the entire junction
intersection, endpoint containment, unclipped exposed sides, clean alpha, exact image inventory,
dimensions/hashes, and deterministic pixel rebuild. Negative corrupt-evidence and sampler-drift
checks and Python lint passed. No unresolved code finding remains; the flat endpoint limitation
remains visible and documented for owner review.
All validators, formatting, zero-warning build, 5,033 tests, scenario validation, and the full
host Godot smoke suite passed. Sandbox Godot user-log restrictions required the identical host
retry; no product workaround was introduced. Compatibility launch passed.

Next: review all six corrected views for junction seam, repetition/width, Southern Road endpoint,
and marker/label layering. Correct only this external proof if requested. After acceptance,
prepare one exact named promotion proposal and obtain separate approval before modifying
production assets or runtime. Preserve all physical/gameplay authority and every other route.
