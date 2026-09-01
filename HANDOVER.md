# Allrpcs Handover

## Current state

PR #24 merged Phase 2e1 into `origin/main` at `f28ce61` on 2026-09-01. The current branch,
`codex/phase-2e2-world-object-aggregate`, starts directly from that merge.

Phase 2e2 is implemented, reviewed, and locally validated. It adds only the immutable
world-object aggregate. The combined location, object, item, and environmental-state roadmap
item remains incomplete.

## Phase 2e2 result

- `WorldObject` requires and retains exactly one existing `WorldObjectIdentity` and one existing
  `WorldObjectDescription` instance.
- It exposes only those components through get-only properties and adds no cross-field policy,
  methods, events, or runtime behavior.
- Four focused tests cover exact retention, an empty description, both null inputs, and the
  complete deliberately limited read-only public surface.
- `README.md` documents the aggregate. `ROADMAP.md` records Phase 2e2 without ticking the
  incomplete combined model item. `Domain` retains no project references.

Review found no remaining correctness, architecture, documentation, or scope issue.

## Validation and limits

The test-first run failed because `WorldObject` did not exist. After implementation, all four
focused tests passed on the host shell. The restricted full sequence produced a zero-warning
build, then VSTest could not bind its required loopback transport
(`SocketException (13): Permission denied`).

The identical host-shell `./scripts/quality.sh` passed on Apple Silicon macOS with .NET SDK
8.0.130 and Godot 4.6.3.stable.mono.official: 220 Domain tests, 100 Application tests, six
architecture tests, four Godot smokes, and the bounded graphical regression launch.

No manual play acceptance is required for this engine-independent Domain slice. Godot results
are regression evidence only. Hosted desktop outputs remain unsigned development evidence, not
releases.

## Exact next bounded slice

After the user reports this pull request merged, take **Phase 2f1: immutable item classification**.

Acceptance boundary:

1. Add a sealed immutable `Item` Domain aggregate in `Allrpcs.Domain.Items` that requires and
   retains exactly one existing `WorldObject` instance.
2. Expose only that component through a get-only `WorldObject` property. Do not duplicate its
   identifier, name, or description or add other policy, methods, events, or runtime behavior.
3. Add focused pure tests for exact component retention, null rejection, and the complete
   deliberately limited read-only public surface.
4. Preserve `Domain -> []`; update `README.md`, `ROADMAP.md`, and this file. Keep the combined
   location/object/item/environmental-state roadmap item unticked.
5. Run `./scripts/quality.sh`, review every intended path, and commit only the reviewed slice.

Explicit exclusions:

- Do not add item properties, tags, portability, quantities, stacks, containers, inventory
  mechanics, ownership, use, mutation, lifecycle, events, or interactions.
- Do not add location assignment, hierarchy, containment, connections, coordinates, regions,
  navigation topology, terrain, access rules, environmental state, or mutable occupancy.
- Do not add visibility, perception, discovery, filtered views, scenario content, persistence,
  serialization, save schemas, migrations, Godot resources or nodes, UI, providers, or plugins.
- Keep point-and-click movement, disease-time pausing, and the world-time debug overlay deferred
  until their own retained prototype or authoritative-system prerequisites exist.
- Do not merge, release, distribute, sign, notarize, change repository visibility, or add
  credentials without explicit user authorization.
