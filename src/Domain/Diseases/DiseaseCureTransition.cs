using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Diseases;

/// <summary>Removes one exact infection after an authoritative successful cure.</summary>
public static class DiseaseCureTransition
{
    /// <summary>Returns new disease truth without the exact successfully cured infection.</summary>
    public static DiseaseTruthSnapshot Apply(
        SuccessfulDiseaseCureOutcome outcome,
        DiseaseTruthSnapshot truth,
        WorldEventSnapshot worldEvents,
        WorldEventLogSnapshot eventLog)
    {
        if (!DiseaseCureEligibilityPolicy.CanApply(outcome, truth, worldEvents, eventLog))
        {
            throw new InvalidOperationException("The supplied successful cure cannot be applied.");
        }

        CharacterDiseaseState[] infections = truth.Infections
            .Where(infection => !ReferenceEquals(infection, outcome.Infection))
            .ToArray();

        return new DiseaseTruthSnapshot(truth.Definitions, infections);
    }
}
