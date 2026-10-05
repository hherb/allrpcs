# Village Edge feather-16 runtime scope proposal

Named runtime approval is pending. The owner approved feather-16 **at all three
entrances** on 2026-10-06: [exact artwork selection](../village-edge-ground-feasibility/owner-selection.json).
The selection is complete and must not be requested again. This packet installs
no runtime assets or scene changes. [Source bindings](source-bindings.json) freeze
this proposal and its selected inputs against main `e186182c`.

Promote exactly one combined underlay, `terrain.village_edge_entrance_ground`,
from `village-edge-ground-feasibility/proof-inputs/feather-16-layer.png`.
Keep its source bytes, alpha and three disjoint entrance supports unchanged.
Baseline, hard support, feather-24 and raw world paint remain external diagnostics.

| Property | Proposed value |
| --- | --- |
| Owner | Independent root `Node2D`, `VillageEdgeGroundPresentation`, no script |
| Owner transform | Position `(1480,620)`, z=0, relative depth, scale1, rotation0 |
| Child | One `PackedEarth` Sprite2D, 500x390, local position/offset `(0,0)` |
| Child transform | centered=false, scale1, rotation0, white modulation, no region |
| Effective depth | -78, above woodland transitions -80/-79 and below routes -75/-70 and shell |
| Sampling | Linear, repeat disabled; source world phase `(0,0)`, period128 |
| Envelope | `(1480,620)-(1980,1010)`, right/bottom exclusive |
| Coverage | Three audited exterior alpha segments, radius24 support, inward16px feather |
| Root order | After accepted Laundry roots, before final `WholeChannelPresentation` |

Do not crop, repad, recolour, repaint, rotate, resample or split this asset. Preserve
all route alpha/transforms, the retained Village Edge shell/openings/labels and
all physical owners. The selected support's 2px filtering guard has zero overlap
with all five accepted channel sprites and seven accepted ground sprites.

## Allowed implementation after named approval

Copy selected bytes to `village-edge-entrance-ground.png` in existing production,
isolated-proof and runtime terrain directories. Copy the selected 1000x780 master
unchanged to `production-proof/corrected/masters/village-edge-entrance-ground.ora`.
Append one manifest asset/provenance block through a new scoped
`production-proof/corrected/tools/build_village_edge_ground.py`, preserving all
existing entries and masters: **96 assets / 47 masters become 97 / 48**.

Add independent lossless proof/runtime `.import` sidecars with distinct new
UIDs/cache paths. Compression0; no mipmaps, alpha-border fixing, premultiplication,
channel remapping or size limit. Prove exact decoded runtime RGBA from an empty
disposable cache and proof UID independence. Preserve all old imports byte-exact.

Scene edits are limited to one texture resource, the independent owner/child and
one inactive environment-gated `RuntimeVillageEdgeGroundSmokeCheck` node/resource
in `game/Main.tscn`, its C# script and `.uid`. Preserve every old resource/node
block and sibling order. Add one isolated sample/resource in
`DirectionAImportProof.tscn` and bounded live-copy parity in its import validator.

New tooling is limited to `scripts/validate-village-edge-ground-runtime.py`,
`scripts/capture-village-edge-ground-runtime.gd` and `.uid`, and a separate
`village-edge-ground-runtime-promotion/` authorization/baseline/evidence packet.
Allow bounded `scripts/quality.sh` orchestration and continuation documentation.
No other production file is selected; extensions require an updated named scope.

Freeze the actual starting main head after named approval. Keep frozen inventories
and validators unchanged. A successor must prove all predecessor bytes and only
these additions. Run frozen checks in their accepted checkout when their closed
membership gates reject successor-only external files; record that distinction
and bind full quality evidence to unchanged inputs. Runtime/product changes
require fresh build/tests/scenario/smokes/formatting and scoped successor checks.
Do not weaken a validator or treat a membership rejection as a product test result.

## Preservation and acceptance

Preserve all accepted ground owners, quiet moss, route paint/geometry, Garden
sibling order, Village Edge physical boundaries/opening centers and labels.
WholeChannel remains final: five sprites, native three-node x<1664 shader scope,
UVs/caps/crossing, Shrine triangle, Clearing near side, gate/Forester and
x=1664/x=2000 limits. No physics/navigation/layout/scenario/gameplay/dialogue/
clue/discovery/treatment/save/provider/functional-UI/audio/release/signing changes.

Require append-only manifest/master parity, exact old scene blocks, predecessor
preservation, import independence and unchanged selected paint/mask. Use disposable
rejection probes for wrong variant/phase/depth/transform, duplicate/dropped owner,
altered paint/coverage/import processing and physical/route changes. Run scoped
successor checks, cold-import verification and required fresh product quality;
distinguish sandbox failures from exact host retries.

Capture ordinary Main using the **actual imported runtime texture**, without an
external proof sprite. Require exact external/runtime RGBA parity for nine
selected views and nine paired baselines: center `(1700,840)`, 0.8x/1x/2x in wide
1280x720 and tall1280x827, plus dedicated 2x wide detail views centered at
Woodland `(1556,800)`, Stream `(1750,680)` and Square `(1844,900)`.
Retain camera clamps and identical disposable actor/HUD settings. Compare all
changed GPU pixels against guarded support, including translucent edge rows.

Present all nine actual runtime views for separate owner runtime confirmation
before publishing a runtime promotion. Named scope approval alone does not grant
that later visual confirmation or broader accessibility/pacing/terrain acceptance.

## Current verification and next slice

Fresh consolidated asset/master/board/baseline/support checks pass. Every published
product and CI input is unchanged; preceding passing zero-warning/5,033-test evidence
is retained. No fresh full-suite run is claimed. Historical pending metadata in
the immutable comparison is superseded by the exact new owner-selection record.
[Verification](verification.json) and [session review](session-review.json) record
this checkpoint. All existing ground/channel/Laundry approvals remain closed.

Approve this named runtime scope, then install only the unchanged selected asset
under it. No ordinary-scene promotion is authorized yet.

```sh
python3 art-direction/village-edge-ground-runtime-scope/check.py
python3 art-direction/village-edge-ground-runtime-scope/check.py --check-rejections
```
