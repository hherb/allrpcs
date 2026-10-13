using Allrpcs.Application.Audio;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Application.Tests.Audio;

public sealed class HerbCollectionPresentationProjectionTests
{
    [Fact]
    public void ExactActorTargetSafetyAndEventDefinitionsAreRequired()
    {
        var actor = new CharacterId("healer"); var target = new ObjectId("herb-patch");
        var examined = new WorldEvent(new("examined"), WorldEventTimeEffect.Advance);
        var collected = new WorldEvent(new("collected"), WorldEventTimeEffect.Advance);
        var exam = new CharacterTurnWorldEventOccurrence(examined, new(actor, new(CharacterIntentionType.ExamineObject, target)));
        var result = new CharacterExaminationResult(exam, CharacterExaminationSafety.SafeToCollect);
        var occurrence = new CharacterTurnWorldEventOccurrence(collected, new(actor, new(CharacterIntentionType.CollectObject, target)));
        bool Valid(CharacterExaminationResult? candidate, CharacterTurnWorldEventOccurrence? commit,
            CharacterId? owner, ObjectId? site, WorldEvent? examination, WorldEvent? collection) =>
            HerbCollectionPresentationProjection.IsCommit(candidate, commit, owner, site, examination, collection);
        Assert.True(Valid(result, occurrence, actor, target, examined, collected));
        Assert.False(Valid(null, occurrence, actor, target, examined, collected));
        Assert.False(Valid(result, null, actor, target, examined, collected));
        Assert.False(Valid(result, occurrence, new("healer"), target, examined, collected));
        Assert.False(Valid(result, occurrence, actor, new("herb-patch"), examined, collected));
        Assert.False(Valid(result, occurrence, actor, target, new(new("examined"), examined.TimeEffect), collected));
        Assert.False(Valid(result, occurrence, actor, target, examined, new(new("collected"), collected.TimeEffect)));
        Assert.False(Valid(new(exam, CharacterExaminationSafety.UnsafeToCollect), occurrence, actor, target, examined, collected));
        Assert.False(Valid(result, new(collected, new(actor, new(CharacterIntentionType.ExamineObject, target))), actor, target, examined, collected));
        Assert.False(Valid(result, new(collected, new(new CharacterId("healer"), occurrence.TranscriptEntry.PhysicalAction!)), actor, target, examined, collected));
    }
    [Theory]
    [InlineData(32, 191, true)]
    [InlineData(32, 192, false)]
    [InlineData(33, 191, false)]
    [InlineData(32.001, 0, false)]
    [InlineData(0, 191.999, true)]
    [InlineData(float.NaN, 0, false)]
    [InlineData(0, float.PositiveInfinity, false)]
    public void ActorInclusiveAndListenerExclusiveRanges(float actor, float listener, bool expected) =>
        Assert.Equal(expected, HerbCollectionPresentationProjection.Local(new(actor, 0), new(listener, 0), new(0, 0)));
    [Theory]
    [InlineData(0, false)]
    [InlineData(-1, false)]
    [InlineData(double.NaN, false)]
    [InlineData(double.PositiveInfinity, false)]
    [InlineData(.101, false)]
    [InlineData(.1, true)]
    [InlineData(.0166666667, true)]
    public void InvalidClockCannotRetainFeedback(double delta, bool expected) =>
        Assert.Equal(expected, HerbCollectionPresentationProjection.ValidDelta(delta));
}
