# Allrpcs Handover

## Current state

`main` contains merged PR #249 at `1f9ae68`. The current branch is
`codex/phase-7-move-model-binding` and advances five bounded Phase 7 slices:

- Phase 7fx binds one exact OpenAI move-decision model to its canonical
  `CharacterModelSettings` and composes it through a caller-owned factory with default or custom
  validated endpoints;
- Phase 7fy extends the move-model source with exact configured-binding initialization,
  compare-and-swap replacement and clearing, preserved one-shot precedence, and stable fail-closed
  offline fallback;
- Phase 7fz composes, replaces, and clears that move binding through the existing selected-model
  settings flow: OpenAI activates it, an OpenAI setting change replaces it, and Ollama clears it;
- Phase 7ga observes each fresh exact completed move-decision result at most once and presents only
  a selected result through the existing stale-safe placement seam; and
- Phase 7gb explicitly acknowledges only an already-observed completed result whose exact selection
  presentation, or exact abstention request snapshots, remain current.

Implementation commit `982ad10` is published in mergeable draft PR #250. Hosted run `34886260455`
failed before execution with `steps: []` for all five jobs; this is Actions/account infrastructure
state, not product-test or unsigned-build evidence, and the unchanged run was not retried.

Binding, factory, source, and settings composition perform no credential or network input/output
and invoke no model. Startup remains inert. Nothing chooses timing, starts sabotage automatically,
recovers a move automatically, persists sabotage, adds input/UI/events/time/transcripts/navigation
or actor movement, adds another provider, or adds another sabotage effect. The unrelated untracked
`scripts/code_statistics.py` remains untouched and must stay unstaged.

## Validation and review

- Tests began with the expected missing binding, factory, source-operation, coordinator, and
  acknowledgement compile failures. The settings-composition smoke also failed as expected before
  the move binding was wired.
- Focused binding tests pass 3 Application and 5 Infrastructure cases. The configured-source,
  settings-composition, isolated selected/abstaining/failed presentation and acknowledgement, and
  real-main-scene inert-wiring Godot smokes pass.
- The first sandboxed focused VSTest was blocked by local IPC `SocketException (13)` and the first
  sandboxed Godot smoke by its `user://logs` restriction; the identical host-local commands passed.
  Product code was not changed to work around either sandbox restriction.
- Host-local `./scripts/quality.sh` passes workflow validation, restore, formatting, a zero-warning
  build, 2,407 Domain tests, 515 Application tests, 682 Infrastructure tests, 8 architecture tests,
  scenario validation, every credential-free headless Godot smoke, and the final Compatibility-
  renderer launch. No live-conformance switch was enabled and no provider was invoked.
- All changed authored C# files are below 500 lines with no lines longer than 120 characters.
  `git diff --check` passes, and manual review found no unresolved findings.

## Exact next bounded slice

Take **Phase 7gc: bind one exact substitute-sabotage occurrence to one authored replacement**.

Acceptance boundary:

1. Add one immutable Domain value that accepts one exact `CharacterObjectSabotageOccurrence` and one
   exact authored replacement `WorldObject`, retaining both without copying or applying state.
2. Accept only an occurrence whose proposal uses the exact canonical
   `CharacterSabotageActionKind.Substitute`; reject nulls, non-substitute occurrences, the exact
   occurrence target as its own replacement, and a replacement with the same object identifier.
3. Prove exact nested actor, access requirement, target, and replacement retention; non-substitute
   rejection; independently composed value-equal replacement rejection; unchanged source objects;
   and a closed get-only surface.
4. Do not infer inventory, current placement, physical similarity, plausibility, path, reachability,
   critical-route safety, or replacement eligibility; do not replace a world object or body, treat
   binding as occurrence or success, leave evidence, change truth or perception, or add selection,
   application, recovery, scheduling, model, provider, Application, Infrastructure, scene,
   persistence, input, UI, event, time, transcript, navigation, or presentation behavior.
