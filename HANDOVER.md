# Allrpcs Handover

## Current state

`main` contains merged PR #185 at `5c9bf5b`. The current branch is
`codex/phase-5-memory-save-continuity`; draft PR #186 is open and mergeable.

Phase 5 now has a complete canonical version-2 character-memory JSON contract. The existing
version-1 document is byte-for-byte unchanged. Version 2 writes every ordered aggregate constituent,
uses category plus canonical identity for deterministic index references, uses exact memory identity
for belief-assessment references, and represents optional values explicitly. It contains no provider
identifier, credential, provider state, hidden reasoning, chain of thought, or additional
authoritative transcript.

Strict version-2 deserialization rejects unknown, missing, or duplicate properties, invalid value
kinds and closed values, invalid bounded collections, and unresolved memory references. It rebuilds
belief assessments and index entries against the exact deserialized memory instances. The explicit
`DeserializeCurrent` entry point returns version 2 unchanged and executes the existing pure
version-1 to version-2 empty-memory migration for version-1 documents.

Live loading remains deliberately gated to version 1. `SaveSnapshotJsonController` still uses exact
deserialization rather than current-schema migration, and the scene has no character-memory
controller. This prevents a populated version-2 root from restoring discovery and world events while
silently discarding memory. No live memory capture, storage/controller continuity, provider-context
memory projection, summary generation, relationship scene transition, commitment completion, UI, or
authoritative transcript change exists yet.

## Validation

- workflow validation, restore, formatting, and zero-warning complete solution build
- 1,534 Domain, 365 Application, 422 Infrastructure, and 8 architecture tests
- representative discovery scenario: 5 valid evidence definitions
- all 76 credential-free Godot smokes
- bounded 60-frame Compatibility-renderer launch on Godot 4.6.3 .NET

The generated `ScenarioValidator` native app host stalled during macOS dynamic-loader startup. The
same rebuilt validator DLL passed immediately through `dotnet exec`; this is the known local app-host
limitation, not a failing validator assertion.
GitHub Actions budget failures with `steps: []` are account infrastructure state, not test evidence,
and those jobs must not be redispatched solely to compensate. All five PR #186 jobs on run
`34551815140` were reported failed with empty step lists.

## Exact next bounded slice

Take **Phase 5k6: scene-owned character memory and atomic current-schema restoration**.

Acceptance boundary:

1. Add one scene-owned controller that retains an exact immutable `CharacterMemorySnapshot`, starts
   with explicit empty memory, and can install one exact caller-supplied replacement without deriving
   or mutating memory.
2. Capture version-2 roots with that exact scene snapshot and extend complete restoration preflight
   and installation across discovery, world events, and character memory without partial mutation.
3. Route serialized loading through `DeserializeCurrent` so legacy version-1 documents migrate to
   empty-memory version 2 before preflight while existing version-2 roots retain exact loaded memory.
4. Prove exact populated-memory capture/JSON/restore and legacy migration through focused Godot
   smokes, including repeated restoration, rejection preservation, and unchanged unrelated state.
5. Do not yet derive live memory, project it into provider context, complete commitments, change
   relationships, claim provider-switch recall, add save UI, or change authoritative transcripts.
