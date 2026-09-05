using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;

namespace Allrpcs.Domain.Tests.Discovery;

public sealed class FocusedSearchOutcomeProjectionTests
{
    [Fact]
    public void ReturnsNewEmptySnapshotWhenThereAreNoOutcomes()
    {
        CharacterId searcherCharacterId = new("seeker");
        FocusedSearchOutcomeSnapshot outcomes = Outcomes();

        FocusedSearchOutcomeSnapshot result =
            FocusedSearchOutcomeProjection.ForCharacter(searcherCharacterId, outcomes);

        Assert.Empty(result.Entries);
        Assert.NotSame(outcomes, result);
    }

    [Fact]
    public void ReturnsEmptySnapshotWhenAllOutcomesAreUnrelated()
    {
        CharacterId searcherCharacterId = new("seeker");
        FocusedSearchOutcome first = Outcome(
            "travelling-healer",
            "concealed-healing-herb",
            VisibilityState.Identified);
        FocusedSearchOutcome second = Outcome(
            "sage",
            "weathered-inscription",
            VisibilityState.Understood);
        FocusedSearchOutcomeSnapshot outcomes = new(new[] { first, second });

        FocusedSearchOutcomeSnapshot result =
            FocusedSearchOutcomeProjection.ForCharacter(searcherCharacterId, outcomes);

        Assert.Empty(result.Entries);
    }

    [Fact]
    public void MatchesAValueEqualCharacterIdentifierAndRetainsExactOutcomeState()
    {
        CharacterId searcherCharacterId = new("seeker");
        CharacterId outcomeCharacterId = new("seeker");
        FocusedSearchRequest request = new(
            outcomeCharacterId,
            new ObjectId("weathered-inscription"));
        VisibilityState resultingVisibilityState = VisibilityState.Identified;
        FocusedSearchOutcome matching = new(request, resultingVisibilityState);
        FocusedSearchOutcomeSnapshot outcomes = new(new[] { matching });

        FocusedSearchOutcomeSnapshot result =
            FocusedSearchOutcomeProjection.ForCharacter(searcherCharacterId, outcomes);

        Assert.NotSame(searcherCharacterId, outcomeCharacterId);
        Assert.Equal(searcherCharacterId, outcomeCharacterId);
        Assert.Collection(
            result.Entries,
            entry =>
            {
                Assert.Same(matching, entry);
                Assert.Same(request, entry.Request);
                Assert.Same(resultingVisibilityState, entry.ResultingVisibilityState);
            });
    }

    [Fact]
    public void ReturnsAllMatchesInStableOrderAndIgnoresUnrelatedOutcomes()
    {
        CharacterId searcherCharacterId = new("seeker");
        FocusedSearchOutcome firstMatch = Outcome(
            "seeker",
            "weathered-inscription",
            VisibilityState.Identified);
        FocusedSearchOutcome unrelated = Outcome(
            "sage",
            "wagon-residue",
            VisibilityState.Noticed);
        FocusedSearchOutcome secondMatch = Outcome(
            "seeker",
            "concealed-healing-herb",
            VisibilityState.Understood);
        FocusedSearchOutcomeSnapshot outcomes =
            new(new[] { firstMatch, unrelated, secondMatch });

        FocusedSearchOutcomeSnapshot result =
            FocusedSearchOutcomeProjection.ForCharacter(searcherCharacterId, outcomes);

        Assert.Collection(
            result.Entries,
            entry => Assert.Same(firstMatch, entry),
            entry => Assert.Same(secondMatch, entry));
    }

    [Fact]
    public void DoesNotMutateAnyInput()
    {
        CharacterId searcherCharacterId = new("seeker");
        FocusedSearchOutcome matching = Outcome(
            "seeker",
            "weathered-inscription",
            VisibilityState.Identified);
        FocusedSearchOutcome unrelated = Outcome(
            "sage",
            "wagon-residue",
            VisibilityState.Noticed);
        FocusedSearchOutcomeSnapshot outcomes = new(new[] { matching, unrelated });
        FocusedSearchRequest matchingRequest = matching.Request;
        VisibilityState matchingVisibilityState = matching.ResultingVisibilityState;

        FocusedSearchOutcomeSnapshot result =
            FocusedSearchOutcomeProjection.ForCharacter(searcherCharacterId, outcomes);

        Assert.Equal("seeker", searcherCharacterId.Value);
        Assert.Collection(
            outcomes.Entries,
            entry => Assert.Same(matching, entry),
            entry => Assert.Same(unrelated, entry));
        Assert.Same(matchingRequest, matching.Request);
        Assert.Same(matchingVisibilityState, matching.ResultingVisibilityState);
        Assert.NotSame(outcomes, result);
    }

    [Fact]
    public void RejectsNullInputs()
    {
        CharacterId searcherCharacterId = new("seeker");
        FocusedSearchOutcomeSnapshot outcomes = Outcomes();

        Assert.Throws<ArgumentNullException>(
            () => FocusedSearchOutcomeProjection.ForCharacter(null!, outcomes));
        Assert.Throws<ArgumentNullException>(
            () => FocusedSearchOutcomeProjection.ForCharacter(searcherCharacterId, null!));
    }

    [Fact]
    public void ExposesOnlyTheSinglePureProjectionOperation()
    {
        Type projectionType = typeof(FocusedSearchOutcomeProjection);
        System.Reflection.MethodInfo method = Assert.Single(
            projectionType
                .GetMethods()
                .Where(candidate => candidate.DeclaringType == projectionType));

        Assert.Equal("ForCharacter", method.Name);
        Assert.Equal(typeof(FocusedSearchOutcomeSnapshot), method.ReturnType);
        Assert.True(method.IsStatic);
        Assert.Collection(
            method.GetParameters(),
            parameter => Assert.Equal(typeof(CharacterId), parameter.ParameterType),
            parameter => Assert.Equal(
                typeof(FocusedSearchOutcomeSnapshot),
                parameter.ParameterType));
        Assert.Empty(projectionType.GetConstructors());
        Assert.Empty(projectionType.GetProperties());
        Assert.Empty(projectionType.GetEvents());
        Assert.True(projectionType.IsAbstract);
        Assert.True(projectionType.IsSealed);
    }

    private static FocusedSearchOutcome Outcome(
        string characterId,
        string targetObjectId,
        VisibilityState resultingVisibilityState) =>
        new(
            new FocusedSearchRequest(
                new CharacterId(characterId),
                new ObjectId(targetObjectId)),
            resultingVisibilityState);

    private static FocusedSearchOutcomeSnapshot Outcomes(
        params FocusedSearchOutcome[] entries) =>
        new(entries);
}
