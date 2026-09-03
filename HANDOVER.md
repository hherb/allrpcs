# Allrpcs Handover

## Current state

Phase 2k14 is implemented and reviewed without findings on
`codex/phase-2k14-focused-search-eligibility-policy`, based directly on the PR #61 merge at
`c49511c`.

`FocusedSearchEligibilityPolicy.CanStartSearch` is one pure Domain decision over an existing
request, activity snapshot, discovery-condition set, capability set, and inventory. It rejects
null inputs and returns true only when the existing `FocusedSearchStartPolicy` permits the request
and the existing `DiscoveryConditionPolicy` is satisfied for the request's target object. It
inherits both delegates' fail-closed and unrelated-entry behavior and changes no supplied object.
The existing controller, scene, lifecycle transitions, indicator, smokes, and ordinary play are
unchanged.

Host-shell `./scripts/quality.sh` passed with a zero-warning build, 491 Domain tests, 107
Application tests, six architecture tests, six credential-free Godot smokes, and the bounded
graphical launch. Eleven focused tests cover capability-only, tool-only, combined, absent, and
unsatisfied conditions; empty, unrelated, and same-searcher activity; null inputs; request-target
delegation by value; the single static API; and exact instance/order retention without mutation.
The sandboxed run restored, formatted, and built successfully before VSTest was denied its local
loopback bind. No manual visual acceptance was needed because this slice changes no presentation.
Hosted desktop outputs remain unsigned development evidence, not releases.

## Exact next bounded slice

Take **Phase 2k15: condition-aware focused-search controller start**.

Acceptance boundary:

1. Update only the existing Godot `FocusedSearchActivityController` start path to accept an
   existing `DiscoveryConditionSet`, `CharacterCapabilitySet`, and `CharacterInventory` with the
   existing request and owned activity snapshot.
2. Keep the existing owned-character rejection, then delegate the complete start decision to
   `FocusedSearchEligibilityPolicy`; only an eligible request may reach the existing
   `FocusedSearchStartTransition`, replace the owned snapshot, and trigger presentation.
3. Fail closed for absent or unsatisfied request-target conditions and an already-active Seeker;
   ignore unrelated activities and requirements according to the existing delegates; preserve
   every supplied object and the controller's prior snapshot when rejected.
4. Extend the credential-free controller smoke for capability-only, tool-only, combined, absent,
   unsatisfied, unrelated, already-active, and wrong-character starts; successful lifecycle
   continuation; presentation calls; and exact input/snapshot instance retention.
5. Do not change the scene, indicator adapter, Domain policies or transitions, cancellation or
   completion signatures, or ordinary-play behavior in this slice. Document the controller gate
   in `README.md`, record Phase 2k15 in `ROADMAP.md`, leave the broader bounded-focused-search item
   incomplete, and replace this file with current evidence, limits, and one next bounded slice.
6. Use test-first development, run `./scripts/quality.sh`, review every intended path, and commit
   only the reviewed slice.

Explicit exclusions:

- Do not add player input, search-target selection, duration, progress, timing, outcomes,
  discovery grants, visibility changes, events, effects, reason values, or final UI/art.
- Do not inspect a complete character, perception snapshot, ownership/capacity snapshot, world
  state, object existence, location, coordinates, range, radius, interaction focus, or other UI
  state.
- Do not add passive noticing, reinterpretation, configurable Boolean expressions, alternative
  condition groups, scenario content, serialization, persistence, providers, or plugins.
- Keep point-and-click movement, disease-time pausing, and the world-time debug overlay deferred
  until their own retained prototype or authoritative-system prerequisites exist.
- Do not merge, release, distribute, sign, notarize, change repository visibility, or add
  credentials without explicit user authorization.
