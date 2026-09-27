# Allrpcs Handover

## Current state

This direct-main continuation completes the representative ordinary first-patient recovery loop after
the investigation-route completion work that was already on `main` at `dbdc63f`:

- Physical `P` input at the exact healer-garden station advances one authored preparation transition
  per press and installs the single full first-remedy batch. Apothecary access remains optional.
- Physical `T` input beside the exact patient administers that batch once through the existing
  application controller, records the authored administration event and experiment, and atomically
  stabilizes the patient.
- Exact stabilization emits one fixed provider-neutral ordinary patient testimony to the in-range
  Seeker. Physical `K` input beside the sage then emits one fixed provider-neutral recovery hint from
  the exact heard testimony and latest exact administration.
- The representative journal copies the patient testimony and sage consultation in order without
  duplicating the dialogue controller's latest event. It retains only safe authored content.
- `ROADMAP.md` now marks the first-patient stabilization/testimony and non-blocking sage-consultation
  objectives complete. The final Phase 8 playtest/tuning objective remains open.

## Validation and review

- Post-review host-local `./scripts/quality.sh` passed formatting, a zero-warning build, 2,827 Domain,
  916 Application, 1,255 Infrastructure, and 8 architecture tests, the five-evidence scenario
  validator, every credential-free Godot smoke, and the graphical Compatibility launch.
- The four new ordinary-scene smokes cover preparation, administration, testimony, and consultation
  startup, exact provenance/range, one-shot behavior, rejection paths, and unrelated-state isolation.
- Session review checked the complete diff, scene order, mapped input, fixed-copy/provider boundary,
  exact-reference authority, journal order, documentation, whitespace, 120-character limits, and
  practical file sizes. It fixed ambiguous multi-batch input, tied testimony to the exact treatment
  context, accepted the latest exact administration after earlier experiments, and preserved both
  treatment dialogue entries in the journal. No known issue remains inside this session's boundary.
- Restricted direct Godot execution still hits the known `user://logs` sandbox boundary; identical
  host-local smokes and the full host-local quality run passed.

## Preserved boundaries and known continuation gap

- No provider, hidden disease truth, conclusion inference, route choice, clue ordering, background
  requirement, production UI, or save schema was added. No manual playtest or release claim is made.
- Preparation activity and the scene coordinators' one-shot testimony/consultation tokens are not new
  persistence fields. Existing safe journal content still persists through the current schema.
- Save restoration rebuilds first-patient state while ordinary treatment input intentionally uses
  exact live provenance. Save/reopen continuity at the new preparation, administration, testimony,
  and consultation boundaries has not been proven and must not be inferred from the automated fresh
  scene smokes.
- Do not release, sign, notarize, publish a build, invoke credential-backed providers, change remote
  services, or make other remote changes without separate authorization.

## Exact next bounded slice

Prove and, only where required, close save/reopen continuity for the new ordinary first-patient
recovery loop. Stop before general Phase 8 tuning, new UI, additional dialogue, or provider work.

Acceptance boundary:

1. Add switch-gated restore-path coverage for saves captured before preparation, with a full prepared
   batch before administration, immediately after stabilization, after patient testimony, and after
   sage consultation. Each restored state must either continue the exact remaining steps once or
   remain correctly complete without replaying an already persisted safe journal entry.
2. Rebind restored state to the current authored treatment context through existing canonical
   definitions. Do not weaken exact-reference checks, compare by free text, or let a rebuilt/foreign
   recipe, patient, disease, batch, event, testimony, or consultation acquire authority.
3. Prefer the existing schema when it already contains enough durable truth. Add only the minimum
   explicit durable marker if an exact one-shot boundary cannot otherwise be reconstructed safely;
   migrate older versions to the inert/default meaning and cover serialization and atomic restore.
4. Rejected malformed, stale, rebuilt, foreign, repeated, and incompatible restores must preserve
   disease truth, batches, both experiment journals, world events, safe journal order, map and route
   state, dialogue, movement, and unrelated presentation.
5. Update the handover with the observed restore behavior and leave the first instrumented manual
   end-to-end Phase 8 playtest as the next slice. Do not mark the remaining roadmap playtest complete
   from automated evidence alone.
