# Allrpcs Handover

## Current state

PR #22 merged Phase 2d1 into `origin/main` at `6a86445` on 2026-09-01. The current branch,
`codex/phase-2d2-location-aggregate`, starts directly from that merge.

Phase 2d2 is implemented, reviewed, and locally validated. It adds only the immutable `Location`
aggregate. The broader location/object/item/environmental-state roadmap item remains incomplete.

The private remote has open draft PR #23 for reviewed slice commit `e10e5e6`. Re-query its final
checks after this handover-only head update. Pull-request merge remains separately authorized.

Before continuing after this branch merges, verify the live branch, pull request, and checks.

## Phase 2d2 result

- `Location` requires and retains exactly one existing `LocationIdentity` and one existing
  `LocationDescription` instance.
- Its complete public surface is two get-only properties. It adds no cross-field policy, methods,
  events, or runtime behavior.
- Four focused tests cover exact composition, an empty description, both null inputs, and the
  complete read-only public surface. `Domain` retains no project references.
- `README.md` documents the completed aggregate boundary. `ROADMAP.md` records Phase 2d2 without
  ticking the incomplete combined model item.

Review found no remaining correctness, architecture, or scope issue.

## Validation and limits

The test-first run failed on the missing `Location` type. After implementation, all four focused
tests passed on the host shell. The restricted full sequence produced a zero-warning build, then
VSTest could not bind its required loopback transport (`SocketException (13): Permission denied`).

The identical host-shell `./scripts/quality.sh` passed on Apple Silicon macOS with .NET SDK
8.0.130 and Godot 4.6.3.stable.mono.official:

- workflow lint, serial restore, formatting verification, and a zero-warning build;
- all 199 Domain tests, 100 Application tests, and six architecture tests;
- navigation, healer-command, interaction-focus, and movement-boundary Godot smokes; and
- the bounded 60-frame graphical regression launch with the Compatibility renderer.

No manual play acceptance is required for this engine-independent Domain slice. Godot results are
regression evidence only. Hosted desktop outputs remain unsigned development evidence, not
releases. Merge, releases, distribution, credentials, signing, notarization, and visibility
changes remain separately unauthorized.

## Exact next bounded slice

After the user reports this pull request merged, take **Phase 2e1: immutable world-object identity
and description boundary**.

Acceptance boundary:

1. Add a sealed immutable `WorldObjectIdentity` Domain value that retains exactly one existing
   `ObjectId` and one authored display name. Require nonblank names of at most 80 UTF-16 code units
   and preserve valid text exactly.
2. Add a structurally separate sealed immutable `WorldObjectDescription` value. Permit empty text,
   reject null or more than 4,000 UTF-16 code units, and preserve valid authored text exactly.
3. Add focused pure tests for exact retention, boundary values, invalid values, value equality,
   deliberately limited read-only public surfaces, and separation from location and character
   values.
4. Preserve `Domain -> []`; update `README.md`, `ROADMAP.md`, and this file. Do not add the
   world-object aggregate yet, and keep the combined roadmap item unticked.
5. Run `./scripts/quality.sh`, review every intended path, and commit only the reviewed slice.

Explicit exclusions:

- Do not add an object aggregate, item type or model, object-item classification, properties,
  tags, portability, quantities, stacks, containers, inventory mechanics, ownership, or use.
- Do not add location assignment, hierarchy, containment, connections, coordinates, regions,
  navigation topology, terrain, access rules, environmental state, mutable occupancy, lifecycle,
  events, or interactions.
- Do not add visibility, perception, discovery, filtered views, scenario content, persistence,
  serialization, save schemas, migrations, Godot resources or nodes, UI, providers, or plugins.
- Keep point-and-click movement, disease-time pausing, and the world-time debug overlay deferred
  until their own retained prototype or authoritative-system prerequisites exist.
- Do not merge, release, distribute, sign, notarize, change repository visibility, or add
  credentials without explicit user authorization.
