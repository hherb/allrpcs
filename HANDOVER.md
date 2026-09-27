# Allrpcs Handover

## Current state

`main` includes merged PR #413 at `2c694a5` and the direct-main continuation that completes the
representative Phase 8 investigation-route loop:

- The first exact token in
  `OpeningRegionInvestigationRouteCompletionCoordinator.AcceptedCompletions` is projected only when
  no completion has been consumed. Later, rebuilt, unaccepted, stale, repeated, null, and
  restored-consumed inputs remain inert.
- `OpeningRegionInvestigationRouteCompletionPresentationProjection` maps only the three canonical
  route keys to fixed authored title/body copy. Its safe snapshot exposes only those two strings: no
  route/progress object, evidence text, provider content, or hidden state reaches the UI.
- The ordinary greybox panel renders only its coordinator's exact current safe snapshot. Its explicit
  Acknowledge control must consume the same bound accepted token through the existing consumption
  coordinator before clearing. Failed consumption leaves the exact presentation visible.
- Successful acknowledgement retains ordered accepted tokens, route progress, discovery, map,
  journal, perception, world objects, and save schema 7. A consumed first token permanently prevents
  a later accepted completion from replacing the one-shot flow.
- `ROADMAP.md` now marks multiple authored investigation routes complete. Testimony, stabilization,
  sage recovery, objectives, clue ordering, disease pressure, and additional evidence remain
  unchanged.

## Validation and review

- Host-local `./scripts/quality.sh` passed workflow validation, formatting, a zero-warning build,
  2,827 Domain, 911 Application, 1,255 Infrastructure, and 8 architecture tests, the five-evidence
  scenario validator, every credential-free Godot smoke, and the graphical Compatibility launch.
- A restricted direct Godot run reached the known `user://logs` sandbox boundary. The identical
  host-local success, failed-consumption, and restored-consumed presentation smokes all passed.
- Session review inspected the complete implementation, tests, scene order, fixed-copy boundary,
  exact first-token binding, consumption-before-close ordering, failed-consumption retention,
  restored-consumed behavior, later-token preservation, documentation, formatting, 120-character
  limits, and whitespace. No review issue remains.

## Preserved boundaries

- Route progress is initialized only by explicit map open or save restoration. Evidence discovery
  still does not initialize missing route state or roll back successful discovery.
- Completion presentation does not write testimony, prepare or administer a remedy, alter disease
  truth, infer conclusions, choose clue order, call a provider, or change persistence schema.
- The first-patient treatment context remains its existing authored evidence-confirmed source; this
  slice does not claim ordinary investigation-conclusion wiring.
- Do not release, sign, notarize, publish a build, run credential-backed provider activity, or make
  other remote changes without separate authorization.

## Exact next bounded slice

Add one provider-neutral ordinary player flow that prepares the already-authored first-patient remedy
at the exact healer-garden preparation station and installs its full prepared batch. Stop before dose
administration, patient stabilization, testimony, sage recovery, or persistence changes.

Acceptance boundary:

1. Add one explicit mapped preparation input. It is inert unless the Seeker is within the exact
   current `OpeningRegionRemedyPreparationSceneBinding` station range and the scene retains the exact
   current treatment context, recipe, ingredient state, and first-patient state.
2. Delegate only to `FirstPatientPreparationController`: start the exact recipe once, advance only
   its exact next authored grinding/heating/steeping/cooling step, complete it once, and then install
   that exact outcome through `FirstPatientStateController.TryAddPreparedBatch`.
3. Do not make the Apothecary background mandatory for scenario completion. Preserve its existing
   optional route-access token without treating that token as sole preparation authority or changing
   any other background route.
4. Rejected far, stale, rebuilt, wrong-context, wrong-recipe, out-of-order, repeated, released, and
   unrelated input must preserve ingredients, activity, outcome, prepared batches, disease truth,
   experiment journals, world events, movement, map, journal, and route state.
5. Add a switch-gated ordinary-scene smoke for inert startup, exact range/input progression, one full
   batch installation, rejection paths, and unrelated-state isolation. Update `README.md`, run focused
   checks and `./scripts/quality.sh`, review the full diff, and leave one exact next slice for explicit
   dose administration and stabilization.
