# Allrpcs Handover

## Current state

`main` includes merged PR #373 at `6588edf`. Branch
`codex/phase-8-opening-region-projection` projects the authored Phase 8 topology at gameplay scale and
starts the sickhouse/first-patient greybox scene.

The exact Phase 8 topology now projects all eleven retained locations into a 1600-by-900-pixel scene.
Each projected edge retains its exact topology provenance and has a real route over the synchronized
Godot navigation map. Anchors require 24 pixels of endpoint clearance and 120 pixels of landmark
separation; missing, duplicate, foreign, rebuilt, non-finite, out-of-bounds, or crowded inputs fail
before publication.

The exact sickhouse entry now binds the representative deep-sleeping first patient within 96 pixels.
The room, bed, body, and label are temporary scale evidence only. Disease and treatment state remain
unchanged. Phase 7 is closed; do not add another Move chain or sabotage cadence cycle.

## Validation and review

- Test-first focused Application builds failed at the absent projection and first-patient binding types.
- Seven focused host-local Application tests pass for exact provenance, anchor rejection, bounds,
  clearances, sickhouse binding, deep-sleep state, and bounded patient placement.
- Focused host-local Godot smokes pass all eleven endpoint-clearance probes, all twelve authored
  navigation routes, and the exact sickhouse/deep-sleeping-patient scene binding.
- Host-local `./scripts/quality.sh` passed workflow validation, formatting, a zero-warning build,
  2,789 Domain, 593 Application, 1,220 Infrastructure, and 8 architecture tests, the representative
  scenario validator, every credential-free Godot smoke, and the Compatibility launch.
- Complete-diff review confirmed whitespace and 120-character cleanliness, kept every changed C# file
  below 500 lines, and corrected one overbroad focused-test name before publication.
- Draft PR #374 targets `main`. Hosted run `36134253771` failed all five jobs with `steps: []`; no
  workflow step executed, so this is Actions/account infrastructure rather than product-test evidence.
- Ordinary provider/model startup remains unchanged; no Phase 7 action/cadence work, garden, village
  transition, discovery, production art, weather, sound, persistence, signing, release, or distribution
  work was added.

## Exact next bounded slice

Make the greybox sickhouse enterable without broadening into treatment interaction or another landmark.

Acceptance boundary:

1. Replace only the temporary sickhouse rectangle with collision-aware greybox walls and one explicit
   entrance while retaining the exact sickhouse anchor and first-patient binding.
2. Update the navigation surface so the entrance and patient approach remain traversable with the
   existing 24-pixel clearance; reject routes that cross a wall or exit the 1600-by-900 bounds.
3. Prove the Seeker can enter, reach the patient approach, and leave over synchronized Godot navigation;
   keep treatment, dialogue, focus, discovery, and disease state unchanged.
4. Stop before the healer's garden, other landmarks, final camera/projection/art direction, production
   art, weather, lighting, sound, story variations, journal, or map.
5. Add no Phase 7 action/cadence work and no provider, persistence, production UI, audio, signing,
   release, or distribution work.
6. Run `./scripts/quality.sh`, review the complete diff, and leave one exact next Phase 8 slice.
