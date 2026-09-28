# Allrpcs Handover

## Current state

Phase 8 and Phase 9.1 remain complete on `main`. Direction A, painterly storybook gouache, remains
selected; Direction D is only the documented contingency. Phase 9.2 now has a deterministic
58-asset corrected pipeline. The terrain proof includes quiet moss, a 96-pixel straight packed-earth
path, one 90-degree bend, and one T-junction with matching ports. All 58 exports are present only in
the isolated Godot import proof; `Main.tscn` and ordinary gameplay reference none of this art.

## Evidence and boundaries to retain

- `art-direction/production-proof/corrected/asset-manifest.json` owns 58 exact exports with source
  provenance, geometry, 2x master dimensions, pivots, baselines, digests, animation timing, and
  isolated-import status. `tools/build.py` reproducibly creates nine editable OpenRaster masters
  and all review evidence.
- `terrain.path_t_junction` is one transparent 256x256 export with an exact 512x512 master. Its
  centered top, left, and right ports are 96 pixels wide and retain full 32-pixel logical
  continuations from the straight path. The entire lower 64-pixel band and every unused edge region
  remain transparent. `review/path-t-junction-review.png` composes it only with quiet moss and
  vertical/rotated copies of the straight module.
- The T-junction source was made with Codex's built-in image tool using only repository-owned quiet
  moss, straight-path, and path-bend exports as references. The exact prompt is retained. The tool
  exposed no model/version; no third-party content or licence claim was introduced.
- `game/art/direction-a-import-proof` contains exact copies of all 58 exports.
  `scripts/validate-direction-a-import-proof.py` proves source/manifest/import byte parity and
  lossless settings with mipmaps off, alpha-border correction on, and premultiplied alpha off.
- `DirectionAImportProof.tscn` checks the T-junction as one uncentred 256x256 texture alongside the
  prior terrain, props, cues, and 12 named animations. Its freshly built smoke proves linear
  filtering and absence of collision, navigation, input, gameplay, or UI authority.
- Host-local `./scripts/quality.sh` passes both asset validators, zero-warning build, 2,839 Domain,
  924 Application, 1,261 Infrastructure, and 8 architecture tests, scenario validation, the full
  credential-free Godot smoke sequence, and Compatibility graphical launch. The restricted run
  first hit the known `user://logs` crash; only the identical host-local command was retried.
- These remain proof assets, not final production quality or evidence of runtime terrain assembly,
  animation selection, gameplay, collision, navigation, UI, accessibility, complete-region art,
  audio, release, or signing readiness.

## Exact next bounded slice

Extend only the external corrected pipeline with one modular packed-earth cross-junction. Use the
existing quiet-moss, straight-path, path-bend, and T-junction proofs as the only image references.
Produce one transparent 256x256 logical export and exact 512x512 editable master with centered
96-pixel paths entering at all four edges. Add manifest geometry, digest, and source provenance;
prove clean alpha, transparent outer corners, full 32-pixel connector parity at all four ports, and
one external review composition. Keep the cross-junction outside `game/` until those gates pass.

Do not add a transition bank, water, ground variants, structures, character art, runtime terrain
assembly or animation selection, ordinary Seeker replacement, gameplay/UI/collision/navigation
authority, audio, release, or signing work. After this one cross-junction passes externally, the
following slice may import only that exact manifest-bound asset into the isolated proof.
