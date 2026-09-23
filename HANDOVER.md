# Allrpcs Handover

## Current state

`main` includes merged PR #328 at `03eba735`. Branch
`codex/phase-7-sabotage-delay-recovery-route` completes **Phase 7pp** and **Phase 7pq**.
Implementation commit `33c6608` is published in draft PR #329 targeting `main`.

`CharacterSabotageOutcomeDelayRecoveryCoordinator` now accepts only the exact completed recovery
for its own started generic Delay request after the existing resolution owner has installed the
selected exact candidate's fresh append-only event-log and world-time snapshots. It acknowledges
that same result once and retains the exact completed consideration, applied recovery, and snapshots
as terminal Delay output. Rebuilt, wrong-branch, stale, unresolved, non-current, and repeated calls
remain inert without another model call or state change.

`CharacterSabotageOutcomeInfluenceRecoveryCoordinator` now composes only the retained exact generic
Influence eligibility into the existing recovery-request owner. It rechecks the acknowledged generic
and typed branch, exact applied outcome, inverse candidate, and current relationship authority. It
does not initialize recovery authority, consume a model, start a task, resolve a decision, or apply
recovery. Ordinary startup remains inert. The unrelated untracked `scripts/code_statistics.py`
remains untouched and must stay unstaged.

## Validation and review

- Test-first game builds failed only for the deliberately missing Phase 7pp surface and then the
  deliberately missing Phase 7pq coordinator before each implementation was added.
- The focused switch-gated Delay smoke passes host-local with one exact recovery applied through the
  existing resolution owner, acknowledged once, and retained without a second model call.
- The focused switch-gated Influence smoke passes host-local with one exact recovery request composed
  while recovery authority, task, and model consumption remain inert.
- Sandboxed Godot still hits the known `user://logs` permission crash; identical focused commands
  pass host-local with Godot 4.6.3 Mono.
- Final host-local `./scripts/quality.sh` passes restore, formatting, a zero-warning build, 2,731
  Domain tests, 586 Application tests, 1,220 Infrastructure tests, all 8 architecture tests,
  scenario validation, every credential-free Godot smoke, and the Compatibility renderer launch.
  The sandboxed run reached the zero-warning build before VSTest hit its known loopback restriction;
  the identical host-local command passed.
- Session review covers exact author/result/candidate provenance, branch acknowledgement, current
  world/relationship authority, append-only Delay snapshots, one-shot acknowledgement, model
  preservation, scene order, ordinary-startup inertness, public-surface containment, file size,
  formatting, and whitespace. The over-500-line Delay smoke helper was extracted into a focused
  internal model; no further defect was found after final validation.
- Draft PR #329's five hosted jobs failed before execution with `steps: []`. No checkout, build,
  test, smoke, or unsigned development export step ran, so this is Actions/account infrastructure
  state rather than hosted product-test evidence.

## Exact next bounded slice

Take **Phase 7pr: explicitly start only the exact composed generic Influence recovery request**.

Acceptance boundary:

1. Test-first extend the focused Influence route to accept only Phase 7pq's exact author and composed
   request while its exact typed outcome, inverse candidate, and relationship authority remain current.
2. Initialize the existing Influence-recovery authority from that request's exact relationship and
   explicitly start only that same request through the existing candidate consideration flow.
3. Consume one queued recovery model only after the full exact graph passes preflight; null, rebuilt,
   replaced, foreign, wrong-branch, stale, already-started, occupied, and repeated paths must preserve
   the queued model and leave source, recovery, request, and other branch state unchanged.
4. Extend the switch-gated Influence proof with a pending recovery model and prove one exact explicit
   start. Quit before decision completion, resolution, acknowledgement, or recovery application.
5. Ordinary startup and all earlier branch proofs must remain unchanged.
6. Do not schedule sabotage, persist state, or add provider transport, credentials, UI/input, timing,
   navigation, movement, perception, dialogue delivery, or new Domain behavior.
7. Update README/ROADMAP, run `./scripts/quality.sh`, review the complete diff, and leave the next
   slice to bind the completed exact Influence recovery back to its generic author route.
