# Allrpcs Handover

## Current state

`main` contains merged PR #188 at `709b3d7`. The current branch is
`codex/phase-6-disease-foundation`.

Phase 6 now has canonical hidden disease truth independent of character belief, memory, dialogue,
and providers. `DiseaseDefinitionSnapshot` retains up to 32 exact unique definitions;
`DiseaseTruthSnapshot` retains up to 256 stable exact character-disease infections and requires each
to reference its exact authored definition. The six closed stages are exposed, early illness,
drowsing, deep sleep, wasting, and final decline.

One pure transition advances an exact infection by exactly one stage only after an exact authored
`Advance` world event is retained, recorded, and current. Per-infection event provenance prevents
replay, final decline cannot progress, and unrelated infection instances remain exact. Application
projections select a dormant effective controller from deep sleep onward without replacing the
configured assignment, and expose only fixed nonnumeric qualitative conditions while keeping the
exposed stage and disease identity hidden. There is no scene ownership, persistence, automatic
progression, transmission, assessment, treatment, cure, ingredient, experiment, journal, or
model-context wiring yet.

## Validation

- workflow validation, restore, formatting, and zero-warning complete solution build
- 1,571 Domain, 386 Application, 423 Infrastructure, and 8 architecture tests
- representative discovery scenario: 5 valid evidence definitions through the rebuilt validator DLL
- all 77 credential-free Godot smokes
- bounded 60-frame Compatibility-renderer launch on Godot 4.6.3 .NET

Draft PR #189 is open from this branch. Its first hosted run `34566859552` ended with all five jobs
reporting `steps: []`; no checkout or test step ran. Treat that as unavailable account/infrastructure
evidence, not a product failure, and do not retry or dispatch it without a changed external condition.

## Exact next bounded slice

Take **Phase 6b3: successful-cure removal and configured-controller restoration**.

Acceptance boundary:

1. Add the smallest immutable successful-cure outcome contract with exact authoritative infection and
   exact authored world-event provenance; do not infer cure from provider text or a condition label.
2. Add a pure eligibility policy and transition that require the exact retained infection and exact
   occurred authoritative cure event, remove only that infection, and retain the exact definitions
   and every unrelated infection in stable order.
3. Prove that the existing effective-controller selector returns the exact configured assignment
   after the successful cure removes the last deep infection, while another deep infection keeps the
   controller dormant.
4. Do not yet define remedy recipes or cure discovery, comfort/stabilization semantics, transmission,
   protection, assessment, ingredients, experiments, persistence, scene wiring, UI, or provider tools;
   leave the broad treatment and controller-roadmap items unchecked until their full mechanics exist.
