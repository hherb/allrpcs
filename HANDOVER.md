# Allrpcs Handover

## Current state

Phase 2k27 is implemented and reviewed without findings on
`codex/phase-2k27-focused-search-outcome-lookup`, based directly on PR #74's merge commit
`27fe3c0`. Feature commit `16e4f34` is pushed, and draft PR #75
(`https://github.com/hherb/allrpcs/pull/75`) is open. Pull-request merge remains separately
unauthorized.

`Allrpcs.Domain.Discovery.FocusedSearchOutcomeLookup.FindByRequest` is one pure static operation
that accepts an existing `FocusedSearchRequest` and `FocusedSearchOutcomeSnapshot`. It matches the
complete request by value, including searcher character and target object, and returns the exact
retained `FocusedSearchOutcome` or `null`. It rejects null inputs, relies on the snapshot's
duplicate-request invariant, preserves exact nested instances, and leaves every input unchanged.
It does not produce or apply outcomes, compare or advance visibility states, change perception,
or connect outcomes to completion.

The focused tests failed first with `CS0103`/`CS0246` while the lookup type was absent, then all
seven passed after the implementation and one nullable-test assertion correction. Host-shell
`./scripts/quality.sh` passed with a zero-warning build, 517 Domain tests, 107 Application tests,
six architecture tests, all six Godot smokes, and the bounded graphical launch. The sandboxed
focused rerun compiled but stalled at the known VSTest loopback restriction; the identical
host-shell focused test and complete quality spine passed. No runtime Godot behavior changed, so
no separate manual visual acceptance was performed. Hosted desktop outputs remain unsigned
development evidence, not releases.

## Exact next bounded slice

Take **Phase 2k28: exact-object character-perception lookup**.

Acceptance boundary:

1. Add one pure static `Allrpcs.Domain.Perception.CharacterPerceptionLookup.FindByObject`
   operation accepting an existing `ObjectId` and `CharacterPerceptionSnapshot` and returning the
   exact matching `WorldObjectVisibility`, or `null` when none exists.
2. Match the object identifier by value, ignore unrelated entries, preserve the exact retained
   visibility entry, object-identifier, visibility-state, and snapshot-character instances, and
   leave every input unchanged.
3. Reject either null input and rely on the snapshot's duplicate-object invariant rather than
   adding ambiguity handling, a new collection, or complete-character inspection.
4. Add focused Domain tests for empty and unrelated snapshots, a value-equal distinct identifier,
   an exact match among surrounding unrelated entries, exact instance retention, unchanged inputs,
   null rejection, and the intended single-operation pure static API surface.
5. Do not consume a focused-search outcome, apply or mutate perception, compare or advance
   visibility states, connect outcomes to completion, or alter the existing focused-search
   controller, temporary inputs, activity ring, or result label.
6. Document Phase 2k28 in `README.md` and compact `ROADMAP.md`, leave the broader focused-search
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
