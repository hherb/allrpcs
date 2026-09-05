# Allrpcs Handover

## Current state

Phase 2k26 is implemented and reviewed without findings on
`codex/phase-2k26-focused-search-outcome-projection`, based directly on PR #73's merge commit
`a9d2439`. Feature commit `5c18a80` is pushed, and draft PR #74
(`https://github.com/hherb/allrpcs/pull/74`) is open. Pull-request merge remains separately
unauthorized.

`Allrpcs.Domain.Discovery.FocusedSearchOutcomeProjection.ForCharacter` is one pure static
operation that accepts an existing `CharacterId` and `FocusedSearchOutcomeSnapshot` and returns a
new snapshot containing exactly the outcomes whose requests have a value-equal searcher character
identifier. It preserves exact outcome, request, and resulting-visibility-state instances in
stable relative order, permits empty and multiple-match results, ignores unrelated outcomes,
rejects null inputs, and leaves every input unchanged. It does not produce or apply outcomes,
compare or advance visibility states, change perception, or connect outcomes to completion.

The focused tests failed first with `CS0103`/`CS0246` while the projection type was absent, then
all seven passed after the implementation. Host-shell `./scripts/quality.sh` passed with a
zero-warning build, 510 Domain tests, 107 Application tests, six architecture tests, all six Godot
smokes, and the bounded graphical launch. The sandboxed focused rerun compiled but stalled at the
known VSTest loopback restriction; the identical host-shell focused test and complete quality spine
passed. No runtime Godot behavior changed, so no separate manual visual acceptance was performed.
Hosted desktop outputs remain unsigned development evidence, not releases.

## Exact next bounded slice

Take **Phase 2k27: exact-request focused-search outcome lookup**.

Acceptance boundary:

1. Add one pure static `Allrpcs.Domain.Discovery.FocusedSearchOutcomeLookup.FindByRequest`
   operation accepting an existing `FocusedSearchRequest` and `FocusedSearchOutcomeSnapshot` and
   returning the exact matching `FocusedSearchOutcome`, or `null` when none exists.
2. Match the complete request by value, including both searcher character and target object;
   reject same-searcher/different-target and different-searcher/same-target outcomes, and ignore
   unrelated outcomes.
3. Preserve the exact retained outcome, request, identifier, target, and visibility-state
   instances; reject either null input and leave every input unchanged. Rely on the snapshot's
   duplicate-request invariant rather than adding ambiguity handling or a new collection.
4. Add focused Domain tests for empty and unrelated snapshots, a value-equal distinct request,
   both partial mismatch cases, an exact match among surrounding unrelated entries, exact instance
   retention, unchanged inputs, null rejection, and the intended single-operation pure static API
   surface.
5. Do not produce outcomes, apply them to perception, connect them to completion, compare or
   advance visibility states, or alter the existing focused-search controller, temporary inputs,
   activity ring, or result label.
6. Document Phase 2k27 in `README.md` and compact `ROADMAP.md`, leave the broader focused-search
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
