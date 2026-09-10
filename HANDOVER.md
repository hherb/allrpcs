# Allrpcs Handover

## Current state

`main` contains merged PR #171 at `2caa206`. The current branch is
`codex/phase-4-provider-settings-snapshot` and advances Phase 4 through Phase 4n6.

- Phase 4n1 adds `CharacterModelSettingsSelectionSnapshot`: it copies one to three exact canonical
  provider settings in stable caller order, requires exactly one setting per provider, and retains
  one exact explicitly selected configured provider.
- Phase 4n2 adds `CharacterModelSettingsBindingSelector`: configuration resolves only to a
  caller-composed binding that retains the exact selected settings instance. Missing or
  value-equivalent-but-distinct bindings remain unavailable without substitution.
- Phase 4n3 adds configured selection to redacted connection testing. It dispatches only the exact
  authorized binding and otherwise returns provider-neutral `Unavailable` without touching another
  tester.
- Phase 4n4 adds the immutable `CharacterModelSettingsSelectionTransition` and makes the temporary
  scene source retain exact settings and binding snapshots. Provider switching preserves settings
  order and instances and still leaves the stable offline model as the unconfigured default.
- Phases 4n5 and 4n6 add stable version-1 settings JSON serialization and strict deserialization.
  The closed schema contains only canonical provider identifiers and bounded model names; malformed,
  future, duplicate, missing, unknown, excessive, non-canonical, and incomplete selections fail
  closed. Credentials have no representation.

No settings-document store, filesystem/preferences access, settings UI, production scene provider
composition, automatic local-server discovery, model process ownership, Anthropic credential store,
diagnostic export, generated non-wait intention execution, or gameplay persistence/mutation was
added. Native Linux/Windows credential behavior remains unproved, and full Phase 4 acceptance is
still incomplete.

## Validation

The complete host-local `./scripts/quality.sh` passes:

- workflow and formatting validation, restore, and a zero-warning build;
- 1,388 Domain tests, 351 Application tests, 366 Infrastructure tests, and eight architecture tests;
- the five-evidence representative scenario validator;
- all 68 opt-in Godot smoke invocations, including the exact configured provider-switch flow; and
- the bounded graphical Compatibility-renderer launch.

Focused red/green suites cover immutable copying, exact-instance retention, stable order, complete
public surfaces, selection transitions, exact binding and connection-test authorization, strict JSON
round trips, and fail-closed invalid documents. The final reviewed connection-test API rename passed
its six focused tests after the complete run. The restricted sandbox reproduced the known VSTest
loopback denial and Godot user-log failure; identical host-local commands passed. GitHub Actions
budget is exhausted for the month: do not dispatch hosted workflows, and treat zero-step hosted
failures as account infrastructure rather than test evidence.

## Exact next bounded slice

Take **Phase 4o1: provider-settings document storage port**.

Acceptance boundary:

1. Add one provider-settings-specific Application port that saves one already-serialized non-empty
   settings document and loads the current serialized document through bounded result contracts.
2. Distinguish saved/loaded, missing, and failed outcomes without paths, exceptions, provider details,
   credentials, JSON parsing, or storage implementation details crossing the port.
3. Prove the closed public API, null/empty rejection, exact loaded-document retention, and complete
   result invariants through focused Application tests.
4. Do not add a filesystem, registry, preferences, Keychain, Secret Service, or Credential Manager
   adapter; do not reuse the gameplay-save port; and do not compose or call a provider.
5. Leave JSON serialization/deserialization orchestration, Godot scene wiring, UI, default settings,
   credential changes, automatic discovery, model process ownership, and generated intention
   execution for later slices.
