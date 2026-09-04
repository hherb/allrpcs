# Allrpcs Handover

## Current state

Phase 2k25 is implemented and reviewed without findings on
`codex/phase-2k25-focused-search-outcome-snapshot`, based directly on PR #72's merge commit
`f401900`.

`Allrpcs.Domain.Discovery.FocusedSearchOutcomeSnapshot` is one sealed immutable container for a
stable supplied sequence of existing `FocusedSearchOutcome` values. It materializes caller input
exactly once, copies it, preserves supplied order and exact outcome instances, permits an empty
sequence, and exposes only a get-only `IReadOnlyList<FocusedSearchOutcome>`. It rejects a null
sequence, null entries, and entries whose requests are value-equal duplicates. It does not compare
or advance resulting visibility states, produce outcomes, apply them to perception, or connect
them to focused-search completion.

The focused tests failed first with `CS0246` while the snapshot type was absent, then all eight
passed after the implementation. Host-shell `./scripts/quality.sh` passed with a zero-warning
build, 503 Domain tests, 107 Application tests, six architecture tests, all six Godot smokes, and
the bounded graphical launch. The sandboxed focused test run reached the known VSTest loopback
restriction after compilation; the identical host-shell focused test and complete quality spine
passed. No runtime Godot behavior changed, so no separate manual visual acceptance was performed.
Hosted desktop outputs remain unsigned development evidence, not releases.

## Exact next bounded slice

Take **Phase 2k26: focused-search outcome projection by character**.

Acceptance boundary:

1. Add one pure static `Allrpcs.Domain.Discovery.FocusedSearchOutcomeProjection.ForCharacter`
   operation accepting an existing `CharacterId` and `FocusedSearchOutcomeSnapshot` and returning
   a new `FocusedSearchOutcomeSnapshot`.
2. Include exactly the outcomes whose request has a value-equal `SearcherCharacterId`; preserve
   their existing instances and stable relative order, permit no matches and multiple matches,
   and ignore unrelated outcomes.
3. Reject either null input and leave the supplied identifier, snapshot, requests, outcomes, and
   visibility-state instances unchanged.
4. Add focused Domain tests for empty and unrelated inputs, value-equal identifier matching,
   multiple stable matches around unrelated entries, exact instance retention, unchanged inputs,
   null rejection, and the intended single-operation pure static API surface.
5. Do not produce outcomes, apply them to perception, connect them to completion, compare or
   advance visibility states, or alter the existing focused-search controller, temporary inputs,
   activity ring, or result label.
6. Document Phase 2k26 in `README.md` and compact `ROADMAP.md`, leave the broader focused-search
   item incomplete, run `./scripts/quality.sh`, review every intended path, and replace this file
   with current evidence, limits, and one next bounded slice.

Explicit exclusions:

- Do not add outcome production or application, discovery grants, visibility mutation or
  advancement policy, events, effects, reason/error taxonomies, progress, duration, automatic
  lifecycle behavior, authoritative world objects, or new Godot/Application behavior.
- Do not add complete-character inspection, ownership/capacity, world state, location,
  coordinates, range policy, passive noticing, reinterpretation, serialization, persistence,
  providers, plugins, credentials, or final interaction UI.
- Keep point-and-click movement, disease-time pausing, and the world-time debug overlay deferred
  until their retained prototype or authoritative-system prerequisites exist.
- Do not merge, release, distribute, sign, notarize, or change repository visibility without
  explicit user authorization.
