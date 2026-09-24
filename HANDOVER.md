# Allrpcs Handover

## Current state

`main` includes merged PR #347 at `fbced06`. Branch
`codex/phase-7-continuation-move-applied-result` completes **Phase 7sd** through **Phase 7sh**. The
implementation is committed at `25db309` and published in draft PR #348 targeting `main`. The
unrelated untracked `scripts/code_statistics.py` remains untouched and must stay unstaged.

The exact fourth-cycle continuation Move now completes its guarded applied-result and recovery path:

- Only its exact completed typed result is observed with the fresh placement and evidence snapshots
  installed by its task; the typed request's pre-application snapshots need not remain current.
- Only that exact observation and authority are bound and acknowledged. Its typed task alone resets,
  while the exact result and snapshot tokens remain retained as downstream provenance.
- One recovery candidate derives the unique exact pre-move source placement from that provenance.
  A distinct eligibility owner pairs it with one explicitly supplied visible-evidence view.
- Only that exact eligible pair reaches the existing stale-safe Move controller, which restores the
  source placement and retains the accumulated authoritative evidence.

Null, running, rebuilt-result/request/candidate, foreign-actor, wrong-candidate, unseen-evidence,
replaced-authority, stale, and repeated paths remain inert. Both continuation models and all six
prior generic/typed models are each called once, the ordinary turn stays empty, and ordinary startup
remains inert. No terminal route, turn completion, ordinary-turn reservation, automatic cadence,
provider, credential, persistence, UI/input, navigation, perception, dialogue, timer, or new Domain
behavior was added.

## Validation and review

- Each new coordinator was introduced test-first with the focused game build failing for the
  deliberately absent type before implementation.
- The focused switch-gated Move proof passes host-local with Godot 4.6.3 Mono and .NET 8 through the
  exact restored placement while all authoritative snapshots and model-call counts remain exact.
- Review added explicit foreign-actor and selected wrong-candidate rejection to the applied-result
  proof and updated the smoke success report to describe the completed recovery boundary.
- Final host-local `./scripts/quality.sh` passes a zero-warning build, 2,731 Domain, 586 Application,
  1,220 Infrastructure, and 8 architecture tests, scenario validation, every credential-free Godot
  smoke, formatting, and the Compatibility launch.
- Draft PR #348's five hosted jobs failed with `steps: []`; no hosted product step executed, so this
  is Actions/account infrastructure state rather than contradictory product-test evidence.

## Exact next bounded slice

Take **Phase 7si: retain the exact recovered continuation Move as a distinct terminal route without
resetting it**.

Acceptance boundary:

1. Test-first add one terminal-route coordinator over the exact continuation author request,
   canonical `Move` kind, and Phase 7sh recovered placement/evidence authority.
2. Require the exact retained continuation generic acknowledgement/candidate provenance and the
   exact still-current recovery output. Reject null, rebuilt or foreign author, wrong kind, replaced
   placement/evidence, stale, malformed, and repeated inputs without changing any owner.
3. Retain only those exact references once. Do not reset the route, clear upstream recovery,
   complete or admit a turn, reserve the ordinary-turn slot, authorize or enter another cycle,
   invoke another author/model, or add cadence.
4. Extend only the focused switch-gated Move proof. Keep ordinary startup inert and all eight model
   call counts at one.
5. Preserve all existing exclusions: no timers, provider transport, credentials, persistence,
   UI/input, navigation, perception, dialogue delivery, or new Domain behavior.
6. Update README/ROADMAP, run `./scripts/quality.sh`, review the complete diff, and leave the next
   slice to reset only that exact terminal route while preserving recovery authority.
