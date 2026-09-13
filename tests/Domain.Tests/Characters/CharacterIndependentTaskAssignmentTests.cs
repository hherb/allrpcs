using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterIndependentTaskAssignmentTests
{
    [Theory]
    [InlineData("examine")]
    [InlineData("collect")]
    public void RetainsExactSupportedAssignmentValues(string intentionKind)
    {
        CharacterIndependentTaskId taskId = new("inspect-moonwort");
        CharacterId characterId = new("healer");
        ObjectId targetObjectId = new("moonwort-patch");
        CharacterIntention intention = new(
            intentionKind == "examine"
                ? CharacterIntentionType.ExamineObject
                : CharacterIntentionType.CollectObject,
            targetObjectId);

        CharacterIndependentTaskAssignment assignment = new(taskId, characterId, intention);

        Assert.Same(taskId, assignment.TaskId);
        Assert.Same(characterId, assignment.CharacterId);
        Assert.Same(intention, assignment.Intention);
    }

    [Fact]
    public void RejectsWaitAndNullValues()
    {
        CharacterIndependentTaskId taskId = new("wait-for-player");
        CharacterId characterId = new("healer");
        CharacterIntention intention = new(CharacterIntentionType.ExamineObject, null);

        Assert.Throws<ArgumentException>(() => new CharacterIndependentTaskAssignment(
            taskId,
            characterId,
            new CharacterIntention(CharacterIntentionType.Wait, null)));
        Assert.Throws<ArgumentNullException>(() => new CharacterIndependentTaskAssignment(
            null!,
            characterId,
            intention));
        Assert.Throws<ArgumentNullException>(() => new CharacterIndependentTaskAssignment(
            taskId,
            null!,
            intention));
        Assert.Throws<ArgumentNullException>(() => new CharacterIndependentTaskAssignment(
            taskId,
            characterId,
            null!));
    }

    [Fact]
    public void EqualityUsesAssignmentValues()
    {
        CharacterIndependentTaskAssignment first = Assignment("task", "healer", "moonwort");
        CharacterIndependentTaskAssignment same = Assignment("task", "healer", "moonwort");
        CharacterIndependentTaskAssignment differentTask = Assignment(
            "other-task",
            "healer",
            "moonwort");

        Assert.Equal(first, same);
        Assert.True(first == same);
        Assert.NotEqual(first, differentTask);
    }

    [Fact]
    public void ExposesOnlyTheImmutableAssignmentSurface()
    {
        Type assignmentType = typeof(CharacterIndependentTaskAssignment);
        (string Name, Type Type)[] expectedProperties =
        [
            ("CharacterId", typeof(CharacterId)),
            ("Intention", typeof(CharacterIntention)),
            ("TaskId", typeof(CharacterIndependentTaskId)),
        ];
        System.Reflection.PropertyInfo[] actualProperties = assignmentType
            .GetProperties()
            .OrderBy(property => property.Name, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(
            expectedProperties,
            actualProperties.Select(property => (property.Name, property.PropertyType)).ToArray());
        Assert.All(actualProperties, property => Assert.False(property.CanWrite));
        Assert.Empty(assignmentType.GetEvents());
        Assert.True(assignmentType.IsSealed);
    }

    internal static CharacterIndependentTaskAssignment Assignment(
        string taskId,
        string characterId,
        string objectId) =>
        new(
            new CharacterIndependentTaskId(taskId),
            new CharacterId(characterId),
            new CharacterIntention(
                CharacterIntentionType.ExamineObject,
                new ObjectId(objectId)));
}
