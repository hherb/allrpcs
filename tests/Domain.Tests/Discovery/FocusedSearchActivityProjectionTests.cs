using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Discovery;

public sealed class FocusedSearchActivityProjectionTests
{
    [Fact]
    public void ReturnsEmptySnapshotWhenThereAreNoActivities()
    {
        CharacterId searcherCharacterId = new("seeker");
        FocusedSearchActivitySnapshot activities = Activities();

        FocusedSearchActivitySnapshot result =
            FocusedSearchActivityProjection.ForCharacter(searcherCharacterId, activities);

        Assert.Empty(result.Entries);
        Assert.NotSame(activities, result);
    }

    [Fact]
    public void ReturnsEmptySnapshotWhenAllActivitiesAreUnrelated()
    {
        CharacterId searcherCharacterId = new("seeker");
        FocusedSearchActivity first = Activity("travelling-healer", "concealed-healing-herb");
        FocusedSearchActivity second = Activity("sage", "weathered-inscription");
        FocusedSearchActivitySnapshot activities = new(new[] { first, second });

        FocusedSearchActivitySnapshot result =
            FocusedSearchActivityProjection.ForCharacter(searcherCharacterId, activities);

        Assert.Empty(result.Entries);
    }

    [Fact]
    public void ReturnsTheExactActivityForAValueEqualCharacterIdentifier()
    {
        CharacterId searcherCharacterId = new("seeker");
        CharacterId activityCharacterId = new("seeker");
        FocusedSearchActivity matching = new(
            new FocusedSearchRequest(
                activityCharacterId,
                new ObjectId("weathered-inscription")));
        FocusedSearchActivitySnapshot activities = new(new[] { matching });

        FocusedSearchActivitySnapshot result =
            FocusedSearchActivityProjection.ForCharacter(searcherCharacterId, activities);

        Assert.NotSame(searcherCharacterId, activityCharacterId);
        Assert.Equal(searcherCharacterId, activityCharacterId);
        Assert.Collection(result.Entries, entry => Assert.Same(matching, entry));
    }

    [Fact]
    public void ReturnsAllMatchesInStableOrderAndIgnoresUnrelatedActivities()
    {
        CharacterId searcherCharacterId = new("seeker");
        FocusedSearchActivity firstMatch = Activity("seeker", "weathered-inscription");
        FocusedSearchActivity unrelated = Activity("sage", "wagon-residue");
        FocusedSearchActivity secondMatch = Activity("seeker", "concealed-healing-herb");
        FocusedSearchActivitySnapshot activities =
            new(new[] { firstMatch, unrelated, secondMatch });

        FocusedSearchActivitySnapshot result =
            FocusedSearchActivityProjection.ForCharacter(searcherCharacterId, activities);

        Assert.Collection(
            result.Entries,
            entry => Assert.Same(firstMatch, entry),
            entry => Assert.Same(secondMatch, entry));
    }

    [Fact]
    public void RejectsNullInputs()
    {
        CharacterId searcherCharacterId = new("seeker");
        FocusedSearchActivitySnapshot activities = Activities();

        Assert.Throws<ArgumentNullException>(
            () => FocusedSearchActivityProjection.ForCharacter(null!, activities));
        Assert.Throws<ArgumentNullException>(
            () => FocusedSearchActivityProjection.ForCharacter(searcherCharacterId, null!));
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

        FocusedSearchActivitySnapshot result =
            FocusedSearchActivityProjection.ForCharacter(searcherCharacterId, activities);

        Assert.Equal("seeker", searcherCharacterId.Value);
        Assert.Collection(
            activities.Entries,
            entry => Assert.Same(firstMatch, entry),
            entry => Assert.Same(unrelated, entry),
            entry => Assert.Same(secondMatch, entry));
        Assert.NotSame(activities, result);
    }

    [Fact]
    public void ExposesOnlyTheSinglePureProjectionOperation()
    {
        Type projectionType = typeof(FocusedSearchActivityProjection);
        System.Reflection.MethodInfo method = Assert.Single(
            projectionType
                .GetMethods()
                .Where(candidate => candidate.DeclaringType == projectionType));

        Assert.Equal("ForCharacter", method.Name);
        Assert.Equal(typeof(FocusedSearchActivitySnapshot), method.ReturnType);
        Assert.True(method.IsStatic);
        Assert.Collection(
            method.GetParameters(),
            parameter => Assert.Equal(typeof(CharacterId), parameter.ParameterType),
            parameter => Assert.Equal(
                typeof(FocusedSearchActivitySnapshot),
                parameter.ParameterType));
        Assert.Empty(projectionType.GetConstructors());
        Assert.Empty(projectionType.GetProperties());
        Assert.Empty(projectionType.GetEvents());
        Assert.True(projectionType.IsAbstract);
        Assert.True(projectionType.IsSealed);
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
