namespace Allrpcs.Domain.Dialogue;

/// <summary>Decides whether an immutable character-turn activity can complete.</summary>
public static class CharacterTurnCompletionPolicy
{
    /// <summary>Returns true only while the complete response is being spoken.</summary>
    public static bool CanComplete(CharacterTurnActivity activity)
    {
        ArgumentNullException.ThrowIfNull(activity);

        return activity.State == CharacterTurnActivityState.Speaking;
    }
}
