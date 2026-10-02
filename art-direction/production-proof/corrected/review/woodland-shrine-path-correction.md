# Woodland Clearing–Shrine external shrine-entry correction

This bounded pass follows the [failed unchanged-source strip](woodland-shrine-path-feasibility.md)
under the owner's standing authorization for routine external proof slices. Its 18 failed
views, overlay, source recipe and evidence remain immutable. The correction is external-only:
no production/runtime/import/manifest/physical/gameplay file changes and no saved Main instance.

The correction keeps the same 704x208 canvas, `(352,224)` world origin, `(704,328)` center,
z=-74 and exact finite route sampling. It removes pixels at x<620 (inside the shrine shell),
then clips the remaining 8px marker-depth band to the unchanged opening y=252..348. It removes
20,672 visible pixels and changes no surviving sampled pixel. Both gold markers, shell/floor,
labels, walls/collisions and all accepted adjacent path sprites remain unchanged.

**Result: ready for owner visual review.** The tinted interior path and opening-band fringe
are removed. The external texture meets the retained shrine opening without relocating it.
The clipping preserves the original offset physical route; it does not imply physical-path,
pacing, usability, accessibility or gameplay acceptance. Inspect the endpoint contour and
Clearing junction in the close views. A small brown notch remains at the Clearing join's
lower edge; its exact owner/cause is unconfirmed. This shrine-entry-only pass does not
change the two 2.0x Clearing-junction views. Review that retained detail explicitly before
acceptance; no production/runtime promotion is authorized.

All 18 targets/zooms/frames retain ordinary camera limits. Independent projection confines
all capture differences to the removed shrine-entry region with a two-screen-pixel filtering
allowance. Sixteen views differ; the two 2.0x Clearing-junction views are byte-identical to the
failed proof because the clipped region is outside their frame. Every shrine-entry view differs.

## Midpoint

| Zoom | Wide | Tall |
| --- | --- | --- |
| 0.8x | [view](woodland-shrine-path-correction-midpoint-0.8x-wide.png) | [view](woodland-shrine-path-correction-midpoint-0.8x-tall.png) |
| 1.0x | [view](woodland-shrine-path-correction-midpoint-1.0x-wide.png) | [view](woodland-shrine-path-correction-midpoint-1.0x-tall.png) |
| 2.0x | [view](woodland-shrine-path-correction-midpoint-2.0x-wide.png) | [view](woodland-shrine-path-correction-midpoint-2.0x-tall.png) |

## Clearing Junction

| Zoom | Wide | Tall |
| --- | --- | --- |
| 0.8x | [view](woodland-shrine-path-correction-clearing-junction-0.8x-wide.png) | [view](woodland-shrine-path-correction-clearing-junction-0.8x-tall.png) |
| 1.0x | [view](woodland-shrine-path-correction-clearing-junction-1.0x-wide.png) | [view](woodland-shrine-path-correction-clearing-junction-1.0x-tall.png) |
| 2.0x | [view](woodland-shrine-path-correction-clearing-junction-2.0x-wide.png) | [view](woodland-shrine-path-correction-clearing-junction-2.0x-tall.png) |

## Shrine Entry

| Zoom | Wide | Tall |
| --- | --- | --- |
| 0.8x | [view](woodland-shrine-path-correction-shrine-entry-0.8x-wide.png) | [view](woodland-shrine-path-correction-shrine-entry-0.8x-tall.png) |
| 1.0x | [view](woodland-shrine-path-correction-shrine-entry-1.0x-wide.png) | [view](woodland-shrine-path-correction-shrine-entry-1.0x-tall.png) |
| 2.0x | [view](woodland-shrine-path-correction-shrine-entry-2.0x-wide.png) | [view](woodland-shrine-path-correction-shrine-entry-2.0x-tall.png) |

## Ordinary camera clamping

At 0.8x, midpoint and shrine-entry centers clamp to `(800,450)` in wide frames and
`(800,516.875)` in tall frames; clearing-junction centers retain x=1000 with those same y
values. At 1.0x, all y centers clamp to 360 (wide) or 413.5 (tall); shrine-entry x clamps to
640 while midpoint x=690 and clearing-junction x=1000 remain. At 2.0x all requested centers
remain unchanged. The ordinary 3200x1800 world limits are never overridden.

## Reproduction and evidence

```sh
/opt/homebrew/bin/python3 art-direction/production-proof/corrected/tools/build_woodland_shrine_path_correction.py
godot --path game --script "$PWD/scripts/capture-woodland-shrine-path-correction.gd"
/opt/homebrew/bin/python3 art-direction/production-proof/corrected/tools/check_woodland_shrine_path_correction.py --check-live-baseline
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

Next boundary: obtain owner visual review of this correction. Routine external correction
scope approvals are no longer required; named production/runtime promotion still needs
accepted external views and separate approval. Preserve navigation, camera, scenario,
dialogue, clues, discovery, treatment, save, provider, UI, audio, release and signing.
