# Allrpcs Handover

## Current state

PR #27 merged Phase 2g1 into `origin/main` at `47eab71` on 2026-09-02 Brisbane
time. The current branch, `codex/phase-2g2-environmental-state-set`, starts directly
from that merge.

Phase 2g2 is implemented, reviewed, and locally validated. It completes the structural
location, world-object, item, and environmental-state roadmap item without adding runtime
mechanics. Pull-request merge remains separately authorized.

## Phase 2g2 result

- `EnvironmentalStateSet` contains existing `EnvironmentalState` values in stable authored
  order.
- It permits empty input, copies caller input, exposes `IReadOnlyList<EnvironmentalState>`,
  and retains the supplied state instances.
- It rejects a null sequence, null entries, and duplicate canonical keys using ordinal
  equality.
- Six focused tests cover empty input, order and reference retention, source mutation
  isolation, duplicate and null rejection, read-only behavior, and the complete deliberately
  limited public surface.
- `README.md` documents the boundary. `ROADMAP.md` marks the combined structural model item
  complete. `Domain` retains no project references.

Review found no remaining correctness, API-surface, architecture, documentation, or scope
issue.

## Validation and limits

The test-first run failed because `EnvironmentalStateSet` did not exist. The final focused
host-shell run passed all six tests. The restricted full sequence produced a zero-warning
build, then VSTest could not bind its required loopback transport
(`SocketException (13): Permission denied`).

The identical host-shell `./scripts/quality.sh` passed on Apple Silicon macOS with .NET SDK
8.0.130 and Godot 4.6.3.stable.mono.official: 243 Domain tests, 100 Application tests, six
architecture tests, four Godot smokes, and the bounded graphical regression launch.

No manual play acceptance is required for this engine-independent Domain slice. Godot results
are regression evidence only. Hosted desktop outputs remain unsigned development evidence, not
releases.

## Exact next bounded slice

After the user reports this pull request merged, take **Phase 2h1: closed visibility-state
values**.

Acceptance boundary:

1. Add a sealed immutable `VisibilityState` in `Allrpcs.Domain.Perception` defining exactly
   `unnoticed`, `noticed`, `identified`, `understood`, `altered`, and `collected` as canonical
   singleton values.
2. Expose the complete set in that stable roadmap order, canonical `Value` and `ToString()`
   text, exact `Parse` round-tripping, and type-safe equality. Reject null, unknown, or
   noncanonical text rather than normalizing or registering it.
3. Add focused pure tests for the complete set and order, parse round-tripping, equality,
   rejected values, and the deliberately limited public surface.
4. Preserve `Domain -> []`; update `README.md`, `ROADMAP.md`, and this file. Mark only the
   visibility-state checklist item complete after review and validation.
5. Run `./scripts/quality.sh`, review every intended path, and commit only the reviewed slice.

Explicit exclusions:

- Do not assign visibility to characters or objects or add per-character perception snapshots,
  filtered views, visibility progression, transition rules, discovery, reinterpretation,
  permissions, actions, events, or mutable state.
- Do not treat `altered` or `collected` as item ownership, inventory mutation, persistence, or
  a world-event result.
- Do not add scenario content, serialization, save schemas, migrations, Godot resources or
  nodes, UI, providers, or plugins.
- Keep point-and-click movement, disease-time pausing, and the world-time debug overlay deferred
  until their own retained prototype or authoritative-system prerequisites exist.
- Do not merge, release, distribute, sign, notarize, change repository visibility, or add
  credentials without explicit user authorization.
