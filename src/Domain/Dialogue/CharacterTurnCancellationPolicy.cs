namespace Allrpcs.Domain.Dialogue;

/// <summary>
/// Decides whether an immutable character-turn activity can be cancelled.
/// </summary>
public static class CharacterTurnCancellationPolicy
{
    /// <summary>Returns true only while a response is awaited or its speech is being presented.</summary>
    public static bool CanCancel(CharacterTurnActivity activity)
    {
        ArgumentNullException.ThrowIfNull(activity);

        return activity.State == CharacterTurnActivityState.AwaitingResponse
            || activity.State == CharacterTurnActivityState.Speaking;
    }
}
