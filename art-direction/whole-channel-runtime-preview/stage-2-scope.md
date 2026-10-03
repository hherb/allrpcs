# Proposed whole-channel Stage 2 production/runtime promotion

Status: **awaiting named owner approval**. Stage 1 implementation approval is recorded;
no owner acceptance of these runtime views or Stage 2 implementation is inferred. Assess
[the runtime preview](README.md), including the x=1664/x=2000 sampling transitions, and
approve this scope to implement these exact outputs. No seam correction is included.
The [specification](stage-2-specification.json) binds every full SHA-256 and the evidence.

## Exact assets and imports

Add only these five PNG exports and exact nearest 2x OpenRaster masters:

| Asset ID | PNG | Master | Effective z |
| --- | --- | --- | --- |
| `terrain.whole_channel_water` | `channel-water.png` | `terrain-whole-channel-water.ora` | -68 |
| `terrain.whole_channel_banks` | `channel-banks.png` | `terrain-whole-channel-banks.ora` | -67 |
| `terrain.whole_channel_decks` | `channel-decks.png` | `terrain-whole-channel-decks.ora` | -66 |
| `terrain.whole_channel_detail` | `channel-detail.png` | `terrain-whole-channel-detail.ora` | -66 |
| `terrain.whole_channel_fringe` | `channel-fringe.png` | `terrain-whole-channel-fringe.ora` | -65 |

Every PNG retains 2604×620 pixels, origin `(-64,212)`, centred pivot, position `(1238,522)`,
identity transform and no repeat. Each master is 5208×1240. Production becomes 89 exports
and 40 masters; preserve the existing 84 exports/35 masters exactly. Extend only the existing
production builder/manifest/provenance for deterministic reproduction from frozen recipes.
Copy matching manifest-bound PNGs into production `exports/terrain/`, isolated proof art
and `game/art/runtime/terrain/`. Independent proof/runtime imports must decode to the exact
Stage 1 RGBA bytes; disable mipmaps, alpha-border processing and premultiplication, retaining
linear terrain filtering. Preview UIDs/import-cache identities must not be reused.
Do not promote the opaque CPU reconstruction, comparison road/crossing layers or capture UI.

## Bounded presentation wiring

Add one dedicated `WholeChannelPresentation` Node2D owner/C# script as a final root child
in `Main.tscn`, after the existing Stream scene source has initialized. Add five texture
bindings and the [exact overlap shader](exports/channel-retained-overlap.gdshader) binding.
Use unique resource IDs; the current available range starts after `435_runtime_closed_forest_path`.
The owner creates exactly five sprite children in water → banks → decks → detail → fringe
order, enforcing exact dimensions, transforms, source identity and z values at startup.
Keep the existing `GreyboxNavigation/RoutePresentation` subtree, children and order unchanged.

Apply the exact fragment shader as a shared material only to the existing StreamWater,
NorthWestBank and SouthWestBank presentation nodes. Discard x<1664 fragments to remove only
the duplicate western join footprint. Retain their original polygons, UVs, regions, transforms,
caps and actual crossing colour. Validate those inputs once before applying the material;
fail closed if geometry, texture/source ownership or an existing material differs. Every other
Stream node retains its original presentation. The shader changes no physics or UV sampling.

Do not edit `OpeningRegionStreamLaundrySceneSource`, `OpeningRegionStreamLaundryGeometry`,
physical bindings/layouts, navigation construction, collision polygons/shapes, outer boundaries,
Clearing/gate or Forester behavior. Do not edit existing route helper methods or approved assets.
All new water/decks are visual only; walking beneath the new water remains possible in the
unchanged physical model. Fixing that behavior, B1/B2 fringe actor clearance or a visible
sampler seam would require a separately named scope/correction.

## Checks, capture checkpoint and publication

Add isolated import-proof presentation/checks for the five new entries, plus a focused
ordinary runtime-tree smoke for exact placement/order, shader targets, retained source
geometry/materials, decoded pixels and unchanged physics. Keep existing terrain/route/Stream/
forest smokes and assertions. Extend production/runtime validators only to recognize these
five assets; preserve old evidence/report bytes, historical inventories and pinned source hashes.

Freeze a complete new baseline at the approved Stage 1 successor head. Permit only the
five production asset/master/import sets, builder/manifest, new shader/owner/script bindings,
focused validation/smoke/capture wiring and continuation documentation. For affected old
validators use a new successor wrapper that verifies their preserved subsets and reproduces
historical checks in pinned trees; never rewrite old inventories to accept promotion changes.
Integrate the new focused validator/smoke into `quality.sh`/`smoke-godot.sh` without dropping
existing runtime/navigation/forest engine checks.

Create a separate capture script for actual ordinary runtime, keeping all capture-only HUD/
Seeker hiding, frozen processing and camera framing out of startup. Capture the same 37 views
as Stage 1 and require exact decoded RGBA equality with all 37 candidate PNGs under equivalent
Visibility/framing. Bind current source/import/recipe inputs and reject stale captures. Run
`./scripts/quality.sh`, master/export/import parity, UID/cache independence, restricted-change
checks and cumulative review, including translucent joins and both full-width deck ends.

Present the exact resulting runtime views and obtain owner runtime visual confirmation
before publishing the promotion. Preserve scenario, dialogue, clues, discovery, treatment,
saves, providers, UI, audio, release/signing and all prior accepted runtime art, including the
closed-side-only forest texture and accepted Shrine triangle. This scope grants no gameplay,
pacing, usability, accessibility, layout or physical acceptance.
