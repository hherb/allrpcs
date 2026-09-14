using Allrpcs.Domain.Characters;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterBeliefGoalIncompatibilityConsiderationResultTests
{
    [Theory]
    [InlineData(0, "maintain-position")]
    [InlineData(0, "seek-reconciliation")]
    [InlineData(1, "reconsider-belief")]
    [InlineData(1, "reconsider-goal")]
    public void RetainsAnExactAcceptedRequestAndResponse(int perspectiveIndex, string kindValue)
    {
        CharacterBeliefGoalIncompatibility declaration =
            RepresentativeSageEnemyAgentIncompatibilitySource.Create();
        CharacterBeliefGoalIncompatibilityPerspective perspective =
            CharacterBeliefGoalIncompatibilityPerspectiveProjection.Project(declaration)[perspectiveIndex];
        CharacterBeliefGoalIncompatibilityConsiderationRequest request = new(perspective);
        CharacterBeliefGoalIncompatibilityResponse response = new(
            perspective,
            CharacterBeliefGoalIncompatibilityResponseKind.Parse(kindValue),
            "An exact explicit response to the retained perspective.");

        CharacterBeliefGoalIncompatibilityConsiderationResult result = new(request, response);

        Assert.Same(request, result.Request);
        Assert.Same(response, result.Response);
        Assert.Same(request.Perspective, result.Response.Perspective);
    }

    [Fact]
    public void RejectsOtherAndDistinctValueEqualPerspectivesWithoutChangingInputs()
    {
        CharacterBeliefGoalIncompatibility declaration =
            RepresentativeSageEnemyAgentIncompatibilitySource.Create();
        IReadOnlyList<CharacterBeliefGoalIncompatibilityPerspective> perspectives =
            CharacterBeliefGoalIncompatibilityPerspectiveProjection.Project(declaration);
        CharacterBeliefGoalIncompatibilityConsiderationRequest request = new(perspectives[0]);
        CharacterBeliefGoalIncompatibilityPerspective replacement = new(
            declaration,
            declaration.FirstCharacter);
        CharacterBelief[] beliefs = [.. declaration.FirstCharacter.Beliefs.Entries];
        CharacterGoal[] goals = [.. declaration.FirstCharacter.Goals.Entries];

        Assert.Throws<ArgumentException>(() =>
            new CharacterBeliefGoalIncompatibilityConsiderationResult(
                request,
                Response(perspectives[1])));
        Assert.Throws<ArgumentException>(() =>
            new CharacterBeliefGoalIncompatibilityConsiderationResult(
                request,
                Response(replacement)));
        Assert.Equal(beliefs, declaration.FirstCharacter.Beliefs.Entries);
        Assert.Equal(goals, declaration.FirstCharacter.Goals.Entries);
    }

    [Fact]
    public void RejectsIndependentAndValueEqualReplacementDeclarations()
    {
        CharacterBeliefGoalIncompatibility declaration =
            RepresentativeSageEnemyAgentIncompatibilitySource.Create();
        CharacterBeliefGoalIncompatibilityPerspective perspective =
            CharacterBeliefGoalIncompatibilityPerspectiveProjection.Project(declaration)[1];
        CharacterBeliefGoalIncompatibilityConsiderationRequest request = new(perspective);
        CharacterBeliefGoalIncompatibility independentDeclaration =
            RepresentativeSageEnemyAgentIncompatibilitySource.Create();
        CharacterBeliefGoalIncompatibilityPerspective independentPerspective =
            CharacterBeliefGoalIncompatibilityPerspectiveProjection.Project(independentDeclaration)[1];
        CharacterBeliefGoalIncompatibility replacementDeclaration = new(
            declaration.FirstCharacter,
            declaration.FirstBelief,
            declaration.FirstGoal,
            declaration.SecondCharacter,
            declaration.SecondBelief,
            declaration.SecondGoal,
            declaration.Rationale);
        CharacterBeliefGoalIncompatibilityPerspective replacementPerspective = new(
            replacementDeclaration,
            replacementDeclaration.SecondCharacter);

        Assert.Throws<ArgumentException>(() =>
            new CharacterBeliefGoalIncompatibilityConsiderationResult(
                request,
                Response(independentPerspective)));
        Assert.Throws<ArgumentException>(() =>
            new CharacterBeliefGoalIncompatibilityConsiderationResult(
                request,
                Response(replacementPerspective)));
    }

    [Fact]
    public void RejectsNullsAndExposesOnlyTheImmutableResultSurface()
    {
        CharacterBeliefGoalIncompatibility declaration =
            RepresentativeSageEnemyAgentIncompatibilitySource.Create();
        CharacterBeliefGoalIncompatibilityPerspective perspective =
            CharacterBeliefGoalIncompatibilityPerspectiveProjection.Project(declaration)[0];
        CharacterBeliefGoalIncompatibilityConsiderationRequest request = new(perspective);
        CharacterBeliefGoalIncompatibilityResponse response = Response(perspective);

        Assert.Throws<ArgumentNullException>(() =>
            new CharacterBeliefGoalIncompatibilityConsiderationResult(null!, response));
        Assert.Throws<ArgumentNullException>(() =>
            new CharacterBeliefGoalIncompatibilityConsiderationResult(request, null!));

        Type resultType = typeof(CharacterBeliefGoalIncompatibilityConsiderationResult);
        System.Reflection.ConstructorInfo constructor = Assert.Single(resultType.GetConstructors());
        (string Name, Type Type, bool CanWrite)[] properties = resultType
            .GetProperties()
            .OrderBy(property => property.Name, StringComparer.Ordinal)
            .Select(property => (property.Name, property.PropertyType, property.CanWrite))
            .ToArray();

        Assert.Equal(
            [
                typeof(CharacterBeliefGoalIncompatibilityConsiderationRequest),
                typeof(CharacterBeliefGoalIncompatibilityResponse),
            ],
            constructor.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Equal(
            [
                ("Request", typeof(CharacterBeliefGoalIncompatibilityConsiderationRequest)),
                ("Response", typeof(CharacterBeliefGoalIncompatibilityResponse)),
            ],
            properties.Select(property => (property.Name, property.Type)).ToArray());
        Assert.All(properties, property => Assert.False(property.CanWrite));
        Assert.Empty(resultType.GetEvents());
        Assert.True(resultType.IsSealed);
    }

    private static CharacterBeliefGoalIncompatibilityResponse Response(
        CharacterBeliefGoalIncompatibilityPerspective perspective) => new(
            perspective,
            CharacterBeliefGoalIncompatibilityResponseKind.SeekReconciliation,
            "Seek a jointly supported reconciliation.");
}
