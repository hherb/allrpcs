# Woodland–Shrine production/runtime result

The owner accepted the external junction correction ("Acceptable") and separately approved
its [named promotion](woodland-shrine-path-promotion-scope.md) ("approved") on 2026-10-02.
The ordinary runtime now uses one `WoodlandShrinePath` sprite under
`GreyboxNavigation/RoutePresentation`, center `(704,328)`, z=-74, identity scale/rotation,
with the exact accepted 704x208 PNG. Only the exact 96px visual line `(1000,350)` → `(380,300)`
is replaced. The corrected Clearing overlap, shrine clipping, adjacent Laundry path/end cap,
physical routes, boundaries, markers and labels remain intact.

Manifest `terrain.woodland_shrine_path`, production export, runtime PNG and isolated import
source have exact accepted SHA-256 `dd38db38f7a11d8d621ac938e822650c3b5a5d1be9ea190e60e39d101906d18c`.
The 1408x416 OpenRaster master has two visible, disjoint editable layers reconstructing the
nearest-neighbour 2x accepted export. Both imports are lossless and independently identified,
with mipmaps, alpha-border processing and premultiplication disabled. The production totals
are 80 exports and 31 editable masters; existing exports and masters are byte-identical.

**Result: all 18 runtime PNGs exactly match the accepted external junction views.** Repeated
Compatibility capture reproduces that parity. Ordinary startup retains its Seeker/HUD and
camera limits; review-only hiding and framing remain in the separate capture harness.
The owner accepted all 18 runtime views on 2026-10-02 ("accepted").

## Clearing junction

| Zoom | Wide | Tall |
| --- | --- | --- |
| 0.8x | [view](woodland-shrine-path-runtime-clearing-junction-0.8x-wide.png) | [view](woodland-shrine-path-runtime-clearing-junction-0.8x-tall.png) |
| 1.0x | [view](woodland-shrine-path-runtime-clearing-junction-1.0x-wide.png) | [view](woodland-shrine-path-runtime-clearing-junction-1.0x-tall.png) |
| 2.0x | [view](woodland-shrine-path-runtime-clearing-junction-2.0x-wide.png) | [view](woodland-shrine-path-runtime-clearing-junction-2.0x-tall.png) |

## Midpoint

| Zoom | Wide | Tall |
| --- | --- | --- |
| 0.8x | [view](woodland-shrine-path-runtime-midpoint-0.8x-wide.png) | [view](woodland-shrine-path-runtime-midpoint-0.8x-tall.png) |
| 1.0x | [view](woodland-shrine-path-runtime-midpoint-1.0x-wide.png) | [view](woodland-shrine-path-runtime-midpoint-1.0x-tall.png) |
| 2.0x | [view](woodland-shrine-path-runtime-midpoint-2.0x-wide.png) | [view](woodland-shrine-path-runtime-midpoint-2.0x-tall.png) |

## Shrine entry

| Zoom | Wide | Tall |
| --- | --- | --- |
| 0.8x | [view](woodland-shrine-path-runtime-shrine-entry-0.8x-wide.png) | [view](woodland-shrine-path-runtime-shrine-entry-0.8x-tall.png) |
| 1.0x | [view](woodland-shrine-path-runtime-shrine-entry-1.0x-wide.png) | [view](woodland-shrine-path-runtime-shrine-entry-1.0x-tall.png) |
| 2.0x | [view](woodland-shrine-path-runtime-shrine-entry-2.0x-wide.png) | [view](woodland-shrine-path-runtime-shrine-entry-2.0x-tall.png) |

## Verification

```sh
/opt/homebrew/bin/python3 art-direction/production-proof/corrected/tools/build.py
godot --headless --editor --path game --quit
godot --headless --path game --script "$PWD/scripts/check-woodland-shrine-runtime.gd"
godot --path game --script "$PWD/scripts/capture-woodland-shrine-path-runtime.gd"
/opt/homebrew/bin/python3 art-direction/production-proof/corrected/tools/validate_woodland_shrine_path.py --check-captures --check-promotion-scope
PATH=/opt/homebrew/bin:$PATH ./scripts/quality.sh
```

Focused validation binds proof bytes, editable layers, export/runtime/import parity,
independent UID/cache identity, source provenance and all 18 runtime captures. The new scope
record preserves 429 of the original 432 files verbatim; the three approved exceptions are
the manifest, `Main.tscn` and `GreyboxNavigation.cs`. Removing the one new manifest asset and
provenance or the one scene binding restores the exact prior bytes. Navigation/collision
bodies remain byte-identical. All earlier proof recipes, captures and evidence stay intact;
optional archived live-baseline checks intentionally reject this approved promotion.
The actual runtime smoke verifies decoded imported pixels, sprite placement and retained
Laundry join. Full host `./scripts/quality.sh` passed asset validators, formatting, zero-warning
build, all 5,033 tests, scenario validation, the complete credential-free Godot smoke suite
and Compatibility graphical launch. Cumulative review found and fixed the stale accessibility
manifest binding and old exact line/sprite smoke counts; no further implementation issue
remains. The final scope/capture check passes, and the diff is whitespace-clean.

The accessibility report's whole-manifest binding is refreshed for the added asset; its
review images and measurements are unchanged. No new accessibility acceptance is inferred.

Next boundary: reviewed direct-main publication, followed by the bounded external
Woodland Clearing–Village Edge feasibility proof under standing authorization. Preserve navigation, camera, scenario,
dialogue, clues, discovery, treatment, save, provider, UI, audio, release and signing.
