# Allrpcs Handover

## Current state

`main` contains merged PR #187 at `e11bad9`. The current branch is
`codex/phase-5-memory-context-continuity`.

Phase 5 is complete. Canonical provider-neutral memory is local, bounded, version-2 serialized,
migrated from version 1, and restored atomically with discovery and world-event state. One pure
Application projection selects only the responding character's unfinished commitments and emits
stable bounded `Memory` context containing commitment kind, counterparty, completed source-turn
identity, and exact description. Missing character memory yields no entries; canonical constructors
and the existing model-context limits reject duplicate, mismatched, null, or excessive input.

The scene-owned memory controller supplies those entries to the existing generation-input assembler.
A credential-free ordinary-flow smoke serializes a healer promise, resets and restores memory, then
proves the same promise and source reach fresh requests across an exact future-turn OpenAI-to-Anthropic
binding replacement without the complete campaign transcript. Providers do not own or mutate memory.
Live memory derivation, summaries, commitment completion, relationship changes, save UI, and semantic
recall claims from a live model remain unimplemented.

## Validation

- workflow validation, restore, formatting, and zero-warning complete solution build
- 1,536 Domain, 369 Application, 423 Infrastructure, and 8 architecture tests
- representative discovery scenario: 5 valid evidence definitions through the rebuilt validator DLL
- all 77 credential-free Godot smokes
- bounded 60-frame Compatibility-renderer launch on Godot 4.6.3 .NET

The generated `ScenarioValidator` native app host again stalled during macOS dynamic-loader startup.
The same rebuilt validator DLL passed immediately through `dotnet exec`; this remains a local app-host
limitation, not a validator assertion failure.

## Exact next bounded slice

Take **Phase 6a1: canonical disease identity and hidden-definition foundation**.

Acceptance boundary:

1. Add one canonical `DiseaseId` and the smallest immutable Domain disease-definition contract needed
   to establish engine-owned disease truth independently of character beliefs, memory, dialogue, or
   provider state.
2. Add a copied read-only disease-definition snapshot with stable supplied order, unique identities,
   explicit bounds, exact-instance retention, null rejection, and reflection-backed closed-surface
   tests.
3. Keep authored disease truth unavailable to perception and model context; do not reuse character
   belief or epistemic-memory types as authoritative truth.
4. Do not yet add disease stages, character infection state, symptoms, progression, dormancy,
   transmission, assessment, treatment, ingredients, experiments, journal/evidence conclusions,
   persistence, UI, or scene wiring, and do not check the full roadmap truth item until its required
   mechanics exist.
