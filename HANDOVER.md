# Allrpcs Handover

## Current state

Phase 8 and Phase 9.1 remain complete on `main`. Direction A, painterly storybook gouache, remains
selected; Direction D is only the documented contingency. Phase 9.2 has a deterministic 59-asset
corrected pipeline. Its modular terrain proof now includes quiet moss, a 96-pixel straight
packed-earth path, a 90-degree bend, a T-junction, and a cross-junction with matching ports. Exact
copies of all 59 exports exist only in the isolated Godot import proof; `Main.tscn` and ordinary
gameplay reference none of this art.

## Evidence and boundaries to retain

- `art-direction/production-proof/corrected/asset-manifest.json` owns all 59 exports with source
  provenance, geometry, exact 2x master dimensions, pivots, baselines, digests, animation timing,
  and isolated-import status. `tools/build.py` reproducibly creates ten editable OpenRaster masters
  and all review evidence.
- `terrain.path_cross_junction` is one transparent 256x256 export with an exact 512x512 master. Its
  centered top, bottom, left, and right ports are 96 pixels wide and preserve full 32-pixel logical
  straight-path connectors. All four outer corners remain transparent, and
  `review/path-cross-junction-review.png` proves the module between four exact straight copies.
- The cross source was made with Codex's built-in image tool using only repository-owned quiet-moss,
  straight-path, path-bend, and T-junction exports as references. The exact prompt is retained. The
  tool exposed no model/version; no third-party content or licence claim was introduced.
- `game/art/direction-a-import-proof` contains exact byte copies of all 59 exports with lossless
  imports, mipmaps off, alpha-border correction on, and premultiplied alpha off.
  `DirectionAImportProof.tscn` checks the cross as one uncentred 256x256 texture; its fresh smoke
  proves linear filtering and no collision, navigation, input, gameplay, or UI authority.
- Host-local `./scripts/quality.sh` passes both asset validators, zero-warning build, 2,839 Domain,
  924 Application, 1,261 Infrastructure, and 8 architecture tests, scenario validation, every
  credential-free Godot smoke, formatting, and Compatibility graphical launch. The restricted run
  first hit the known Godot settings/`user://logs` failure; only the identical command was retried.
- These are pipeline proofs, not final production quality or evidence of runtime terrain assembly,
  animation selection, gameplay, collision, navigation, UI, accessibility, complete-region art,
  audio, release, or signing readiness.

## Exact next bounded slice

Extend only the external corrected pipeline with one modular packed-earth path end-cap. Use the
existing quiet-moss, straight-path, path-bend, T-junction, and cross-junction exports as the only
image references. Produce one transparent 256x256 logical export and exact 512x512 editable master
with one centered 96-pixel top entry that ends in a soft rounded worn-earth terminus before the
lower half. Add manifest geometry, digest, and source provenance; prove clean alpha, transparent
side and bottom edges, a transparent lower 64-pixel band, full 32-pixel top-connector parity, and
one external review composition. Keep the end-cap outside `game/` until those gates pass.

Do not add transition banks, water, ground variants, structures, character art, runtime terrain
assembly or animation selection, ordinary Seeker replacement, gameplay/UI/collision/navigation
authority, audio, release, or signing work. After this end-cap passes externally, the following
slice may import only that exact manifest-bound asset into the isolated proof.
