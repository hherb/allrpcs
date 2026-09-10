# Allrpcs Handover

## Current state

`main` contains merged PR #168 at `26cf6c2`. The current branch is
`codex/phase-4-provider-selection` and advances Phase 4 through Phase 4k3.

- Phase 4k1 adds `CharacterModelBinding`, a copied non-empty snapshot capped at the three canonical
  providers, and exact selection. Bindings retain exact caller-composed settings, model, and
  connection-tester instances. The snapshot rejects null, empty, excessive, duplicate,
  non-canonical, and settings/provider-mismatched entries. Missing selection is `null`; there is no
  default, provider substitution, credential fallback, or model-name rewriting.
- Phase 4k2 adds redacted connection-test dispatch through the exact selected binding. A missing
  provider returns `Unavailable` without calling another tester. The existing ordinary Godot model
  source can now retain one exact binding snapshot and switch only to an available canonical
  provider. The OpenAI-to-Anthropic fake-binding smoke proves exact persistent selection across
  distinct ordinary turns and rejects unavailable Ollama selection without substitution.
- Phase 4k3 adds the caller-composed Anthropic model connection tester. It retrieves only the exact
  configured model through `GET /v1/models/{model_id}`, uses the established `x-api-key` and
  `anthropic-version: 2023-06-01` headers, reads no response body, and maps only redacted
  provider-neutral outcomes.

The new bindings do not construct Infrastructure adapters or credentials. The scene still has no
production provider composition, settings UI/persistence, automatic local-server discovery, model
process ownership, Anthropic credential store, Windows/Linux credential store, diagnostic export,
generated non-wait intention execution, or gameplay persistence/mutation. The stable offline model
remains the ordinary default when no bindings are initialized, and full Phase 4 acceptance remains
incomplete.

## Validation

The complete host-local `./scripts/quality.sh` passes:

- workflow and formatting validation, restore, and a zero-warning build;
- 1,388 Domain tests, 336 Application tests, 307 Infrastructure tests, and eight architecture
  tests;
- the five-evidence representative scenario validator;
- all 68 opt-in Godot smoke invocations, including the exact bound-provider switch; and
- the bounded graphical Compatibility-renderer launch.

Focused red/green evidence also covered nine binding/snapshot/selector tests, four exact binding
connection-dispatch tests, 11 Anthropic connection-tester tests, and a rebuilt focused Godot smoke.

The restricted sandbox reproduces the known MSBuild/Godot IPC and log-directory denials; identical
host-local commands pass. GitHub Actions budget is exhausted for the month: do not dispatch hosted
workflows, and treat zero-step hosted failures as account infrastructure rather than test evidence.

## Exact next bounded slice

Take **Phase 4l1: Linux OpenAI credential lifecycle behind a deterministic native seam**.

Acceptance boundary:

1. Add a Linux Secret Service process-runner seam and an `IOpenAiCredentialStore` implementation
   using one fixed application/service and account identity. Keep all standard tests deterministic;
   do not access a real keyring.
2. Supply secret bytes only through redirected standard input. Never place them in command
   arguments, environment variables, stdout/stderr, exceptions, or logs. Do not invoke a shell.
3. Bound native input/output, clear retained secret buffers, discard provider/OS details, and map
   save/replace, load, missing, delete, cancellation, launch failure, and nonzero exit to the existing
   redacted outcomes without broadening the Application credential contract.
4. Keep Linux availability explicit and fail closed when the required Secret Service facility is
   absent. Do not claim Linux support until the same bytes are proved on Linux; macOS-local fake
   tests alone are contract evidence.
5. Do not add Windows Credential Manager, Anthropic credential storage, scene/provider composition,
   settings UI/persistence, credential migration, telemetry/export, release, or distribution work.
