# Allrpcs Handover

## Current state

Phases 2m23–2m25 are implemented on `codex/phase-2m23-plus-passive-noticing`, based
directly on PR #124's merge commit `e228d90`.

The temporary Seeker perception source now consumes the exact retained passive-noticing candidate
and resolves its value-equal target through the authoritative world-object snapshot. It retains
every exact source object identifier in stable order, seeds only the resolved target at `Unnoticed`,
retains the candidate's exact observer identifier, and seeds every other object at `Noticed`. An
absent candidate target fails explicitly before perception construction; no greybox marker,
authored identity text, coordinate, focus, or fallback supplies the target.

During `Main._Ready`, after exact world-object and perception initialization, the scene resolves
the candidate target through the live world-object controller and produces it from the exact source
candidate, complete temporary character, and looked-up world object. It then invokes the existing
stable resolve-next boundary exactly once. The atomic result changes only the live target from
`Unnoticed` to `Noticed`, consumes the complete candidate, and leaves the source candidate and
source perception unchanged. Focused-search state, movement, input, and presentation remain
unchanged; there is no polling, distance, line-of-sight, timing, or new UI authority. The roadmap's
bounded passive-noticing item is complete.

All three slices were test-first. Phase 2m23 failed the focused perception-source smoke on the old
all-`Noticed` seed; Phase 2m24 failed the candidate-source smoke while ordinary play still retained
no candidate; and Phase 2m25 failed the same end-to-end smoke before atomic resolution was wired.
After final code review, host-shell `./scripts/quality.sh` passed workflow and formatting
validation, a zero-warning build, 801 Domain tests, 107 Application tests, six architecture tests,
all eighteen Godot smokes, and the bounded graphical launch.

## Exact next bounded slice

Take **Phase 2n1: immutable observation-reinterpretation candidate**.

Acceptance boundary:

1. Add one sealed immutable `Allrpcs.Domain.Discovery.ObservationReinterpretationCandidate`
   containing only an existing `CharacterId ObserverCharacterId`, `ObjectId TargetObjectId`, and
   `KnowledgeId TriggerKnowledgeId`.
2. Reject any null input and otherwise retain all three exact supplied identifier instances. Add no
   custom equality or behavior beyond the identifiers' existing value semantics.
3. Add focused tests first for exact nested-instance retention, distinct value-equal identifiers,
   independently different observer/target/knowledge values, all null boundaries, unchanged inputs,
   and the minimal three-property public API surface.
4. Update README and ROADMAP evidence, keep knowledge-triggered reinterpretation incomplete, run
   `./scripts/quality.sh`, review every intended path, and prune this file again to current evidence
   plus one next slice.

Explicit exclusions:

- Do not collect, project, look up, select, rank, order, or evaluate reinterpretation candidates;
  inspect character knowledge or perception; define trigger satisfaction or a resulting visibility
  state; reinterpret an observation; change perception; or grant knowledge, evidence, or discovery.
- Do not add Application/Godot sources, controllers, runtime wiring, events, persistence,
  serialization, providers, plugins, credentials, or UI.
- Do not change passive noticing, focused-search lifecycle behavior, world-object or character
  ownership, movement, input, or presentation.
- Keep point-and-click movement, disease-time pausing, and the world-time debug overlay deferred.
- Do not merge, release, distribute, sign, notarize, or change repository visibility without
  explicit user authorization.
