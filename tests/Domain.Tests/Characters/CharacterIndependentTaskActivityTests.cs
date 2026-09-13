using Allrpcs.Domain.Characters;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterIndependentTaskActivityTests
{
    [Theory]
    [InlineData("assigned")]
    [InlineData("active")]
    [InlineData("completed")]
    [InlineData("cancelled")]
    public void RetainsExactAssignmentAndDefinedState(string stateValue)
    {
        CharacterIndependentTaskAssignment assignment =
            CharacterIndependentTaskAssignmentTests.Assignment("task", "healer", "moonwort");
        CharacterIndependentTaskActivityState state =
            CharacterIndependentTaskActivityState.Parse(stateValue);

        CharacterIndependentTaskActivity activity = new(assignment, state);

        Assert.Same(assignment, activity.Assignment);
        Assert.Same(state, activity.State);
    }

    [Fact]
    public void RejectsNullsAndExposesOnlyImmutableLifecycleState()
    {
        CharacterIndependentTaskAssignment assignment =
            CharacterIndependentTaskAssignmentTests.Assignment("task", "healer", "moonwort");

        Assert.Throws<ArgumentNullException>(() => new CharacterIndependentTaskActivity(
            null!,
            CharacterIndependentTaskActivityState.Assigned));
        Assert.Throws<ArgumentNullException>(() => new CharacterIndependentTaskActivity(
            assignment,
            null!));

        Type activityType = typeof(CharacterIndependentTaskActivity);
        (string Name, Type Type)[] expectedProperties =
        [
            ("Assignment", typeof(CharacterIndependentTaskAssignment)),
            ("State", typeof(CharacterIndependentTaskActivityState)),
        ];
        System.Reflection.PropertyInfo[] actualProperties = activityType
            .GetProperties()
            .OrderBy(property => property.Name, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(
            expectedProperties,
            actualProperties.Select(property => (property.Name, property.PropertyType)).ToArray());
        Assert.All(actualProperties, property => Assert.False(property.CanWrite));
        Assert.Empty(activityType.GetEvents());
        Assert.True(activityType.IsSealed);
    }
}
