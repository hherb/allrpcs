using Allrpcs.Domain.Characters;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterIndependentTaskDepartureResultTests
{
    [Fact]
    public void RetainsExactActiveTaskAndResultingSnapshots()
    {
        CharacterIndependentTaskActivity activity =
            CharacterIndependentTaskSelectionPolicyTests.Activity("task", "healer", "active");
        CharacterIndependentTaskActivitySnapshot activities = new([activity]);
        CharacterPartySnapshot party = new([]);

        CharacterIndependentTaskDepartureResult result = new(activity, activities, party);

        Assert.Same(activity, result.ActiveActivity);
        Assert.Same(activities, result.Activities);
        Assert.Same(party, result.Party);
    }

    [Fact]
    public void RejectsNullsAndExposesOnlyTheImmutableResultSurface()
    {
        CharacterIndependentTaskActivity activity =
            CharacterIndependentTaskSelectionPolicyTests.Activity("task", "healer", "active");
        CharacterIndependentTaskActivitySnapshot activities = new([activity]);
        CharacterPartySnapshot party = new([]);

        Assert.Throws<ArgumentNullException>(() =>
            new CharacterIndependentTaskDepartureResult(null!, activities, party));
        Assert.Throws<ArgumentNullException>(() =>
            new CharacterIndependentTaskDepartureResult(activity, null!, party));
        Assert.Throws<ArgumentNullException>(() =>
            new CharacterIndependentTaskDepartureResult(activity, activities, null!));

        Type resultType = typeof(CharacterIndependentTaskDepartureResult);
        (string Name, Type Type)[] expectedProperties =
        [
            ("ActiveActivity", typeof(CharacterIndependentTaskActivity)),
            ("Activities", typeof(CharacterIndependentTaskActivitySnapshot)),
            ("Party", typeof(CharacterPartySnapshot)),
        ];
        System.Reflection.PropertyInfo[] properties = resultType
            .GetProperties()
            .OrderBy(property => property.Name, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(
            expectedProperties,
            properties.Select(property => (property.Name, property.PropertyType)).ToArray());
        Assert.All(properties, property => Assert.False(property.CanWrite));
        Assert.Single(resultType.GetConstructors());
        Assert.Empty(resultType.GetEvents());
        Assert.True(resultType.IsSealed);
    }
}
