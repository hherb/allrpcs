# Allrpcs Handover

## Current state

`main` includes merged PR #341 at `99b32ba`. Branch
`codex/phase-7-follow-up-move-authoring-chain` completes **Phase 7ra** through **Phase 7rc**. The
implementation is committed at `f4a796e` and published in draft PR #342 targeting `main`. The
unrelated untracked `scripts/code_statistics.py` remains untouched and must stay unstaged.

The exact admitted follow-up cycle now advances through three more explicit seams:

- `CharacterSabotageActionMoveCycleAuthoringCoordinator` invokes a third scene-owned Move author
  only for the exact current cycle admission, recovered placement/evidence authority, and one fresh
  source-owned cycle candidate.
- `CharacterSabotageActionMoveCycleCompositionCoordinator` retains that exact admission and author
  output while composing one distinct generic Move request without starting it.
- `CharacterSabotageActionMoveCycleCompositionStartCoordinator` revalidates the complete retained
  graph and explicitly starts only that request on a fresh task/model-source pair.

The inherited acceptance wording called for reusing `FollowUpCandidate`, but its evidence remains
authoritative after recovery, so the existing freshness policy correctly rejects it. The bounded
implementation adds only a third temporary source candidate with a distinct evidence identifier; no
Domain rule was weakened. The recovery owner now retains and revalidates the exact authoritative
evidence token as well as the recovered placement, closing a stale-evidence gap found in review.

Null, rebuilt-admission/boundary/request, malformed-provenance, foreign-actor, wrong-kind,
changed/active-slot, stale-route/recovery/authority, and repeated paths remain inert. Rejected start
preflight preserves the queued model. The two earlier model call counts remain one, the new generic
model is called once, and its completion is not observed. No candidate binding, acknowledgement,
task reset, typed request, dispatch/application, prior-author reset, turn reservation, terminal
route, cadence, provider, credential, persistence, UI/input, navigation, perception, dialogue, timer,
or new Domain behavior was added. Ordinary startup remains inert.

## Validation and review

- Each new coordinator was test-first; its focused game build failed only for the deliberately
  absent coordinator before implementation.
- The focused switch-gated Move proof passes host-local with Godot 4.6.3 Mono and .NET 8 through the
  exact admission, third author, generic composition, and explicit fresh-task start.
- Sandboxed Godot cannot open `user://logs`; the identical focused command passes host-local.
- Final host-local `./scripts/quality.sh` passes restore, formatting, a zero-warning build, 2,731
  Domain tests, 586 Application tests, 1,220 Infrastructure tests, all 8 architecture tests,
  scenario validation, every credential-free Godot smoke, and the Compatibility renderer launch.
- Session review covers exact-reference provenance, recovered placement/evidence authority,
  one-shot author/composition/start behavior, queued-model preservation, model call counts, inert
  startup, docstrings, file/line limits, scene dependency order, formatting, and whitespace. It
  fixed one tautological smoke assertion and the stale-evidence identity gap; no unresolved findings
  remain.

## Exact next bounded slice

Take **Phase 7rd: observe only the exact completed third-cycle generic Move result without
acknowledging, binding, dispatching, or applying it**.

Acceptance boundary:

1. Test-first add one one-shot observation coordinator over the fresh cycle task and start owner.
2. Retain only the exact completed result whose request is the exact started request while the full
   admission, recovery authority, authoring, composition, and start graph remains current.
3. Reject null, idle/running/cancelled/failed, rebuilt-result/request, foreign-actor, wrong-kind,
   stale-graph/authority, and repeated paths without changing the task, prior owners, authoritative
   state, ordinary turn, or any model call count.
4. Extend only the focused switch-gated Move proof. Do not acknowledge the result, bind its typed
   cycle candidate, reset the task, compose/start a typed Move request, dispatch/apply another Move,
   recover it, enter another terminal route, or add automatic cadence.
5. Keep ordinary startup inert and preserve all existing exclusions: no timers, provider transport,
   credentials, persistence, UI/input, navigation, perception, dialogue delivery, or new Domain
   behavior.
6. Update README/ROADMAP, run `./scripts/quality.sh`, review the complete diff, and leave the next
   slice to bind that exact observed result to the third cycle candidate without acknowledging it.
