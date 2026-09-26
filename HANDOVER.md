# Allrpcs Handover

## Current state

`main` includes merged PR #385 at `76753ec`. Branch
`codex/phase-8-customized-character-projection` continues Phase 8 through three bounded
customization-application slices.

The Domain now owns a pure `SeekerCustomizedCharacterProjection` and immutable
`SeekerCustomizedCharacterApplication` token. Projection requires a selected profile whose exact
`CharacterId` is the current temporary Seeker's identifier and rejects null, empty, rebuilt,
wrong-character, and already-projected inputs. It creates one fresh `Character` with the profile's
exact identity and public background while retaining the current character's exact private
background, capabilities, limitations, controller assignment, conditions, inventory, knowledge,
beliefs, relationships, and goals. The token retains the exact selected state, prior character,
and projected character.

`TemporarySeekerCharacterSource` starts with no application and compare-and-swaps one token bound
to its exact current character. Null, rebuilt, foreign, stale, and repeated applications leave both
owned properties unchanged. `SeekerCustomizationApplicationCoordinator` preflights the exact empty
selection state, representative options, selected predecessor, current character, and empty
application slot before synchronously installing the exact selected state and projected character.

The U-key customization menu now uses only that coordinator. A complete valid submission atomically
installs the selected profile and live customized temporary Seeker, closes the menu, and restores
the Seeker's prior physics-processing state. Rejected and repeated paths preserve selection,
character, application, movement, and presentation state. The Phase 8 Seeker name, pronouns,
appearance-choice, and background-selection roadmap item is complete at this boundary.

Pronouns are retained but are not projected into dialogue. No appearance asset is rendered. The
four backgrounds do not yet grant capabilities or route variations. Customization is not persisted.
Provider/model behavior and all unrelated discovery, environment, movement, journal, map, signing,
release, and distribution behavior remain untouched.

## Validation and review

- Focused Domain coverage passes three projection tests for exact token/component provenance and
  null, empty, rebuilt-identifier, wrong-character, and already-projected rejection.
- The temporary-character-source smoke passes inert startup, one exact application, and
  null/foreign/rebuilt/repeated rejection without changing focused-search, perception, movement,
  or presentation state.
- The menu smoke passes exact coordinator rejection, complete-field submission, atomic selected
  state and live-character installation, movement restoration, and repeated-input preservation.
- Host-local `./scripts/quality.sh` passed workflow validation, formatting, a zero-warning build,
  2,811 Domain, 660 Application, 1,220 Infrastructure, and 8 architecture tests, scenario
  validation, every credential-free Godot smoke, and the graphical Compatibility launch. The
  sandbox-only run reached all .NET tests and scenario validation before the known Godot
  `user://logs` restriction; the identical host run supplied the Godot evidence.
- Session review checked the complete diff, exact-reference rejection paths, synchronous scene
  ordering, source consumers, public API shape, documentation claims, 120-character limits,
  sub-500-line C# files, and whitespace. No remaining correctness issue was found.

## Exact next bounded slice

Begin the next open Phase 8 roadmap item with one pure, scene-neutral representative background
route-variation catalogue. Do not change navigation or unlock a route yet.

Acceptance boundary:

1. Add one immutable route-variation definition retaining an exact representative
   `SeekerBackgroundChoice` and one exact existing opening-region destination anchor.
2. Author four definitions in representative background order: Forester to the closed-forest-path
   anchor, Scribe to the old-shrine anchor, Apothecary's Assistant to the healer-garden anchor, and
   Traveller to the southern-road anchor. Every background must have one variation; none may be a
   mandatory default or remove another existing route.
3. Add one pure exact-choice lookup/projection that accepts only the selected background instance
   retained by the applied customization token and the exact supplied opening-region projection.
   Reject null, empty/unapplied, rebuilt, foreign, missing-anchor, and duplicate-catalogue inputs
   without changing customization or navigation state.
4. Add focused Domain/Application coverage as appropriate. Do not grant capabilities, mutate the
   live character, open the closed gate, change collision/navigation, alter discovery visibility,
   render appearance, project pronouns, persist customization, or add provider/UI behavior.
5. Run `./scripts/quality.sh`, review the complete diff, update `README.md`, and leave one exact next
   slice for applying a selected variation through an explicit scene-owned route state.
