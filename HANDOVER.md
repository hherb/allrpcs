# Allrpcs Handover

## Current state

PR #130 merged Phases 2o1–2o5 into `main` at `54f8e69`. The continuation branch
`codex/phase-2o6-plus-world-events` implements Phases 2o6–2o8.

The scene now has one independent `TemporaryWorldEventSource` retaining the canonical
`greybox-ready` `WorldEventId`. During `Main._Ready`, the exact source identifier is appended once
through `WorldEventLogController`; inability to record it fails explicitly. The source exposes no
controller access, and startup recording changes no world-object, perception, focused-search,
movement, input, or presentation state. Event payloads and time behavior remain absent.

The Domain also defines the closed `WorldEventTimeEffect` vocabulary as exact `no-advance` and
`advance` singleton values with stable ordering and strict canonical parsing. No event has an
assigned effect yet, and no progression behavior exists.

All three slices were test-first through focused Domain tests and dedicated opt-in Godot smokes.
After code review,
`./scripts/quality.sh` passed workflow and formatting validation, a zero-warning build, 960 Domain
tests, 107 Application tests, six architecture tests, all 24 opt-in Godot smokes, and the bounded
graphical launch.

## Exact next bounded slice

Take **Phase 2o9: immutable authoritative world event**.

Acceptance boundary:

1. Add one sealed, engine-independent Domain value that retains one existing `WorldEventId` and one
   existing `WorldEventTimeEffect` through get-only properties.
2. Reject null inputs, retain both exact supplied instances, use their existing value semantics,
   and expose no other public state or behavior. Add focused tests first.
3. Update README and ROADMAP evidence, keep logging and progression integration incomplete, run
   `./scripts/quality.sh`, review every intended path, and prune this file again.

Explicit exclusions:

- Do not change the identifier-only log or define payloads, actors, targets, timestamps, durations,
  clock units, phase names, progression thresholds, disease effects, replay, persistence,
  serialization, or migration.
- Do not connect time behavior to Godot, Application, movement, dialogue, discovery, characters,
  world objects, providers, credentials, or UI.
- Keep point-and-click movement, disease-time pausing, and the world-time debug overlay deferred.
- Do not merge, release, distribute, sign, notarize, or change repository visibility without
  explicit user authorization.
