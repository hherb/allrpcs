namespace Allrpcs.Domain.Dialogue;

/// <summary>Decides whether a pending character turn can resolve through a non-dialogue action.</summary>
public static class CharacterTurnActionResolutionPolicy
{
    /// <summary>Returns true only for an exact response-free awaiting activity.</summary>
    public static bool CanResolve(CharacterTurnActivity activity)
    {
        ArgumentNullException.ThrowIfNull(activity);

        return ReferenceEquals(activity.State, CharacterTurnActivityState.AwaitingResponse)
            && activity.Response is null;
    }
}
