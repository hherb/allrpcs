# Allrpcs Handover

## Current state

`main` contains merged PR #248 at `e26aa4d`. The current branch is
`codex/phase-7-move-provider-flow` and advances four bounded Phase 7 slices:

- Phase 7ft strictly decodes one complete move selection-or-abstention JSON document against its
  exact current request and resolves accepted identifiers back to request-owned instances;
- Phase 7fu serializes only bounded trusted move context and a closed strict OpenAI Responses
  schema into one stateless streaming request;
- Phase 7fv completes one bounded strict Responses SSE lifecycle into the exact accepted move
  decision while failing closed on malformed, incomplete, refused, failed, or trailing input; and
- Phase 7fw exposes one caller-composed authenticated OpenAI move-decision adapter with redacted
  transport failures, strict response validation, and preserved caller cancellation.

No settings binding, scene composition, automatic timing, move application or recovery,
presentation, persistence, input, UI, events, time, transcripts, navigation, or actor movement was
added. Construction performs no credential load, HTTP send, or model invocation. The unrelated
untracked `scripts/code_statistics.py` remains untouched and must stay unstaged.

Implementation commit `41dc01d` is published in mergeable draft PR #249. Initial hosted run
`34880050881` queued all five credential-free jobs; its final status must be re-queried rather than
inferred from the host-local result.

## Validation and review

- Focused tests pass for the strict decoder, serializer, stream accumulator/completion, and HTTP
  adapter. The initial missing-type builds failed as expected before each implementation.
- The first sandboxed decoder test was blocked by VSTest's loopback `SocketException (13)`; the
  identical host-local command passed. Product code was not changed to work around the sandbox.
- Host-local `./scripts/quality.sh` passes workflow validation, restore, formatting, a zero-warning
  build, 2,407 Domain tests, 512 Application tests, 677 Infrastructure tests, 8 architecture tests,
  scenario validation, every credential-free headless Godot smoke, and the final Compatibility-
  renderer launch. No live-conformance switch was enabled and no provider was invoked.
- All changed authored C# files are below 500 lines with no lines longer than 120 characters.
  `git diff --cached --check` and formatting verification pass, and manual review found no
  unresolved findings.

## Exact next bounded slice

Take **Phase 7fx: bind one exact OpenAI move-decision model without construction-time input/output**.

Acceptance boundary:

1. Add one immutable Application binding that retains the exact OpenAI `CharacterModelSettings`
   and exact `ICharacterObjectSabotageMoveDecisionModel` without invoking the model.
2. Add one Infrastructure factory with default and caller-supplied validated `OpenAiEndpoint`
   overloads that composes the existing move adapter over exact caller-owned `HttpClient` and
   `IOpenAiCredentialSource` dependencies.
3. Reject null inputs and non-OpenAI settings before credential access, HTTP send, or model
   invocation; prove exact retained identities and deferred endpoint use in focused tests.
4. Keep the public surfaces to the immutable binding and two exact factory overloads, then update
   README and ROADMAP evidence.
5. Do not scene-own, replace, clear, or select a configured binding; compose settings into
   `Main.tscn`; start a decision; choose timing; apply/recover/present a move; or add persistence,
   input, UI, events, time, transcripts, navigation, or actor movement.
