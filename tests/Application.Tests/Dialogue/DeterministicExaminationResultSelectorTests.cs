using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Application.Tests.Dialogue;

public sealed class DeterministicExaminationResultSelectorTests
{
    [Fact]
    public void SelectsAFreshSafeResultForTheExactObservedExaminationAndTarget()
    {
        ObjectId targetObjectId = new("flowers");
        CharacterTurnWorldEventOccurrence occurrence = Occurrence(
            CharacterIntentionType.ExamineObject,
            targetObjectId);

        CharacterExaminationResult? first =
            DeterministicExaminationResultSelector.Select(occurrence, targetObjectId);
        CharacterExaminationResult? second =
            DeterministicExaminationResultSelector.Select(occurrence, targetObjectId);

        Assert.NotNull(first);
        Assert.NotNull(second);
        Assert.NotSame(first, second);
        Assert.Same(occurrence, first.Occurrence);
        Assert.Same(targetObjectId, first.TargetObjectId);
        Assert.Same(CharacterExaminationSafety.SafeToCollect, first.Safety);
        Assert.Same(occurrence.WorldEvent, first.Occurrence.WorldEvent);
        Assert.Same(occurrence.TranscriptEntry, first.Occurrence.TranscriptEntry);
    }

    [Fact]
    public void AcceptsADistinctValueEqualTargetAndRetainsTheObservedIdentity()
    {
        ObjectId observedTargetObjectId = new("flowers");
        ObjectId suppliedTargetObjectId = new("flowers");
        CharacterTurnWorldEventOccurrence occurrence = Occurrence(
            CharacterIntentionType.ExamineObject,
            observedTargetObjectId);
        WorldEvent originalWorldEvent = occurrence.WorldEvent;
        CharacterTurnTranscriptEntry originalTranscriptEntry = occurrence.TranscriptEntry;
        CharacterIntention originalAction = originalTranscriptEntry.PhysicalAction!;

        CharacterExaminationResult? result =
            DeterministicExaminationResultSelector.Select(occurrence, suppliedTargetObjectId);

        Assert.NotNull(result);
        Assert.Equal(observedTargetObjectId, suppliedTargetObjectId);
        Assert.NotSame(observedTargetObjectId, suppliedTargetObjectId);
        Assert.Same(occurrence, result.Occurrence);
        Assert.Same(observedTargetObjectId, result.TargetObjectId);
        Assert.Same(originalWorldEvent, occurrence.WorldEvent);
        Assert.Same(originalTranscriptEntry, occurrence.TranscriptEntry);
        Assert.Same(originalAction, occurrence.TranscriptEntry.PhysicalAction);
        Assert.Same(observedTargetObjectId, originalAction.TargetObjectId);
    }

    [Fact]
    public void RejectsSpeechCollectionAndUntargetedExaminationOccurrences()
    {
        ObjectId targetObjectId = new("flowers");
        CharacterTurnWorldEventOccurrence speech = new(
            Event(),
            new CharacterTurnTranscriptEntry(
                new DialogueUtterance(new CharacterId("healer"), "I examined it.")));

        Assert.Null(DeterministicExaminationResultSelector.Select(speech, targetObjectId));
        Assert.Null(DeterministicExaminationResultSelector.Select(
            Occurrence(CharacterIntentionType.CollectObject, targetObjectId),
            targetObjectId));
        Assert.Null(DeterministicExaminationResultSelector.Select(
            Occurrence(CharacterIntentionType.ExamineObject, null),
            targetObjectId));
    }

    [Fact]
    public void RejectsNullInputs()
    {
        ObjectId targetObjectId = new("flowers");
        CharacterTurnWorldEventOccurrence occurrence = Occurrence(
            CharacterIntentionType.ExamineObject,
            targetObjectId);

        Assert.Throws<ArgumentNullException>(() =>
            DeterministicExaminationResultSelector.Select(null!, targetObjectId));
        Assert.Throws<ArgumentNullException>(() =>
            DeterministicExaminationResultSelector.Select(occurrence, null!));
    }

    [Fact]
    public void ExposesOnlyTheSinglePureSelectionOperation()
    {
        Type selectorType = typeof(DeterministicExaminationResultSelector);
        System.Reflection.MethodInfo method = Assert.Single(
            selectorType.GetMethods().Where(candidate => candidate.DeclaringType == selectorType));

        Assert.Equal("Select", method.Name);
        Assert.Equal(typeof(CharacterExaminationResult), method.ReturnType);
        Assert.True(method.IsStatic);
        Assert.Equal(
            new[] { typeof(CharacterTurnWorldEventOccurrence), typeof(ObjectId) },
            method.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Empty(selectorType.GetConstructors());
        Assert.Empty(selectorType.GetProperties());
        Assert.Empty(selectorType.GetEvents());
        Assert.True(selectorType.IsAbstract);
        Assert.True(selectorType.IsSealed);
    }

    private static CharacterTurnWorldEventOccurrence Occurrence(
        CharacterIntentionType type,
        ObjectId? targetObjectId) =>
        new(
            Event(),
            new CharacterTurnTranscriptEntry(
                new CharacterId("healer"),
                new CharacterIntention(type, targetObjectId)));

    private static WorldEvent Event() =>
        new(new WorldEventId("healer-examined-flowers"), WorldEventTimeEffect.Advance);
}
