# Code Health and Playable Vertical Slice — Program Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Turn a well-tested but largely unplayable codebase into a vertical slice that a human can
play from a fresh start to an ending. Fix the verified defects from code review #1 along the way,
and keep the code from getting harder to change.

**Architecture:** Five phases, run in order. Each phase is one ledger slice with a finite exit
condition. Phase 0 (governance) and Phase 1 (a test safety net you can trust) are detailed below
task by task. Phases 2–5 are scoped here with exit criteria; each gets its own detailed plan file
when it becomes the active slice, written against the code as it stands then.

**Tech Stack:** .NET 8 / C# (xUnit), Godot 4.6.3 .NET, POSIX sh, Python ≥ 3.12, GitHub Actions.

**Spec:** [CLAUDE_CODE_REVIEW_1.md](../../../CLAUDE_CODE_REVIEW_1.md) (defects, critique and
suggested order) and [ROADMAP.md § First vertical-slice definition](../../../ROADMAP.md) (what
"functional game" means).

## Where we are (measured 2026-10-17 at `18c6ed91`)

- **The engineering baseline is green.** `dotnet build` produces 0 warnings, and all 5,657 xUnit
  tests pass in about 3 s.
- **18 verified defects are open** (#414–#431, label `code-review-1`).
- **The vertical slice is not playable end to end.** Status of each step, from reading the code;
  the game was not run:

| Slice step | Status | Main gap |
| --- | --- | --- |
| 1 Create Seeker, choose background | optional, no effect | Hidden behind U, not shown at start; nothing found that gates on background |
| 2 Converse with healer | works only with Ollama or OpenAI configured | Fails silently otherwise |
| 3 Explore six areas | playable | Greybox |
| 4 Discover evidence | partial | Several evidence states auto-resolve in `Main._Ready` (`Main.cs:315-319`); outcomes come from a `Temporary*` source |
| 5 Conditional task in language | partial | One hard-wired target; the conditional path is enabled only by smoke checks (`Main.cs:725`) |
| 6 Form and test hypotheses | **not reachable** | No game wiring for `InvestigationHypothesis*`; the journal is pre-filled |
| 7 Identify ingredients, prepare remedy | preparation only | Fixed recipe from `TemporaryFirstPatientTreatmentContextSource` |
| 8 Stabilize patient | cannot fail | Disease progression, exposure and cure transitions are unwired |
| 9 Save/reopen with memory | **memory never written** | `CharacterMemoryController` is only `Install`ed on restore; transcript, customization and positions are not saved |

Also missing: the hermits, any visible enemy agent, and an ending.

- **Process load dominates the work.** In the last 300 commits, evidence directories
  (`*-direction/`) took about 14,400 file touches, `game/` 2,726, and `src/` plus `tests/` 386.

## Owner decisions this plan needs

Each decision has a recommendation. Task 0.1 records the owner's answers in the ledger before any
code changes.

- **D1 — Pause the microphone trial.** Move the active slice `phase10-player-microphone-qualification`
  to `deferred_slices` unchanged, and make this program the active work. *Recommended.* The trial
  is owner-operated and can resume later with nothing lost.
- **D2 — Lighter evidence regime for this program.** Evidence for a code fix is the GitHub issue,
  its tests and the commit (`Fixes #n`). Do not create new `*-direction/` evidence packages for code
  fixes. The full `./quality.sh` runs once per phase exit, not per commit. Between phase exits,
  run `dotnet format/build/test` plus the affected smoke launches. *Recommended.* As written,
  CLAUDE.md requires the full gate for every product or CI change, which makes a phase of about 20
  small fixes impractical.
- **D3 — Retire the uncommitted `game/project.godot` exception.** Commit the editor rewrite. Its
  only semantic change drops `window/size/resizable=true`, which is already the default. Make
  `quality.py` read the pinned bytes from git rather than the working tree. *Recommended.* As
  things stand, any engine-config change (autoload, input action) trips
  `Recorded editor exception changed` (`quality.py:210`). Phase 1 needs an autoload.
- **D4 — Slice scope.** "Functional" means a new player can complete slice steps 1–9 against a
  local Ollama model (the owner's `gemma4:e4b`), and the game reaches an explicit ending. Defer the
  two hermits and a visible enemy agent. *Recommended.*
- **D5 — Freeze sabotage.** Add no new sabotage decision types or coordinators until the Phase 3
  pipeline consolidation lands. Sabotage is already 42% of `game/`, and its effects are barely
  visible to the player. *Recommended.*
- **D6 — Defer the evidence-repository split** (Git LFS, or a separate repository pinned by hash).
  Revisit after Phase 4.

## Global Constraints

- `TreatWarningsAsErrors`; `dotnet format Allrpcs.sln --no-restore --verify-no-changes` must pass.
- No Godot types in `src/`; reference directions as enforced by `tests/Architecture.Tests`.
- Never alter frozen art/audio evidence, acceptance hashes, historical validators, or
  `scripts/quality.sh`.
- Never prune closed ledger entries. After a ledger edit, run
  `python3 scripts/check-continuation.py --sync`, then the checker, then `--self-test`.
- Preserve `.DS_Store` and unrelated concurrent work under `speech-direction/`.
- Godot 4.6.3 .NET, .NET 8 SDK (`global.json`), Python ≥ 3.12. Change these pins together.
- File-scoped namespaces matching folders, braces on new lines, `System` usings first, LF line
  endings, 4-space indent (2 for csproj/json/md/tscn). Tests use xUnit.
- Commit and push when a task is finished (standing grant in `HANDOVER.md`). Branch first if the
  owner asks for PRs.

## Review Focus

These are the failure modes a player is most likely to hit that no current test exercises, most
likely first. Each one gets a pinning test in the sub-plan of the phase named in brackets.

1. Loading a save while an LLM reply is pending shows the stale reply after the load (#420)
   [Phase 2]. Expected: the reply is discarded and the dialogue panel shows idle.
2. No provider configured, or Ollama not running: talking to the healer silently does nothing
   (§3.2 "Silent drops") [Phase 2]. Expected: a visible "provider unavailable" status, and the
   game stays saveable.
3. Clicking HUD SAVE/LOAD, or opening two modals, permanently freezes the Seeker (#427, #428)
   [Phase 2]. Expected: world input resumes once every modal is closed.
4. A provider stalls mid-stream and the healer's decision hangs forever (§3.1 Timeouts)
   [Phase 2]. Expected: a deadline produces a retryable failure and a visible status.
5. Resizing to a tall or portrait window throws in `CameraFramingCalculator` (§3.4) [Phase 2].
   Expected: the zoom clamps and the game keeps running.

---

## Phase 0 — Governance (no product code)

### Task 0.1: Record the program in the ledger and roadmap

**Files:**
- Modify: `CONTINUATION.json` (`active_slice`, `deferred_slices`, `reopenings`)
- Modify: `ROADMAP.md` (new section)
- Modify: `HANDOVER.md` (regenerated active block; one-line checkpoint note)
- Add to git: `CLAUDE_CODE_REVIEW_1.md`, this plan

- [ ] **Step 1:** Wait for the owner's answers to D1–D6. Quote the owner's approval message
  verbatim; it becomes the `owner_statement`.
- [ ] **Step 2:** In `ROADMAP.md`, add `## Code health and playable vertical slice (owner request 2026-10-17)`
  directly after `## Phase 1 — Remaining exploration work`. Give it one unchecked item per phase,
  exactly these lines:
  - `- [ ] Phase 1: smoke launches prove completion and every check is registered.`
  - `- [ ] Phase 2: player-facing code-review-1 defects are fixed with pinning tests.`
  - `- [ ] Phase 3: one structured-decision pipeline and a thin Main composition root.`
  - `- [ ] Phase 4: a new player completes slice steps 1–9 and reaches an ending on local Ollama.`
  - `- [ ] Phase 5: game/ is organised by feature and Temporary* scaffolding is resolved.`
- [ ] **Step 3:** Move the current `active_slice` object unchanged into `deferred_slices`, with
  `status: "deferred"` and `authority_status: "owner_paused_for_code_health_program_2026-10-17"`.
- [ ] **Step 4:** Append a `reopenings` row: `id: "owner-code-review-1-2026-10-17"`, the
  `owner_statement` from Step 1, evidence `CLAUDE_CODE_REVIEW_1.md` with its sha256, and
  `scope_ids`/`workstream_ids` listing the closed scopes whose code Phase 1 edits. Find them by
  grepping `closed_scopes` for `smoke`, `ci` and `quality`; use empty lists if none match.
- [ ] **Step 5:** Set the new `active_slice`:
  - `id: "code-health-phase1-safety-net"`, `workstream: "code-health-program"`,
    `status: "in_progress"`, `reopening_id: "owner-code-review-1-2026-10-17"`;
  - `roadmap_item`: the exact Phase 1 line from Step 2;
  - `missing_deliverable`: "Smoke launches fail on hang or exception; every check gate and scene
    is launched or allowlisted; complete architecture test; CI runs the solution tests and Python
    tooling.";
  - `completion_condition`: "Issues #421, #422 and #423 are closed by commits with tests, CI is
    green on main, and one full ./quality.sh passes.";
  - `outputs`: the files created in Tasks 1.1–1.5;
  - `boundary`: "Test and CI infrastructure only; no gameplay behaviour changes.".
- [ ] **Step 6:** Run `python3 scripts/check-continuation.py --sync && python3 scripts/check-continuation.py && python3 scripts/check-continuation.py --self-test`.
  Expected: `PASS:` lines and exit 0.
- [ ] **Step 7:** Commit:
  `docs: start code-health program and pause microphone trial`.

### Task 0.2: Retire the `project.godot` editor exception (only if D3 is approved)

**Files:**
- Modify: `game/project.godot` (commit the existing working-tree bytes as they are)
- Modify: `quality.py:205-212` (the `editor` overlay)
- Modify: `HANDOVER.md:140-142`, `CLAUDE.md` (the "Preserve the uncommitted…" bullet)

- [ ] **Step 1:** Confirm `shasum -a 256 game/project.godot` prints `36bfb7c7a54399cc8d27c02b73c7990766d7db44e7452655cb6a496866653fca`.
- [ ] **Step 2:** Commit `game/project.godot` alone: `chore: commit Godot editor rewrite of project.godot`.
  Record the commit hash as `EDITOR_REVISION`.
- [ ] **Step 3:** In `quality.py`, add the constant `EDITOR_REVISION` beside the other `*_REVISION`
  pins. Change the overlay so it reads `git show {EDITOR_REVISION}:game/project.godot`, not the
  working-tree file, and keep the `EDITOR_SHA` comparison on those bytes. Historical snapshot
  replays then stay byte-identical when later tasks add an autoload.
- [ ] **Step 4:** Run `./quality.sh --self-test`. Expected: pass.
- [ ] **Step 5:** Update the HANDOVER and CLAUDE.md lines to say the exception is committed at
  `EDITOR_REVISION` and that `project.godot` may now change normally. Commit:
  `chore: pin historical editor overlay to a committed revision`.

---

## Phase 1 — A test safety net you can trust (#421, #422, #423, CI)

Why first: until a hung or throwing check fails CI, no engine-level guarantee is real. The
refactors in Phases 3 and 5 depend on these checks catching regressions.

### Task 1.1: Complete the architecture test (#423)

**Files:**
- Modify: `tests/Architecture.Tests/ProjectReferenceTests.cs`
- Create: `tests/Architecture.Tests/ProjectSdkAndPackageTests.cs`
- Modify: `tests/Domain.Tests/DomainAssemblyTests.cs`
- Modify: `Allrpcs.sln` (add `tools/SpeechServiceProbe/Allrpcs.SpeechServiceProbe.csproj`)

**Interfaces:**
- Produces: `ProjectReferenceTests.AllowedReferences` covers all 10 tracked `.csproj` files.
  Entries to add: `tools/SpeechServiceProbe/…` → Infrastructure, and
  `tests/Architecture.Tests/…` → none. Task 1.5 relies on the existing `FindRepositoryRoot()`.

- [ ] **Step 1: Write the failing tests.**
  - `ProjectReferenceTests.EveryRepositoryProjectIsInAllowedGraph`: enumerate `*.csproj` under the
    repository root, skipping any path containing a `bin`, `obj`, `artifacts`, `.git` or `.godot`
    segment. Assert the set equals the `AllowedReferences` keys (expect exactly 10).
  - `ProjectReferenceTests.EveryAllowedProjectIsInSolution`: every key appears in `Allrpcs.sln`.
  - `ProjectSdkAndPackageTests.RuleLayerProjectsUsePlainSdk` (Theory over Domain, Application,
    Infrastructure): the root `Sdk` attribute is `"Microsoft.NET.Sdk"`, and there are no
    `PackageReference` or `FrameworkReference` elements.
  - `ProjectSdkAndPackageTests.DirectoryBuildPropsAddsNoReferences`: `Directory.Build.props` has no
    `ProjectReference`, `PackageReference` or `FrameworkReference`.
  - `DomainAssemblyTests.ReferencesNoGodotAssembly`: no name in
    `typeof(DomainAssembly).Assembly.GetReferencedAssemblies()` starts with `"Godot"`.
- [ ] **Step 2:** `dotnet test tests/Architecture.Tests/Allrpcs.Architecture.Tests.csproj`.
  Expected: the two `ProjectReferenceTests` additions FAIL (the probe is missing from the graph and
  the solution); the others pass.
- [ ] **Step 3:** Add the two graph entries, and `dotnet sln Allrpcs.sln add tools/SpeechServiceProbe/Allrpcs.SpeechServiceProbe.csproj`.
- [ ] **Step 4:** `dotnet build Allrpcs.sln -m:1 /nr:false && dotnet format Allrpcs.sln --verify-no-changes && dotnet test Allrpcs.sln --no-build`.
  Expected: all pass. The probe now builds under warnings-as-errors; fix any warnings it surfaces.
- [ ] **Step 5:** Commit `test: enforce the complete project graph and rule-layer SDKs (Fixes #423)`.

### Task 1.2: CI runs everything the repository can check cheaply

**Files:**
- Modify: `.github/workflows/ci.yml`

- [ ] **Step 1:** Replace the four per-project `dotnet test` steps with one step,
  `dotnet test Allrpcs.sln --no-build`.
- [ ] **Step 2:** Add a top-level `concurrency:` block: `group: ci-${{ github.ref }}`,
  `cancel-in-progress: ${{ github.event_name == 'pull_request' }}`.
- [ ] **Step 3:** In "Verify macOS bundle has no bundle signature", first run
  `test -d "${RUNNER_TEMP}/allrpcs-macos-verification/Allrpcs.app" || { echo "Allrpcs.app missing" >&2; exit 1; }`.
- [ ] **Step 4:** Run each candidate command locally from a clean shell. Add a job
  `python-tooling` (ubuntu-24.04, `actions/setup-python` pinned by SHA, Python 3.12) containing
  only the commands that pass without the quality venv:
  `./scripts/validate-workflows.sh`, `python3 scripts/check-continuation.py --self-test`,
  `python3 scripts/test-player-speech-worker.py`, and
  `python3 -m py_compile quality.py scripts/check-continuation.py scripts/*-speech-worker.py`.
  If `./quality.sh --self-test` also passes, add it with `fetch-depth: 0`. List anything left out,
  with the reason, in the commit message.
- [ ] **Step 5:** Run `./scripts/validate-workflows.sh`. Expected: no actionlint findings.
- [ ] **Step 6:** Commit `ci: run solution tests and Python tooling; cancel superseded runs`.
  Push, and confirm every job is green on the pushed commit.

### Task 1.3: Smoke launches fail on hang and on unhandled exception (#421)

**Depends on:** Task 0.2 (an autoload edits `project.godot`).

**Files:**
- Create: `game/CheckDeadline.cs` (autoload node)
- Modify: `game/project.godot` (add `[autoload]` `CheckDeadline="*res://CheckDeadline.cs"`)
- Create: `game/SmokeHarnessHangProbe.tscn` + `.cs`, `game/SmokeHarnessThrowProbe.tscn` + `.cs`,
  `game/SmokeHarnessPassProbe.tscn` + `.cs`
- Create: `scripts/smoke-lib.sh`, `scripts/test-smoke-harness.sh`
- Modify: `scripts/smoke-godot.sh` (all launches go through `run_check`)

**Interfaces:**
- Produces: `run_check <deadline-frames|-> <godot args…>` in `scripts/smoke-lib.sh`. `-` means
  "no deadline" and is used for `--script` and `--editor` launches. Environment contract:
  `ALLRPCS_CHECK_DEADLINE_FRAMES=<positive int>`. On expiry the process exits with code **124**
  and prints `FAIL: check deadline of <N> frames reached`.

- [ ] **Step 1: Record what an unhandled exception actually prints.** Write
  `SmokeHarnessThrowProbe.cs`: `_Ready` throws `InvalidOperationException("smoke harness probe")`;
  `_Process` calls `GetTree().Quit()` on frame 3. Run
  `out=$(mktemp); godot --headless --path game res://SmokeHarnessThrowProbe.tscn >"$out" 2>&1; echo $?; cat "$out"`.
  Expected: exit 0 (this is the bug). Save the exact line that names the exception as the
  wrapper's failure pattern, as an `grep -E` expression, in `scripts/smoke-lib.sh`. Also check
  whether existing passing checks print that pattern: run
  `./scripts/smoke-godot.sh 2>&1 | grep -cE '<pattern>'`. If they do, narrow the pattern until
  that count is 0.
- [ ] **Step 2: Write the harness self-test** `scripts/test-smoke-harness.sh`. It asserts that
  `run_check 30 … res://SmokeHarnessPassProbe.tscn` (quits 0 on frame 1) succeeds, that
  `run_check 30 … res://SmokeHarnessHangProbe.tscn` (never quits) fails with code 124, and that
  `run_check 30 … res://SmokeHarnessThrowProbe.tscn` fails. It prints
  `PASS: smoke harness self-test` and exits 0 only when all three hold.
- [ ] **Step 3:** Run it. Expected: FAIL (`run_check` and the autoload do not exist yet).
- [ ] **Step 4: Implement `CheckDeadline : Node`.** In `_Ready`, it removes itself
  (`QueueFree()`) unless `OS.IsDebugBuild()` is true and the environment variable parses as a
  positive `int`. It sets `ProcessMode = ProcessModeEnum.Always`. `_Process` counts frames; at
  frame N+1 it prints the failure line with `GD.PrintErr` and calls `GetTree().Quit(124)`.
- [ ] **Step 5: Implement `run_check`.** Export the deadline variable; pass
  `--quit-after $((2 * N + 60))` as a backstop; tee the combined output to a `mktemp` file. Fail
  if the exit code is nonzero or the output matches the Step 1 pattern. Echo the launch command on
  failure.
- [ ] **Step 6:** Run `./scripts/test-smoke-harness.sh`. Expected: `PASS: smoke harness self-test`.
- [ ] **Step 7:** Rewrite `scripts/smoke-godot.sh` mechanically. Source `smoke-lib.sh`, and
  replace every `godot … --quit-after N` with `run_check N godot …` (dropping `--quit-after N`).
  Use `run_check -` for the `--editor` import launch and the `--script` launches. Run
  `./scripts/test-smoke-harness.sh` first inside `smoke-godot.sh`. Check the conversion with
  `grep -c '^ *\(ALLRPCS_[A-Z0-9_]*=1 *\\\?\)* *run_check' scripts/smoke-godot.sh` against
  `git show HEAD:scripts/smoke-godot.sh | grep -c 'godot --headless'`: both must be 293.
- [ ] **Step 8:** Commit `test: smoke launches fail on hang or unhandled exception (refs #421)`.
  The launches will now go red; Task 1.4 makes them green before any push to main. Commit on a
  branch `code-health/phase1` if pushing is needed in between.

### Task 1.4: Make every smoke launch pass under the new harness

**Files:**
- Modify: whichever `game/*Check*.cs` launches fail
- Create: `scripts/smoke-quarantine.txt` (format: `<launch-id> #<issue>`, one per line)
- Modify: `scripts/smoke-lib.sh` (quarantined launches run and report `QUARANTINED`, but do not
  fail)

- [ ] **Step 1:** Run `./scripts/smoke-godot.sh` with `set +e` semantics, using a temporary
  `ALLRPCS_SMOKE_CONTINUE=1` flag in `run_check` that collects failures instead of exiting. Record
  the failing launches.
- [ ] **Step 2:** Classify each failure:
  - (a) The check succeeds but never calls `Quit()`: it relied on the budget. Add an explicit
    `GetTree().Quit()` on its success path.
  - (b) The check's own frame limit is ≥ the budget (the 37 launches named in #421): set the
    budget to the check's limit + 30.
  - (c) A real hang or exception in product code: fix it now if the cause is local and obvious.
    Otherwise open an issue labelled `smoke-quarantine` and add the launch to
    `scripts/smoke-quarantine.txt`.
- [ ] **Step 3:** Re-run `./scripts/smoke-godot.sh`. Expected: exit 0, and a final line
  `smoke: <n> passed, <q> quarantined`. Remove the temporary continue flag.
- [ ] **Step 4:** Commit `test: make smoke launches prove completion (Fixes #421)`. Push, and
  confirm the `godot-smoke` CI job is green. If the 20-minute job timeout is exceeded, raise it and
  note the new duration in the commit.

### Task 1.5: Every check is registered (#422)

**Files:**
- Create: `tests/Architecture.Tests/CheckRegistrationTests.cs`
- Create: `tests/Architecture.Tests/ManualCheckRegistrations.txt`
- Modify: `scripts/smoke-godot.sh` (launch the four audio Main checks)

**Interfaces:**
- Consumes: `ProjectReferenceTests.FindRepositoryRoot()`; make it `internal static` if it is
  private.
- Produces: allowlist format `<GATE-or-res://Scene.tscn>  # <reason>`, one per line. `#` lines
  are comments.

- [ ] **Step 1: Write the failing tests.** Live launch sources are `scripts/smoke-godot.sh` and
  `quality.py`. Frozen `scripts/quality.sh` does not count.
  - `EveryCheckGateIsLaunchedOrAllowlisted`: every `ALLRPCS_[A-Z0-9_]+_CHECK` found in
    `game/**/*.cs` and `game/**/*.gd` appears in a launch source or in the allowlist.
  - `EveryCheckSceneIsLaunchedOrAllowlisted`: every `game/*Check*.tscn` and `game/*Proof*.tscn`
    appears as `res://<name>` in a launch source or in the allowlist.
  - `AllowlistHasNoStaleEntries`: every allowlisted gate or scene still exists in `game/`.
- [ ] **Step 2:** Run them. Expected: FAIL, listing the four audio gates from #422 plus about 24
  other gates and 32 scenes.
- [ ] **Step 3:** Add the four `ALLRPCS_{MUSIC,MATERIAL,FOOTSTEP,POSITIONAL}_MAIN_CHECK` launches
  to `smoke-godot.sh` through `run_check`. Run each one, and triage failures as in Task 1.4.
- [ ] **Step 4:** Handle every remaining unregistered gate or scene. Prefer adding the launch.
  Allowlist it only with a concrete reason, e.g. `# frozen quality.sh only (art revision X)` or
  `# owner-operated manual check`.
- [ ] **Step 5:** `dotnet test Allrpcs.sln --no-build && ./scripts/smoke-godot.sh`. Expected:
  both pass.
- [ ] **Step 6:** Commit `test: require every engine check to be launched or allowlisted (Fixes #422)`.

### Phase 1 exit

- [ ] Run `./quality.sh` once. Expected: pass. Tick the Phase 1 roadmap line. Close the ledger
  slice: add a `closed_scopes` row, with this plan and the passing quality `summary.json` as
  evidence. Make Phase 2 the active slice. Then `--sync` and run the checker.

---

## Phase 2 — Fix what a player hits (write `…-phase2-player-defects.md` when it starts)

Scope, in order. Each item becomes a task with a pinning test in xUnit when the logic allows,
otherwise as a Main check run through `run_check`.

1. **Providers.** Fix the following:
   - #417 with one shared stream-document depth helper used by all 12 accumulators: introduce
     the helper first and switch the copies to it in the same task. Confirm against a live call
     and add a recorded real OpenAI stream fixture.
   - #418, #419 (stop at the terminal event and process incrementally), #425.
   - §3.1: a per-request deadline and idle-read timeout; `AllowAutoRedirect = false`; `Retry-After`
     backoff; usage telemetry treated as unknown rather than invalid.
   - Run the suspected Keychain save check manually with the owner.
2. **Restore ownership.** Introduce an `IRestoreParticipant` registry with two-phase
   validate-then-commit restore (§1.6). Then fix #420, #426 (identifier value equality), #429, and
   transcript reset.
3. **Input.** Fix #427 and #428, and add a controls-document migration that fills in missing
   actions (§3.3).
4. **Visible status instead of silent drops.** Show "provider unavailable" and "busy" statuses;
   set the processed-delivery marker before side effects (§3.2); keep the healer from hearing
   while absent.
5. **Camera.** Clamp to the effective minimum zoom and recompute on `size_changed` (§3.4).
6. **Speech.** Fix #430 and #431, the Moonshine linked-token timeout, and the worker dispose
   timeouts (§3.5).
7. **Shipping hygiene.** Exclude `*Check*`, `*Proof*` and `*Probe*` in `export_presets.cfg`, and
   honour test-store environment variables only when `OS.IsDebugBuild()` (§1.3).
8. **Tooling bugs.** Fix #414, #415 and #416, and add a per-stage timeout to `quality.py` (§3.6).
9. **Long sessions.** Evict the oldest relationship reasons instead of freezing at 64; add a
   capacity check to summary recording; make transcript and world-event appends O(1) amortised;
   use `SequenceEqual` (or sealed classes) for the 19 list-holding records (§1.7, §3.2, §3.4).
   Fix #424 (ducking peak) here too.

**Exit:** all `code-review-1` issues are closed; the five Review Focus items have pinning tests;
one full `./quality.sh` passes.

## Phase 3 — Stop the duplication before adding gameplay (write its own plan)

The hypothesis loop in Phase 4 adds a new LLM decision type, so the pipeline must be consolidated
first (§1.2).

1. A `StructuredDecisionPipeline<TRequest, TDecision>` in `src/Application`, plus one
   `OpenAiResponsesStructuredClient<TRequest, TDecision>` and a shared send/failure helper for the
   three providers. Migrate the 10 sabotage decision types one at a time, each behind its existing
   tests. Target: about 6–7k lines of adapter code down to under 1k. Generalise the bindings so
   Anthropic and Ollama work for sabotage too.
2. Extract `ConversationTurnCoordinator` and `FocusedSearchInputCoordinator` from `Main.cs`, with
   their state machines in `src/Application` under xUnit tests (§1.4). Give each `ProcessPriority`
   explicitly, and add a check that asserts the order (§1.5).
3. Add a `SmokeCheckHost` that instantiates only the requested check, which removes the 213
   always-present check nodes from `Main.tscn` (§1.3).
4. Rolling rule from here on: any coordinator touched for gameplay work moves its state machine
   into `src/Application` with tests, leaving a thin Node adapter (§1.1).

**Exit:** no OpenAI per-decision-type files remain; `Main.cs` is under 400 lines; `Main.tscn`
holds no check nodes; all tests, smoke checks and `./quality.sh` pass.

## Phase 4 — Playable vertical slice (one plan per item; owner playtest at the end)

1. **New-game flow.** Customization opens at the start, and the background gates at least one
   route-access consumer.
2. **Memory that survives a save (step 9).** Wire episodic memory writes from dialogue and
   observation into `CharacterMemoryController`. Save the transcript, customization and character
   positions. Offer continue-on-launch.
3. **Ingredients (steps 5, 7).** Unify `CharacterInventory` with `InventoryOwnershipSnapshot`;
   wire Give/Take/Use; make conditional collection the normal path (remove the
   `ConditionalCollectionScaffolding` gate). Ingredients are identified through evidence, and a
   wrong remedy is possible.
4. **Hypotheses (step 6).** Add a journal input that proposes a hypothesis. The interpreter runs
   through the Phase 3 pipeline and the result is tested via an experiment.
5. **Stakes and ending (step 8).** Wire disease progression to journey events, so stabilization
   can fail. Add an explicit slice-complete screen.
6. **Remove the boot-time evidence auto-resolution** (`Main.cs:315-319`) and the
   `Temporary*` outcome sources it relies on.
7. **Acceptance:** the owner plays from a fresh profile to the ending on local Ollama. The result
   is recorded as an issue comment or a short session note. Then tick the slice items in
   `ROADMAP.md`.

## Phase 5 — Repository hygiene (can interleave once Phase 3 is done)

- **Split `game/` into feature folders:** `Dialogue/`, `Sabotage/`, `Audio/`, `World/`, `Save/`,
  `Checks/`. Move each `.cs`, `.tscn` and `.uid` together, then rerun the smoke harness. Before
  moving anything, confirm that `quality.py`'s current-revision stages do not hard-code `game/`
  paths.
- **Resolve every `Temporary*` type:** rename it, or delete it once Phase 4 has replaced it.
- **Dead Domain types:** delete the 30 unreferenced ones, or mark them staged.
- **Shorten `README.md`** to setup, commands and a map; move the boundary prose to `docs/`.
- **Central package versions:** create `Directory.Packages.props` and update xunit and the Test SDK.
- **Revisit D6** (the evidence-repository split).

## Not in this program

Production art and audio (Phase 9 of the roadmap), the remaining physical microphone cases,
packaging and signing (Phase 11), the two hermits, and a visible enemy agent. These go back on the
queue after Phase 4.
