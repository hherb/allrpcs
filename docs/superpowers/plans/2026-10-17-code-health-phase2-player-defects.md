# Phase 2 Player Defects Implementation Plan

> **For agentic workers:** Use superpowers:subagent-driven-development task by task, with a task review and a final phase review. Track completion below; preserve incomplete work across continuation.

**Goal:** Fix the player-facing code-review-1 defects and the program's Phase 2 scope with regression tests.

**Architecture:** Keep the existing rule/application/infrastructure boundaries. Introduce only the shared stream parsing and restore ownership needed for these fixes; pipeline consolidation belongs to Phase 3. Use deterministic streams, clocks and native Main checks for integration boundaries.

**Tech Stack:** .NET 8, xUnit, Godot 4.6.3 .NET, Python >= 3.12, POSIX shell.

**Spec:** [Program Phase 2](2026-10-17-code-health-and-playable-slice.md), [review](../../../CLAUDE_CODE_REVIEW_1.md), and issues #414-#420 and #424-#431.

## Global Constraints

- Work only on `code-health-phase2-player-defects`; preserve closed scopes and deferred physical trials.
- No Godot types in `src/`; warnings are errors. Use file-scoped namespaces and existing formatting.
- Never alter frozen evidence, acceptance hashes, historical validators or `scripts/quality.sh`.
- Preserve unrelated work, `.DS_Store` and concurrent `speech-direction/` changes.
- For each fix, observe a failing regression test, implement, then run its tests. Run format/build/solution tests before task publication and affected native checks for engine changes.
- Evidence is issue + tests + commit under D2; run full `./quality.sh` once at phase exit.
- Commit and push under the standing publication grant. Do not merge or release.
- Do not expose credentials or retain private prompts/audio. A captured live stream must use fictional content and remove secrets/identifying metadata. Record unavailable live or human checks honestly.
- New check scenes must be registered in `scripts/smoke-godot.sh`/`scripts/check-smoke-registration.py` and run through `run_check`.

## Review Focus

1. Loading during pending participation discards the stale result and leaves dialogue idle (Task 5).
2. No provider or unavailable Ollama produces visible provider-unavailable status and leaves saves usable (Task 7).
3. HUD save/load and overlapping modals release world input after the final modal closes (Task 6).
4. A stalled provider request terminates with a retryable failure and visible status (Tasks 3, 7).
5. Portrait startup and resize clamp camera zoom without throwing (Task 8).

## Baseline and execution record

- Starting checkout: clean `code-health/phase1`, `e55463b0`.
- Phase 1 CI: [run 38997183007](https://github.com/hherb/allrpcs/actions/runs/38997183007), successful at `9a7e2715`; merge `e55463b0` run 38998433672 initially pending. Record its final result before product changes.
- Phase 1 follow-ups #432-#435, #437 and #439 remain separate; this phase does not silently close them.
- The owner-operated Keychain save check has been requested. No result is assumed.

### Task 1: Accept bounded schema echoes in every Responses accumulator (#417)

**Files:** Create `src/Infrastructure/Dialogue/OpenAiStreamDocument.cs`; modify all twelve `OpenAi*StreamAccumulator.cs` files under Infrastructure/Dialogue and Infrastructure/Characters; add `tests/Infrastructure.Tests/Dialogue/OpenAiStreamDocumentTests.cs` and schema-echo regression cases in the existing accumulator suites.

**Interfaces:** Shared `OpenAiStreamDocument.Parse(string serializedEvent)` returns a caller-disposed `JsonDocument`. Preserve each accumulator's event/text bounds and failure mapping. Maximum event depth 64, strict JSON, no comments/trailing commas.

- [ ] Add schema-echo events deeper than eight levels to dialogue, participation and all ten decision accumulators; assert ordinary completion remains possible. Assert depth >64 and overlong/malformed events still fail closed.
- [ ] Run focused tests and observe the old depth rejection.
- [ ] Introduce the helper and switch all twelve accumulators in the same task; do not consolidate their pipelines.
- [ ] Run Infrastructure tests, format/build/solution tests; inspect diff for all twelve consumers.
- [ ] Confirm with a fictional live OpenAI request and retain a sanitized real stream fixture when a configured credential is available; otherwise record the missing prerequisite and keep this step open.
- [ ] Commit `fix: accept bounded Responses schema echoes (Fixes #417)` only when the issue's completion evidence is satisfied; otherwise use `Refs #417`.

### Task 2: Stream incrementally and preserve dialogue and valid answers (#418, #419)

**Files:** `OpenAiServerSentEventReader.cs`, `AnthropicServerSentEventReader.cs`, `OllamaCharacterModelNdjsonCompletion.cs`, the OpenAI/Anthropic completion classes, `*StreamAccumulator.cs`, `AnthropicCharacterModelRequestSerializer.cs`, `OpenAiChatCompletionRequestSerializer.cs`, and matching Infrastructure tests. Shared readers live in Infrastructure/Dialogue.

**Interfaces:** Add an incremental `IAsyncEnumerable<string>` document API; keep `ReadAsync` as a bounded compatibility collector if used by tests. Completion consumers return immediately on the terminal semantic event, disposing the enumerator but leaving caller-owned streams open.

- [ ] Pin prior dialogue text in Anthropic and loopback requests (no unresolved `messageIndex` placeholder), a valid >1,024-delta reply within the response budget, and terminal-event completion before an unread/stalling tail.
- [ ] Pin bounded per-document/total-byte consumption for endless nonterminal streams, malformed JSON/UTF-8, cancellation, and split multibyte reads.
- [ ] Pin malformed, absent and partial usage telemetry as unknown while retaining a valid response, including Ollama `eval_count` without `prompt_eval_count`.
- [ ] Observe failures, implement incremental readers with finite total budgets sized for supported output tokens, then update every completion consumer and usage parser.
- [ ] Run affected Infrastructure tests and the solution checks; commit with `Fixes #418` and `Fixes #419`.

### Task 3: Bound provider requests, redirects and retries

**Files:** provider models in Infrastructure/Dialogue and Infrastructure/Characters; `game/TemporaryCharacterModelCompositionController.cs`; `src/Application/Dialogue/CharacterModelRetryPolicy.cs`; HTTP failure policy and matching tests. Add focused shared transport helpers under Infrastructure/Dialogue as needed.

**Interfaces:** All provider requests have a finite request deadline (120 seconds) and idle-read deadline (30 seconds), separately preserving caller cancellation. Use linked tokens, not abandoned reads. Production provider handlers set `AllowAutoRedirect = false`. Retryable server errors honour bounded `Retry-After` with cancellable delay.

- [ ] Use deterministic stalled headers/body streams to pin retryable timeout, caller cancellation and stream disposal; cover turn, participation, connection-test and decision consumers.
- [ ] Pin cross-host redirect rejection without forwarding Anthropic credentials; test handler construction rather than contacting a third party.
- [ ] Pin `Retry-After` seconds/date and malformed/excessive fallback without real long sleeps. Clamp delays to 30 seconds, use a one-second fallback and retain bounded attempt count.
- [ ] Observe failures, implement, run affected adapter/retry tests and solution checks; commit.
- [ ] Record the owner-operated Keychain result. Reproduce a confirmed defect through the process-runner seam before fixing it; never print or pass a key in argv. Leave this manual step open until reported.

### Task 4: Fit context within domain limits (#425)

**Files:** `src/Application/Dialogue/CharacterModelContextAssembler.cs`, `CharacterModel*ContextProjection.cs`, matching `tests/Application.Tests/Dialogue/` tests.

**Interfaces:** Preserve required current utterance, validation affordances and responding-character identity; select bounded optional memory/perception/history deterministically without constructing an overflowing intermediate snapshot.

- [ ] Pin maximum domain-valid memory/perception volumes, whitespace descriptions, and long required/optional combinations. Assert count/text limits, current utterance retention and deterministic ordering.
- [ ] Observe failure, implement budget-aware projection/selection with explicit precedence, run Application tests and solution checks; commit `fix: bound context assembly for valid world state (Fixes #425)`.

### Task 5: Restore every owner atomically (#420, #426, #429)

**Files:** `game/SaveSnapshotRestorationController.cs`, save-state owner controllers, `Main.cs`, ordinary participation controllers/coordinators, transcript/perception/journal/map owners; `src/Domain/Characters/CharacterSabotageTurnOpportunityCadence*.cs`; matching Domain tests and native Main/restore smoke checks. Create `game/IRestoreParticipant.cs` and registry beside the restore controller.

**Interfaces:** Registry preparation validates/builds every participant's pending state before any installed state changes. Commit prepared nonthrowing state in a fixed order; publication/reset notifications follow successful commits. Failed validation leaves all installed state and in-flight generation unchanged. Restore clears session-only transcript, participation/budget, perception labels and reading modals, and fences late async completions.

- [ ] Pin a failure in the final preparation stage leaving earlier state unchanged; verify all stateful restore owners are registered explicitly.
- [ ] Pin loading during a held participation result, then completing it; assert idle panel, no stale transcript/action/reply, and successful next submission.
- [ ] Pin cadence after actual serialization/deserialization with equal-valued distinct identifier instances. Use value equality for IDs while preserving authored-definition identity.
- [ ] Pin perception text cleared, journal/map closed and transcript empty after successful load; failed load preserves them.
- [ ] Observe failures, implement two-phase restore and required resets, run Domain/Application tests and affected native Main/restore checks; run solution checks and commit with issue closures.

### Task 6: Release input consistently and migrate controls (#427, #428)

**Files:** `game/SeekerCustomizationMenuController.cs`, `JournalOpenCloseInputController.cs`, `SeekerMapOpenCloseInputController.cs`, `SaveLoadInputController.cs`, Seeker input-suspension owner; `src/Infrastructure/Input/ControlPreferencesJson.cs`, control preference types/tests; relevant native input checks.

**Interfaces:** A single modal/input suspension owner tracks active causes; world input resumes only after all causes close. Save/load buttons release focus after activation. Existing controls documents missing newly introduced actions receive catalogue defaults for missing actions only, preserving valid customized bindings.

- [ ] Pin both open/close orders for overlapping map/journal/customization, HUD SAVE/LOAD focus and resumed movement. Keep other deliberate suspensions active.
- [ ] Pin old controls with a missing action, retained custom bindings, unknown/duplicate/invalid actions and migration idempotence.
- [ ] Observe failures, implement, run affected Infrastructure tests and native input checks; run solution checks and commit with `Fixes #427`/`Fixes #428`.

### Task 7: Make unavailable/busy dialogue visible and own delivery once

**Files:** `game/Main.cs`, `OrdinaryPlayerDialogueParticipationCoordinator.cs`, dialogue panel/input controllers, `TemporaryCharacterTurnResponseSpeechRecorder.cs`, participation/provider composition; relevant native Main checks.

**Interfaces:** Accepted submission shows busy while a current round owns it; another submission must not claim acceptance after a drop. Unconfigured/unavailable/timed-out provider exposes a retryable visible status. Mark delivery processed before side effects. Absent healer is excluded from hearing admission and speech history.

- [ ] Pin no-provider and stopped-Ollama failures, saving after failure, double submission during held response, and provider-timeout visible failure/retry.
- [ ] Pin a failing submission side effect is not retried every frame; absent healer hears/records no speech while the next visible healer turn works.
- [ ] Observe failures, implement without adding new gameplay, run registered Main checks and solution checks; commit.

### Task 8: Keep the camera valid on portrait startup and resize

**Files:** `src/Application/Camera/CameraFramingCalculator.cs`, `game/FollowCamera.cs`, `tests/Application.Tests/CameraFramingCalculatorTests.cs`, native camera check.

**Interfaces:** Compute effective minimum zoom as `max(configuredMinimum, viewportWidth/worldWidth, viewportHeight/worldHeight)`. Clamp default zoom and lift maximum if necessary to cover the world. Keep validation of nonfinite/nonpositive tuning. Recompute on viewport `SizeChanged`, unsubscribe on exit.

- [ ] Pin 9:16/2,275-pixel-height viewport and a covering zoom greater than configured maximum; assert ordered finite zoom and no exposed out-of-bounds area.
- [ ] Pin native landscape-to-portrait resize and return; ensure framing updates without exception and smoothing remains usable.
- [ ] Observe failures, implement, update former tests requiring a coverage exception to assert clamping; run Application/native camera and solution checks; commit.

### Task 9: Keep speech workers bounded and reusable (#430, #431)

**Files:** `src/Infrastructure/Speech/PocketSpeechSynthesizer.cs`, `PocketSpeechWorkerProcess.cs`, `MoonshineSpeechRecognizer.cs`, `PlayerSpeechWorker.cs`, `scripts/player-speech-worker.py`, `scripts/pocket-speech-worker.py`, and existing speech/worker test suites.

**Interfaces:** A valid request-scoped Pocket error ends that synthesis without waiting for an impossible `completed` event or discarding the warm worker. Capture `finished` is normal at supported sample rates. Timed-out reads are cancelled through linked tokens. Worker exit/stderr drains have finite disposal bounds and terminate their own process when needed.

- [ ] Pin error followed by successful reuse of the same fake Pocket worker, and avoid a two-second delay assertion based on scheduler luck.
- [ ] Pin `finished` at 11,025/22,050/44,100/48,000 Hz, malformed/error distinction and explicit stop.
- [ ] Pin Moonshine read cancellation on timeout and hung process/stderr shutdown using bounded test seams; retain ordinary cancellation semantics.
- [ ] Observe failures, implement, run speech xUnit and Python worker tests plus affected native speech checks; run solution checks and commit with issue closures.

### Task 10: Exclude harness assets and gate test overrides in exports

**Files:** `game/export_presets.cfg`, credential/settings/ordinary-participation controllers reading test-store variables, `scripts/check-smoke-registration.py`, focused shipping checks.

**Interfaces:** All six export presets exclude `*Check*`, `*Proof*`, `*Probe*` resources. Test-store/model overrides take effect only in debug builds; ordinary production settings continue to work.

- [ ] Pin every preset's exclusions and all test-environment read sites behind a debug-only policy; verify Main still imports/loads under debug smoke checks.
- [ ] Implement minimal exclusions and gating, inspect exported resource references for missing required runtime scripts; run focused checks and solution checks; commit.

### Task 11: Make quality resume/retry and ledger protection reliable (#414, #415, #416)

**Files:** root `quality.py`, `scripts/check-continuation.py` and their existing self-tests. Never edit `scripts/quality.sh`.

**Interfaces:** Product-smoke resume accepts a fresh failed report, preserves its failed-stage record until recovery succeeds, and does not lose it on early resume errors. Native shutdown retry is forbidden after any `FAIL:` marker. Continuation validation uses a persistent committed baseline, not solely a mutable HEAD comparison; rejects empty/whitespace roadmap items. Each quality stage has a finite wall-clock timeout with a recorded failed stage.

- [ ] Pin fresh `product-10` resume, failed early resume retaining prior report, and shutdown stderr containing both native mutex abort and `FAIL:`.
- [ ] In temporary Git fixtures, pin pruning committed into HEAD remains rejected, preserved historical entries pass, and empty/whitespace roadmap item fails.
- [ ] Pin a hung child stage is terminated and recorded within an injected short timeout, including process cleanup. Use a documented generous default for long frozen validator stages.
- [ ] Observe failures, implement, run both self-tests, continuation sync/check/self-test and Python compilation; commit with issue closures.

### Task 12: Preserve bounded long-session state and correct ducking (#424)

**Files:** `src/Application/Audio/AudioDuckingEnvelope.cs`; Domain character relationship/summary transitions, `CharacterTurnTranscript*`, `WorldEventLog*`, the list-holding records identified in review §1.7 and corresponding Domain/Application tests.

**Interfaces:** Ducking follows strongest currently active overlap, preserving attack/release behavior. Relationship reasons evict oldest beyond 64. Full summary recording returns an explicit capacity rejection instead of throwing. Transcript/world-event appends use persistent/chunked storage with O(1) amortized append and immutable earlier snapshots. List-bearing domain values use structural equality or explicit identity classes, consistently with callers.

- [ ] Pin strongest foreground ending while a weaker one remains, then release to baseline.
- [ ] Pin the 65th relationship reason and recording beyond summary capacity; retain valid prior state and finite storage.
- [ ] Pin old transcript/event snapshots remain unchanged after many appends, ordering/lookups and bounded linear total append work using structural metrics rather than flaky timings.
- [ ] Inventory the 19 list-holding records, pin equality/hash consistency for equal-valued separately allocated lists or document identity-class semantics and update callers/tests.
- [ ] Observe failures, implement, run Domain/Application tests, native audio/transcript checks and solution checks; commit `Fixes #424` with the related bounded-session fixes.

### Task 13: Review, phase exit and publication

- [ ] Resolve task reviews and run a fresh whole-phase review covering all five Review Focus cases.
- [ ] Run `dotnet format Allrpcs.sln --no-restore --verify-no-changes`, build with zero warnings, and `dotnet test Allrpcs.sln --no-build`.
- [ ] Run one full `./quality.sh`; diagnose failures, preserve logs and repair relevant defects. Do not infer a pass from an incomplete run.
- [ ] Confirm original issues #414-#420/#424-#431 closed by tested commits; list genuinely unavailable manual/live evidence rather than claiming it.
- [ ] Only after all exit conditions pass, close Phase 2 in the ledger and roadmap, select Phase 3's named missing deliverable, sync/check/self-test and commit/push. Otherwise keep Phase 2 in progress with the exact remaining task.

## Progress

No product task completed yet. Full quality gate is reserved for phase exit.
