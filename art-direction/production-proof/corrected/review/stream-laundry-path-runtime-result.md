# Stream–Laundry production/runtime result

The owner confirmed the external views, retained bridge fit and separate
[named promotion](stream-laundry-path-promotion-scope.md) on 2026-10-03 ("confirmed").
One ordinary `StreamLaundryPath` sprite replaces only the 96px visual line
`(1800,500)` → `(1500,220)`, centered `(1640,360)`, z=-75, identity scale/rotation.
It is the final route child, immediately after Woodland–Village Edge and Village Edge–Stream
in their accepted relative order. All other route children retain their order. Startup
validates exact route and texture geometry; physical navigation/collision remain unchanged.

`terrain.stream_laundry_path` retains the exact accepted 432x416 PNG, SHA-256
`6422441f218b6ff0062269f55583ef488a0d3c877787d0b7892a4605950b2185`.
The 864x832 editable OpenRaster master reconstructs the nearest-neighbour 2x export with
separate editable layers. Runtime and isolated proof copies have independent UIDs/cache paths
and lossless imports, with mipmaps, alpha-border processing and premultiplication disabled.
Totals: 83 exports and 34 masters; every existing export/master/import source remains exact.

**All 18 runtime PNGs exactly match accepted external correction PNGs**, including repeat
captures. The owner accepted the path in principle and explicitly authorized publication on 2026-10-03.
Western stream continuity and any additional crossing are separate work, not a publication gate. All 6,029 Stream-overlap
source samples are retained: 4,843 inside bridge x bounds and 1,186 masked by water/banks.
The accepted north shoulder/approach remains unchanged. Bridge polygon/color/opacity and
water/bank owners remain intact. No bridge approach correction was made. Camera framing and
HUD/Seeker hiding remain capture-only, outside ordinary startup.

## Laundry Entry

| Zoom | Wide | Tall |
| --- | --- | --- |
| 0.8x | [view](stream-laundry-path-runtime-laundry-entry-0.8x-wide.png) | [view](stream-laundry-path-runtime-laundry-entry-0.8x-tall.png) |
| 1.0x | [view](stream-laundry-path-runtime-laundry-entry-1.0x-wide.png) | [view](stream-laundry-path-runtime-laundry-entry-1.0x-tall.png) |
| 2.0x | [view](stream-laundry-path-runtime-laundry-entry-2.0x-wide.png) | [view](stream-laundry-path-runtime-laundry-entry-2.0x-tall.png) |

## Midpoint

| Zoom | Wide | Tall |
| --- | --- | --- |
| 0.8x | [view](stream-laundry-path-runtime-midpoint-0.8x-wide.png) | [view](stream-laundry-path-runtime-midpoint-0.8x-tall.png) |
| 1.0x | [view](stream-laundry-path-runtime-midpoint-1.0x-wide.png) | [view](stream-laundry-path-runtime-midpoint-1.0x-tall.png) |
| 2.0x | [view](stream-laundry-path-runtime-midpoint-2.0x-wide.png) | [view](stream-laundry-path-runtime-midpoint-2.0x-tall.png) |

## Stream Entry

| Zoom | Wide | Tall |
| --- | --- | --- |
| 0.8x | [view](stream-laundry-path-runtime-stream-entry-0.8x-wide.png) | [view](stream-laundry-path-runtime-stream-entry-0.8x-tall.png) |
| 1.0x | [view](stream-laundry-path-runtime-stream-entry-1.0x-wide.png) | [view](stream-laundry-path-runtime-stream-entry-1.0x-tall.png) |
| 2.0x | [view](stream-laundry-path-runtime-stream-entry-2.0x-wide.png) | [view](stream-laundry-path-runtime-stream-entry-2.0x-tall.png) |

## Verification and boundary

```sh
/opt/homebrew/bin/python3 art-direction/production-proof/corrected/tools/validate_stream_laundry_path.py --check-captures --check-promotion-scope
godot --headless --path game --script "$PWD/scripts/check-stream-laundry-runtime.gd"
godot --path game --script "$PWD/scripts/capture-stream-laundry-path-runtime.gd"
PATH=/opt/homebrew/bin:$PATH ./scripts/quality.sh
```

Focused validation binds exact recipe/export/runtime/import/master pixels, provenance,
independent lossless imports and all 18 runtime PNGs. The new promotion record retains
447 authored files from the frozen 450-file baseline, excluding only the approved manifest,
Main scene and navigation presentation owner. Transient Finder metadata is absent. Removing
one asset/provenance and scene additions restores the prior exact manifest/scene bytes;
physical/navigation method bodies remain byte-identical. Both external stages and older
immutable records remain intact; historical optional live-baseline/scope checks intentionally
reject later approved promotions.

The actual runtime smoke checks decoded pixels, transform/geometry/order, old-line absence,
consecutive accepted siblings and actual bridge polygon/color/opacity. Both prior order
smokes retain decoded pixel/placement checks with their approved consecutive three-sprite
order. Ground counts are two lines and thirteen sprites, preserving the total. Accessibility
metadata refreshes only the whole-manifest hash; review images/measurements are unchanged.
Full host quality passed asset validators, formatting, zero-warning build, all 5,033 tests,
scenario validation, complete credential-free Godot smokes and Compatibility graphical launch.
The sequential repeat capture reproduces all 18 exact PNG hashes. Cumulative review finds no
further implementation issue.

Next slice: a separate external western watercourse/topology proposal, including any additional
path crossings. This issue does not block publication of the accepted path promotion. Preserve
navigation, camera, scenario, dialogue, clues, discovery, treatment, save, provider, UI, audio,
other assets, release and signing. Acceptance establishes no gameplay or accessibility acceptance.
