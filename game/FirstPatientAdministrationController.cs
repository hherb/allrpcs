using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.Experiments;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Ingredients;
using Allrpcs.Domain.WorldEvents;
using Godot;

namespace Allrpcs.Game;

/// <summary>Coordinates one atomic authored first-patient administration occurrence.</summary>
public partial class FirstPatientAdministrationController : Node
{
    private FirstPatientStateController firstPatientStateController = null!;
    private WorldEventController worldEventController = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        firstPatientStateController = GetNode<FirstPatientStateController>(
            "../FirstPatientStateController");
        worldEventController = GetNode<WorldEventController>("../WorldEventController");
    }

    /// <summary>Occurs and installs one exact eligible authored first-patient dose.</summary>
    public bool TryAdminister(
        ExperimentId experimentId,
        FirstPatientTreatmentContext context,
        CharacterId administratorId,
        WorldEvent worldEvent,
        PreparedIngredientBatch batch,
        IngredientQuantity dosageQuantity,
        CharacterConditionSet observedConditions,
        string procedure,
        string observedOutcome)
    {
        ArgumentNullException.ThrowIfNull(experimentId);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(administratorId);
        ArgumentNullException.ThrowIfNull(worldEvent);
        ArgumentNullException.ThrowIfNull(batch);
        ArgumentNullException.ThrowIfNull(dosageQuantity);
        ArgumentNullException.ThrowIfNull(observedConditions);
        ArgumentNullException.ThrowIfNull(procedure);
        ArgumentNullException.ThrowIfNull(observedOutcome);

        WorldEventSnapshot? worldEvents = worldEventController.WorldEvents;
        WorldEventLogSnapshot? eventLog = worldEventController.EventLog;
        WorldTimeSnapshot? worldTime = worldEventController.WorldTime;
        DiseaseTruthSnapshot? diseaseTruth = firstPatientStateController.DiseaseTruth;
        PreparedIngredientBatchSnapshot? preparedBatches =
            firstPatientStateController.PreparedBatches;
        IngredientExperimentJournalSnapshot? administrationExperiments =
            firstPatientStateController.AdministrationExperiments;
        ExperimentJournalSnapshot? experiments = firstPatientStateController.Experiments;
        if (worldEvents is null
            || eventLog is null
            || worldTime is null
            || diseaseTruth is null
            || preparedBatches is null
            || administrationExperiments is null
            || experiments is null)
        {
            return false;
        }

        // The patient may have worsened since the authored scenario; a wrong dose must keep that.
        context = context.WithDiseaseTruth(diseaseTruth);

        FirstPatientAdministrationOccurrenceResult result;
        try
        {
            result = FirstPatientAdministrationOccurrenceTransition.Occur(
                experimentId,
                context,
                administratorId,
                worldEvent,
                batch,
                dosageQuantity,
                observedConditions,
                procedure,
                observedOutcome,
                worldEvents,
                eventLog,
                worldTime,
                preparedBatches,
                administrationExperiments,
                experiments);
        }
        catch (ArgumentException)
        {
            return false;
        }
        catch (InvalidOperationException)
        {
            return false;
        }

        worldEventController.InstallFirstPatientAdministration(result);
        firstPatientStateController.InstallAdministrationOccurrence(result);
        return true;
    }
}
