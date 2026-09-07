# Allrpcs Handover

## Current state

PR #127 merged Phase 2n5–2n8 into `main` at `eb547c1`. The continuation branch
`codex/phase-2n9-plus-observation-reinterpretation` implements Phases 2n9–2n19 in implementation
commit `94d8602`; draft PR #128 tracks the branch.

The Domain path now supports complete-candidate consumption, atomic trigger-aware
`Noticed`-to-`Identified` resolution, duplicate-free recording, production bound to an existing
character and world object, stable first-eligible selection, and deterministic resolve-next
sequencing. All operations are pure, accept value-equal identifiers where appropriate, preserve
stable order and exact unrelated instances, and fail closed for mismatches or missing state.

Each Phase 2n9–2n19 test suite first failed because its production type did not exist. After the
minimal implementations and code review, host-shell `./scripts/quality.sh` passed workflow and
formatting validation, a zero-warning build, 928 Domain tests, 107 Application tests, six
architecture tests, all eighteen Godot smokes, and the bounded graphical launch. Controller
ownership, temporary sources, and runtime wiring remain incomplete.

## Exact next bounded slice

Take **Phase 2n20: controller ownership and authoritative candidate production**.

Acceptance boundary:

1. Extend `Allrpcs.Game.FocusedSearchActivityController` with one owned
   `ObservationReinterpretationCandidateSnapshot`, initialized empty in `_Ready`, and one
   `TryProduceObservationReinterpretationCandidate(candidate, character, worldObject)` operation.
2. Require the candidate observer to identify this controller's Seeker, then delegate all remaining
   authority and duplicate checks to the existing Domain production policy and transition.
3. On success install the exact returned snapshot. On rejection preserve reinterpretation,
   perception, focused-search activity/outcome, passive-noticing, and presentation state.
4. Add one credential-free opt-in Godot smoke covering empty ownership, exact and distinct
   value-equal success, observer/character/object mismatch, complete duplicate rejection, exact
   candidate retention, null rejection, and unchanged unrelated state.
5. Update README and ROADMAP evidence, run `./scripts/quality.sh`, review every intended path, and
   prune this file again to current evidence plus one next slice.

Explicit exclusions:

- Do not resolve, select, consume, or apply reinterpretation candidates.
- Do not add a temporary candidate or knowledge source, scene startup wiring, events, persistence,
  serialization, providers, plugins, credentials, or UI.
- Do not change passive noticing, focused search, world-object or character ownership, movement,
  input, or presentation.
- Keep point-and-click movement, disease-time pausing, and the world-time debug overlay deferred.
- Do not merge, release, distribute, sign, notarize, or change repository visibility without
  explicit user authorization.
