# Allrpcs Handover

## Current state

`main` contains merged PR #247 at `5352935`. The current branch is
`codex/phase-7-move-model-source` and advances three bounded Phase 7 slices:

- Phase 7fq supplies one exact one-shot move-decision model or one stable fail-closed offline
  model, with caller cancellation taking precedence over request validation;
- Phase 7fr composes the authored move consideration from exact current scene snapshots and starts
  the existing bounded task only on explicit request, preserving an overlapping queued model; and
- Phase 7fs wires that model source, task, and explicit flow into `Main.tscn` after move-state
  initialization while ordinary startup remains inert.

No configured binding, serializer, decoder, SSE path, HTTP adapter, credential access, or provider
invocation was added. Nothing chooses sabotage timing, starts a decision automatically, recovers or
presents a model-selected move, persists sabotage, or adds input, UI, events, time, transcripts,
navigation, or actor movement.

Implementation commit `3fedb1a` is published in mergeable draft PR #248. Initial hosted run
`34860416372` failed before execution with `steps: []` for all five jobs; this is Actions/account
infrastructure state, not product-test evidence, and the unchanged run was not retried.

## Validation and review

- The first focused build failed as expected because the new move-model source did not exist.
- The focused host-local game build passes with zero warnings.
- Dedicated host-local Godot smokes pass exact one-shot selection and consumption, overlap
  rejection, later resupply, stable offline identity, cancellation-first failure, null rejection,
  no invocation during selection, unchanged request state, exact authored request composition,
  atomic main-thread result resolution, offline task failure, and inert main-scene wiring.
- Host-local `./scripts/quality.sh` passes workflow validation, restore, formatting, a zero-warning
  build, 2,407 Domain tests, 512 Application tests, 607 Infrastructure tests, 8 architecture tests,
  scenario validation, every credential-free headless Godot smoke, and the final Compatibility-
  renderer launch. No live-conformance switch was enabled and no provider was invoked.
- All changed authored C# files are at most 210 lines with no lines longer than 120 characters.
  `git diff --cached --check` passes, and manual review found no unresolved findings.
- The first sandboxed focused Godot smoke was blocked by its `user://logs` crash; the identical
  host-local smoke passed. Product code was not changed to work around the sandbox.
- Hosted run `34860416372` executed no job steps; host-local quality is the product evidence.
- Unrelated untracked `scripts/code_statistics.py` remains untouched and must stay unstaged.

## Exact next bounded slice

Take **Phase 7ft: strictly decode one move decision JSON document for an exact request**.

Acceptance boundary:

1. Add one Infrastructure deserializer accepting the exact
   `CharacterObjectSabotageMoveConsiderationRequest` and one complete JSON document.
2. Require exactly `characterId` and nullable `selectedEvidenceId`; reject duplicate, missing,
   unknown, non-string, malformed, trailing, over-depth, and over-length input.
3. Resolve valid parsed identifier text to the request-owned exact character or candidate-evidence
   identifier, materialize a provider-neutral `CharacterObjectSabotageMoveDecision`, and delegate
   the existing exact-request acceptance policy. Invalid identifiers and selections must fail as
   one bounded invalid-response argument error.
4. Prove accepted exact selection and abstention, exact request-owned identifier resolution,
   complete rejection coverage, unchanged request state, and the smallest static public surface in
   focused Infrastructure tests.
5. Do not add SSE accumulation, request serialization, HTTP or provider adapters, model bindings,
   scene composition, automatic timing, recovery/presentation, persistence, input, UI, events,
   time, transcripts, navigation, actor movement, or another sabotage effect.
