namespace Allrpcs.Domain.Dialogue;

/// <summary>Produces a fresh awaiting activity without replacing active character-turn work.</summary>
public static class CharacterTurnStartTransition
{
    /// <summary>Starts an exact request when the absent or terminal current state permits it.</summary>
    public static CharacterTurnActivity Start(
        CharacterTurnActivity? currentActivity,
        CharacterTurnRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!CharacterTurnStartPolicy.CanStart(currentActivity))
        {
            throw new InvalidOperationException(
                "A fresh character turn cannot replace awaiting or speaking activity.");
        }

        return new CharacterTurnActivity(
            request,
            null,
            CharacterTurnActivityState.AwaitingResponse);
    }
}
