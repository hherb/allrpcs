# Allrpcs Handover

## Current state

Phase 8 remains complete on `main`. Phase 9.1 now has four preview-only representative-scene visual
directions: storybook gouache, ink and wash, low-poly diorama, and pixel art. The same-project
greybox reference, full-size PNG files, exact prompts, provenance, integrity hashes, comparison
sheet, and neutral tradeoff review are under `art-direction/representative-scene/`. No direction has
been selected and no generated image has entered the runtime.

## Current evidence and boundaries

- The comparison keeps one building, two characters, plants, one interactable preparation table,
  the existing 16:9 gameplay scale, and status/dialogue UI layering visible in every direction.
- Every interactable treatment combines geometry or a marker with value contrast rather than using
  colour alone. The review notes compare perspective, proposed resolution, palette, lighting,
  texture, outlines, atmosphere, character proportions, legibility, and likely production cost.
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

Conduct a human review of the four full-size previews and comparison notes. Record either one chosen
direction or one explicitly bounded hybrid/refinement, together with decisions for perspective,
projection, camera framing, base asset resolution, palette, lighting language, texture treatment,
outline policy, atmosphere, character proportions, animation needs, and non-colour-only rules for
ordinary, noticeable, hidden, identified, and dangerous objects.

Do not infer a winner from the comparison, generate a second round before review, import previews
into Godot, replace runtime assets, modify gameplay or UI behavior, begin the production asset set,
use untracked third-party assets, or start audio, release, or signing work. After the human decision,
the following implementation slice should be only one refined representative scene and the first
bounded art-bible draft for that selected direction.
