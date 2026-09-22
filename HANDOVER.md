# Allrpcs Handover

## Current state

`main` includes merged PR #314 at `42f581f`. Branch
`codex/phase-7-sabotage-action-decision` advances Phase 7 through eight bounded slices:

- Phase 7oc strictly decodes one bounded closed sabotage-action decision JSON document, rebinds
  value-matching actor and selected-kind text to exact request instances, and delegates final
  acceptance to the existing Domain policy.
- Phase 7od accumulates one ordered bounded OpenAI Responses semantic stream and completes framed
  SSE only after an exact accepted selection or abstention.
- Phase 7oe serializes one stateless streaming structured-output request with bounded trusted actor
  and caller-filtered action-kind context plus a closed exact schema.
- Phase 7of retains one exact OpenAI setting and caller-composed model without invoking it.
- Phase 7og executes one request through caller-owned OpenAI credential, HTTP, settings, and endpoint
  dependencies, and creates its exact binding without construction-time input/output.
- Phase 7oh supplies one configured, one-shot, or stable fail-closed offline model through
  exact-instance initialized/replaced/cleared bindings.
- Phase 7oi owns one isolated decision task with overlap rejection, caller cancellation, explicit
  terminal reset, and main-thread-only accepted-result installation.
- Phase 7oj explicitly composes one already-authored request with that isolated source and task,
  preserving a queued one-shot model across rejected overlap and remaining inert at startup.

The chain only selects one caller-prefiltered sabotage kind or abstains. It does not infer
action-specific eligibility, choose a target or candidate, dispatch or apply an action, add these
nodes to `Main.tscn`, schedule work autonomously, author evidence, persist state, or add input/UI,
timing, navigation, movement, perception, dialogue, or other world behavior. The unrelated
untracked `scripts/code_statistics.py` remains untouched and must stay unstaged.

## Validation and review

- Test-first focused runs failed only on each absent decoder, stream/SSE, serializer, binding,
  provider model/factory, model source, task, and flow before the smallest implementation was added.
- Focused host-local runs pass 22 decoder tests, 24 stream/SSE tests, 5 request-serialization tests,
  3 binding tests, 9 provider model/factory tests, zero-warning game builds, and all three new
  credential-free Godot smokes.
- Final host-local `./scripts/quality.sh` passes workflow validation, restore, formatting, a
  zero-warning build, 2,728 Domain tests, 586 Application tests, 1,220 Infrastructure tests, all 8
  architecture tests, scenario validation, every credential-free Godot smoke, and the Compatibility
  renderer launch.
- Session review covers exact request-instance rebinding, closed JSON/schema surfaces, ordered and
  bounded SSE, trusted-context minimization, credential-before-send behavior, caller cancellation,
  bounded redacted failures, exact dependency retention, binding CAS, one-shot precedence and
  overlap preservation, stable offline failure, main-thread task installation, inert startup,
  documentation accuracy, file/line limits, whitespace, and scoped staged paths. One ambiguous
  layer-local documentation exclusion was clarified; no unresolved finding remains.

## Exact next bounded slice

Take **Phase 7ok: compose one sabotage-action model binding from explicit game settings**.

Acceptance boundary:

1. Test-first add one isolated composition seam over the exact existing
   `TemporaryCharacterSabotageActionModelSource`.
2. Accept exact already-authored OpenAI settings plus caller-owned HTTP and credential dependencies;
   create bindings only through `OpenAiCharacterSabotageActionModelBindingFactory` and install,
   replace, or clear only this composition's exact binding through the source's existing operations.
3. Prove configured, replacement, clear, invalid-input, and stable offline-fallback behavior in one
   isolated credential-free smoke, including zero credential loads, HTTP sends, or model calls.
4. Update README/ROADMAP, run `./scripts/quality.sh`, and review the complete diff.
5. Do not add `Main.tscn` wiring, global model-composition-controller integration, automatic
   scheduling, request creation, target/candidate dispatch, action application, evidence authorship,
   persistence, input/UI, timing, navigation, movement, perception, dialogue, or other world
   behavior.
