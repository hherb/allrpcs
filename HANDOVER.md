# Allrpcs Handover

## Current state

`main` contains merged PR #200 at `f188b82`. The current branch is
`codex/phase-6-first-patient-json`.

This branch completes two bounded Phase 6 persistence slices:

- **Phase 6y9** adds canonical version-3 first-patient JSON serialization.
- **Phase 6y10** adds strict version-3 first-patient JSON deserialization.

The document writes complete disease definitions and infections, a stable registry of completed
preparations, retained prepared batches, administration-specific experiment records, and universal
experiment records. Document-local preparation indices preserve exact shared preparation identity
without serializing runtime object references. Supplied order, optional progression events, units,
qualitative states, identifiers, observed conditions, and authored procedure/outcome text remain
exact.

The strict reader reconstructs the complete immutable graph, including exact disease-definition,
preparation, and repeated world-event references. It rejects unknown or missing structure, invalid
or non-canonical numbers, out-of-range preparation references, incomplete preparations, conflicting
shared event definitions, invalid closed vocabulary, duplicate identities, and existing Domain
bounds violations. Versions 1 and 2 retain their exact prior JSON documents and behavior.

This branch does not enable version-3 load policy; execute version-2 migration during scene load;
rebind deserialized state to exact scene-authored disease, recipe, ingredient, or event definitions;
restore first-patient controllers; change storage or live scene capture; add UI; call a provider;
begin Phase 7; release; or distribute.

Draft PR #201 contains this work.

## Validation and review

- 6 focused version-3 persistence tests passed, alongside 7 existing serializer tests
- complete representative version-3 state round-tripped byte-for-byte with shared references
- strict malformed-shape, bad-reference, incomplete-preparation, conflicting-event, and canonical
  number rejection passed
- `./scripts/quality.sh` passed cleanly:
  - workflow validation, restore, formatting, and zero-warning solution build passed
  - 1,928 Domain, 402 Application, 446 Infrastructure, and 8 architecture tests passed
  - the representative discovery scenario validated 5 evidence definitions
  - all 82 credential-free Godot smokes passed
  - the bounded 60-frame Compatibility-renderer launch passed
- `git diff --check`, practical 500-line/120-character limits, exact-identity, stable-order,
  no-silent-discard, strict-input, minimal-surface, credential-free, and network-free review passed

The restricted focused VSTest run reproduced the known localhost-bind restriction; the identical
host-local command passed. Draft PR #201 automatically triggered run `34653632459`; all five jobs
completed with `steps: []` under the unchanged Actions account-budget condition. Do not retry or
dispatch hosted workflows unless that external condition changes.

## Exact next bounded slice

Take **Phase 6y11: pure authored first-patient restoration preflight and rebinding**.

Acceptance boundary:

1. Add one pure Domain transition that accepts a deserialized `FirstPatientSaveState`, the exact
   scene-authored initial first-patient treatment context, and authoritative world-event definitions.
2. Fail closed unless every saved disease, recipe, ingredient, preparation step/unit, infection,
   administration, and experiment event resolves uniquely to compatible authored definitions.
3. Return a fresh immutable first-patient composition rebound to the exact authored disease,
   ingredient, recipe, preparation-step, quantity-unit, and world-event instances while preserving
   saved order, quantities, disease progress/care, observations, and journal text. Prove exact
   identity, unchanged inputs, and atomic failure with focused Domain tests.
4. Do not yet enable version-3 load policy, execute migration in the JSON controller, install state
   into scene controllers, change save capture/storage, add UI, or broaden authored scenario data.
