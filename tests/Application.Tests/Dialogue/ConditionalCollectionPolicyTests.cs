using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Application.Tests.Dialogue;

public sealed class ConditionalCollectionPolicyTests
{
    private static readonly ObjectId Patch = new("herb-patch");

    [Fact]
    public void CollectsAfterASafeResultWhenTheTurnConditionallyAccepted()
    {
        Assert.True(ConditionalCollectionPolicy.Collects(
            Response(CharacterTurnDisposition.ConditionallyAccept),
            Result(CharacterExaminationSafety.SafeToCollect)));
        Assert.False(ConditionalCollectionPolicy.Collects(
            Response(CharacterTurnDisposition.ConditionallyAccept),
            Result(CharacterExaminationSafety.UnsafeToCollect)));
        Assert.False(ConditionalCollectionPolicy.Collects(
            Response(CharacterTurnDisposition.ConditionallyAccept, examined: new ObjectId("stone-marker")),
            Result(CharacterExaminationSafety.SafeToCollect)));
    }

    [Fact]
    public void CollectsAnAcceptedExaminationOnlyWhenItCommittedToCollectingThatObject()
    {
        CharacterExaminationResult safe = Result(CharacterExaminationSafety.SafeToCollect);

        Assert.False(ConditionalCollectionPolicy.Collects(
            Response(CharacterTurnDisposition.Accept), safe));
        Assert.True(ConditionalCollectionPolicy.Collects(
            Response(CharacterTurnDisposition.Accept, new CharacterIntention(
                CharacterIntentionType.CollectObject, Patch)),
            safe));
        Assert.False(ConditionalCollectionPolicy.Collects(
            Response(CharacterTurnDisposition.Accept, new CharacterIntention(
                CharacterIntentionType.CollectObject, new ObjectId("stone-marker"))),
            safe));
        Assert.Throws<ArgumentNullException>(() => ConditionalCollectionPolicy.Collects(null!, safe));
    }

    private static CharacterTurnResponse Response(
        CharacterTurnDisposition disposition,
        CharacterIntention? committed = null,
        ObjectId? examined = null)
    {
        CharacterId seekerId = new("seeker");
        CharacterId healerId = new("healer");
        DialogueUtterance utterance = new(seekerId, "Examine the herbs and gather them if safe.");
        CharacterTurnRequest request = new(
            healerId,
            new DialogueEvent(
                new DialogueDelivery(utterance, DialogueDeliveryMode.OrdinarySpeech),
                new DialogueAudienceSnapshot(seekerId, [healerId])),
            new DialogueAct(utterance, DialogueActType.ConditionalPlan));
        return new CharacterTurnResponse(
            request,
            disposition,
            null,
            new CharacterIntention(CharacterIntentionType.ExamineObject, examined ?? Patch),
            committed is null ? [] : [new CharacterCommitment([committed])],
            []);
    }

    private static CharacterExaminationResult Result(CharacterExaminationSafety safety) =>
        new(
            new CharacterTurnWorldEventOccurrence(
                new WorldEvent(new WorldEventId("herb-patch-examination-1"), WorldEventTimeEffect.Advance),
                new CharacterTurnTranscriptEntry(
                    new CharacterId("healer"),
                    new CharacterIntention(CharacterIntentionType.ExamineObject, Patch))),
            safety);
}
