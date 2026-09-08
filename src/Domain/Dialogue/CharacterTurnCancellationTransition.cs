namespace Allrpcs.Domain.Dialogue;

/// <summary>
/// Produces cancelled immutable character-turn activity without controlling runtime work.
/// </summary>
public static class CharacterTurnCancellationTransition
{
    /// <summary>Returns a cancelled activity retaining the exact request and optional response.</summary>
    public static CharacterTurnActivity Cancel(CharacterTurnActivity activity)
    {
        if (!CharacterTurnCancellationPolicy.CanCancel(activity))
        {
            throw new InvalidOperationException(
                "Only awaiting-response or speaking character-turn activity can be cancelled.");
        }

        return new CharacterTurnActivity(
            activity.Request,
            activity.Response,
            CharacterTurnActivityState.Cancelled);
    }
}
