using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterIndependentTaskWorldEventOccurrenceResultTests
{
    [Fact]
    public void RetainsExactCandidateActivitiesAndWorldEventResult()
    {
        CharacterIndependentTaskActivity activity =
            CharacterIndependentTaskWorldEventOccurrenceTests.ActiveActivity();
        CharacterIndependentTaskWorldEventOccurrence candidate = new(
            activity,
            CharacterIndependentTaskWorldEventOccurrenceTests.Occurrence(activity));
        CharacterIndependentTaskActivitySnapshot activities = new([activity]);
        CharacterTurnWorldEventOccurrenceResult worldEventResult = new(
            candidate.Occurrence,
            new WorldEventOccurrenceResult(
                new WorldEventLogSnapshot([]),
                new WorldTimeSnapshot(null)),
            new CharacterTurnTranscriptSnapshot([]));

        CharacterIndependentTaskWorldEventOccurrenceResult result = new(
            candidate,
            activities,
            worldEventResult);

        Assert.Same(candidate, result.Candidate);
        Assert.Same(activities, result.Activities);
        Assert.Same(worldEventResult, result.WorldEventResult);
    }

    [Fact]
    public void RejectsNullsAndExposesOnlyTheImmutableResultSurface()
    {
        CharacterIndependentTaskActivity activity =
            CharacterIndependentTaskWorldEventOccurrenceTests.ActiveActivity();
        CharacterIndependentTaskWorldEventOccurrence candidate = new(
            activity,
            CharacterIndependentTaskWorldEventOccurrenceTests.Occurrence(activity));
        CharacterIndependentTaskActivitySnapshot activities = new([activity]);
        CharacterTurnWorldEventOccurrenceResult worldEventResult = new(
            candidate.Occurrence,
            new WorldEventOccurrenceResult(
                new WorldEventLogSnapshot([]),
                new WorldTimeSnapshot(null)),
            new CharacterTurnTranscriptSnapshot([]));

        Assert.Throws<ArgumentNullException>(() =>
            new CharacterIndependentTaskWorldEventOccurrenceResult(
                null!, activities, worldEventResult));
        Assert.Throws<ArgumentNullException>(() =>
            new CharacterIndependentTaskWorldEventOccurrenceResult(
                candidate, null!, worldEventResult));
        Assert.Throws<ArgumentNullException>(() =>
            new CharacterIndependentTaskWorldEventOccurrenceResult(
                candidate, activities, null!));

        Type resultType = typeof(CharacterIndependentTaskWorldEventOccurrenceResult);
        (string Name, Type Type)[] expectedProperties =
        [
            ("Activities", typeof(CharacterIndependentTaskActivitySnapshot)),
            ("Candidate", typeof(CharacterIndependentTaskWorldEventOccurrence)),
            ("WorldEventResult", typeof(CharacterTurnWorldEventOccurrenceResult)),
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
