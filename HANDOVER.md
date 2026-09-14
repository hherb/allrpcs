# Allrpcs Handover

## Current state

`main` contains merged PR #239 at `7f8cbb0`. The current branch is
`codex/phase-7-incompatibility-response-round` and advances five bounded Phase 7 slices:

- Phase 7el adds `CharacterBeliefGoalIncompatibilityResponseRound`, retaining exactly two exact
  declaration-ordered perspectives and one exact response for each in copied read-only collections.
  It rejects incomplete, null, duplicate, reversed, cross-declaration, value-equal replacement, and
  non-exactly aligned entries before retaining state.
- Phase 7em adds `CharacterBeliefGoalIncompatibilityConsiderationRequest`, retaining one exact
  perspective and every nested declaration reference without copying character state.
- Phase 7en adds `CharacterBeliefGoalIncompatibilityResponsePolicy`, accepting a response only when
  it retains the request's exact perspective instance.
- Phase 7eo adds `CharacterBeliefGoalIncompatibilityConsiderationResult`, binding an accepted exact
  request and response only after that policy succeeds.
- Phase 7ep adds `ICharacterBeliefGoalIncompatibilityResponseModel`, one provider-neutral
  cancellation-aware Application interface for obtaining a response to an exact request.

These slices do not judge or select a response, alter belief, goal, relationship, or allegiance
state, apply a response, resolve the incompatibility, implement or call a model, add retry or timeout
policy, schedule dialogue or sabotage, or add Infrastructure, scene, persistence, input, UI, event,
time, transcript, movement, or sabotage-effect behavior.

Draft PR #240 is open from implementation commit `8a85c4a`. Hosted run `34809723070` exposed
`steps: []` for all five jobs; four failed immediately and the pure-C# job remained queued without
steps at the final pre-documentation query. This is Actions/account infrastructure state, not
product-test or unsigned-build evidence, and should not be retried unchanged.

## Validation and review

- Tests began with the expected missing-type compile failures for every slice. Focused host-local
  tests pass: 6 response-round, 3 request, 4 policy, 7 result, and 1 interface test.
- Host-local `./scripts/quality.sh` passed workflow validation, restore, formatting, zero-warning
  build, 2,352 Domain tests, 488 Application tests, 607 Infrastructure tests, 8 architecture tests,
  scenario validation, and every credential-free headless Godot smoke. Its terminal session closed
  after the Compatibility-renderer graphical launch started, so the exact final
  `godot --path game --quit-after 60` command was rerun separately and passed.
- All ten authored C# files are below 500 lines with no lines longer than 120 characters.
  `git diff --cached --check` passes. Manual staged review found no unresolved findings. No live-
  conformance switch was enabled and no provider was invoked.
- Unrelated untracked `scripts/code_statistics.py` remains untouched and unstaged.

## Exact next bounded slice

Take **Phase 7eq: obtain and validate one exact incompatibility consideration response**.

Acceptance boundary:

1. Add one static Application controller that accepts an exact
   `ICharacterBeliefGoalIncompatibilityResponseModel`, one exact
   `CharacterBeliefGoalIncompatibilityConsiderationRequest`, and the caller's cancellation token.
2. Reject null model or request and already-cancelled input before model invocation. Otherwise call
   `RespondAsync` exactly once with the exact request and token, then return one
   `CharacterBeliefGoalIncompatibilityConsiderationResult` so null or non-exact model output fails
   through the existing Domain validation.
3. Prove exact forwarding and result retention, all four accepted canonical response kinds,
   fail-closed other-perspective and value-equal replacement output, pre-cancellation, propagated
   model cancellation/failure, unchanged request and character state, and a closed one-method
   static public surface.
4. Do not add retries, deadlines, response-round orchestration, model implementation or binding,
   provider serialization or streaming, response judgment or application, character mutation,
   dialogue or sabotage scheduling, Infrastructure, scene, persistence, input, UI, event, time,
   transcript, movement, or sabotage-effect behavior.
