# Allrpcs Handover

## Current state

`main` includes merged PR #346 at `bdd2709`. Branch
`codex/phase-7-third-move-next-cycle` completes **Phase 7rr** through **Phase 7sc**. The unrelated
untracked `scripts/code_statistics.py` remains untouched and must stay unstaged.

The exact recovered third-cycle Move now advances through a distinct continuation cycle:

- Its admitted completion binds once to the exact still-empty ordinary-turn boundary, produces one
  fresh scheduling authorization, and enters one explicit continuation boundary and authoring
  admission without reserving the ordinary turn.
- That admission guards a distinct Move author over one fresh exact continuation candidate, then
  composes and explicitly starts one fresh generic request on its own scene-owned task.
- Only that exact completed generic result is observed, bound to the continuation candidate, and
  acknowledged; only its task is reset while the exact result and candidate tokens remain retained.
- Those tokens compose one typed Move request over the current recovered placement/evidence
  authority and explicitly start it on a separate fresh scene-owned task.

Null, rebuilt, foreign-actor, wrong-kind, replaced-authority, stale, occupied, and repeated paths
remain inert. The two continuation models and all six prior generic/typed models are each called
once, the ordinary turn stays empty, and ordinary startup remains inert. No typed-result
observation, typed-task reset, applied-result binding, recovery, terminal route, turn completion,
ordinary-turn reservation, automatic cadence, provider, credential, persistence, UI/input,
navigation, perception, dialogue, timer, or new Domain behavior was added.

## Validation and review

- Each new surface was introduced test-first with a focused game build failing for the deliberately
  absent coordinator before implementation.
- The focused switch-gated Move proof passes host-local with Godot 4.6.3 Mono and .NET 8 through
  exact typed continuation start while all authoritative snapshots and earlier model counts remain
  exact.
- Review found and fixed a missing explicit foreign-actor rejection in the continuation generic
  result-observation proof; the reviewed focused build and smoke pass afterward.
- Sandboxed VSTest could not bind its loopback socket; the identical full command passed host-local.
- Final host-local `./scripts/quality.sh` passes a zero-warning build, 2,731 Domain, 586 Application,
  1,220 Infrastructure, and 8 architecture tests, scenario validation, every credential-free Godot
  smoke, and the Compatibility launch. Formatting and whitespace checks also pass.

## Exact next bounded slice

Take **Phase 7sd: observe the exact applied typed continuation Move result and task-installed
authority without acknowledging it**.

Acceptance boundary:

1. Test-first add one observation coordinator over the exact Phase 7sc started request, its typed
   task result, and that task's exact fresh resolved placement/evidence snapshots.
2. Require the retained exact generic acknowledgement/candidate provenance after application; do
   not require the typed request's pre-application placement/evidence snapshots to remain current.
3. Accept only the exact selected continuation candidate and exact authoritative snapshots installed
   by that task once. Reject null, running, rebuilt-result/request/candidate, foreign actor,
   wrong candidate, stale or replaced snapshots, and repeated inputs without changing any owner or
   model call count.
4. Extend only the focused switch-gated Move proof. Do not acknowledge or reset the typed task,
   bind the applied result, author or publish recovery, route a terminal result, reserve the ordinary
   turn, invoke another author/model, or add cadence.
5. Keep ordinary startup inert and preserve all existing exclusions: no timers, provider transport,
   credentials, persistence, UI/input, navigation, perception, dialogue delivery, or new Domain
   behavior.
6. Update README/ROADMAP, run `./scripts/quality.sh`, review the complete diff, and leave the next
   slice to bind and acknowledge only that exact observed applied result.
