# Village Edge–Stream external Village-only correction

This bounded external proof uses the unchanged 96x256 straight-path source along the retained
`(1700,900)` → `(1800,500)` visual route. Canvas 224x528, world origin `(1640,440)`, sprite
center `(1752,704)`, z=-75, identity scale/rotation. Its disposable Main instance hides exactly
one matching 96px line and appends one external ImageTexture. It never packs or saves a scene.
Runtime, production, imports, physical navigation, markers, camera and gameplay remain unchanged.
The frozen 451-file baseline includes accepted Woodland Clearing–Village Edge assets and the
Village/Stream geometry, presentation and layout owners.

The Village north opening is `(1750,700)`, width 96. The normal-width 96px strip has a nominal
98.954535px horizontal cross-section. Sample counting finds one nonzero-alpha source sample
outside the opening in its 16px marker band, but that sample is inside the opaque floor.
All 17,406 source samples at y>=700 are hidden by that floor. The Stream crossing remains
`[1736,452]`–`[1864,548]`; 4,477 strip samples lie beneath its existing translucent bridge.
The actual bridge appearance includes its retained underlay; these samples are not fully masked.

The correction zeros samples at y>=700, plus samples at y>=692 outside x=1750±48.
It removes exactly 17,406 nonzero-alpha source samples. Every surviving RGBA pixel is exact,
including the entire bridge underlay. **All 18 correction PNGs exactly match the initial
feasibility PNGs.** This is a source-containment correction with no visible difference in the
tested views. Both stages retain clean zero-alpha RGB and transparent canvas gutters.

Visual acceptance is pending. This evidence does not authorize production/runtime promotion.
The [named promotion scope](village-edge-stream-path-promotion-scope.md) remains unapproved.

## Village Entry

| Zoom | Wide | Tall |
| --- | --- | --- |
| 0.8x | [view](village-edge-stream-path-correction-village-entry-0.8x-wide.png) | [view](village-edge-stream-path-correction-village-entry-0.8x-tall.png) |
| 1.0x | [view](village-edge-stream-path-correction-village-entry-1.0x-wide.png) | [view](village-edge-stream-path-correction-village-entry-1.0x-tall.png) |
| 2.0x | [view](village-edge-stream-path-correction-village-entry-2.0x-wide.png) | [view](village-edge-stream-path-correction-village-entry-2.0x-tall.png) |

## Midpoint

| Zoom | Wide | Tall |
| --- | --- | --- |
| 0.8x | [view](village-edge-stream-path-correction-midpoint-0.8x-wide.png) | [view](village-edge-stream-path-correction-midpoint-0.8x-tall.png) |
| 1.0x | [view](village-edge-stream-path-correction-midpoint-1.0x-wide.png) | [view](village-edge-stream-path-correction-midpoint-1.0x-tall.png) |
| 2.0x | [view](village-edge-stream-path-correction-midpoint-2.0x-wide.png) | [view](village-edge-stream-path-correction-midpoint-2.0x-tall.png) |

## Stream Entry

| Zoom | Wide | Tall |
| --- | --- | --- |
| 0.8x | [view](village-edge-stream-path-correction-stream-entry-0.8x-wide.png) | [view](village-edge-stream-path-correction-stream-entry-0.8x-tall.png) |
| 1.0x | [view](village-edge-stream-path-correction-stream-entry-1.0x-wide.png) | [view](village-edge-stream-path-correction-stream-entry-1.0x-tall.png) |
| 2.0x | [view](village-edge-stream-path-correction-stream-entry-2.0x-wide.png) | [view](village-edge-stream-path-correction-stream-entry-2.0x-tall.png) |

## Validation and boundaries

```sh
/opt/homebrew/bin/python3 art-direction/production-proof/corrected/tools/check_village_edge_stream_path.py --check-live-baseline
```

The validator pins recipes, capture harnesses, renderer, source pixels, records and 38 images;
it reproduces both overlays, checks each clipped/surviving sample, source measurements,
transparent gutters, exact 18-view parity and optionally all 451 baseline hashes. Repeat captures reproduce all 36 archived PNG hashes. Full host quality passes asset validators,
formatting, zero-warning build, all 5,033 tests, scenario validation, the complete credential-free
Godot smoke suite and Compatibility graphical launch. Cumulative review clarified the bridge
underlay and the next promotion's prior-sprite order check; no unresolved implementation issue. Ordinary 3200x1800 camera
limits remain enabled; HUD/Seeker hiding and framing remain capture-only.

Preserve earlier evidence and accepted assets, navigation/collision, labels, scenario, dialogue,
clues, discovery, treatment, save, provider, UI, audio, release and signing. Visual acceptance
establishes no pacing, usability, accessibility or gameplay acceptance.
