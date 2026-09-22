# Allrpcs Handover

## Current state

`main` includes merged PR #311 at `d935d4b`. Branch
`codex/phase-7-delay-recovery-resolution` adds three bounded target-delay recovery slices:

- `CharacterTargetSabotageDelayRecoveryResolutionCoordinator` observes each fresh exact completed
  task result once, resolves it through the recovery authority, and resets only while the observed
  result plus resolved event-log, world-time, and latest-result tokens remain current.
- `Main.tscn` dependency-orders that authority and coordinator with the existing source, task, and
  explicit flow. Ordinary startup remains uninitialized and inert; the switch-gated smoke alone
  initializes authority, proves one applied selection, guarded acknowledgement, and stable failure.
- `CharacterTargetSabotageDelayRecoveryCandidateController` owns one candidate for the exact current
  applied delay result and a caller-authored authoritative advancing recovery event. It is isolated
  and one-shot; stale, rebuilt, missing, recorded, non-advancing, null, and repeated inputs are inert.

Nothing automatically creates a recovery request, starts or schedules recovery work, authors a
recovery event, persists recovery, or adds input/UI or other world behavior. The candidate controller
is not in `Main.tscn`. The unrelated untracked `scripts/code_statistics.py` remains untouched and
must stay unstaged.

## Validation and review

- Test-first checkpoints proved the missing coordinator, missing main-scene nodes, and missing
  candidate controller before implementation. Both focused isolated smokes and the strengthened
  main-scene smoke pass.
- The sandbox reached a zero-warning build before the known VSTest loopback
  `SocketException (13): Permission denied`; the identical host-local `./scripts/quality.sh` passed.
- Host validation passes workflow checks, restore, formatting, a zero-warning build, 2,720 Domain
  tests, 578 Application tests, 1,160 Infrastructure tests, all 8 architecture tests, scenario
  validation, every credential-free Godot smoke, and the Compatibility launch.
- Session review covers one-shot result observation, all acknowledgement authority tokens,
  selection and abstention, stale-authority preservation, dependency order and ordinary inertness,
  exact-current candidate ownership, inapplicable recovery events, source immutability, closed
  surfaces, documentation, whitespace, line limits, and scoped paths. No unresolved finding remains.

## Exact next bounded slice

Take **Phase 7nt: retain one consideration request for the exact current target-delay recovery
candidate in an isolated controller**.

Acceptance boundary:

1. Test-first add `CharacterTargetSabotageDelayRecoveryCandidateRequestController`, depending only
   on the isolated candidate controller. Ordinary unavailable state remains inert.
2. Accept only the candidate controller's exact current candidate plus a caller-supplied world-event
   snapshot for which the existing recovery policy still accepts that candidate against its exact
   delay-result event-log and world-time authority.
3. Create one request retaining the exact occurrence target as actor, exact supplied world events,
   exact delay-result event log and world time, and exact candidate. Preflight an exact-actor
   abstention through the existing decision policy before installation.
4. Reject stale, rebuilt, foreign, missing-event, inapplicable, null, and repeated input without
   replacing the one-shot request. Prove exact nested identity, unchanged sources, and the closed
   surface in a dedicated isolated Godot smoke; update README/ROADMAP and run `./scripts/quality.sh`.
5. Do not yet add candidate/request controllers to `Main.tscn`, start the live recovery flow,
   schedule work, author events, persist recovery, or add input/UI behavior.
