namespace Allrpcs.Domain.Dialogue;

/// <summary>
/// Retains one immutable provider-neutral character-turn activity state.
/// </summary>
public sealed record CharacterTurnActivity
{
    /// <summary>Creates an activity whose response shape agrees with its lifecycle state.</summary>
    public CharacterTurnActivity(
        CharacterTurnRequest request,
        CharacterTurnResponse? response,
        CharacterTurnActivityState state)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(state);

        if (response is not null && !ReferenceEquals(response.Request, request))
        {
            throw new ArgumentException(
                "A character-turn response must answer the activity request.",
                nameof(response));
        }

        if (state == CharacterTurnActivityState.AwaitingResponse && response is not null)
        {
            throw new ArgumentException(
                "An awaiting-response activity cannot already retain a response.",
                nameof(response));
        }

        if ((state == CharacterTurnActivityState.Speaking
                || state == CharacterTurnActivityState.Completed)
            && response is null)
        {
            throw new ArgumentException(
                "Speaking and completed activities require a response.",
                nameof(response));
        }

        Request = request;
        Response = response;
        State = state;
    }

    /// <summary>Gets the exact request whose activity is represented.</summary>
    public CharacterTurnRequest Request { get; }

    /// <summary>Gets the exact complete response when one is available.</summary>
    public CharacterTurnResponse? Response { get; }

    /// <summary>Gets the exact defined lifecycle state.</summary>
    public CharacterTurnActivityState State { get; }
}
