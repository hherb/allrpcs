# Allrpcs Handover

## Current state

Phase 8 and Phase 9.1 remain complete on `main`. Direction A, painterly storybook gouache, remains
selected; Direction D is only the documented contingency. Phase 9.2 now has a deterministic
40-asset corrected proof, an isolated four-direction walk import resource, and an external
four-direction idle/breathe proof. `Main.tscn` and ordinary gameplay reference none of this art.

## Evidence and boundaries to retain

- `art-direction/production-proof/corrected/asset-manifest.json` owns 40 exact exports: terrain,
  path, plants, table/cues, 24 walk frames, and eight idle/breathe frames. Every entry records exact
  dimensions, 2x master geometry, contact baseline, pivot, SHA-256 digest, and import-proof status.
- `tools/build.py` reproducibly creates clean-alpha exports, six layered OpenRaster masters, review
  evidence, and four directional walk and idle/breathe loops. `tools/validate.py` proves seams,
  transparent gutters, grayscale cues, clearance, unique frames, fixed 52-pixel contact baselines,
  120 ms walks, 600 ms idle/breathe loops, and valid editable masters.
- The four idle sources were made with Codex's built-in image tool using only the repository-owned
  walk sheet as a reference. Exact prompts are retained. The tool exposed no model/version; no
  third-party content or licence claim was introduced.
- Exactly 32 manifest entries are marked for the isolated Godot import proof. The eight idle frames
  remain external. `scripts/validate-direction-a-import-proof.py` proves byte parity and lossless
  import settings without allowing the external idle set to leak into `game/`.
- `DirectionAImportProof.tscn` owns `walk_south`, `walk_west`, `walk_east`, and `walk_north` as
  separate six-frame loops. Its smoke switches through every name and proves exact frame counts,
  direction-specific dimensions, 24 unique texture resources, looping, 120 ms timing, linear
  filtering, fixed origin, and absence of collision, navigation, input, gameplay, or UI authority.
- Host-local full quality passed the asset validators, zero-warning build, 2,839 Domain, 924
  Application, 1,261 Infrastructure, and 8 architecture tests, scenario validation, the complete
  credential-free Godot smoke sequence, and Compatibility graphical launch. The restricted run
  first hit the known `user://logs` crash; only the identical host-local command was retried.
- These remain proof assets, not final production quality or evidence of gameplay, collision,
  navigation, UI, accessibility, complete-region art, audio, release, or signing readiness.

## Exact next bounded slice

Extend only the isolated import proof to the eight existing idle/breathe exports. Copy those exact
manifest-bound bytes under `game/art/direction-a-import-proof`, mark only those entries as included
in the import proof, and add four named two-frame loops: `idle_south`, `idle_west`, `idle_east`, and
`idle_north`. Retain 600 ms timing, direction-specific dimensions, the 52-pixel contact baseline,
smooth filtering, unique texture resources, and the existing inertness checks. Keep the proof
disconnected from `Main.tscn`.

Do not replace the ordinary Seeker, add runtime facing or animation selection, alter movement or
gameplay authority, add collision/navigation/UI, create interaction gestures, expand other art,
switch to Direction D, or begin audio, release, or signing work. After the isolated idle import
passes, the following slice may create one external south-facing four-frame interaction proof
before any four-direction interaction expansion or runtime use.
