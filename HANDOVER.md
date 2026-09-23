# Allrpcs Handover

## Current state

`main` includes merged PR #340 at `9b48da6`. Branch
`codex/phase-7-follow-up-move-cycle-admission` completes **Phase 7qw** through **Phase 7qz**. The
implementation is committed at `8852755` and published in draft PR #341 targeting `main`. The
unrelated untracked `scripts/code_statistics.py` remains untouched and must stay unstaged.

The recovered follow-up Move now advances through four additional explicit seams:

- `CharacterSabotageActionMoveSchedulingAuthorizationCoordinator` retains one fresh scheduling
  authorization containing only the exact follow-up completion admission and bound empty ordinary
  turn reference while the complete route, recovery, completion, admission, and boundary graph is
  still current.
- `CharacterSabotageActionMoveCycleBoundaryCoordinator` consumes only that exact authorization once
  at a distinct explicit follow-up cycle boundary without copying its provenance.
- The existing pure `CharacterSabotageCycleStartEligibilityPolicy` revalidates the exact follow-up
  boundary, authorization, completion admission, and empty ordinary-turn reference.
- `CharacterSabotageActionMoveCycleAuthoringAdmissionCoordinator` retains one fresh authoring
  admission containing only that exact still-current follow-up cycle boundary.

Null, rebuilt-completion, rebuilt-authorization, rebuilt-boundary, malformed-provenance,
changed/active-slot, stale-route, stale-recovery, foreign-chain, and repeated paths remain inert.
Both model call counts remain one. No turn reservation, action-author invocation, request
composition/start, another typed action, author reset, provider, credential, persistence, UI/input,
navigation, perception, dialogue, cadence, timer, or new Domain behavior was added. Ordinary startup
remains inert.

## Validation and review

- Each new coordinator was test-first; its focused game build first failed only for the deliberately
  absent next coordinator.
- The focused switch-gated Move proof passes host-local with Godot 4.6.3 Mono and .NET 8 through the
  exact follow-up scheduling authorization, explicit cycle boundary, pure cycle-start gate, and
  distinct future-authoring admission.
- Sandboxed Godot cannot open `user://logs`; the identical focused command passes host-local.
- Final host-local `./scripts/quality.sh` passes restore, formatting, a zero-warning build, 2,731
  Domain tests, 586 Application tests, 1,220 Infrastructure tests, all 8 architecture tests,
  scenario validation, every credential-free Godot smoke, and the Compatibility renderer launch.
- Session review covered exact-reference provenance, complete live route/recovery ownership,
  one-shot authorization/boundary/admission behavior, pure eligibility, empty-slot freshness,
  malformed/rebuilt/repeated rejection, model call counts, inert startup, docstrings, file/line
  limits, scene dependency order, formatting, and whitespace. Import ordering and one unused import
  were fixed; no unresolved findings remain.

## Exact next bounded slice

Take **Phase 7ra: let only the exact current follow-up-cycle authoring admission guard one fresh
invocation of a distinct scene-owned Move author without composing or starting its request**.

Acceptance boundary:

1. Test-first add one distinct scene-owned `CharacterSabotageActionMoveRequestController` and one
   one-shot coordinator that accepts only
   `CharacterSabotageActionMoveCycleAuthoringAdmissionCoordinator`'s exact admission while its
   complete cycle, authorization, completion, recovery, and empty-turn graph remains current.
2. Invoke that fresh author once for the admitted actor, current authoritative recovered placements
   and evidence, and the existing exact follow-up Move candidate; retain the exact admission and new
   author request without changing the prior two author owners.
3. Reject null, rebuilt-admission, rebuilt-boundary, malformed-provenance, changed/active-slot,
   stale-route/recovery/authority, foreign-chain, and repeated paths without author output, state
   changes, or another model call.
4. Extend only the focused switch-gated Move proof. Do not compose or start the new generic request,
   supply or consume a model, reserve the ordinary turn, reset any author, clear prior provenance,
   dispatch/apply another Move, or enter another terminal route.
5. Keep ordinary startup inert and preserve all existing exclusions: no cadence, timers, provider
   transport, credentials, persistence, UI/input, navigation, perception, dialogue delivery, or new
   Domain behavior.
6. Update README/ROADMAP, run `./scripts/quality.sh`, review the complete diff, and leave the next
   slice to compose only that exact newly guarded author output without starting consideration or
   consuming a model.
