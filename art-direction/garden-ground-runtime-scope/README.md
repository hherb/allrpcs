# Garden ground presentation runtime scope

Status: **proposal; named runtime approval absent**. Prepared on 2026-10-04
against `bc64aac87eda8d0a1f013f7e1a76a6226dada375`.
[Source bindings](source-bindings.json) pin the selected composite, its editable
master, masks, transparent underlays, preview captures and live presentation inputs.
The owner's [external variant selection](../garden-ground-feasibility/owner-selection.json)
does not approve this runtime scope or the subsequent GPU previews.

## Exact proposed promotion

Approve the two named assets below together under one independent root-level
`Node2D`, `GardenGroundPresentation`, at world origin **(2048,1120)**. The owner
has no script, identity, interaction, collision or navigation responsibility.
Its z index is 0; child depths are relative and therefore equal their world depths.
Each child is a native **512x384** `Sprite2D`, local position/offset `(0,0)`,
`centered=false`, unit scale, zero rotation, white modulation, no region,
linear filtering and repeat disabled. No full opaque reference image is imported.

| Asset ID / child | Export and runtime filename | Effective z | Exact PNG SHA-256 |
| --- | --- | --- | --- |
| `terrain.garden_ground_meadow` / `Meadow` | `garden-ground-meadow.png` | -90 | `a8cd9cc627c48048ec5cea9e080fff9990e37a0247f2e55f7cdc8fda848e770b` |
| `terrain.garden_ground_soil` / `Soil` | `garden-ground-soil.png` | -89 | `8570244b7e30ade4493d58a5be495a5f9717a4d0a4393fe951ca5a5524f425fb` |

Copy the selected `proof-inputs/meadow-layer.png` and `soil-layer.png` bytes
unchanged. Proposed copies live in production `exports/terrain/`,
`game/art/direction-a-import-proof/terrain/` and `game/art/runtime/terrain/`;
the binding file gives every full path. Never sample from a screenshot,
crop padding, rotate/resize the fields, change alpha or add a second soil paint.
Both fields retain absolute-world phase `(0,0)` with 128px periods; the existing
quiet-moss ground keeps its 256px period and z=-100.

Soil follows the exact union of the two retained bed alpha silhouettes within
`(2196,1275)`–`(2384,1355)`. Meadow uses the selected 32px apron within
`(2164,1243)`–`(2416,1387)`, including its original rounded/feathered coverage.
These bounds are exclusive at right/bottom, describe paint only, and grant no
cultivation polygon or reusable transition module. The soil changes just 886
visible native pixels, mostly translucent edges/shadows; about 88% of its coverage
is occluded. **Retain it anyway:** the selected composite includes both layers.
Removing it would require a separate selection, not an implementation shortcut.

## Imports, editable masters and allowed files

Append exactly two production manifest entries, preserving the existing 89 assets
and 40 masters. The resulting counts are 91 assets and 42 masters. New isolated
masters are `masters/garden-ground-meadow.ora` and `garden-ground-soil.ora`,
1024x768, nearest-neighbour 2x of the selected native layers, with separate editable
field paint, coverage and an empty Corrections layer. Check their merged pixels
against exact 2x source layers and their native exports against the hashes above.
Record their new container hashes after derivation. The selected composite master
`masters/meadow-apron.ora` remains immutable; do not import it as an underlay.

Use independent lossless proof/runtime imports, compression mode 0, no mipmaps,
alpha-border fixing, premultiplication, channel remapping or size limit.
The binding file specifies exact parameter values. Generate fresh independent
UIDs and cache destinations per copy; do not borrow retained terrain UIDs.
Prove regeneration with an empty import cache and no dependence on proof UIDs.

After approval, permitted implementation changes are limited to:

- A new production `tools/build_garden_ground.py`, two manifest entries and
  bounded Garden provenance; the two exports and two new masters above.
- The four named proof/runtime PNG copies and their four `.import` files.
- Two new texture resources and the new presentation owner/children in
  `game/Main.tscn`; retain every existing node block and relative sibling order.
- Two isolated samples in `game/DirectionAImportProof.tscn`; the associated
  manifest/import checks in `scripts/validate-direction-a-import-proof.py`.
- A new `scripts/validate-garden-ground-runtime.py`, an environment-gated
  `game/RuntimeGardenGroundSmokeCheck.cs` and its Main scene resource/node,
  its new Godot `.uid` sidecar, plus invocation in `scripts/quality.sh`.
  The smoke is inactive during
  ordinary play, following the existing herb-bed smoke convention.
- A separate explicit `scripts/capture-garden-ground-runtime.gd`, its `.uid`, and new
  `art-direction/garden-ground-runtime-promotion/` evidence package.
- Current README/ROADMAP/HANDOVER continuation guidance and runtime-art documentation.

No existing general art builder/validator is weakened to accept this successor.
Historical checks run on their pinned trees when their input hashes predate an
approved scene change; retain their records and prove the complete preserved subset.
Any additional implementation file or visual adjustment requires an updated scope.

## Draw order and retained authority

| Owner/layer | Effective z / order |
| --- | --- |
| Existing quiet-moss ground | -100, unchanged |
| New Garden meadow, soil | -90 then -89 |
| Retained Square–Garden / Square–Sickhouse paths | -71, existing sibling order |
| Retained Village Square junction | -70 |
| Existing two beds, six foliage sprites, preparation table and Garden label | 0, unchanged Garden child order |

Keep `OpeningRegionRemedyPreparationSceneSource` at `(2300,1300)` with its exact
binding, access, distance rule, sprites, offsets and label. It remains the sole
Garden station owner; the ground node does not become its parent or source of
placement/gameplay facts. Do not change `GreyboxNavigation`, layout anchors,
route geometry, physical openings, collision shapes, navigation cells or inputs.
Keep all retained texture bytes, Garden node blocks and relative sibling ordering.
No actor/HUD hiding, camera reframing, capture trigger or proof path enters ordinary
startup. The explicit capture script may change only a disposable scene instance.

Preserve all other accepted terrain, channel/Stream ownership, native UVs, crossing,
caps, five-sprite channel composition, Shrine triangle, Clearing near side,
gate/Forester and x=1664/x=2000 transitions. No scenario, gameplay, dialogue,
clue, discovery, treatment, save, provider, functional UI, audio, release or signing
changes. No claim of terrain completeness, human accessibility or pacing acceptance.

## Validation and remaining gate

The proposal creates no product files, imports or runtime copies. Garden
`build.py --check` and `inspect_package.py`, the accepted tiling inspector and
compatibility `review_check.py` pass on the preparation head. Existing Garden
evidence remains immutable, including rejected dark mats and all six GPU previews.
[Session review](session-review.json) records proposal/source verification.
Unchanged full product suites are not replayed for this external proposal.

Recheck the frozen proposal inputs from the repository root without rewriting them:

```sh
/opt/homebrew/bin/python3 - <<'PY'
import hashlib, json
from pathlib import Path
scope = json.loads(Path('art-direction/garden-ground-runtime-scope/source-bindings.json').read_text())
for name, expected in scope['source_sha256'].items():
    if hashlib.sha256(Path(name).read_bytes()).hexdigest() != expected:
        raise SystemExit('Proposal source drift: ' + name)
print('PASS: all', len(scope['source_sha256']), 'bound sources unchanged')
PY
```

The next slice starts **only after approval of this README and its binding digest**:
freeze a complete predecessor inventory at the actual starting head, reject source
drift, derive/install exactly the two assets, and prove exports/master/import parity,
phase/depth, independent ownership and unchanged physical/Garden state. Include
rejection checks for modified source/masks, dropped soil, wrong phase/depth,
duplicate owner, import processing and changes outside the approved scope.

Run `./scripts/quality.sh` and bounded runtime/import checks. Add six actual runtime
views centred at `(2300,1300)` at 0.8x/1x/2x in 1280x720 and 1280x827 viewports,
with paired candidate-disabled baselines under identical disposable capture settings.
Bind all capture inputs and outputs; verify differences stay within the selected
envelopes with a documented linear-filter allowance, and inspect path/bed/foliage
edges at full size. For 1x/2x wide views also compare with the retained external GPU
previews, reporting rendering differences without silently altering selected pixels.
These are proposed acceptance criteria; no new capture parity is claimed here.

Present those exact ordinary-scene views for **separate owner runtime visual
confirmation** before calling the promotion accepted or publishing that runtime
change. Approval of this scope does not itself provide that confirmation. If the
sources or proposed placement must change, prepare a corrected packet first.
