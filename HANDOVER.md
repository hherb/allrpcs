# Allrpcs Handover

## Current state

`main` includes merged PR #335 at `f4eedce`. Branch
`codex/phase-7-sabotage-action-authoring-composition` completes **Phase 7qf** and
**Phase 7qg**. Publication details are recorded below after the draft PR is opened.
The unrelated untracked `scripts/code_statistics.py` remains untouched and must
stay unstaged.

`CharacterSabotageActionMoveCompositionCoordinator` accepts only the exact
admission-guarded follow-up Move author output while its typed candidate binding,
cycle boundary, authorization, completed-turn admission, and empty ordinary-turn
slot remain current. It retains the exact admission and guarded author request,
then creates one fresh generic request containing the exact actor and only
canonical `Move`.

`CharacterSabotageActionMoveCompositionStartCoordinator` explicitly starts only
that exact fresh request on a fresh scene-owned generic sabotage task. Rejected
null, source-author, rebuilt, stale-graph, occupied-task, and repeated preflight
paths do not consume the queued one-shot model. A successful start retains the
exact request and calls the fresh model once.

The new request is not observed, acknowledged, selected, bound to its typed Move
candidate, dispatched, applied, or reset. Neither author is reset, the ordinary
turn remains empty, all first-cycle owners and authoritative placement/evidence
remain unchanged, and ordinary startup remains inert. No cadence, timer,
provider, credential, persistence, UI/input, navigation, movement, perception,
dialogue, or new Domain behavior was added.

## Validation and review

- Both slices were test-first. The focused game build failed only for each
  deliberately missing coordinator before its implementation was added.
- The focused switch-gated Move route passes host-local through exact recovery,
  terminal reset, completion, scheduling admission, authorization, cycle and
  authoring boundaries, guarded follow-up authoring, fresh composition, and one
  explicit fresh-task start. The new model is called once; its result remains
  unobserved and the first-cycle model counts remain unchanged.
- Sandboxed Godot still cannot open `user://logs`, and sandboxed VSTest still
  cannot bind its local communication socket. Identical host-local commands pass
  with Godot 4.6.3 Mono and .NET 8.
- Final host-local `./scripts/quality.sh` passes restore, formatting, a
  zero-warning build, 2,731 Domain tests, 586 Application tests, 1,220
  Infrastructure tests, all 8 architecture tests, scenario validation, every
  credential-free Godot smoke, and the Compatibility renderer launch.
- Session review covered exact-reference provenance, full graph revalidation,
  one-shot behavior, rejected-preflight model preservation, fresh task isolation,
  unchanged prior owners and authoritative state, inert startup, docstrings,
  120-character limits, the 500-line focused smoke, formatting, and whitespace.
  Review corrected an overbroad README coverage claim and restored the focused
  smoke to the 500-line limit. No unresolved code-review findings remain.

## Exact next bounded slice

Take **Phase 7qh: observe one exact completed fresh generic Move result without
acknowledging or dispatching it**.

Acceptance boundary:

1. Test-first add one scene-owned, one-shot observation seam for the scheduled
   generic task. It must accept only `StartedRequest` from
   `CharacterSabotageActionMoveCompositionStartCoordinator` after that exact task
   reaches `Completed` and its result retains the same exact request.
2. Revalidate the complete current composition/start graph before retaining the
   exact completed result. The selected result must retain the exact admitted
   actor and canonical `Move`; do not rebuild its request, decision, actor, kind,
   admission, candidate binding, boundary, authorization, or completion.
3. Reject null, running, failed, cancelled, rebuilt-result, rebuilt-request,
   foreign-actor, wrong-kind, stale-graph, replaced-task, and repeated paths
   without consuming another model or changing any prior owner or state.
4. Extend only the focused switch-gated Move proof. Do not acknowledge or reset
   the scheduled task, bind the selected typed candidate, compose or start the
   action-specific Move request, apply another move, reserve the ordinary turn,
   clear provenance, or add automatic scheduling.
5. Keep ordinary startup inert and preserve all existing exclusions: no cadence,
   timers, provider transport, credentials, persistence, UI/input, navigation,
   movement, perception, dialogue delivery, or new Domain behavior.
6. Update README/ROADMAP, run `./scripts/quality.sh`, review the complete diff, and
   leave the following slice to acknowledge and bind only that exact observed
   result to the retained follow-up Move candidate.
