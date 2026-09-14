# Allrpcs Handover

## Current state

`main` contains merged PR #241 at `fa7f4c8`. The current branch is
`codex/phase-7-move-sabotage-destination` and advances four bounded Phase 7 slices:

- Phase 7et adds `CharacterObjectSabotageMoveDestination`, retaining one exact move occurrence and
  one exact authored `Location` while rejecting every non-move object-sabotage kind.
- Phase 7eu adds `WorldObjectPlacement` and `WorldObjectPlacementSnapshot`, retaining up to 64 exact
  authoritative object/location pairs in stable copied order with duplicate object IDs rejected.
- Phase 7ev adds `CharacterObjectSabotageMoveCandidate`, binding one destination and one evidence
  candidate only when both retain the same exact occurrence instance.
- Phase 7ew adds `CharacterObjectSabotageMovePolicy`, transition, and result. One eligible exact
  target placement is replaced at the same stable index with its exact different authored
  destination while exact fresh evidence is appended atomically.

These Domain-only slices do not infer current location, destination eligibility, path, reachability,
travel duration, timing, or scene coordinates; move an actor or Godot body; change the authoritative
world object; apply a value-equal replacement; recover the move; select sabotage behavior; or add
Application, Infrastructure, scene, persistence, input, UI, event, time, transcript, navigation,
provider, model, or presentation behavior.

## Validation and review

- Tests began with the expected missing-type compile failures for each slice.
- Focused host-local tests pass: 6 destination-binding, 6 placement, 3 move-candidate, and 6
  move-policy/transition cases (21 combined). The first sandboxed VSTest run was blocked by its local
  socket; the identical focused commands passed host-local.
- Host-local `./scripts/quality.sh` passes workflow validation, restore, formatting, a zero-warning
  build, 2,373 Domain tests, 506 Application tests, 607 Infrastructure tests, 8 architecture tests,
  scenario validation, every credential-free headless Godot smoke, and the final Compatibility-
  renderer launch. No live-conformance switch was enabled and no provider was invoked.
- All eleven authored C# files are below 500 lines with no lines longer than 120 characters.
  `git diff --check` passes, and manual review found no unresolved findings.
- Unrelated untracked `scripts/code_statistics.py` remains untouched and must stay unstaged.

## Exact next bounded slice

Take **Phase 7ex: retain one exact move-sabotage recovery candidate**.

Acceptance boundary:

1. Add one immutable Domain value that accepts one exact `CharacterObjectSabotageMoveCandidate` and
   one exact pre-move `WorldObjectPlacement`, retaining both without applying recovery.
2. Accept only a source placement whose world object is the move occurrence's exact target and whose
   authored location ID differs from the move destination; reject nulls, a value-equal replacement
   target, and exact or value-equal destination-location reuse.
3. Prove exact candidate, occurrence, target, source-location, destination, and evidence retention;
   independently composed value-equal target and location rejection; unchanged inputs; and a closed
   get-only public surface.
4. Do not check current placement or evidence visibility, infer where an object was, apply or present
   recovery, consume evidence, move a body, select timing or behavior, or add policy, transition,
   Application, Infrastructure, scene, persistence, input, UI, event, time, transcript, navigation,
   provider, model, or presentation behavior.
