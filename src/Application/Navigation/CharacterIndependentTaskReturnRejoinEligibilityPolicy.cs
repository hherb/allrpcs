using Allrpcs.Domain.Characters;

namespace Allrpcs.Application.Navigation;

/// <summary>Gates party rejoin on exact completed independent-task return travel.</summary>
public static class CharacterIndependentTaskReturnRejoinEligibilityPolicy
{
    /// <summary>Returns whether the completed task character can rejoin after its exact return.</summary>
    public static bool IsEligible(
        CharacterIndependentTaskReturnTravelPlan plan,
        CharacterIndependentTaskReturnTravelProgress progress,
        CharacterPartySnapshot party)
    {
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(progress);
        ArgumentNullException.ThrowIfNull(party);

        return ReferenceEquals(progress.Plan, plan)
            && progress.IsComplete
            && plan.CompletedActivity.State == CharacterIndependentTaskActivityState.Completed
            && CharacterPartyRejoinPolicy.CanRejoin(
                plan.CompletedActivity.Assignment.CharacterId,
                party);
    }
}
