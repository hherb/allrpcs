using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Discovery;

public sealed class FocusedSearchCancellationTransitionTests
{
    [Fact]
    public void RejectsCancellationWhenSnapshotIsEmpty()
    {
        FocusedSearchRequest request = Request("seeker", "weathered-inscription");
        FocusedSearchActivitySnapshot activities = Activities();

        Assert.Throws<InvalidOperationException>(
            () => FocusedSearchCancellationTransition.CancelSearch(request, activities));
    }

    [Theory]
    [InlineData("seeker", "concealed-healing-herb")]
    [InlineData("travelling-healer", "weathered-inscription")]
    public void RejectsCancellationForNearMatch(
        string activeCharacterId,
        string activeTargetObjectId)
    {
        FocusedSearchRequest request = Request("seeker", "weathered-inscription");
        FocusedSearchActivity existing = Activity(activeCharacterId, activeTargetObjectId);
        FocusedSearchActivitySnapshot activities = new(new[] { existing });

        Assert.Throws<InvalidOperationException>(
            () => FocusedSearchCancellationTransition.CancelSearch(request, activities));
        Assert.Collection(activities.Entries, entry => Assert.Same(existing, entry));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void RemovesValueEqualCompleteRequestFromRepresentativePosition(int matchingIndex)
    {
        FocusedSearchRequest request = Request("seeker", "weathered-inscription");
        FocusedSearchActivity first = Activity("travelling-healer", "concealed-healing-herb");
        FocusedSearchActivity matching = Activity("seeker", "weathered-inscription");
        FocusedSearchActivity third = Activity("sage", "wagon-residue");
        FocusedSearchActivity[] entries = [first, matching, third];
        (entries[matchingIndex], entries[1]) = (entries[1], entries[matchingIndex]);
        FocusedSearchActivitySnapshot activities = new(entries);

        FocusedSearchActivitySnapshot result =
            FocusedSearchCancellationTransition.CancelSearch(request, activities);
        FocusedSearchActivity[] expected = entries
            .Where(entry => !ReferenceEquals(entry, matching))
            .ToArray();

        Assert.NotSame(request, matching.Request);
        Assert.Equal(request, matching.Request);
        Assert.Collection(
            result.Entries,
            entry => Assert.Same(expected[0], entry),
            entry => Assert.Same(expected[1], entry));
    }

    [Fact]
    public void PreservesStableOrderAndExactInstancesOfRemainingActivities()
    {
        FocusedSearchRequest request = Request("seeker", "weathered-inscription");
        FocusedSearchActivity first = Activity("travelling-healer", "concealed-healing-herb");
        FocusedSearchActivity matching = new(request);
        FocusedSearchActivity third = Activity("sage", "wagon-residue");
        FocusedSearchActivitySnapshot activities = new(new[] { first, matching, third });

        FocusedSearchActivitySnapshot result =
            FocusedSearchCancellationTransition.CancelSearch(request, activities);

        Assert.Collection(
            result.Entries,
            entry => Assert.Same(first, entry),
            entry => Assert.Same(third, entry));
    }

    [Fact]
    public void RejectsNullInputsThroughCancellationPolicy()
    {
        FocusedSearchRequest request = Request("seeker", "weathered-inscription");
        FocusedSearchActivitySnapshot activities = Activities();

        Assert.Throws<ArgumentNullException>(
            () => FocusedSearchCancellationTransition.CancelSearch(null!, activities));
        Assert.Throws<ArgumentNullException>(
            () => FocusedSearchCancellationTransition.CancelSearch(request, null!));
    }

    [Fact]
    public void DoesNotMutateInputsWhenCancellationSucceeds()
    {
        CharacterId searcherCharacterId = new("seeker");
        ObjectId targetObjectId = new("weathered-inscription");
        FocusedSearchRequest request = new(searcherCharacterId, targetObjectId);
        FocusedSearchActivity matching = new(request);
        FocusedSearchActivity existing = Activity("sage", "wagon-residue");
        FocusedSearchActivitySnapshot activities = new(new[] { matching, existing });

        FocusedSearchActivitySnapshot result =
            FocusedSearchCancellationTransition.CancelSearch(request, activities);

        Assert.Same(searcherCharacterId, request.SearcherCharacterId);
        Assert.Same(targetObjectId, request.TargetObjectId);
        Assert.Collection(
            activities.Entries,
            entry => Assert.Same(matching, entry),
            entry => Assert.Same(existing, entry));
        Assert.Collection(result.Entries, entry => Assert.Same(existing, entry));
        Assert.NotSame(activities, result);
    }

    [Fact]
    public void DoesNotMutateInputsWhenCancellationIsRejected()
    {
        CharacterId searcherCharacterId = new("seeker");
        ObjectId targetObjectId = new("weathered-inscription");
        FocusedSearchRequest request = new(searcherCharacterId, targetObjectId);
        FocusedSearchActivity existing = Activity("travelling-healer", "concealed-healing-herb");
        FocusedSearchActivitySnapshot activities = new(new[] { existing });

        _ = Assert.Throws<InvalidOperationException>(
            () => FocusedSearchCancellationTransition.CancelSearch(request, activities));

        Assert.Same(searcherCharacterId, request.SearcherCharacterId);
        Assert.Same(targetObjectId, request.TargetObjectId);
        Assert.Collection(activities.Entries, entry => Assert.Same(existing, entry));
    }

    [Fact]
    public void ExposesOnlyTheSinglePureTransitionOperation()
    {
        Type transitionType = typeof(FocusedSearchCancellationTransition);
        System.Reflection.MethodInfo method = Assert.Single(
            transitionType
                .GetMethods()
                .Where(candidate => candidate.DeclaringType == transitionType));

        Assert.Equal("CancelSearch", method.Name);
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
