# Allrpcs Handover

## Current state

Phase 2k28 is implemented and reviewed without findings on
`codex/phase-2k28-character-perception-lookup`, based directly on PR #75's merge commit
`3a7495a`. Commit, push, and draft-PR creation are authorized and pending.

`Allrpcs.Domain.Perception.CharacterPerceptionLookup.FindByObject` is one pure static operation
that accepts an existing `ObjectId` and `CharacterPerceptionSnapshot`. It matches the object
identifier by value and returns the exact retained `WorldObjectVisibility`, or `null`. It rejects
null inputs, relies on the snapshot's duplicate-object invariant, preserves the entry and all
nested instances, and leaves every input unchanged. It does not inspect a complete character,
compare or advance visibility, mutate perception, or produce or apply a focused-search outcome.

The seven focused tests failed first with `CS0103`/`CS0246` while the lookup type was absent, then
passed after the implementation. The sandboxed green rerun compiled but stalled at the known
VSTest loopback restriction; the identical host-shell focused test passed. Host-shell
`./scripts/quality.sh` passed with a zero-warning build, 524 Domain tests, 107 Application tests,
six architecture tests, all six Godot smokes, and the bounded graphical launch. No runtime Godot
behavior changed, so no separate manual visual acceptance was performed. Hosted desktop outputs
remain unsigned development evidence, not releases.

## Exact next bounded slice

Take **Phase 2k29: focused-search outcome applicability policy**.

Acceptance boundary:

1. Add one pure static
   `Allrpcs.Domain.Discovery.FocusedSearchOutcomeApplicabilityPolicy.CanApply` operation accepting
   an existing `FocusedSearchOutcome` and `CharacterPerceptionSnapshot` and returning a Boolean.
2. Return true only when the snapshot's character identifier is value-equal to the outcome
   request's searcher identifier and the existing `CharacterPerceptionLookup` finds the outcome
   request's target object in that snapshot; otherwise return false.
3. Delegate exact-object selection to `CharacterPerceptionLookup`, reject either null input,
   preserve all exact retained instances, and leave every input unchanged.
4. Add focused Domain tests for matching eligibility, wrong character, absent target, unrelated
   entries, value-equal distinct identifiers, unchanged inputs, null rejection, delegation-backed
   duplicate-invariant behavior, and the intended single-operation pure static API surface.
5. Do not compare current and resulting visibility states, decide a visibility transition, apply
   or mutate perception, consume an outcome, or connect the decision to focused-search completion.
6. Document Phase 2k29 in `README.md` and compact `ROADMAP.md`, leave the broader focused-search
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
