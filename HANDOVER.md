# Allrpcs Handover

## Current state

Phase 2k45 is implemented on `codex/phase-2k45-temporary-perception-source`, based directly on
PR #92's verified merge commit `7b3795f`.

The greybox scene now owns one `TemporarySeekerPerceptionSource`. During scene readiness it creates
one immutable `CharacterPerceptionSnapshot` retaining the focused-search controller's exact parsed
Seeker `CharacterId`. Its only entries retain the herb patch and stone marker's exact parsed
`ObjectId` instances in authored order, each paired with the exact `VisibilityState.Noticed`
singleton. Repeated access returns the same snapshot instance.

The source does not read focus or lifecycle state, create an outcome, call a controller, change
perception or visibility, or make the markers authoritative world objects. It is not connected to
`Main` or ordinary play. Activity and outcome snapshots and presentation remain unchanged.

The test-first game build failed with `CS0246` before the source existed. After implementation and
review, host-shell `./scripts/quality.sh` passed with a zero-warning build, 631 Domain tests, 107
Application tests, six architecture tests, all nine Godot smokes, and the bounded graphical launch.
The sandbox-only VSTest failure was `SocketException (13): Permission denied` while binding its
loopback transport. No separate manual visual acceptance was needed because visible behavior did
not change. Hosted desktop outputs remain unsigned development evidence, not releases.

## Exact next bounded slice

Take **Phase 2k46: controller-owned Seeker perception initialization**.

Acceptance boundary:

1. Extend `FocusedSearchActivityController` with one immutable perception snapshot property and one
   narrow one-time initialization boundary accepting an existing `CharacterPerceptionSnapshot`.
2. Accept only a snapshot for the controller's exact Seeker identifier and only before perception
   has already been initialized. Retain the exact supplied snapshot instance. Wrong-character and
   repeated initialization must fail closed while preserving controller state and presentation.
3. During `Main._Ready`, pass the temporary perception source's exact retained snapshot to that
   initialization boundary once. Do not resolve an outcome or add input.
4. Add focused credential-free smoke coverage for exact instance retention, one-time and
   wrong-character rejection, scene wiring, and unchanged activity/outcome snapshots and
   presentation.
5. Document Phase 2k46 compactly, leave the broader focused-search item incomplete, run
   `./scripts/quality.sh`, review every intended path, and prune both planning documents to current
   evidence and one exact next slice.

Explicit exclusions:

- Do not resolve, apply, or consume an outcome; change perception or visibility; grant discovery;
  or add lifecycle input, automatic processing, failure-reason presentation, or final interaction
  UI.
- Do not add target-specific outcome rules, visibility ranking, coordinates, range policy, passive
  noticing, reinterpretation, authoritative objects or characters, persistence, serialization,
  providers, plugins, or credentials.
- Keep point-and-click movement, disease-time pausing, and the world-time debug overlay deferred
  until their retained prototype or authoritative-system prerequisites exist.
- Do not merge, release, distribute, sign, notarize, or change repository visibility without
  explicit user authorization.
