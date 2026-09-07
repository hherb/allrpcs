# Allrpcs Handover

## Current state

Phases 2m9–2m14 are implemented and reviewed without findings on
`codex/phase-2m9-plus-passive-noticing`, based directly on PR #121's merge commit `1356351`.

The pure Domain passive-noticing chain now removes one complete retained candidate, combines
perception-change and consumption eligibility, returns both resulting snapshots from one atomic
resolution, and records new candidates without duplicates. Resolution changes only an applicable
target from `Unnoticed` to `Noticed` and removes only its value-equal complete candidate. Recording
allows both partial observer/target matches, rejects a complete duplicate, and appends the exact
supplied candidate. Every operation preserves stable order, exact unrelated instances and nested
identifiers, and all original inputs.

All six slices were test-first: each focused suite failed with `CS0103`/`CS0246` while its new type
was absent and passed after the minimal implementation. The final host-shell `./scripts/quality.sh`
passed workflow and formatting validation, a zero-warning build, 770 Domain tests, 107 Application
tests, six architecture tests, all fourteen Godot smokes, and the bounded graphical launch.

PR #122's initial hosted headless job exposed a navigation-startup race in the existing
interaction-focus smoke: map iteration counters could advance before the authored healer move
destination resolved onto the map, so the synthetic move input was intermittently rejected. The
failure reproduced locally on pre-fix stress run 3. The smoke now gates startup on that exact
destination using the healer's existing one-pixel surface tolerance; 100 consecutive isolated
post-fix launches and the complete quality sequence passed.

README and ROADMAP record Phases 2m9–2m14. Passive noticing remains incomplete: no candidate is yet
bound to an authoritative existing character and world object, and candidates are not selected or
connected to Application or Godot. No runtime behavior changed.

## Exact next bounded slice

Take **Phase 2m15: authoritative passive-noticing production eligibility**.

Acceptance boundary:

1. Add one pure static
   `Allrpcs.Domain.Discovery.PassiveNoticingCandidateProductionPolicy.CanProduce` operation accepting
   an existing `PassiveNoticingCandidate`, `Character`, `WorldObject`, and
   `PassiveNoticingCandidateSnapshot`, returning only a Boolean.
2. Require the supplied character identity to match the candidate observer and the supplied world
   object identity to match the candidate target by value, then delegate duplicate-free eligibility
   to `PassiveNoticingCandidateRecordingPolicy`.
3. Accept distinct value-equal character, object, and candidate identifier instances while retaining
   every supplied instance unchanged. Reject either identity mismatch, a complete retained
   candidate, and every null input without changing any input.
4. Add focused Domain tests first for exact and distinct value-equal authority, both identity
   mismatches, both partial candidate matches, complete-duplicate rejection, null boundaries,
   unchanged nested inputs, and the minimal one-method static public surface.
5. Update README and ROADMAP evidence, keep passive noticing incomplete, run
   `./scripts/quality.sh`, review every intended path, and prune this file again to current evidence
   plus one next slice.

Explicit exclusions:

- Do not create or record a candidate, change perception, consume or resolve candidates, select,
  rank, reorder, or evaluate candidates, or inspect character details, world-object descriptions,
  complete world-object snapshots, locations, coordinates, distance, line of sight, environmental
  state, capabilities, inventory, focus, input, or time.
- Do not grant discovery, create knowledge or evidence, or add Application/Godot sources,
  controllers, runtime wiring, events, persistence, serialization, providers, plugins,
  credentials, or UI.
- Keep point-and-click movement, disease-time pausing, and the world-time debug overlay deferred.
- Do not merge, release, distribute, sign, notarize, or change repository visibility without
  explicit user authorization.
