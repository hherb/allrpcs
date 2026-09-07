# Allrpcs Handover

## Current state

PR #131 merged Phases 2o6–2o8 into `main` at `08125c7`. The continuation branch
`codex/phase-2o9-plus-authoritative-events` implements Phases 2o9–2o18.

The Domain now has immutable authoritative events pairing exact existing `WorldEventId` and
`WorldEventTimeEffect` instances, ordered duplicate-ID-free snapshots, and exact value-equal
lookup. The identifier log resolves deterministically to exact event instances in log order and
fails closed when a recorded identifier lacks a definition.

Qualitative world time is represented only by the exact latest advancing event identifier, with
null as the explicit initial state. Pure eligibility and transition contracts reject `no-advance`
events and value-equal repeats; deterministic projection reconstructs the same state from ordered
events. No clock, duration, phase, threshold, or disease progression exists.

The temporary source now retains the full `greybox-ready`/`no-advance` event while startup continues
to append its exact identifier once. A separate `WorldTimeController` owns initial qualitative time
and delegates eligible advances without changing the event log, source, world objects, perception,
focused search, movement, input, or presentation. Ordinary play does not advance time.

All ten slices were test-first through focused Domain tests or a dedicated opt-in Godot smoke.
After code review, `./scripts/quality.sh` passed workflow and formatting validation, a zero-warning
build, 999 Domain tests, 107 Application tests, six architecture tests, all 25 opt-in Godot smokes,
and the bounded graphical launch.

## Exact next bounded slice

Take **Phase 2o19: authoritative world-event controller ownership and lookup**.

Acceptance boundary:

1. Add one `Allrpcs.Game.WorldEventController` node with an optional get-only
   `WorldEventSnapshot`, one-time exact snapshot initialization, and `WorldEventId` lookup that
   delegates to `WorldEventLookup` only after initialization.
2. Repeated initialization returns false and preserves the exact first snapshot; lookup returns
   the exact retained event for exact or distinct value-equal identifiers, and returns null before
   initialization or for absent identifiers. Reject null inputs.
3. Add one credential-free opt-in Godot smoke for the complete minimal controller surface and
   unchanged source, identifier log, world time, world objects, perception, focused-search,
   movement, input, and presentation state.
4. Do not initialize the controller from the temporary source or change startup recording yet.
   Update README and ROADMAP evidence, run `./scripts/quality.sh`, review every intended path, and
   prune this file again.

Explicit exclusions:

- Do not change the identifier log, advance world time, add event production or processing, or
  define payloads, actors, targets, timestamps, durations, phase names, thresholds, disease
  effects, replay, persistence, serialization, or migration.
- Do not connect events or time to Application, movement, dialogue, discovery, characters, world
  objects, providers, credentials, or UI.
- Keep point-and-click movement, disease-time pausing, and the world-time debug overlay deferred.
- Do not merge, release, distribute, sign, notarize, or change repository visibility without
  explicit user authorization.
