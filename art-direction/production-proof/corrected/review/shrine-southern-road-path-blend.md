# Shrine–Southern Road external endpoint blend

The continuation authorized after the [world-aligned correction](shrine-southern-road-path-correction.md)
adds a local texture blend at the Shrine–Road / accepted Road–Wagon crossing. The original
18 correction views, six failed-strip views, recipes and evidence remain immutable.
This is external-only. No runtime, production, manifest, import, physical or gameplay file changes.

The disposable scene retains the original incoming correction at `z=-72`, and adds one 160x160
patch at world origin `(1400,1280)`, centered `(1480,1360)`, `z=-69`. Its visible pixels stay
inside a 48px band around the leading edge of the accepted Road–Wagon leg: the retained
`(1500,1400)` → `(1820,1580)` heading, with its existing 48px leading extension.
The patch blends only existing source colors from the incoming Shrine path and the accepted
Square/Road–Wagon textures. It matches the underlying Square-over-Shrine draw order, extends
color from the Road–Wagon leg's first interior row, and feathers color/opacity through the band.
Transparent pixels have no hidden RGB; the patch canvas has a fully transparent gutter.

The existing Square/Road–Wagon sprite nodes and their texture files stay unchanged. The patch
locally draws over their crossing in the proof; it has no authority beyond presentation.
The unchanged original correction still hides only the Shrine physical-route presentation
line and upstream brown fill. Shrine shell/opening, world labels, outlines, collisions,
physical routes, camera limits and all other accepted terrain keep their ordinary ownership.

**Result: a softened endpoint transition ready for owner review.** The long straight diagonal
line seen in the original 2.0x junction view is covered by the blend. Judge the resulting
texture transition and ragged edges in the close views before accepting it. The dark
Road–Wagon boundary outlines are unchanged. This proof does not authorize runtime promotion.

All 18 original camera targets, zooms and frame sizes are retained. The camera keeps its
ordinary world limits; the shrine-exit views are byte-identical to the original correction.

## Midpoint

| Zoom | Wide | Tall |
| --- | --- | --- |
| 0.8x | [view](shrine-southern-road-path-blend-midpoint-0.8x-wide.png) | [view](shrine-southern-road-path-blend-midpoint-0.8x-tall.png) |
| 1.0x | [view](shrine-southern-road-path-blend-midpoint-1.0x-wide.png) | [view](shrine-southern-road-path-blend-midpoint-1.0x-tall.png) |
| 2.0x | [view](shrine-southern-road-path-blend-midpoint-2.0x-wide.png) | [view](shrine-southern-road-path-blend-midpoint-2.0x-tall.png) |

## Shrine exit

| Zoom | Wide | Tall |
| --- | --- | --- |
| 0.8x | [view](shrine-southern-road-path-blend-shrine-exit-0.8x-wide.png) | [view](shrine-southern-road-path-blend-shrine-exit-0.8x-tall.png) |
| 1.0x | [view](shrine-southern-road-path-blend-shrine-exit-1.0x-wide.png) | [view](shrine-southern-road-path-blend-shrine-exit-1.0x-tall.png) |
| 2.0x | [view](shrine-southern-road-path-blend-shrine-exit-2.0x-wide.png) | [view](shrine-southern-road-path-blend-shrine-exit-2.0x-tall.png) |

## Southern Road junction

| Zoom | Wide | Tall |
| --- | --- | --- |
| 0.8x | [view](shrine-southern-road-path-blend-junction-0.8x-wide.png) | [view](shrine-southern-road-path-blend-junction-0.8x-tall.png) |
| 1.0x | [view](shrine-southern-road-path-blend-junction-1.0x-wide.png) | [view](shrine-southern-road-path-blend-junction-1.0x-tall.png) |
| 2.0x | [view](shrine-southern-road-path-blend-junction-2.0x-wide.png) | [view](shrine-southern-road-path-blend-junction-2.0x-tall.png) |

## Reproduction and verification

```sh
/opt/homebrew/bin/python3 art-direction/production-proof/corrected/tools/build_shrine_southern_road_path_blend.py
godot --path game --script "$PWD/scripts/capture-shrine-southern-road-path-blend.gd"
/opt/homebrew/bin/python3 art-direction/production-proof/corrected/tools/check_shrine_southern_road_path_blend.py --check-live-baseline
```

The pinned record binds both accepted input textures, exact recipe, patch and all 18 RGBA
views. The patch has 3,354 visible pixels; seven captures differ only within its projection.
The validator reproduces the patch, checks its transparent gutter/clean alpha and
48px band, then independently projects its canvas using the ordinary camera limits. All
capture differences against the original correction must stay inside that projected canvas,
with a two-screen-pixel filtering allowance. All six junction views must show the patch.
It also validates the immutable original evidence and the optional 417-file live baseline.
Regular quality includes this new validator.

Session code/visual review found no further implementation issue after tightening capture
alpha validation. Repeated host Compatibility capture reproduced all 18 hashes; frozen live
baseline checks passed. Full host `./scripts/quality.sh` passed asset validators, formatting,
zero-warning build, all 5,033 tests, scenario validation, complete credential-free Godot smokes
and Compatibility launch. Initial graphical capture stalled before the junction views; the
restarted and repeated captures completed and matched. Owner visual acceptance remains pending.

Next bounded slice: obtain owner visual review of the blend, including its close texture
transition and ragged edges. After acceptance, propose an exact named promotion scope;
production assets, editable master, imports and runtime wiring need separate approval.
No pacing, usability, accessibility, gameplay, next-asset, release or signing acceptance is inferred.
