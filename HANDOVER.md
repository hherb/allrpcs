# Allrpcs Handover

## Current state

Phase 2h1 is implemented, reviewed, and locally validated on
`codex/phase-2h1-visibility-state`, based directly on the PR #28 merge at `6666de4`.
The draft pull request has not yet been opened. Pull-request merge remains separately
authorized.

## Phase 2h1 result

- `Allrpcs.Domain.Perception.VisibilityState` defines exactly `unnoticed`, `noticed`,
  `identified`, `understood`, `altered`, and `collected` as canonical singleton values.
- `All` exposes the complete set in stable roadmap order through a read-only collection.
- `Value`, `ToString()`, and exact `Parse` round-trip canonical text. Null, unknown, and
  noncanonical text are rejected rather than normalized or registered.
- Type-safe equality identifies the defined singleton state. No public constructor permits
  undeclared values.
- Six focused test methods cover the complete set and order, parsing, equality, rejected text,
  null, and the deliberately limited construction/property surface.
- `README.md` documents the boundary and `ROADMAP.md` marks only the visibility-state item
  complete. `Domain` retains no project references.

Review found no remaining correctness, API-surface, architecture, documentation, or scope issue.

## Validation and limits

The test-first focused run failed because the `Allrpcs.Domain.Perception` namespace and
`VisibilityState` did not exist. The final focused host-shell run passed all 11 discovered test
cases.

The restricted full sequence produced a zero-warning build, then VSTest could not bind its
required loopback transport (`SocketException (13): Permission denied`). The identical
host-shell `./scripts/quality.sh` passed on Apple Silicon macOS with .NET SDK 8.0.130 and Godot
4.6.3.stable.mono.official: 254 Domain tests, 100 Application tests, six architecture tests,
four Godot smokes, and the bounded graphical regression launch.

No manual play acceptance is required for this engine-independent Domain slice. Godot results
are regression evidence only. Hosted desktop outputs remain unsigned development evidence, not
releases.

## Exact next bounded slice

After the user reports this pull request merged, take **Phase 2h2: immutable world-object
visibility association** as the next prerequisite for per-character perception snapshots.

Acceptance boundary:

1. Add a sealed immutable `WorldObjectVisibility` in `Allrpcs.Domain.Perception` associating
   exactly one existing `ObjectId` with one existing `VisibilityState`.
2. Require and retain both supplied instances, reject null inputs, and expose only get-only
   `ObjectId` and `VisibilityState` properties. Add no cross-field policy or default state.
3. Add focused pure tests for reference retention, null rejection, value equality, and the
   deliberately limited public surface.
4. Preserve `Domain -> []`; update `README.md`, `ROADMAP.md`, and this file. Keep the broader
   per-character perception-snapshot checklist item unticked.
5. Run `./scripts/quality.sh`, review every intended path, and commit only the reviewed slice.

Explicit exclusions:

- Do not add the snapshot collection yet or assign visibility entries to a character.
- Do not add filtered views, visibility progression, transition rules, discovery,
  reinterpretation, permissions, actions, events, or mutable state.
- Do not treat `altered` or `collected` as item ownership, inventory mutation, persistence, or
  a world-event result.
- Do not add locations, evidence, environmental state, scenario content, serialization, save
  schemas, migrations, Godot resources or nodes, UI, providers, or plugins to this association.
- Keep point-and-click movement, disease-time pausing, and the world-time debug overlay deferred
  until their own retained prototype or authoritative-system prerequisites exist.
- Do not merge, release, distribute, sign, notarize, change repository visibility, or add
  credentials without explicit user authorization.
