namespace Allrpcs.Domain.Characters;

/// <summary>Retains one complete exact two-character incompatibility response round.</summary>
public sealed class CharacterBeliefGoalIncompatibilityResponseRound
{
    /// <summary>Creates one immutable round in stable declaration order.</summary>
    public CharacterBeliefGoalIncompatibilityResponseRound(
        IReadOnlyList<CharacterBeliefGoalIncompatibilityPerspective> perspectives,
        IReadOnlyList<CharacterBeliefGoalIncompatibilityResponse> responses)
    {
        ArgumentNullException.ThrowIfNull(perspectives);
        ArgumentNullException.ThrowIfNull(responses);

        CharacterBeliefGoalIncompatibilityPerspective[] retainedPerspectives =
            [.. perspectives];
        CharacterBeliefGoalIncompatibilityResponse[] retainedResponses = [.. responses];

        RequireCompletePerspectives(retainedPerspectives, nameof(perspectives));
        RequireCompleteResponses(
            retainedPerspectives,
            retainedResponses,
            nameof(responses));

        Perspectives = Array.AsReadOnly(retainedPerspectives);
        Responses = Array.AsReadOnly(retainedResponses);
    }

    /// <summary>Gets the two exact perspectives in stable declaration order.</summary>
    public IReadOnlyList<CharacterBeliefGoalIncompatibilityPerspective> Perspectives { get; }

    /// <summary>Gets the exact response for each retained perspective in the same order.</summary>
    public IReadOnlyList<CharacterBeliefGoalIncompatibilityResponse> Responses { get; }

    private static void RequireCompletePerspectives(
        CharacterBeliefGoalIncompatibilityPerspective[] perspectives,
        string parameterName)
    {
        if (perspectives.Length != 2 || perspectives.Any(perspective => perspective is null))
        {
            throw new ArgumentException(
                "Exactly two non-null perspectives are required.",
                parameterName);
        }

        CharacterBeliefGoalIncompatibility declaration = perspectives[0].Declaration;
        if (!ReferenceEquals(declaration, perspectives[1].Declaration) ||
            !ReferenceEquals(declaration.FirstCharacter, perspectives[0].SubjectCharacter) ||
            !ReferenceEquals(declaration.SecondCharacter, perspectives[1].SubjectCharacter))
        {
            throw new ArgumentException(
                "Perspectives must retain one exact declaration in stable subject order.",
                parameterName);
        }
    }

    private static void RequireCompleteResponses(
        CharacterBeliefGoalIncompatibilityPerspective[] perspectives,
        CharacterBeliefGoalIncompatibilityResponse[] responses,
        string parameterName)
    {
        if (responses.Length != 2 || responses.Any(response => response is null))
        {
            throw new ArgumentException(
                "Exactly two non-null responses are required.",
                parameterName);
        }

        if (!ReferenceEquals(perspectives[0], responses[0].Perspective) ||
            !ReferenceEquals(perspectives[1], responses[1].Perspective))
        {
            throw new ArgumentException(
                "Each response must retain its exact perspective in stable order.",
                parameterName);
        }
    }
}
