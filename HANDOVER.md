# Allrpcs Handover

## Current state

Phase 2k43 is implemented and reviewed without findings on
`codex/phase-2k43-temporary-outcome-source`, based directly on PR #90's verified merge commit
`faabe0c`. The reviewed slice commit and draft pull request remain to be recorded below; merging
remains separately unauthorized.

`Allrpcs.Game.TemporaryFocusedSearchOutcomeSource` is a narrow scene-owned source whose
`CreateOutcome` method accepts an existing request and returns a distinct outcome retaining that
exact request and the exact `VisibilityState.Identified` singleton. It rejects null and has no
focus, controller, snapshot, perception, world-state, or lifecycle dependency. The source is not
invoked from `Main`, and ordinary play retains the request-only temporary completion path.

The test-first Game build failed with the expected missing-source type before implementation.
Host-shell `./scripts/quality.sh` passed with a zero-warning build, 631 Domain tests, 107 Application
tests, six architecture tests, all eight Godot smokes, and the bounded graphical launch. The
sandboxed quality attempt reached the zero-warning build but remained inconclusive when VSTest
could not open its required loopback socket. The focused smoke proves exact input and singleton
retention, distinct repeated results, null rejection, and unchanged controller snapshots and
indicator presentation. No ordinary-play or visible behavior changed, so no separate manual visual
acceptance was needed. Hosted desktop outputs remain unsigned development evidence, not releases.

## Exact next bounded slice

Take **Phase 2k44: temporary outcome-aware completion input**.

Acceptance boundary:

1. Give `Main` the existing scene outcome source and, on the existing pressed E action with an
   active Seeker request, create one temporary outcome from that exact request and pass that exact
   outcome to `FocusedSearchActivityController.TryCompleteSearchWithOutcome`.
2. Preserve no-active-search and repeated-completion behavior, handled-input behavior, the existing
   Boolean-derived result text, F start, C cancellation, healer inputs, and released or unrelated
   input behavior. Do not add or remap an input action.
3. Extend focused credential-free smoke coverage to prove exact outcome recording, completed
   activity removal, indicator hiding, result-text changes, rejection preservation, and unchanged
   existing input paths.
4. Document Phase 2k44 compactly, leave the broader focused-search item incomplete, run
   `./scripts/quality.sh`, review every intended path, and prune this file to current evidence,
   limits, and one exact next slice.

Explicit exclusions:

- Do not resolve, apply, or consume the recorded outcome; change perception or visibility; grant
  discovery; or add automatic progress, duration, completion, polling, or lifecycle behavior.
- Do not add target-specific result rules, visibility ranking, coordinates, range policy, passive
  noticing, reinterpretation, authoritative objects or characters, persistence, serialization,
  providers, plugins, credentials, failure-reason presentation, or final interaction UI.
- Keep point-and-click movement, disease-time pausing, and the world-time debug overlay deferred
  until their retained prototype or authoritative-system prerequisites exist.
- Do not merge, release, distribute, sign, notarize, or change repository visibility without
  explicit user authorization.
