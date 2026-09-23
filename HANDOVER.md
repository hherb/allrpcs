# Allrpcs Handover

## Current state

`main` includes merged PR #327 at `bdf5e1d`. Branch
`codex/phase-7-sabotage-delay-recovery-request` completes **Phase 7pn** and **Phase 7po**.

`CharacterSabotageOutcomeDelayRecoveryCoordinator` now composes the retained exact Delay eligibility
through the existing recovery-request owner, then explicitly starts only that exact request. It
rechecks the generic author and Delay selection, exact typed candidate/result, branch
acknowledgement, applied delay outcome, eligibility-owned recovery candidate, and current delay
event-log/world-time authority immediately before each one-shot boundary. Composition retains the
typed request's exact authoritative world-event definitions and never rebuilds the candidate.
Starting initializes the existing recovery authority from the exact composed snapshots and consumes
one model only after stale/rebuilt/repeated preflight has passed.

The switch-gated main-scene proof carries one real generic Delay selection through typed application,
eligibility, exact recovery-request composition, and an explicit pending-model start. It proves
ordinary startup remains inert and quits before recovery decision completion, resolution, or
application. No Domain rule, autonomous scheduling, influence routing, persistence, transport,
credentials, UI/input, timing, navigation, movement, perception, or dialogue behavior was added.
The unrelated untracked `scripts/code_statistics.py` remains untouched and must stay unstaged.

## Validation and review

- The focused main-scene proof passes host-local with Godot 4.6.3 Mono after both the composition
  and explicit-start slices. Sandboxed launches hit the known `user://logs` permission crash; the
  identical host-local commands passed.
- Final host-local `./scripts/quality.sh` passes restore, formatting, a zero-warning build, 2,731
  Domain tests, 586 Application tests, 1,220 Infrastructure tests, all 8 architecture tests,
  scenario validation, every credential-free Godot smoke, and the Compatibility renderer launch.
  The sandboxed run reached the zero-warning build before VSTest hit its known loopback permission
  boundary; the identical host-local run passed. The full suite passed again after review fixes.
- Session-wide review covered exact author/result/candidate provenance, branch acknowledgement,
  current delay and recovery authority, exact world-event/request identity, rejected-call model
  preservation, one-shot behavior, partial-mutation safety, scene order, ordinary-startup inertness,
  public-surface containment, documentation, file size, formatting, and whitespace. Review found
  that adding both slices directly would take the existing multi-branch coordinator past its
  practical file-size boundary, so the Delay path was extracted into a focused 178-line coordinator;
  no further defect was found.

## Exact next bounded slice

Take **Phase 7pp: bind the completed exact delay recovery back to its generic author route**.

Acceptance boundary:

1. Test-first extend the strongly typed route to accept only Phase 7po's exact started recovery
   request after the existing recovery task completes with that same request and exact candidate.
2. Require the existing recovery resolution owner to have installed the exact append-only recovery
   result from the started request while its fresh event-log and world-time snapshots remain current.
3. Explicitly acknowledge only that same completed result through the existing resolution owner,
   then retain the exact recovered result and snapshots as the generic route's terminal Delay output.
4. Null, abstained, rebuilt, replaced, foreign, wrong-branch, stale, unstarted, unresolved,
   non-current, and repeated paths must fail closed without consuming another model or changing
   delay state, recovery state, request state, or another branch.
5. Extend the switch-gated Delay proof with a completing recovery model and prove one exact selected
   recovery is applied and acknowledged once; ordinary startup and the five earlier branch proofs
   must remain unchanged.
6. Do not route Influence, schedule sabotage, persist state, or add provider transport, credentials,
   UI/input, timing, navigation, movement, perception, dialogue delivery, or new Domain behavior.
7. Update README/ROADMAP, run `./scripts/quality.sh`, review the complete diff, and leave the next
   slice to compose only the retained exact Influence eligibility into its existing recovery request.
