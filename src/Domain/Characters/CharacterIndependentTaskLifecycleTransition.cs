namespace Allrpcs.Domain.Characters;

/// <summary>Produces one legal immutable independent-task lifecycle transition.</summary>
public static class CharacterIndependentTaskLifecycleTransition
{
    /// <summary>Returns fresh activity in the requested legal state.</summary>
    public static CharacterIndependentTaskActivity Transition(
        CharacterIndependentTaskActivity activity,
        CharacterIndependentTaskActivityState nextState)
    {
        if (!CharacterIndependentTaskLifecyclePolicy.CanTransition(activity, nextState))
        {
            throw new InvalidOperationException(
                "The independent task cannot enter the requested lifecycle state.");
        }

        return new CharacterIndependentTaskActivity(activity.Assignment, nextState);
    }
}
