# Allrpcs Handover

## Current state

Phases 2n1–2n4 are implemented on
`codex/phase-2n1-plus-observation-reinterpretation`, based directly on current `origin/main` merge
commit `b81a252` after PR #125 merged.

The Domain now has one sealed immutable `ObservationReinterpretationCandidate` containing only an
existing observer `CharacterId`, target `ObjectId`, and triggering `KnowledgeId`. It rejects nulls,
retains every exact supplied identifier instance, and relies only on the identifiers' existing
value semantics.

An immutable `ObservationReinterpretationCandidateSnapshot` copies supplied candidates into a
read-only list in stable order. It permits an empty snapshot and candidates sharing any proper
subset of their three identifiers, retains exact candidate instances, and rejects null input, null
entries, and value-equal complete duplicates.

The pure `ObservationReinterpretationCandidateProjection.ForCharacter` returns a new snapshot with
every candidate for a value-equal observer in stable source order, retaining exact candidate and
nested identifier instances. The pure `ObservationReinterpretationCandidateLookup.Find` matches
all three identifier values and returns the exact retained candidate or `null`; every partial match
is rejected. Neither boundary mutates its inputs.

Each slice was test-first: its focused test project build failed on the missing production type
before the minimal implementation was added. After Phase 2n4 and code review, host-shell
`./scripts/quality.sh` passed workflow and formatting validation, a zero-warning build, 828 Domain
tests, 107 Application tests, six architecture tests, all eighteen Godot smokes, and the bounded
graphical launch. Knowledge-triggered reinterpretation remains incomplete.

## Exact next bounded slice

Take **Phase 2n5: knowledge-trigger satisfaction policy**.

Acceptance boundary:

1. Add one pure static
   `Allrpcs.Domain.Discovery.ObservationReinterpretationCandidateTriggerPolicy` with one
   `CanTrigger(ObservationReinterpretationCandidate candidate, Character character)` operation.
2. Return true only when the complete character's identity is value-equal to the candidate's
   observer identifier and its existing `CharacterKnowledge.KnowledgeReferences` contains a
   value-equal trigger knowledge identifier. Accept distinct value-equal identifier instances.
3. Return false for a wrong character, empty knowledge, absent trigger knowledge, and partial
   combinations. Reject either null input. Leave the candidate, character, every complete
   character component, knowledge collection, order, and nested instance unchanged.
4. Add focused tests first for those boundaries and the minimal single-operation public API.
   Update README and ROADMAP evidence, keep reinterpretation incomplete, run
   `./scripts/quality.sh`, review every intended path, and prune this file again to current evidence
   plus one next slice.

Explicit exclusions:

- Do not inspect perception or visibility; require a target perception entry; define a resulting
  visibility state; decide whether reinterpretation would change anything; reinterpret an
  observation; change perception; or grant knowledge, evidence, or discovery.
- Do not collect, project, look up, record, produce, select, rank, order, apply, consume, or resolve
  candidates, and do not introduce a knowledge lookup or mutate character state.
- Do not add Application/Godot sources, controllers, runtime wiring, events, persistence,
  serialization, providers, plugins, credentials, or UI.
- Do not change passive noticing, focused-search lifecycle behavior, world-object or character
  ownership, movement, input, or presentation.
- Keep point-and-click movement, disease-time pausing, and the world-time debug overlay deferred.
- Do not merge, release, distribute, sign, notarize, or change repository visibility without
  explicit user authorization.
