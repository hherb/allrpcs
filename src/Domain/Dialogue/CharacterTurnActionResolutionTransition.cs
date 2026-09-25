namespace Allrpcs.Domain.Dialogue;

/// <summary>Produces a terminal character turn after its exact non-dialogue action resolves.</summary>
public static class CharacterTurnActionResolutionTransition
{
    /// <summary>Returns a response-free terminal activity retaining the exact request.</summary>
    public static CharacterTurnActivity Resolve(CharacterTurnActivity activity)
    {
        if (!CharacterTurnActionResolutionPolicy.CanResolve(activity))
        {
            throw new InvalidOperationException(
                "Only a response-free awaiting character turn can resolve through an action.");
        }

        return new CharacterTurnActivity(
            activity.Request,
            null,
            CharacterTurnActivityState.Resolved);
    }
}
