# Allrpcs Handover

## Current state

`main` contains merged PR #190 at `f8eefe0`. The current branch is
`codex/phase-6-patient-assessment`.

Phase 6 now includes bounded hidden disease truth, authored meaningful-event progression,
qualitative condition projection, dormant-controller selection, exact-event cure and exposure,
discoverable protection, patient assessment, and supportive care.

Patient assessment retains exact assessor/patient identities and succeeds only for a distinct
perceived patient, the matching exact assessor, an authored `assess-patients` capability, and at most
32 inspected capabilities. It returns only deduplicated fixed qualitative conditions for that
patient. Exposure, disease identity, stage, diagnosis, beliefs, and treatment authority stay hidden.

Every infection has a closed unassisted, comforted, or stabilized care state. Exact authored current
events can monotonically apply comfort or stabilization to one exact infection. Comfort does not stop
progression; stabilization does. The existing explicit cure removes the infection. No provider text
or qualitative condition can create any outcome.

There is no disease scene ownership, persistence, automatic progression/transmission, protection
duration/removal, treatment discovery, ingredient/preparation model, experiment journal, conclusion
system, UI, or provider wiring.

## Validation

- complete `./scripts/quality.sh` passed: workflow validation, restore, formatting, and zero-warning
  solution build
- 1,626 Domain, 393 Application, 423 Infrastructure, and 8 architecture tests passed
- representative discovery scenario validated 5 evidence definitions
- all 77 credential-free Godot smokes passed
- bounded 60-frame Compatibility-renderer launch passed on Godot 4.6.3 .NET

Draft PR #191 is open from this branch. Its initial hosted run `34573907536` ended with all five
jobs reporting `steps: []`; no checkout or test step ran. Treat that as unavailable
account/infrastructure evidence, not a product failure, and do not retry or dispatch it without a
changed external condition.

## Exact next bounded slice

Take **Phase 6f1: ingredient identity and quantity foundation**.

Acceptance boundary:

1. Add a canonical ingredient identity distinct from world-object identity and the smallest immutable
   authored ingredient definition retaining that exact identity.
2. Add an immutable positive bounded quantity with an explicit closed unit vocabulary suitable for
   later dosage work; validate limits without floating-point arithmetic or implicit unit conversion.
3. Add focused reflection-backed tests for exact-instance retention, singleton vocabulary parsing,
   numeric boundaries, nulls, and a minimal public API.
4. Do not yet add freshness, physical condition, contamination, preparation steps, recipes, dosage,
   administration, inventory/scene/persistence/provider wiring, experiments, journal behavior, or
   treatment discovery; keep the broader ingredient roadmap item unchecked.
