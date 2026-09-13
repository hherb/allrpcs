using Allrpcs.Domain.Characters;

namespace Allrpcs.Application.Navigation;

/// <summary>Gates an independent-task occurrence on exact completed travel state.</summary>
public static class CharacterIndependentTaskTravelOccurrenceEligibilityPolicy
{
    /// <summary>Returns whether the exact active task has completed its exact travel plan.</summary>
    public static bool IsEligible(
        CharacterIndependentTaskTravelPlan plan,
        CharacterIndependentTaskTravelProgress progress,
        CharacterIndependentTaskActivity activeActivity)
    {
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(progress);
        ArgumentNullException.ThrowIfNull(activeActivity);

        return ReferenceEquals(progress.Plan, plan)
            && ReferenceEquals(plan.Activity, activeActivity)
            && activeActivity.State == CharacterIndependentTaskActivityState.Active
            && progress.IsComplete;
    }
}
