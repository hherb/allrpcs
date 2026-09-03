using Allrpcs.Application.Discovery;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Application.Tests.Discovery;

public sealed class FocusedSearchActivityIndicatorPolicyTests
{
    [Fact]
    public void HidesIndicatorWhenThereAreNoActivities()
    {
        CharacterId searcherCharacterId = new("seeker");
        FocusedSearchActivitySnapshot activities = Activities();

        bool result =
            FocusedSearchActivityIndicatorPolicy.ShouldShow(searcherCharacterId, activities);

        Assert.False(result);
    }

    [Fact]
    public void HidesIndicatorWhenAllActivitiesAreUnrelated()
    {
        CharacterId searcherCharacterId = new("seeker");
        FocusedSearchActivitySnapshot activities = Activities(
            ("travelling-healer", "concealed-healing-herb"),
            ("sage", "weathered-inscription"));

        bool result =
            FocusedSearchActivityIndicatorPolicy.ShouldShow(searcherCharacterId, activities);

        Assert.False(result);
    }

    [Fact]
    public void ShowsIndicatorForOneValueEqualCharacterIdentifier()
    {
        CharacterId searcherCharacterId = new("seeker");
        CharacterId activityCharacterId = new("seeker");
        FocusedSearchActivity matching = new(
            new FocusedSearchRequest(
                activityCharacterId,
                new ObjectId("weathered-inscription")));
        FocusedSearchActivitySnapshot activities = new(new[] { matching });

        bool result =
            FocusedSearchActivityIndicatorPolicy.ShouldShow(searcherCharacterId, activities);

        Assert.True(result);
        Assert.NotSame(searcherCharacterId, activityCharacterId);
        Assert.Equal(searcherCharacterId, activityCharacterId);
    }

    [Fact]
    public void ShowsIndicatorWhenProjectionContainsMultipleDistinctRequests()
    {
        CharacterId searcherCharacterId = new("seeker");
        FocusedSearchActivitySnapshot activities = Activities(
            ("seeker", "weathered-inscription"),
            ("sage", "wagon-residue"),
            ("seeker", "concealed-healing-herb"));

        bool result =
            FocusedSearchActivityIndicatorPolicy.ShouldShow(searcherCharacterId, activities);

        Assert.True(result);
        Assert.Equal(
            2,
            FocusedSearchActivityProjection
                .ForCharacter(searcherCharacterId, activities)
                .Entries.Count);
    }

    [Fact]
    public void RejectsNullInputs()
    {
        CharacterId searcherCharacterId = new("seeker");
        FocusedSearchActivitySnapshot activities = Activities();

        Assert.Throws<ArgumentNullException>(
            () => FocusedSearchActivityIndicatorPolicy.ShouldShow(null!, activities));
        Assert.Throws<ArgumentNullException>(
            () => FocusedSearchActivityIndicatorPolicy.ShouldShow(searcherCharacterId, null!));
    }

    [Fact]
    public void DoesNotMutateInputs()
    {
        CharacterId searcherCharacterId = new("seeker");
        FocusedSearchActivity firstMatch = Activity("seeker", "weathered-inscription");
        FocusedSearchActivity unrelated = Activity("sage", "wagon-residue");
        FocusedSearchActivity secondMatch = Activity("seeker", "concealed-healing-herb");
        FocusedSearchActivitySnapshot activities =
            new(new[] { firstMatch, unrelated, secondMatch });

        bool result =
            FocusedSearchActivityIndicatorPolicy.ShouldShow(searcherCharacterId, activities);

        Assert.True(result);
        Assert.Equal("seeker", searcherCharacterId.Value);
        Assert.Collection(
            activities.Entries,
            entry => Assert.Same(firstMatch, entry),
            entry => Assert.Same(unrelated, entry),
            entry => Assert.Same(secondMatch, entry));
    }

    [Fact]
    public void ExposesOnlyTheSinglePureIndicatorOperation()
    {
        Type policyType = typeof(FocusedSearchActivityIndicatorPolicy);
        System.Reflection.MethodInfo method = Assert.Single(
            policyType
                .GetMethods()
                .Where(candidate => candidate.DeclaringType == policyType));

        Assert.Equal("ShouldShow", method.Name);
        Assert.Equal(typeof(bool), method.ReturnType);
        Assert.True(method.IsStatic);
        Assert.Collection(
            method.GetParameters(),
            parameter => Assert.Equal(typeof(CharacterId), parameter.ParameterType),
            parameter => Assert.Equal(
                typeof(FocusedSearchActivitySnapshot),
                parameter.ParameterType));
        Assert.Empty(policyType.GetConstructors());
        Assert.Empty(policyType.GetProperties());
        Assert.Empty(policyType.GetEvents());
        Assert.True(policyType.IsAbstract);
        Assert.True(policyType.IsSealed);
    }

    private static FocusedSearchActivity Activity(string characterId, string targetObjectId) =>
        new(
            new FocusedSearchRequest(
                new CharacterId(characterId),
                new ObjectId(targetObjectId)));

    private static FocusedSearchActivitySnapshot Activities(
        params (string CharacterId, string TargetObjectId)[] entries) =>
        new(entries.Select(entry => Activity(entry.CharacterId, entry.TargetObjectId)));
}
