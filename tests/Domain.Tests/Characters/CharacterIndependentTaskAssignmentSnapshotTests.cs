using Allrpcs.Domain.Characters;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterIndependentTaskAssignmentSnapshotTests
{
    [Fact]
    public void PreservesOrderExactInstancesAndCopiesCollection()
    {
        CharacterIndependentTaskAssignment first = Assignment("first", "healer");
        CharacterIndependentTaskAssignment second = Assignment("second", "sage");
        CharacterIndependentTaskAssignment[] source = [first, second];

        CharacterIndependentTaskAssignmentSnapshot snapshot = new(source);
        source[0] = second;

        Assert.Collection(
            snapshot.Assignments,
            assignment => Assert.Same(first, assignment),
            assignment => Assert.Same(second, assignment));
        Assert.Throws<NotSupportedException>(() =>
            ((IList<CharacterIndependentTaskAssignment>)snapshot.Assignments).Add(first));
    }

    [Fact]
    public void AcceptsEmptyAndExactCountBound()
    {
        CharacterIndependentTaskAssignment[] maximum = Enumerable
            .Range(0, CharacterIndependentTaskAssignmentSnapshot.MaximumAssignmentCount)
            .Select(index => Assignment($"task-{index}", $"character-{index}"))
            .ToArray();

        Assert.Empty(new CharacterIndependentTaskAssignmentSnapshot([]).Assignments);
        Assert.Equal(
            CharacterIndependentTaskAssignmentSnapshot.MaximumAssignmentCount,
            new CharacterIndependentTaskAssignmentSnapshot(maximum).Assignments.Count);
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new CharacterIndependentTaskAssignmentSnapshot(
                maximum.Append(Assignment("extra-task", "extra-character"))));
    }

    [Fact]
    public void RejectsValueEqualDuplicateTaskIdsNullEntriesAndNullCollection()
    {
        CharacterIndependentTaskAssignment first = Assignment("task", "healer");
        CharacterIndependentTaskAssignment duplicate = Assignment("task", "sage");

        Assert.Throws<ArgumentException>(() =>
            new CharacterIndependentTaskAssignmentSnapshot([first, duplicate]));
        Assert.Throws<ArgumentException>(() =>
            new CharacterIndependentTaskAssignmentSnapshot([null!]));
        Assert.Throws<ArgumentNullException>(() =>
            new CharacterIndependentTaskAssignmentSnapshot(null!));
    }

    [Fact]
    public void ExposesOnlyTheImmutableSnapshotSurface()
    {
        Type snapshotType = typeof(CharacterIndependentTaskAssignmentSnapshot);
        System.Reflection.PropertyInfo property = Assert.Single(snapshotType.GetProperties());
        System.Reflection.ConstructorInfo constructor = Assert.Single(snapshotType.GetConstructors());
        string[] publicMethods = snapshotType
            .GetMethods()
            .Where(method => method.DeclaringType == snapshotType && !method.IsSpecialName)
            .Select(method => method.Name)
            .ToArray();

        Assert.Equal("Assignments", property.Name);
        Assert.Equal(typeof(IReadOnlyList<CharacterIndependentTaskAssignment>), property.PropertyType);
        Assert.False(property.CanWrite);
        Assert.Collection(
            constructor.GetParameters(),
            parameter => Assert.Equal(
                typeof(IEnumerable<CharacterIndependentTaskAssignment>),
                parameter.ParameterType));
        Assert.Empty(snapshotType.GetEvents());
        Assert.Empty(publicMethods);
        Assert.True(snapshotType.IsSealed);
    }

    private static CharacterIndependentTaskAssignment Assignment(string taskId, string characterId) =>
        CharacterIndependentTaskAssignmentTests.Assignment(taskId, characterId, $"{taskId}-object");
}
