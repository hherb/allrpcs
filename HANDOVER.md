# Allrpcs Handover

## Current state

Phase 8 and Phase 9.1 remain complete on `main`. Direction A, painterly storybook gouache, remains
selected; Direction D is only the documented contingency. Phase 9.2 now has a deterministic
60-asset corrected pipeline. Its modular packed-earth path set includes the quiet-moss base,
96-pixel straight, 90-degree bend, T-junction, cross-junction, and rounded end-cap. All 60 exports
have exact copies only in the isolated Godot import proof; `Main.tscn` and ordinary gameplay
reference none of this art.

## Evidence and boundaries to retain

- `art-direction/production-proof/corrected/asset-manifest.json` owns all 60 exports with source
  provenance, geometry, exact 2x master dimensions, pivots, baselines, digests, animation timing,
  and isolated-import status. `tools/build.py` reproducibly creates eleven editable OpenRaster
  masters and all review evidence from seventeen retained image-tool sources plus the earlier
  repository-owned walk sheet.
- `terrain.path_end_cap` is one transparent 256x256 export with an exact 512x512 master, one
  centered 96-pixel top port, a rounded terminus before the lower half, transparent side/bottom
  edges, and a transparent lower 64-pixel band. Its exact prompt and repository-only references
  are retained; the built-in image tool exposed no model/version.
- `review/path-end-cap-review.png` proves the end-cap below one exact straight copy.
  `review/path-module-topology-review.png` deterministically connects the straight, bend,
  T-junction, cross-junction, and end-cap using only exact exports and rotations.
- `game/art/direction-a-import-proof` contains exact byte copies of all 60 exports with lossless
  imports, mipmaps off, alpha-border correction on, and premultiplied alpha off.
  `DirectionAImportProof.tscn` checks the end-cap as one uncentred 256x256 texture; its fresh smoke
  proves linear filtering and no collision, navigation, input, gameplay, or UI authority.
- Host-local `./scripts/quality.sh` passes both asset validators, all 60 exact import hashes,
  zero-warning build, 2,839 Domain, 924 Application, 1,261 Infrastructure, and 8 architecture
  tests, scenario validation, every credential-free Godot smoke, formatting, and Compatibility
  graphical launch. The restricted run first hit the known Godot settings/`user://logs` failure;
  only the identical full command was retried. An earlier focused retry exposed a missing editor
  import and stale assembly; both were corrected before the final full pass.
- These remain pipeline proofs, not final production quality or evidence of runtime terrain
  assembly, animation selection, gameplay, collision, navigation, UI, accessibility,
  complete-region art, audio, release, or signing readiness.

## Exact next bounded slice

Extend only the external corrected pipeline with one repeatable woodland leaf-litter ground tile.
Use the existing quiet-moss and packed-earth path set as the only image references. Produce one
opaque 256x256 logical export and exact 512x512 editable master with muted brown leaf litter,
restrained low-frequency gouache texture, and exact left/right plus top/bottom edge parity. Add
manifest geometry, digest, and source provenance; prove all four seams in a 2x2 external review,
useful opaque coverage, no path-like corridor, and readable contrast beside quiet moss and the
packed-earth modules. Keep it outside `game/` until those gates pass.

Do not add transitions, banks, water, structures, props, character art, runtime terrain assembly
or animation selection, ordinary Seeker replacement, gameplay/UI/collision/navigation authority,
audio, release, or signing work. After the leaf-litter tile passes externally, the following slice
may import only that exact manifest-bound asset into the isolated proof.
