# Phase 4 item 4: hypotheses

Owner authority: `owner-playable-phase4-2026-10-19` in `CONTINUATION.json`, under program Phase 4
item 4 (slice step 6) in [the program plan](2026-10-17-code-health-and-playable-slice.md).

## Measured state (at `de7dde35`)

- **Hypothesis types exist but nothing in play uses them.** Domain has `InvestigationHypothesis`,
  its snapshot, and `InvestigationHypothesisRecognitionPolicy`, which accepts a match only for an
  authored hypothesis of an open question. Application has `IInvestigationHypothesisInterpreter` and
  `InvestigationHypothesisRecognitionController`. The only interpreter is Infrastructure's
  `SerializedInvestigationHypothesisInterpreter`, which predates the Phase 3 pipeline and sits on its
  own transport seam. Only tests construct it.
- **The journal is pre-filled and frozen.** `RepresentativeJournalCompositionCoordinator` composes
  the journal once, on the first J press, and `JournalController.TryInitialize` refuses any later
  change. The hypotheses section is every plan's authored hypothesis. Open questions are the plan
  questions without a confirmed conclusion. `TemporaryFirstPatientTreatmentContextSource`
  pre-confirms the only conclusion, so that section is always empty. Experiments recorded after the
  first open never appear.
- **The pipeline.** Phase 3 decision kinds are `StructuredDecisionDescriptor`s (instructions,
  trusted context, strict schema, deserializer). `StructuredDecisionClientFactory` selects the
  OpenAI, Anthropic or Ollama client, which all feed `StructuredDecisionPipeline`.
  `TemporaryCharacterModelCompositionController` composes each kind for the selected provider.
- **Experiments.** Each dose at the patient (`OpeningRegionFirstPatientAdministrationInputCoordinator`)
  records an `IngredientAdministrationExperimentRecord`. Its dosage names the prepared recipe: the
  remedy or the station's look-alike. It also records a journal-safe `ExperimentJournalRecord`.
  Only the remedy stabilizes (`FirstPatientAdministrationOccurrenceTransition`). Both record lists
  are already in the save (`FirstPatientSaveState`). The journal's hypothesis entries are also
  saved (`JournalSaveState`, schema 10).

## Change

1. **Authored hypothesis tests (Application).** `FirstPatientHypothesisTest` pairs a hypothesis
   with the recipe whose dose tests it. `RepresentativeFirstPatientHypothesisSource` authors two
   tests for the one open question. The plan's "cooled moonwort" hypothesis is tested by the remedy.
   A new "the station's dried leaves" hypothesis is tested by the look-alike.
   `FirstPatientHypothesisTestPolicy` reads the dose records. A hypothesis is untested until its
   recipe is dosed. It is confirmed if that recipe is the remedy, and refuted otherwise.
2. **A live journal (Application).** `JournalInvestigationTransition` holds two operations.
   *Propose* appends a recognized hypothesis once. *Refresh* replaces the experiments from the
   current records and re-evaluates each hypothesis's outcome. It also reopens every plan question
   that no confirmed hypothesis answers. The journal starts with no hypotheses, so the open
   question is listed. `JournalHypothesisAuthoredCopy` carries the outcome, and the hypotheses
   section shows it. `JournalController` gains an exact-expected replacement. The journal refreshes
   each time it opens. Item 6 removes the pre-confirmed treatment context; this slice does not use
   it for the journal.
3. **Interpretation through the pipeline (Infrastructure).** A new decision kind,
   `investigation_hypothesis_recognition_decision`, reuses the existing request serializer as its
   trusted context. In that context the player's text is marked untrusted in-world data. The strict
   schema is `{hypothesisId: <offered id> | null}`. The deserializer rejects any id that was not
   offered. Only hypotheses of open questions are offered. `StructuredInvestigationHypothesisInterpreter`
   implements `IInvestigationHypothesisInterpreter` over the shared provider client. The composition
   controller composes it for the selected provider. The transport-seam interpreter is deleted along
   with its tests, because the pipeline supersedes it.
4. **The journal input (game).** The journal panel's hypotheses section shows a text field and a
   Propose button (mouse, plus Enter in the field; no new input action). It also shows a status
   line. `JournalHypothesisProposalController` runs one interpretation at a time and reports the
   result: added, already noted, no match, or interpreter unavailable/failed. It refreshes the
   panel when it is done. Loading a save cancels a pending interpretation through a session-reset
   participant, so a stale result never lands (Review Focus 1). The interpretation, its canonical
   recognition and the append are Application code (rolling rule): the interpreter call, then
   `JournalHypothesisProposalFlow`. The node only holds the task.
5. **Save.** No schema change. Proposed hypotheses are already saved as journal entries, and the
   outcome is derived from the saved dose records on every refresh, so a save cannot hold an outcome
   that contradicts its own experiments. Loading a schema-10 save that had opened the journal
   keeps its pre-filled authored hypothesis as an ordinary proposed hypothesis. This is harmless,
   and its outcome is derived the same way.
6. **Proof.** `HypothesisTestFlowCheck` is a standalone scene that instantiates Main
   (`ALLRPCS_HYPOTHESIS_TEST_CHECK`). It drives a real Ollama structured client over a scripted
   in-process HTTP stream. The scenario: the journal starts with no hypotheses and the open question
   listed. Proposing "the dried leaves" adds it as untested, and an unrelated statement is not
   matched. A look-alike dose refutes the leaves hypothesis. Proposing "moonwort" adds it. Save, a
   fresh Main and Continue keep both hypotheses with their outcomes. A moonwort remedy dose then
   confirms the moonwort hypothesis and closes the open question.

Decided while implementing:

- *Refresh* returns the same journal when no outcome, experiment or open question changed. This
  keeps the journal-modal contract that a reopen presents the same retained presentation.
  `JournalSectionPresentationController.IsCurrent` re-projects after a journal change, including
  a load, which previously could leave a stale presentation.
- A newly proposed hypothesis is refreshed at once, so one proposed after its dose shows its
  outcome immediately.
- Refresh on open is best effort, so proof scenes that install a journal without first-patient
  state still open it.
- The check's gate joins the isolated save-path list (`SaveDocumentStorageController`), so it
  never touches the developer's real save.
- Updated pins: the composition check (empty hypotheses, open question, wider surface), the
  presentation surface (`IsCurrent`), the restore-owner registry (`journal-hypothesis-proposal`),
  and the two Application text and copy-shape tests.

Out of scope: the testimony and observations sections stay composed once (pre-existing). Evidence
or conclusion state is not changed by a confirmed hypothesis; that is item 6's boot-time
resolution. No stakes or ending (item 5), no new input action, no hypothesis memory or LLM context
for the Healer, no new sabotage kind, no provider qualification, no merge or release.

## Verification

`dotnet format`/`build`/the four test projects. New xUnit tests cover the hypothesis source and
test policy, the journal transition and proposal flow, the decision kind's schema/deserializer,
and the three-provider contract case. Then the new check, the journal, save/load and
administration launches, and the full smoke suite.
