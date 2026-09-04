# Allrpcs Handover

## Current state

Phase 2k16 is implemented and reviewed without findings on
`codex/phase-2k16-stable-focused-search-target-bridge`, based directly on the PR #63 merge at
`47413eb`. The reviewed commit and draft pull request are pending creation.

Each of the four existing `GreyboxInteractionTarget` nodes now authors distinct canonical object-ID
text. The node parses that text once through the Domain `ObjectId` boundary while ready and retains
the exact identifier instance. `InteractionFocusController.FocusedTargetObjectId` exposes that
instance only for the existing visible, enabled, in-range selection and clears it with focus. Focus
range, order, geometry, labels, presentation, scene layout, and ordinary input are unchanged. No
focused-search request is constructed and no lifecycle operation is invoked.

Host-shell `./scripts/quality.sh` passed with a zero-warning build, 491 Domain tests, 107
Application tests, six architecture tests, six credential-free Godot smokes, and the bounded
graphical launch. The extended interaction-focus smoke covers the four distinct authored IDs,
nearest selection, authored-order ties, focus changes, out-of-range clearing, hidden and disabled
exclusion, exact target-ID instance retention, and absence of an ID without valid focus. The
sandboxed sequence restored, formatted, and built successfully before VSTest was denied its local
loopback bind. No manual visual acceptance was needed because presentation behavior is unchanged.
Hosted desktop outputs remain unsigned development evidence, not releases.

## Exact next bounded slice

Take **Phase 2k17: focused-search request bridge**.

Acceptance boundary:

1. Extend only the existing `InteractionFocusController` with one request-construction boundary
   that accepts an existing `CharacterId` and returns a new `FocusedSearchRequest` only when
   `FocusedTargetObjectId` is present.
2. Retain the exact supplied character-ID and currently focused object-ID instances in each
   constructed request. Return no request when focus is absent, hidden, disabled, or out of range.
3. Extend the existing credential-free interaction-focus smoke to supply the exact existing
   `FocusedSearchActivityController.SearcherCharacterId`, cover nearest selection, authored-order
   ties, focus changes, clearing, hidden and disabled exclusion, and prove exact identifier-instance
   retention without changing focus presentation.
4. Do not invoke `FocusedSearchActivityController.TryStartSearch`, add search input, author or
   synthesize discovery conditions, capabilities, or inventory, or change lifecycle state. Do not
   change focus range, ordering, geometry, labels, scene layout, or ordinary input behavior.
5. Document the request bridge in `README.md`, record Phase 2k17 in `ROADMAP.md`, leave the broader
   bounded-focused-search item incomplete, and replace this file with current evidence, limits,
   and one next bounded slice.
6. Use test-first development, run `./scripts/quality.sh`, review every intended path, and commit
   only the reviewed slice.

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
