# Allrpcs Handover

## Current state

Phase 8 and Phase 9.1 remain complete on `main`. Direction A, painterly storybook gouache, remains
selected; Direction D is only the documented contingency. Phase 9.2 now has a deterministic
56-asset corrected pipeline and an isolated Godot import proof for four-direction walk,
idle/breathe, and generic reach-and-inspect animations. `Main.tscn` and ordinary gameplay reference
none of this art.

## Evidence and boundaries to retain

- `art-direction/production-proof/corrected/asset-manifest.json` owns 56 exact exports: terrain,
  path, plants, table/cues, 24 walk frames, eight idle/breathe frames, and 16 reach-and-inspect
  frames. Every entry records dimensions, 2x master geometry, pivot, baseline, digest, animation
  order/timing, and isolated-import status.
- `tools/build.py` reproducibly creates clean-alpha exports, seven layered OpenRaster masters, and
  review evidence. `tools/validate.py` proves seams, transparent gutters, grayscale cues, clearance,
  fixed direction-specific origins and 52-pixel baselines, unique frames, 120 ms walk loops, 600 ms
  idle loops, 160 ms non-looping interactions, and valid editable masters.
- The four idle and four interaction sources were made with Codex's built-in image tool using only
  the repository-owned walk proof as a reference. Exact prompts are retained. The tool exposed no
  model/version; no third-party content or licence claim was introduced.
- `game/art/direction-a-import-proof` contains exact copies of all 56 exports.
  `scripts/validate-direction-a-import-proof.py` proves source/manifest/import byte parity and
  lossless settings with mipmaps off, alpha-border correction on, and premultiplied alpha off.
- `DirectionAImportProof.tscn` owns four named walk, four idle, and four interaction animations. Its
  smoke switches through all 12 names and proves exact counts, timing, loop policy,
  direction-specific dimensions, 48 unique character texture resources, linear filtering, fixed
  origin, and absence of collision, navigation, input, gameplay, or UI authority.
- Host-local full quality passed the asset validators, zero-warning build, 2,839 Domain, 924
  Application, 1,261 Infrastructure, and 8 architecture tests, scenario validation, the complete
  credential-free Godot smoke sequence, and Compatibility graphical launch. The restricted run
  first hit the known `user://logs` crash; only the identical host-local command was retried.
- The push-triggered hosted workflow failed before product validation: all five jobs reported
  `steps: []`, with no runner steps or failed logs. Treat this as Actions/account infrastructure,
  not contradictory product evidence; the complete host-local quality evidence above remains the
  validation result for this handover.
- These remain proof assets, not final production quality or evidence of runtime animation
  selection, gameplay, collision, navigation, UI, accessibility, complete-region art, audio,
  release, or signing readiness.

## Exact next bounded slice

Extend only the external corrected pipeline with one modular 90-degree packed-earth path bend. Use
the existing quiet-moss and straight-path proofs as the only image references. Produce one
transparent 256x256 logical export and exact 512x512 editable master with a 96-pixel path entering
at the centered top edge and leaving at the centered right edge. Add manifest geometry, digest, and
source provenance; prove clean alpha, transparent outer regions, exact top-entry parity with the
straight path, compatible right-exit geometry, and one external review composition. Keep the bend
outside `game/` until those gates pass.

Do not add a junction, transition bank, water, ground variants, structures, other character art,
runtime animation selection, ordinary Seeker replacement, gameplay/UI/collision/navigation
authority, audio, release, or signing work. After this one bend passes externally, the following
slice may import only that exact manifest-bound asset into the isolated proof.
