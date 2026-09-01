# Allrpcs Handover

## Current state

PR #23 merged Phase 2d2 into `origin/main` at `1113c4e` on 2026-09-01. The current branch,
`codex/phase-2e1-world-object-identity-description`, starts directly from that merge.

Phase 2e1 is implemented, reviewed, and locally validated. It adds only immutable world-object
identity and description values. The world-object aggregate and the broader combined roadmap item
remain incomplete.

The private remote has open draft PR #24 for reviewed slice commit `961d3ae`. Re-query its final
checks after this handover-only head update. Pull-request merge remains separately authorized.

Before continuing after this branch merges, verify the live branch, pull request, and checks.

## Phase 2e1 result

- `WorldObjectIdentity` requires and retains exactly one existing `ObjectId` and an authored
  display name. Names must be nonblank and at most 80 UTF-16 code units; valid text is preserved
  exactly.
- `WorldObjectDescription` is structurally separate, permits empty text, rejects null or more than
  4,000 UTF-16 code units, and preserves valid authored text exactly.
- Seventeen focused tests cover retention, boundaries, invalid values, value equality, deliberately
  limited read-only public surfaces, and separation from location and character values.
- `README.md` documents the new boundary. `ROADMAP.md` records Phase 2e1 without ticking the
  incomplete combined model item. `Domain` retains no project references.

Review found no remaining correctness, architecture, documentation, or scope issue.

## Validation and limits

The test-first run failed because the new world-object namespace did not exist. After
implementation, all 17 focused tests passed on the host shell. The restricted full sequence
produced a zero-warning build, then VSTest could not bind its required loopback transport
(`SocketException (13): Permission denied`).

The identical host-shell `./scripts/quality.sh` passed on Apple Silicon macOS with .NET SDK
8.0.130 and Godot 4.6.3.stable.mono.official:

- workflow lint, serial restore, formatting verification, and a zero-warning build;
- all 216 Domain tests, 100 Application tests, and six architecture tests;
- navigation, healer-command, interaction-focus, and movement-boundary Godot smokes; and
- the bounded 60-frame graphical regression launch with the Compatibility renderer.

No manual play acceptance is required for this engine-independent Domain slice. Godot results are
regression evidence only. Hosted desktop outputs remain unsigned development evidence, not
releases. Merge, releases, distribution, credentials, signing, notarization, and visibility
changes remain separately unauthorized.

## Exact next bounded slice

After the user reports this pull request merged, take **Phase 2e2: immutable world-object
aggregate**.

Acceptance boundary:

1. Add a sealed immutable `WorldObject` Domain aggregate that requires and retains exactly one
   existing `WorldObjectIdentity` and one existing `WorldObjectDescription` instance.
2. Expose only those two components through get-only properties. Add no other cross-field policy,
   public methods, events, or runtime behavior.
3. Add focused pure tests for exact component retention, composition with an empty description,
   both null inputs, and the complete deliberately limited read-only public surface.
4. Preserve `Domain -> []`; update `README.md`, `ROADMAP.md`, and this file. Keep the combined
   location/object/item/environmental-state roadmap item unticked.
5. Run `./scripts/quality.sh`, review every intended path, and commit only the reviewed slice.

Explicit exclusions:

- Do not add an item type or model, object-item classification, properties, tags, portability,
  quantities, stacks, containers, inventory mechanics, ownership, or use.
- Do not add location assignment, hierarchy, containment, connections, coordinates, regions,
  navigation topology, terrain, access rules, environmental state, mutable occupancy, lifecycle,
  events, or interactions.
- Do not add visibility, perception, discovery, filtered views, scenario content, persistence,
  serialization, save schemas, migrations, Godot resources or nodes, UI, providers, or plugins.
- Keep point-and-click movement, disease-time pausing, and the world-time debug overlay deferred
  until their own retained prototype or authoritative-system prerequisites exist.
- Do not merge, release, distribute, sign, notarize, change repository visibility, or add
  credentials without explicit user authorization.
