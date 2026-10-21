# Phase 4 item 3: ingredients

Owner authority: `owner-playable-phase4-2026-10-19` in `CONTINUATION.json`, under program Phase 4
item 3 (slice steps 5 and 7) in [the program plan](2026-10-17-code-health-and-playable-slice.md).

## Measured state (at `7365ea98`)

- **Three inventories, none used in play.** `Character.Inventory` (`CharacterInventory`) is
  authored empty for the Seeker and Healer. Focused search passes its own fixed
  `TemporaryFocusedSearchContextSource.Inventory` (the field lens) to `DiscoveryToolPolicy`.
  `InventoryOwnershipSnapshot` with its Capacity/Take/Give/Drop/Use policies exists only in Domain
  and its tests: nothing in Application or `game/` owns, changes or saves an inventory.
- **Collection is scaffolding.** The normal path, `HerbExaminationActionController`, executes only
  `Accept` + `ExamineObject` and always reports "herbs left in place". Collection exists only in
  `TemporaryCharacterExaminationFlowController`. In Main it runs only while
  `Main.ConditionalCollectionScaffolding` is set (two checks set it); otherwise
  `ConversationTurnNodePorts.OfferExamination` switches it off at the first offer. Checks that
  install turns directly (turn completion, turn repeat, herb-collection audio) still drive it.
  A collection only appends a world event and a transcript entry; nobody receives the herbs.
- **Nothing to gather, no wrong remedy.** `Main.Initialization` starts preparation with the
  scenario stock (moonwort 500 mg, clean water 250 ml), so four presses of P prepare the one
  authored recipe at the station. `FirstPatientStateController.TryAddPreparedBatch`,
  `FirstPatientSaveStateRestorationTransition` and the stabilization policy accept only
  `Scenario.Remedy.Recipe`, and `FirstPatientAdministrationOccurrenceTransition` throws unless the
  dose stabilizes. Preparation is one-shot.
- **Save** is schema 9 (`Continuation`) and holds no inventory.

## Change

1. **One inventory owner.** `InventoryOwnershipSnapshot.InventoryOf(character)` returns that
   character's `CharacterInventory` view, so a per-character inventory is a projection of the one
   snapshot. `InventoryExchangeTransition` (Application) applies Take, Give and Use (Use consumes)
   through the Domain policies and returns `null` when a policy refuses. `InventoryController`
   (game) holds the snapshot and capacities (Seeker and Healer: 4 each). It is seeded from the
   authored character inventories plus the field lens for the Seeker. Focused search reads the
   Seeker's view from it.
2. **Conditional collection is the normal path.** `ConditionalCollectionPolicy` (Application)
   collects after a safe examination when the turn conditionally accepted, or committed to
   collecting that object ("examine the herbs and gather them if they are safe").
   `HerbExaminationActionController` now executes `Accept` and `ConditionallyAccept`
   examinations. When the policy agrees, it records the existing post-examination collection
   event in the same frame, and the Healer takes `moonwort-leaves`. The examination is the
   evidence that identifies the herb, and its status names moonwort. The gate is removed (the
   Main property and the port parameter). The temporary flow stays only as a check fixture: it is
   off unless a check turns on its processing. Phase 5 resolves `Temporary*`.
3. **Ingredients and a look-alike (Domain).** `FirstPatientScenario` gains `GatheredIngredients`
   and `AlternativeRecipes`. Moonwort must be gathered: the station stock is the scenario
   ingredients minus the gathered ones. The alternative recipe is a look-alike preparation from
   unlabelled dried leaves at the station (`false-moonwort-leaf`). It uses the same steps and
   never stabilizes. The structural policy also requires each alternative to be preparable from
   the station stock.
4. **The station (P): Give, Use and a wrong remedy.** `RemedyStationPolicy` (Application)
   chooses the next idle action. If the Seeker carries moonwort, P prepares the remedy from it
   and the station stock. Otherwise, if the Healer is within reach and carries moonwort, the
   Healer gives it to the Seeker. Otherwise P prepares the look-alike, and the status says the
   leaves are unidentified. The moonwort is used up when the batch is made. Batches, saves and
   administration accept the remedy or an alternative recipe. A look-alike dose records its
   experiment (`IngredientAdministrationExperimentExecutionTransition`) with the observed outcome
   "no change", and the patient is not stabilized. After that, P starts a new preparation. The
   preparation lifecycle (recipe choice, reset after a failed dose, restoration) moves into
   Application as `RemedyPreparationLifecycle`, under the rolling rule.
5. **Save schema 10 (`Inventory`).** The root adds the ownership snapshot as strict JSON
   (object/owner pairs). Schema 9 migrates to 10 with no inventory, which restores the authored
   starting inventory. Unknown versions stay rejected. Restoration preflight rejects an unknown
   object, an unknown owner, or an inventory over capacity. The whole load fails closed.
6. **Proof.** `IngredientRemedyFlowCheck` (standalone scene that instantiates Main,
   `ALLRPCS_INGREDIENT_REMEDY_CHECK`) runs a controlled Ollama fixture. A look-alike dose leaves
   the patient unstabilized. A conditional examine-and-gather request makes the Healer collect
   moonwort through the normal path. Save, a fresh Main and Continue keep the Healer's moonwort.
   At the station the Healer gives, the Seeker uses, and the remedy stabilizes. Existing checks
   that prepare and expect stabilization first give the Seeker moonwort.

Decided while implementing:

- `AcceptedCharacterExamination` also admits `ConditionallyAccept`, so a conditional request reaches
  the walk. The Healer gathers only into a free hand (`InventoryController.CanTake`), and the
  collection event occurs once, so the event log never records an empty take.
- Retrying a wrong dose needs a second authored dose event (`healer-administered-first-patient-second-dose`)
  and experiment id. A second wrong dose ends the doses: the station then starts nothing, so no
  moonwort is lost. Item 5 decides what follows.
- Saved-inventory checks are `InventoryRestorationPolicy` (Application). Authored starting objects,
  such as the field lens, must survive a load.
- A save without a Seeker profile still reopens customization on Continue (item 2), so the proof
  customizes first.

Out of scope: per-frame testimony probing before stabilization (pre-existing); hypotheses, stakes/ending and boot-time auto-resolution (items 4–6); an LLM give
intention or inventory in model context; a new input action (the controls catalog is accepted
and closed); herb-collection audio on the normal path (it still follows the legacy fixture); no
new sabotage kind; no provider qualification; no merge or release.

## Verification

`dotnet format`/`build`/the four test projects (new xUnit tests: ownership view, exchange
transition, conditional collection policy, scenario/structural policy, station policy,
preparation lifecycle, schema 10 and its JSON and migration), the new check, the examination,
preparation, administration, recovery-restoration and save/load launches, then the full smoke
suite.
