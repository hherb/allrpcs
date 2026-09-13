# Allrpcs Handover

## Current state

`main` contains merged PR #231 at `5f1409d`. The current branch is
`codex/phase-7-sabotage-decision-flow` and advances four bounded Phase 7 slices:

- Phase 7dp binds one accepted concealment decision to its exact consideration request and retains
  the exact selected request candidate, or `null` for abstention;
- Phase 7dq defines one provider-neutral cancellation-aware interface for obtaining that decision;
- Phase 7dr obtains one decision for the exact request and returns only its validated exact result;
- Phase 7ds applies an accepted selection through the existing immutable concealment/evidence
  transition while explicit abstention returns no application.

The flow does not yet own or install an accepted result in the scene. It also does not choose timing,
construct or supply a model, decode or serialize model data, invoke a provider, add another sabotage
kind, or add UI/input/events/time/transcripts/persistence or movement behavior.

## Validation and review

- Every slice began with the expected compile failure for its absent contract or operation.
- Focused host-local Domain tests pass exact result/candidate retention, abstention, identifier and
  eligibility rejection, immutable application, unchanged inputs, and closed public surfaces.
- Focused host-local Application tests pass exact request and cancellation forwarding, one model
  call, abstention, invalid-decision rejection, cancellation, unchanged input state, and closed
  public surfaces.
- Host-local `./scripts/quality.sh` passes workflow validation, restore, formatting, zero-warning
  build, 2,306 Domain tests, 484 Application tests, 532 Infrastructure tests, 8 architecture tests,
  scenario validation, every credential-free headless Godot smoke, and the Compatibility-renderer
  graphical launch.
- All changed authored C# files are below 500 lines and use no lines longer than 120 characters.
  No live-conformance switch was enabled and no provider was invoked.

## Exact next bounded slice

Take **Phase 7dt: scene-own resolution of one exact accepted concealment consideration result**.

Acceptance boundary:

1. Extend `CharacterObjectSabotageConcealmentController` with one narrowly named operation accepting
   an exact `CharacterObjectSabotageConcealmentConsiderationResult`.
2. Compare the result request's exact concealment and evidence snapshots against current scene-owned
   instances before resolving it; stale or uninitialized state must fail without partial mutation.
3. Install both fresh snapshots from an accepted selection atomically; accept an exact abstention
   while retaining the same scene snapshot instances.
4. Prove selection, abstention, stale/null/repeated rejection, unchanged surrounding state, and the
   smallest controller surface in the focused credential-free Godot smoke.
5. Do not obtain a decision, create a model source or task controller, choose timing, trigger
   sabotage automatically, decode/serialize provider data, invoke a provider, add UI/events/time/
   transcripts/persistence, or add non-conceal effects or movement behavior.
