# Woodland Clearing–Shrine failed external strip proof

The owner approved the [unchanged-source test scope](woodland-shrine-path-feasibility-scope.md)
on 2026-10-02 and removed repeated proposal approval for routine bounded external proof work.
This first test renders the unchanged 96x256 straight-path source along the exact retained
`(1000,350)` → `(380,300)` route. It hides only that route's visual line in a disposable Main
instance and adds one 704x208 overlay at world origin `(352,224)`, center `(704,328)`, z=-74.
The instance is never saved. Production/runtime/manifest/import/physical files remain unchanged.

**Result: failed feasibility evidence, not a promotion candidate.** The route crosses the
shrine boundary 19.354839px below its unchanged opening center. The overlay has 229 visible
samples outside the 96px-high opening within its 16px-wide marker band. The strip also shows
through the translucent shrine floor as a tinted interior path. The woodland/shrine bounds
retain their 20px x-overlap. Do not alter the openings, shell or physical route to repair this.
All source pixels, labels, markers, accepted adjacent paths and outlines retain their owners.

The following [bounded external correction](woodland-shrine-path-correction.md) preserves
these 18 failed views, overlay, recipe and pinned evidence as immutable.

## Midpoint

| Zoom | Wide | Tall |
| --- | --- | --- |
| 0.8x | [view](woodland-shrine-path-feasibility-midpoint-0.8x-wide.png) | [view](woodland-shrine-path-feasibility-midpoint-0.8x-tall.png) |
| 1.0x | [view](woodland-shrine-path-feasibility-midpoint-1.0x-wide.png) | [view](woodland-shrine-path-feasibility-midpoint-1.0x-tall.png) |
| 2.0x | [view](woodland-shrine-path-feasibility-midpoint-2.0x-wide.png) | [view](woodland-shrine-path-feasibility-midpoint-2.0x-tall.png) |

## Clearing Junction

| Zoom | Wide | Tall |
| --- | --- | --- |
| 0.8x | [view](woodland-shrine-path-feasibility-clearing-junction-0.8x-wide.png) | [view](woodland-shrine-path-feasibility-clearing-junction-0.8x-tall.png) |
| 1.0x | [view](woodland-shrine-path-feasibility-clearing-junction-1.0x-wide.png) | [view](woodland-shrine-path-feasibility-clearing-junction-1.0x-tall.png) |
| 2.0x | [view](woodland-shrine-path-feasibility-clearing-junction-2.0x-wide.png) | [view](woodland-shrine-path-feasibility-clearing-junction-2.0x-tall.png) |

## Shrine Entry

| Zoom | Wide | Tall |
| --- | --- | --- |
| 0.8x | [view](woodland-shrine-path-feasibility-shrine-entry-0.8x-wide.png) | [view](woodland-shrine-path-feasibility-shrine-entry-0.8x-tall.png) |
| 1.0x | [view](woodland-shrine-path-feasibility-shrine-entry-1.0x-wide.png) | [view](woodland-shrine-path-feasibility-shrine-entry-1.0x-tall.png) |
| 2.0x | [view](woodland-shrine-path-feasibility-shrine-entry-2.0x-wide.png) | [view](woodland-shrine-path-feasibility-shrine-entry-2.0x-tall.png) |

## Ordinary camera clamping

At 0.8x, midpoint and shrine-entry centers clamp to `(800,450)` in wide frames and
`(800,516.875)` in tall frames; clearing-junction centers retain x=1000 with those same y
values. At 1.0x, all y centers clamp to 360 (wide) or 413.5 (tall); shrine-entry x clamps to
640 while midpoint x=690 and clearing-junction x=1000 remain. At 2.0x all requested centers
remain unchanged. The ordinary 3200x1800 world limits are never overridden.

## Reproduction and evidence

```sh
/opt/homebrew/bin/python3 art-direction/production-proof/corrected/tools/build_woodland_shrine_path_feasibility.py
godot --path game --script "$PWD/scripts/capture-woodland-shrine-path-feasibility.gd"
/opt/homebrew/bin/python3 art-direction/production-proof/corrected/tools/check_woodland_shrine_path_feasibility.py --check-live-baseline
```

The evidence binds source/recipe/capture hashes, the unchanged shared renderer, exact RGBA
views, route/opening geometry and a 432-file frozen live baseline. Regular quality validates
archived evidence without freezing future approved promotions. Both validators are registered
in `scripts/quality.sh`; focused reproduction, clean-alpha, clipping and live-baseline checks
passed. Repeated host Compatibility captures reproduced all 36 feasibility/correction PNG
hashes. Full host `./scripts/quality.sh` passed asset validators, formatting, zero-warning
build, all 5,033 tests, scenario validation, the complete credential-free Godot smoke suite
and Compatibility launch. Cumulative review found no further code issue; the retained
Clearing-junction notch remains an explicit visual-review detail. The frozen live-baseline
check passed after captures and quality; no production/runtime/import/physical change occurred.
