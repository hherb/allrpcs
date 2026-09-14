using Allrpcs.Domain.Characters;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterBeliefGoalIncompatibilityResponseTests
{
    [Fact]
    public void RetainsTheExactPerspectiveKindAndBoundedExplanation()
    {
        CharacterBeliefGoalIncompatibility declaration =
            RepresentativeSageEnemyAgentIncompatibilitySource.Create();
        CharacterBeliefGoalIncompatibilityPerspective perspective = new(
            declaration,
            declaration.SecondCharacter);

        CharacterBeliefGoalIncompatibilityResponse response = new(
            perspective,
            CharacterBeliefGoalIncompatibilityResponseKind.ReconsiderGoal,
            "Protecting civilians requires reconsidering the delay objective.");

        Assert.Same(perspective, response.Perspective);
        Assert.Same(
            CharacterBeliefGoalIncompatibilityResponseKind.ReconsiderGoal,
            response.Kind);
        Assert.Equal(
            "Protecting civilians requires reconsidering the delay objective.",
            response.Explanation);
    }

    [Fact]
    public void AcceptsEveryCanonicalResponseKindWithoutApplyingIt()
    {
        CharacterBeliefGoalIncompatibility declaration =
            RepresentativeSageEnemyAgentIncompatibilitySource.Create();
        CharacterBeliefGoalIncompatibilityPerspective perspective = new(
            declaration,
            declaration.FirstCharacter);

        CharacterBeliefGoalIncompatibilityResponse[] responses =
            CharacterBeliefGoalIncompatibilityResponseKind.All
                .Select(kind => new CharacterBeliefGoalIncompatibilityResponse(
                    perspective,
                    kind,
                    "Explicit authored explanation."))
                .ToArray();

        Assert.Equal(CharacterBeliefGoalIncompatibilityResponseKind.All.Count, responses.Length);
        Assert.All(responses, response => Assert.Same(perspective, response.Perspective));
        Assert.Equal(
            CharacterBeliefGoalIncompatibilityResponseKind.All,
            responses.Select(response => response.Kind));
        Assert.Same(declaration.FirstBelief, perspective.SubjectBelief);
        Assert.Same(declaration.FirstGoal, perspective.SubjectGoal);
    }

    [Fact]
    public void RejectsNullComponentsAndRequiresANonBlankBoundedExplanation()
    {
        CharacterBeliefGoalIncompatibility declaration =
            RepresentativeSageEnemyAgentIncompatibilitySource.Create();
        CharacterBeliefGoalIncompatibilityPerspective perspective = new(
            declaration,
            declaration.FirstCharacter);
        CharacterBeliefGoalIncompatibilityResponseKind kind =
            CharacterBeliefGoalIncompatibilityResponseKind.MaintainPosition;

        CharacterBeliefGoalIncompatibilityResponse maximum = new(
            perspective,
            kind,
            new string('e', CharacterBeliefGoalIncompatibilityResponse.MaximumExplanationLength));

        Assert.Equal(
            CharacterBeliefGoalIncompatibilityResponse.MaximumExplanationLength,
            maximum.Explanation.Length);
        Assert.Throws<ArgumentNullException>(() =>
            new CharacterBeliefGoalIncompatibilityResponse(null!, kind, "explanation"));
        Assert.Throws<ArgumentNullException>(() =>
            new CharacterBeliefGoalIncompatibilityResponse(perspective, null!, "explanation"));
        Assert.Throws<ArgumentNullException>(() =>
            new CharacterBeliefGoalIncompatibilityResponse(perspective, kind, null!));
        Assert.Throws<ArgumentException>(() =>
            new CharacterBeliefGoalIncompatibilityResponse(perspective, kind, string.Empty));
        Assert.Throws<ArgumentException>(() =>
            new CharacterBeliefGoalIncompatibilityResponse(perspective, kind, " \t\r\n"));
        Assert.Throws<ArgumentException>(() =>
            new CharacterBeliefGoalIncompatibilityResponse(
                perspective,
                kind,
                new string(
                    'e',
                    CharacterBeliefGoalIncompatibilityResponse.MaximumExplanationLength + 1)));
    }

    [Fact]
    public void ExposesOnlyImmutableResponseState()
    {
        Type responseType = typeof(CharacterBeliefGoalIncompatibilityResponse);
        (string Name, Type Type, bool CanWrite)[] properties = responseType
            .GetProperties()
            .OrderBy(property => property.Name, StringComparer.Ordinal)
            .Select(property => (property.Name, property.PropertyType, property.CanWrite))
            .ToArray();

        Assert.Equal(
            [
                ("Explanation", typeof(string)),
                ("Kind", typeof(CharacterBeliefGoalIncompatibilityResponseKind)),
                ("Perspective", typeof(CharacterBeliefGoalIncompatibilityPerspective)),
            ],
            properties.Select(property => (property.Name, property.Type)).ToArray());
        Assert.All(properties, property => Assert.False(property.CanWrite));
        Assert.Empty(responseType.GetEvents());
        Assert.True(responseType.IsSealed);
    }
}
