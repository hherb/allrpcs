# Allrpcs Handover

## Current state

`main` contains merged PR #174 at `202f542`. The current branch is
`codex/phase-4-provider-settings-save` and advances Phase 4 through Phase 4p8.

- Phase 4p6 adds explicit exact-current persistence to `CharacterModelSettingsController`.
  Uninitialized and storage-failed saves return only `Failed`, preserve scene state, and never leak
  document or storage details. No settings transition saves automatically.
- Phase 4p7 adds atomic apply: an exact canonical setting is updated and selected before the scene
  snapshot changes, so rejected input cannot retain a partial transition.
- Phase 4p8 adds the temporary secret-free settings panel. It begins without a default provider,
  supports bounded model apply and configured unselected-provider removal in memory, saves only on
  the separate button, and tells the player that activation requires restart.

No credential input, credential operation, automatic save, live recomposition, default settings,
automatic server discovery, model installation or process ownership, Anthropic credential store,
provider metadata read, diagnostic export, generated non-wait intention execution, or gameplay
persistence/mutation was added. Composition still performs no credential read or HTTP request;
missing, invalid, failed, unsupported-platform, and Anthropic selections preserve stable offline
behavior. Native Linux/Windows credential behavior remains unproved, and full Phase 4 acceptance is
incomplete.

## Validation

The complete host-local `./scripts/quality.sh` passes:

- workflow and formatting validation, restore, and a zero-warning build;
- 1,388 Domain tests, 365 Application tests, 378 Infrastructure tests, and eight architecture tests;
- the five-evidence representative scenario validator;
- all 72 opt-in Godot smoke invocations, including isolated settings apply/removal/save and exact
  shared persisted Ollama composition; and
- the bounded graphical Compatibility-renderer launch.

Focused red/green game builds and Godot smokes cover uninitialized and storage-failed saves, exact
state preservation, atomic apply, absence of automatic persistence, canonical panel choices,
invalid-input rejection, explicit removal/save, and restart-required presentation. The restricted
sandbox reproduced the known Godot user-log crash; the identical host-local smoke passed. GitHub
Actions budget is exhausted for the month: do not dispatch hosted workflows, and treat zero-step
hosted failures as account infrastructure rather than test evidence.

## Exact next bounded slice

Take **Phase 4q1: scene-owned OpenAI credential operations**.

Acceptance boundary:

1. Add one scene-owned controller that composes the existing OS-specific
   `IOpenAiCredentialStore` on macOS, Linux, or Windows and exposes only explicit asynchronous save
   and delete operations with the existing redacted bounded statuses.
2. Construction and `_Ready` must not read, save, delete, test, or retain a credential. Save may
   receive one bounded key transiently but must not expose it through properties, logs, scene state,
   environment variables, command arguments, settings JSON, diagnostics, or model context.
3. Add a deterministic injected test seam and opt-in smoke proving exact-store delegation,
   cancellation, saved/deleted/missing/failed outcomes, and no startup operation without touching a
   real OS credential facility.
4. Do not add credential UI yet, load a credential for presentation, call a provider, recompose the
   live model, add Anthropic credential storage, change native store contracts, or claim native
   Linux/Windows proof.
