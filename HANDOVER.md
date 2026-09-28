# Allrpcs Handover

## Current state

Phase 8 and Phase 9.1 remain complete on `main`. Direction A, painterly storybook gouache, remains
selected; Direction D is still only the documented contingency. Three bounded Phase 9.2 slices are
complete: the failed sheet workflow was corrected into individual manifest-bound assets, all four
six-frame Seeker walk directions were normalized, and all corrected exports received an isolated
Godot import proof. `Main.tscn` and ordinary gameplay still reference no production-proof art.

## Evidence and boundaries to retain

- `art-direction/production-proof/corrected/asset-manifest.json` owns 32 exact exports: ground,
  straight path, herb bed, foliage, table, three separate cue overlays, and 24 Seeker frames. Every
  entry records its logical/export rectangle, 2x master dimensions, contact baseline, pivot, and
  SHA-256 digest.
- `tools/build.py` reproducibly creates five byte-stable layered OpenRaster masters, individual
  clean-alpha exports, a 960x540 module-only review scene, grayscale evidence, and four directional
  six-frame loops. `tools/validate.py` proves exact opposite-edge terrain/path seams, transparent
  gutters with no hidden RGB, distinct grayscale cues, fixed-obstacle path clearance, unique frames,
  one 52-pixel foot baseline, and 120 ms timing.
- `game/art/direction-a-import-proof` contains exact byte-for-byte copies of all 32 exports. Import
  metadata is lossless with mipmaps off, alpha-border correction on, and premultiplied alpha off.
  `DirectionAImportProof.tscn` inherits linear filtering and currently assembles only the south loop.
  The other 18 directional frames are imported and parity-checked but not yet engine animations.
- `scripts/quality.sh` includes both asset validators. `scripts/smoke-godot.sh` performs an editor
  import for clean checkouts and runs the isolated scene before ordinary smokes. Generated `*.cs.uid`
  files are ignored. The reviewed import command and isolated smoke run without diagnostics.
- Host-local full quality passed the zero-warning build, 2,839 Domain, 924 Application, 1,261
  Infrastructure, and 8 architecture tests, scenario validation, the complete credential-free
  Godot smoke sequence, and Compatibility graphical launch. The restricted isolated smoke first
  hit the known `user://logs` crash; only the identical host-local command was retried.
- The five new painterly inputs used Codex's built-in image tool with only repository-owned proof
  sheets as references. Exact prompts are retained. The tool exposed no model/version identifier;
  no third-party content or licence claim was introduced.
- Do not treat these proof assets as final production quality or as gameplay, collision, navigation,
  UI, accessibility, release, signing, or complete-opening-region evidence.

## Exact next bounded slice

Extend only `DirectionAImportProof.tscn` (or one isolated resource it owns) from its current south
animation to four named six-frame animations: `walk_south`, `walk_west`, `walk_east`, and
`walk_north`. Keep the manifest's 120 ms timing, each direction's exact six-frame order, the
52-pixel contact baseline, and smooth filtering. Extend the isolated smoke to switch through all
four names and prove exact frame count, unique imported texture resources, dimensions, looping,
timing, and inert presentation. Keep the scene disconnected from `Main.tscn`.

Do not replace the ordinary Seeker, alter movement/facing/gameplay authority, add collision or
navigation, create idle/breathe or interaction frames, expand terrain/structures/portraits/UI,
switch to Direction D, or begin audio, release, or signing work. After this isolated four-animation
resource passes, the following slice may create one external south-facing two-frame idle/breathe
proof before any four-direction idle expansion or runtime use.
