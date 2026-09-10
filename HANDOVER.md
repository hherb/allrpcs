# Allrpcs Handover

## Current state

`main` contains merged PR #169 at `1ff4f6f`. The current branch is
`codex/phase-4-linux-credential-lifecycle` and advances Phase 4 through Phase 4l2.

- Phase 4l1 adds `LinuxSecretServiceOpenAiCredentialStore` behind an exact
  `ILinuxSecretServiceProcessRunner` seam. It uses only `/usr/bin/secret-tool` with fixed
  `Allrpcs.OpenAI` application and `api-key` account attributes. Save bytes travel only through
  redirected standard input without a shell or added newline. Lookup output is bounded and cleared;
  stderr is observed only as present/absent and discarded. Availability is explicit and failures
  remain redacted. Delete performs a bounded, immediately cleared lookup probe because native
  `clear` does not distinguish a missing item.
- Phase 4l2 adds `WindowsOpenAiCredentialStore` behind `IWindowsCredentialManager`. The native
  implementation uses one fixed generic `Allrpcs.OpenAI` target and `api-key` user, current-user
  local-machine persistence, exact replace/read/delete operations, and redacted error mapping.
  Managed input, unmanaged write memory, copied output, and the OS-returned read blob are cleared.

Standard tests use only deterministic fakes and never access a real keyring or credential set. The
macOS host proves both contracts but not native Linux or Windows behavior; do not claim platform
support until the same bytes pass save, replace, load, missing, and delete checks on each platform.
The scene still has no production provider composition, settings UI/persistence, automatic local
server discovery, model process ownership, Anthropic credential store, diagnostic export, generated
non-wait intention execution, or gameplay persistence/mutation. The stable offline model remains the
ordinary default, and full Phase 4 acceptance remains incomplete.

## Validation

The complete host-local `./scripts/quality.sh` passes:

- workflow and formatting validation, restore, and a zero-warning build;
- 1,388 Domain tests, 336 Application tests, 335 Infrastructure tests, and eight architecture tests;
- the five-evidence representative scenario validator;
- all 68 opt-in Godot smoke invocations; and
- the bounded graphical Compatibility-renderer launch.

Focused red/green evidence covered 16 Linux Secret Service tests and 12 Windows Credential Manager
tests. The restricted sandbox reproduced the known MSBuild IPC denial; identical host-local commands
passed. GitHub Actions budget is exhausted for the month: do not dispatch hosted workflows, and treat
zero-step hosted failures as account infrastructure rather than test evidence.

## Exact next bounded slice

Take **Phase 4m1: exact caller-composed OpenAI binding construction**.

Acceptance boundary:

1. Add one Infrastructure composition helper that creates an exact `CharacterModelBinding` for
   caller-supplied OpenAI settings, `HttpClient`, and `IOpenAiCredentialSource`. The model and
   connection tester must share those exact dependencies; retain the exact settings instance.
2. Support the existing optional redacted diagnostic sink and caller-owned `TimeProvider` without
   adding logging, export, dialogue content, provider details, or a second credential read boundary.
3. Composition must perform no credential load, HTTP request, provider discovery, or scene mutation.
   Reject null or non-OpenAI settings before constructing any partial binding.
4. Prove exact dependency retention and no-I/O construction through focused deterministic tests.
   Do not wire the Godot scene or access a real credential/network.
5. Do not add Anthropic/Ollama composition, settings UI/persistence, automatic local-server
   discovery, model process ownership, Anthropic credential storage, generated intention execution,
   telemetry/export, native-platform proof, release, or distribution work.
