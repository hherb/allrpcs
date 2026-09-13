using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterIndependentTaskWorldEventOccurrenceTests
{
    [Fact]
    public void RetainsExactActivityAndWorldEventOccurrence()
    {
        CharacterIndependentTaskActivity activity = ActiveActivity();
        CharacterTurnWorldEventOccurrence occurrence = Occurrence(activity);

        CharacterIndependentTaskWorldEventOccurrence taskOccurrence = new(activity, occurrence);

        Assert.Same(activity, taskOccurrence.Activity);
        Assert.Same(occurrence, taskOccurrence.Occurrence);
    }

    [Fact]
    public void RejectsNullsAndExposesOnlyTheImmutableCandidateSurface()
    {
        CharacterIndependentTaskActivity activity = ActiveActivity();
        CharacterTurnWorldEventOccurrence occurrence = Occurrence(activity);

        Assert.Throws<ArgumentNullException>(() =>
            new CharacterIndependentTaskWorldEventOccurrence(null!, occurrence));
        Assert.Throws<ArgumentNullException>(() =>
            new CharacterIndependentTaskWorldEventOccurrence(activity, null!));

        Type candidateType = typeof(CharacterIndependentTaskWorldEventOccurrence);
        (string Name, Type Type)[] expectedProperties =
        [
            ("Activity", typeof(CharacterIndependentTaskActivity)),
            ("Occurrence", typeof(CharacterTurnWorldEventOccurrence)),
        ];
        System.Reflection.PropertyInfo[] properties = candidateType
            .GetProperties()
            .OrderBy(property => property.Name, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(
            expectedProperties,
            properties.Select(property => (property.Name, property.PropertyType)).ToArray());
        Assert.All(properties, property => Assert.False(property.CanWrite));
        Assert.Single(candidateType.GetConstructors());
        Assert.Empty(candidateType.GetEvents());
        Assert.True(candidateType.IsSealed);
    }

    internal static CharacterIndependentTaskActivity ActiveActivity(
        string taskId = "examine-moonwort")
    {
        CharacterIndependentTaskAssignment assignment =
            CharacterIndependentTaskAssignmentTests.Assignment(taskId, "healer", "moonwort");
        return new CharacterIndependentTaskActivity(
            assignment,
            CharacterIndependentTaskActivityState.Active);
    }

    internal static CharacterTurnWorldEventOccurrence Occurrence(
        CharacterIndependentTaskActivity activity,
        WorldEvent? worldEvent = null,
        CharacterId? characterId = null,
        CharacterIntention? intention = null) =>
        new(
            worldEvent ?? Event(),
            new CharacterTurnTranscriptEntry(
                characterId ?? activity.Assignment.CharacterId,
                intention ?? activity.Assignment.Intention));

    internal static WorldEvent Event() =>
        new(new WorldEventId("healer-examined-moonwort"), WorldEventTimeEffect.Advance);
}
