namespace Allrpcs.Domain.Characters;

/// <summary>Decides whether one independent-task lifecycle transition is legal.</summary>
public static class CharacterIndependentTaskLifecyclePolicy
{
    /// <summary>Returns true only for a forward transition from assigned or active state.</summary>
    public static bool CanTransition(
        CharacterIndependentTaskActivity activity,
        CharacterIndependentTaskActivityState nextState)
    {
        ArgumentNullException.ThrowIfNull(activity);
        ArgumentNullException.ThrowIfNull(nextState);

        return (activity.State == CharacterIndependentTaskActivityState.Assigned
                && (nextState == CharacterIndependentTaskActivityState.Active
                    || nextState == CharacterIndependentTaskActivityState.Cancelled))
            || (activity.State == CharacterIndependentTaskActivityState.Active
                && (nextState == CharacterIndependentTaskActivityState.Completed
                    || nextState == CharacterIndependentTaskActivityState.Cancelled));
    }
}
