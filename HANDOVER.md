# Allrpcs Handover

## Current state

`main` includes merged PR #344 at `c21e1e6`. Branch
`codex/phase-7-third-move-recovery` completes **Phase 7rj** through **Phase 7rm**. The implementation
is committed at `a6122a0` and published in draft PR #345 targeting `main`. The unrelated untracked
`scripts/code_statistics.py` remains untouched and must stay unstaged.

The exact observed applied third-cycle Move now advances through four bounded seams:

- An applied-result binding coordinator binds only the exact result and fresh authoritative
  placement/evidence snapshots, then resets only its typed task and retains those downstream tokens.
- A recovery-candidate coordinator derives the unique exact pre-move source placement from the
  retained typed request and selected third-cycle candidate.
- An eligibility coordinator pairs only that exact recovery candidate with one explicitly supplied
  current evidence view accepted by the existing recovery policy.
- A recovery coordinator delegates only that exact pair through the existing stale-safe Move state
  owner, retaining the fresh restored placement and unchanged authoritative evidence.

Null, rebuilt-result/request/candidate/snapshot/view, foreign-candidate, unseen-evidence,
stale-authority, running, and repeated paths remain inert. Each of the four bounded model calls still
occurs once, the ordinary turn stays empty, and ordinary startup remains inert. No terminal route,
route reset, turn completion, ordinary-turn reservation, cadence, author reset, provider, credential,
persistence, UI/input, navigation, perception, dialogue, timer, or new Domain behavior was added.

## Validation and review

- Each slice was test-first: the focused game build failed only for the deliberately absent binding,
  recovery-candidate, eligibility, or recovery coordinator before its implementation.
- The focused switch-gated Move proof passes host-local with Godot 4.6.3 Mono and .NET 8 through
  exact binding, acknowledgement, recovery-candidate authoring, eligibility, and stale-safe recovery.
- Sandboxed Godot could not open `user://logs`; the identical focused command passed host-local.
- Final host-local `./scripts/quality.sh` passes a zero-warning build, 2,731 Domain, 586 Application,
  1,220 Infrastructure, and 8 architecture tests, scenario validation, every credential-free Godot
  smoke, and the Compatibility launch. Formatting, whitespace, and the complete diff also pass.
- Draft PR #345's hosted jobs failed with `steps: []`; no hosted product step executed, so this is
  Actions/account infrastructure state rather than contradictory product-test evidence.

## Exact next bounded slice

Take **Phase 7rn: retain the exact recovered third-cycle Move as one terminal route**.

Acceptance boundary:

1. Test-first add one terminal-route coordinator over the exact third-cycle recovery, its guarded
   author request/canonical `Move` kind, and current recovered placement/evidence authority.
2. Retain only that exact still-current graph once; do not copy the placement output or clear any
   upstream provenance.
3. Reject null, rebuilt-request/placement, foreign actor, wrong kind, replaced authority, stale
   recovery, and repeated inputs without changing prior owners, authoritative state, ordinary turn,
   or any model call count.
4. Extend only the focused switch-gated Move proof. Do not reset the route, complete the sabotage
   turn, reserve the ordinary turn, authorize scheduling, start another cycle, or add cadence.
5. Keep ordinary startup inert and preserve all existing exclusions: no timers, provider transport,
   credentials, persistence, UI/input, navigation, perception, dialogue delivery, or new Domain
   behavior.
6. Update README/ROADMAP, run `./scripts/quality.sh`, review the complete diff, and leave the next
   slice to reset only that exact retained terminal route while preserving recovery authority.
