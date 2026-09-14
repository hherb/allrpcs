namespace Allrpcs.Domain.Characters;

/// <summary>Requests one provider-neutral response consideration for one exact perspective.</summary>
public sealed record CharacterBeliefGoalIncompatibilityConsiderationRequest
{
    /// <summary>Creates one immutable request without selecting or applying a response.</summary>
    public CharacterBeliefGoalIncompatibilityConsiderationRequest(
        CharacterBeliefGoalIncompatibilityPerspective perspective)
    {
        ArgumentNullException.ThrowIfNull(perspective);

        Perspective = perspective;
    }

    /// <summary>Gets the exact incompatibility perspective to consider.</summary>
    public CharacterBeliefGoalIncompatibilityPerspective Perspective { get; }
}
