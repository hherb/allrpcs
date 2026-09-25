namespace Allrpcs.Domain.Dialogue;

/// <summary>Decides whether a fresh immutable character-turn activity can start.</summary>
public static class CharacterTurnStartPolicy
{
    /// <summary>Returns true only when no activity exists or the current activity is terminal.</summary>
    public static bool CanStart(CharacterTurnActivity? currentActivity)
    {
        return currentActivity is null
            || currentActivity.State == CharacterTurnActivityState.Completed
            || currentActivity.State == CharacterTurnActivityState.Resolved
            || currentActivity.State == CharacterTurnActivityState.Cancelled;
    }
}
