using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Diseases;

/// <summary>Applies one exact authored comfort or stabilization outcome.</summary>
public static class DiseaseCareTransition
{
    /// <summary>Returns disease truth with only the cared-for infection replaced.</summary>
    public static DiseaseTruthSnapshot Apply(
        DiseaseCareOutcome outcome,
        DiseaseTruthSnapshot truth,
        WorldEventSnapshot worldEvents,
        WorldEventLogSnapshot eventLog)
    {
        if (!DiseaseCareEligibilityPolicy.CanApply(outcome, truth, worldEvents, eventLog))
        {
            throw new InvalidOperationException("The supplied disease-care outcome cannot apply.");
        }

        CharacterDiseaseState caredFor = new(
            outcome.Infection.CharacterId,
            outcome.Infection.Disease,
            outcome.Infection.Stage,
            outcome.Infection.LastProgressionEventId,
            outcome.ResultingCareState);
        CharacterDiseaseState[] infections = truth.Infections
            .Select(infection => ReferenceEquals(infection, outcome.Infection) ? caredFor : infection)
            .ToArray();

        return new DiseaseTruthSnapshot(truth.Definitions, infections);
    }
}
