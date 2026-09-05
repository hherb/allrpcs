# Allrpcs Handover

## Current state

Phase 2k42 is implemented and reviewed without findings on
`codex/phase-2k42-outcome-controller-completion`, based directly on PR #89's verified merge commit
`8400113`. Slice commit `d6d1492` is pushed, and draft PR #90
(`https://github.com/hherb/allrpcs/pull/90`) is open. Pull-request merge remains separately
unauthorized.

`Allrpcs.Game.FocusedSearchActivityController` now owns immutable activity and outcome snapshots,
initializes both empty, and exposes their current values. Its new
`TryCompleteSearchWithOutcome` operation accepts an existing outcome only for the controller's
Seeker, delegates eligibility and transition to the existing Domain boundaries, replaces both
snapshots with the transition result's exact instances, and then presents the resulting activity
snapshot. Rejected calls preserve both snapshots and presentation. Existing start, cancellation,
and request-only completion paths preserve outcome state.

The test-first Game build failed with ten missing-member errors before implementation. Host-shell
`./scripts/quality.sh` passed with a zero-warning build, 631 Domain tests, 107 Application tests,
six architecture tests, all seven Godot smokes, and the bounded graphical launch. The sandboxed
quality attempt reached the clean build but remained inconclusive when VSTest could not open its
required loopback socket. No ordinary-play or visible behavior changed, so no separate manual
visual acceptance was needed. Hosted desktop outputs remain unsigned development evidence, not
releases.

## Exact next bounded slice

Take **Phase 2k43: temporary focused-search outcome source**.

Acceptance boundary:

1. Add one narrow Godot-layer source that accepts an existing `FocusedSearchRequest` and constructs
   a `FocusedSearchOutcome` retaining that exact request plus the exact existing
   `VisibilityState.Identified` singleton as one explicitly temporary deterministic result.
2. Reject a null request. Do not inspect focus, activity, outcomes, perception, or world state;
   decide eligibility; call a controller; or own lifecycle state in the source.
3. Add the source to the greybox scene and focused credential-free smoke coverage for exact input
   retention, the exact visibility-state instance, distinct result instances across repeated calls,
   null rejection, and unchanged controller snapshots and presentation.
4. Document Phase 2k43 in `README.md` and compact `ROADMAP.md`, leave the broader focused-search
   item incomplete, run `./scripts/quality.sh`, review every intended path, and replace this file
   with current evidence, limits, and one next bounded slice.

Explicit exclusions:

- Do not invoke the outcome source or outcome-aware completion from `Main` or ordinary play, add an
  input action, or remove or alter the existing request-only temporary completion path.
- Do not resolve or apply an outcome, change perception or visibility, grant discovery, consume an
  outcome, or add authoritative object, character, location, inventory, or persistence behavior.
- Do not add target-specific outcome rules, visibility ranking, coordinates, range policy, passive
  noticing, reinterpretation, serialization, providers, plugins, credentials, or final interaction
  UI.
- Keep point-and-click movement, disease-time pausing, and the world-time debug overlay deferred
  until their retained prototype or authoritative-system prerequisites exist.
- Do not merge, release, distribute, sign, notarize, or change repository visibility without
  explicit user authorization.
