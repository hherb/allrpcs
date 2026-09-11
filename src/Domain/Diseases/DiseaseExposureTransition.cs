using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Diseases;

/// <summary>Adds one initial hidden infection after an eligible authored exposure.</summary>
public static class DiseaseExposureTransition
{
    /// <summary>Returns new disease truth with one stable-order exposed infection appended.</summary>
    public static DiseaseTruthSnapshot Apply(
        DiseaseExposureOutcome outcome,
        DiseaseTruthSnapshot truth,
        DiseaseProtectionSnapshot protections,
        WorldEventSnapshot worldEvents,
        WorldEventLogSnapshot eventLog)
    {
        if (!DiseaseExposurePolicy.CanApply(
            outcome,
            truth,
            protections,
            worldEvents,
            eventLog))
        {
            throw new InvalidOperationException("The supplied disease exposure cannot be applied.");
        }

        CharacterDiseaseState infection = new(
            outcome.CharacterId,
            outcome.Disease,
            DiseaseStage.Exposed,
            null,
            DiseaseCareState.Unassisted);

        return new DiseaseTruthSnapshot(
            truth.Definitions,
            truth.Infections.Append(infection));
    }
}
