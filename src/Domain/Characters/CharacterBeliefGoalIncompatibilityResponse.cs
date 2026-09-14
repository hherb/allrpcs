namespace Allrpcs.Domain.Characters;

/// <summary>Retains one provider-neutral response to one exact incompatibility perspective.</summary>
public sealed record CharacterBeliefGoalIncompatibilityResponse
{
    /// <summary>Gets the maximum supported response explanation length.</summary>
    public const int MaximumExplanationLength = 500;

    /// <summary>Creates one explicit response value without evaluating or applying it.</summary>
    public CharacterBeliefGoalIncompatibilityResponse(
        CharacterBeliefGoalIncompatibilityPerspective perspective,
        CharacterBeliefGoalIncompatibilityResponseKind kind,
        string explanation)
    {
        ArgumentNullException.ThrowIfNull(perspective);
        ArgumentNullException.ThrowIfNull(kind);

        Perspective = perspective;
        Kind = kind;
        Explanation = AuthoredCharacterText.RequireNonBlank(
            explanation,
            MaximumExplanationLength,
            nameof(explanation));
    }

    /// <summary>Gets the exact perspective being answered.</summary>
    public CharacterBeliefGoalIncompatibilityPerspective Perspective { get; }

    /// <summary>Gets the exact canonical response kind.</summary>
    public CharacterBeliefGoalIncompatibilityResponseKind Kind { get; }

    /// <summary>Gets the bounded explicit response explanation.</summary>
    public string Explanation { get; }
}
