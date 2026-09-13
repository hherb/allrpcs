using System.Numerics;
using Allrpcs.Domain.Characters;

namespace Allrpcs.Application.Navigation;

/// <summary>Combines an exact final return advance with one bounded party rejoin.</summary>
public static class CharacterIndependentTaskReturnRejoinTransition
{
    /// <summary>Returns fresh completed return progress and party state atomically.</summary>
    public static CharacterIndependentTaskReturnRejoinResult ArriveAndRejoin(
        CharacterIndependentTaskReturnTravelProgress progress,
        CharacterIndependentTaskReturnTravelPlan plan,
        Vector2 reachedWaypoint,
        CharacterPartySnapshot party)
    {
        ArgumentNullException.ThrowIfNull(progress);
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(party);

        CharacterIndependentTaskReturnTravelProgress completedProgress =
            CharacterIndependentTaskReturnTravelProgressTransition.Advance(
                progress,
                plan,
                reachedWaypoint);
        if (!CharacterIndependentTaskReturnRejoinEligibilityPolicy.IsEligible(
                plan,
                completedProgress,
                party))
        {
            throw new InvalidOperationException(
                "A party rejoin requires exact completed return travel and available membership.");
        }

        CharacterPartySnapshot resultingParty = CharacterPartyRejoinTransition.Rejoin(
            plan.CompletedActivity.Assignment.CharacterId,
            party);
        return new CharacterIndependentTaskReturnRejoinResult(
            completedProgress,
            resultingParty);
    }
}
