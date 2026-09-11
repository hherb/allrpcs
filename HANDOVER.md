# Allrpcs Handover

## Current state

`main` contains merged PR #183 at `9615402`. The current branch is
`codex/phase-5-canonical-memory`; draft PR #184 is open and mergeable.

Phase 5 now has one canonical local provider-neutral `CharacterMemoryAggregate` per character. It
retains the exact recent conversation, summaries, episodic and epistemic memories, belief
assessments, relationships, unfinished commitments, and deterministic index. Every constituent
must have value-equal ownership, and every index entry must retain the exact referenced memory in
its corresponding category snapshot. Missing, merely value-equal distinct, and cross-category
matches fail closed. No provider identifier, provider state, reasoning, or chain-of-thought field
exists; existing provider requests remain stateless and reasoning-free.

Relationship changes are immutable and bounded. One `CharacterRelationshipChange` alters one
retained dimension by 1 to 10 points in either direction and carries an exact reason with local-turn
provenance. Pure eligibility requires matching directional ownership, an existing dimension,
available reason capacity, unique reason and source-turn identities, and an in-range result. The
transition rejects overflow rather than clamping, replaces only the affected dimension, appends the
exact reason, and retains unrelated exact instances. A source turn can change a relationship only
once.

The roadmap items for canonical local memory, avoiding hidden chain-of-thought, and bounded
relationship changes are complete. No populated-memory save state, save-schema change or migration,
memory JSON, restart continuity, provider-context memory projection, scene-owned memory state, UI,
summary generation, or commitment-completion policy exists yet.

## Validation

- workflow validation and `dotnet format Allrpcs.sln --no-restore --verify-no-changes`
- zero-warning complete solution build
- 1,525 Domain, 365 Application, 414 Infrastructure, and 8 architecture tests
- representative discovery scenario: 5 valid evidence definitions
- complete `./scripts/smoke-godot.sh`
- bounded 60-frame Compatibility-renderer launch on Godot 4.6.3 .NET

The generated `ScenarioValidator` native app host still stalls during macOS dynamic-loader startup.
The same rebuilt validator DLL passes immediately through `dotnet exec`; this is a local app-host
limitation, not a failing validator assertion. GitHub Actions budget was exhausted for prior draft
PRs. All five PR #184 jobs on run `34547904682` were immediately reported failed with `steps: []`;
this is account infrastructure state, not test evidence, and the jobs must not be dispatched again
solely to compensate.

## Exact next bounded slice

Take **Phase 5k1: immutable all-character memory snapshot**.

Acceptance boundary:

1. Define one immutable provider-neutral snapshot retaining zero to 32 exact
   `CharacterMemoryAggregate` instances in stable supplied order.
2. Require unique value-equal character identities, copy the caller collection, reject nulls and an
   excessive count, and expose only a get-only `IReadOnlyList<CharacterMemoryAggregate>` surface.
3. Add focused tests for empty and complete snapshots, exact-instance/order retention, caller
   mutation, value-equal duplicate ownership, nulls, exact bounds, and immutable API shape.
4. Do not yet change `SaveSnapshot`, schema-version support, JSON, storage, restoration/capture
   controllers, scenes, provider context, relationships, commitments, or the authoritative
   transcript.
5. After the complete quality spine passes, update `README.md`, `ROADMAP.md`, and this handover;
   then take the next migration prerequisite as a separately reviewed slice.
