# Allrpcs Handover

## Current state

Phase 2k13 is implemented and reviewed without findings on
`codex/phase-2k13-focused-search-activity-controller`, based directly on the PR #60 merge at
`73b0eb5`. The branch has not yet been published; record its commit and draft pull request here
after creation. Pull-request merge remains separately unauthorized.

`FocusedSearchActivityController` is the sole live owner of the existing Seeker's immutable
`FocusedSearchActivitySnapshot`. It initializes empty from the one authored `seeker` character
identifier, rejects requests for any other character, delegates eligibility and state changes to
the existing Domain policies and transitions, replaces its snapshot only after a successful
start, cancellation, or completion, and then presents through the existing indicator adapter.
Ordinary play constructs no request and invokes no lifecycle operation.

Host-shell `./scripts/quality.sh` passed with a zero-warning build, 480 Domain tests, 107
Application tests, six architecture tests, six credential-free Godot smokes, and the bounded
graphical launch. The new smoke proved empty initialization and hidden presentation; successful
start, cancellation, and completion; fail-closed unrelated or ineligible operations with the
same snapshot and activity instances; and unchanged request/identifier inputs. The sandboxed run
restored, formatted, and built successfully before VSTest was denied its local loopback bind. No
manual visual acceptance was performed; the cyan ring remains temporary greybox evidence. Hosted
desktop outputs remain unsigned development evidence, not releases.

## Exact next bounded slice

Take **Phase 2k14: focused-search combined eligibility policy**.

Acceptance boundary:

1. Add one small pure Domain `FocusedSearchEligibilityPolicy` operation that accepts an existing
   `FocusedSearchRequest`, `FocusedSearchActivitySnapshot`, `DiscoveryConditionSet`,
   `CharacterCapabilitySet`, and `CharacterInventory`.
2. Return true only when the existing `FocusedSearchStartPolicy` permits the request and the
   existing `DiscoveryConditionPolicy` is satisfied for the request's target object. Delegate both
   decisions; do not duplicate their matching rules or change either existing policy.
3. Reject null inputs, fail closed for absent or unsatisfied target conditions and an already-active
   searcher, ignore unrelated activities and requirements according to the existing delegates, and
   leave every supplied object unchanged.
4. Add focused tests for capability-only, tool-only, combined, absent, and unsatisfied conditions;
   empty, unrelated, and same-searcher activity; null inputs; request target delegation; and exact
   input instance/order retention.
5. Do not change the Godot controller, scene, indicator, smokes, or ordinary play in this slice.
   Document the pure policy in `README.md`, record Phase 2k14 in `ROADMAP.md`, leave the broader
   bounded-focused-search item incomplete, and replace this file with current evidence, limits,
   and one next bounded slice.
6. Use test-first development, run `./scripts/quality.sh`, review every intended path, and commit
   only the reviewed slice.

Explicit exclusions:

- Do not add or change lifecycle transitions, owned snapshots, search targets, player input,
  duration, progress, timing, outcomes, discovery grants, visibility, events, effects, reasons,
  presentation, or final UI/art.
- Do not inspect a complete character, perception snapshot, ownership/capacity snapshot, world
  state, object existence, location, coordinates, range, radius, interaction focus, or other UI
  state.
- Do not add passive noticing, reinterpretation, configurable Boolean expressions, alternative
  condition groups, scenario content, serialization, persistence, providers, or plugins.
- Keep point-and-click movement, disease-time pausing, and the world-time debug overlay deferred
  until their own retained prototype or authoritative-system prerequisites exist.
- Do not merge, release, distribute, sign, notarize, change repository visibility, or add
  credentials without explicit user authorization.
