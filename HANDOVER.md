# Allrpcs Handover

## Current state

Phase 2k17 is implemented and reviewed without findings on
`codex/phase-2k17-focused-search-request-bridge`, based directly on the PR #64 merge at
`27a72aa`. Feature commit `09fa075` is pushed, and draft PR #65
(`https://github.com/hherb/allrpcs/pull/65`) is open. Pull-request merge remains separately
unauthorized.

`InteractionFocusController.CreateFocusedSearchRequest` accepts an existing `CharacterId` and
returns a new `FocusedSearchRequest` only while an existing visible, enabled, in-range target has
focus. The request retains the exact supplied character-ID and focused object-ID instances. Cleared,
hidden, disabled, and out-of-range focus returns no request. The bridge does not invoke
`FocusedSearchActivityController`, alter focus presentation, or change lifecycle state.

Host-shell `./scripts/quality.sh` passed with a zero-warning build, 491 Domain tests, 107
Application tests, six architecture tests, six credential-free Godot smokes, and the bounded
graphical launch. The interaction-focus smoke covers nearest selection, authored-order ties, focus
changes, clearing, hidden and disabled exclusion, exact identifier-instance retention in every
constructed request, and absence of a request without valid focus. The test-first Game build failed
only for the deliberately missing factory before implementation. No manual visual acceptance was
needed because presentation behavior is unchanged. Hosted desktop outputs remain unsigned
development evidence, not releases.

## Exact next bounded slice

Take **Phase 2k18: focused-search start bridge**.

Acceptance boundary:

1. Add one narrow Godot-layer start-attempt boundary that composes the existing
   `InteractionFocusController.CreateFocusedSearchRequest` with the existing
   `FocusedSearchActivityController.TryStartSearch`.
2. Supply the activity controller's exact `SearcherCharacterId` to the request factory. Accept
   existing `DiscoveryConditionSet`, `CharacterCapabilitySet`, and `CharacterInventory` inputs and
   pass those exact instances to the existing condition-aware start path.
3. Return false without invoking the start path when focus is absent, hidden, disabled, or out of
   range. For valid focus, return the existing start path's result without adding alternate
   eligibility or transition logic.
4. Extend only the credential-free focused-search/focus smoke coverage needed to prove eligible and
   ineligible composition, exact request identifier retention, unchanged inputs, failure-state
   identity, and the existing activity-indicator presentation.
5. Do not add ordinary search input or author/synthesize production discovery conditions,
   capabilities, or inventory. Do not integrate cancellation or completion with interaction focus.
6. Document Phase 2k18 in `README.md` and `ROADMAP.md`, leave the broader bounded-focused-search item
   incomplete, run `./scripts/quality.sh`, review every intended path, and replace this file with
   current evidence, limits, and one next bounded slice.

Explicit exclusions:

- Do not add search duration, progress, timing, outcomes, discovery grants, visibility changes,
  events, effects, reason values, or final UI/art.
- Do not add complete-character, perception, ownership/capacity, or world-state inspection;
  object-existence validation; location, coordinate, radius, or interaction-range policy; or
  scenario object aggregates.
- Do not add passive noticing, reinterpretation, configurable Boolean expressions, alternative
  condition groups, serialization, persistence, providers, plugins, or credentials.
- Keep point-and-click movement, disease-time pausing, and the world-time debug overlay deferred
  until their retained prototype or authoritative-system prerequisites exist.
- Do not merge, release, distribute, sign, notarize, or change repository visibility without
  explicit user authorization.
