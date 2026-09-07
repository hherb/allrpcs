# Allrpcs Handover

## Current state

Phases 2n1–2n8 are implemented on
`codex/phase-2n1-plus-observation-reinterpretation`, based on current `origin/main` merge commit
`b81a252` after PR #125 merged.

The Domain now has an immutable observation-reinterpretation candidate and ordered duplicate-free
snapshot, stable observer projection, and exact complete-candidate lookup. Pure policies require a
complete value-equal observer character with the triggering knowledge plus an observer-owned
perception snapshot containing the target. Change detection accepts only a target at exactly
`Noticed`; pure application returns a new snapshot with only that target changed to `Identified`,
retaining the perception identifier, target object identifier, stable order, and unrelated entry
instances. All inputs remain unchanged.

Phases 2n5–2n8 were test-first. The focused tests failed on each missing production type before the
minimal implementation was added. After implementation and code review, host-shell
`./scripts/quality.sh` passed workflow and formatting validation, a zero-warning build, 859 Domain
tests, 107 Application tests, six architecture tests, all eighteen Godot smokes, and the bounded
graphical launch. Knowledge-triggered reinterpretation remains incomplete.

## Exact next bounded slice

Take **Phase 2n9: reinterpretation-candidate consumption eligibility**.

Acceptance boundary:

1. Add one pure static
   `Allrpcs.Domain.Discovery.ObservationReinterpretationCandidateConsumptionPolicy` with one
   `CanConsume(ObservationReinterpretationCandidate candidate,
   ObservationReinterpretationCandidateSnapshot candidates)` operation.
2. Return true only when exact complete-candidate lookup finds a value-equal retained candidate.
   Accept a distinct value-equal candidate instance; reject an empty snapshot, absent candidates,
   and every observer/target/trigger partial match.
3. Reject either null input. Leave the candidate, snapshot, stable order, every retained candidate,
   and every nested identifier instance unchanged.
4. Add focused tests first for those boundaries and the minimal single-operation public API. Update
   README and ROADMAP evidence, keep reinterpretation incomplete, run `./scripts/quality.sh`, review
   every intended path, and prune this file again to current evidence plus one next slice.

Explicit exclusions:

- Do not remove a candidate; apply or resolve reinterpretation; inspect a character, knowledge,
  perception, visibility, or world object; or change any state.
- Do not record, produce, select, rank, reorder, sequence, or wire candidates.
- Do not add Application/Godot sources, controllers, runtime wiring, events, persistence,
  serialization, providers, plugins, credentials, or UI.
- Do not change passive noticing, focused search, world-object or character ownership, movement,
  input, or presentation.
- Keep point-and-click movement, disease-time pausing, and the world-time debug overlay deferred.
- Do not merge, release, distribute, sign, notarize, or change repository visibility without
  explicit user authorization.
