# Allrpcs Handover

## Current state

`main` includes merged PR #313 at `45ad065`. Branch
`codex/phase-7-delay-recovery-start` advances Phase 7 through four bounded slices:

- Phase 7ny dependency-orders `CharacterTargetSabotageDelayRecoveryStartCoordinator` in
  `Main.tscn`. Only the existing switch-gated target-delay smoke calls it after one exact applied
  delay; repeated start preserves the exact candidate/request graph and consumes no second model.
- Phase 7nz adds `CharacterSabotageActionConsiderationRequest`, retaining one exact actor and a
  stable one-pass defensive copy of distinct caller-supplied canonical sabotage kinds.
- Phase 7oa accepts only that actor's exact request-member action kind or explicit abstention and
  binds the decision immutably to its exact request.
- Phase 7ob adds `ICharacterSabotageActionDecisionModel` and an Application controller that forwards
  the exact request and caller cancellation once, then returns only a validated result.

The action-level contracts do not infer physical, informational, capability, or current-state
eligibility; callers must prefilter kinds through the existing action-specific policies. Nothing
selects a target or action-specific candidate, dispatches or applies an action, starts work, binds a
provider, triggers recovery autonomously, authors events/evidence, persists state, or adds input/UI,
timing, navigation, movement, perception, dialogue, or other world behavior. The unrelated
untracked `scripts/code_statistics.py` remains untouched and must stay unstaged.

## Validation and review

- Test-first compilation failed only on each absent action-level contract, and the focused
  main-scene smoke failed on the absent delay-recovery start node before the smallest
  implementations were added.
- Focused Domain tests pass 17 sabotage-action tests; focused Application tests pass all 5 new
  controller tests; the updated target-delay main-scene smoke passes host-locally.
- The sandbox reached a zero-warning build before the known VSTest loopback
  `SocketException (13): Permission denied`; the identical host-local `./scripts/quality.sh` passed.
- Host validation passes workflow checks, restore, formatting, a zero-warning build, 2,728 Domain
  tests, 583 Application tests, 1,160 Infrastructure tests, all 8 architecture tests, scenario
  validation, every credential-free Godot smoke, and the Compatibility renderer launch.
- Session review covers exact actor/kind identity, stable single enumeration and defensive copying,
  null/duplicate/member gates, accepted abstention, invalid/null model output, exact cancellation
  forwarding, failure propagation, closed read-only surfaces, delay-recovery dependency order,
  repeated-start preservation, ordinary inertness, documentation, file/line limits, whitespace,
  and scoped paths. No unresolved finding remains.

## Exact next bounded slice

Take **Phase 7oc: strictly deserialize one sabotage-action decision**.

Acceptance boundary:

1. Test-first add an Infrastructure deserializer for one complete bounded JSON object containing
   exactly `actorId` and nullable `selectedActionKind`.
2. Require `actorId` to match the request actor text ordinally and rebind it to the request's exact
   identifier; accept only `null` or the canonical text of one exact request-member action kind and
   rebind that selection to the exact member instance.
3. Reject malformed, trailing, commented, duplicate, missing, extra, non-string, over-depth,
   oversized, wrong-actor, unavailable-kind, null-request, and null-document input through one
   stable content-free failure without changing request state.
4. Delegate final acceptance to `CharacterSabotageActionDecisionPolicy`; update README/ROADMAP, run
   `./scripts/quality.sh`, and review the complete diff.
5. Do not add a JSON schema or request serializer, semantic/SSE accumulation, provider adapter or
   binding, retry/task/scene ownership, action-specific target/candidate dispatch, autonomous
   triggering, authoritative mutation, evidence authorship, persistence, input/UI, timing,
   navigation, movement, perception, dialogue, or other world behavior.
