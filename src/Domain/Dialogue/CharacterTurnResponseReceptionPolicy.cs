namespace Allrpcs.Domain.Dialogue;

/// <summary>Decides whether an immutable response can enter an awaiting character turn.</summary>
public static class CharacterTurnResponseReceptionPolicy
{
    /// <summary>Returns true only for the exact request awaited by the activity.</summary>
    public static bool CanReceive(
        CharacterTurnActivity activity,
        CharacterTurnResponse response)
    {
        ArgumentNullException.ThrowIfNull(activity);
        ArgumentNullException.ThrowIfNull(response);

        return activity.State == CharacterTurnActivityState.AwaitingResponse
            && ReferenceEquals(response.Request, activity.Request);
    }
}
