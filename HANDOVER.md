# Allrpcs Handover

## Current state

`main` contains merged PR #172 at `57840a3`. The current branch is
`codex/phase-4-provider-settings-storage` and advances Phase 4 through Phase 4p1.

- Phase 4o1 adds the provider-settings-specific `ICharacterModelSettingsDocumentStore` Application
  port. Its bounded results distinguish saved, loaded, missing, and failed outcomes without exposing
  paths, JSON, credentials, provider details, exceptions, or the gameplay-save contract.
- Phase 4o2 adds `FileCharacterModelSettingsDocumentStore`: strict UTF-8 without a BOM, one trusted
  absolute path, unique same-directory temporary writes, flushed atomic replacement, explicit
  missing/failure outcomes, and best-effort temporary cleanup.
- Phase 4o3 adds `CharacterModelSettingsDocumentController`. Canonical secret-free serialization and
  strict materialization are composed with the store while missing, storage-failed, and invalid
  documents remain distinct and content-free.
- Phase 4o4 adds `CharacterModelSettingsStorageController` at the fixed Godot
  `user://character-model-settings-v1.json` path. It exposes explicit save/load calls but does not
  react to startup or input itself; its smoke uses only a process-specific temporary file.
- Phase 4p1 adds `TemporaryCharacterModelCompositionController`. At startup it loads a valid persisted
  selection and composes only the exact selected Ollama or OS-supported OpenAI binding into the
  ordinary healer source. Composition performs no credential read or HTTP request; missing, invalid,
  failed, unsupported-platform, and Anthropic selections preserve stable offline behavior. The smoke
  harness disables automatic persisted composition and uses an isolated document for its explicit
  composition check, so validation cannot inherit ordinary user settings or credentials.

No settings or credential UI, default settings, automatic server discovery, model installation or
process ownership, Anthropic credential store, provider metadata read, diagnostic export, generated
non-wait intention execution, or gameplay persistence/mutation was added. Native Linux/Windows
credential behavior remains unproved. A stored selection is therefore currently supplied outside
ordinary gameplay, and full Phase 4 acceptance is still incomplete.

## Validation

The complete host-local `./scripts/quality.sh` passes:

- workflow and formatting validation, restore, and a zero-warning build;
- 1,388 Domain tests, 357 Application tests, 378 Infrastructure tests, and eight architecture tests;
- the five-evidence representative scenario validator;
- all 70 opt-in Godot smoke invocations, including isolated settings storage and persisted Ollama
  composition without credential or network input/output; and
- the bounded graphical Compatibility-renderer launch.

Focused red/green suites cover the storage-independent public API, exact document and selection
retention, null/empty rejection, closed outcomes, atomic strict-UTF-8 file replacement, canonical
serialization, fail-closed materialization, and startup composition. The restricted sandbox
reproduced the known VSTest local-IPC denial; identical host-local commands passed. GitHub Actions
budget is exhausted for the month: do not dispatch hosted workflows, and treat zero-step hosted
failures as account infrastructure rather than test evidence.

## Exact next bounded slice

Take **Phase 4p2: immutable provider-settings update transition**.

Acceptance boundary:

1. Add one pure Application transition that replaces the setting for an exact canonical provider at
   its existing stable position, or appends one previously unconfigured canonical provider within the
   three-provider bound.
2. Retain every unrelated exact settings instance, stable order, and the exact selected provider. A
   replacement or append must retain the exact supplied setting; supplying the already-retained exact
   setting should return the original snapshot.
3. Reject null inputs, non-canonical providers, and appending beyond the bound without changing the
   input snapshot. Continue to rely on `CharacterModelSettings` for model-name validation.
4. Prove the complete public surface and transition behavior through focused Application tests.
5. Do not persist, compose, select, test, or call a provider; do not add UI, defaults, credentials,
   automatic discovery, process ownership, Anthropic credential storage, or generated intention
   execution.
