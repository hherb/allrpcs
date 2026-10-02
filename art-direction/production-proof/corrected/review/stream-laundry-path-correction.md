# Stream–Laundry external Laundry-only correction

This bounded external proof uses the unchanged 96x256 straight-path source along the retained
`(1800,500)` → `(1500,220)` visual route. Canvas 432x416, origin `(1424,152)`, center
`(1640,360)`, z=-75, identity scale/rotation. The disposable Main instance hides exactly one
matching 96px line and appends one external ImageTexture. It never packs or saves a scene.
All 450 authored runtime/production/import/physical baseline files remain unchanged; transient
Finder metadata is excluded. Accepted assets and earlier immutable proofs remain intact.

The Laundry east opening is `(1580,294.666667)`, width 96. The strip's nominal vertical
cross-section is 131.317021px. Its source has 296 nonzero-alpha samples outside the opening in
the 16px marker band, plus 9,538 samples entering the Laundry floor. These are source-support
measurements, not a human acceptance threshold.

The retained Stream crossing is `[1736,452]`–`[1864,548]`. Its north route crossing is
`(1748.571429,452)`, 51.428571px west of the bridge center. The strip has a nominal horizontal
cross-section of 140.696808px there. Of 6,029 source samples in the water-height region,
4,843 are inside the bridge x bounds and 1,186 outside them. Existing water/banks mask the
outside underlay; the translucent bridge includes its underlay. The north approach's visible
shoulder extends west of the crossing. Neither stage corrects or repositions this retained
bridge approach, water, banks or physical geometry; assess its fit in the views below.

The correction zeros source samples at x<=1580, plus samples at x<=1588 outside
y=294.666667±48. It removes exactly 9,680 nonzero-alpha samples; every surviving RGBA pixel
is exact. All 18 rendered views change only inside independently projected Laundry clip
bounds. All 6,029 Stream-overlap source samples are untouched. Both overlays retain clean
zero-alpha RGB and transparent canvas gutters.

Visual acceptance is pending; the [named promotion scope](stream-laundry-path-promotion-scope.md)
is unapproved. These external stages do not authorize runtime promotion.

## Laundry Entry

| Zoom | Wide | Tall |
| --- | --- | --- |
| 0.8x | [view](stream-laundry-path-correction-laundry-entry-0.8x-wide.png) | [view](stream-laundry-path-correction-laundry-entry-0.8x-tall.png) |
| 1.0x | [view](stream-laundry-path-correction-laundry-entry-1.0x-wide.png) | [view](stream-laundry-path-correction-laundry-entry-1.0x-tall.png) |
| 2.0x | [view](stream-laundry-path-correction-laundry-entry-2.0x-wide.png) | [view](stream-laundry-path-correction-laundry-entry-2.0x-tall.png) |

## Midpoint

| Zoom | Wide | Tall |
| --- | --- | --- |
| 0.8x | [view](stream-laundry-path-correction-midpoint-0.8x-wide.png) | [view](stream-laundry-path-correction-midpoint-0.8x-tall.png) |
| 1.0x | [view](stream-laundry-path-correction-midpoint-1.0x-wide.png) | [view](stream-laundry-path-correction-midpoint-1.0x-tall.png) |
| 2.0x | [view](stream-laundry-path-correction-midpoint-2.0x-wide.png) | [view](stream-laundry-path-correction-midpoint-2.0x-tall.png) |

## Stream Entry

| Zoom | Wide | Tall |
| --- | --- | --- |
| 0.8x | [view](stream-laundry-path-correction-stream-entry-0.8x-wide.png) | [view](stream-laundry-path-correction-stream-entry-0.8x-tall.png) |
| 1.0x | [view](stream-laundry-path-correction-stream-entry-1.0x-wide.png) | [view](stream-laundry-path-correction-stream-entry-1.0x-tall.png) |
| 2.0x | [view](stream-laundry-path-correction-stream-entry-2.0x-wide.png) | [view](stream-laundry-path-correction-stream-entry-2.0x-tall.png) |

## Validation and boundary

```sh
/opt/homebrew/bin/python3 art-direction/production-proof/corrected/tools/check_stream_laundry_path.py --check-live-baseline
```

Focused checks pin recipes, harnesses, source, renderer, records and all 38 images; reproduce
both overlays; verify every clipped/surviving sample, clean alpha/gutters and independently
projected capture changes; and optionally verify all 450 retained authored files. Repeat
captures reproduce all 36 archived PNG hashes. Full host quality passed asset validators, formatting, zero-warning build, all 5,033 tests,
scenario validation, the complete credential-free Godot smokes and Compatibility graphical
launch. Cumulative review clarified the bridge-overlap split and added an exact check for it;
no unresolved implementation issue remains.
Ordinary 3200x1800 camera limits remain enabled. HUD/Seeker hiding and framing are capture-only.

Preserve navigation/collision, markers, labels, camera, scenario, dialogue, clues, discovery,
treatment, save, provider, UI, audio, other assets, release and signing. Visual acceptance
establishes no pacing, usability, accessibility or gameplay acceptance.
