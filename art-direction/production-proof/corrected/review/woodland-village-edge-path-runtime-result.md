# Woodland Clearing–Village Edge production/runtime result

The owner accepted the 18 corrected external views and separately approved the
[named promotion](woodland-village-edge-path-promotion-scope.md) on 2026-10-03 ("approved").
The ordinary scene now uses one `WoodlandVillageEdgePath` sprite under
`GreyboxNavigation/RoutePresentation`, center `(1352,632)`, z=-75, identity scale/rotation.
It is the final sibling, preserving the proof's ordering against other z=-75 lines. Only
the exact 96px visual route `(1700,900)` → `(1000,350)` is omitted; all other route children
retain their relative order. Physical/navigation authority and accepted adjacent assets remain intact.

`terrain.woodland_village_edge_path`, its production export, runtime PNG and isolated import
source retain the exact accepted 816x688 PNG, SHA-256
`ce4cdb9aed6494490ebfba4efa33d38aff950856ae60e7af735e5149ddcb062c`.
The 1632x1376 OpenRaster master has two disjoint visible editable layers reconstructing the
nearest-neighbour 2x export. Independent lossless imports disable mipmaps, alpha-border
processing and premultiplication. Totals are 81 exports and 32 masters; existing exports
and masters are byte-identical.

**All 18 runtime PNGs exactly match the accepted external correction.** The owner confirmed all runtime views on 2026-10-03 ("confirmed"). Seeker/HUD hiding and framing remain capture-only;
ordinary startup retains its normal presentation and camera limits.

## Clearing junction

| Zoom | Wide | Tall |
| --- | --- | --- |
| 0.8x | [view](woodland-village-edge-path-runtime-clearing-junction-0.8x-wide.png) | [view](woodland-village-edge-path-runtime-clearing-junction-0.8x-tall.png) |
| 1.0x | [view](woodland-village-edge-path-runtime-clearing-junction-1.0x-wide.png) | [view](woodland-village-edge-path-runtime-clearing-junction-1.0x-tall.png) |
| 2.0x | [view](woodland-village-edge-path-runtime-clearing-junction-2.0x-wide.png) | [view](woodland-village-edge-path-runtime-clearing-junction-2.0x-tall.png) |

## Midpoint

| Zoom | Wide | Tall |
| --- | --- | --- |
| 0.8x | [view](woodland-village-edge-path-runtime-midpoint-0.8x-wide.png) | [view](woodland-village-edge-path-runtime-midpoint-0.8x-tall.png) |
| 1.0x | [view](woodland-village-edge-path-runtime-midpoint-1.0x-wide.png) | [view](woodland-village-edge-path-runtime-midpoint-1.0x-tall.png) |
| 2.0x | [view](woodland-village-edge-path-runtime-midpoint-2.0x-wide.png) | [view](woodland-village-edge-path-runtime-midpoint-2.0x-tall.png) |

## Village entry

| Zoom | Wide | Tall |
| --- | --- | --- |
| 0.8x | [view](woodland-village-edge-path-runtime-village-entry-0.8x-wide.png) | [view](woodland-village-edge-path-runtime-village-entry-0.8x-tall.png) |
| 1.0x | [view](woodland-village-edge-path-runtime-village-entry-1.0x-wide.png) | [view](woodland-village-edge-path-runtime-village-entry-1.0x-tall.png) |
| 2.0x | [view](woodland-village-edge-path-runtime-village-entry-2.0x-wide.png) | [view](woodland-village-edge-path-runtime-village-entry-2.0x-tall.png) |

## Verification and boundary

```sh
/opt/homebrew/bin/python3 art-direction/production-proof/corrected/tools/build.py
godot --headless --editor --path game --quit
godot --headless --path game --script "$PWD/scripts/check-woodland-village-edge-runtime.gd"
godot --path game --script "$PWD/scripts/capture-woodland-village-edge-path-runtime.gd"
/opt/homebrew/bin/python3 art-direction/production-proof/corrected/tools/validate_woodland_village_edge_path.py --check-captures --check-promotion-scope
PATH=/opt/homebrew/bin:$PATH ./scripts/quality.sh
```

Focused checks bind recipe/accepted pixels, editable master, export/runtime/import bytes,
independent UIDs/cache paths and all 18 runtime captures. The promotion scope record preserves
438 files from the 438-file proof baseline plus three supplemental Village port owners,
excluding only the approved manifest, Main scene and navigation presentation owner. Removing
one asset/provenance or scene binding restores the prior exact manifest/scene bytes;
navigation/collision bodies are byte-identical. Earlier proof stages and evidence are intact;
optional archived live-baseline checks intentionally reject the approved promotion.
The actual runtime smoke verifies decoded source parity, final sibling order, placement,
absence of the old visual line and retained Woodland–Shrine/Laundry presentation.

The accessibility report refreshes only its whole-manifest hash; review images and measurements
are unchanged. The ground smoke's exact counts now reflect four lines and eleven sprites.
Full host quality passed asset validators, formatting, zero-warning build, all 5,033 tests,
scenario validation, the complete credential-free Godot smoke suite and Compatibility graphical
launch. Repeated runtime captures reproduce all 18 exact PNG hashes. Cumulative review corrected
a stale clean-alpha asset count and capture diagnostic wording; no implementation issue remains.

Next slice: bounded external Village Edge–Stream feasibility after reviewed direct-main publication. Preserve
navigation, camera, scenario, dialogue, clues, discovery, treatment, save, provider, UI, audio,
other assets, release and signing. Acceptance establishes no gameplay or accessibility acceptance.
