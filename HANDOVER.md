# Allrpcs Handover

## Current state

`main` contains merged PR #180 at `2ecc376`. The current branch is
`codex/phase-5-character-memory`.

Phase 5 now has its first two Domain-only local-memory slices. A recent-conversation entry retains
one canonical `CharacterTurnId` and the exact completed provider-neutral activity. Its immutable
per-character window retains exact entries in oldest-to-newest order, rejects mismatched characters
and duplicate turn IDs, copies its collection, and caps retention at 16 turns and 16,000 aggregate
UTF-16 dialogue code units. Pure append preserves the exact owner and newest entries while evicting
only as many oldest entries as the count or text bounds require.

A significant episodic memory has a canonical `EpisodicMemoryId`, exact remembering character,
stable unique participants including that character, exact location, exact qualitative-time
snapshot, exact source `CharacterTurnId`, and a non-blank summary of at most 1,000 UTF-16 code
units. Its immutable per-character snapshot copies and retains at most 128 exact entries in stable
order with unique memory IDs.

No memory derivation, retrieval, persistence, save-schema change, provider-context integration,
scene state, summarization, beliefs, claims, confidence, relationships, promises, tasks, UI,
migration, provider-side state, or native-platform claim was added.

## Validation

- `dotnet format Allrpcs.sln --no-restore --verify-no-changes`
- zero-warning complete solution build
- 1,414 Domain, 365 Application, 414 Infrastructure, and 8 architecture tests
- representative discovery scenario: 5 valid evidence definitions
- complete `./scripts/smoke-godot.sh`
- bounded 60-frame Compatibility-renderer launch

The complete quality script passed through every managed-code test, then the unchanged generated
ScenarioValidator native app host stalled in macOS dynamic-loader startup. Rebuilding did not change
that host behavior. The same rebuilt validator DLL passed immediately through `dotnet exec`, after
which the complete Godot smoke script and graphical launch passed separately. This is a local app-host
execution limitation, not a failing validator assertion or product failure. The first sandboxed
VSTest attempt also hit the known local-socket restriction; its identical host run passed.

GitHub Actions budget was exhausted for prior draft PRs. Do not dispatch hosted workflows solely to
compensate for that account infrastructure state; jobs with zero steps are not test evidence.

## Exact next bounded slice

Take **Phase 5c: separate confirmed facts, attributed claims, and uncertain beliefs**.

Acceptance boundary:

1. Define the smallest provider-neutral immutable per-character epistemic-memory contracts that
   structurally distinguish confirmed facts, attributed claims, and uncertain beliefs. Retain exact
   local provenance; require an attributable speaker only for claims; bound identifiers and text.
2. Add focused contract tests for each category, invalid category-specific shapes, stable ordering,
   copied read-only collections, duplicate identity, nulls, and exact-instance retention.
3. Do not add confidence/evidence scoring, relationship state, promises/tasks, retrieval, memory
   derivation, persistence/save schema, provider context, scene state, UI, or migration in this slice.
4. Update `README.md`, `ROADMAP.md`, and this handover after the complete quality spine passes.
