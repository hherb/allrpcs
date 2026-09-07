# Allrpcs Handover

## Current state

PR #130 merged Phases 2o1–2o5 into `main` at `54f8e69`. The continuation branch
`codex/phase-2o6-plus-world-events` implements Phases 2o6–2o7.

The scene now has one independent `TemporaryWorldEventSource` retaining the canonical
`greybox-ready` `WorldEventId`. During `Main._Ready`, the exact source identifier is appended once
through `WorldEventLogController`; inability to record it fails explicitly. The source exposes no
controller access, and startup recording changes no world-object, perception, focused-search,
movement, input, or presentation state. Event payloads and time behavior remain absent.

Both slices were test-first through dedicated opt-in Godot smokes. After code review,
`./scripts/quality.sh` passed workflow and formatting validation, a zero-warning build, 949 Domain
tests, 107 Application tests, six architecture tests, all 24 opt-in Godot smokes, and the bounded
graphical launch.

## Exact next bounded slice

Take **Phase 2o8: immutable world-event time-effect classification**.

Acceptance boundary:

1. Add one closed, engine-independent Domain value identifying whether an existing world event has
   no time effect or advances meaningful world time.
2. Give both states canonical text, exact singleton identity, stable ordering, strict parsing, and
   the smallest public surface; add focused tests first for all of those guarantees.
3. Update README and ROADMAP evidence, keep assignment and progression incomplete, run
   `./scripts/quality.sh`, review every intended path, and prune this file again.

Explicit exclusions:

- Do not attach the classification to an event, change the identifier-only log, or define payloads,
  actors, targets, timestamps, durations, clock units, phase names, progression thresholds, disease
  effects, replay, persistence, serialization, or migration.
- Do not connect time behavior to Godot, Application, movement, dialogue, discovery, characters,
  world objects, providers, credentials, or UI.
- Keep point-and-click movement, disease-time pausing, and the world-time debug overlay deferred.
- Do not merge, release, distribute, sign, notarize, or change repository visibility without
  explicit user authorization.
