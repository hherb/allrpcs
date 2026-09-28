# Allrpcs Handover

## Current state

Phase 8 and Phase 9.1 remain complete on `main`. Direction A, painterly storybook gouache, remains
selected; Direction D is only the documented contingency. Phase 9.2 now has a deterministic
66-asset corrected pipeline with seventeen editable OpenRaster masters. The terrain proof includes
quiet moss, repeatable woodland leaf litter, the straight, two outer-corner, and two inset-corner
quiet-moss/leaf-litter transitions, and the complete straight/bend/T/cross/end-cap packed-earth
set. Exact copies of all 66 exports exist only in the isolated Godot import proof; `Main.tscn` and
ordinary gameplay reference none of this art.

## Evidence and boundaries to retain

- `asset-manifest.json` owns all 66 exports with source provenance, exact 2x geometry, pivots,
  baselines, digests, animation timing, and isolated-import status. `tools/build.py` reproducibly
  creates seventeen `.ora` masters and all review evidence from eighteen retained image-tool
  sources plus the earlier repository-owned walk sheet.
- `terrain.woodland_leaf_litter_inset_corner_transition_left` is derived deterministically from the
  exact quiet-moss, leaf-litter, and straight-transition masters. Its opaque 256x256 export and
  512x512 master place quiet moss in the upper-left, retain exact top/left transition connectors,
  and retain exact leaf-litter right/lower edges. The layered master keeps all three sources hidden
  and editable.
- Its 768x768 review uses only exact source and transition tiles. Focused validation checks the
  material orientation, all four edge contracts, exact connected review placements, master layers,
  manifest provenance and digest, and full opacity. The connected review was also inspected
  visually; no independent human art or accessibility judgement was supplied.
- `game/art/direction-a-import-proof` contains exact byte copies of all 66 exports with lossless
  imports, exact resource/cache sidecar paths, unique UIDs, mipmaps off, alpha-border correction on,
  and premultiplied alpha off. The proof scene checks both inset corners as uncentred 256x256 sprites
  with linear filtering and no collision, navigation, input, gameplay, or UI authority.
- Host-local `./scripts/quality.sh` passes both asset validators, all 66 exact import hashes,
  zero-warning build, 2,839 Domain, 924 Application, 1,261 Infrastructure, and 8 architecture tests,
  scenario validation, every credential-free Godot smoke, formatting, and the Compatibility
  graphical launch. The restricted full run reached the known `user://logs` crash only after all
  non-Godot stages passed; the identical full command was retried on the host.
- These remain pipeline proofs, not final production quality or evidence of runtime terrain
  assembly, animation selection, collision, navigation, gameplay/UI, complete-region art,
  accessibility, audio, release, or signing readiness.

## Exact next bounded slice

Add one deterministic external-only colour-vision and contrast review for the existing terrain
family: quiet moss, woodland leaf litter, packed earth, and all five quiet-moss/leaf-litter
transitions. Document one named, reproducible simulation method and its parameters; render
side-by-side original, grayscale, protanopia, deuteranopia, and tritanopia views; and add focused
validation that binds every view to the exact current manifest exports and records measurable
luminance/texture separation without treating numeric thresholds as human acceptance. Keep the
review outside `game/`; do not add or alter asset IDs, masters, exports, import copies, or runtime
scene references.

Do not add new ground types, transition shapes, paths, banks, water, structures, props, character
art, ordinary Seeker replacement, runtime terrain assembly or animation selection,
gameplay/UI/collision/navigation authority, audio, release, or signing work. After the external
review passes, the following slice may extend the same documented simulation method only to the
existing object-state cues and Seeker samples; it must still avoid claiming complete-region or human
accessibility acceptance.
