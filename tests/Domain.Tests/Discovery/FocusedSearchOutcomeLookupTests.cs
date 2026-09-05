using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;

namespace Allrpcs.Domain.Tests.Discovery;

public sealed class FocusedSearchOutcomeLookupTests
{
    [Fact]
    public void ReturnsNullForEmptyOrUnrelatedSnapshots()
    {
        FocusedSearchRequest request = Request("seeker", "weathered-inscription");
        FocusedSearchOutcome unrelated = Outcome(
            "travelling-healer",
            "concealed-healing-herb",
            VisibilityState.Identified);

        FocusedSearchOutcome? emptyResult = FocusedSearchOutcomeLookup.FindByRequest(
            request,
            Outcomes());
        FocusedSearchOutcome? unrelatedResult = FocusedSearchOutcomeLookup.FindByRequest(
            request,
            Outcomes(unrelated));

        Assert.Null(emptyResult);
        Assert.Null(unrelatedResult);
    }

    [Fact]
    public void MatchesAValueEqualDistinctRequestAndRetainsExactOutcomeState()
    {
        CharacterId retainedCharacterId = new("seeker");
        ObjectId retainedTargetObjectId = new("weathered-inscription");
        FocusedSearchRequest retainedRequest = new(
            retainedCharacterId,
            retainedTargetObjectId);
        VisibilityState retainedVisibilityState = VisibilityState.Understood;
        FocusedSearchOutcome retainedOutcome = new(
            retainedRequest,
            retainedVisibilityState);
        FocusedSearchRequest lookupRequest = Request(
            "seeker",
            "weathered-inscription");
        FocusedSearchOutcomeSnapshot outcomes = Outcomes(retainedOutcome);

        FocusedSearchOutcome? result = FocusedSearchOutcomeLookup.FindByRequest(
            lookupRequest,
            outcomes);

        Assert.NotSame(lookupRequest, retainedRequest);
        Assert.Equal(lookupRequest, retainedRequest);
        Assert.NotNull(result);
        Assert.Same(retainedOutcome, result);
        Assert.Same(retainedRequest, result.Request);
        Assert.Same(retainedCharacterId, result.Request.SearcherCharacterId);
        Assert.Same(retainedTargetObjectId, result.Request.TargetObjectId);
        Assert.Same(retainedVisibilityState, result.ResultingVisibilityState);
    }

    [Fact]
    public void RejectsBothPartialRequestMatches()
    {
        FocusedSearchRequest request = Request("seeker", "weathered-inscription");
        FocusedSearchOutcome sameSearcherDifferentTarget = Outcome(
            "seeker",
            "concealed-healing-herb",
            VisibilityState.Identified);
        FocusedSearchOutcome differentSearcherSameTarget = Outcome(
            "travelling-healer",
            "weathered-inscription",
            VisibilityState.Understood);
        FocusedSearchOutcomeSnapshot outcomes = Outcomes(
            sameSearcherDifferentTarget,
            differentSearcherSameTarget);

        FocusedSearchOutcome? result = FocusedSearchOutcomeLookup.FindByRequest(
            request,
            outcomes);

        Assert.Null(result);
    }

    [Fact]
    public void ReturnsTheExactMatchAmongSurroundingUnrelatedOutcomes()
    {
        FocusedSearchRequest request = Request("seeker", "weathered-inscription");
        FocusedSearchOutcome before = Outcome(
            "sage",
            "wagon-residue",
            VisibilityState.Noticed);
        FocusedSearchOutcome matching = new(request, VisibilityState.Identified);
        FocusedSearchOutcome after = Outcome(
            "seeker",
            "concealed-healing-herb",
            VisibilityState.Understood);
        FocusedSearchOutcomeSnapshot outcomes = Outcomes(before, matching, after);

        FocusedSearchOutcome? result = FocusedSearchOutcomeLookup.FindByRequest(
            request,
            outcomes);

        Assert.Same(matching, result);
    }

    [Fact]
    public void DoesNotMutateAnyInput()
    {
        FocusedSearchRequest request = Request("seeker", "weathered-inscription");
        FocusedSearchOutcome matching = new(request, VisibilityState.Identified);
        FocusedSearchOutcome unrelated = Outcome(
            "sage",
            "wagon-residue",
            VisibilityState.Noticed);
        FocusedSearchOutcomeSnapshot outcomes = Outcomes(matching, unrelated);
        CharacterId searcherCharacterId = request.SearcherCharacterId;
        ObjectId targetObjectId = request.TargetObjectId;
        VisibilityState resultingVisibilityState = matching.ResultingVisibilityState;

        FocusedSearchOutcome? result = FocusedSearchOutcomeLookup.FindByRequest(
            request,
            outcomes);

        Assert.Same(matching, result);
        Assert.Collection(
            outcomes.Entries,
            entry => Assert.Same(matching, entry),
            entry => Assert.Same(unrelated, entry));
        Assert.Same(searcherCharacterId, request.SearcherCharacterId);
        Assert.Same(targetObjectId, request.TargetObjectId);
        Assert.Same(resultingVisibilityState, matching.ResultingVisibilityState);
    }

    [Fact]
    public void RejectsNullInputs()
    {
        FocusedSearchRequest request = Request("seeker", "weathered-inscription");
        FocusedSearchOutcomeSnapshot outcomes = Outcomes();

        Assert.Throws<ArgumentNullException>(
            () => FocusedSearchOutcomeLookup.FindByRequest(null!, outcomes));
        Assert.Throws<ArgumentNullException>(
            () => FocusedSearchOutcomeLookup.FindByRequest(request, null!));
    }

    [Fact]
    public void ExposesOnlyTheSinglePureLookupOperation()
    {
        Type lookupType = typeof(FocusedSearchOutcomeLookup);
        System.Reflection.MethodInfo method = Assert.Single(
            lookupType
                .GetMethods()
                .Where(candidate => candidate.DeclaringType == lookupType));

        Assert.Equal("FindByRequest", method.Name);
        Assert.Equal(typeof(FocusedSearchOutcome), method.ReturnType);
        Assert.True(method.IsStatic);
        Assert.Collection(
            method.GetParameters(),
            parameter => Assert.Equal(
                typeof(FocusedSearchRequest),
                parameter.ParameterType),
            parameter => Assert.Equal(
                typeof(FocusedSearchOutcomeSnapshot),
                parameter.ParameterType));
        Assert.Empty(lookupType.GetConstructors());
        Assert.Empty(lookupType.GetProperties());
        Assert.Empty(lookupType.GetEvents());
        Assert.True(lookupType.IsAbstract);
        Assert.True(lookupType.IsSealed);
    }

    private static FocusedSearchOutcome Outcome(
        string characterId,
        string targetObjectId,
        VisibilityState resultingVisibilityState) =>
        new(Request(characterId, targetObjectId), resultingVisibilityState);

    private static FocusedSearchOutcomeSnapshot Outcomes(
        params FocusedSearchOutcome[] entries) =>
        new(entries);

    private static FocusedSearchRequest Request(
        string characterId,
        string targetObjectId) =>
        new(new CharacterId(characterId), new ObjectId(targetObjectId));
}
