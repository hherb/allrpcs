# Allrpcs Handover

## Current state

Phase 2k24 is implemented and reviewed without findings on
`codex/phase-2k24-focused-search-outcome`, based directly on PR #71's merge commit `522e8f2`.

`Allrpcs.Domain.Discovery.FocusedSearchOutcome` is one sealed immutable record retaining exactly
one supplied `FocusedSearchRequest` and one supplied existing `VisibilityState` through `Request`
and `ResultingVisibilityState`. It rejects either null input, preserves the exact supplied
instances, and uses record value equality. It does not validate or advance visibility, produce an
outcome, apply one to perception, or connect one to focused-search completion.

The focused tests failed first with `CS0246` while the type was absent, then all four passed after
the implementation. Host-shell `./scripts/quality.sh` passed with a zero-warning build, 495 Domain
tests, 107 Application tests, six architecture tests, all six Godot smokes, and the bounded
graphical launch. The sandboxed quality run reached its expected VSTest loopback restriction; the
identical host-shell quality spine passed. No Godot behavior changed, so no separate manual visual
acceptance was performed. Hosted desktop outputs remain unsigned development evidence, not
releases.

## Exact next bounded slice

Take **Phase 2k25: immutable focused-search outcome snapshot**.

Acceptance boundary:

1. Add one sealed immutable `Allrpcs.Domain.Discovery.FocusedSearchOutcomeSnapshot` containing a
   stable supplied sequence of existing `FocusedSearchOutcome` values as an `Entries`
   `IReadOnlyList<FocusedSearchOutcome>`.
2. Materialize caller input exactly once, copy it, preserve supplied order and exact outcome
   instances, permit an empty sequence, and expose no mutable collection surface.
3. Reject a null sequence, null entries, and entries whose requests are value-equal duplicates;
   do not compare, validate, rank, merge, or advance their resulting visibility states.
4. Add focused Domain tests for empty and populated snapshots, exact instance/order retention,
   one-shot enumeration, caller-mutation isolation, read-only exposure, invalid inputs, duplicate
   requests, and the intended get-only sealed API surface.
5. Do not produce outcomes, apply them to perception, connect them to completion, or alter the
   existing focused-search controller, temporary inputs, activity ring, or result label.
6. Document Phase 2k25 in `README.md` and compact `ROADMAP.md`, leave the broader focused-search
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
