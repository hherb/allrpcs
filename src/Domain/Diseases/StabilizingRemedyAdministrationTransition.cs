using Allrpcs.Domain.Ingredients;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Diseases;

/// <summary>Applies one exact eligible stabilizing-remedy administration to disease truth.</summary>
public static class StabilizingRemedyAdministrationTransition
{
    /// <summary>Returns new disease truth with only the exact first-patient infection stabilized.</summary>
    public static DiseaseTruthSnapshot Apply(
        FirstPatientTreatmentContext context,
        IngredientAdministrationOutcome administration,
        WorldEventSnapshot worldEvents,
        WorldEventLogSnapshot eventLog)
    {
        if (!StabilizingRemedyAdministrationPolicy.CanStabilize(
            context,
            administration,
            worldEvents,
            eventLog))
        {
            throw new InvalidOperationException(
                "The supplied administration cannot stabilize the first patient.");
        }

        FirstPatientScenario scenario = context.Scenario;
        CharacterDiseaseState infection = scenario.DiseaseTruth.Infections.Single(candidate =>
            ReferenceEquals(candidate.CharacterId, scenario.PatientId)
            && ReferenceEquals(candidate.Disease, scenario.Remedy.Disease));
        DiseaseCareOutcome careOutcome = new(
            infection,
            DiseaseCareState.Stabilized,
            administration.WorldEvent);

        return DiseaseCareTransition.Apply(
            careOutcome,
            scenario.DiseaseTruth,
            worldEvents,
            eventLog);
    }
}
