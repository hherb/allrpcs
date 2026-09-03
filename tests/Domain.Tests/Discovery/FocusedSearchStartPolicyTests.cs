using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Discovery;

public sealed class FocusedSearchStartPolicyTests
{
    [Fact]
    public void AllowsSearchWhenSnapshotIsEmpty()
    {
        FocusedSearchRequest request = Request("seeker", "weathered-inscription");
        FocusedSearchActivitySnapshot activities = Activities();

        bool result = FocusedSearchStartPolicy.CanStartSearch(request, activities);

        Assert.True(result);
    }

    [Fact]
    public void AllowsSearchWhenExistingActivitiesHaveDifferentSearchers()
    {
        FocusedSearchRequest request = Request("seeker", "weathered-inscription");
        FocusedSearchActivitySnapshot activities = Activities(
            ("travelling-healer", "concealed-healing-herb"),
            ("sage", "wagon-residue"));

        bool result = FocusedSearchStartPolicy.CanStartSearch(request, activities);

        Assert.True(result);
    }

    [Theory]
    [InlineData("weathered-inscription")]
    [InlineData("concealed-healing-herb")]
    public void RejectsSearchWhenSearcherAlreadyHasAnActivity(string existingTargetObjectId)
    {
        FocusedSearchRequest request = Request("seeker", "weathered-inscription");
        FocusedSearchActivitySnapshot activities = Activities(
            ("seeker", existingTargetObjectId));

        bool result = FocusedSearchStartPolicy.CanStartSearch(request, activities);

        Assert.False(result);
    }

    [Fact]
    public void MatchesSearcherIdentifiersByValue()
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

        bool result = FocusedSearchStartPolicy.CanStartSearch(request, activities);

        Assert.False(result);
        Assert.NotSame(requestSearcherCharacterId, activitySearcherCharacterId);
        Assert.Equal(requestSearcherCharacterId, activitySearcherCharacterId);
    }

    [Fact]
    public void AllowsDifferentSearcherToSearchTheSameTarget()
    {
        FocusedSearchRequest request = Request("seeker", "weathered-inscription");
        FocusedSearchActivitySnapshot activities = Activities(
            ("travelling-healer", "weathered-inscription"));

        bool result = FocusedSearchStartPolicy.CanStartSearch(request, activities);

        Assert.True(result);
    }

    [Fact]
    public void RejectsNullInputs()
    {
        FocusedSearchRequest request = Request("seeker", "weathered-inscription");
        FocusedSearchActivitySnapshot activities = Activities();

        Assert.Throws<ArgumentNullException>(
            () => FocusedSearchStartPolicy.CanStartSearch(null!, activities));
        Assert.Throws<ArgumentNullException>(
            () => FocusedSearchStartPolicy.CanStartSearch(request, null!));
    }

    [Fact]
    public void RetainsInputsWithoutMutation()
    {
        CharacterId searcherCharacterId = new("seeker");
        ObjectId targetObjectId = new("weathered-inscription");
        FocusedSearchRequest request = new(searcherCharacterId, targetObjectId);
        FocusedSearchActivity first = Activity("travelling-healer", "concealed-healing-herb");
        FocusedSearchActivity second = Activity("sage", "wagon-residue");
        FocusedSearchActivitySnapshot activities = new(new[] { first, second });

        _ = FocusedSearchStartPolicy.CanStartSearch(request, activities);

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
        Type policyType = typeof(FocusedSearchStartPolicy);
        System.Reflection.MethodInfo method = Assert.Single(
            policyType
                .GetMethods()
                .Where(candidate => candidate.DeclaringType == policyType));

        Assert.Equal("CanStartSearch", method.Name);
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
