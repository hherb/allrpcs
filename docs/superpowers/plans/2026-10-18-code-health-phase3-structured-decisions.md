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
- [x] Generalize structured request descriptors and provider-neutral bindings; add Anthropic
  and Ollama clients using the shared pipeline, and migrate production composition.
- [x] Remove per-decision OpenAI compatibility wrappers after their consumers/tests migrate.
- [x] Review final changes, run zero-warning build, format verification, solution tests,
  affected native checks and continuation sync/check/self-test.
- [x] Commit/push the reviewed branch and publish draft [PR #443](https://github.com/hherb/allrpcs/pull/443).
  The owner explicitly approved commit/push and the draft PR on 18 October. Product commit: `19745021`.

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

The provider-neutral continuation below supersedes this historical next boundary. Phase 2 live checks
remain deferred.


## Provider-neutral continuation (18 October)

PR [#442](https://github.com/hherb/allrpcs/pull/442) merged at `a33cdc12`, with the same tree as
`53e44e76`. Merge [CI 39086701001](https://github.com/hherb/allrpcs/actions/runs/39086701001) passed.
This continuation completes the structured-decision item, rather than all of Phase 3.

`CharacterStructuredDecisions` names the ten unchanged prompt/context, schema, deserializer,
128-token bound and document/lifecycle policies. One serializer maps those contracts to OpenAI
Responses, Anthropic Messages and Ollama chat. The typed models depend on the Application
`IStructuredDecisionClient` contract. Three generic provider clients share response ownership,
deadlines and redacted HTTP failures; each provider has one bounded stream adapter feeding the
same Application pipeline. All fifty per-kind OpenAI model, binding, request, SSE and accumulator
files have been removed; the twenty typed model/binding adapters are provider-neutral.

The native provider composition installs all ten decision kinds, including the previously unwired
misquotation-evidence source, and preserves exact binding replacement and one-shot supply.
Ollama uses the exact caller-selected validated loopback endpoint and needs no credential.
Anthropic supports explicit caller-owned credential injection before composition; no Anthropic
credential settings UI or native credential store is implemented here. Without that injected
source, Anthropic remains unavailable. No live hosted request, model qualification or credential
store success is inferred from controlled tests.

Wire contracts were checked against the providers' official documentation:
[Anthropic structured outputs](https://platform.claude.com/docs/en/build-with-claude/structured-outputs),
[Anthropic stream lifecycle](https://platform.claude.com/docs/en/build-with-claude/streaming), and
[Ollama chat](https://docs.ollama.com/api/chat). Anthropic publishes only after a complete text block,
`end_turn` and `message_stop`; Ollama requires assistant content plus `done: true` and `done_reason:
stop`. Truncation, refusal, malformed/out-of-order data and incomplete streams cannot publish a decision.

Validation:

- New generic provider tests were first observed RED with the missing descriptor API; the initial
  nine protocol tests then passed. The completed suite adds 30 exact-selection/payload cases
  (ten kinds × three providers), eight header/body deadline/cancellation cases, early completion
  and response ownership, malformed ordering/bounds, terminal fencing and sanitized failures.
- `dotnet build Allrpcs.sln --no-restore`: zero warnings/errors.
- `dotnet test Allrpcs.sln --no-build`: 5,994 passed (37 Architecture, 1,484 Application,
  2,877 Domain, 1,596 Infrastructure). Removal of reflection-only tests for deleted wrapper APIs
  explains the difference from the earlier checkpoint; behavioral cases were retained.
- Seventeen affected native launches passed: five standalone compositions, the persisted
  composition, ordinary provider switching, and ten sabotage/misquotation consideration or
  presentation checks. Sixteen passed initially; the composition fixture's obsolete claim that
  injected Anthropic replacement must fail was updated, and that one fixture passed on retry.
  It now checks all ten selected Ollama/Anthropic decision bindings and no credential I/O.
- Manual review confirmed unchanged request-content, schema-writer and deserializer source;
  exact native binding/one-shot ownership; three legacy prelude-optional OpenAI policies and
  delay-recovery's metadata-after-text rejection. No per-decision OpenAI source files remain.
- Existing native ObjectDB exit warnings remain; no leak qualification is claimed.

Local logs: `/private/tmp/allrpcs-phase3-host-{build,tests}.log`,
`/private/tmp/allrpcs-phase3-format-verify.log`, and the sixteen passing launches plus obsolete
fixture failure at `/private/tmp/allrpcs-phase3-provider-native-initial.log`. The final corrected
composition pass is recorded in the closure receipt. Sandbox build attempts produced no complete
result; the identical authorized host build/test retry supplies the reported evidence.
The [closure receipt](../evidence/2026-10-18-phase3-structured-decisions.json) binds final sources.
Full solution format verification and continuation sync/check/self-test passed (15 regression probes
plus temporary Git history probes).
Next program item: plan and extract the conversation-turn and focused-search coordinators from
`Main.cs`, retaining behavior and explicit process order. SmokeCheckHost remains subsequent work.
One complete root gate is still required at full Phase 3 exit under approved D2.

## PR #443 review fixes (owner reopening, 18 October)

The owner [asked](../evidence/2026-10-18-pr443-review-fixes-authority.json) for every PR #443
review finding to be fixed, re-reviewed, verified and pushed, with GitHub issues for anything that
cannot be fixed. Fixes:

- **Ollama:** the trusted context is now a `user` message after the system instructions. Before,
  both were `system` messages, which some chat templates render as an empty prompt.
- **Anthropic:**
  - Streams now skip leading `thinking`/`redacted_thinking` blocks, which current Claude models
    emit by default.
  - Decisions request `max_tokens: 4096`, the same as the Anthropic dialogue request, because
    thinking counts against that budget.
- **Provider selection:** the serializer and factory now match each provider explicitly and throw
  for an unknown one.
- **Descriptor and OpenAI client:** the OpenAI client takes the descriptor, the lifecycle flags
  have no defaults, and the shared 128-token bound is one constant.
- **Media type:** an unsupported response media type keeps its own error message.
- **Logging:** the ten decision task controllers log the type of a faulted decision.
- **Tests:**
  - New tests pin the exact request fields and roles for every kind and provider, real Anthropic
    stream framing, text split across events and the bound on the joined text, the index of each
    malformed event, missing stop reasons, and credential rejection.
  - Fifty per-kind test files no longer carry the names of deleted `OpenAiCharacter*` types.

**Ollama budget measurement:** four local models ran all ten kinds (40 runs). The most any run
used was 29 output tokens, every run ended with `done_reason: stop`, and every run returned valid
JSON. The Ollama bound therefore stays at 128.

**OpenAI:** request bytes and stream policies are unchanged.

**Results:** zero-warning build; format clean; 6,007 solution tests passing; full smoke suite at
363 passed, 0 failed (the two existing quarantines also passed). An independent re-review found no
blocking issues. See the [receipt](../evidence/2026-10-18-pr443-review-fixes.json).

**Not fixed here (filed as GitHub issues):**
- live Anthropic qualification;
- thinking-block handling in the closed Anthropic dialogue adapter;
- a separate failure category for deadline expiry versus other transport failures in the shared
  provider transport.
