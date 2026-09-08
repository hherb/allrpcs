namespace Allrpcs.Domain.Dialogue;

/// <summary>Produces completed immutable character-turn activity without executing proposals.</summary>
public static class CharacterTurnCompletionTransition
{
    /// <summary>Returns completed activity retaining the exact request and response.</summary>
    public static CharacterTurnActivity Complete(CharacterTurnActivity activity)
    {
        if (!CharacterTurnCompletionPolicy.CanComplete(activity))
        {
            throw new InvalidOperationException(
                "Only speaking character-turn activity can complete.");
        }

        return new CharacterTurnActivity(
            activity.Request,
            activity.Response,
            CharacterTurnActivityState.Completed);
    }
}
