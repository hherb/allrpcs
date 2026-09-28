# Allrpcs Handover

## Current state

Phase 8 and Phase 9.1 remain complete on `main`. Direction A, painterly storybook gouache, remains
selected; Direction D is only the documented contingency. Phase 9.2 now has a deterministic
65-asset corrected pipeline with sixteen editable OpenRaster masters. The terrain proof includes
quiet moss, repeatable woodland leaf litter, exact straight, two outer-corner, and one inset-corner
quiet-moss/leaf-litter transitions, and the complete straight/bend/T/cross/end-cap packed-earth set.
Exact copies of all 65 exports exist only in the isolated Godot import proof; `Main.tscn` and
ordinary gameplay reference none of this art.

## Evidence and boundaries to retain

- `asset-manifest.json` owns all 65 exports with source provenance, exact 2x geometry, pivots,
  baselines, digests, animation timing, and isolated-import status. `tools/build.py` reproducibly
  creates sixteen `.ora` masters and all review evidence from eighteen retained image-tool sources
  plus the earlier repository-owned walk sheet.
- `terrain.woodland_leaf_litter` is one opaque 256x256 export with an exact 512x512 two-layer master.
  Its opposing edges match exactly; focused validation proves useful tonal variation, measurable
  contrast from quiet moss and packed earth, and no 96-pixel full-span path-like corridor.
- The leaf source was made with Codex's built-in image tool using only the repository-owned quiet
  moss and packed-earth topology review. The exact prompt is retained; the tool exposed no
  model/version, and no third-party content or licence claim was introduced.
- `terrain.woodland_leaf_litter_transition` is derived deterministically from the exact moss and leaf
  masters. Its 256x256 export is fully opaque, retains exact 32-pixel source connector regions, and
  repeats exactly top-to-bottom. The 512x512 `.ora` retains both sources as hidden editable layers.
- The two outer corners turn that boundary toward the right and left while retaining exact moss on
  unused edges. The first inset corner keeps exact leaf litter on unused left/lower edges. Three
  768x768 reviews prove the corners against exact source and rotated-transition neighbours.
- Validation now compares all RGBA channels for exact-region claims. This caught and fixed the old
  export step's one-pixel left/right averaging; the straight transition now genuinely retains both
  full 32-pixel source connectors, and all dependent external/import hashes were refreshed.
- `game/art/direction-a-import-proof` contains exact byte copies of all 65 exports with lossless
  imports, exact resource/cache sidecar paths, unique UIDs, mipmaps off, alpha-border correction on,
  and premultiplied alpha off. The proof scene checks all ground transitions as uncentred 256x256
  sprites with linear filtering and no collision, navigation, input, gameplay, or UI authority.
- Host-local `./scripts/quality.sh` passes both asset validators, all 65 exact import hashes,
  zero-warning build, 2,839 Domain, 924 Application, 1,261 Infrastructure, and 8 architecture tests,
  scenario validation, every credential-free Godot smoke, formatting, and the Compatibility
  graphical launch. The restricted full run reached the known `user://logs` crash only after all
  non-Godot stages passed; the identical full command was retried on the host.
- Hosted `Credential-free CI` run `36395942633` at asset head `65faacf` failed before executing any
  product command: all five jobs reported `steps: []`. This is Actions/account infrastructure state,
  not contradictory product validation or development-build evidence.
- These remain pipeline proofs, not final production quality or evidence of runtime terrain
  assembly, animation selection, collision, navigation, gameplay/UI, accessibility, complete-region
  art, audio, release, or signing readiness.

## Exact next bounded slice

Extend only the external corrected pipeline with the complementary quiet-moss inset corner in leaf
litter as `terrain.woodland_leaf_litter_inset_corner_transition_left`. Produce one opaque 256x256
logical export and exact 512x512 editable master whose boundary enters at the top midpoint and exits
at the left midpoint, with quiet moss in the upper-left and leaf litter on the right and lower edges.
Derive it from the existing exact ground and straight-transition masters; retain exact top/left
transition connectors, exact unused leaf-litter outer edges, one connected 768x768 review, manifest
provenance, digest, and focused validator coverage. Keep it outside `game/` until those external
gates pass.

Do not add further ground types, other transition shapes, banks, water, structures, props,
character art, runtime terrain assembly or animation selection, ordinary Seeker replacement,
gameplay/UI/collision/navigation authority, audio, release, or signing work. After the complementary
inset passes externally, the following slice may import only that exact manifest-bound asset into
the isolated proof.
