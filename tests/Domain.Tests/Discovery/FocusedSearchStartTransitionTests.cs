using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Discovery;

public sealed class FocusedSearchStartTransitionTests
{
    [Fact]
    public void StartsSearchInEmptySnapshotWithExactRequestInstance()
    {
        FocusedSearchRequest request = Request("seeker", "weathered-inscription");
        FocusedSearchActivitySnapshot activities = Activities();

        FocusedSearchActivitySnapshot result =
            FocusedSearchStartTransition.StartSearch(request, activities);

        FocusedSearchActivity startedActivity = Assert.Single(result.Entries);
        Assert.Same(request, startedActivity.Request);
        Assert.NotSame(activities, result);
        Assert.Empty(activities.Entries);
    }

    [Fact]
    public void AppendsSearchAfterExistingActivitiesInStableOrder()
    {
        FocusedSearchRequest request = Request("seeker", "weathered-inscription");
        FocusedSearchActivity first = Activity("travelling-healer", "concealed-healing-herb");
        FocusedSearchActivity second = Activity("sage", "wagon-residue");
        FocusedSearchActivitySnapshot activities = new(new[] { first, second });

        FocusedSearchActivitySnapshot result =
            FocusedSearchStartTransition.StartSearch(request, activities);

        Assert.Collection(
            result.Entries,
            entry => Assert.Same(first, entry),
            entry => Assert.Same(second, entry),
            entry => Assert.Same(request, entry.Request));
    }

    [Theory]
    [InlineData("weathered-inscription")]
    [InlineData("concealed-healing-herb")]
    public void RejectsSearchWhenSearcherAlreadyHasAnActivity(string existingTargetObjectId)
    {
        FocusedSearchRequest request = Request("seeker", "weathered-inscription");
        FocusedSearchActivitySnapshot activities = Activities(("seeker", existingTargetObjectId));

        Assert.Throws<InvalidOperationException>(
            () => FocusedSearchStartTransition.StartSearch(request, activities));
    }

    [Fact]
    public void MatchesSearcherIdentifiersByValueWhenRejectingSearch()
    {
        CharacterId requestSearcherCharacterId = new("seeker");
        CharacterId activitySearcherCharacterId = new("seeker");
        FocusedSearchRequest request = new(
            requestSearcherCharacterId,
            new ObjectId("weathered-inscription"));
        FocusedSearchActivity existingActivity = new(
            new FocusedSearchRequest(
                activitySearcherCharacterId,
                new ObjectId("concealed-healing-herb")));
        FocusedSearchActivitySnapshot activities = new(new[] { existingActivity });

        Assert.Throws<InvalidOperationException>(
            () => FocusedSearchStartTransition.StartSearch(request, activities));
        Assert.NotSame(requestSearcherCharacterId, activitySearcherCharacterId);
        Assert.Equal(requestSearcherCharacterId, activitySearcherCharacterId);
    }

    [Fact]
    public void AllowsDifferentSearcherToSearchTheSameTarget()
    {
        FocusedSearchRequest request = Request("seeker", "weathered-inscription");
        FocusedSearchActivity existing = Activity("travelling-healer", "weathered-inscription");
        FocusedSearchActivitySnapshot activities = new(new[] { existing });

        FocusedSearchActivitySnapshot result =
            FocusedSearchStartTransition.StartSearch(request, activities);

        Assert.Collection(
            result.Entries,
            entry => Assert.Same(existing, entry),
            entry => Assert.Same(request, entry.Request));
    }

    [Fact]
    public void RejectsNullInputs()
    {
        FocusedSearchRequest request = Request("seeker", "weathered-inscription");
        FocusedSearchActivitySnapshot activities = Activities();

        Assert.Throws<ArgumentNullException>(
            () => FocusedSearchStartTransition.StartSearch(null!, activities));
        Assert.Throws<ArgumentNullException>(
            () => FocusedSearchStartTransition.StartSearch(request, null!));
    }

    [Fact]
    public void DoesNotMutateInputsWhenSearchIsRejected()
    {
        CharacterId searcherCharacterId = new("seeker");
        ObjectId targetObjectId = new("weathered-inscription");
        FocusedSearchRequest request = new(searcherCharacterId, targetObjectId);
        FocusedSearchActivity existing = Activity("seeker", "concealed-healing-herb");
        FocusedSearchActivitySnapshot activities = new(new[] { existing });

        _ = Assert.Throws<InvalidOperationException>(
            () => FocusedSearchStartTransition.StartSearch(request, activities));

        Assert.Same(searcherCharacterId, request.SearcherCharacterId);
        Assert.Same(targetObjectId, request.TargetObjectId);
        Assert.Collection(activities.Entries, entry => Assert.Same(existing, entry));
    }

    [Fact]
    public void ExposesOnlyTheSinglePureTransitionOperation()
    {
        Type transitionType = typeof(FocusedSearchStartTransition);
        System.Reflection.MethodInfo method = Assert.Single(
            transitionType
                .GetMethods()
                .Where(candidate => candidate.DeclaringType == transitionType));

        Assert.Equal("StartSearch", method.Name);
        Assert.Equal(typeof(FocusedSearchActivitySnapshot), method.ReturnType);
        Assert.True(method.IsStatic);
        Assert.Collection(
            method.GetParameters(),
            parameter => Assert.Equal(typeof(FocusedSearchRequest), parameter.ParameterType),
            parameter => Assert.Equal(
                typeof(FocusedSearchActivitySnapshot),
                parameter.ParameterType));
        Assert.Empty(transitionType.GetConstructors());
        Assert.Empty(transitionType.GetProperties());
        Assert.Empty(transitionType.GetEvents());
        Assert.True(transitionType.IsAbstract);
        Assert.True(transitionType.IsSealed);
    }

    private static FocusedSearchRequest Request(string characterId, string targetObjectId) =>
        new(new CharacterId(characterId), new ObjectId(targetObjectId));

    private static FocusedSearchActivity Activity(string characterId, string targetObjectId) =>
        new(Request(characterId, targetObjectId));

    private static FocusedSearchActivitySnapshot Activities(
        params (string CharacterId, string TargetObjectId)[] entries) =>
        new(entries.Select(entry => Activity(entry.CharacterId, entry.TargetObjectId)));
}
