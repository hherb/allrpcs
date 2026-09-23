# Allrpcs Handover

## Current state

`main` includes merged PR #339 at `72b5fa3`. Branch
`codex/phase-7-follow-up-move-terminal-route` completes **Phase 7qq** through **Phase 7qv**. The
implementation is committed at `9f392c1` and pushed to `origin`. Draft PR creation is pending
explicit destination confirmation for `github.com/hherb/allrpcs` after the environment disclosure
guard rejected the authorized command. The unrelated untracked `scripts/code_statistics.py` remains
untouched and must stay unstaged.

The acknowledged follow-up Move now continues through six explicit, dependency-ordered seams:

- `CharacterSabotageActionMoveTerminalRouteCoordinator` retains only the exact guarded author
  request, canonical Move kind, and current recovered placement, then resets only those retained
  route references once while recording exact request/kind provenance.
- `CharacterSabotageActionMoveTurnCompletionCoordinator` exposes one fresh exact completed-turn
  token only after that reset while the recovery owner and authoritative placement remain current.
- The existing pure scheduling gate accepts only that exact current token;
  `CharacterSabotageActionMoveTurnAdmissionCoordinator` retains it once without copying provenance.
- `CharacterSabotageActionMoveOrdinaryTurnBoundaryCoordinator` binds only that exact admission and
  the existing empty ordinary-turn reference without reserving the slot.

Null, rebuilt-request, foreign-actor, wrong-kind, rebuilt/replaced-placement, stale-authority,
foreign-chain, active-slot, and repeated paths remain inert. Both model call counts remain one. No
scheduling authorization, slot reservation, another cycle, author reset, provider, credential,
persistence, UI/input, navigation, perception, dialogue, cadence, timer, or new Domain behavior was
added. Ordinary startup remains inert.

## Validation and review

- Each implementation seam was test-first; its focused build first failed only for the deliberately
  absent next verifier.
- The focused switch-gated Move proof passes host-local with Godot 4.6.3 Mono and .NET 8 through the
  exact follow-up terminal route, reset, completion, pure scheduling gate, admission, and empty
  ordinary-turn boundary.
- Sandboxed Godot cannot open `user://logs`; each identical focused command passes host-local.
- Final host-local `./scripts/quality.sh` passes restore, formatting, a zero-warning build, 2,731
  Domain tests, 586 Application tests, 1,220 Infrastructure tests, all 8 architecture tests,
  scenario validation, every credential-free Godot smoke, and the Compatibility renderer launch.
- Session review covered exact-reference provenance, live recovery ownership, one-shot reset and
  completion, pure scheduling eligibility, admission/boundary freshness, rejected and repeated
  paths, model call counts, inert startup, docstrings, file/line limits, formatting, and whitespace.
  The unused verifier parameter and one documentation wrap found during review were fixed; no
  unresolved findings remain.
- Hosted checks have not started because the draft PR could not be created without the additional
  explicit GitHub-destination confirmation described above.

## Exact next bounded slice

Take **Phase 7qw: authorize ordinary scheduling for only the exact current follow-up Move boundary
without reserving the turn slot or starting another sabotage cycle**.

Acceptance boundary:

1. Test-first add one scene-owned, one-shot coordinator that accepts only
   `CharacterSabotageActionMoveOrdinaryTurnBoundaryCoordinator`'s exact admission and bound ordinary
   turn reference while its complete route, completion, admission, and empty-slot graph remains
   current.
2. Retain one fresh `CharacterSabotageOrdinaryTurnSchedulingAuthorization` containing only those
   exact references. Do not reuse `CharacterSabotageOrdinaryTurnSchedulingAuthorizationCoordinator`;
   its one-shot authorization belongs to the completed first cycle.
3. Reject null, rebuilt-completion, replaced-admission, changed/active-slot, stale-authority,
   foreign-chain, and repeated paths without state changes or another model call.
4. Extend only the focused switch-gated Move proof. Do not consume the authorization, enter another
   cycle boundary, invoke an action author, reset either author, or reserve the ordinary turn.
5. Keep ordinary startup inert and preserve all existing exclusions: no cadence, timers, provider
   transport, credentials, persistence, UI/input, navigation, perception, dialogue delivery, or new
   Domain behavior.
6. Update README/ROADMAP, run `./scripts/quality.sh`, review the complete diff, and leave the next
   slice to consume only that exact still-current authorization at a distinct follow-up cycle
   boundary without reserving the slot or starting an action.
