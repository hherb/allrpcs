using System.Text.Json;
using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Infrastructure.Dialogue;

namespace Allrpcs.Infrastructure.Tests.Dialogue;

public sealed class OllamaCachedPromptMetadataTests
{
    [Fact]
    public void ParticipationRetainsValidDecisionWithOutputOnlyUsageAsUnknown()
    {
        OllamaCharacterTurnParticipationStreamAccumulator accumulator = new(
            OpenAiCharacterTurnParticipationStreamAccumulatorTests.CreateRequest(true));
        string document = Native("{\"kind\":\"respond\",\"motiveKey\":null}", "0")
            .Replace("\"prompt_eval_count\":5,", "", StringComparison.Ordinal);
        Assert.Same(CharacterTurnParticipationKind.Respond, accumulator.Apply(document)?.Kind);
        Assert.Null(accumulator.InputTokenCount);
        Assert.Null(accumulator.OutputTokenCount);
        Assert.Null(accumulator.TotalTokenCount);
    }

    [Theory]
    [InlineData("0", true)]
    [InlineData("4", true)]
    [InlineData("-1", false)]
    [InlineData("6", true)]
    [InlineData("1000001", false)]
    [InlineData("\"0\"", false)]
    public void AcceptsOnlyBoundedCachedUsageWithoutChangingDecisionOrReply(string cached, bool valid)
    {
        CharacterTurnParticipationConsiderationRequest participation =
            OpenAiCharacterTurnParticipationStreamAccumulatorTests.CreateRequest(true);
        OllamaCharacterTurnParticipationStreamAccumulator decision = new(participation);
        string document = Native("{\"kind\":\"respond\",\"motiveKey\":null}", cached);
        if (valid)
        {
            Assert.Same(CharacterTurnParticipationKind.Respond, decision.Apply(document)?.Kind);
            Assert.Equal(5, decision.InputTokenCount);
            Assert.Equal(2, decision.OutputTokenCount);
        }
        else
        {
            Assert.Same(CharacterTurnParticipationKind.Respond, decision.Apply(document)?.Kind);
            Assert.Null(decision.InputTokenCount);
            Assert.Null(decision.OutputTokenCount);
        }

        DialogueUtterance utterance = new(new CharacterId("seeker"), "Please wait.");
        DialogueEvent heard = new(new DialogueDelivery(utterance, DialogueDeliveryMode.OrdinarySpeech),
            new DialogueAudienceSnapshot(utterance.SpeakerCharacterId, [new CharacterId("healer")]));
        CharacterTurnRequest turn = new(new CharacterId("healer"), heard, new DialogueAct(utterance, DialogueActType.Request));
        OllamaCharacterModelStreamAccumulator reply = new(turn);
        const string response = "{\"disposition\":\"accept\",\"speech\":null,\"deliveryMode\":null,"
            + "\"immediateIntention\":{\"type\":\"wait\",\"targetObjectId\":null},\"commitments\":[],\"stateProposals\":[]}";
        CharacterModelStreamTermination? result = reply.Apply(Native(response, cached));
        Assert.Equal(CharacterModelStreamTerminationStatus.Completed, result?.Status);
        if (!valid) { Assert.Null(reply.TotalTokenCount); }
        if (valid) { Assert.Same(turn, result?.Response?.Request); Assert.Equal(7, reply.TotalTokenCount); }
    }

    [Fact]
    public void RejectsEarlyCachedUsageAndInvalidEmptyDecision()
    {
        string[] invalid = [
            "{\"message\":{\"role\":\"assistant\",\"content\":\"\"},\"done\":false,\"prompt_eval_cached_count\":0}",
            "{\"message\":{\"role\":\"assistant\",\"content\":\"\"},\"done\":true,\"done_reason\":\"stop\",\"prompt_eval_cached_count\":0}",
        ];
        foreach (string document in invalid)
        {
            OllamaCharacterTurnParticipationStreamAccumulator accumulator = new(
                OpenAiCharacterTurnParticipationStreamAccumulatorTests.CreateRequest(true));
            Assert.Throws<InvalidDataException>(() => accumulator.Apply(document));
        }
    }

    private static string Native(string content, string cached) =>
        "{\"message\":{\"role\":\"assistant\",\"content\":" + JsonSerializer.Serialize(content)
        + "},\"done\":true,\"done_reason\":\"stop\",\"prompt_eval_count\":5,\"eval_count\":2,\"prompt_eval_cached_count\":" + cached + "}";
}
