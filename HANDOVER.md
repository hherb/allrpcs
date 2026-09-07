# Allrpcs Handover

## Current state

PR #132 merged Phases 2o9–2o18 into `main` at `3d92d2f`. The continuation branch
`codex/phase-2o19-plus-world-events` implements Phases 2o19–2o28.

The scene now owns an optional immutable authoritative `WorldEventSnapshot`, initialized once from
the temporary source's exact one-event snapshot. Lookup returns exact retained events for
value-equal identifiers. Startup resolves `greybox-ready` through that controller and the log
controller records it only against the authoritative definitions.

Pure Domain recording contracts require a defined, absent event and append its exact authoritative
identifier. Atomic occurrence contracts return new immutable log/time state: every event is
recorded, `no-advance` preserves the exact time snapshot, and `advance` retains the exact event ID as
qualitative time. Undefined, duplicate, and repeated-current events fail closed.

After code review, `./scripts/quality.sh` passed workflow and formatting validation, a zero-warning
build, 1,020 Domain tests, 107 Application tests, six architecture tests, all 26 opt-in Godot
smokes, and the bounded graphical launch.

## Exact next bounded slice

Take **Phase 2o29: atomic world-event occurrence controller orchestration**.

Acceptance boundary:

1. Add one minimal scene controller that accepts a `WorldEventId`, initialized authoritative events,
   current event log, and current world time; delegate eligibility and transition to the Phase
   2o26–2o28 Domain contracts.
2. On success, install both exact result snapshots together. On undefined, duplicate,
   repeated-current, uninitialized, or null input, return false or reject null and preserve every
   controller snapshot.
3. Route ordinary `greybox-ready` startup through that controller. Prove exact authoritative ID
   retention, unchanged initial qualitative time for `no-advance`, and unchanged source, world,
   discovery, movement, input, and presentation state in one opt-in Godot smoke.
4. Do not add an advancing runtime event yet. Update README and ROADMAP evidence, run
   `./scripts/quality.sh`, review intended paths, and prune this file again.

Explicit exclusions: no payloads, actors, targets, clocks, durations, phases, thresholds, disease
effects, replay, persistence, serialization, migration, Application/provider/credential/UI
integration, point-and-click movement, disease-time pausing, or debug overlay. Do not merge,
release, distribute, sign, notarize, or change repository visibility without explicit authorization.
