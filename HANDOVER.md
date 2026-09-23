# Allrpcs Handover

## Current state

`main` includes merged PR #334 at `e26a6df`. Branch
`codex/phase-7-sabotage-action-authoring-admission` completes **Phase 7qd** and
**Phase 7qe**. The unrelated untracked `scripts/code_statistics.py` remains
untouched and must stay unstaged.

`CharacterSabotageActionAuthoringAdmissionCoordinator` admits the exact current
sabotage-cycle boundary once only after revalidating its authorization,
completed-turn admission, and empty ordinary-turn slot. Its fresh token contains
only that boundary and exposes the actor by following retained provenance rather
than copying identity.

`CharacterSabotageActionMoveAuthoringCoordinator` accepts only that exact current
admission and invokes a fresh instance of the existing Move author once. The
temporary Move source supplies a follow-up candidate with a distinct evidence ID,
so the existing author can use the recovered placement and retained first-cycle
evidence without resetting any prior owner. The coordinator retains the exact
admission and exact authored request; the author retains the exact typed candidate
binding.

Null, rebuilt, malformed, changed-slot, active-slot, stale, and repeated paths
remain inert. The completed route, recovery output, first-cycle author, scheduling
graph, ordinary turn, authoritative placement/evidence, tasks, and both model call
counts remain unchanged. Nothing composes or starts another generic consideration,
reserves a turn, clears an owner, or enables automatic enemy-agent sabotage.
Ordinary startup remains inert.

## Validation and review

- Each slice was test-first: the focused game build initially failed only for the
  deliberately missing Phase 7qd and Phase 7qe coordinators/verifiers.
- The focused switch-gated Move proof passes host-local through exact recovery,
  terminal reset, completion, scheduling admission, empty ordinary-turn binding,
  authorization, explicit cycle boundary, action-authoring admission, and one
  guarded follow-up Move author output. Rejected graphs remain inert and neither
  model is called again.
- The changed temporary Move-source and Move-author smokes pass independently.
- Sandboxed Godot still cannot open `user://logs`, and sandboxed VSTest still
  cannot bind its local communication socket. Identical host-local commands pass
  with Godot 4.6.3 Mono and .NET 8.
- Final host-local `./scripts/quality.sh` passes restore, formatting, a zero-warning
  build, 2,731 Domain tests, 586 Application tests, 1,220 Infrastructure tests,
  all 8 architecture tests, scenario validation, every credential-free Godot
  smoke, and the Compatibility renderer launch.
- Session review covered exact-reference provenance, one-shot behavior, fresh
  evidence, unchanged prior owners and authoritative state, model-call counts,
  dependency order, inert startup, docstrings, 120-character limits, the 500-line
  focused smoke, formatting, and whitespace. Review removed a new tautological
  source assertion, added nullable verifier preflight, and verified the guarded
  author's exact typed binding before retaining output.

## Exact next bounded slice

Take **Phase 7qf: compose the exact admission-guarded Move author output into one
fresh generic sabotage request without starting consideration**.

Acceptance boundary:

1. Test-first add one scene-owned, one-shot composition seam that accepts only
   `CharacterSabotageActionMoveAuthoringCoordinator.AuthorRequest` while that
   coordinator's exact admission, scheduled Move author request, typed candidate
   binding, cycle boundary, authorization, completion, and empty ordinary-turn
   graph all remain current.
2. Produce one fresh `CharacterSabotageActionConsiderationRequest` containing the
   exact admitted actor and only canonical `Move`. Retain the exact guarded source
   request and admission; do not rebuild or copy the candidate, evidence,
   placement, actor, completion, boundary, or authorization.
3. Reject null, rebuilt-request, rebuilt-admission, replaced-binding, foreign-actor,
   wrong-kind, changed-slot, active-slot, stale, and repeated inputs without
   changing either author, any prior owner, authoritative state, task, result,
   model source, or model count.
4. Extend only the focused switch-gated Move proof. Ordinary startup must remain
   inert; do not start consideration, supply or consume a model, select an action,
   reset either author, reserve the ordinary turn, or clear prior provenance.
5. Do not add cadence, timers, provider transport, credentials, persistence,
   UI/input, navigation, movement, perception, dialogue delivery, new Domain
   behavior, or a claim that ordinary enemy-agent sabotage is enabled.
6. Update README/ROADMAP, run `./scripts/quality.sh`, review the complete diff, and
   leave the next slice to start only that exact fresh composed request explicitly.
