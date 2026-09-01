# Allrpcs Handover

## Current state

PR #21 merged Phase 2c10 into `origin/main` at `dca38d9` on 2026-09-01. The current branch,
`codex/phase-2d1-location-identity-description`, starts directly from that merge.

Phase 2d1 is implemented, reviewed, and locally validated. It adds immutable location identity
and description values while leaving the broader location/object/item/environmental-state
roadmap item open. No spatial, discovery, runtime, persistence, engine, or UI mechanics were
added.

The private remote has open draft PR #22 for reviewed slice commit `ff1617e`. Re-query its final
checks after this handover-only head update. Pull-request merge remains separately authorized.

At the start of the next session, verify the live branch, pull request, and checks before acting:

```sh
git status --short --branch
git log -4 --oneline --decorate
git fetch origin main
gh pr view codex/phase-2d1-location-identity-description --repo hherb/allrpcs \
  --json number,state,isDraft,headRefName,headRefOid,url
gh pr checks codex/phase-2d1-location-identity-description --repo hherb/allrpcs
```

## Phase 2d1 result

- `LocationIdentity` retains exactly one existing `LocationId` and one authored display name.
  Names must be nonblank and at most 80 UTF-16 code units; valid text is preserved exactly.
- `LocationDescription` is structurally separate from identity, identifiers, and character text.
  It permits empty text, rejects null or more than 4,000 UTF-16 code units, and preserves valid
  authored text exactly.
- Both public values are sealed immutable records with deliberately limited get-only surfaces.
  Location text validation remains internal to the location boundary.
- Seventeen focused tests cover exact retention, boundary values, invalid values, equality,
  public surfaces, and structural separation. `Domain` retains no project references.
- `README.md` documents the boundary and exclusions. `ROADMAP.md` records Phase 2d1 without
  ticking the incomplete combined model item.

Review found no remaining correctness, architecture, or scope issue.

## Validation and limits

The focused test-first run initially failed because the location namespace did not exist. After
implementation, the identical focused run passed all 17 tests on the host shell. The restricted
full sequence produced a zero-warning build, then VSTest could not bind its required loopback
transport (`SocketException (13): Permission denied`).

The identical host-shell `./scripts/quality.sh` passed on Apple Silicon macOS with .NET SDK
8.0.130 and Godot 4.6.3.stable.mono.official:

- workflow lint, serial restore, formatting verification, and a zero-warning build;
- all 195 Domain tests, 100 Application tests, and six architecture tests;
- navigation, healer-command, interaction-focus, and movement-boundary Godot smokes; and
- the bounded 60-frame graphical regression launch with the Compatibility renderer.

No manual play acceptance is required for this engine-independent Domain slice. Godot results are
regression evidence only and make no new visual, input, physics, scene, location-correctness, or
runtime-behavior claim. Hosted desktop outputs remain unsigned development evidence, not releases.
Merge, releases, distribution, credentials, signing, notarization, and visibility changes remain
separately unauthorized.

## Exact next bounded slice

After the user reports this pull request merged, take **Phase 2d2: immutable location aggregate
boundary**.

Acceptance boundary:

1. Add a sealed immutable `Location` Domain aggregate that retains exactly one existing
   `LocationIdentity` and one existing `LocationDescription`.
2. Require both components and retain the supplied immutable instances exactly. Expose only the
   two get-only component properties; add no cross-field policy, methods, or events.
3. Add focused pure tests for exact composition, an empty description, both null cases, and the
   complete read-only public surface.
4. Preserve `Domain -> []`; update `README.md`, `ROADMAP.md`, and this file. Keep the broader
   location/object/item/environmental-state roadmap item unticked.
5. Run `./scripts/quality.sh`, review every intended path, and commit only the reviewed slice.

Explicit exclusions:

- Do not add hierarchy, containment, connections, coordinates, regions, navigation topology,
  terrain, access rules, environmental state, mutable occupancy, lifecycle, or event handling.
- Do not add public/private or per-character filtered views, visibility, perception, discovery,
  reinterpretation, focused search, or interaction behavior.
- Do not add object or item models, inventory mechanics, scenario content, persistence,
  serialization, save schemas, migrations, Godot resources or nodes, UI, providers, or plugins.
- Keep point-and-click movement, disease-time pausing, and the world-time debug overlay deferred
  until their own retained prototype or authoritative-system prerequisites exist.
- Do not merge, release, distribute, sign, notarize, change repository visibility, or add
  credentials without explicit user authorization.
