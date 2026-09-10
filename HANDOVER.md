# Allrpcs Handover

## Current state

`main` contains merged PR #170 at `526d048`. The current branch is
`codex/phase-4-openai-composition` and advances Phase 4 through Phase 4m3.

- Phase 4m1 adds `OpenAiCharacterModelBindingFactory`. Its plain and diagnostics-enabled overloads
  create one exact OpenAI binding from caller-supplied settings, `HttpClient`, and credential source.
  The model and connection tester share those exact dependencies, and the model retains the exact
  settings plus optional caller-owned `TimeProvider` and redacted diagnostic sink.
- Phase 4m2 adds the equivalent `AnthropicCharacterModelBindingFactory` over the existing Anthropic
  model, tester, and read-only credential source.
- Phase 4m3 adds `OllamaCharacterModelBindingFactory`. Its model and tester share the exact
  caller-owned `HttpClient` and validated loopback `OllamaEndpoint`; its model retains the exact
  settings and optional diagnostics dependencies.

All three factories reject null and provider-mismatched inputs before constructing a binding.
Construction performs no credential load, HTTP request, provider discovery, or scene mutation.
The scene still has no production provider composition, settings UI/persistence, automatic local
server discovery, model process ownership, Anthropic credential store, diagnostic export,
generated non-wait intention execution, or gameplay persistence/mutation. The stable offline model
remains the ordinary default, native Linux/Windows credential behavior remains unproved, and full
Phase 4 acceptance remains incomplete.

## Validation

The complete host-local `./scripts/quality.sh` passes:

- workflow and formatting validation, restore, and a zero-warning build;
- 1,388 Domain tests, 336 Application tests, 347 Infrastructure tests, and eight architecture tests;
- the five-evidence representative scenario validator;
- all 68 opt-in Godot smoke invocations; and
- the bounded graphical Compatibility-renderer launch.

Focused red/green evidence covered four construction tests per provider: exact binding and dependency
retention, optional diagnostics retention, rejection, closed public factory surface, and zero-I/O
construction. The restricted sandbox reproduced the known MSBuild IPC denial; identical host-local
commands passed. GitHub Actions budget is exhausted for the month: do not dispatch hosted workflows,
and treat zero-step hosted failures as account infrastructure rather than test evidence.

## Exact next bounded slice

Take **Phase 4n1: immutable provider-settings selection snapshot**.

Acceptance boundary:

1. Add one provider-neutral Application snapshot that copies one to three exact existing
   `CharacterModelSettings` instances in stable caller order and retains one exact canonical selected
   provider. Require exactly one setting per provider and require the selected provider to be present.
2. Reject null, empty, excessive, duplicate, non-canonical, and selected-provider-absent input before
   retaining any caller-owned mutable collection. Do not rewrite model names, select a default, or
   substitute another provider.
3. Prove immutability, exact-instance retention, ordering, complete public API shape, and fail-closed
   rejection through focused Application tests.
4. Do not add a persistence port or format, filesystem/registry/preferences access, credentials,
   Infrastructure adapter construction, HTTP, connection testing, Godot scene wiring, UI, automatic
   local-server discovery, model process ownership, or generated intention execution.
5. Keep the existing `CharacterModelBindingSnapshot` and stable offline scene default unchanged; this
   slice records trusted configuration intent only and does not claim a provider is available.
