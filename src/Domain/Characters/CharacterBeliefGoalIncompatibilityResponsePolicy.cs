namespace Allrpcs.Domain.Characters;

/// <summary>Validates one response against one exact incompatibility consideration request.</summary>
public static class CharacterBeliefGoalIncompatibilityResponsePolicy
{
    /// <summary>Returns whether the response retains the request's exact perspective.</summary>
    public static bool CanAccept(
        CharacterBeliefGoalIncompatibilityConsiderationRequest request,
        CharacterBeliefGoalIncompatibilityResponse response)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(response);

        return ReferenceEquals(request.Perspective, response.Perspective);
    }
}
