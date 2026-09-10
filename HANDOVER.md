# Allrpcs Handover

## Current state

`main` contains merged PR #175 at `fe68e46`. The current branch is
`codex/phase-4-openai-credential-controls` and advances Phase 4 through Phase 4q3.

- Phase 4q1 adds `OpenAiCredentialController`. It composes the current OS store without startup I/O
  and exposes only explicit bounded save/delete operations. It never loads or retains a credential.
- Phase 4q2 adds masked OpenAI key controls. Submitted text is cleared immediately, never read back,
  and only fixed redacted outcomes reach the scene; exit cancels an active operation.
- Phase 4q3 adds explicit connection testing for the exact current selected supported binding. It
  presents only connected/busy/failed, generates no content, and changes no authoritative state.

No automatic credential read/write/delete/test, live recomposition, default settings, automatic
server discovery, model installation or process ownership, Anthropic credential store, provider
metadata read, diagnostic export, generated non-wait intention execution, or gameplay
persistence/mutation was added. Native Linux/Windows credential behavior remains unproved, and full
Phase 4 acceptance is incomplete.

## Validation

The complete host-local `./scripts/quality.sh` passes:

- workflow and formatting validation, restore, and a zero-warning build;
- 1,388 Domain tests, 365 Application tests, 378 Infrastructure tests, and eight architecture tests;
- the five-evidence representative scenario validator;
- all 75 opt-in Godot smoke invocations, including isolated credential operations, masked controls,
  and exact selected-setting connection testing; and
- the bounded graphical Compatibility-renderer launch.

Focused red/green game builds and Godot smokes cover zero-I/O startup, exact injected-store and
binding delegation, bounded validation, immediate masked-input clearing, no readback, redacted
saved/deleted/missing/failed/connected presentation, cancellation, and absence of provider access,
generation, or scene mutation. GitHub Actions budget is exhausted for the month: do not dispatch
hosted workflows, and treat zero-step hosted failures as account infrastructure rather than test
evidence.

## Exact next bounded slice

Take **Phase 4q4: explicit live binding replacement**.

Acceptance boundary:

1. Add an atomic replacement operation to `TemporaryCharacterModelSource` that accepts one exact
   settings selection and matching non-empty binding snapshot, validates the selected exact binding
   before changing either retained value, and affects only future `TakeModel` calls.
2. Add one explicit composition-controller operation that constructs the exact current selected
   supported binding and delegates replacement without credential read, HTTP, generation, or save.
   Missing, Anthropic, unsupported-platform, and mismatched bindings must preserve the current model.
3. Prove exact new-model use on the next request, stable subsequent reuse, one-shot supplied-model
   precedence, rejected-state identity, and no effect on an already-taken model through deterministic
   tests and an opt-in smoke.
4. Do not activate automatically, add the UI activation control yet, interrupt an active turn, call
   a provider, add fallback/substitution, store Anthropic credentials, or claim native Linux/Windows
   proof.
