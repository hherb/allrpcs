# Allrpcs Handover

## Current state

`main` contains merged PR #243 at `0b528cc`. The current branch is
`codex/phase-7-move-sabotage-source` and advances four bounded Phase 7 slices:

- Phase 7fb adds `TemporaryCharacterObjectSabotageMoveSource`, retaining the existing exact tail
  water cask, one representative enemy and its complete access, exact source and destination
  locations, one initial placement, empty evidence, one eligible move, and its aligned recovery.
- Phase 7fc adds `TemporaryCharacterObjectSabotageMoveFlowController`, initializing exact
  scene-owned placement and evidence state inertly, then applying only on explicit request and
  recovering only through perception-projected exact evidence.
- Phase 7fd composes that source, move controller, and explicit flow in `Main.tscn` in dependency
  order while ordinary startup retains exact empty initial state and applies nothing.
- Phase 7fe adds `CharacterObjectSabotageMovePresentationController`, projecting only one exact
  target at its exact source or destination `Location` onto explicit finite scene coordinates with
  stale-safe presentation state.

These slices do not choose sabotage behavior or timing; automatically call apply or recovery; infer
coordinates, paths, reachability, travel, or duration; move an actor; consume evidence; change an
authoritative world object; or add persistence, input, UI, events, time, transcripts, navigation,
providers, models, or ordinary-flow presentation.

Draft publication is pending from the implementation branch.

## Validation and review

- Tests began with the expected missing-source, missing-flow, missing-main-node, and missing-
  presentation-controller failures.
- The isolated source, explicit apply/recovery flow, inert main-scene composition, and exact
  placement-presentation Godot smokes pass host-local. The first source smoke was blocked in the
  sandbox by `user://logs` and signal 11; the identical smoke and subsequent Godot validation pass
  host-local.
- Host-local `./scripts/quality.sh` passes workflow validation, restore, formatting, a zero-warning
  build, 2,385 Domain tests, 506 Application tests, 607 Infrastructure tests, 8 architecture tests,
  scenario validation, every credential-free headless Godot smoke, and the final Compatibility-
  renderer launch. No live-conformance switch was enabled and no provider was invoked.
- All seven authored C# files are below 500 lines with no lines longer than 120 characters.
  `git diff --check` passes, and manual review found no unresolved findings.
- Unrelated untracked `scripts/code_statistics.py` remains untouched and must stay unstaged.

## Exact next bounded slice

Take **Phase 7ff: coordinate explicit main-scene move placement presentation**.

Acceptance boundary:

1. Add the move-presentation controller and one temporary presentation-flow coordinator to
   `Main.tscn` in dependency order. Initialize it with the exact move source target, source and
   destination locations, the water-cask body, its exact authored source position, the exact current
   placement snapshot, and one explicit finite authored destination position; startup stays inert.
2. Expose only explicit `TryApply` and perception-gated `TryRecover` operations. Before changing
   state, require the presentation controller's exact last snapshot to match scene-owned placements;
   after the existing flow succeeds, present its exact fresh placement once.
3. Add one opt-in main-scene smoke proving exact inert source presentation, one explicit move to the
   authored destination with exact evidence, repeat and unseen recovery rejection, exact noticed-
   evidence recovery to the retained source position, unchanged authoritative world objects and
   source snapshots, retained evidence, and closed coordinator behavior.
4. Do not add an ordinary caller or automatic schedule; choose sabotage timing; move an actor;
   derive a coordinate, path, reachability, travel, or duration; consume evidence; change an
   authoritative world object; or add persistence, input, UI, events, time, transcripts, navigation,
   providers, or models.
