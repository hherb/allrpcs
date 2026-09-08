namespace Allrpcs.Domain.Dialogue;

/// <summary>Produces speaking activity from an eligible already-complete response.</summary>
public static class CharacterTurnResponseReceptionTransition
{
    /// <summary>Returns speaking activity retaining the exact request and response.</summary>
    public static CharacterTurnActivity Receive(
        CharacterTurnActivity activity,
        CharacterTurnResponse response)
    {
        if (!CharacterTurnResponseReceptionPolicy.CanReceive(activity, response))
        {
            throw new InvalidOperationException(
                "Only a response for the exact awaited character-turn request can be received.");
        }

        return new CharacterTurnActivity(
            activity.Request,
            response,
            CharacterTurnActivityState.Speaking);
    }
}
