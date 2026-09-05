# Allrpcs Handover

## Current state

Phase 2k44 is implemented on `codex/phase-2k44-outcome-aware-completion-input`, based directly on
PR #91's verified merge commit `07cdc183`. `Main` now resolves the existing scene outcome source
once and, only for a pressed E action with an active Seeker request, creates one temporary outcome
from that exact request and passes it to
`FocusedSearchActivityController.TryCompleteSearchWithOutcome`.

Slice commit `658b569` is pushed, and draft PR #92
(`https://github.com/hherb/allrpcs/pull/92`) is open. Pull-request merge remains separately
unauthorized.

Successful E completion removes the activity, records the exact new outcome with the exact
`VisibilityState.Identified` singleton, hides the indicator, and retains the existing Boolean-derived
result text. No-active and repeated completion preserve both immutable controller snapshots.
Existing F start, C cancellation, healer, movement, released, unrelated, and handled-input paths are
unchanged. No outcome is resolved, applied, or consumed, and perception does not change.

The test-first interaction-focus smoke failed at exact outcome recording before the `Main` change.
After implementation and review, host-shell `./scripts/quality.sh` passed with a zero-warning build,
631 Domain tests, 107 Application tests, six architecture tests, all eight Godot smokes, and the
bounded graphical launch. The focused smoke covers exact outcome append, prior-outcome retention,
activity removal, indicator hiding, result text, rejection identity, and unchanged existing inputs.
No separate manual visual acceptance was needed because visible behavior did not change. Hosted
desktop outputs remain unsigned development evidence, not releases.

## Exact next bounded slice

Take **Phase 2k45: temporary Seeker perception source**.

Acceptance boundary:

1. Add one narrow scene-owned source exposing a single immutable `CharacterPerceptionSnapshot` for
   the existing Seeker. Retain the controller's exact authored `CharacterId` and the visible herb
   patch and stone marker's exact parsed `ObjectId` instances in authored order, each initially
   paired with the exact `VisibilityState.Noticed` singleton.
2. Construct the snapshot once during scene readiness and return that same snapshot instance on
   repeated access. Do not read focus or lifecycle state, create an outcome, call a controller, or
   make the temporary markers authoritative world objects.
3. Add the source to the greybox scene and focused credential-free smoke coverage for exact nested
   instance and order retention, stable repeated access, the intended two-entry boundary, and
   unchanged activity/outcome snapshots and presentation.
4. Document Phase 2k45 compactly, leave the broader focused-search item incomplete, run
   `./scripts/quality.sh`, review every intended path, and prune both planning documents to current
   evidence and one exact next slice.

Explicit exclusions:

- Do not resolve, apply, or consume a recorded outcome; change controller-owned perception or
  visibility; grant discovery; or connect this source to `Main` or any lifecycle input.
- Do not add target-specific outcome rules, visibility ranking, coordinates, range policy, passive
  noticing, reinterpretation, authoritative objects or characters, persistence, serialization,
  providers, plugins, credentials, failure-reason presentation, or final interaction UI.
- Keep point-and-click movement, disease-time pausing, and the world-time debug overlay deferred
  until their retained prototype or authoritative-system prerequisites exist.
- Do not merge, release, distribute, sign, notarize, or change repository visibility without
  explicit user authorization.
