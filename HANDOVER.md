# Allrpcs Handover

## Current state

`main` includes merged PR #306 at `e1532b2`. Branch
`codex/phase-7-delay-recovery-decision` continues target-delay recovery through seven bounded
slices:

- Phase 7na–7nb add one provider-neutral exact selection-or-abstention decision, immutable accepted
  result, and pure consideration transition. Selection requires the request actor's exact identifier,
  the exact request-member candidate, that candidate's exact target identifier, and unchanged
  authoritative event/log/time eligibility. Abstention remains inert; selection delegates through
  the existing append-only recovery transition.
- Phase 7nc adds the provider-neutral decision-model interface and cancellation-aware Application
  controller. It forwards one exact request and caller token, then relies on the Domain result to
  reject foreign, stale, null, or otherwise invalid output.
- Phase 7nd–7ng add a construction-only OpenAI binding, configured/one-shot/offline scene model
  source, isolated bounded decision task, and explicit start flow. Rejected overlap preserves a
  queued one-shot model; completion installs only an accepted result on the main thread; cancellation
  and terminal reset remain explicit.

Nothing decodes provider output, sends HTTP, accesses credentials, composes provider dependencies,
applies recovery automatically, owns authoritative recovery state, schedules work automatically,
authors evidence, persists state, or adds input/UI, navigation, movement, perception, dialogue, or
other world behavior. The unrelated untracked `scripts/code_statistics.py` remains untouched and
must stay unstaged.

## Validation and review

- Every slice began with a focused missing-contract compile failure. Focused final checks pass 7
  decision/result tests, 3 consideration-transition tests, 6 Application-controller tests, 3 binding
  tests, a zero-warning Game build, and the model-source, task, and explicit-flow Godot smokes.
- The sandbox reproduced the known VSTest loopback/MSBuild IPC restrictions and Godot `user://logs`
  crash; identical host-local focused commands passed without product changes.
- Host-local `./scripts/quality.sh` passes workflow validation, restore, formatting, a zero-warning
  build, 2,720 Domain tests, 578 Application tests, 1,091 Infrastructure tests, all 8 architecture
  tests, scenario validation, every credential-free Godot smoke, and the final Compatibility launch.
- Session review covers exact actor/target/candidate identity, full current-state eligibility,
  immutable abstention and append-only recovery, request preservation, nulls, cancellation, overlap,
  one-shot precedence and preservation, stable offline failure, closed public surfaces, documentation,
  file and line limits, formatting, whitespace, and scoped staged paths. No unresolved finding remains.

## Exact next bounded slice

Take **Phase 7nh: strictly decode one bounded provider target-delay recovery decision**.

Acceptance boundary:

1. Test-first add one Infrastructure JSON deserializer for the exact current
   `CharacterTargetSabotageDelayRecoveryConsiderationRequest`. Accept only a complete closed object
   containing `actorId` and nullable integer `selectedCandidateIndex`, with no comments, trailing
   content/commas, duplicate, missing, or unknown properties, wrong types, excessive depth, or input
   beyond the existing 4,096-code-unit transport boundary.
2. Rebind value-equal actor text and an in-range candidate index to the request's exact actor and
   candidate instances, then delegate final acceptance to
   `CharacterTargetSabotageDelayRecoveryDecisionPolicy`. Reject foreign actors, negative/out-of-range
   indices, stale state, wrong target ownership, and otherwise ineligible selections through one
   bounded content-free error; accepted abstention remains exact and inert.
3. Leave the request actor, authoritative event definitions, event log, world time, candidates, and
   every nested instance unchanged. Do not yet add schemas, request serialization, stream/SSE
   accumulation, HTTP, credentials, an OpenAI model, composition, main-scene ownership, automatic
   application, evidence, persistence, or input/UI behavior.
4. Update README/ROADMAP, run `./scripts/quality.sh`, and review the complete diff.
