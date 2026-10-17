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
- New check scenes must be registered in current smoke tooling and run through `run_check`; validate their registration with the existing harness or a focused check.

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
- The owner-operated Keychain save check has been requested. No result is assumed. A host existence-only query returned item-not-found for the application-specific credential; no secret was requested and the live OpenAI fixture remains unavailable.
- Preparatory source inventory corrected two stale plan assumptions: three existing export presets, and eighteen directly collection-bearing domain records. Cover every current instance; do not invent extra presets or records. The preflight at `66282920` recorded 214 matching Main harness resource edges plus the CheckDeadline autoload; export filtering must enumerate the current graph after new checks are added.

### Task 1: Accept bounded schema echoes in every Responses accumulator (#417)

**Files:** Create `src/Infrastructure/Dialogue/OpenAiStreamDocument.cs`; modify all twelve `OpenAi*StreamAccumulator.cs` files under Infrastructure/Dialogue and Infrastructure/Characters; add `tests/Infrastructure.Tests/Dialogue/OpenAiStreamDocumentTests.cs` and schema-echo regression cases in the existing accumulator suites.

**Interfaces:** Shared `OpenAiStreamDocument.Parse(string serializedEvent)` returns a caller-disposed `JsonDocument`. Preserve each accumulator's event/text bounds and failure mapping. Maximum event depth 64, strict JSON, no comments/trailing commas.

- [x] Add schema-echo events deeper than eight levels to dialogue, participation and all ten decision accumulators; assert ordinary completion remains possible. Assert depth >64 and overlong/malformed events still fail closed.
- [x] Run focused tests and observe the old depth rejection.
- [x] Introduce the helper and switch all twelve accumulators in the same task; do not consolidate their pipelines.
- [x] Run Infrastructure tests, format/build/solution tests; inspect diff for all twelve consumers.
- [ ] Confirm with a fictional live OpenAI request and retain a sanitized real stream fixture when a configured credential is available; otherwise record the missing prerequisite and keep this step open.
- [x] Commit `fix: accept bounded Responses schema echoes (Fixes #417)` only when the issue's completion evidence is satisfied; otherwise use `Refs #417`.

### Task 2: Stream incrementally and preserve dialogue and valid answers (#418, #419)

**Files:** `OpenAiServerSentEventReader.cs`, `AnthropicServerSentEventReader.cs`, `OllamaCharacterModelNdjsonCompletion.cs`, the OpenAI/Anthropic completion classes, `*StreamAccumulator.cs`, `AnthropicCharacterModelRequestSerializer.cs`, `OpenAiChatCompletionRequestSerializer.cs`, and matching Infrastructure tests. Shared readers live in Infrastructure/Dialogue.

**Interfaces:** Add an incremental `IAsyncEnumerable<string>` document API; keep `ReadAsync` as a bounded compatibility collector if used by tests. Completion consumers return immediately on the terminal semantic event, disposing the enumerator but leaving caller-owned streams open.

- [x] Pin prior dialogue text in Anthropic and loopback requests (no unresolved `messageIndex` placeholder), a valid >1,024-delta reply within the response budget, and terminal-event completion before an unread/stalling tail.
- [x] Pin bounded per-document/total-byte consumption for endless nonterminal streams, malformed JSON/UTF-8, cancellation, and split multibyte reads.
- [x] Pin malformed, absent and partial usage telemetry as unknown while retaining a valid response, including Ollama `eval_count` without `prompt_eval_count`.
- [x] Observe failures, implement incremental readers with finite total budgets sized for supported output tokens, then update every completion consumer and usage parser.
- [x] Run affected Infrastructure tests and the solution checks; commit with `Fixes #418` and `Fixes #419`.

### Task 3: Bound provider requests, redirects and retries

**Files:** provider models in Infrastructure/Dialogue and Infrastructure/Characters; `game/TemporaryCharacterModelCompositionController.cs`; `src/Application/Dialogue/CharacterModelRetryPolicy.cs`; HTTP failure policy and matching tests. Add focused shared transport helpers under Infrastructure/Dialogue as needed.

**Interfaces:** All provider requests have a finite request deadline (120 seconds) and idle-read deadline (30 seconds), separately preserving caller cancellation. Use linked tokens, not abandoned reads. Production provider handlers set `AllowAutoRedirect = false`. Retryable server errors honour bounded `Retry-After` with cancellable delay.

- [x] Use deterministic stalled headers/body streams to pin retryable timeout, caller cancellation and stream disposal; cover turn, participation, connection-test and decision consumers.
- [x] Pin cross-host redirect rejection without forwarding Anthropic credentials; test handler construction rather than contacting a third party.
- [x] Pin `Retry-After` seconds/date and malformed/excessive fallback without real long sleeps. Clamp delays to 30 seconds, use a one-second fallback and retain bounded attempt count.
- [x] Observe failures, implement, run affected adapter/retry tests and solution checks; commit.
- [ ] Record the owner-operated Keychain result. Reproduce a confirmed defect through the process-runner seam before fixing it; never print or pass a key in argv. Leave this manual step open until reported.

### Task 4: Fit context within domain limits (#425)

**Files:** `src/Application/Dialogue/CharacterModelContextAssembler.cs`, `CharacterModel*ContextProjection.cs`, matching `tests/Application.Tests/Dialogue/` tests.

**Interfaces:** Preserve required current utterance, validation affordances and responding-character identity; select bounded optional memory/perception/history deterministically without constructing an overflowing intermediate snapshot.

- [x] Pin maximum domain-valid memory/perception volumes, whitespace descriptions, and long required/optional combinations. Assert count/text limits, current utterance retention and deterministic ordering.
- [x] Observe failure, implement budget-aware projection/selection with explicit precedence, run Application tests and solution checks; commit with `Refs #425` until Task 7 integrates the typed mandatory-budget failure into the game caller.

### Task 5: Restore every owner atomically (#420, #426, #429)

**Files:** `game/SaveSnapshotRestorationController.cs`, save-state owner controllers, `Main.cs`, ordinary participation controllers/coordinators, transcript/perception/journal/map owners; `src/Domain/Characters/CharacterSabotageTurnOpportunityCadence*.cs`; matching Domain tests and native Main/restore smoke checks. Create `game/IRestoreParticipant.cs` and registry beside the restore controller.

**Interfaces:** Registry preparation validates/builds every participant's pending state before any installed state changes. Commit prepared nonthrowing state in a fixed order; publication/reset notifications follow successful commits. Failed validation leaves all installed state and in-flight generation unchanged. Restore clears session-only transcript, participation/budget, perception labels and reading modals, and fences late async completions.

- [x] Pin a failure in the final preparation stage leaving earlier state unchanged; verify all stateful restore owners are registered explicitly.
- [x] Pin loading during a held participation result, then completing it; assert idle panel, no stale transcript/action/reply, and successful next submission.
- [x] Pin cadence after actual serialization/deserialization with equal-valued distinct identifier instances. Use value equality for IDs while preserving authored-definition identity.
- [x] Pin perception text cleared, journal/map closed and transcript empty after successful load; failed load preserves them.
- [x] Observe failures, implement two-phase restore and required resets, run Domain/Application tests and affected native Main/restore checks; run solution checks and commit with issue closures.

### Task 6: Release input consistently and migrate controls (#427, #428)

**Files:** `game/SeekerCustomizationMenuController.cs`, `JournalOpenCloseInputController.cs`, `SeekerMapOpenCloseInputController.cs`, `SaveLoadInputController.cs`, Seeker input-suspension owner; `src/Infrastructure/Input/ControlPreferencesJson.cs`, control preference types/tests; relevant native input checks.

**Interfaces:** A single modal/input suspension owner tracks active causes; world input resumes only after all causes close. Save/load buttons release focus after activation. Existing controls documents missing newly introduced actions receive catalogue defaults for missing actions only, preserving valid customized bindings.

- [x] Pin both open/close orders for overlapping map/journal/customization, HUD SAVE/LOAD focus and resumed movement. Keep other deliberate suspensions active.
- [x] Pin old controls with a missing action, retained custom bindings, unknown/duplicate/invalid actions and migration idempotence.
- [x] Observe failures, implement, run affected Infrastructure tests and native input checks; run solution checks and commit with `Fixes #427`/`Fixes #428`.

### Task 7: Make unavailable/busy dialogue visible and own delivery once

**Files:** `game/Main.cs`, `OrdinaryPlayerDialogueParticipationCoordinator.cs`, dialogue panel/input controllers, `TemporaryCharacterTurnResponseSpeechRecorder.cs`, participation/provider composition; relevant native Main checks.

**Interfaces:** Accepted submission shows busy while a current round owns it; another submission must not claim acceptance after a drop. Unconfigured/unavailable/timed-out provider exposes a retryable visible status. Mark delivery processed before side effects. Absent healer is excluded from hearing admission and speech history.

- [x] Pin no-provider and stopped-Ollama failures, saving after failure, double submission during held response, and provider-timeout visible failure/retry.
- [x] Pin a failing submission side effect is not retried every frame; absent healer hears/records no speech while the next visible healer turn works.
- [x] Observe failures, implement without adding new gameplay, run registered Main checks and solution checks; commit.

### Task 8: Keep the camera valid on portrait startup and resize

**Files:** `src/Application/Camera/CameraFramingCalculator.cs`, `game/FollowCamera.cs`, `tests/Application.Tests/CameraFramingCalculatorTests.cs`, native camera check.

**Interfaces:** Compute effective minimum zoom as `max(configuredMinimum, viewportWidth/worldWidth, viewportHeight/worldHeight)`. Clamp default zoom and lift maximum if necessary to cover the world. Keep validation of nonfinite/nonpositive tuning. Recompute on viewport `SizeChanged`, unsubscribe on exit.

- [x] Pin 9:16/2,275-pixel-height viewport and a covering zoom greater than configured maximum; assert ordered finite zoom and no exposed out-of-bounds area.
- [x] Pin native landscape-to-portrait resize and return; ensure framing updates without exception and smoothing remains usable.
- [x] Observe failures, implement, update former tests requiring a coverage exception to assert clamping; run Application/native camera and solution checks; commit.

### Task 9: Keep speech workers bounded and reusable (#430, #431)

**Files:** `src/Infrastructure/Speech/PocketSpeechSynthesizer.cs`, `PocketSpeechWorkerProcess.cs`, `MoonshineSpeechRecognizer.cs`, `PlayerSpeechWorker.cs`, `scripts/player-speech-worker.py`, `scripts/pocket-speech-worker.py`, and existing speech/worker test suites.

**Interfaces:** A valid request-scoped Pocket error ends that synthesis without waiting for an impossible `completed` event or discarding the warm worker. Capture `finished` is normal at supported sample rates. Timed-out reads are cancelled through linked tokens. Worker exit/stderr drains have finite disposal bounds and terminate their own process when needed.

- [x] Pin error followed by successful reuse of the same fake Pocket worker, and avoid a two-second delay assertion based on scheduler luck.
- [x] Pin `finished` at 11,025/22,050/44,100/48,000 Hz, malformed/error distinction and explicit stop.
- [x] Pin Moonshine read cancellation on timeout and hung process/stderr shutdown using bounded test seams; retain ordinary cancellation semantics.
- [x] Observe failures, implement, run speech xUnit and Python worker tests plus affected native speech checks; run solution checks and commit with issue closures.

### Task 10: Exclude harness assets and gate test overrides in exports

**Files:** `game/export_presets.cfg`, credential/settings/ordinary-participation controllers reading test-store variables, focused shipping/registration checks (the planned `scripts/check-smoke-registration.py` does not exist in this checkout).

**Interfaces:** All three existing development export presets exclude `*Check*`, `*Proof*`, `*Probe*` resources. Test-store/model overrides take effect only in debug builds; ordinary production settings continue to work.

- [x] Pin every preset's exclusions and all test-environment read sites behind a debug-only policy; verify Main still imports/loads under debug smoke checks.
- [x] Implement minimal exclusions and gating, inspect exported resource references for missing required runtime scripts; run focused checks and solution checks; commit.

### Task 11: Make quality resume/retry and ledger protection reliable (#414, #415, #416)

**Files:** root `quality.py`, `scripts/check-continuation.py` and their existing self-tests. Never edit `scripts/quality.sh`.

**Interfaces:** Product-smoke resume accepts a fresh failed report, preserves its failed-stage record until recovery succeeds, and does not lose it on early resume errors. Native shutdown retry is forbidden after any `FAIL:` marker. Continuation validation uses a persistent committed baseline, not solely a mutable HEAD comparison; rejects empty/whitespace roadmap items. Each quality stage has a finite wall-clock timeout with a recorded failed stage.

- [x] Pin fresh `product-10` resume, failed early resume retaining prior report, and shutdown stderr containing both native mutex abort and `FAIL:`.
- [x] In temporary Git fixtures, pin pruning committed into HEAD remains rejected, preserved historical entries pass, and empty/whitespace roadmap item fails.
- [x] Pin a hung child stage is terminated and recorded within an injected short timeout, including process cleanup. Use a documented generous default for long frozen validator stages.
- [x] Observe failures, implement, run both self-tests, continuation sync/check/self-test and Python compilation; commit with issue closures.

### Task 12: Preserve bounded long-session state and correct ducking (#424)

**Files:** `src/Application/Audio/AudioDuckingEnvelope.cs`; Domain character relationship/summary transitions, `CharacterTurnTranscript*`, `WorldEventLog*`, the list-holding records identified in review §1.7 and corresponding Domain/Application tests.

**Interfaces:** Ducking follows strongest currently active overlap, preserving attack/release behavior. Relationship reasons evict oldest beyond 64. Full summary recording returns an explicit capacity rejection instead of throwing. Transcript/world-event appends use persistent/chunked storage with O(1) amortized append and immutable earlier snapshots. List-bearing domain values use structural equality or explicit identity classes, consistently with callers.

- [x] Pin strongest foreground ending while a weaker one remains, then release to baseline.
- [x] Pin the 65th relationship reason and recording beyond summary capacity; retain valid prior state and finite storage.
- [x] Pin old transcript/event snapshots remain unchanged after many appends, ordering/lookups and bounded linear total append work using structural metrics rather than flaky timings.
- [x] Inventory all collection-bearing records (18 direct records verified; the historical review says 19), pin equality/hash consistency for equal-valued separately allocated lists or document identity-class semantics and update callers/tests.
- [x] Observe failures, implement, run Domain/Application tests, native audio/transcript checks and solution checks; commit `Fixes #424` with the related bounded-session fixes.

### Task 13: Review, phase exit and publication

- [x] Resolve task reviews and run a fresh whole-phase review covering all five Review Focus cases.
- [x] Run `dotnet format Allrpcs.sln --no-restore --verify-no-changes`, build with zero warnings, and `dotnet test Allrpcs.sln --no-build`.
- [ ] Run one full `./quality.sh`; diagnose failures, preserve logs and repair relevant defects. Do not infer a pass from an incomplete run.
- [ ] Confirm original issues #414-#420/#424-#431 closed by tested commits; list genuinely unavailable manual/live evidence rather than claiming it.
- [ ] Only after all exit conditions pass, close Phase 2 in the ledger and roadmap, select Phase 3's named missing deliverable, sync/check/self-test and commit/push. Otherwise keep Phase 2 in progress with the exact remaining task.

## Progress

| Task | State / commits | Evidence |
| --- | --- | --- |
| 1 Responses depth | Local implementation reviewed; `285a5128` (Refs #417) | 68 new regressions; 5,756 solution tests. Live request/captured stream remains open. |
| 2 Incremental providers | Complete; `d0a04c21` (Fixes #418/#419) | Independent review; 5,776 solution tests. |
| 3 Deadlines/retries | Local implementation reviewed; `e1e4a0e9` | 18 consumers bounded; 5,858 solution tests and seven native checks. Owner Keychain save check remains open. |
| 3 Fixture follow-ups | Complete; `94de440b`, `b06085a5` | Retry-hint and deterministic 10 ms diagnostic fixtures; independent reviews; 5,889 concurrent solution tests. |
| 4 Context budgets | Complete with Task 7 caller; `bf118728`, `36d06fad` | Required typed failure, optional priority and invalid-input precedence; scoped review; 1,455 Application tests. |
| 5 Atomic restore | Complete; `e6dccb64`, `39d88edb` (Fixes #420/#426/#429) | 5,866 solution tests; native ownership, held-load and observer ordering; scoped review. |
| 6 Input/controls migration | Complete; `e56cf1be` (Fixes #427/#428) | 5,867 solution tests and 24 native input checks; independent review. |
| 7 Dialogue ownership/status | Complete; `5fece811` (Fixes #425) | 62 affected native variants across diagnosed batches; exact reply ownership; 15 launcher self-tests; independent review. |
| 8 Camera | Complete; `cf652f1c`, `fce0ad5c` | Actual-edge native checks at fixed and ordinary cadence; 21 camera/1,459 Application tests; overflow review correction. |
| 9 Speech workers | Complete; `550a53e4` (Fixes #430/#431) | 42 speech tests, seven Python tests, four native modes; independent review. |
| 10 Shipping hygiene | Complete; `4592f02d` | Three presets; compiled release guard; real pack graph (1,245 entries/431 dependencies); eight native checks; independent review. Executable export remains unsupported. |
| 11 Tooling | Complete; `76f72ec9`, `544d2d5c` (Fixes #414/#415/#416) | 20 quality self-tests; real Git history and cleanup-interruption regressions; continuation/actionlint; scoped review. |
| 12 Long sessions/ducking | Complete; `075d3ac2` (Fixes #424) | 5,921 solution tests, zero-warning build/format, four native checks; eighteen identity classes, bounded persistent append, capacity/eviction and ducking regressions; independent review. |
| 13 Phase exit | In progress; draft PR [#440](https://github.com/hherb/allrpcs/pull/440) | Whole-phase review and scoped repairs approved through `c062a5a4`; provider Ready `37bc8fcd` and music eligibility `b4eab186` corrections also approved. Full gate and latest CI remain. Live #417 and owner Keychain result stay open. |

Phase 1 branch CI run 38998433672 and the subsequently merged main run 38998468190 both failed the same unquarantined 50 ms model-timeout presentation fixture (324 passed, two existing quarantines). Main `032c5b30` merged PR #438 and has an empty tree diff from the starting `e55463b0`, so no upstream product change was lost. Task 3 reproduced and repaired the fixture, including a passing 10 FPS run. PR #440 run 39007686673 passed C# formatting/build/tests, Python/tooling and Linux unsigned-build jobs; Godot was still running when the reviewed CI-only correction was prepared. The correction enables the existing twenty quality-runner self-tests in the now full-history Python job. Latest run 39008345177 passed 5,921 solution tests and 23 runner self-tests but failed four native fixtures (359 passed, two existing quarantines). `b5d020cd` restores the new Task 7 harness before the accepted final channel node and explicitly makes the two direct audio fixture speakers present. All four exact RED checks then passed, together with three hidden/once ownership cases; build/format and scoped review passed. Run 39009633071 then passed those four checks (362 native passes, one different failure, two existing quarantines). The scripted provider fixture's generic terminal assertion failed; its original individual predicate values were not logged. Controlled experiments proved a separate three-second independent-task occurrence changes the asserted transcript/EventLog/WorldTime during real retry, while the exact model and turn predicates remain correct. The named repeated-offline peer has the same demonstrated vulnerability; no hosted peer failure is claimed. `2d10ba23` isolates elapsed independent-task time in those two disposable fixtures by calling Main once per frame with zero delta, retaining physics, all original assertions, real retry and frame/deadline bounds. Both controlled RED-to-GREEN cases, eight final native checks, zero-warning build/format and scoped review pass. Manual fixture ordering differs from automatic parent dispatch; automatic ownership/timeout/navigation/healer-command checks supply the explicit integration coverage. A final hosted result is still required. #432-#435/#437/#439 stay separate.

Verification limits remain explicit:

- The application-specific Keychain item was absent in an existence-only host query; no secret was retrieved. Live #417 capture and the owner-operated save result remain open.
- An initial incorrectly isolated Task 7 case may have contacted an inherited provider binding; traffic was not instrumented, so it supplies no live qualification. Subsequent synthetic evidence isolates settings and configuration.
- Task 10's real resource pack/remap/load checks pass using local editor assemblies. Existing missing `game/Allrpcs.Game.sln` errors leave no managed DLL in that export; no executable export passed.
- A late current controls assertion fails identically with the pre-Task-10 controller code. Accepted frozen controls evidence remains unchanged; no new controls audit or repair was opened.
- Recorded native ObjectDB/editor shutdown warnings do not establish memory-lifecycle qualification. Physical microphone/controller/reader qualification remains paused or deferred at its existing scope.

The first full-gate attempt stopped before stage 1 because the live export preset successor was compared against its frozen herb predecessor. `571f1e22` narrowly validates that exact predecessor at the existing pinned revision; unchanged hashes, corrupt-history and unrelated-drift rejection are pinned by three new tests. Actual guard, all 23 self-tests and scoped review pass. The next run completed 25 stages, then the controller stopped it to repair the four concrete hosted regressions above. Its failed/interrupted report and logs are retained; neither incomplete run counts as a full pass. Ctrl-C cleanup reported a host `killpg` permission error; a host process inventory confirmed no stage, build or Godot process remained. No broad host cancellation qualification is inferred. A subsequent run completed 34 stages before the controller stopped product-10 for the new scripted-fixture diagnosis; its report/logs are retained, and that cancellation cleaned up normally. Restart the full gate after the reviewed repairs; do not weaken frozen validators or infer success from partial evidence.

Hosted run 39011243428 on `7c348508` is green: 5,921 solution tests, 23 quality-runner self-tests, native 363 passed / zero failed / two existing quarantines. The same source's complete local gate passed 44 stages (all nineteen product checks), then failed `interior-normal` because its direct reply fixture also lacked a visible Healer. A bounded inventory found this to be the only remaining current direct-audio-recorder precondition gap. `c062a5a4` adds one visibility line; exact RED, all 22 current interior/water/cask/tool modes and complete save/transcript/count parity pass. Build, format, actual frozen-evidence guard and scoped review pass without guard/production/frozen-input changes. The failed report is retained; a complete restarted gate and latest hosted result remain required. At 06:55:52 UTC the existence-only Keychain recheck still returned item-not-found; no secret or live evidence was obtained.

Hosted run 39013194774 on `d1ef178d` failed the scripted provider check (362 passed, one failed, two existing quarantines). Its new diagnostics prove unrelated independent-task mutation despite the earlier freeze: the model and exact cancellation predicates hold, but a physical-action entry replaces transcript/EventLog/WorldTime. Godot automatically enables an overridden parent process callback after child readiness. Both actual first-frame lifecycle probes reproduced the problem before manual dispatch. `37bc8fcd` freezes each parent from its Ready signal and asserts exclusive manual processing and zero independent progress on every frame. Slow 2 FPS probes passed over 18.77 and 5.84 seconds; the offline check also waits within its existing frame bound for participation readiness before all unchanged exact capture assertions. Eight exact/automatic integration checks, build, format and frozen guard pass. Scoped spec/quality review passed with no findings. The controller retains the explicit limitation that these two manual fixtures do not qualify automatic parent dispatch; the separate integration checks provide that evidence. The stopped full gate had 34 passing stages; its interruption report and logs are retained under `interrupted-before-ready-freeze-fix`, and no complete pass is claimed. The initial clock-isolation implementation was insufficient and is explicitly reworked, not validated by the earlier green CI.

Hosted run 39014801698 on `9dfa4b7b` passed the provider checks and three non-Godot jobs, but failed the music fixture's earlier exploration admission (native 362 passed, one failed, two existing quarantines). The local gate was stopped during historical snapshot extraction after 33 passing stages; `interrupted-before-music-exploration-fix` retains the failed report and logs. Read-only diagnosis identified an unproved fixture prerequisite: after arbitrary startup time, adding 180 seconds in quarter-second increments need not make its cooldown exactly zero. A controlled actual-controller/native RED reproduces a positive `2.842170943040401e-14`-second remainder and empty pending/active music before region transitions. The exact hosted pre-transition value was not logged, so this numeric cause remains inferred for that historical run. `b4eab186` settles only an actual finite residual at most 1e-9 seconds and requires exact zero before the unchanged transitions. Identical fractional-seed GREEN and real 180-second cooldown rejection pass. Eight native modes/integrations, four-mode complete-save/transcript parity, build/format/frozen guard and scoped spec/quality review pass. The controller accepts the reviewer's declined retrospective numeric judgment: the precise original hosted value stays inferred. Production schedule rules, real movement and all original admission/timing assertions remain unchanged. A complete restarted gate and fresh hosted result remain required.

The fresh whole-phase reviewer read the entire 293-file diff and all twelve reports. All five Review Focus pins passed review; the single P3 workflow omission was fixed and its scoped rereview approved. The controller accepted each explicit declined judgment: broader unchanged sabotage/new save coverage stays at its later schema boundary; standalone executable export remains unverified; the unchanged late controls assertion is not treated as passing; excluded follow-ups/quarantines and Phase 3 remain excluded; synthetic/native evidence establishes no physical, platform-wide or leak certification. These limits remain open records, not implementation passes.

## Execution decisions and rework costs

These are controller rulings, in decision order. They supplement the exact implementation and test evidence; they do not grant a successor scope.

| Decision | Cost if wrong |
| --- | --- |
| Reuse the clean feature checkout in place. | Branch-local rework if separate isolation was preferred. |
| Execute the approved write-then-execute plan without another approval boundary. | Reversible implementation rework. |
| Reuse successful Phase 1 evidence for an identical tree while its redundant run finishes. | Delayed discovery of a runner-only failure; the observed timeout failure was subsequently fixed in Task 3. |
| Return typed required-context capacity failure while retaining Assemble compatibility. | An extra API seam; its game caller is now integrated. |
| Prioritize completed outcomes and goals before optional perception/history. | Lower-priority optional history can be omitted at limits. |
| Use central named restore adapters with prepare/commit/publish. | Explicit owner inventory maintenance. |
| Make the current held-load fixture prove exact immediate retirement. | Fixture-contract risk if identity checks were weak; no-stale and fresh-submit assertions remain. |
| Pace the current repeated-offline fixture at 30 FPS with its existing bounds. | Slower, scheduler-dependent check; no production clock seam or historical invocation changed. |
| Reset smoothing on resize only when actual camera edges exceed bounds. | A brief snap for an invalid resized view. |
| Use export-owned PackedScene replacement and temporary restored autoload settings. | Dependence on Godot export hooks and upgrade rework. |
| Protect pinned closure state plus descendant history and require full Git history. | Fetch/storage cost and dependence on baseline reachability. |
| Use chunked skew storage and a compressed exact-ID crit-bit index. | O(log n) older indexing, implementation complexity and a bounded but potentially large historical-root constant. |
| Give all eighteen list-bearing domain types explicit instance identity. | Unseen callers lose record equality/copy/deconstruction semantics; repository callers are compiled and tested. |
| Smooth partial ducking release toward the strongest remaining target. | Changed partial-overlap envelope; existing final hold/release behavior remains covered. |
| Isolate unrelated independent-task elapsed time in the two disposable provider fixtures. | Manual Main dispatch changes fixture frame ordering; automatic-loop integration remains separately checked. |
| Settle only a bounded numeric cooldown residual before current music fixture region transitions. | Fixture numeric-threshold and observer-order maintenance; larger or nonfinite residuals must fail and production scheduling remains exact. |
