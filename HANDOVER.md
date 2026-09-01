# Allrpcs Handover

## Current state

PR #25 merged Phase 2e2 into `origin/main` at `7bcb9ae` on 2026-09-01. The current branch,
`codex/phase-2f1-item-classification`, starts directly from that merge.

Phase 2f1 is implemented, reviewed, and locally validated. It adds only the immutable item
classification boundary. The combined location, object, item, and environmental-state roadmap
item remains incomplete.

The private remote has draft PR #26 for reviewed slice commit `6693613`. Re-query its final
checks after this handover-only head update. Pull-request merge remains separately authorized.

## Phase 2f1 result

- `Item` requires and retains exactly one existing `WorldObject` instance.
- It exposes only that component through a get-only property and does not duplicate the world
  object's identifier, name, or description.
- Three focused tests cover exact retention, null rejection, and the complete deliberately
  limited read-only public surface.
- `README.md` documents the classification. `ROADMAP.md` records Phase 2f1 without ticking the
  incomplete combined model item. `Domain` retains no project references.

Review found no remaining correctness, API-surface, architecture, documentation, or scope issue.

## Validation and limits

The test-first run failed because `Allrpcs.Domain.Items` and `Item` did not exist. After
implementation, all three focused tests passed on the host shell. The restricted full sequence
produced a zero-warning build, then VSTest could not bind its required loopback transport
(`SocketException (13): Permission denied`).

The identical host-shell `./scripts/quality.sh` passed on Apple Silicon macOS with .NET SDK
8.0.130 and Godot 4.6.3.stable.mono.official: 223 Domain tests, 100 Application tests, six
architecture tests, four Godot smokes, and the bounded graphical regression launch.

No manual play acceptance is required for this engine-independent Domain slice. Godot results
are regression evidence only. Hosted desktop outputs remain unsigned development evidence, not
releases.

## Exact next bounded slice

After the user reports this pull request merged, take **Phase 2g1: immutable qualitative
environmental-state entry**.

Acceptance boundary:

1. Add a sealed immutable `EnvironmentalState` authored value in
   `Allrpcs.Domain.EnvironmentalStates` with a canonical `Key` and a nonblank `Description`
   bounded to 500 UTF-16 code units.
2. Reuse the existing canonical identifier-text contract. Preserve valid description whitespace,
   casing, line endings, and Unicode exactly rather than normalizing it.
3. Add focused pure tests for canonical-key boundaries, exact description preservation, value
   equality, null/blank/oversized rejection, structural separation from `CharacterCondition`,
   and the complete deliberately limited public surface.
4. Preserve `Domain -> []`; update `README.md`, `ROADMAP.md`, and this file. Keep the combined
   location/object/item/environmental-state roadmap item unticked.
5. Run `./scripts/quality.sh`, review every intended path, and commit only the reviewed slice.

Explicit exclusions:

- Do not add environmental-state collections, assignment to locations or objects, weather,
  lighting, terrain, hazards, transitions, mutation, lifecycle, events, time, or simulation.
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
