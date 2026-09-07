# Allrpcs Handover

## Current state

PR #133 merged Phases 2o19–2o28 into `main` at `ff66464`. The branch
`codex/phase-2o29-plus-world-events` implements Phase 2o29.

`WorldEventController` is now the scene's single ordinary world-event authority. It initializes
once from exact authoritative-event, identifier-log, and qualitative-time snapshots, then delegates
occurrences to the pure Domain policy and transition. Successful occurrences install both returned
snapshots together. Startup routes `greybox-ready` through this path, retaining its exact
authoritative ID and the exact initial time snapshot because the event is `no-advance`.

After code review, `./scripts/quality.sh` passed workflow and formatting validation, a zero-warning
build, 1,020 Domain tests, 107 Application tests, six architecture tests, all 26 opt-in Godot
smokes, and the bounded graphical launch. The first full run exposed two obsolete fixture
assumptions about the old startup log; those smokes now correctly distinguish the aggregate runtime
controller from the unchanged standalone log/time fixtures, and the complete rerun passed.

## Exact next bounded slice

Take **Phase 2o30: first authored advancing greybox event definition**.

Acceptance boundary:

1. Extend `TemporaryWorldEventSource` with exactly one stable `herb-patch-identified` event whose
   time effect is `advance`, preserving `greybox-ready` first and exposing an unambiguous exact
   startup-event reference instead of relying on a single-entry snapshot.
2. Update startup to select only that exact `greybox-ready` reference. Prove stable source order,
   exact identifiers/effects, and unchanged startup log/time/world/discovery/presentation state.
3. Do not trigger the new advancing event in ordinary play yet; cross-controller focused-search
   integration is a later atomicity slice.
4. Update README and ROADMAP evidence, run `./scripts/quality.sh`, review intended paths, and prune
   this file again.

Explicit exclusions: no generic repeatable event types, payloads, actors, targets, clocks,
durations, disease effects, replay, persistence, serialization, migration,
Application/provider/credential/UI integration, point-and-click movement, disease-time pausing, or
debug overlay. Do not merge, release, distribute, sign, notarize, or change repository visibility
without explicit authorization.
