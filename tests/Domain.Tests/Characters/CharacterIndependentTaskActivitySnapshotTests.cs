using Allrpcs.Domain.Characters;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterIndependentTaskActivitySnapshotTests
{
    [Fact]
    public void PreservesOrderExactInstancesAndCopiesCollection()
    {
        CharacterIndependentTaskActivity first = Activity("first", "healer");
        CharacterIndependentTaskActivity second = Activity("second", "sage");
        CharacterIndependentTaskActivity[] source = [first, second];

        CharacterIndependentTaskActivitySnapshot snapshot = new(source);
        source[0] = second;

        Assert.Collection(
            snapshot.Activities,
            activity => Assert.Same(first, activity),
            activity => Assert.Same(second, activity));
        Assert.Throws<NotSupportedException>(() =>
            ((IList<CharacterIndependentTaskActivity>)snapshot.Activities).Add(first));
    }

    [Fact]
    public void AcceptsEmptyAndExactCountBound()
    {
        CharacterIndependentTaskActivity[] maximum = Enumerable
            .Range(0, CharacterIndependentTaskActivitySnapshot.MaximumActivityCount)
            .Select(index => Activity($"task-{index}", $"character-{index}"))
            .ToArray();

        Assert.Empty(new CharacterIndependentTaskActivitySnapshot([]).Activities);
        Assert.Equal(
            CharacterIndependentTaskActivitySnapshot.MaximumActivityCount,
            new CharacterIndependentTaskActivitySnapshot(maximum).Activities.Count);
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new CharacterIndependentTaskActivitySnapshot(
                maximum.Append(Activity("extra-task", "extra-character"))));
    }

    [Fact]
    public void RejectsValueEqualDuplicateTaskIdsNullEntriesAndNullCollection()
    {
        CharacterIndependentTaskActivity first = Activity("task", "healer");
        CharacterIndependentTaskActivity duplicate = Activity("task", "sage");

        Assert.Throws<ArgumentException>(() =>
            new CharacterIndependentTaskActivitySnapshot([first, duplicate]));
        Assert.Throws<ArgumentException>(() =>
            new CharacterIndependentTaskActivitySnapshot([null!]));
        Assert.Throws<ArgumentNullException>(() =>
            new CharacterIndependentTaskActivitySnapshot(null!));
    }

    [Fact]
    public void RejectsMoreThanOneActiveTaskForValueEqualCharacter()
    {
        CharacterIndependentTaskActivity first =
            CharacterIndependentTaskSelectionPolicyTests.Activity("first", "healer", "active");
        CharacterIndependentTaskActivity second =
            CharacterIndependentTaskSelectionPolicyTests.Activity("second", "healer", "active");

        Assert.Throws<ArgumentException>(() =>
            new CharacterIndependentTaskActivitySnapshot([first, second]));
    }

    [Fact]
    public void ExposesOnlyTheImmutableSnapshotSurface()
    {
        Type snapshotType = typeof(CharacterIndependentTaskActivitySnapshot);
        System.Reflection.PropertyInfo property = Assert.Single(snapshotType.GetProperties());
        System.Reflection.ConstructorInfo constructor = Assert.Single(snapshotType.GetConstructors());

        Assert.Equal("Activities", property.Name);
        Assert.Equal(typeof(IReadOnlyList<CharacterIndependentTaskActivity>), property.PropertyType);
        Assert.False(property.CanWrite);
        Assert.Collection(
            constructor.GetParameters(),
            parameter => Assert.Equal(
                typeof(IEnumerable<CharacterIndependentTaskActivity>),
                parameter.ParameterType));
        Assert.Empty(snapshotType.GetEvents());
        Assert.Empty(snapshotType.GetMethods().Where(method =>
            method.DeclaringType == snapshotType && !method.IsSpecialName));
        Assert.True(snapshotType.IsSealed);
    }

    internal static CharacterIndependentTaskActivity Activity(string taskId, string characterId) =>
        new(
            CharacterIndependentTaskAssignmentTests.Assignment(
                taskId,
                characterId,
                $"{taskId}-object"),
            CharacterIndependentTaskActivityState.Assigned);
}
