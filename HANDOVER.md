# Allrpcs Handover

## Current state

Continue from live `main`. Phase 8 and 9.1 are complete. Approved frozen packages:
[whole-channel runtime](art-direction/whole-channel-runtime-promotion/owner-acceptance.json)
(89 exports/40 masters, 37 runtime views), [woodland](art-direction/woodland-environment-batch/README.md)
and [structures](art-direction/structures-batch/README.md) (42 each),
[characters](art-direction/characters-batch/owner-acceptance.json) (456 active new pairs,
48 inherited cloak frames, including 288 polished walks), and
[objects](art-direction/objects-batch/owner-acceptance.json) (42). Do not rebuild these.
Preserve original/failed character and walking snapshots as lineage; do not sum them
into active counts. Except whole-channel, runtime integration remains separate.

## Exact next slice

Collectively review the complete [UI artwork series](art-direction/ui-batch/README.md):
36 candidates across journal/map/inventory/dialogue/settings/provider-state, editable
2x OpenRaster masters, six labelled boards and two 1280x720 export-scale compositions.
**Owner approval is pending.** Revise from feedback while retaining superseded evidence,
then record exact-hash owner acceptance and consolidated scoped approval checks.
Do not infer approval from publication. After acceptance, the next category is the entire
weather/lighting/discovery/treatment/disease-state effects series, following the same
whole-category artwork/review workflow. Residual terrain work remains open.

## Boundaries

External artwork only; no functional UI, runtime/collision/navigation/layout/scenario/
gameplay/dialogue/clue/discovery/treatment/save/provider/audio/release/signing changes.
Keep blank content fields; artwork establishes no scenario or efficacy facts. Dimensions,
anchors, text fitting, accessibility and interaction remain provisional. Preserve Direction A
paint detail, editable 2x masters, all source sheets and rejected evidence. Routine bounded
external proof approvals are waived; named runtime promotion and runtime visual approval
remain separate. Preserve five-sprite channel ownership, shader only on three native Stream
nodes at x<1664, native UVs/polygons/regions/caps/crossing, route sibling order, untextured
Clearing near side, gate/Forester, Shrine triangle and x=1664/x=2000 transitions. Water/decks
remain visual only; walking-under-water and fringe-clearance limitations remain.

## Evidence for continuation

UI `inspect_package.py` passes source/hash coverage, crop accounting, transparent padding,
OpenRaster layers and exact source/master/export parity. `session-review.json` records
review fixes and byte reproduction. Faint distant pixels below alpha 8 can be excluded
by recorded crops with an eight-source-pixel margin; unchanged sources are retained.
Product/runtime/CI inputs remain unchanged from `2fcc849`; retain
[full-suite evidence](art-direction/woodland-environment-batch/validation.json): zero-warning/
error build, 5,033 tests and Godot/Compatibility checks. Do not replay unchanged product
suites for external artwork. The frozen whole-channel validator rejects later art folders;
never weaken it. Runtime changes require exact promotion scope and full validation.
Keep capture controls out of startup and exclude `.DS_Store`.
