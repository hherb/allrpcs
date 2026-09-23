# Allrpcs Handover

## Current state

`main` includes merged PR #338 at `5397400`. Branch
`codex/phase-7-sabotage-action-move-recovery-candidate` completes **Phase 7qn**, **Phase 7qo**, and
**Phase 7qp**. The unrelated untracked `scripts/code_statistics.py` remains untouched and must stay
unstaged.

The acknowledged typed follow-up Move route now owns three explicit, dependency-ordered seams:

- `CharacterSabotageActionMoveRecoveryCandidateCoordinator` revalidates the exact acknowledged
  result and fresh authoritative snapshots, resolves the occurrence target's single exact placement
  from the typed request's pre-move snapshot, and retains one fresh exact recovery candidate.
- `CharacterSabotageActionMoveRecoveryEligibilityCoordinator` pairs only that exact candidate with
  one explicitly supplied evidence view accepted against the live placement/evidence state by the
  existing recovery policy.
- `CharacterSabotageActionMoveRecoveryCoordinator` delegates only that exact published pair to the
  existing stale-safe Move controller, retains the exact fresh restored placement snapshot, and
  preserves the authoritative evidence snapshot.

Null, rebuilt-result, rebuilt-placement, rebuilt-evidence, rebuilt/foreign-candidate,
missing/ambiguous-source-placement, unseen-evidence, stale-authority, and repeated paths remain
inert. No terminal route, turn completion/admission, ordinary-turn reservation, author reset,
additional cycle, cadence, timer, provider, credential, persistence, UI/input, navigation,
perception, dialogue, or new Domain behavior was added. Ordinary startup remains inert and both
follow-up models are called once.

## Validation and review

- Each slice was test-first; its focused build first failed only for the deliberately absent next
  coordinator.
- The focused switch-gated Move proof passes host-local with Godot 4.6.3 Mono and .NET 8 through
  exact candidate authoring, evidence-view eligibility, and stale-safe recovery.
- Sandboxed Godot cannot open `user://logs`; each identical focused command passes host-local.
- Final host-local `./scripts/quality.sh` passes restore, formatting, a zero-warning build, 2,731
  Domain tests, 586 Application tests, 1,220 Infrastructure tests, all 8 architecture tests,
  scenario validation, every credential-free Godot smoke, and the Compatibility renderer launch.
- Session review covered exact-reference provenance, source-placement cardinality, live-state
  revalidation, policy delegation, compare-and-swap recovery, evidence preservation, rejected and
  repeated paths, model call counts, inert startup, docstrings, file/line limits, formatting, and
  whitespace. No unresolved findings remain.

## Exact next bounded slice

Take **Phase 7qq: retain the exact recovered follow-up Move as a terminal route without resetting
it or completing the sabotage turn**.

Acceptance boundary:

1. Test-first add one scene-owned, one-shot coordinator that accepts only the exact generic author
   request and canonical Move kind retained by the follow-up chain, plus the exact fresh recovered
   placement token retained by `CharacterSabotageActionMoveRecoveryCoordinator` while it remains
   the live Move state.
2. Retain those exact tokens as one follow-up terminal route without copying the recovered snapshot.
   Do not reuse `CharacterSabotageTerminalRouteCoordinator`: its one-shot reset provenance belongs
   to the completed first cycle and deliberately prevents a second retention.
3. Reject null, rebuilt-author-request, wrong-kind, rebuilt/replaced-placement, stale-authority,
   foreign-chain, and repeated paths without state changes or another model call.
4. Extend only the focused switch-gated Move proof. Do not reset the new terminal route, complete or
   admit the sabotage turn, reserve the ordinary turn, reset either author, or start another cycle.
5. Keep ordinary startup inert and preserve all existing exclusions: no cadence, timers, provider
   transport, credentials, persistence, UI/input, navigation, perception, dialogue delivery, or new
   Domain behavior.
6. Update README/ROADMAP, run `./scripts/quality.sh`, review the complete diff, and leave the next
   slice to reset only that exact retained follow-up terminal route without completing the turn.
