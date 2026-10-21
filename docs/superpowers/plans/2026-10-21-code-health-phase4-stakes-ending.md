# Phase 4 item 5: stakes and ending

Owner authority: `owner-playable-phase4-2026-10-19` in `CONTINUATION.json`, under program Phase 4
item 5 (slice step 8) in [the program plan](2026-10-17-code-health-and-playable-slice.md).

## Measured state (at `c6455ab8`)

- **Progression exists only in Domain.** `DiseaseStage` runs `Exposed → EarlyIllness → Drowsing →
  DeepSleep → Wasting → FinalDecline`; there is no dead or lost stage. `DiseaseProgressionTransition`
  moves one stage on an occurred `Advance` world event that is the current world time. It refuses a
  stabilized patient and `FinalDecline`. Nothing in Application or game calls it. Exposure, cure and
  protection are likewise only used by tests. The first patient starts at `DeepSleep`, so two
  progressions reach `FinalDecline`.
- **Time is event-sourced.** No clock or journey type exists. "Time passing" means an `Advance`
  world event: herb identified, the Healer's examination and collection, every repeatable
  `herb-patch-examination-N`, and the two dose events. `WorldEventController` owns the log. A normal
  successful path has about four `Advance` events, the dose included; each extra examination adds one.
- **Truth is pinned to the authored scenario.** `FirstPatientTreatmentContext.Scenario.DiseaseTruth`
  is the authored snapshot. The stabilization policy reads it, a wrong dose writes it back, and four
  game guards require `ReferenceEquals(context.Scenario.DiseaseTruth, liveTruth)`. A progressed truth
  would therefore block preparing and dosing, and a wrong dose would undo the progression.
- **Only two doses exist.** Each dose event can occur once. After two wrong doses the game shows
  "the patient cannot take another dose" and is stuck (a `shortcut:` in `TemporaryWorldEventSource`
  defers this to item 5).
- **No ending.** After stabilization the status line leads to the testimony and then the sage
  ("Sage consulted. Press Q to save."). No end, completion or failure UI exists.
- **Save.** Stage, `lastProgressionEventId` and care state are already in the schema-10 save, as are
  the event log, dose records and recovery stage (`SageConsulted`).

## Change

1. **Live treatment context (Domain).** `FirstPatientTreatmentContext.WithDiseaseTruth(truth)` returns
   the same context for the same truth, or a context over a scenario that shares every authored
   reference and holds the live truth. The game guards rebind the authored context to the live truth
   instead of requiring the authored instance. Domain transitions are unchanged.
2. **Progression schedule (Application).** `FirstPatientDiseaseProgressionSchedule`: the patient
   worsens one stage once `StageInterval` (3) `Advance` events have occurred since the last progression
   (or since the start). A wrong dose is an `Advance` event, so it costs time; a remedy dose stabilizes
   first, and a stabilized patient never progresses.
3. **Slice outcome (Application).** `FirstPatientSliceOutcomePolicy` derives `InProgress`, `Completed`
   (stabilized and the sage consulted), `LostToDecline` (at `FinalDecline` and another interval has
   passed) or `LostAfterDoses` (both doses spent without stabilization). It is derived from saved truth,
   log, dose records and recovery stage, so no schema change is needed and a save cannot contradict
   itself. `SliceEndingCopy` authors the end screen's title and text.
4. **Stakes node (game).** `FirstPatientStakesController` polls each frame: it installs a due
   progression into `FirstPatientStateController`, shows the patient's worsened condition
   (`DiseaseConditionProjection`) on the recovery status line, and opens the end panel when the
   outcome first becomes terminal. A load re-presents a terminal outcome. Polling (not an event
   hook) keeps dose ordering simple: the dose result is installed before the next frame evaluates it.
   The node is active only in a plain launch (the same gate as new-game customization) or when a
   check enables it, so existing proof scenes keep their stable patient.
5. **End panel (game).** `SliceEndPanel` in a new `CanvasLayer`, listed as a reading modal in
   `ControlInputContext`. Completion offers *Keep exploring* and *Return to title*; a loss offers
   *Return to title*, which reloads Main and so reaches Continue/New game.
6. **Proof.** `SliceStakesEndingFlowCheck` (`ALLRPCS_SLICE_STAKES_CHECK`, isolated save path). Fresh
   Main with stakes enabled: three examinations worsen the patient to `Wasting`; a save and Continue
   keep it; three more reach `FinalDecline`; three more lose the patient and open the loss panel; a
   Continue of the earlier save then completes the slice through the remedy, testimony and sage, and
   the completion panel survives a save and Continue. A second scenario spends both doses wrong and
   shows `LostAfterDoses`.

Decided while implementing:

- The end panel is a full-screen backdrop that swallows clicks, with the dialog box centred in it.
  Review found that a centred box left the overlay's Save button clickable, so a lost game could
  overwrite the only save. Return to title stays the way out.
- The proof drives time with events that are consistent on their own: identification, the Healer's
  errand and wrong doses. Restore cross-checks history, so logging the collection event without
  the gathered herb is rejected. The decline loss is proven by a Wasting save edited to final
  decline with three examination events logged; the flow check never edits a save otherwise.
- The shared flow-check steps (customize, station presses, dose, map) moved to
  `MainFlowCheckSteps`, used by the ingredient, hypothesis and stakes checks.
- New nodes sit before `WholeChannelPresentation`, which must stay Main's last child.

Out of scope: exposure, cure and protection stay unwired (no second patient or contagion in the
slice). No pause rule (no event occurs during reading). No boot-time auto-resolution change (item 6),
no new sabotage kind, no provider qualification, no merge or release. `StageInterval` is a first
guess for the owner playtest (item 7) to tune.

## Verification

`dotnet format`/`build`/the four test projects. New xUnit tests cover `WithDiseaseTruth`, the
schedule and the outcome policy. Then the new check, the first-patient, administration, save/load,
ingredient, hypothesis and new-game launches, and the full smoke suite.
