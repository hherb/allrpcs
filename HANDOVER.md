# Allrpcs Handover

## Current state

`main` contains merged PR #238 at `57f9177`. The current branch is
`codex/phase-7-incompatibility-perspectives` and advances three bounded Phase 7 slices:

- Phase 7ei adds `CharacterBeliefGoalIncompatibilityPerspective` and its pure projection. Only the
  declaration's exact first or second character is accepted; the read-only projection returns both
  orientations in stable declaration order with every nested reference retained.
- Phase 7ej adds the closed canonical `CharacterBeliefGoalIncompatibilityResponseKind` values
  `maintain-position`, `reconsider-belief`, `reconsider-goal`, and `seek-reconciliation`.
- Phase 7ek adds `CharacterBeliefGoalIncompatibilityResponse`, retaining one exact perspective,
  one exact canonical kind, and one non-blank explanation of at most 500 UTF-16 code units.

These Domain-only values do not infer or select a response, judge either position, resolve the
conflict, alter a belief or goal, change allegiance, schedule dialogue or sabotage, call a provider,
or add Application, Infrastructure, scene, persistence, input, UI, event, time, transcript,
movement, or sabotage-effect behavior.

## Validation and review

- Tests began with the expected missing-type compile failures for the perspective and response
  vocabulary/value. The combined focused incompatibility-response suite passes 13 tests.
- Host-local `./scripts/quality.sh` passes workflow validation, restore, formatting, zero-warning
  build, 2,332 Domain tests, 487 Application tests, 607 Infrastructure tests, 8 architecture tests,
  scenario validation, every credential-free headless Godot smoke, and the Compatibility-renderer
  graphical launch. The sandbox-only run reached a clean build before VSTest loopback was denied.
- All eight authored C# files are below 500 lines with no lines longer than 120 characters.
  `git diff --check` passes. No live-conformance switch was enabled and no provider was invoked.
- Unrelated untracked `scripts/code_statistics.py` remains untouched and unstaged.

## Exact next bounded slice

Take **Phase 7el: retain one complete exact two-character incompatibility response round**.

Acceptance boundary:

1. Define one immutable Domain round that retains an exact stable two-perspective projection and
   exactly one exact response for each perspective in the same declaration order.
2. Fully validate before construction: reject null entries, missing or duplicate subjects, reversed
   order, response perspectives not retained by exact reference, and perspectives from different or
   value-equal replacement declarations. Retain all accepted nested instances without copying or
   mutating either character.
3. Prove exact two-entry bounds, stable order, exact-instance retention, complete rejection, fresh
   independent fixture composition, unchanged inputs, and a closed immutable public surface.
4. Do not choose or apply a response, change belief, goal, relationship, or allegiance state,
   schedule dialogue or sabotage, call a provider, or add Application, Infrastructure, scene,
   persistence, input, UI, event, time, transcript, movement, or sabotage-effect behavior.
