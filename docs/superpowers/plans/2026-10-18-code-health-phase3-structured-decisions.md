# Phase 3 structured-decision consolidation

Owner authority: [18 October decision](../evidence/2026-10-18-phase3-authority.json).
The owner explicitly deferred live #417 capture and the Keychain check to begin Phase 3.
Both remain outstanding; Phase 2 is deferred rather than accepted as complete.

## Scope and completion

Implement program Phase 3 item 1: a shared Application state machine and generic provider
clients for the ten existing structured-decision kinds, with provider-neutral bindings.
Preserve prompts, schemas, exact request/decision identities, deserialization policies,
transport deadlines, error redaction, bounded incremental reads and caller-owned clients.
No new sabotage type, gameplay rule or physical/provider qualification is authorized.
Main extraction and SmokeCheckHost are later Phase 3 items, outside this slice.

## Tasks

- [x] Record exact owner decision, defer Phase 2 without closing its roadmap item, select
  this named slice, sync/check/self-test the ledger before changing product code.
- [x] Add a provider-neutral `StructuredDecisionPipeline<TRequest,TDecision>` state machine
  with pinning tests for ordered completion, terminal fencing, request identity, text bounds,
  malformed/duplicate events and the existing legacy lifecycle policy.
- [x] Add generic OpenAI event accumulation/completion and one Responses client using the
  existing deadline transport. Pin response disposal, caller cancellation and sanitized errors.
- [x] Migrate each existing type behind its existing public contracts, running its existing
  model, SSE, accumulator and binding tests before moving to the next type:
  sabotage-action; target-delay; target-delay-recovery; target-influence;
  target-influence-recovery; object-move; object-concealment; object-substitution;
  object-contamination; dialogue-misquotation-evidence.
- [ ] Generalize structured request descriptors and provider-neutral bindings; add Anthropic
  and Ollama clients using the shared pipeline, and migrate production composition.
- [ ] Remove per-decision OpenAI compatibility wrappers after their consumers/tests migrate.
- [ ] Review final changes, run zero-warning build, format verification, solution tests,
  affected native checks and continuation sync/check/self-test. Publish a review PR.

## Implementation decisions

The first extraction retains typed public facades so existing tests pin their behavior during
migration. Three older object accumulators (move, concealment, substitution) accept output without
created/in-progress events; preserve that explicit policy rather than silently tightening it.
All other decision accumulators require the ordered lifecycle. Delay-recovery additionally rejects
metadata after final text; retain that separate policy. Decision-specific deserializers
and request schema/content remain separate domain-policy boundaries.

Full Phase 3 exit still requires the program's Main/SmokeCheckHost work and one complete
`./quality.sh`. Under approved D2, use scoped checks between phase exits.

## Evidence

Baseline: merged PR #440 at `3c4d6c63`, identical tree to reviewed `b71d057f`.
Merge CI [39077177772](https://github.com/hherb/allrpcs/actions/runs/39077177772) passed all four
jobs, with 5,924 solution tests and native 363 passed / zero failed / two existing quarantines.
The complete 218-stage root pass remains applicable to that baseline only.

## First implementation checkpoint

All ten existing models, SSE completion facades and stream accumulators now delegate to one
`StructuredDecisionPipeline`, `OpenAiStructuredDecisionAccumulator` and
`OpenAiResponsesStructuredClient`. The extracted transport/SSE/accumulator code falls from 3,790
to 1,270 lines, including shared code and all compatibility facades. Request serializers,
schemas, deserializers and domain policies are unchanged. Provider-neutral bindings and removal
of the public compatibility facades remain outstanding; this is not full Phase 3 completion.

Validation on the first checkpoint:

- New pipeline tests were first observed failing because the new API was absent; all 17 now pass.
- Each decision kind's existing suite passed before migration continued. Delay-recovery's existing
  metadata-after-text regression first failed and pinned its distinct preserved policy.
- Five new shared-client cases prove early terminal return before an unread tail, response-body
  disposal, caller-client reuse, redacted transport/semantic errors and rejected-response disposal.
- Existing deadline/cancellation coverage passed for all ten migrated consumers (the focused
  shared-client plus decision-deadline run passed 53 cases).
- `dotnet build Allrpcs.sln --no-restore`: zero warnings/errors.
- `dotnet format Allrpcs.sln --no-restore --verify-no-changes`: passed.
- `dotnet test Allrpcs.sln --no-build`: 5,946 passed (37 Architecture, 1,482 Application,
  2,877 Domain, 1,550 Infrastructure).
- Sixteen affected native launches through `run_check`: five standalone composition scenes,
  ordinary composition, and ten sabotage/misquotation consideration/presentation checks passed.
  The first standalone composition launch reproduced its outdated private-field cast before
  updating the six current native fixtures to inspect the shared transport. Original exact-client,
  exact-settings, exact-credential, inert-construction and replacement assertions remain.
- Manual extraction review confirmed all ten type/serializer/deserializer pairings, both lifecycle
  policies, the delay-recovery metadata policy, original bounds and unchanged decision policies.

Logs are local at `/private/tmp/allrpcs-phase3-{build,format,tests,native}.log`; initial native RED
is `/private/tmp/allrpcs-phase3-composition-red.log`. Per-type migration logs are retained beside
these files. Existing native ObjectDB exit warnings remain; no leak qualification is claimed.
No full root gate was repeated: approved D2 runs it at complete Phase 3 exit.

Next implementation boundary: generalize request/schema descriptors and provider-neutral bindings,
add Anthropic/Ollama structured clients using the shared Application policy, migrate production
composition and then remove the typed OpenAI facades. Keep Phase 2 live checks deferred.
