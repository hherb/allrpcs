# Allrpcs Handover

## Current state

`main` is verified at merged PR #254 (`a91e254`). Branch
`codex/phase-7-substitution-task-flow` advances the exact Phase 7 substitution-decision chain
through these bounded slices:

- a closed scene task owns one exact decision through idle, running, completed, cancelled, and
  failed states, resolves a successful non-stale result on the main thread, and resets only
  explicitly from terminal state;
- a temporary model source consumes one exact caller-supplied model once or returns one stable
  fail-closed offline model;
- the main scene composes the authored substitution candidate and exact current substitution and
  evidence snapshots into one request only on explicit start, retaining overlap and leaving
  selection, abstention, failure, and stale-result behavior atomic;
- a strict JSON decoder accepts only one exact selection or abstention for its current request,
  canonicalizes value-equal identifier text to the request's exact instances, and maps malformed,
  unsupported, or oversized input to one bounded error; and
- a pure OpenAI Responses serializer emits one stateless streaming request with a closed strict
  schema and bounded trusted character, target, replacement, and eligibility context.

Implementation commit `66c485e` is published in draft PR #255. Hosted run `34922959779` failed
before executing any steps in all five credential-free jobs (`steps: []`), so it is Actions/account
infrastructure state rather than product-test evidence and was not retried.

The slice does not add SSE handling, HTTP transport, credentials, provider adapters or bindings,
automatic invocation, task acknowledgement, model-selected presentation, persistence, timing,
scheduling, events, time, transcripts, navigation, input, or UI. Authoritative world objects and
all supplied request, candidate, substitution, evidence, and source snapshots remain unchanged.
The unrelated untracked `scripts/code_statistics.py` remains untouched and must stay unstaged.

## Validation and review

- Tests began with the expected missing task surface, model source, strict decoder, and OpenAI
  serializer compile failures.
- Focused host-local checks pass the exact substitution task smoke, one-shot/offline model-source
  smoke, main-scene consideration smoke, 22 decoder tests, 6 serializer tests, and a zero-warning
  game build. Sandboxed Godot hit the known `user://logs` crash and sandboxed VSTest hit the known
  IPC `SocketException (13)`; identical host-local commands passed without product changes.
- Host-local `./scripts/quality.sh` passes workflow validation, restore, formatting, a zero-warning
  build, 2,455 Domain tests, 521 Application tests, 710 Infrastructure tests, 8 architecture tests,
  scenario validation, every credential-free headless Godot smoke, and the final Compatibility
  renderer launch. No live-conformance switch was enabled and no provider was invoked.
- Hosted run `34922959779` contains no executed job steps and therefore adds no product-validation
  evidence; the host-local quality result above remains authoritative for this draft head.
- Every new authored C# file is below 500 lines with no lines longer than 120 characters.
  `git diff --check` passes. Slice-by-slice review found no unresolved findings.

## Exact next bounded slice

Take **Phase 7gx: complete one strict bounded OpenAI substitution-decision SSE stream**.

Acceptance boundary:

1. Add one request-bound accumulator that accepts only the supported OpenAI Responses semantic
   lifecycle, counts bounded output deltas, validates the complete output-text document through the
   existing strict substitution decoder, and exposes the exact accepted decision only after one
   successful `response.completed` event.
2. Add one asynchronous completion seam over the existing bounded SSE reader; reject empty,
   malformed, incomplete, refused, failed, unframed, duplicate-terminal, and post-completion input
   without exposing provider text in bounded validation errors.
3. Preserve caller cancellation and transport I/O failure identity, leave the supplied stream open,
   and retain no provider response text or identifiers after exact decision materialization.
4. Do not add HTTP transport, a provider adapter or binding, credentials, scene composition,
   automatic invocation, acknowledgement, presentation, persistence, input, UI, timing,
   scheduling, events, time, transcripts, or navigation behavior.
