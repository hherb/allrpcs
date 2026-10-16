# Claude Code review #1 — suggestions and critique

Date: 2026-10-16 · Reviewed revision: `18c6ed91` (main) · Reviewer: Claude Code (Opus 5.5)

## Scope and method

The whole repository was reviewed in eight parallel, read-only passes:

- `src/Domain`
- `src/Application`
- the LLM provider adapters
- the rest of `src/Infrastructure`, plus `tools/`
- the `game/` dialogue, speech and async path
- the `game/` world, save and input path
- the Python tooling and CI
- the tests, plus the architecture as a whole

No build, test or Godot run was performed during the review.

Every claimed defect was traced again against the source before being filed. Defects that held up are GitHub issues labelled `code-review-1`. This document holds everything else:

- design critique and improvement suggestions;
- defects too minor or too hypothetical to file;
- *suspected* problems that were not proven.

Items marked **(suspected)** need confirmation before anyone acts on them.

## Verified defects filed as issues

| # | Area | Severity | Title |
| --- | --- | --- | --- |
| [#417](https://github.com/hherb/allrpcs/issues/417) | Providers | high | OpenAI Responses stream accumulators reject events that echo the request schema (MaxDepth 8) |
| [#421](https://github.com/hherb/allrpcs/issues/421) | Tests/CI | high | Smoke harness passes hung checks and unhandled exceptions |
| [#420](https://github.com/hherb/allrpcs/issues/420) | Game dialogue | medium | Loading a save does not cancel in-flight participation rounds |
| [#426](https://github.com/hherb/allrpcs/issues/426) | Domain/save | medium | Sabotage cadence miscounts after load (reference-equal IDs) |
| [#427](https://github.com/hherb/allrpcs/issues/427) | Game input | medium | Overlapping modals can leave the Seeker permanently frozen |
| [#428](https://github.com/hherb/allrpcs/issues/428) | Game input | medium | HUD SAVE/LOAD buttons keep focus and block world input |
| [#430](https://github.com/hherb/allrpcs/issues/430) | Speech | medium | Pocket TTS error stalls 2 s and then kills the warm worker |
| [#414](https://github.com/hherb/allrpcs/issues/414) | Tooling | medium | `quality.py --resume-smoke` always rejects a fresh run |
| [#416](https://github.com/hherb/allrpcs/issues/416) | Tooling | medium | Continuation pruning guard bypassed by committing |
| [#418](https://github.com/hherb/allrpcs/issues/418) | Providers | medium | Anthropic and loopback requests drop prior dialogue lines |
| [#422](https://github.com/hherb/allrpcs/issues/422) | Tests | medium | Four audio Main checks are never launched |
| [#423](https://github.com/hherb/allrpcs/issues/423) | Architecture | medium | Architecture test gaps; SpeechServiceProbe outside the solution |
| [#419](https://github.com/hherb/allrpcs/issues/419) | Providers | low | Stream document caps reject in-budget replies |
| [#424](https://github.com/hherb/allrpcs/issues/424) | Audio | low | Ducking envelope holds the historical peak |
| [#425](https://github.com/hherb/allrpcs/issues/425) | Dialogue | low | Context assembly throws on domain-valid volume |
| [#429](https://github.com/hherb/allrpcs/issues/429) | Game save | low | Load leaves stale perception labels and modals |
| [#431](https://github.com/hherb/allrpcs/issues/431) | Speech | low | Mic capture treats `finished` as failure at some sample rates |
| [#415](https://github.com/hherb/allrpcs/issues/415) | Tooling | low | `native_shutdown()` retries checks that printed `FAIL:` |

Two cross-cutting causes recur:

- **Load/restore does not reach every owner** (#420, #426, #429).
- **The tests use synthetic fixtures that never resemble real provider output or real engine failures** (#417, #421).

Both are addressed in the recommendations below.

---

## 1. Architecture

### 1.1 Most complexity lives in the untestable Godot layer
`game/` holds 838 `.cs` files and about 135.7k lines, more than all of `src/`: 1,166 files and about 62k lines (Domain 25.6k, Application 16.5k, Infrastructure 19.8k).

- About 82k of the `game/` lines (60%) are smoke checks and proofs.
- The remaining ~53.6k lines include 148 `*Coordinator` and 134 `*Controller` nodes. They hold real orchestration: reservation, recovery, binding, admission, turn state machines.

The stated rule "keep game rules out of `game/`" is therefore not met in practice. That logic can only be exercised by booting Godot, which is exactly where the smoke harness is weakest (#421).

**Recommendation:**
- Move coordinator state machines into `src/Application` as plain classes driven by an explicit `Tick`/event API, leaving thin Node adapters in `game/`.
- Replace most `*SmokeCheck` nodes with xUnit tests.
- Keep about 20 genuine engine checks: navigation, collision, rendering, input routing, audio buses.

### 1.2 One pipeline duplicated per decision type
Each LLM decision type is implemented as a long chain of near-identical types:

- **Per decision type:** `TargetSabotageDelay` alone spans about 112 `.cs` files (45 in `src`, 31 test, 36 `game`). Sabotage overall is 403 `game` files (56.5k lines, 42% of `game/`), 227 `src` files and 165 test files.
- **OpenAI per-decision files:** `src/Infrastructure/Characters` holds 50 of them (5,064 lines). That is 10 decision types × {Model, BindingFactory, RequestSerializer, SseCompletion, StreamAccumulator}. The SseCompletion and BindingFactory files have identical line counts across all 10, and the accumulators differ only in message strings.
- **Supporting files:** 30 more files (3.1k lines) of per-type Deserializer, SchemaWriter and RequestContent.
- **Byte-identical copies:** `TryReadTokenCount`, `Terminate`, `OpenAiCredentialPolicy`/`AnthropicCredentialPolicy`, and the macOS/Linux `ReadBoundedAsync`.
- **In Application:** `HandToolTapController`, `CaskCoverHandlingController` and `StreamWaterContactController` differ only in site checks and cooldown. The six `OpeningRegion*RoutePolicy` files each repeat `IsInside`, `CrossesHorizontalWall` and `CrossesVerticalWall`. The 11 `*ConsiderationController` + `*ModelBinding` pairs are near-identical.

The cost is concrete. The depth-cap defect in #417 exists in **12 copies**, and any fix must be applied to every one.

**Recommendation:**
- Introduce one generic `StructuredDecisionPipeline<TRequest, TDecision>` (consideration, task, resolution) parameterised by a schema descriptor and a validator.
- Introduce one `OpenAiResponsesStructuredClient<TRequest, TDecision>` plus one shared HTTP-send and failure-mapping helper for all three providers.
- A realistic target is replacing about 6–7k lines of adapter code with under 1k.
- Do this before adding further decision types.

### 1.3 The shipping scene carries the test harness
- `Main.tscn` is 244 KB with 976 nodes. 213 of them (22%) are always-instantiated Check/Proof scripts.
- `export_presets.cfg` uses `exclude_filter=""` on all six presets, so exports ship every check and proof scene.
- Test-only environment switches stay live in exported builds: `ALLRPCS_OPENAI_CREDENTIAL_TEST_STORE` (`OpenAiCredentialController.cs:9-19`), `ALLRPCS_CHARACTER_MODEL_SETTINGS_TEST_STORAGE` and `ALLRPCS_ORDINARY_PARTICIPATION_SMOKE_MODEL`.
- `OllamaParticipationHttpHandler.cs` is a test double in the production assembly, and it mutates a non-thread-safe `List` from `SendAsync`.

**Recommendation:**
- Attach checks dynamically from one `SmokeCheckHost` that instantiates only the requested check.
- Exclude `*Check*` and `*Proof*` from the export presets.
- Ignore test-store variables unless `OS.IsDebugBuild()`.

### 1.4 `Main.cs` is an orchestrator, not a composition root
`game/Main.cs` is 1,100 lines with about 70 fields. It mixes:

- wiring for about 15 controllers;
- the per-frame independent-task tick;
- the LLM turn state machine;
- dialogue hearing and submission;
- focused-search input;
- healer commands;
- HUD text.

**Recommendation:** extract a `ConversationTurnCoordinator` (`Main.cs:640-854` plus the cancel block at `:862-866`) and a `FocusedSearchInputCoordinator` (`:868-931`, `:957-1073`). The conversation coordinator should own load and cancel handling, which removes the root cause of #420.

### 1.5 Correctness depends on undocumented scene-tree order
Several same-frame interactions only work because of child order in `Main.tscn`:

- Main processes before its children.
- The round controller processes before the coordinator.
- `TemporaryCharacterTurnResponseSpeechRecorder` (node 482) processes before `TemporaryCharacterTurnCompletionController` (node 485). If those two swapped, a turn could stay in Speaking forever.
- `Main.cs:295` uses a magic index: `MoveChild(examinationAction, GetChildCount() - 2)`.

**Recommendation:** set `ProcessPriority` explicitly, or have one owner call each step in sequence. Add a check that asserts the order.

### 1.6 Load/restore needs one owner
`SaveSnapshotRestorationController` raises `PresentationReset`, and each subscriber decides on its own what to do. Many stateful owners never subscribe:

- the participation round, coordinator, budget and generation flow (#420);
- the perception indicators and the reading modals (#429);
- the transcript;
- the sabotage placements.

Restore is also not atomic. Memory, first patient, journal, map and routes are installed *before* the discovery and world-event installs, which can still fail (`SaveSnapshotRestorationController.cs:184-218`). `SeekerMapController.InstallRestored` and `RouteProgress.InstallRestored` can also throw part-way through.

**Recommendation:**
- Make restore a two-phase operation: validate and build everything, then commit.
- Give it an explicit registry of `IRestoreParticipant` owners, so that adding stateful state without restore handling becomes a visible decision.
- Decide and document what a save covers.
  - Not covered today: Seeker and Healer positions, Healer command mode, Seeker customization, the transcript, and object-sabotage placements (`CharacterObjectSabotageMoveController.Placements` and siblings are neither captured nor reset) **(suspected impact)**.
  - Uncovered customization lets a restarted session pick a different background after loading.

### 1.7 Identity semantics are mixed
`src/Domain` has 301 `ReferenceEquals` uses, about 40 of them on value-equal ID records. Examples: `DiseaseProgressionPolicy`, the `*DecisionPolicy` files, `ExperimentRecordingPolicy:28` and `DiseaseProtectionApplicationPolicy:23`. Correctness then depends on who constructed an ID, which is the root cause of #426.

In addition, 19 `record` types hold `IReadOnlyList` members, for example `CharacterEpisodicMemory`, `CharacterTurnResponse` and `CharacterBeliefAssessment`. Their synthesized equality compares those lists by reference, so two identical values are unequal.

**Recommendation:**
- Compare identifiers by value everywhere, and keep `ReferenceEquals` for authored definition objects only.
- Turn list-holding records into sealed classes, or override equality with `SequenceEqual`.

### 1.8 Repository shape and process overhead
**Flat `game/` directory.**
- Git tracks 1,076 top-level files there: 838 `.cs`, 135 `.tscn`, 82 `.uid`, 17 `.gd`. Split them by feature: `Dialogue/`, `Sabotage/`, `Audio/`, `World/`, `Checks/`.
- Some file names do not match their class. `CharacterSabotageTurnOpportunityRecoverySmokeCheck.cs` contains `CharacterSabotageTurnOpportunityControllerSmokeCheck`.

**`Temporary*` scaffolding.**
- 148 files and 21.5k lines, of which 76 files are production code.
- Anything that has its own smoke check and scene is no longer temporary. Either set a removal milestone or rename it.

**Documentation size.**
- `README.md` is 444 KB with only 21 `##` sections. "Character model boundaries" alone is 196 KB.
- Move boundary prose into per-feature `docs/` files and keep README to setup, commands and a map.
- `CONTINUATION.json` (92 KB) and the evidence packages are reasonable as audit trails, but they dominate the repository.

**Repository weight.**
- The evidence directories hold about 14k of the 19.2k tracked files. `audio-direction/` alone is about 1.7 GB, and `.git` is 2.4 GB.
- Consider Git LFS or a separate evidence repository pinned by hash.

**Dead and unwired code.**
- 30 Domain types have no production reference. Examples: the disease progression, exposure, cure and protection transitions; the inventory Give/Take/Use policies; `DiscoveryScenarioValidationPolicy`; `CharacterRecentConversationWindowAppendTransition`; `CharacterTurnSpeakerSchedulingPolicy`.
- `IInvestigationHypothesisTransport` has no production implementation.
- There are two separate inventory models, `CharacterInventory` and `InventoryOwnershipSnapshot`, with no consistency check between them.
- Tested-but-unwired code hides integration bugs like #426. Wire it up, or mark it as staged.

---

## 2. Tests and CI

### 2.1 What is in good shape
- The xUnit suites are disciplined. A scan of 4,067 test methods in 875 files found no `async void`, no `Skip`, no tautological asserts and no missing `[Fact]`.
- Name-level coverage of public `src/` types is about 99%: only 9 of 1,173 are never named in a test.

### 2.2 Recommendations, in priority order
1. **Make smoke launches prove completion.** See #421.
2. **Add a registration test.** Every `ALLRPCS_*_CHECK` gate and every `*Check.tscn`/`*Proof.tscn` must be launched somewhere or listed in an explicit manual-only allowlist (see #422). Today 32 Check/Proof scenes have no launcher.
   - `ALLRPCS_ORDINARY_MODEL_TIMEOUT_CHECK` runs with `--quit-after 100000000`, which is effectively unbounded.
3. **Use recorded real-provider fixtures.** Each provider's SSE/NDJSON tests should include at least one recorded real stream with full `response` objects. The minimal synthetic events (`{"type":"response.created"}`) are why #417 went unnoticed.
4. **CI covers less than the local quality gate.** Add a cheap Python-tooling job:
   - `actionlint` (`scripts/validate-workflows.sh`)
   - `check-continuation.py --self-test`
   - `scripts/test-player-speech-worker.py`
   - `py_compile` of the live scripts
   - `quality.py --self-test` (needs `fetch-depth: 0`)
5. **Run tests via the solution.** CI lists the test projects one by one, so a new test project would be silently skipped. Use `dotnet test Allrpcs.sln --no-build`.
6. **Fix the fail-open macOS signature check.** It passes if `Allrpcs.app` is missing or renamed, because `codesign` errors and the `if` falls through (`ci.yml:138-142`). Assert that the bundle exists first.
7. **Add `concurrency:` to CI** so superseded pull-request runs stop consuming macOS and Windows runners.
8. **Move package pins.** xunit 2.4.2 and Test SDK 17.6.0 are old and duplicated across four csproj files. Move them to `Directory.Packages.props`. Coverlet is referenced, but CI never collects coverage.
9. **Add missing tests:**
   - ducking with mixed foregrounds (#424);
   - context-assembly overflow (#425);
   - cadence after a save/load round trip (#426);
   - a Main check that loads during a pending participation round (#420);
   - `CameraFramingCalculator` with expand-aspect viewports (§3.4);
   - synthetic tests for `recognizer()`/`local_model()` in `player-speech-worker.py`, whose docstring claims model-boundary coverage it does not have.
10. **CI cost.** 293 separate Godot launches (91 of them standalone scenes) dominate CI time. Point 1.1, moving coordinators into xUnit, plus batching the remaining engine checks per scene would cut it by about an order of magnitude.

---

## 3. Component findings not filed as issues

### 3.1 LLM provider adapters
- **(suspected, high impact) macOS Keychain save probably never works.**
  - `MacOsOpenAiCredentialStore.cs:50-57` runs `security add-generic-password … -w` with `-w` last, which makes `security` prompt for the password *twice*.
  - `MacOsKeychainProcessRunner.cs:142-147` writes the key once and then closes stdin. The retype prompt reads EOF and the save fails; launched from a terminal, it may hang on `/dev/tty` instead.
  - This was not run during review, to avoid writing to the login keychain. Confirm it manually.
  - Fix: write `key\nkey\n`, or better, use `SecItemAdd`/`SecItemCopyMatching` via P/Invoke.
- **(suspected) OpenAI strict mode may reject `minLength`/`maxLength`.** The character-turn schema emits these with `includeBounds: true` (`CharacterTurnResponseJsonSchemaWriter.cs:234-237,285-289`). Strict structured outputs has historically rejected them with a 400, which maps to ProviderRejected. Verify together with #417.
- **(suspected) Ollama usage parsing.** It requires `prompt_eval_count` whenever `eval_count` is present (`OllamaCharacterModelStreamAccumulator.cs:165-174`), but Ollama marks that field `omitempty`.
  - More generally, every accumulator turns malformed *usage telemetry* into `InvalidResponse` and throws away a valid answer. Treat bad usage as unknown instead.
- **Timeouts.**
  - The game's shared `HttpClient` uses `Timeout = InfiniteTimeSpan` (`TemporaryCharacterModelCompositionController.cs:83-86`).
  - The readers buffer to end-of-stream with no idle timeout.
  - The ten sabotage decision controllers and the connection testers impose no deadline, and `Main.cs` never calls their `TryCancel`. A server that accepts the connection and then stalls hangs those decisions forever.
  - Add a per-request deadline and an idle-read timeout in the adapters.
- **Whole-stream buffering.** The readers collect up to 1,024 documents × 64k chars before processing anything, so about 130 MB from a hostile endpoint. Process incrementally and stop at the terminal event; this also resolves #419.
- **Redirects leak the Anthropic key.** .NET strips `Authorization` on a cross-host redirect but not `x-api-key` (`AnthropicCharacterModel.cs:93`). Use `AllowAutoRedirect = false` for provider clients.
- **Rate limiting.** `RateLimited`/`Unavailable` are retried immediately, with no backoff and ignoring `Retry-After` (`CharacterModelRetryPolicy.cs:37`).
- **Schema/domain mismatches.**
  - Whitespace-only `speech` passes every schema but is rejected by `DialogueUtterance`, giving a non-retryable `InvalidResponse`.
  - The Anthropic and Ollama schemas carry no bounds, so over-long speech passes the schema and then fails in the domain.
- **Smaller adapter issues:**
  - Streamed `output_text.delta` text is counted but never compared with `output_text.done`.
  - `OpenAiCharacterTurnParticipationModel.cs:68` always posts to `/responses`, while turns switch to Chat Completions on loopback, so a loopback-only server serves turns but fails participation.
  - The SSE reader rejects `id:`/`retry:` fields, CR-only line endings, a BOM, and a final frame without a trailing blank line. That is acceptable today, but brittle behind proxies.
  - Sabotage bindings are hard-coded to OpenAI (e.g. `CharacterTargetSabotageDelayModelBinding.cs:15`). With Anthropic or Ollama selected, no sabotage decisions occur. Document this or generalise it.
- **Confirmed fine:**
  - UTF-8 sequences split across reads decode correctly.
  - The process runners use `ArgumentList` with fixed paths and no shell, so there is no injection.
  - Secrets are zeroed and never appear in exceptions.
  - The Ollama endpoint is restricted to numeric loopback http with an explicit port.
  - `CharacterModelHttpFailurePolicy` is sensible.

### 3.2 Game dialogue, speech and async
- **Godot threading is sound.**
  - There is no `async void`, and no node access from thread-pool continuations: tasks are polled via `IsCompleted` in `_Process`, and stream updates cross threads through a `ConcurrentQueue`.
  - Cancellation sources are cancelled and disposed in `_ExitTree`, and handlers are unsubscribed.
  - The microphone stops on focus loss, panel close, language change and new delivery.
- **A `throw` inside `_Process` is not fail-fast in Godot.** Godot logs it and runs the same path again every frame.
  - `ProcessDialogueSubmission` sets `lastProcessedDialogueDelivery` only at the end (`Main.cs:819`). If anything after `TryOccur` throws, the same delivery is recorded into history and transcript every frame.
  - Set the processed marker before side effects, as `Main.cs:754` already does.
- **(suspected) Stale terminal results.** The failure and cancel path at `Main.cs:735-741` cancels any AwaitingResponse or Speaking activity without checking that the termination belongs to `activity.Request`. A slow-to-cancel old generation could cancel the next turn.
- **(suspected) An absent healer can hear.** While away on the independent task the healer is hidden, but hearing (`Main.cs:793`, `TemporaryCharacterTurnResponseSpeechRecorder.cs:112`) still uses `healer.Position`. `HerbExaminationActionController.cs:126` does check `healer.Visible`.
- **Silent drops.**
  - A second utterance while a round or turn is pending is discarded, yet the panel says "ACCEPTED".
  - With no provider configured, the offline participation model always faults and the round resets silently (`OrdinaryPlayerDialogueParticipationCoordinator.cs:206`).
  - Show a busy or provider-unavailable status, or queue the utterance.
- **(suspected, theoretical) Speech-session race.** `PlayerSpeechController.cs:161-175` drains events and *then* reads `Status`. If the final event and `Completed` both land in between, the final text is dropped. `DialogueSpeechController.cs:140-154` has the same shape. Snapshot the status first, then drain.
- **Workers on quit.** The `Shutdown` paths are awaited only by smoke checks; real quit relies on the workers exiting on stdin EOF. Handle `NOTIFICATION_WM_CLOSE_REQUEST` with a bounded wait.
- **The transcript grows without bound.**
  - `CharacterTurnTranscriptAppendTransition` copies the whole list per append, which is O(n²) per session.
  - The transcript is never reset on restore. `WorldEventLogSnapshot` has the same unbounded copy-on-append.
- **Smaller items:**
  - `SpaceHoldSpeechController.cs:157` defers a closure without an instance-validity check.
  - `TemporaryCharacterModelCompositionController.TryReplaceCurrentBindings` (`:166-183`) is not atomic.
  - `TemporaryCharacterTurnResponseLabel` keeps typing out a cancelled response.

### 3.3 Game world, save and input
- **Guidance text.**
  - `OpeningRegionFirstPatientStabilizationTestimonyCoordinator.cs:140,161` writes `RecoveryStatus.Text` directly instead of calling `SetGuidance`. "press K" is never rewritten after remapping, and a later controls Apply restores an older guidance source.
  - The guidance regex in `SaveLoadInputController.cs:15` only rewrites "press X", so "close it with M" goes stale after remapping.
- **(suspected, unmeasured) Navigation polygon scanned every frame.** The cask-cover, stream-water and hand-tool coordinators call `GetVertices()` and `GetPolygon(i)` over the whole navigation polygon every frame. With 8 px cells that is roughly 10k polygons and about 3×10⁴ interop calls plus allocations per frame. Cache the bindings and invalidate them on navigation change. Profile before and after.
- **Seeker animation.**
  - `Seeker.cs:49` animates from the requested velocity, so walking into a wall plays the walk cycle in place. Use the post-slide `GetRealVelocity()`.
  - Suspending physics leaves the looping walk clip running. Call `PresentMovement(Vector2.Zero)` when suspending.
- **Controls document versioning.** `ControlPreferences` requires exactly the 29 catalogue rows. Adding any action resets every user's bindings to defaults ("Unreadable controls"), and the invalid file stays on disk, so the warning repeats until the user saves. Add a migration that fills in missing actions from the defaults.
- **Cross-context conflicts.** These are not checked (`ControlPreferences.cs:52`). A gameplay key can be bound to Space or Enter (`ui_accept`), and both fire while a button has focus.
- **Smaller items:**
  - `Healer.cs:172` re-targets navigation every physics frame while following.
  - The credits URL regex in `MaterialAudioCreditsPanel.cs:60` swallows trailing `)`, `]` and `.`.
- **Confirmed fine:**
  - all `res://` loads resolve;
  - bus indices are checked;
  - all 29 actions exist;
  - deadzones are bounded;
  - movement uses `MoveAndSlide` in `_PhysicsProcess`.

### 3.4 Domain and Application
- **Domain is very defensive.** Every collection is copied into `AsReadOnly()`, and the 28 closed-set "enum" classes are consistent. One exception: `FirstPatientRecoveryStage` has value equality but no `==` overload and no `All`.
- **Validation gaps (low; reachable only through an edited save):**
  - `FocusedSearchActivitySnapshot` (`:28`) and `DiscoverySaveStateRestorationPolicy` accept two active searches for one searcher. The start policy forbids this, and `CharacterIndependentTaskActivitySnapshot` enforces the equivalent rule in its constructor.
  - `InvestigationRouteSaveState` does not check that the consumed key is accepted, and `MapSaveState` has no de-duplication or note-length bound **(suspected; may be covered in Application)**.
- **Reachability validator.** `DiscoveryScenarioReachabilityValidationPolicy.cs:74-82` requires an *initial* `Noticed` state for reinterpretation. It therefore reports the passive-noticing → reinterpretation chain as unreachable even though it works at runtime. The policy has no production caller.
- **Stabilization ignores current disease state.**
  - `StabilizingRemedyAdministrationPolicy` and `FirstPatientAdministrationOccurrenceTransition` read `context.Scenario.DiseaseTruth`, never the current truth. Re-treatment is blocked only by a game-layer guard.
  - `FirstPatientSaveStateRestorationTransition.RebindDiseaseTruth` (`:58-62`) rejects truths that the (unwired) cure and exposure transitions would produce.
- **Relationships freeze after 64 changes.** `CharacterRelationshipChangePolicy.cs:16` rejects every change once 64 reasons are stored, and nothing evicts old ones. Evict the oldest, as the conversation window does.
- **Summary snapshot overflow.** `CharacterConversationSummaryRecordingTransition` has no capacity check, so the snapshot constructor throws once the summary snapshot is full.
- **(suspected) Visibility ordering.**
  - `DiscoveryEvidenceProjection.cs:9-12` ranks `Altered`/`Collected` above `Understood`.
  - `FocusedSearchOutcomeChangePolicy` allows an outcome to downgrade visibility.
  - Confirm both are intended.
- **Camera framing.**
  - `CameraFramingCalculator.Create` runs once (`FollowCamera.cs:73`) with `stretch/aspect="expand"`. A tall or portrait window (e.g. 9:16 gives about 2,275 px of viewport height against the 1,800 px world) throws at startup.
  - Resizes are never re-validated, and `MinimumZoom`/`MaximumZoom` are validated but never applied.
  - Clamp to an effective minimum zoom and recompute on `size_changed`.
- **Speech and generation sessions:**
  - `SpeechPlaybackSession.cs:95` treats every `OperationCanceledException` as Cancelled. Filter on its own token, as `PlayerSpeechSession.cs:58` does.
  - `PlayerSpeechSession` disposes the generation's cancellation sources when `RecognizeAsync` returns. Document on `ISpeechRecognizer` that the audio pump must be finished by then.
  - `EditableSpeechDraft.Apply` mutates state before the 2,000-char check can throw.
  - `CharacterModelGenerationController.RunAttemptAsync` converts only `OperationCanceledException` into a terminal outcome, contrary to its "provider-neutral" contract.
  - `CharacterModelStreamAppendTransition` revalidates the whole snapshot on every delta, which is O(n²).
- **Content and style drift.**
  - `src/Domain/Actions/*` hard-codes scene data (`"village-water-cask"`, `Vector2(1700,900)` compared with exact float equality).
  - 12 `Representative*Source` files put authored content in the rules layer.
  - `OpeningRegionClosedForestPathLayout.cs:84-85` divides by `|direction.Y|` without a guard.
- **Boilerplate.**
  - 91 files hand-roll the same null, duplicate and limit loop; a `BoundedUniqueList.Create(...)` helper would remove most of it.
  - `AuthoredCharacterText`, `AuthoredLocationText` and `AuthoredWorldObjectText` are identical, and `JournalSaveText` throws a different exception type.
  - `Copy<T>` is triplicated across the save states.
- **Doc and naming errors:**
  - `CharacterBeliefAssessment.ConfidenceBasisPoints` says "zero to one thousand", but the maximum is 10,000.
  - `AudioChannelSetting` reports an invalid category as `nameof(gain)`.
  - `PositionalCueHistory.Record` lacks the `long.MaxValue` guard its siblings have.
  - `ControlBindingEdit.Remove` lacks a null check.

### 3.5 Infrastructure (persistence and speech)
- **Persistence is solid.** Writes use a temp file in the same directory and then rename (atomic on Unix). Writer and reader field names agree (checked by script), and nothing in scope serializes culture-sensitive data.
- **Durability:**
  - No store fsyncs the directory after the rename. A power loss can revert a save to its previous version, but cannot corrupt it.
  - `FileSaveDocumentStore.Load` (`:87`) and `FileAudioPreferencesStore.Load` (`:29`) read with no size limit. Reuse the bounded read in `FileControlPreferencesStore`.
- **Save migrations.** `SaveSnapshotJsonDeserializer.DeserializeCurrent` runs migrations outside the try/catch that maps exceptions to `ArgumentException` (`:58-91`). A future migration throwing anything else would crash load instead of failing it.
- **Old-version serialization.** `CharacterMemoryJsonWriter.cs:111-115` silently drops delivery modes for v2/v3 snapshots. That is harmless today, but serializing a non-current version should be rejected outside tests.
- **Speech IPC robustness:**
  - `MoonshineSpeechRecognizer.cs:101` leaves the real read running after a `WaitAsync` timeout. Use a linked token source with `CancelAfter`, as `PocketSpeechSynthesizer` does.
  - **(suspected)** The 10 s ready deadline may be too tight for importing onnxruntime, hashing 8 model files and loading the model. A mic-permission prompt longer than 10 s would also time out the recognizer read.
  - **(suspected)** `PlayerSpeechWorker.DisposeAsync` and `PocketSpeechWorkerProcess.DisposeAsync` await process exit and the stderr drain with no timeout.
  - **(suspected)** `pocket-speech-worker.py` does not redirect stdout while loading models, unlike the player worker. A library print there would corrupt the first protocol line.
  - Protocol bounds disagree:
    - C# sends Pocket requests with no length check, while Python silently exits on lines over 16k.
    - Python bounds recognizer text in code points, C# in UTF-16 units.
- **Small cleanups:**
  - a no-op `catch { throw; }` at `MoonshineSpeechRecognizer.cs:64`;
  - `PortAudioMicrophoneCapture`'s static `Gate` blocks all captures if an enumerator is not disposed;
  - `SpeechServiceProbe` calls `Process.GetProcessById` per line, which throws if the worker has just exited.

### 3.6 Python tooling
- **Retained reports lose their failure state on early errors.** In `quality.py`, `Run(directory)` overwrites `summary.json` before the resumed work starts. An early failure leaves a report without `failed_stage`, which blocks every later resume. Keep the prior report until the resume succeeds.
- **No per-stage timeout.** `Run.stage` has none, so a hung Godot or dotnet process stalls the gate forever.
- **Machine-specific paths:**
  - `/private/tmp/allrpcs-*-missing.json` (`quality.py:947-1027`) relies on a file not existing in a world-writable directory. Use a fresh `TemporaryDirectory`.
  - `/Users/hherb/Library/...` (`:134-135`) ties resume eligibility to one machine.
- **Dead code and CLI shape.** `finish_foliage`, `finish_rain` and `finish_cooling_uids` are pinned to old hashes and are now dead. The `--finish-*` flags should be mutually exclusive.
- **Masked exceptions.** `main()`'s `finally` runs `git ls-files` unguarded, which can mask the original exception.
- **Archive attributes.** `snapshot()` uses `git archive`, which honours `export-ignore`/`export-subst` attributes from the archived tree. A future attribute would silently alter frozen snapshots.
- **Worker request validation.** In `pocket-speech-worker.py:124,159`, a `language`/`voiceProfile` sent as a list raises `TypeError: unhashable`, and deep nesting raises `RecursionError`. Both kill the worker instead of returning `invalid_request`. They fail closed with no privacy leak, but should be validated with `isinstance` checks.
- **(suspected) Unsynchronized output.** `player-speech-worker.py`'s `emit` has no lock, unlike the Pocket worker's. If moonshine calls back from its own thread, protocol lines could interleave.
- **Shell scripts.** They use `set -eu` and quote correctly. `smoke-godot.sh` relies only on exit codes; see #421.
- **For future validators (frozen ones untouched):** keep using `tarfile` `filter='data'`, no `shell=True`, and `require()`. Consider one shared helper module instead of per-file copies of `sha`, `require` and the snapshot code.

---

## 4. Suggested order of work
1. **#417:** verify against a live OpenAI call, and fix all 12 accumulator copies with one shared helper (start of §1.2).
2. **#421 and #422:** until smoke launches prove completion, every other engine-level guarantee is weaker than it looks.
3. **Restore ownership:** #420, #426, #429, §1.6.
4. **Input regressions:** #427, #428.
5. **Speech:** #430, #431. Also confirm the suspected Keychain save failure (§3.1).
6. **Structural work:** move coordinators into Application and consolidate the decision pipeline (§1.1, §1.2). Doing this before adding more decision types prevents the duplication from growing further.
