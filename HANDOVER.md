# Allrpcs Handover

## Current state

`main` contains merged PR #184 at `36e6c14`. The current branch is
`codex/phase-5-memory-persistence`; draft PR #185 is open and mergeable.

Phase 5 now has a canonical provider-neutral memory aggregate for each character and an immutable
all-character `CharacterMemorySnapshot`. The snapshot copies zero to 32 exact aggregates in stable
order and rejects null entries, excessive counts, and value-equal duplicate character identities.

The in-memory `SaveSnapshot` retains an exact character-memory snapshot. Version `1` preserves its
existing equality and serialization behavior through one shared immutable empty-memory snapshot and
rejects populated memory. `SaveSchemaVersion.CharacterMemory` defines version `2`, while load and
JSON support remain deliberately disabled. The version-1 serializer now rejects unsupported roots,
so it cannot silently discard character memory.

`SaveSnapshotCharacterMemoryMigration` is a pure one-way version-1 to version-2 transition. It
creates a fresh root retaining the exact discovery, world-event, and empty character-memory
instances, rejects every other source version and null, and leaves the source unchanged.

No version-2 memory JSON, version-2 load support, populated-memory migration, storage/controller or
scene memory state, restart/provider-switch continuity, provider-context memory projection,
summary generation, relationship scene transition, commitment completion, UI, or authoritative
transcript change exists yet. The roadmap migration and continuity items therefore remain open.

## Validation

- workflow validation and `dotnet format Allrpcs.sln --no-restore --verify-no-changes`
- zero-warning complete solution build
- 1,534 Domain, 365 Application, 415 Infrastructure, and 8 architecture tests
- representative discovery scenario: 5 valid evidence definitions
- complete `./scripts/smoke-godot.sh`
- bounded 60-frame Compatibility-renderer launch on Godot 4.6.3 .NET

The generated `ScenarioValidator` native app host still stalls during macOS dynamic-loader startup.
The same rebuilt validator DLL passes immediately through `dotnet exec`; this is a local app-host
limitation, not a failing validator assertion. All five PR #185 jobs on run `34549910877` were
immediately reported failed with `steps: []` because the GitHub Actions budget remains exhausted;
this is account infrastructure state, not test evidence, and the jobs must not be redispatched
solely to compensate.

## Exact next bounded slice

Take **Phase 5k4: canonical version-2 character-memory JSON serialization**.

Acceptance boundary:

1. Extend `SaveSnapshotJsonSerializer` to write a stable complete version-2 `characterMemory`
   representation for every aggregate constituent and deterministic index reference while leaving
   the exact version-1 document unchanged.
2. Encode only provider-neutral canonical state. Preserve supplied collection order and reference
   index targets by category plus canonical memory identity; include no provider identifiers,
   credentials, hidden reasoning, chain of thought, or authoritative transcript duplication.
3. Add focused exact-document tests for empty and representative complete version-2 roots, stable
   value-equal output, all categories, null/optional fields, and unchanged inputs.
4. Do not yet enable version-2 load support, deserialize or execute migration, change storage or
   scene controllers, capture live memory, project provider context, or claim restart continuity.
5. After the complete quality spine passes, update `README.md`, `ROADMAP.md`, and this handover;
   then take strict version-2 deserialization plus version-1 migration execution separately.
