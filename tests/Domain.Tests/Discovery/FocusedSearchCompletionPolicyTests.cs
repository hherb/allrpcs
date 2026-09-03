using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Discovery;

public sealed class FocusedSearchCompletionPolicyTests
{
    [Fact]
    public void RejectsCompletionWhenSnapshotIsEmpty()
    {
        FocusedSearchRequest request = Request("seeker", "weathered-inscription");
        FocusedSearchActivitySnapshot activities = Activities();

        bool result = FocusedSearchCompletionPolicy.CanCompleteSearch(request, activities);

        Assert.False(result);
    }

    [Fact]
    public void AllowsCompletionWhenExactRequestIsActiveAmongUnrelatedActivities()
    {
        FocusedSearchRequest request = Request("seeker", "weathered-inscription");
        FocusedSearchActivity matchingActivity = new(request);
        FocusedSearchActivitySnapshot activities = new(
            new[]
            {
                Activity("travelling-healer", "concealed-healing-herb"),
                matchingActivity,
                Activity("sage", "wagon-residue"),
            });

        bool result = FocusedSearchCompletionPolicy.CanCompleteSearch(request, activities);

        Assert.True(result);
        Assert.Same(request, matchingActivity.Request);
    }

    [Fact]
    public void MatchesCompleteRequestsByValue()
    {
        FocusedSearchRequest request = Request("seeker", "weathered-inscription");
        FocusedSearchRequest activeRequest = Request("seeker", "weathered-inscription");
        FocusedSearchActivitySnapshot activities = new(
            new[] { new FocusedSearchActivity(activeRequest) });

        bool result = FocusedSearchCompletionPolicy.CanCompleteSearch(request, activities);

        Assert.True(result);
        Assert.NotSame(request, activeRequest);
        Assert.Equal(request, activeRequest);
    }

    [Fact]
    public void RejectsDifferentTargetForSameSearcher()
    {
        FocusedSearchRequest request = Request("seeker", "weathered-inscription");
        FocusedSearchActivitySnapshot activities = Activities(
            ("seeker", "concealed-healing-herb"));

        bool result = FocusedSearchCompletionPolicy.CanCompleteSearch(request, activities);

        Assert.False(result);
    }

    [Fact]
    public void RejectsDifferentSearcherForSameTarget()
    {
        FocusedSearchRequest request = Request("seeker", "weathered-inscription");
        FocusedSearchActivitySnapshot activities = Activities(
            ("travelling-healer", "weathered-inscription"));

        bool result = FocusedSearchCompletionPolicy.CanCompleteSearch(request, activities);

        Assert.False(result);
    }

    [Fact]
    public void RejectsUnrelatedActivities()
    {
        FocusedSearchRequest request = Request("seeker", "weathered-inscription");
        FocusedSearchActivitySnapshot activities = Activities(
            ("travelling-healer", "concealed-healing-herb"),
            ("sage", "wagon-residue"));

        bool result = FocusedSearchCompletionPolicy.CanCompleteSearch(request, activities);

        Assert.False(result);
    }

    [Fact]
    public void RejectsNullInputs()
    {
        FocusedSearchRequest request = Request("seeker", "weathered-inscription");
        FocusedSearchActivitySnapshot activities = Activities();

        Assert.Throws<ArgumentNullException>(
            () => FocusedSearchCompletionPolicy.CanCompleteSearch(null!, activities));
        Assert.Throws<ArgumentNullException>(
            () => FocusedSearchCompletionPolicy.CanCompleteSearch(request, null!));
    }

    [Fact]
    public void RetainsInputsWithoutMutation()
    {
        CharacterId searcherCharacterId = new("seeker");
        ObjectId targetObjectId = new("weathered-inscription");
        FocusedSearchRequest request = new(searcherCharacterId, targetObjectId);
        FocusedSearchActivity first = Activity("travelling-healer", "concealed-healing-herb");
        FocusedSearchActivity second = new(request);
        FocusedSearchActivitySnapshot activities = new(new[] { first, second });

        _ = FocusedSearchCompletionPolicy.CanCompleteSearch(request, activities);

        Assert.Same(searcherCharacterId, request.SearcherCharacterId);
        Assert.Same(targetObjectId, request.TargetObjectId);
        Assert.Collection(
            activities.Entries,
            entry => Assert.Same(first, entry),
            entry => Assert.Same(second, entry));
    }

    [Fact]
    public void ExposesOnlyTheSinglePureEligibilityOperation()
    {
        Type policyType = typeof(FocusedSearchCompletionPolicy);
        System.Reflection.MethodInfo method = Assert.Single(
            policyType
                .GetMethods()
                .Where(candidate => candidate.DeclaringType == policyType));

        Assert.Equal("CanCompleteSearch", method.Name);
        Assert.Equal(typeof(bool), method.ReturnType);
        Assert.True(method.IsStatic);
        Assert.Collection(
            method.GetParameters(),
            parameter => Assert.Equal(typeof(FocusedSearchRequest), parameter.ParameterType),
            parameter => Assert.Equal(
                typeof(FocusedSearchActivitySnapshot),
                parameter.ParameterType));
        Assert.Empty(policyType.GetConstructors());
        Assert.Empty(policyType.GetProperties());
        Assert.Empty(policyType.GetEvents());
        Assert.True(policyType.IsAbstract);
        Assert.True(policyType.IsSealed);
    }

    private static FocusedSearchRequest Request(string characterId, string targetObjectId) =>
        new(new CharacterId(characterId), new ObjectId(targetObjectId));

    private static FocusedSearchActivity Activity(string characterId, string targetObjectId) =>
        new(Request(characterId, targetObjectId));

    private static FocusedSearchActivitySnapshot Activities(
        params (string CharacterId, string TargetObjectId)[] entries) =>
        new(entries.Select(entry => Activity(entry.CharacterId, entry.TargetObjectId)));
}
