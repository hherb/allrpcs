using Allrpcs.Domain.Characters;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterBeliefGoalIncompatibilityResponsePolicyTests
{
    [Fact]
    public void AcceptsEveryCanonicalKindForTheRequestsExactPerspective()
    {
        CharacterBeliefGoalIncompatibility declaration =
            RepresentativeSageEnemyAgentIncompatibilitySource.Create();
        CharacterBeliefGoalIncompatibilityPerspective perspective =
            CharacterBeliefGoalIncompatibilityPerspectiveProjection.Project(declaration)[0];
        CharacterBeliefGoalIncompatibilityConsiderationRequest request = new(perspective);

        foreach (CharacterBeliefGoalIncompatibilityResponseKind kind in
                 CharacterBeliefGoalIncompatibilityResponseKind.All)
        {
            CharacterBeliefGoalIncompatibilityResponse response = new(
                perspective,
                kind,
                "One explicit response explanation.");

            Assert.True(CharacterBeliefGoalIncompatibilityResponsePolicy.CanAccept(request, response));
            Assert.Same(perspective, response.Perspective);
            Assert.Same(kind, response.Kind);
        }
    }

    [Fact]
    public void RejectsOtherAndDistinctReplacementPerspectivesWithoutMutation()
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

        Assert.False(CharacterBeliefGoalIncompatibilityResponsePolicy.CanAccept(
            request,
            Response(perspectives[1])));
        Assert.False(CharacterBeliefGoalIncompatibilityResponsePolicy.CanAccept(
            request,
            Response(replacement)));
        Assert.NotSame(perspectives[0], replacement);
        Assert.Equal(perspectives[0], replacement);
        Assert.Equal(beliefs, declaration.FirstCharacter.Beliefs.Entries);
        Assert.Equal(goals, declaration.FirstCharacter.Goals.Entries);
    }

    [Fact]
    public void RejectsPerspectivesFromIndependentAndValueEqualReplacementDeclarations()
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

        Assert.False(CharacterBeliefGoalIncompatibilityResponsePolicy.CanAccept(
            request,
            Response(independentPerspective)));
        Assert.False(CharacterBeliefGoalIncompatibilityResponsePolicy.CanAccept(
            request,
            Response(replacementPerspective)));
        Assert.NotSame(declaration, replacementDeclaration);
        Assert.Equal(declaration, replacementDeclaration);
    }

    [Fact]
    public void RejectsNullsAndExposesOnlyOnePureAcceptanceQuery()
    {
        CharacterBeliefGoalIncompatibility declaration =
            RepresentativeSageEnemyAgentIncompatibilitySource.Create();
        CharacterBeliefGoalIncompatibilityPerspective perspective =
            CharacterBeliefGoalIncompatibilityPerspectiveProjection.Project(declaration)[0];
        CharacterBeliefGoalIncompatibilityConsiderationRequest request = new(perspective);
        CharacterBeliefGoalIncompatibilityResponse response = Response(perspective);

        Assert.Throws<ArgumentNullException>(() =>
            CharacterBeliefGoalIncompatibilityResponsePolicy.CanAccept(null!, response));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterBeliefGoalIncompatibilityResponsePolicy.CanAccept(request, null!));

        Type policyType = typeof(CharacterBeliefGoalIncompatibilityResponsePolicy);
        System.Reflection.MethodInfo method = Assert.Single(
            policyType.GetMethods().Where(candidate => candidate.DeclaringType == policyType));
        Assert.Equal("CanAccept", method.Name);
        Assert.Equal(typeof(bool), method.ReturnType);
        Assert.True(method.IsStatic);
        Assert.Equal(
            [
                typeof(CharacterBeliefGoalIncompatibilityConsiderationRequest),
                typeof(CharacterBeliefGoalIncompatibilityResponse),
            ],
            method.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Empty(policyType.GetConstructors());
        Assert.Empty(policyType.GetProperties());
        Assert.Empty(policyType.GetEvents());
        Assert.True(policyType.IsAbstract && policyType.IsSealed);
    }

    private static CharacterBeliefGoalIncompatibilityResponse Response(
        CharacterBeliefGoalIncompatibilityPerspective perspective) => new(
            perspective,
            CharacterBeliefGoalIncompatibilityResponseKind.SeekReconciliation,
            "Seek a response supported by both characters' evidence.");
}
