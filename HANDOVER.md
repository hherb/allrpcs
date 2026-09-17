# Allrpcs Handover

## Current state

`main` includes merged PR #292 at `18b396a`. Branch
`codex/phase-7-influence-recovery-json` advances the influence-recovery path through two bounded
Infrastructure slices:

- Phase 7lj adds an internal closed JSON schema and public strict decoder for one exact recovery
  decision. Actor text and a nullable zero-based candidate index rebind only to exact request
  instances; final acceptance remains owned by the existing Domain policy. All malformed,
  adversarial, stale, foreign, wrong-owner, oversized, and policy-invalid input fails with one
  redacted error.
- Phase 7lk adds one stateless strict OpenAI Responses request serializer. It supplies bounded
  actor-owned character data, current directional relationship values, and exact indexed recovery
  candidate context without credential access, network traffic, model invocation, or mutation.

No slice parses semantic response events or SSE bytes, sends HTTP, loads credentials, constructs an
OpenAI recovery model, composes a configured binding, wires `Main.tscn`, applies or acknowledges
recovery in scene authority, persists relationships, authors candidates or evidence, or adds
input/UI, elapsed time, navigation, movement, perception, dialogue, or other world behavior. The
unrelated untracked `scripts/code_statistics.py` remains untouched and must stay unstaged.

## Validation and review

- Test-first focused checks failed because each new decoder/serializer contract was absent, then
  passed 29 strict JSON tests and 5 request-serializer tests.
- Final host-local `./scripts/quality.sh` passes workflow validation, restore, formatting, a
  zero-warning build, 2,655 Domain tests, 560 Application tests, 993 Infrastructure tests, 8
  architecture tests, scenario validation, every credential-free Godot smoke, and the final
  Compatibility renderer launch.
- Review covers exact actor and candidate rebinding, abstention, the 64-candidate boundary, closed
  schema output, bounded trusted context, stale and wrong-owner rejection, adversarial JSON,
  redacted failures, unchanged source graphs, public surfaces, file size, line length,
  documentation, and whitespace. The one review finding tightened redaction coverage to inspect the
  complete exception chain; no unresolved finding remains.

## Exact next bounded slice

Take **Phase 7ll: semantically accumulate one OpenAI influence-recovery response**.

Acceptance boundary:

1. Test-first add one request-bound Infrastructure accumulator for an ordered OpenAI Responses
   semantic-event lifecycle: created, in-progress, permitted metadata, bounded output-text deltas,
   one verified output-text completion, and response completion.
2. Materialize the decision only through the existing strict recovery decoder and expose it only
   after successful response completion. Bound each semantic event and the accumulated output;
   permanently terminate on malformed, unknown, duplicate, out-of-order, incomplete, refused,
   failed, oversized, stale, or policy-invalid input with one content-free error.
3. Prove exact selection and abstention, unchanged request/relationship/reason state, terminal
   success and failure behavior, redaction, and a closed public surface.
4. Update README/ROADMAP, run `./scripts/quality.sh`, and review the complete diff. Do not yet add SSE
   byte framing/completion, HTTP execution, credential access, OpenAI model construction, binding
   composition, `Main.tscn` integration, recovery application/acknowledgement, persistence, UI,
   elapsed time, navigation, movement, perception, dialogue, or other world behavior.
