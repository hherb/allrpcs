using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Infrastructure.Dialogue;

namespace Allrpcs.Infrastructure.Tests.Dialogue;

public sealed class CharacterModelJsonResponseCompletionTests
{
    private const string ValidResponse = """
        {
          "disposition":"accept",
          "speech":"I will inspect it.",
          "deliveryMode":"whispering",
          "immediateIntention":{"type":"examine-object","targetObjectId":"herb-patch"},
          "commitments":[],
          "stateProposals":[]
        }
        """;

    [Fact]
    public void CompletesWithAnExactProviderNeutralResponseForTheRequest()
    {
        CharacterTurnRequest request = CreateRequest();

        CharacterModelStreamTermination result = CharacterModelJsonResponseCompletion.Create(
            request,
            ValidResponse);

        Assert.Equal(CharacterModelStreamTerminationStatus.Completed, result.Status);
        Assert.Same(request, result.Response?.Request);
        Assert.Equal("I will inspect it.", result.Response?.Speech?.Text);
        Assert.Same(DialogueDeliveryMode.Whispering, result.Response?.Delivery?.Mode);
        Assert.Same(CharacterIntentionType.ExamineObject, result.Response?.ImmediateIntention.Type);
        Assert.Equal(new ObjectId("herb-patch"), result.Response?.ImmediateIntention.TargetObjectId);
        Assert.Null(result.Failure);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-json")]
    [InlineData("{}")]
    public void MapsInvalidDocumentsToARedactedProviderNeutralFailure(string serialized)
    {
        CharacterModelStreamTermination result = CharacterModelJsonResponseCompletion.Create(
            CreateRequest(),
            serialized);

        Assert.Equal(CharacterModelStreamTerminationStatus.Failed, result.Status);
        Assert.Equal(CharacterModelFailureCode.InvalidResponse, result.Failure?.Code);
        Assert.Null(result.Response);
        if (serialized.Length > 0)
        {
            Assert.DoesNotContain(serialized, result.Failure?.ToString() ?? string.Empty);
        }
    }

    [Fact]
    public void MapsAnExcessiveDocumentAndRejectsNullInputs()
    {
        CharacterTurnRequest request = CreateRequest();

        CharacterModelStreamTermination excessive = CharacterModelJsonResponseCompletion.Create(
            request,
            new string('x', CharacterTurnResponseJsonDeserializer.MaximumDocumentLength + 1));

        Assert.Equal(CharacterModelFailureCode.InvalidResponse, excessive.Failure?.Code);
        Assert.Throws<ArgumentNullException>(() => CharacterModelJsonResponseCompletion.Create(
            null!,
            ValidResponse));
        Assert.Throws<ArgumentNullException>(() => CharacterModelJsonResponseCompletion.Create(
            request,
            null!));
    }

    private static CharacterTurnRequest CreateRequest()
    {
        CharacterId seekerId = new("seeker");
        DialogueUtterance utterance = new(seekerId, "Please inspect the herb.");
        DialogueEvent dialogueEvent = new(
            new DialogueDelivery(utterance, DialogueDeliveryMode.OrdinarySpeech),
            new DialogueAudienceSnapshot(seekerId, [new CharacterId("healer")]));
        return new CharacterTurnRequest(
            new CharacterId("healer"),
            dialogueEvent,
            new DialogueAct(utterance, DialogueActType.Request));
    }
}
