# Allrpcs Handover

## Current state

`main` includes merged PR #384 at `5036fe2`. Branch
`codex/phase-8-seeker-customization-state` continues Phase 8 with two bounded Seeker-customization
slices.

The Domain now owns an immutable `SeekerCustomizationSelectionState`. Initial state retains one
exact representative options catalogue and no profile. A successful pure transition composes only
through the existing exact-choice `SeekerCustomizationSelection`, retains its exact empty
predecessor, and returns one fresh state with the exact identity, five pronoun forms, appearance,
background, and profile. Already-selected, null, rebuilt, foreign, wrong-character, and incomplete
inputs fail without changing the original state.

`SeekerCustomizationSelectionSceneSource` creates one representative catalogue and exact empty
current state at ordinary startup. Its compare-and-swap installation requires the exact current
state, exact shared options, and a selected state whose exact predecessor is that current state.
No default profile is selected.

The main scene has a hidden one-shot customization menu. U opens it explicitly; released,
unrelated, and post-selection input stay inert. While open it suspends Seeker physics and retains
the prior processing state. It requires a display name, all five pronoun forms, and explicit
appearance/background choices with no defaults. Successful confirmation uses the existing live
Seeker character's exact `CharacterId`, installs one exact selected state, closes the menu, and
restores the prior movement-processing state. Invalid or incomplete submission remains open and
does not change selection state.

The selected profile does not yet replace the live temporary `Character`; therefore the Phase 8
Seeker-customization roadmap item remains open. Pronoun dialogue projection, appearance assets,
background capabilities/routes, persistence, provider/model work, and all unrelated discovery,
environmental, movement, journal, map, signing, release, and distribution behavior remain
untouched.

## Validation and review

- Focused customization coverage passes 12 Domain tests, including exact predecessor retention,
  stable representative order, one-shot composition, and null/rebuilt/foreign/wrong-character
  rejection.
- The selection-source smoke passes inert startup, exact predecessor-bound compare-and-swap,
  representative order, one install, and rejected/repeated-path preservation.
- The menu smoke passes hidden startup, explicit U activation, prior movement-state preservation,
  complete-field validation, exact live Seeker identifier retention, one-shot installation, and
  repeated-input rejection.
- Host-local `./scripts/quality.sh` passed workflow validation, formatting, a zero-warning build,
  2,808 Domain, 660 Application, 1,220 Infrastructure, and 8 architecture tests, scenario
  validation, every credential-free Godot smoke, and the graphical Compatibility launch.
- Session review added exact predecessor provenance so results derived from rebuilt current state
  cannot install, preserved the Seeker's prior physics-processing state across repeated U input,
  and reused the live character source's exact `CharacterId`. No remaining correctness issue was
  found. New C# files are below 500 lines; 120-character and whitespace checks pass.

## Exact next bounded slice

Add one pure, scene-neutral customized-`Character` projection and exact application token without
mutating the live scene yet.

Acceptance boundary:

1. Add one immutable application token retaining the exact selected state, exact current temporary
   Seeker `Character`, and one freshly projected customized `Character`.
2. Add one pure projection that accepts only a selected state whose profile identity retains the
   current character's exact `CharacterId`. Reject null, empty, rebuilt-identifier, wrong-character,
   foreign, and repeated/already-applied inputs without changing either input.
3. The projected character must use the profile's exact identity and public background while
   retaining the current character's exact private background, capabilities, limitations,
   controller assignment, conditions, inventory, knowledge, beliefs, relationships, and goals.
   The token must retain all three exact provenance references.
4. Add focused Domain/Application coverage as appropriate. Do not change
   `TemporarySeekerCharacterSource.Character`, wire the menu to character replacement, project
   pronouns into dialogue, grant background capabilities/routes, render appearance assets, or add
   persistence yet.
5. Run `./scripts/quality.sh`, review the complete diff, update `README.md`, and leave one exact
   next Phase 8 slice for the later atomic scene application boundary.
