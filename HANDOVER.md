# Allrpcs Handover

## Current state

`main` is verified at merged PR #252 (`03fe0c7`). Branch
`codex/phase-7-substitution-source` advances Phase 7 substitution through these bounded slices:

- an inert temporary source retains the existing exact authoritative world-object snapshot and its
  tail `village-water-cask`, one representative enemy, complete target access, a distinct
  `tainted-water-cask`, empty substitution/evidence snapshots, and one exact eligible
  evidence-bound candidate;
- isolated and main-scene flows initialize exact scene-owned state, apply only on explicit request,
  and recover only after exact evidence is visible through supplied perception;
- a stale-safe presentation controller swaps only the original and substitute cask polygons while
  preserving authoritative world objects and all scene positions;
- ordinary focused-search resolution recovers the substitution only for the exact water-cask
  outcome; unrelated outcomes leave it active; and
- a provider-neutral consideration request retains exact authority plus at most 64 stable copied
  candidates, while an exact-character selection-or-abstention decision accepts only an exact
  currently eligible evidence identifier.

Implementation commit `fe7b7cd` is published in draft PR #253. Hosted run `34916632446` failed
before executing any steps in all five credential-free jobs (`steps: []`), so it is recorded as
Actions/account infrastructure state rather than product-test evidence and was not retried.

The implementation does not replace or mutate authoritative world objects, consume evidence,
infer inventory, placement, similarity, plausibility, reachability, critical-route safety, or
replacement eligibility, or add timing, scheduling, a task, model, provider, persistence, new
input, UI controls, events, time, transcripts, navigation, or automatic sabotage. The unrelated
untracked `scripts/code_statistics.py` remains untouched and must stay unstaged.

## Validation and review

- Tests began with the expected missing source, flow, presentation, request, and decision compile or
  focused-smoke failures.
- Focused host-local checks pass the inert source, explicit apply/recovery flow, main-scene
  composition, stale-safe presentation, ordinary water-cask investigation, 5 consideration-request
  cases, and 8 decision/policy cases. Sandboxed Godot hit the known `user://logs` crash and
  sandboxed VSTest hit the known IPC `SocketException (13)`; identical host-local commands passed
  without product changes.
- Host-local `./scripts/quality.sh` passes workflow validation, restore, formatting, a zero-warning
  build, 2,446 Domain tests, 515 Application tests, 682 Infrastructure tests, 8 architecture tests,
  scenario validation, every credential-free headless Godot smoke, and the final Compatibility
  renderer launch. No live-conformance switch was enabled and no provider was invoked.
- All changed authored C# files are below 500 lines with no lines longer than 120 characters.
  `dotnet format Allrpcs.sln --no-restore --verify-no-changes` and `git diff --check` pass. Final
  staged-diff review found no unresolved findings.

## Exact next bounded slice

Take **Phase 7gr: bind one accepted substitution consideration result**.

Acceptance boundary:

1. Add one immutable provider-neutral result retaining the exact substitution consideration
   request, accepted decision, and exact selected candidate, or null only for explicit abstention.
2. Add one pure transition that delegates the existing exact decision policy, resolves a selection
   only by the decision's exact evidence-identifier instance, and returns the request's exact
   candidate without applying it.
3. Reject null, invalid, stale, unknown, distinct value-equal, or currently ineligible decisions;
   preserve the exact request and every authority snapshot and candidate on success.
4. Do not apply or present a substitution or add a model interface, controller, task, provider,
   persistence, `Main.tscn` wiring, input, UI, timing, scheduling, events, time, transcripts, or
   navigation behavior.
