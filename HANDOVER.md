# Allrpcs Handover

## Current state

`main` includes merged PR #353 at `fba1ba8`. Branch
`codex/phase-7-fifth-move-typed-request` advances the exact fifth Move cycle through two bounded
seams: typed-request composition and explicit typed-task start. The reviewed implementation is
committed at `9fd9b02` and published in draft PR #354 targeting `main`.

The new path:

- composes only the exact retained fifth-cycle acknowledged result and source-owned candidate
  binding into one fresh `CharacterObjectSabotageMoveConsiderationRequest`;
- retains the exact actor, physical access, world objects, current authoritative placement and
  evidence snapshots, and only the fifth candidate;
- starts only that same still-current request on a fresh scene-owned typed task with a fresh
  one-shot model source; and
- preserves the queued model across rejected preflight and leaves successful completion unobserved.

Null, rebuilt-result, rebuilt-binding, wrong-candidate, stale-authority, occupied, and repeated
inputs remain inert. No typed result is observed or acknowledged; no movement is recovered; the
ordinary turn remains empty; and all earlier model-call counts and retained provenance remain
unchanged. No cadence, provider transport, credentials, persistence, UI/input, navigation,
perception, dialogue, timer, or new Domain behavior was added. Ordinary startup remains inert. The
unrelated untracked `scripts/code_statistics.py` remains untouched and must stay unstaged.

## Validation and review

- Both seams were introduced test-first. Focused game builds first failed with `CS0246` for each
  absent coordinator, then passed after the minimum implementation was added.
- The focused switch-gated Move recovery proof passes host-local with Godot 4.6.3 Mono and .NET 8,
  including exact typed composition and start while every earlier model-call count remains one.
- Final host-local `./scripts/quality.sh` passes a zero-warning build, 2,731 Domain, 586 Application,
  1,220 Infrastructure, and 8 architecture tests, scenario validation, every credential-free Godot
  smoke, formatting, and the Compatibility launch.
- The sandboxed quality run reached the zero-warning build but VSTest could not bind its loopback
  socket (`SocketException (13): Permission denied`); the identical host-local run passed.
- Complete-diff review found and fixed a missing still-current predecessor-graph revalidation in
  typed composition; the final review found no remaining provenance, scene-ordering, public-surface,
  formatting, line-length, documentation, or whitespace issues.
- Draft PR #354's five hosted jobs completed as failures with `steps: []`; no hosted product step
  executed, so this is Actions/account infrastructure state rather than contradictory product-test
  evidence.

## Exact next bounded slice

Observe only the exact completed applied fifth-cycle typed Move result together with the fresh
placement and evidence snapshots installed by its task, stopping before acknowledgement or reset.

Acceptance boundary:

1. Test-first add one distinct applied-result observation coordinator over only the exact started
   request, task-installed completed result, and exact fresh resolved placement/evidence snapshots.
2. Revalidate the complete retained acknowledgement, candidate binding, typed request, and start
   graph; require canonical selection of the exact fifth candidate.
3. Reject null, running, rebuilt-request, rebuilt-result, foreign-actor, wrong-candidate,
   stale-authority, stale-graph, and repeated inputs without changing any owner.
4. Retain the exact result and task-installed snapshots once; do not require the typed request's
   pre-application placement/evidence snapshots to remain current after successful application.
5. Do not acknowledge or reset the typed task, recover movement, author recovery, reserve the
   ordinary turn, clear provenance, or invoke another model.
6. Extend only the focused switch-gated Move proof. Keep ordinary startup inert.
7. Preserve all existing exclusions: no cadence, timers, provider transport, credentials,
   persistence, UI/input, navigation, perception, dialogue delivery, or new Domain behavior.
8. Update README/ROADMAP, run `./scripts/quality.sh`, review the complete diff, and leave the next
   slice to bind and acknowledge only that exact applied result and authoritative snapshots.
