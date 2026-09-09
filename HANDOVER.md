# Allrpcs Handover

## Current state

`main` contains merged PR #161 at `53c7d11`. The branch
`codex/phase-4-controller-diagnostics` advances Phase 4 through Phase 4e2 and is prepared for a
draft pull request.

- Phase 4d12 adds opt-in controller diagnostics over an exact canonical provider, caller-owned
  monotonic `TimeProvider`, observational sink, and cancellation independent of generation.
  Existing generation calls are unchanged. The controller records exactly one content-free
  diagnostic only when it synthesizes `Cancelled` or `TimedOut`; provider terminal updates and
  controller validation failures are not duplicated.
- Controller diagnostic elapsed time covers the bounded retry lifecycle and clamps negative values
  to zero and excessive values to the established one-hour maximum. Time-source, sink, and
  diagnostic-cancellation failures cannot change the authoritative stream result.
- Phase 4e1 adds five canonical content-free presentation states: generating, completed,
  cancelled, timed out, and failed. Projection retains neither the supplied stream nor text,
  response, failure, provider, usage, or diagnostic data.
- Phase 4e2 mounts a temporary hidden status label in the dialogue overlay. Its explicit presentation
  seam can show the safe canonical timeout text and clear it without changing or retaining the
  supplied stream. It is not yet driven by a scene-owned model-stream lifecycle.

No live key, provider call, or hosted workflow was used. Runtime model composition, actual
controller-to-label scene wiring, partial display streaming, generated-intention execution,
Anthropic/Ollama adapters, Windows Credential Manager, Linux Secret Service, settings UI or
persistence, logging/telemetry export, and gameplay persistence or mutation remain absent. The
OpenAI adapter and end-to-end cancellation/visible-timeout roadmap items remain incomplete.

## Validation

Each slice began with the expected focused compile or scene-load failure. Focused host-local
results:

- 11 controller-diagnostic tests pass.
- Six content-free generation-status presentation tests pass.
- The focused Godot smoke passes hidden initial state, generating state, visible timeout, exact
  input preservation, and clear state.
- The complete Application project passes 315 tests.

The complete host-local `./scripts/quality.sh` passes workflow and formatting validation, restore,
a zero-warning build, 1,388 Domain tests, 315 Application tests, 159 Infrastructure tests, eight
architecture tests, the five-evidence representative scenario validator, all 52 opt-in Godot
smokes, and the bounded graphical launch.

The restricted sandbox still reproduces the known .NET IPC/VSTest denial; identical host-local
commands pass. GitHub Actions budget is exhausted for the month: do not dispatch hosted workflows,
and treat zero-step hosted failures as account infrastructure rather than test results.

## Exact next bounded slice

Take **Phase 4e3: own and present one exact model-stream lifecycle in the Godot scene without
constructing a provider**.

Acceptance boundary:

1. Add an opt-in scene controller that owns one immutable `CharacterModelStreamSnapshot`, starts
   from an explicit empty snapshot, and appends exact updates only through the existing Application
   transition. Reject null, repeated initialization, and post-terminal appends without changing
   state.
2. Make the temporary model-status label observe that exact controller snapshot and present the
   canonical generating or terminal state. Do not retain or display model text, complete responses,
   failure codes, provider identities, usage, or diagnostics in the label.
3. Prove in a focused Godot smoke that text-prefix appends preserve a visible generating state, an
   exact timeout termination becomes visibly timed out, rejected later updates preserve both
   controller and label state, and no character turn, transcript, world, discovery, movement, or
   existing response presentation changes.
4. Do not construct an OpenAI adapter, access credentials or network, start asynchronous generation,
   wire player cancellation/deadlines, display partial response text, execute intentions, add
   persistence/telemetry, or add Anthropic/Ollama/Windows/Linux provider facilities.
