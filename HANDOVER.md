# Allrpcs Handover

## Current state

`main` contains merged PR #244 at `89e8af9`. The current branch is
`codex/phase-7-move-placement-flow` and advances three bounded Phase 7 slices:

- Phase 7ff composes exact water-cask move placement presentation with the existing scene-owned
  move flow. Startup remains inert; explicit apply presents one authored destination coordinate,
  and noticed exact evidence restores the retained source placement and coordinate.
- Phase 7fg forwards only a successfully resolved exact water-cask focused-search outcome and
  current perception to move recovery. Other resolved targets leave the moved cask unchanged.
- Phase 7fh adds `CharacterObjectSabotageMoveConsiderationRequest`, retaining exact current move
  authority and a defensively copied, evidence-ID-unique collection of at most 64 exact candidates.

These slices do not choose sabotage behavior or timing; start a consideration; define, obtain, or
apply a model decision; automatically apply a move; infer coordinates, paths, reachability, travel,
or duration; move an actor; consume evidence; change authoritative world-object truth; or add
persistence, input, UI, events, time, transcripts, navigation, providers, or models.

## Validation and review

- Tests began with the expected missing move-presentation coordinator, unrecovered moved-cask
  focused-search outcome, and missing move-consideration request failures.
- Focused host-local smokes pass for exact explicit move presentation/recovery and ordinary exact
  focused-search recovery. The first presentation smoke was blocked in the sandbox by
  `user://logs` and signal 11; the identical host command passed.
- All five focused move-consideration request tests pass host-local. Sandbox VSTest was blocked by
  `SocketException (13): Permission denied`; the identical host command passed.
- Host-local `./scripts/quality.sh` passes workflow validation, restore, formatting, a zero-warning
  build, 2,390 Domain tests, 506 Application tests, 607 Infrastructure tests, 8 architecture tests,
  scenario validation, every credential-free headless Godot smoke, and the final Compatibility-
  renderer launch. No live-conformance switch was enabled and no provider was invoked.
- All five new C# files are 77–216 lines with no lines longer than 120 characters.
  `git diff --check` passes, and manual review found no unresolved findings.
- Unrelated untracked `scripts/code_statistics.py` remains untouched and must stay unstaged.

## Exact next bounded slice

Take **Phase 7fi: define an exact move selection-or-abstention decision**.

Acceptance boundary:

1. Add one immutable provider-neutral decision retaining the exact deciding `CharacterId` and the
   exact selected move-candidate `EvidenceId`, or `null` for explicit abstention.
2. Reject a null character identifier while preserving the exact supplied identifier instances;
   rely on the existing identifier value contracts for nonblank values.
3. Add focused Domain tests for exact selection, explicit abstention, null rejection, and a closed
   immutable surface.
4. Do not accept or resolve the decision against a request; add controllers, tasks, scene wiring,
   serialization, provider adapters, model invocation, apply or recovery behavior, presentation,
   timing, persistence, input, UI, events, transcripts, navigation, or actor movement.
