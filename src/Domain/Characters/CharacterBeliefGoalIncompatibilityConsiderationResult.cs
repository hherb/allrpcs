namespace Allrpcs.Domain.Characters;

/// <summary>Binds one accepted incompatibility response to its exact consideration request.</summary>
public sealed record CharacterBeliefGoalIncompatibilityConsiderationResult
{
    /// <summary>Creates one immutable result only for an exact accepted response.</summary>
    public CharacterBeliefGoalIncompatibilityConsiderationResult(
        CharacterBeliefGoalIncompatibilityConsiderationRequest request,
        CharacterBeliefGoalIncompatibilityResponse response)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(response);

        if (!CharacterBeliefGoalIncompatibilityResponsePolicy.CanAccept(request, response))
        {
            throw new ArgumentException(
                "The incompatibility response must be valid for the exact consideration request.",
                nameof(response));
        }

        Request = request;
        Response = response;
    }

    /// <summary>Gets the exact request whose incompatibility response was considered.</summary>
    public CharacterBeliefGoalIncompatibilityConsiderationRequest Request { get; }

    /// <summary>Gets the exact accepted incompatibility response.</summary>
    public CharacterBeliefGoalIncompatibilityResponse Response { get; }
}
