# Allrpcs Handover

## Current state

Phase 8 remains complete on `main`. Human review approved Phase 9.1 Direction A, painterly storybook
gouache, and the refined representative scene; Phase 9.1 is complete. Direction D remains only the
documented contingency. The first bounded Phase 9.2 production-pipeline proof created four
transparent source sheets outside `game/`; none is runtime-ready or imported.

## Current evidence and boundaries

- The selected direction retains the existing 960x540 logical viewport and `Camera2D`, uses an
  orthographic-like high-oblique projection, 2x editable raster masters, modular painterly layers,
  warm late-afternoon light, selective warm-dark outlines, and 48–56-pixel gameplay characters.
- The refined scene keeps one cottage, two characters, two herb beds, clear walkable ground, and one
  preparation table with a double contour plus separate diamond marker rather than a colour-only cue.
- The art bible defines palette roles, texture and outline policy, four-direction animation proof,
  UI boundaries, ordinary/noticeable/hidden/identified/dangerous object rules, five feasibility
  gates, explicit exclusions, and the narrow condition under which Direction D can replace A.
- The production proof contains six terrain/path concepts, six herb-bed/foliage concepts, four
  preparation-table state treatments, and exactly 24 Seeker frames in four six-frame direction rows.
- Every proof PNG has alpha, but terrain/plants retain coloured low-alpha gutter haze and never
  reach fully opaque alpha; all sheets still lack deterministic export rectangles and pivots; table
  cues are baked rather than overlays; and the Seeker foot baselines and loop timing are not
  mechanically normalized. The flattened PNGs also fail the editable-master gate. These are
  explicit pipeline failures, not runtime assets.
- Host-local `./scripts/quality.sh` passed a zero-warning build, 2,839 Domain, 924 Application,
  1,261 Infrastructure, and 8 architecture tests, scenario validation, every credential-free Godot
  smoke, formatting, and the Compatibility graphical launch. The restricted run first reached only
  the known `user://logs` boundary; no product change was made for that environment failure.
- Codex's built-in image-generation tool created the previews independently from only the project's
  own cropped Phase 8 checkpoint. No third-party reference or asset was used. The tool did not
  expose a model/version identifier, and the previews remain concept evidence rather than licensed
  production assets.
- Phase 8's final human checkpoint remains only ordinary save/full-close/reopen/load evidence; do
  not expand it into a granular usability claim.
- Production art, runtime import or replacement, gameplay/UI changes, audio, provider/credential
  work, release, and signing remain out of scope.

## Exact next bounded slice

Perform the one bounded Direction A pipeline correction pass. Replace sheet-dependent gutters with
individual clean-alpha modules, retain or create editable layered masters, and define one manifest
containing each asset's logical/export rectangles, 2x master dimensions, contact baseline, and pivot.
Recompose one external 960x540 review scene from only those modules, verify path seams and grayscale
state cues, and normalize only one six-frame Seeker direction before extending animation further.

Do not import into Godot until the corrected proof passes, switch to Direction D unless an art-bible
feasibility gate still fails after this correction, bulk-produce the opening region, modify gameplay
or UI behaviour, use untracked third-party content, or start audio, release, or signing work.
