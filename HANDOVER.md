# Allrpcs Handover

## Current state

`main` includes merged PR #307 at `efa465f`. Branch
`codex/phase-7-delay-recovery-transport` continues target-delay recovery through two bounded
Infrastructure slices:

- Phase 7nh strictly decodes one complete closed JSON selection-or-abstention document within the
  existing 4,096-code-unit transport boundary. Value-equal actor text and an in-range candidate
  index rebind to the request's exact instances before the unchanged Domain policy rejects foreign,
  stale, wrong-owner, or otherwise ineligible decisions through one content-free error.
- Phase 7ni writes one request-bound strict selection schema and one stateless streaming OpenAI
  Responses request. It includes only the selected model, disabled storage, bounded output budget,
  provider-independent instructions, and trusted bounded actor, current world-event, event-log, and
  recovery-candidate context.

Implementation commit `db0cb3c` is published in draft PR #308 targeting `main`.

Nothing accumulates provider events, parses SSE framing, sends HTTP, accesses credentials, invokes a
model, composes a live binding, applies recovery automatically, owns authoritative recovery state,
schedules work automatically, authors evidence, persists state, or adds input/UI, navigation,
movement, perception, dialogue, or other world behavior. The unrelated untracked
`scripts/code_statistics.py` remains untouched and must stay unstaged.

## Validation and review

- Both slices began with a focused missing-contract compile failure. Focused final checks pass all
  26 strict-deserializer cases and all 8 schema/request-serializer cases.
- The sandbox reproduced the known .NET local-IPC restriction; identical host-local focused commands
  passed without product changes.
- Host-local `./scripts/quality.sh` passes workflow validation, restore, formatting, a zero-warning
  build, 2,720 Domain tests, 578 Application tests, 1,125 Infrastructure tests, all 8 architecture
  tests, scenario validation, every credential-free Godot smoke, and the final Compatibility launch.
- Draft PR #308's first hosted run failed all five jobs before any step executed (`steps: []`); that
  is Actions/account infrastructure state and provides no hosted product-test or build evidence.
- Session review covers exact actor/candidate rebinding, full current-state policy delegation,
  immutable request state, malformed/duplicate/unknown/over-depth/oversized input, content-free
  failures, request-bound abstention and candidate enums, trusted-context scope, credential absence,
  closed public surfaces, documentation, file and line limits, formatting, whitespace, and scoped
  paths. No unresolved finding remains.

## Exact next bounded slice

Take **Phase 7nj: complete one bounded target-delay recovery semantic-event and framed SSE
lifecycle**.

Acceptance boundary:

1. Test-first add one exact-request stream accumulator. Accept only the ordered
   `response.created` -> `response.in_progress` -> zero or more supported metadata/text-delta events
   -> one valid `response.output_text.done` -> `response.completed` lifecycle. Return an exact
   accepted decision only at successful completion.
2. Bound each semantic event to the existing 64,000-code-unit event limit and accumulated response
   text to the decoder's 4,096-code-unit document limit. Fail closed and terminally on malformed,
   duplicate, out-of-order, unknown, incomplete, refused, failed, excessive, or policy-invalid
   content without exposing provider details.
3. Test-first compose the accumulator with `OpenAiServerSentEventReader` so one bounded UTF-8 SSE
   stream returns only that exact completed decision. Reject empty, malformed, unterminated,
   post-completion, or cancelled streams; leave the exact request and every nested instance
   unchanged.
4. Do not yet add HTTP, credentials, an OpenAI decision model, composition, main-scene ownership,
   automatic application, evidence, persistence, or input/UI behavior. Update README/ROADMAP, run
   `./scripts/quality.sh`, and review the complete diff.
