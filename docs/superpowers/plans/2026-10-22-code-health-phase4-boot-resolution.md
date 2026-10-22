# Phase 4 item 6: stop resolving evidence at boot

Owner authority: `owner-playable-phase4-2026-10-19` in `CONTINUATION.json`, under program Phase 4
item 6 in [the program plan](2026-10-17-code-health-and-playable-slice.md). Owner decisions taken
for this slice on 2026-10-22: the remedy dose confirms the conclusion, and arriving at the stream
notices its residue.

## Measured state (at `5a6d3933`)

- **Three boot resolutions.** `Main.InitializeSeekerPerception` (`game/Main.Initialization.cs`)
  resolves, before any input:
  1. a passive-noticing candidate from `TemporaryPassiveNoticingCandidateSource` (world object 0,
     `herb-patch`: Unnoticed → Noticed). Nothing in play reads the result: the HUD label and the
     journal show only Identified objects, and focused search identifies from any state.
  2. the stream residue (`OpeningRegionStreamResiduePassiveNoticingCoordinator.TryResolve`). It keeps
     a residue evidence view; route progress does not exist at boot, so nothing is routed until the
     first map open, which routes it. **The map refuses to open without that view**
     (`SeekerMapOpenCloseInputController.TryEnsureRetainedStreamResidueIsRouted`).
  3. an observation reinterpretation from `TemporaryObservationReinterpretationCandidateSource`
     (world object 1, `stone-marker`: Noticed → Identified), which puts its HUD label on screen and
     a stone-marker entry in the journal before the player has done anything.
- **No play path notices anything passively.** The produce/resolve methods are called only at boot.
  Focused search identifies objects (and routes the wagon clue); the layered inscription routes the
  shrine evidence; map visits verify route locations (`OpeningRegionMapVisitVerificationCoordinator`).
- **The remedy's conclusion is pre-confirmed.** `RepresentativeFirstPatientTreatmentContextSource`
  builds `FirstPatientTreatmentContext` with the plan's only conclusion already in `Conclusions`.
  `StabilizingRemedyAdministrationPolicy` requires it (`FirstPatientTreatmentContextPolicy
  .IsAuthorized`), and so does save restoration. None of the plan's four evidence ids
  (`laundry-cooling-residue`, …) is produced anywhere, so play could never confirm it.
- **Item 4 already derives the answer from play.** A remedy dose confirms the proposed moonwort
  hypothesis (`FirstPatientHypothesisTestPolicy`), which closes the journal's open question
  (`JournalInvestigationTransition`). Outcomes come from saved dose records.
- **Saves.** `DiscoverySaveState` stores the Seeker perception and both candidate lists; route
  progress stores recorded evidence ids and needs no visibility to restore. The residue coordinator's
  own state is never saved. A save needs route progress, which is created with the map on the first
  map open, so every existing save already records residue.
- **Checks on the boot state** (all hosted on Main): `FocusedSearchPerceptionController`,
  `PassiveCandidateSource`, `ReinterpretationCandidateSource`, `SeekerPerceptionSource`,
  `InteractionFocus` (initial labels), `StreamResidueRoute` (`missing`/`route`) and
  `SeekerMapOpenCloseInput` (residue recorded on first open). No xUnit test boots Main.

## Change

1. **The dose is the experiment (Domain).** `FirstPatientTreatmentContext` loses `Conclusions`; the
   authored context holds only the scenario and the remedy-to-conclusion requirement.
   `FirstPatientTreatmentContextPolicy.IsAuthorized` becomes `IsConsistent` (the requirement names
   the scenario's remedy and an authored conclusion) and no longer asks for a confirmed conclusion.
   So the correct remedy stabilizes on its own, and the conclusion is reached when that dose
   confirms the moonwort hypothesis (item 4, unchanged). Save restoration keeps its consistency check.
2. **Residue on arrival (Application).** `StreamResidueArrivalNoticing.Notice(scenario, location)`
   returns the residue evidence view when the verified location is the stream, by resolving the
   scenario's residue candidate from its initial perception; otherwise null. It is pure and stateless.
3. **Thin residue node (game).** The coordinator keeps the authored scenario references and gains
   `TryNoticeOnArrival(location)`: it calls the Application function and routes the view into the
   current route progress. It holds no session state, so a load can never leave a stale "already
   noticed" flag; route progress (saved) is the only record. The visit verifier calls it after a
   location is verified. Visits normally have route progress, because the first map open creates the
   map and the routes together (see the catch-up below for the exception).
4. **The map opens without residue.** `TryEnsureRetainedStreamResidueIsRouted` is deleted.
5. **Boot resolves nothing.** `InitializeSeekerPerception` only installs the authored perception.
   `TemporaryPassiveNoticingCandidateSource` and `TemporaryObservationReinterpretationCandidateSource`
   are deleted with their checks, scene nodes, host entries and launches.
   `TemporarySeekerPerceptionSource` names `herb-patch` as its one Unnoticed object directly. The herb
   and the stone marker become Identified only by focused search.
6. **Checks.** The perception, interaction-focus and map checks assert the unresolved launch state.
   `StreamResidueRoute` drives a stream visit: no residue before, residue recorded after, and a second
   arrival changes nothing.

Decided while implementing:

- A failed first map open can leave the map without routes, and the visit verifier never revisits a
  verified location, so its location and residue would be lost. The first open that creates the
  routes now routes every already-verified location and notices the residue once
  (`ALLRPCS_STREAM_RESIDUE_ROUTE_CHECK=late-routes`). This also fixes the pre-existing loss of the
  location itself.
- `TemporaryWorldObjectSourceSmokeCheck` also pinned the boot stone-marker label; it now expects
  both labels empty at launch.
- The unresolved herb patch is named directly (`herb-patch`) by the perception source.

No save schema change. Schema-10 saves load unchanged: their perception and recorded residue are
restored as saved, and a save from before this change simply keeps its boot-noticed objects.

Out of scope: owner playtest (item 7), renaming other `Temporary*` types (Phase 5), the unused
`InvestigationConclusionConfirmationTransition`, no new sabotage kind, no provider qualification,
no merge or release.

## Verification

`dotnet format`/`build`/the four test projects. New xUnit tests cover the arrival noticing and the
unconfirmed remedy stabilizing; the context tests drop the pre-confirmed conclusion. Native launches:
the changed checks above, `SeekerMapOpenCloseInput`, `ModalInputRecovery map-*`, `DialogueOwnership`,
and the four Main flow scenes (`SaveContinue`, `IngredientRemedy`, `HypothesisTest`, `SliceStakes`),
then the full `scripts/smoke-godot.sh`.
