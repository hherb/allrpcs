# Allrpcs Handover

## Current state

`main` contains merged PR #232 at `038476a`. The current branch is
`codex/phase-7-sabotage-scene-resolution` and advances five bounded Phase 7 slices:

- Phase 7dt resolves one exact accepted concealment consideration result against scene-owned
  snapshots, atomically installing selection output while exact abstention retains both instances;
- Phase 7du owns one cancellation-aware decision task through closed terminal states and resolves
  only a non-stale accepted result on the Godot main thread;
- Phase 7dv supplies one exact one-shot decision model or one stable fail-closed offline model;
- Phase 7dw explicitly composes the authored enemy/access/world/current-state/candidate request and
  starts that task without selecting timing; and
- Phase 7dx wires the model source, task, and explicit flow into `Main.tscn` after sabotage-state
  initialization while startup remains inert.

No real provider is composed or invoked. Nothing starts sabotage automatically, chooses timing,
decodes or serializes provider data, persists concealment, or adds UI/input/events/time/transcripts,
movement, or another sabotage effect.

## Validation and review

- Every slice began with the expected failure for its absent operation, type, or scene node.
- Focused host-local Godot smokes pass exact selection and abstention resolution, stale/null/repeated
  rejection, closed task lifecycle, cancellation, safe failure, one-shot/offline model selection,
  exact request composition, overlap preservation, real-main-scene wiring, inert startup, unchanged
  source/surrounding state, and all-or-nothing main-thread installation.
- Host-local `./scripts/quality.sh` passes workflow validation, restore, formatting, zero-warning
  build, 2,306 Domain tests, 484 Application tests, 532 Infrastructure tests, 8 architecture tests,
  scenario validation, every credential-free headless Godot smoke, and the Compatibility-renderer
  graphical launch.
- All changed authored C# files are below 500 lines and use no lines longer than 120 characters.
  No live-conformance switch was enabled and no provider was invoked.

## Exact next bounded slice

Take **Phase 7dy: strictly decode one concealment decision JSON document for an exact request**.

Acceptance boundary:

1. Add one Infrastructure deserializer accepting the exact
   `CharacterObjectSabotageConcealmentConsiderationRequest` and one complete JSON document.
2. Require exactly `characterId` and nullable `selectedEvidenceId`, reject duplicate, missing,
   unknown, non-string, malformed, trailing, over-depth, and over-length input, and preserve
   identifier text exactly through existing parsers.
3. Materialize a provider-neutral `CharacterObjectSabotageConcealmentDecision` and delegate the
   existing exact-request acceptance policy before returning it; invalid selection and character
   identifiers must fail as one bounded invalid-response argument error.
4. Prove accepted exact selection and abstention, distinct value-equal identifier materialization,
   complete rejection coverage, unchanged request state, and the smallest static public surface in
   focused Infrastructure tests.
5. Do not add SSE accumulation, request serialization, an HTTP adapter, provider/model bindings,
   scene composition, timing/automatic triggers, persistence, UI/events/time/transcripts, movement,
   or another sabotage effect.
