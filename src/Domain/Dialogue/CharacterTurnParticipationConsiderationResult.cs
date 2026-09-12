namespace Allrpcs.Domain.Dialogue;

/// <summary>
/// Binds one accepted provider-neutral participation decision to its exact consideration request.
/// </summary>
public sealed record CharacterTurnParticipationConsiderationResult
{
    /// <summary>
    /// Creates an immutable result only when the decision is valid for the exact request.
    /// </summary>
    public CharacterTurnParticipationConsiderationResult(
        CharacterTurnParticipationConsiderationRequest request,
        CharacterTurnParticipationDecision decision)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(decision);

        if (!CharacterTurnParticipationConsiderationPolicy.CanAccept(request, decision))
        {
            throw new ArgumentException(
                "The participation decision must be valid for the exact consideration request.",
                nameof(decision));
        }

        Request = request;
        Decision = decision;
    }

    /// <summary>Gets the exact request whose participation was considered.</summary>
    public CharacterTurnParticipationConsiderationRequest Request { get; }

    /// <summary>Gets the exact accepted participation decision.</summary>
    public CharacterTurnParticipationDecision Decision { get; }
}
