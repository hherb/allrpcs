using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Tests.Dialogue;

public sealed class CharacterExaminationResultTests
{
    private static readonly string[] ExpectedPropertyNames =
        ["Occurrence", "Safety", "TargetObjectId"];

    [Theory]
    [MemberData(nameof(DefinedSafetyValues))]
    public void RetainsTheExactExaminationOccurrenceTargetAndSafety(
        CharacterExaminationSafety safety)
    {
        CharacterTurnWorldEventOccurrence occurrence = ExaminationOccurrence();
        ObjectId targetObjectId = occurrence.TranscriptEntry.PhysicalAction!.TargetObjectId!;

        CharacterExaminationResult result = new(occurrence, safety);

        Assert.Same(occurrence, result.Occurrence);
        Assert.Same(targetObjectId, result.TargetObjectId);
        Assert.Same(safety, result.Safety);
    }

    [Fact]
    public void RejectsSpeechCollectionAndMissingTargetOccurrences()
    {
        WorldEvent worldEvent = Event();

        Assert.Throws<ArgumentException>(() => new CharacterExaminationResult(
            new CharacterTurnWorldEventOccurrence(
                worldEvent,
                new CharacterTurnTranscriptEntry(
                    new DialogueUtterance(new CharacterId("healer"), "I examined it."))),
            CharacterExaminationSafety.SafeToCollect));
        Assert.Throws<ArgumentException>(() => new CharacterExaminationResult(
            new CharacterTurnWorldEventOccurrence(
                worldEvent,
                new CharacterTurnTranscriptEntry(
                    new CharacterId("healer"),
                    new CharacterIntention(
                        CharacterIntentionType.CollectObject,
                        new ObjectId("flowers")))),
            CharacterExaminationSafety.SafeToCollect));
        Assert.Throws<ArgumentException>(() => new CharacterExaminationResult(
            new CharacterTurnWorldEventOccurrence(
                worldEvent,
                new CharacterTurnTranscriptEntry(
                    new CharacterId("healer"),
                    new CharacterIntention(CharacterIntentionType.ExamineObject, null))),
            CharacterExaminationSafety.SafeToCollect));
    }

    [Fact]
    public void RejectsNullInputs()
    {
        CharacterTurnWorldEventOccurrence occurrence = ExaminationOccurrence();

        Assert.Throws<ArgumentNullException>(() =>
            new CharacterExaminationResult(null!, CharacterExaminationSafety.SafeToCollect));
        Assert.Throws<ArgumentNullException>(() =>
            new CharacterExaminationResult(occurrence, null!));
    }

    [Fact]
    public void ExposesOnlyTheImmutableResultSurface()
    {
        Type resultType = typeof(CharacterExaminationResult);
        string[] propertyNames = resultType.GetProperties()
            .Select(property => property.Name)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(ExpectedPropertyNames, propertyNames);
        Assert.All(resultType.GetProperties(), property => Assert.False(property.CanWrite));
        Assert.Single(resultType.GetConstructors());
    }

    public static TheoryData<CharacterExaminationSafety> DefinedSafetyValues => new()
    {
        CharacterExaminationSafety.SafeToCollect,
        CharacterExaminationSafety.UnsafeToCollect,
    };

    internal static CharacterTurnWorldEventOccurrence ExaminationOccurrence()
    {
        CharacterIntention examination = new(
            CharacterIntentionType.ExamineObject,
            new ObjectId("flowers"));
        return new CharacterTurnWorldEventOccurrence(
            Event(),
            new CharacterTurnTranscriptEntry(new CharacterId("healer"), examination));
    }

    private static WorldEvent Event() =>
        new(new WorldEventId("healer-examined-flowers"), WorldEventTimeEffect.Advance);
}
