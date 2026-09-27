using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.Ingredients;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Application.Dialogue;

/// <summary>Projects one fixed provider-neutral testimony after exact first-patient stabilization.</summary>
public static class FirstPatientStabilizationTestimonyProjection
{
    /// <summary>Gets the fixed authored testimony text.</summary>
    public const string TestimonyText =
        "My breathing is steadier. Before I collapsed, I drank from the village well.";

    /// <summary>Returns one ordinary delivery from exact stabilization provenance.</summary>
    public static DialogueDelivery Project(
        FirstPatientTreatmentContext context,
        DiseaseTruthSnapshot diseaseTruth,
        IngredientExperimentJournalSnapshot administrationExperiments,
        WorldEvent administrationWorldEvent)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(diseaseTruth);
        ArgumentNullException.ThrowIfNull(administrationExperiments);
        ArgumentNullException.ThrowIfNull(administrationWorldEvent);

        FirstPatientScenario scenario = context.Scenario;
        CharacterDiseaseState? infection = diseaseTruth.Infections.SingleOrDefault(candidate =>
            ReferenceEquals(candidate.CharacterId, scenario.PatientId)
            && ReferenceEquals(candidate.Disease, scenario.Remedy.Disease));
        IngredientAdministrationExperimentRecord? record =
            administrationExperiments.Records.Count == 0
                ? null
                : administrationExperiments.Records[^1];
        if (infection is null
            || !ReferenceEquals(infection.CareState, DiseaseCareState.Stabilized)
            || record is null
            || !ReferenceEquals(record.Administration.PatientId, scenario.PatientId)
            || !ReferenceEquals(
                record.Administration.Dosage.Preparation.Activity.Recipe,
                scenario.Remedy.Recipe)
            || !ReferenceEquals(record.Administration.WorldEvent, administrationWorldEvent))
        {
            throw new InvalidOperationException(
                "First-patient testimony requires exact stabilized administration provenance.");
        }

        return new DialogueDelivery(
            new DialogueUtterance(scenario.PatientId, TestimonyText),
            DialogueDeliveryMode.OrdinarySpeech);
    }
}
