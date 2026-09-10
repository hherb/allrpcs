# Allrpcs Handover

## Current state

`main` contains merged PR #173 at `b39bf95`. The current branch is
`codex/phase-4-provider-settings-updates` and advances Phase 4 through Phase 4p5.

- Phase 4p2 adds `CharacterModelSettingsUpdateTransition`: one exact canonical provider setting is
  replaced at its stable position or appended within the three-provider bound. The selected
  provider and every unrelated exact setting remain unchanged; an already-retained exact setting
  returns the original snapshot.
- Phase 4p3 adds `CharacterModelSettingsRemovalTransition`. Only a configured unselected canonical
  provider can be removed, preserving the exact selection and unrelated settings in stable order.
- Phase 4p4 adds the scene-owned `CharacterModelSettingsController`. It initializes once and
  delegates immutable update, selection, and removal while rejected operations preserve its exact
  current snapshot.
- Phase 4p5 loads one valid persisted selection into that controller at startup. The temporary
  composition controller consumes the exact same retained snapshot, eliminating duplicate
  materialization before composing supported Ollama or OS-supported OpenAI bindings.

No automatic save, settings or credential UI, defaults, automatic server discovery, model
installation or process ownership, Anthropic credential store, provider metadata read, diagnostic
export, generated non-wait intention execution, or gameplay persistence/mutation was added.
Composition still performs no credential read or HTTP request; missing, invalid, failed,
unsupported-platform, and Anthropic selections preserve stable offline behavior. Native
Linux/Windows credential behavior remains unproved, and full Phase 4 acceptance is incomplete.

## Validation

The complete host-local `./scripts/quality.sh` passes:

- workflow and formatting validation, restore, and a zero-warning build;
- 1,388 Domain tests, 365 Application tests, 378 Infrastructure tests, and eight architecture tests;
- the five-evidence representative scenario validator;
- all 71 opt-in Godot smoke invocations, including isolated immutable settings-state changes and
  exact shared persisted Ollama composition; and
- the bounded graphical Compatibility-renderer launch.

Focused red/green suites cover the closed transition surfaces, exact-instance retention, stable
replacement/append/removal order, unchanged rejected inputs, and canonical-provider enforcement.
Focused game build and Godot smokes cover one-time scene ownership, fail-closed changes, isolated
persisted loading, and shared composition without credential or network input/output. The restricted
sandbox reproduced the known VSTest local-IPC denial; the identical host-local command passed.
GitHub Actions budget is exhausted for the month: do not dispatch hosted workflows, and treat
zero-step hosted failures as account infrastructure rather than test evidence.

## Exact next bounded slice

Take **Phase 4p6: explicit current-settings save**.

Acceptance boundary:

1. Add one explicit operation on the scene-owned settings controller that saves only its exact
   current initialized snapshot through the existing settings-storage controller.
2. Return a bounded success/failure result without paths, JSON, credentials, exceptions, or provider
   details. An uninitialized controller or storage failure must report failure and preserve all
   in-memory state.
3. Prove isolated successful replacement and fail-closed uninitialized/failing behavior without
   touching ordinary user settings. Do not save automatically on startup, update, selection, or
   removal.
4. Do not compose, select, test, or call a provider as part of saving; do not add UI, defaults,
   credentials, automatic discovery, process ownership, Anthropic credential storage, diagnostic
   export, or generated intention execution.
