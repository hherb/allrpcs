# Allrpcs Handover

## Current state

PR #129 merged Phase 2n20–2n25 into `main` at `c6f507b`. The continuation branch
`codex/phase-2o1-plus-world-events` implements Phases 2o1–2o5.

The Domain now owns an immutable ordered log of unique existing `WorldEventId` instances, exact
value-equal lookup, duplicate-free append eligibility, and pure append. Successful append returns a
new snapshot with the exact supplied identifier after every exact retained entry; rejection changes
nothing. A dedicated Godot controller owns an initially empty snapshot and delegates append without
changing world objects, perception, focused-search state, or other runtime behavior. Event payloads,
time, persistence, replay, and ordinary-play production remain absent.

All five slices were test-first. After code review, host-shell `./scripts/quality.sh` passed workflow
and formatting validation, a zero-warning build, 949 Domain tests, 107 Application tests, six
architecture tests, all 22 opt-in Godot smokes, and the bounded graphical launch.

## Exact next bounded slice

Take **Phase 2o6: temporary authoritative world-event identifier source**.

Acceptance boundary:

1. Add one `Allrpcs.Game.TemporaryWorldEventSource` node that creates and retains one stable
   existing `WorldEventId` for a single explicitly named greybox event.
2. Expose only that identifier through a get-only property; repeated access must return the same
   exact instance. Keep construction independent from the log controller.
3. Add one credential-free opt-in Godot smoke for exact stable retention, canonical authored text,
   minimal public surface, independent construction, and unchanged event log, world-object,
   perception, focused-search, movement, input, and presentation state.
4. Update README and ROADMAP evidence, keep production and startup append incomplete, run
   `./scripts/quality.sh`, review every intended path, and prune this file again to current evidence
   plus one next slice.

Explicit exclusions:

- Do not append the source identifier to the controller, connect `Main._Ready`, or define event
  payloads, kinds, actors, targets, timestamps, world time, ordering policy, replay, persistence,
  serialization, or migration.
- Do not connect events to discovery, movement, dialogue, characters, world objects, Application,
  providers, credentials, or UI.
- Keep point-and-click movement, disease-time pausing, and the world-time debug overlay deferred.
- Do not merge, release, distribute, sign, notarize, or change repository visibility without
  explicit user authorization.
