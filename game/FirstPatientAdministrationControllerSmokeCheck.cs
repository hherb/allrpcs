using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.Experiments;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Ingredients;
using Allrpcs.Domain.WorldEvents;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks the atomic ordinary first-patient administration flow when enabled.</summary>
public partial class FirstPatientAdministrationControllerSmokeCheck : Node
{
    private const string CheckEnvironmentVariable = "ALLRPCS_FIRST_PATIENT_ADMINISTRATION_CHECK";
    private const string EnabledEnvironmentValue = "1";

    /// <inheritdoc />
    public override void _Ready()
    {
        SetProcess(OS.GetEnvironment(CheckEnvironmentVariable) == EnabledEnvironmentValue);
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        SetProcess(false);

        TemporaryFirstPatientTreatmentContextSource contextSource =
            GetNode<TemporaryFirstPatientTreatmentContextSource>(
                "../TemporaryFirstPatientTreatmentContextSource");
        FirstPatientPreparationController preparationController =
            GetNode<FirstPatientPreparationController>("../FirstPatientPreparationController");
        FirstPatientStateController stateController =
            GetNode<FirstPatientStateController>("../FirstPatientStateController");
        FirstPatientAdministrationController administrationController =
            GetNode<FirstPatientAdministrationController>(
                "../FirstPatientAdministrationController");
        TemporaryWorldEventSource eventSource =
            GetNode<TemporaryWorldEventSource>("../TemporaryWorldEventSource");
        WorldEventController worldEventController =
            GetNode<WorldEventController>("../WorldEventController");
        CharacterId healerId = GetNode<TemporaryHealerCharacterSource>(
            "../TemporaryHealerCharacterSource").Character.Identity.Id;
        WorldObjectSnapshot? unchangedWorldObjects = GetNode<WorldObjectController>(
            "../WorldObjectController").WorldObjects;

        FirstPatientTreatmentContext context = contextSource.Context;
        IngredientPreparationRecipe recipe = context.Scenario.Remedy.Recipe;
        IngredientStateSnapshot initialIngredients = preparationController.Ingredients!;
        WorldEventSnapshot worldEvents = eventSource.WorldEvents;
        WorldEvent administrationEvent = eventSource.FirstPatientRemedyAdministeredWorldEvent;
        if (!PrepareExactRemedy(preparationController, recipe, context.Scenario.GatheredIngredients.States[0])
            || preparationController.Outcome is not { } preparation
            || ReferenceEquals(preparationController.Ingredients, initialIngredients)
            || !stateController.TryAddPreparedBatch(context, preparation))
        {
            Fail("The ordinary scene could not prepare and install the exact authored remedy.");
            return;
        }

        PreparedIngredientBatchSnapshot installedBatches = stateController.PreparedBatches!;
        IngredientStateSnapshot remainingIngredients = preparationController.Ingredients!;
        PreparedIngredientBatch batch = AssertSingle(installedBatches.Batches);
        DiseaseTruthSnapshot initialTruth = stateController.DiseaseTruth!;
        IngredientExperimentJournalSnapshot initialAdministrationExperiments =
            stateController.AdministrationExperiments!;
        ExperimentJournalSnapshot initialExperiments = stateController.Experiments!;
        WorldEventLogSnapshot initialEventLog = worldEventController.EventLog!;
        WorldTimeSnapshot initialWorldTime = worldEventController.WorldTime!;
        IngredientQuantity dose = new(
            batch.RemainingQuantity.Amount,
            batch.RemainingQuantity.Unit);
        CharacterConditionSet observedConditions = new(
            [new CharacterCondition("breathing", "Breathing becomes quieter and steadier.")]);

        if (!RejectsReplacementInputsWithoutStateChanges(
                administrationController,
                context,
                healerId,
                administrationEvent,
                batch,
                dose,
                observedConditions,
                worldEventController,
                stateController))
        {
            return;
        }

        ExperimentId experimentId = new("first-patient-remedy-administration");
        if (!administrationController.TryAdminister(
                experimentId,
                context,
                healerId,
                administrationEvent,
                batch,
                dose,
                observedConditions,
                "Administer the complete cooled remedy batch.",
                "The patient's breathing becomes quieter and steadier."))
        {
            Fail("The exact authored first-patient administration was rejected.");
            return;
        }

        WorldEventLogSnapshot resultingEventLog = worldEventController.EventLog!;
        WorldTimeSnapshot resultingWorldTime = worldEventController.WorldTime!;
        DiseaseTruthSnapshot resultingTruth = stateController.DiseaseTruth!;
        PreparedIngredientBatchSnapshot resultingBatches = stateController.PreparedBatches!;
        IngredientExperimentJournalSnapshot resultingAdministrationExperiments =
            stateController.AdministrationExperiments!;
        ExperimentJournalSnapshot resultingExperiments = stateController.Experiments!;
        IngredientAdministrationExperimentRecord administrationRecord =
            AssertSingle(resultingAdministrationExperiments.Records);
        ExperimentJournalRecord universalRecord = AssertSingle(resultingExperiments.Records);

        if (ReferenceEquals(resultingEventLog, initialEventLog)
            || resultingEventLog.Entries.Count != 2
            || !ReferenceEquals(resultingEventLog.Entries[^1], administrationEvent.EventId)
            || ReferenceEquals(resultingWorldTime, initialWorldTime)
            || !ReferenceEquals(resultingWorldTime.CurrentEventId, administrationEvent.EventId)
            || ReferenceEquals(resultingTruth, initialTruth)
            || !ReferenceEquals(
                AssertSingle(resultingTruth.Infections).CareState,
                DiseaseCareState.Stabilized)
            || resultingBatches.Batches.Count != 0
            || ReferenceEquals(
                resultingAdministrationExperiments,
                initialAdministrationExperiments)
            || ReferenceEquals(resultingExperiments, initialExperiments)
            || !ReferenceEquals(administrationRecord.ExperimentId, experimentId)
            || !ReferenceEquals(administrationRecord.Administration.AdministratorId, healerId)
            || !ReferenceEquals(
                administrationRecord.Administration.PatientId,
                context.Scenario.PatientId)
            || !ReferenceEquals(administrationRecord.Administration.Dosage.Preparation, preparation)
            || !ReferenceEquals(administrationRecord.Administration.Dosage.Quantity, dose)
            || !ReferenceEquals(administrationRecord.Administration.WorldEvent, administrationEvent)
            || !ReferenceEquals(administrationRecord.ObservedConditions, observedConditions)
            || !ReferenceEquals(universalRecord.ExperimentId, experimentId)
            || !ReferenceEquals(universalRecord.WorldEvent, administrationEvent))
        {
            Fail("The first-patient administration did not install one exact atomic result.");
            return;
        }

        if (administrationController.TryAdminister(
                new ExperimentId("repeated-first-patient-dose"),
                context,
                healerId,
                administrationEvent,
                batch,
                dose,
                new CharacterConditionSet([]),
                "Repeat the consumed dose.",
                "No repeated outcome should be installed.")
            || !ReferenceEquals(worldEventController.EventLog, resultingEventLog)
            || !ReferenceEquals(worldEventController.WorldTime, resultingWorldTime)
            || !ReferenceEquals(stateController.DiseaseTruth, resultingTruth)
            || !ReferenceEquals(stateController.PreparedBatches, resultingBatches)
            || !ReferenceEquals(
                stateController.AdministrationExperiments,
                resultingAdministrationExperiments)
            || !ReferenceEquals(stateController.Experiments, resultingExperiments)
            || !ReferenceEquals(contextSource.Context, context)
            || !ReferenceEquals(eventSource.WorldEvents, worldEvents)
            || !ReferenceEquals(preparationController.Ingredients, remainingIngredients)
            || !ReferenceEquals(preparationController.Outcome, preparation)
            || !ReferenceEquals(
                GetNode<WorldObjectController>("../WorldObjectController").WorldObjects,
                unchangedWorldObjects))
        {
            Fail("A repeated administration changed authoritative or unrelated scene state.");
            return;
        }

        GD.Print(
            "First-patient administration smoke passed: exact authored preparation, batch, event, "
            + "dose, stabilized infection, and both experiment records installed atomically; "
            + "rejected replacement and repeated calls preserved both controllers and unrelated state.");
        GetTree().Quit();
    }

    private static bool PrepareExactRemedy(
        FirstPatientPreparationController controller,
        IngredientPreparationRecipe recipe,
        IngredientState gatheredHerb)
    {
        if (!controller.TryStart(recipe, gatheredHerb))
        {
            return false;
        }

        foreach (IngredientPreparationStep step in recipe.Plan.Steps)
        {
            if (!controller.TryAdvance(step))
            {
                return false;
            }
        }

        return controller.TryComplete();
    }

    private bool RejectsReplacementInputsWithoutStateChanges(
        FirstPatientAdministrationController controller,
        FirstPatientTreatmentContext context,
        CharacterId healerId,
        WorldEvent worldEvent,
        PreparedIngredientBatch batch,
        IngredientQuantity dose,
        CharacterConditionSet observedConditions,
        WorldEventController worldEventController,
        FirstPatientStateController stateController)
    {
        WorldEvent replacementEvent = new(
            new WorldEventId(worldEvent.EventId.Value),
            worldEvent.TimeEffect);
        PreparedIngredientBatch replacementBatch = new(batch.Preparation);
        FirstPatientTreatmentContext replacementContext =
            RepresentativeFirstPatientTreatmentContextSource.Create();
        WorldEventLogSnapshot eventLog = worldEventController.EventLog!;
        WorldTimeSnapshot worldTime = worldEventController.WorldTime!;
        DiseaseTruthSnapshot diseaseTruth = stateController.DiseaseTruth!;
        PreparedIngredientBatchSnapshot preparedBatches = stateController.PreparedBatches!;
        IngredientExperimentJournalSnapshot administrationExperiments =
            stateController.AdministrationExperiments!;
        ExperimentJournalSnapshot experiments = stateController.Experiments!;

        bool rejected = !Try(controller, context, healerId, replacementEvent, batch, dose, observedConditions)
            && !Try(controller, context, healerId, worldEvent, replacementBatch, dose, observedConditions)
            && !Try(controller, replacementContext, healerId, worldEvent, batch, dose, observedConditions);
        if (!rejected
            || !ReferenceEquals(worldEventController.EventLog, eventLog)
            || !ReferenceEquals(worldEventController.WorldTime, worldTime)
            || !ReferenceEquals(stateController.DiseaseTruth, diseaseTruth)
            || !ReferenceEquals(stateController.PreparedBatches, preparedBatches)
            || !ReferenceEquals(stateController.AdministrationExperiments, administrationExperiments)
            || !ReferenceEquals(stateController.Experiments, experiments))
        {
            Fail("Rejected replacement administration inputs changed controller state.");
            return false;
        }

        return true;
    }

    private static bool Try(
        FirstPatientAdministrationController controller,
        FirstPatientTreatmentContext context,
        CharacterId healerId,
        WorldEvent worldEvent,
        PreparedIngredientBatch batch,
        IngredientQuantity dose,
        CharacterConditionSet observedConditions) => controller.TryAdminister(
            new ExperimentId("rejected-first-patient-dose"),
            context,
            healerId,
            worldEvent,
            batch,
            dose,
            observedConditions,
            "Rejected procedure.",
            "Rejected outcome.");

    private static T AssertSingle<T>(IReadOnlyList<T> values) => values.Count == 1
        ? values[0]
        : throw new InvalidOperationException("Expected one retained value.");

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }
}
