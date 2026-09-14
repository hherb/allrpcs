using Allrpcs.Domain.Characters;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterBeliefGoalIncompatibilityConsiderationRequestTests
{
    [Fact]
    public void RetainsTheExactPerspectiveAndEveryNestedDeclarationReference()
    {
        CharacterBeliefGoalIncompatibility declaration =
            RepresentativeSageEnemyAgentIncompatibilitySource.Create();
        CharacterBeliefGoalIncompatibilityPerspective perspective =
            CharacterBeliefGoalIncompatibilityPerspectiveProjection.Project(declaration)[1];

        CharacterBeliefGoalIncompatibilityConsiderationRequest request = new(perspective);

        Assert.Same(perspective, request.Perspective);
        Assert.Same(declaration, request.Perspective.Declaration);
        Assert.Same(declaration.SecondCharacter, request.Perspective.SubjectCharacter);
        Assert.Same(declaration.SecondBelief, request.Perspective.SubjectBelief);
        Assert.Same(declaration.SecondGoal, request.Perspective.SubjectGoal);
        Assert.Same(declaration.FirstCharacter, request.Perspective.CounterpartCharacter);
        Assert.Same(declaration.FirstBelief, request.Perspective.CounterpartBelief);
        Assert.Same(declaration.FirstGoal, request.Perspective.CounterpartGoal);
    }

    [Fact]
    public void FreshFixturesProduceIndependentRequestsWithoutChangingCharacters()
    {
        CharacterBeliefGoalIncompatibility firstDeclaration =
            RepresentativeSageEnemyAgentIncompatibilitySource.Create();
        CharacterBeliefGoalIncompatibilityPerspective firstPerspective =
            CharacterBeliefGoalIncompatibilityPerspectiveProjection.Project(firstDeclaration)[0];
        CharacterBelief[] firstBeliefs = [.. firstDeclaration.FirstCharacter.Beliefs.Entries];
        CharacterGoal[] firstGoals = [.. firstDeclaration.FirstCharacter.Goals.Entries];
        CharacterBeliefGoalIncompatibility secondDeclaration =
            RepresentativeSageEnemyAgentIncompatibilitySource.Create();
        CharacterBeliefGoalIncompatibilityPerspective secondPerspective =
            CharacterBeliefGoalIncompatibilityPerspectiveProjection.Project(secondDeclaration)[0];

        CharacterBeliefGoalIncompatibilityConsiderationRequest first = new(firstPerspective);
        CharacterBeliefGoalIncompatibilityConsiderationRequest second = new(secondPerspective);

        Assert.NotSame(first, second);
        Assert.NotSame(first.Perspective, second.Perspective);
        Assert.NotSame(first.Perspective.Declaration, second.Perspective.Declaration);
        Assert.Equal(firstBeliefs, firstDeclaration.FirstCharacter.Beliefs.Entries);
        Assert.Equal(firstGoals, firstDeclaration.FirstCharacter.Goals.Entries);
    }

    [Fact]
    public void RejectsNullAndExposesOnlyTheImmutableRequestSurface()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new CharacterBeliefGoalIncompatibilityConsiderationRequest(null!));

        Type requestType = typeof(CharacterBeliefGoalIncompatibilityConsiderationRequest);
        System.Reflection.PropertyInfo property = Assert.Single(requestType.GetProperties());

        Assert.Equal("Perspective", property.Name);
        Assert.Equal(typeof(CharacterBeliefGoalIncompatibilityPerspective), property.PropertyType);
        Assert.False(property.CanWrite);
        Assert.Single(requestType.GetConstructors());
        Assert.Empty(requestType.GetEvents());
        Assert.True(requestType.IsSealed);
    }
}
