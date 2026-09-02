# Allrpcs Handover

## Current state

Phase 2h2 is implemented, reviewed, and locally validated on
`codex/phase-2h2-world-object-visibility`, based directly on the PR #29 merge at `046b46f`.
Draft PR #30 (`https://github.com/hherb/allrpcs/pull/30`) is open. Pull-request merge remains
separately authorized.

`WorldObjectVisibility` is a sealed immutable value association between exactly one existing
`ObjectId` and one existing `VisibilityState`. It rejects null inputs, retains both supplied
instances, exposes only get-only `ObjectId` and `VisibilityState` properties, and compares by
both values. It adds no default state, character assignment, or perception mechanics.

The test-first build failed only because the type did not exist. All four focused tests then
passed. The restricted full sequence produced a zero-warning build before VSTest hit its known
loopback restriction. The identical host-shell `./scripts/quality.sh` passed: 258 Domain tests,
100 Application tests, six architecture tests, four Godot smokes, and the bounded graphical
launch. No manual play acceptance is required for this engine-independent Domain slice. Hosted
desktop outputs remain unsigned development evidence, not releases.

## Exact next bounded slice

After this pull request is reported merged, take **Phase 2h3: immutable per-character perception
snapshot**.

Acceptance boundary:

1. Add a sealed immutable `CharacterPerceptionSnapshot` in `Allrpcs.Domain.Perception` that
   associates exactly one existing `CharacterId` with an ordered collection of existing
   `WorldObjectVisibility` entries.
2. Require and retain the supplied character identifier. Explicitly permit an empty entry
   sequence, copy caller input, preserve entry order, expose a read-only collection, and reject
   a null sequence, null entries, and duplicate `ObjectId` values. Add no implicit `unnoticed`
   entries, default state, or cross-entry policy.
3. Add focused pure tests for reference retention, empty behavior, ordering, caller-copy
   isolation, duplicate and null rejection, read-only exposure, and the deliberately limited
   public surface.
4. Preserve `Domain -> []`; update `README.md`, mark only the per-character perception-snapshot
   roadmap item complete, and replace this file with evidence, limits, and one next bounded slice.
5. Run `./scripts/quality.sh`, review every intended path, and commit only the reviewed slice.

Explicit exclusions:

- Do not build filtered character views or integrate the snapshot into `Character` yet.
- Do not add visibility progression, transition rules, discovery, reinterpretation,
  permissions, actions, events, or mutable state.
- Do not interpret `altered` or `collected` as inventory ownership or world-event outcomes.
- Do not add locations, evidence, environmental-state assignment, scenario content,
  serialization, persistence, Godot, UI, providers, or plugins.
- Keep point-and-click movement, disease-time pausing, and the world-time debug overlay deferred
  until their own retained prototype or authoritative-system prerequisites exist.
- Do not merge, release, distribute, sign, notarize, change repository visibility, or add
  credentials without explicit user authorization.
