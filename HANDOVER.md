# Allrpcs Handover

## Current state

`main` contains merged PR #154 at `5720fe6`. The branch
`codex/phase-4-context-perception` advances Phase 4 through Phase 4b13:

- Phase 4b8-b9 project only exact identified world-object descriptions for the responding
  character and make the assembler own that projection. Other visibility states, empty
  descriptions, identifiers, display names, and absent objects stay hidden; inconsistent
  authoritative inputs fail closed.
- Phase 4b10-b11 add immutable authored `KnowledgeEntry`, `KnowledgeSnapshot`, and value-equal
  `KnowledgeLookup` contracts without adding truth, belief, confidence, provenance, or retrieval
  semantics.
- Phase 4b12-b13 project only authored knowledge referenced by the exact character in character
  order and make the assembler own that projection. Unreferenced content stays hidden and missing
  references fail closed.

The Phase 4 context-assembler roadmap item remains open. Only memory and valid-affordance entries
remain caller-supplied. No memory store/window, affordance eligibility source, provider adapter or
call, deadline executor, retry, credential, provider logging, structured-output validation, Godot
wiring, partial intention execution, persistence, or gameplay/world-state mutation has been added.

## Validation

Each new boundary began with an expected focused compile failure. Focused host-local knowledge,
perception, projection, and assembler tests pass. The complete host-local `./scripts/quality.sh`
passes workflow and formatting validation, a zero-warning build, 1,388 Domain tests, 214
Application tests, 23 Infrastructure tests, eight architecture tests, the five-evidence
representative scenario validator, all 51 opt-in Godot smokes, and the bounded graphical launch.

The restricted sandbox reproduces the known .NET IPC/VSTest denial; identical host-local commands
pass. GitHub Actions budget is exhausted for the month: do not dispatch hosted workflows, and treat
zero-step hosted failures as account infrastructure rather than test results.

## Exact next bounded slice

Take **Phase 4b14: close the assembler's remaining generic filtered-entry input**.

Acceptance boundary:

1. Replace `filteredEntries` with separate memory-entry and valid-affordance-entry inputs; accept
   only exact `Memory` and `ValidAffordance` kinds respectively and reject null collections, null
   entries, and every mismatched kind.
2. Assemble in roadmap order: derived perception, derived knowledge, derived goals, supplied
   memories, derived recent speech, supplied valid affordances, then exact untrusted player
   dialogue. Retain each accepted supplied entry instance and its stable source order.
3. Prove empty inputs, independent source copying, spoofed-kind rejection, bounded aggregate
   failure without input mutation, and the existing character/perception/knowledge fail-closed
   behavior.
4. Do not yet define memory storage or a conversation window, claim that structural intention
   validation proves current affordance eligibility, derive affordances, call a provider, wire
   Godot, validate/execute generated intentions, add retries/timeouts/credentials/logging, persist,
   or mutate gameplay state.
