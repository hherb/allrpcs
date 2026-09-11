# Allrpcs Handover

## Current state

`main` contains merged PR #186 at `997d764`. The current branch is
`codex/phase-5-memory-atomic-restoration`; draft PR #187 is open.

Phase 5 now restores complete canonical character memory. One scene-owned
`CharacterMemoryController` starts with explicit empty memory and retains any exact caller-supplied
immutable replacement. Save capture emits version-2 roots with that exact snapshot. Complete
restoration preflights discovery, world events, and the known schema before installing all three
state areas. Serialized loading uses `DeserializeCurrent`, so version-1 documents migrate to
version 2 with empty memory while version-2 documents retain populated memory.

Focused smokes prove exact populated-memory capture, canonical JSON, repeated restoration, legacy
migration, and preservation after malformed, future-version, missing-object, and missing-event
rejections. Memory is still not derived from dialogue or projected into model context. Commitments
are not completed, relationships are not changed, and provider switching has no memory continuity
claim yet.

## Validation

- workflow validation, restore, formatting, and zero-warning complete solution build
- 1,536 Domain, 365 Application, 423 Infrastructure, and 8 architecture tests
- representative discovery scenario: 5 valid evidence definitions through the rebuilt validator DLL
- all 77 credential-free Godot smokes
- bounded 60-frame Compatibility-renderer launch on Godot 4.6.3 .NET

The generated `ScenarioValidator` native app host again stalled during macOS dynamic-loader startup.
The same rebuilt validator DLL passed immediately through `dotnet exec`; this remains a local app-host
limitation, not a validator assertion failure.

All five draft PR #187 jobs on run `34553240923` failed with `steps: []`. This is GitHub Actions
account infrastructure state rather than test evidence; do not redispatch those jobs solely to
compensate.

## Exact next bounded slice

Take **Phase 5l1: bounded commitment-memory context continuity**.

Acceptance boundary:

1. Add one pure Application projection that selects only the responding character's exact unfinished
   commitments from a supplied canonical memory snapshot and emits bounded deterministic `Memory`
   context entries containing commitment kind, counterparty, completed source-turn identity, and
   description.
2. Preserve stable commitment order, exact source data, closed kinds, existing context limits, and
   fail-closed missing, duplicate, mismatched, null, or excessive input handling without using the
   authoritative transcript as a substitute.
3. Supply those projected entries from the scene-owned memory controller to the existing generation
   input assembler; do not let the scene or a provider derive, rewrite, or own canonical memory.
4. Prove a saved and restored healer promise produces the same bounded request memory before and
   after an exact future-turn provider switch, without requiring the complete campaign transcript.
5. Do not yet derive live memories, generate summaries, complete commitments, change relationships,
   add save UI, add provider-side conversation state, or claim semantic recall from a live model.
