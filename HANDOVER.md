# Allrpcs Handover

## Current state

`main` includes merged PR #348 at `7ba6560`. Branch
`codex/phase-7-continuation-terminal-route` continues the exact fourth-cycle Move chain after its
recovery and completes **Phase 7si** plus the next three bounded seams.

The recovered continuation Move now:

- retains one distinct terminal route only from the exact guarded author request, canonical `Move`
  kind, retained generic acknowledgement/candidate provenance, and current recovered placement and
  evidence authority;
- resets only that route's references once while preserving recovery authority and recording the
  exact reset author/kind provenance;
- exposes one fresh completed-turn token only from that exact reset route; and
- admits only that exact token through the existing pure scheduling gate.

Null, rebuilt, foreign-actor, wrong-kind, replaced-authority, stale, malformed, and repeated inputs
remain inert. The ordinary turn is still empty and unbound. No slot reservation, scheduling
authorization, further cycle, author/model invocation, cadence, provider, credential, persistence,
UI/input, navigation, perception, dialogue, timer, or new Domain behavior was added. The unrelated
untracked `scripts/code_statistics.py` remains untouched and must stay unstaged.

## Validation and review

- Each of the four seams was introduced test-first with the focused game build failing for the
  deliberately absent coordinator or API before implementation.
- The focused switch-gated Move proof passes host-local with Godot 4.6.3 Mono and .NET 8 through
  exact terminal retention, reset, completion, and pure scheduling admission while authoritative
  placement/evidence and all model-call counts remain unchanged.
- Final host-local `./scripts/quality.sh` passes a zero-warning build, 2,731 Domain, 586 Application,
  1,220 Infrastructure, and 8 architecture tests, scenario validation, every credential-free Godot
  smoke, formatting, and the Compatibility launch.
- Complete-diff review found no additional correctness, exact-reference, scene-ordering, formatting,
  or documentation issues after the smoke success report was updated to the new terminal boundary.

## Exact next bounded slice

Bind the exact admitted continuation completion to the still-empty ordinary-turn boundary without
reserving the slot.

Acceptance boundary:

1. Test-first add one boundary coordinator over the exact continuation admission and the exact
   current `CharacterTurnActivityController.Activity` reference.
2. Delegate eligibility to `CharacterSabotageOrdinaryTurnBoundaryEligibilityPolicy`; require the
   exact admitted completion and the same still-startable empty turn reference.
3. Reject null, rebuilt, foreign, wrong-kind, stale-admission, changed-slot, active-slot, and
   repeated inputs without changing any owner.
4. Retain only the exact completion and empty turn references once. Do not reserve or replace the
   turn, authorize scheduling, enter another cycle, invoke another author/model, or add cadence.
5. Extend only the focused switch-gated Move proof. Keep ordinary startup inert and every model
   call count at one.
6. Preserve all existing exclusions: no timers, provider transport, credentials, persistence,
   UI/input, navigation, perception, dialogue delivery, or new Domain behavior.
7. Update README/ROADMAP, run `./scripts/quality.sh`, review the complete diff, and leave the next
   slice to retain one fresh scheduling authorization from only that exact current boundary.
