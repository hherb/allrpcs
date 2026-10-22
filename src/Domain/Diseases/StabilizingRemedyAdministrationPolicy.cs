using Allrpcs.Domain.Ingredients;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Diseases;

/// <summary>Checks whether an exact remedy administration can stabilize the first patient.</summary>
public static class StabilizingRemedyAdministrationPolicy
{
    /// <summary>Reports whether exact authored scenario, dosage, patient, and event state align.</summary>
    public static bool CanStabilize(
        FirstPatientTreatmentContext context,
        IngredientAdministrationOutcome administration,
        WorldEventSnapshot worldEvents,
        WorldEventLogSnapshot eventLog)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(administration);
        ArgumentNullException.ThrowIfNull(worldEvents);
        ArgumentNullException.ThrowIfNull(eventLog);

        FirstPatientScenario scenario = context.Scenario;
        CharacterDiseaseState? infection = scenario.DiseaseTruth.Infections.FirstOrDefault(candidate =>
            ReferenceEquals(candidate.CharacterId, scenario.PatientId)
            && ReferenceEquals(candidate.Disease, scenario.Remedy.Disease));
        if (infection is null)
        {
            return false;
        }

        DiseaseCareOutcome careOutcome = new(
            infection,
            DiseaseCareState.Stabilized,
            administration.WorldEvent);
        return FirstPatientTreatmentContextPolicy.IsConsistent(context)
            && ReferenceEquals(administration.PatientId, scenario.PatientId)
            && ReferenceEquals(
                administration.Dosage.Preparation.Activity.Recipe,
                scenario.Remedy.Recipe)
            && DiseaseCareEligibilityPolicy.CanApply(
                careOutcome,
                scenario.DiseaseTruth,
                worldEvents,
                eventLog);
    }
}
