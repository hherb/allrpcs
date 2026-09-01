# Allrpcs Handover

## Current state

PR #26 merged Phase 2f1 into `origin/main` at `01e7f2e` on 2026-09-02 Brisbane
time. The current branch, `codex/phase-2g1-environmental-state`, starts directly from that
merge.

Phase 2g1 is implemented, reviewed, and locally validated. It adds only the immutable
qualitative environmental-state entry. The combined location, object, item, and
environmental-state roadmap item remains incomplete.

The private remote has draft PR #27 for reviewed slice commit `0af2fad`. Re-query its final
checks after this handover-only head update. Pull-request merge remains separately authorized.

## Phase 2g1 result

- `EnvironmentalState` has a canonical `Key` and a nonblank `Description` bounded to 500
  UTF-16 code units.
- Valid description whitespace, casing, line endings, and Unicode are preserved exactly.
- The value compares by key and description and remains structurally separate from
  `CharacterCondition`.
- Fourteen focused test cases cover canonical-key boundaries, exact description preservation,
  equality, null/blank/oversized rejection, structural separation, and the deliberately limited
  public surface.
- `README.md` documents the boundary. `ROADMAP.md` records Phase 2g1 without ticking the
  incomplete combined model item. `Domain` retains no project references.

Review found no remaining correctness, API-surface, architecture, documentation, or scope issue.

## Validation and limits

The test-first run failed because `Allrpcs.Domain.EnvironmentalStates` and `EnvironmentalState`
did not exist. After implementation, all 14 focused test cases passed on the host shell. The
restricted full sequence produced a zero-warning build, then VSTest could not bind its required
loopback transport (`SocketException (13): Permission denied`).

The identical host-shell `./scripts/quality.sh` passed on Apple Silicon macOS with .NET SDK
8.0.130 and Godot 4.6.3.stable.mono.official: 237 Domain tests, 100 Application tests, six
architecture tests, four Godot smokes, and the bounded graphical regression launch.

No manual play acceptance is required for this engine-independent Domain slice. Godot results
are regression evidence only. Hosted desktop outputs remain unsigned development evidence, not
releases.

## Exact next bounded slice

After the user reports this pull request merged, take **Phase 2g2: immutable ordered
environmental-state set**.

Acceptance boundary:

1. Add a sealed immutable `EnvironmentalStateSet` in
   `Allrpcs.Domain.EnvironmentalStates` containing existing `EnvironmentalState` values.
2. Explicitly permit an empty sequence, preserve authored order, copy caller input, expose only
   a read-only collection, and reject a null sequence, null entries, or duplicate keys using
   ordinal equality.
3. Add focused pure tests for empty input, order preservation, source mutation isolation,
   duplicate and null rejection, and the complete deliberately limited public surface.
4. Preserve `Domain -> []`; update `README.md`, `ROADMAP.md`, and this file. Mark the combined
   location, object, item, and environmental-state model item complete only after this collection
   boundary passes review and validation.
5. Run `./scripts/quality.sh`, review every intended path, and commit only the reviewed slice.

Explicit exclusions:

- Do not assign environmental states to locations or objects or add weather, lighting, terrain,
  hazards, transitions, mutation, lifecycle, events, time, or simulation.
- Do not add item properties, tags, portability, quantities, stacks, containers, inventory
  mechanics, ownership, use, mutation, lifecycle, events, or interactions.
- Do not add location hierarchy, containment, connections, coordinates, regions, navigation
  topology, access rules, mutable occupancy, visibility, perception, discovery, or filtered views.
- Do not add scenario content, persistence, serialization, save schemas, migrations, Godot
  resources or nodes, UI, providers, or plugins.
- Keep point-and-click movement, disease-time pausing, and the world-time debug overlay deferred
  until their own retained prototype or authoritative-system prerequisites exist.
- Do not merge, release, distribute, sign, notarize, change repository visibility, or add
  credentials without explicit user authorization.
