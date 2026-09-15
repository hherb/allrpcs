# Allrpcs Handover

## Current state

`main` is verified at merged PR #253 (`ed48fc1`). Branch
`codex/phase-7-substitution-decision-flow` advances Phase 7 substitution through these exact
provider-neutral and scene-owned slices:

- an immutable consideration result retains the exact request, accepted decision, and exact
  selected request candidate, or null only for explicit abstention;
- one cancellation-aware model interface and pure Application controller obtain a decision for the
  exact request and return only its validated result;
- one pure transition applies an accepted selection through the existing atomic
  substitution/evidence transition while abstention returns no application; and
- the scene-owned substitution controller resolves each exact request at most once, requires its
  exact current substitution/evidence snapshots, atomically installs both fresh selection outputs,
  and retains both exact snapshots for abstention.

Implementation commit `34e8e9a` is published in draft PR #254. GitHub reported no check runs yet
for that implementation head when the draft was created.

The implementation does not present a substitution or add a task, model source, provider adapter,
provider binding, persistence, `Main.tscn` wiring, input, UI, timing, scheduling, automatic
sabotage, events, time, transcripts, or navigation. Authoritative world objects and every supplied
request, candidate, substitution, evidence, and surrounding snapshot remain unchanged. The
unrelated untracked `scripts/code_statistics.py` remains untouched and must stay unstaged.

## Validation and review

- Tests began with the expected missing result, provider-neutral model/controller, consideration
  transition, and scene-resolution compile failures.
- Focused host-local checks pass 14 substitution-consideration Domain tests, 6 substitution
  Application tests, a zero-warning game build, and the exact scene-owned consideration-resolution
  Godot smoke. Sandboxed Godot hit the known `user://logs` crash and sandboxed VSTest hit the known
  IPC `SocketException (13)`; identical host-local commands passed without product changes.
- Host-local `./scripts/quality.sh` passes workflow validation, restore, formatting, a zero-warning
  build, 2,455 Domain tests, 521 Application tests, 682 Infrastructure tests, 8 architecture tests,
  scenario validation, every credential-free headless Godot smoke, and the final Compatibility
  renderer launch. No live-conformance switch was enabled and no provider was invoked.
- All new or changed authored C# files are below 500 lines with no lines longer than 120 characters.
  `git diff --check` passes. Slice-by-slice review found no unresolved findings.

## Exact next bounded slice

Take **Phase 7gw: own one bounded substitution-decision task**.

Acceptance boundary:

1. Add one closed task status with idle, running, completed, cancelled, and failed states, plus one
   scene controller retaining only the exact successfully resolved consideration result.
2. Start only while idle and only when the request carries the exact current scene-owned
   substitution/evidence snapshots; reject null, stale, repeated, or overlapping starts.
3. Observe completion on the main thread, resolve only through the existing exact scene controller,
   keep cancellation, model failure, invalid decisions, and stale successful results mutation-free,
   and permit explicit reset only from terminal state.
4. Cancel an active task on explicit request or tree exit without adding a model source, provider,
   presentation, `Main.tscn` wiring, persistence, input, timing, scheduling, automatic invocation,
   events, time, transcripts, or navigation behavior.
