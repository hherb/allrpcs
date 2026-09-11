# Allrpcs Handover

## Current state

`main` contains merged PR #201 at `f3ea7b9`. The current branch is
`codex/phase-6-first-patient-restoration`.

This branch completes the remaining Phase 6 persistence work:

- pure first-patient restoration preflight rejects any saved disease, infection, preparation
  recipe, ingredient, ordered step, quantity unit, patient administration, or experiment event
  that cannot resolve compatibly against the exact authored treatment context and world events;
- successful preflight returns a fresh immutable graph rebound to exact authored disease,
  character, recipe, plan, step, ingredient, quantity, and event instances while preserving saved
  order, dynamic disease/care state, quantities, observations, and journal text;
- schema versions 1 and 2 migrate in order to version 3 using caller-owned initial first-patient
  state, while an existing version-3 document remains unchanged;
- the complete root policy preflights discovery, world events, memory, and first-patient state
  before scene controllers atomically install the rebound graph;
- live capture, canonical JSON, and local storage now carry complete version-3 first-patient state,
  including shared preparation and event references across remaining batches and both journals.

The final Phase 6 roadmap item is complete. This branch does not add Phase 7 roles or scheduling,
new UI, autosave or startup loading, save slots, provider calls, releases, signing, or distribution.

## Validation and review

- `./scripts/quality.sh` passed cleanly:
  - workflow validation, restore, formatting, and zero-warning solution build passed
  - 1,935 Domain, 402 Application, 447 Infrastructure, and 8 architecture tests passed
  - the representative discovery scenario validated 5 evidence definitions
  - all 82 credential-free Godot smokes passed, including populated first-patient JSON restoration
  - the bounded 60-frame Compatibility-renderer launch passed
- focused restoration/load-policy and JSON migration tests passed (25 Domain and 22 Infrastructure)
- `git diff --check`, practical 500-line/120-character limits, exact-identity, stable-order,
  no-silent-discard, strict-input, atomic-installation, credential-free, and network-free review passed

The restricted focused VSTest run reproduced the known localhost-bind restriction; the identical
host-local command passed. A restricted Godot smoke reproduced the known `user://logs` failure; the
identical host-local command passed. Hosted jobs for recent PRs still had `steps: []` under the
Actions account-budget condition, so do not retry or dispatch them unless that condition changes.

## Exact next bounded slice

Take **Phase 7a: pure representative hermit character definitions**.

Acceptance boundary:

1. Add one pure Domain source that creates exactly two fresh immutable hermit `Character` values in
   stable order with distinct canonical identities and exact LLM controller assignments.
2. Give each hermit a distinct non-empty private background, private knowledge identity, and motive
   represented by existing goal/belief vocabulary; prove neither hermit's private state appears in
   the other's composition.
3. Add focused Domain tests for exact internal identity alignment, stable order, distinct private
   knowledge and motives, fresh repeated compositions, and the smallest public API surface.
4. Do not add Godot nodes, dialogue scheduling, model generation, provider calls, new character
   vocabulary, autonomous actions, sabotage, UI, persistence changes, Phase 8 work, release, or
   distribution.
