# Allrpcs Handover

## Current state

`main` contains merged PR #182 at `ff56055`. The current branch is
`codex/phase-5-memory-retrieval`; draft PR #183 is open and mergeable.

Phase 5 has provider-neutral Domain contracts for per-character recent conversation, episodic and
epistemic memory, belief assessments, directional relationships, and unfinished commitments. All
retain exact local provenance, bounded copied collections, stable order, and explicit ownership;
confirmed facts, attributed claims, uncertain beliefs, relationship dimensions/reasons, promises,
and assigned tasks remain structurally distinct.

Memory retrieval is deterministic and local. Four closed memory categories reference exact existing
episodic, epistemic, relationship, or commitment instances. Entries carry 1 to 16 unique canonical
topic, entity, location, or relationship tags, and a per-character index retains at most 576 entries
with category-specific unique identities. Queries require 1 to 8 exact all-match tags, return 1 to
32 exact entries in index order, and perform no scoring, semantic/fuzzy search, or content copying.

Older-conversation summaries retain one canonical identity, exact character, 1 to 32 ordered unique
source-turn identities, and at most 2,000 UTF-16 code units of non-blank text; snapshots retain at
most 128 exact summaries. Pure recording requires every source turn to remain in the recent window
in relative order, appends the exact supplied summary, and returns the exact recent window and
authoritative transcript unchanged. No summary generation or transcript/recent-window deletion was
added.

No Phase 5 persistence/save-schema change, provider-context projection, provider-side state,
chain-of-thought storage/request, scene state, UI, relationship-change transition, or commitment
completion policy exists yet.

## Validation

- workflow validation and `dotnet format Allrpcs.sln --no-restore --verify-no-changes`
- zero-warning complete solution build
- 1,500 Domain, 365 Application, 414 Infrastructure, and 8 architecture tests
- representative discovery scenario: 5 valid evidence definitions
- complete `./scripts/smoke-godot.sh`
- bounded 60-frame Compatibility-renderer launch on Godot 4.6.3 .NET

The generated `ScenarioValidator` native app host still stalls during macOS dynamic-loader startup.
The same rebuilt validator DLL passes immediately through `dotnet exec`; this is a local app-host
limitation, not a failing validator assertion. GitHub Actions budget was exhausted for prior draft
PRs. All five PR #183 jobs were immediately reported failed with `steps: []`; this is account
infrastructure state, not test evidence, and the jobs should not be dispatched again solely to
compensate.

## Exact next bounded slice

Take **Phase 5i1: canonical local per-character memory aggregate**.

Acceptance boundary:

1. Define one immutable provider-neutral aggregate retaining the exact recent conversation,
   summaries, episodic memories, epistemic memories, belief assessments, relationships,
   commitments, and deterministic index for one character.
2. Require value-equal ownership across every constituent and require each index reference to retain
   the exact corresponding memory instance in its supplied category snapshot; reject absent,
   value-equal-but-distinct, or cross-category references.
3. Add focused tests for empty and complete aggregates, every constituent/category, exact-instance
   retention, mismatched ownership, missing/distinct index targets, nulls, and immutable surface.
4. Do not add provider-owned canonical state, context projection, provider requests, persistence or
   save-schema changes, migration, summary generation, scene state, UI, relationship transitions,
   commitment completion, or chain-of-thought fields.
5. Update `README.md`, `ROADMAP.md`, and this handover after the complete quality spine passes.
