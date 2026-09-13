# Allrpcs Handover

## Current state

`main` contains merged PR #229 at `eeb71a3`. The current branch is
`codex/phase-7-concealment-selection` and advances three bounded Phase 7 slices:

- Phase 7di adds `CharacterObjectSabotageConcealmentSelection`, which fully materializes and
  validates at most 64 ordered evidence-bound candidates, rejects null entries and value-equal
  duplicate evidence identifiers, and returns the exact first candidate accepted by the existing
  complete concealment policy or `null`;
- Phase 7dj composes that selection with the existing immutable concealment transition as one pure
  `CharacterObjectSabotageConcealmentSequence` result, failing before any result when no candidate
  is eligible;
- Phase 7dk exposes first-eligible selection and application through the existing scene-owned
  concealment controller, preserving exact concealment/evidence compare-and-swap and installing no
  stale, absent, or partial result.

No slice selects when sabotage occurs. The temporary source and flow still expose and explicitly
apply one authored candidate directly. This work does not add automatic or model-selected behavior,
another sabotage kind, input/UI/events/time/transcripts/persistence, or provider activity.

## Validation and review

- Each slice began with an expected compile failure for its absent contract or scene seam.
- Focused host-local Domain tests pass: 22 concealment tests cover complete collection validation,
  stable exact-instance selection, every required ineligible case, immutable sequence application,
  null/duplicate/over-bound rejection, and unchanged inputs.
- The focused host-local Godot controller smoke passes exact first-eligible selection after an
  ineligible candidate, stale-state rejection, atomic state installation, and the existing explicit
  apply/recovery behavior. The restricted run reproduced the known `user://logs` crash before
  project code; the identical host-local command passes.
- Host-local `./scripts/quality.sh` passes workflow validation, restore, formatting, zero-warning
  build, 2,286 Domain tests, 478 Application tests, 532 Infrastructure tests, 8 architecture tests,
  scenario validation, every credential-free headless Godot smoke, and the Compatibility-renderer
  graphical launch.
- All changed authored C# files are below 500 lines and use no lines longer than 120 characters.
  `git diff --check` passes. No live-conformance switch was enabled and no provider was invoked.

## Exact next bounded slice

Take **Phase 7dl: route the authored temporary sabotage flow through one bounded candidate
collection**.

Acceptance boundary:

1. Extend `TemporaryCharacterObjectSabotageSource` to retain one stable read-only candidate
   collection bounded by `CharacterObjectSabotageConcealmentSelection.MaximumCandidateCount`; its
   only entry must be the exact existing water-cask candidate without reconstructing it.
2. Route the existing explicit `TemporaryCharacterObjectSabotageFlowController.TryApply()` through
   `CharacterObjectSabotageConcealmentController.TryApplyFirstEligible` with that exact collection.
3. Prove in the focused source, flow, presentation-flow, and main-scene smokes that initialization
   still applies nothing automatically, the explicit call selects and installs the exact cask
   candidate, repeat application fails closed, and recovery remains unchanged.
4. Preserve `Candidate` while it is required by recovery/presentation seams; do not duplicate the
   candidate, introduce another authored sabotage behavior, or broaden existing node surfaces
   unnecessarily.
5. Do not choose timing, trigger sabotage automatically, invoke a model, add input/UI/events/time/
   transcripts/persistence, move an actor or object, infer physical access from coordinates, consume
   evidence, or add non-conceal effects.
