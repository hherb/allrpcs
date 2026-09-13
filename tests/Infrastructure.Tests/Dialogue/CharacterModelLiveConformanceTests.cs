using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Infrastructure.Tests.Dialogue;

public sealed class CharacterModelLiveConformanceTests
{
    [Fact]
    public void AcceptsOnlyOneCompletedBoundedTargetFreeWaitResponseForTheExactRequest()
    {
        CharacterModelGenerationRequest request = CreateRequest();
        CharacterTurnResponse response = new(
            request.TurnRequest,
            CharacterTurnDisposition.Accept,
            new DialogueDelivery(
                new DialogueUtterance(
                    request.TurnRequest.RespondingCharacterId,
                    "I will wait here."),
                DialogueDeliveryMode.OrdinarySpeech),
            new CharacterIntention(CharacterIntentionType.Wait, null),
            [],
            []);
        CharacterModelStreamTermination completed = new(
            CharacterModelStreamTerminationStatus.Completed,
            response,
            null);

        Assert.Equal(
            CharacterModelLiveConformanceOutcome.Conforming,
            CharacterModelLiveConformance.Evaluate(request, 1, completed));
        Assert.Equal(
            CharacterModelLiveConformanceOutcome.StreamViolation,
            CharacterModelLiveConformance.Evaluate(request, 2, completed));
        Assert.Equal(
            CharacterModelLiveConformanceOutcome.StreamViolation,
            CharacterModelLiveConformance.Evaluate(request, 0, null));
    }

    [Fact]
    public void SeparatesTerminalFailuresFromCompletedResponseViolations()
    {
        CharacterModelGenerationRequest request = CreateRequest();
        CharacterModelStreamTermination failed = new(
            CharacterModelStreamTerminationStatus.Failed,
            null,
            new CharacterModelFailure(CharacterModelFailureCode.InvalidResponse));
        CharacterTurnResponse commitmentResponse = new(
            request.TurnRequest,
            CharacterTurnDisposition.Accept,
            new DialogueDelivery(
                new DialogueUtterance(
                    request.TurnRequest.RespondingCharacterId,
                    "I will wait and then examine it."),
                DialogueDeliveryMode.OrdinarySpeech),
            new CharacterIntention(CharacterIntentionType.Wait, null),
            [new CharacterCommitment([new CharacterIntention(CharacterIntentionType.Wait, null)])],
            []);

        Assert.Equal(
            CharacterModelLiveConformanceOutcome.TerminationFailure,
            CharacterModelLiveConformance.Evaluate(request, 1, failed));
        Assert.Equal(
            CharacterModelLiveConformanceOutcome.ResponseViolation,
            CharacterModelLiveConformance.Evaluate(
                request,
                1,
                new CharacterModelStreamTermination(
                    CharacterModelStreamTerminationStatus.Completed,
                    commitmentResponse,
                    null)));
    }

    [Fact]
    public void AcceptsOnlyTheExpectedCurrentlyAffordedKnownTargetedAction()
    {
        CharacterModelGenerationRequest request = CreateRequest();
        ObjectId knownId = new("herb-patch");
        WorldObjectSnapshot worldObjects = new(
            [
                new WorldObject(
                    new WorldObjectIdentity(knownId, "Herb patch"),
                    new WorldObjectDescription("A visible patch of healing herbs.")),
            ]);
        CharacterIntentionAffordanceSnapshot affordances = new(
            [
                new CharacterIntentionAffordance(
                    CharacterIntentionType.ExamineObject,
                    knownId),
            ]);
        CharacterTurnResponse response = new(
            request.TurnRequest,
            CharacterTurnDisposition.Accept,
            new DialogueDelivery(
                new DialogueUtterance(
                    request.TurnRequest.RespondingCharacterId,
                    "I will inspect the herbs."),
                DialogueDeliveryMode.OrdinarySpeech),
            new CharacterIntention(
                CharacterIntentionType.ExamineObject,
                new ObjectId(knownId.Value)),
            [],
            []);
        CharacterModelStreamTermination completed = new(
            CharacterModelStreamTerminationStatus.Completed,
            response,
            null);

        Assert.Equal(
            CharacterModelLiveConformanceOutcome.Conforming,
            CharacterModelLiveConformance.EvaluateTargetedAction(
                request,
                1,
                completed,
                CharacterIntentionType.ExamineObject,
                knownId,
                worldObjects,
                affordances));
        Assert.Equal(
            CharacterModelLiveConformanceOutcome.ResponseViolation,
            CharacterModelLiveConformance.EvaluateTargetedAction(
                request,
                1,
                completed,
                CharacterIntentionType.CollectObject,
                knownId,
                worldObjects,
                affordances));
        Assert.Equal(
            CharacterModelLiveConformanceOutcome.ResponseViolation,
            CharacterModelLiveConformance.EvaluateTargetedAction(
                request,
                1,
                completed,
                CharacterIntentionType.ExamineObject,
                knownId,
                worldObjects,
                new CharacterIntentionAffordanceSnapshot([])));
    }

    [Fact]
    public void SummaryRequiresExactlyThreeOutcomesAndExposesOnlyBoundedCounts()
    {
        CharacterModelLiveConformanceSummary summary =
            CharacterModelLiveConformanceSummary.Create(
                [
                    CharacterModelLiveConformanceOutcome.Conforming,
                    CharacterModelLiveConformanceOutcome.TerminationFailure,
                    CharacterModelLiveConformanceOutcome.ResponseViolation,
                ]);

        Assert.Equal(1, summary.ConformingCount);
        Assert.Equal(0, summary.StreamViolationCount);
        Assert.Equal(1, summary.TerminationFailureCount);
        Assert.Equal(1, summary.ResponseViolationCount);
        Assert.False(summary.IsConforming);
        Assert.Equal(
            "Live conformance aggregate: completed=1; stream-violations=0; "
                + "termination-failures=1; response-violations=1.",
            summary.ToString());
        Assert.Throws<ArgumentException>(() =>
            CharacterModelLiveConformanceSummary.Create(
                [CharacterModelLiveConformanceOutcome.Conforming]));
        Assert.Throws<ArgumentException>(() =>
            CharacterModelLiveConformanceSummary.Create(
                [
                    CharacterModelLiveConformanceOutcome.Conforming,
                    CharacterModelLiveConformanceOutcome.Conforming,
                    CharacterModelLiveConformanceOutcome.Conforming,
                    CharacterModelLiveConformanceOutcome.Conforming,
                ]));
    }

    private static CharacterModelGenerationRequest CreateRequest()
    {
        CharacterId seekerId = new("seeker");
        CharacterId healerId = new("healer");
        DialogueUtterance utterance = new(seekerId, "Please wait here.");
        CharacterTurnRequest turnRequest = new(
            healerId,
            new DialogueEvent(
                new DialogueDelivery(utterance, DialogueDeliveryMode.OrdinarySpeech),
                new DialogueAudienceSnapshot(seekerId, [healerId])),
            new DialogueAct(utterance, DialogueActType.Request));
        return new CharacterModelGenerationRequest(
            turnRequest,
            new CharacterModelContextSnapshot(
                [
                    new CharacterModelContextEntry(
                        CharacterModelContextEntryKind.ValidAffordance,
                        "type=wait;target=null"),
                    new CharacterModelContextEntry(
                        CharacterModelContextEntryKind.UntrustedPlayerDialogue,
                        utterance.Text,
                        DialogueDeliveryMode.OrdinarySpeech),
                ]));
    }
}
