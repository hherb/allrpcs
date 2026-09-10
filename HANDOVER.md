# Allrpcs Handover

## Current state

`main` contains merged PR #181 at `80654ba`. The current branch is
`codex/phase-5-epistemic-memory`.

Phase 5 now has Domain-only contracts for recent conversation, episodic memory, epistemic memory,
belief assessment, relationship memory, and unfinished commitments. Confirmed facts, attributed
claims, and uncertain beliefs are structurally distinct; only claims require a speaker, and every
entry retains an exact completed local source turn. Belief confidence is stored from 0 to 10,000
basis points with bounded, copied, non-overlapping supporting and contradicting `EvidenceId` lists.

Directional relationship memory uses closed trust, affection, respect, and fear dimensions bounded
from -100 to 100. Bounded recorded reasons retain exact source turns and refer only to dimensions
present in that relationship. Unfinished promises, assigned tasks, and other commitments retain an
exact responsible character, counterparty, source turn, and bounded text. All per-character
snapshots copy their inputs, preserve exact instances in stable order, reject mismatched ownership,
nulls, and duplicate identities, and enforce explicit count limits.

No memory derivation, deterministic retrieval, indexing/tags, transcript summarization,
persistence/save-schema change, provider-context integration, scene state, UI, migration,
provider-side state, chain-of-thought, relationship transition, or commitment completion policy was
added.

## Validation

- workflow validation and `dotnet format Allrpcs.sln --no-restore --verify-no-changes`
- zero-warning complete solution build
- 1,463 Domain, 365 Application, 414 Infrastructure, and 8 architecture tests
- representative discovery scenario: 5 valid evidence definitions
- complete `./scripts/smoke-godot.sh`
- bounded 60-frame Compatibility-renderer launch on Godot 4.6.3 .NET

The complete quality script passed workflow validation, restore, format, build, and every managed
test, then the unchanged generated `ScenarioValidator` native app host again stalled in macOS
dynamic-loader startup. The same rebuilt validator DLL passed immediately through `dotnet exec`,
after which the complete Godot smoke script and graphical launch passed separately. This remains a
local app-host execution limitation, not a failing validator assertion or product failure.

GitHub Actions budget was exhausted for prior draft PRs. Do not dispatch hosted workflows solely to
compensate for that account infrastructure state; jobs with zero steps are not test evidence.

## Exact next bounded slice

Take **Phase 5g1: deterministic memory retrieval tags and index contracts**.

Acceptance boundary:

1. Define the smallest provider-neutral immutable tag vocabulary and per-character index-entry
   contracts needed to reference exact existing episodic, epistemic, relationship, and commitment
   memories by bounded topic, entity, location, and relationship tags without copying their content.
2. Preserve exact owner and memory instances, canonical bounded tags, stable supplied order, copied
   read-only collections, category-safe references, and explicit entry/tag count limits.
3. Add focused tests for every memory category and tag kind, invalid cross-character or
   category/reference shapes, duplicate identities/tags, nulls, bounds, and exact-instance
   retention.
4. Do not yet add query selection/ranking, semantic or vector search, memory derivation,
   summarization, persistence/save schema, provider context, scene state, UI, or migration.
5. Update `README.md`, `ROADMAP.md`, and this handover after the complete quality spine passes.
