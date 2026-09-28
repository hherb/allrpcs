# Allrpcs Handover

## Current state

Phase 8 and Phase 9.1 remain complete on `main`. Direction A, painterly storybook gouache, remains
selected; Direction D is only the documented contingency. Phase 9.2 now has a deterministic
57-asset corrected pipeline. The terrain proof includes quiet moss, a 96-pixel straight packed-earth
path, and one 90-degree bend with matching ports. All 57 exports are present only in the isolated
Godot import proof; `Main.tscn` and ordinary gameplay reference none of this art.

## Evidence and boundaries to retain

- `art-direction/production-proof/corrected/asset-manifest.json` owns 57 exact exports with source
  provenance, geometry, 2x master dimensions, pivots, baselines, digests, animation timing, and
  isolated-import status. `tools/build.py` reproducibly creates eight editable OpenRaster masters
  and all review evidence.
- `terrain.path_bend` is one transparent 256x256 export with an exact 512x512 master. Its centered
  top and right ports are 96 pixels wide and retain a full 32-pixel logical continuation from the
  straight path. The left edge, bottom edge, unused port edges, top-right corner, and bottom-left
  corner remain transparent. `review/path-bend-review.png` composes it only with quiet moss and
  rotated/vertical copies of the straight module.
- The bend source was made with Codex's built-in image tool using only the repository-owned quiet
  moss and straight path as references. The exact prompt is retained. The tool exposed no
  model/version; no third-party content or licence claim was introduced.
- `game/art/direction-a-import-proof` contains exact copies of all 57 exports.
  `scripts/validate-direction-a-import-proof.py` proves source/manifest/import byte parity and
  lossless settings with mipmaps off, alpha-border correction on, and premultiplied alpha off.
- `DirectionAImportProof.tscn` checks the bend as one uncentred 256x256 texture alongside the prior
  terrain, props, cues, and 12 named animations. Its smoke proves linear filtering and absence of
  collision, navigation, input, gameplay, or UI authority.
- Host-local `./scripts/quality.sh` passed both asset validators, zero-warning build, 2,839 Domain,
  924 Application, 1,261 Infrastructure, and 8 architecture tests, scenario validation, the full
  credential-free Godot smoke sequence, and Compatibility graphical launch. The restricted run
  first hit the known `user://logs` crash; only the identical host-local command was retried.
- The latest hosted workflow before this change failed before product validation with all five jobs
  reporting `steps: []`; treat that as Actions/account infrastructure and re-query the final head,
  not as contradictory product evidence.
- These remain proof assets, not final production quality or evidence of runtime terrain assembly,
  animation selection, gameplay, collision, navigation, UI, accessibility, complete-region art,
  audio, release, or signing readiness.

## Exact next bounded slice

Extend only the external corrected pipeline with one modular packed-earth T-junction. Use the
existing quiet-moss, straight-path, and path-bend proofs as the only image references. Produce one
transparent 256x256 logical export and exact 512x512 editable master with a 96-pixel path entering
at the centered top edge and leaving at the centered left and right edges. Add manifest geometry,
digest, and source provenance; prove clean alpha, transparent lower outer regions and bottom edge,
full 32-pixel connector parity at all three ports, and one external review composition. Keep the
junction outside `game/` until those gates pass.

Do not add a cross-junction, transition bank, water, ground variants, structures, character art,
runtime terrain assembly or animation selection, ordinary Seeker replacement,
gameplay/UI/collision/navigation authority, audio, release, or signing work. After this one
junction passes externally, the following slice may import only that exact manifest-bound asset
into the isolated proof.
