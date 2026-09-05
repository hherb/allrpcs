# Allrpcs Handover

## Current state

Phase 2k29 is implemented and reviewed without findings on
`codex/phase-2k29-outcome-applicability`, based directly on PR #76's merge commit
`29d480b`. Slice commit `3cec688` is pushed, and draft PR #77
(`https://github.com/hherb/allrpcs/pull/77`) is open. Pull-request merge remains separately
unauthorized.

`Allrpcs.Domain.Discovery.FocusedSearchOutcomeApplicabilityPolicy.CanApply` is one pure static
operation accepting an existing `FocusedSearchOutcome` and `CharacterPerceptionSnapshot`. It
returns true only when the snapshot's character identifier is value-equal to the outcome
request's searcher identifier and `CharacterPerceptionLookup.FindByObject` finds the request's
target object. It rejects null inputs, leaves every retained instance unchanged, and deliberately
does not inspect current or resulting visibility states, rank transitions, mutate perception,
consume the outcome, or connect it to focused-search completion.

The nine focused tests failed first with `CS0103`/`CS0246` while the policy type was absent. The
sandboxed green rerun compiled but stopped at the known VSTest loopback restriction; the identical
host-shell focused test passed. Host-shell `./scripts/quality.sh` passed with a zero-warning build,
533 Domain tests, 107 Application tests, six architecture tests, all six Godot smokes, and the
bounded graphical launch. No runtime Godot behavior changed, so no separate manual visual
acceptance was performed. Hosted desktop outputs remain unsigned development evidence, not
releases.

## Exact next bounded slice

Take **Phase 2k30: focused-search outcome change detection**.

Acceptance boundary:

1. Add one pure static `Allrpcs.Domain.Discovery.FocusedSearchOutcomeChangePolicy.WouldChange`
   operation accepting an existing `FocusedSearchOutcome` and `CharacterPerceptionSnapshot` and
   returning a Boolean.
2. Delegate applicability to `FocusedSearchOutcomeApplicabilityPolicy`; return false when the
   outcome is not applicable.
3. For an applicable outcome, delegate exact target selection to `CharacterPerceptionLookup` and
   return true only when the retained target entry's existing visibility state differs from the
   outcome's resulting visibility state; otherwise return false.
4. Add focused Domain tests for equal-state rejection, every distinct current/resulting state
   pair, wrong character, absent target, unrelated entries, value-equal distinct identifiers,
   unchanged inputs, null rejection, delegate behavior, and the intended single-operation pure
   static API surface.
5. Do not rank visibility states, decide whether a change is advancement or regression, apply or
   mutate perception, consume an outcome, or connect the decision to focused-search completion.
6. Document Phase 2k30 in `README.md` and compact `ROADMAP.md`, leave the broader focused-search
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
