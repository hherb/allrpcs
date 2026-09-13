using System.Text;
using System.Text.Json;
using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Infrastructure.Dialogue;

namespace Allrpcs.Infrastructure.Tests.Dialogue;

public sealed class OpenAiChatCompletionJsonCompletionTests
{
    private const string ResponseDocument =
        "{\"disposition\":\"accept\",\"speech\":\"I will wait.\","
        + "\"deliveryMode\":\"ordinary-speech\","
        + "\"immediateIntention\":{\"type\":\"wait\",\"targetObjectId\":null},"
        + "\"commitments\":[],\"stateProposals\":[]}";

    [Fact]
    public async Task CompletesOneExactBoundedChoiceAndRetainsOnlyUsageCounts()
    {
        CharacterTurnRequest request = CreateRequest();
        await using MemoryStream stream = Stream(Response(
            ResponseDocument,
            "stop",
            new { prompt_tokens = 10, completion_tokens = 6, total_tokens = 16 }));

        OpenAiCharacterModelSseResult result =
            await OpenAiChatCompletionJsonCompletion.CompleteWithUsageAsync(request, stream);

        Assert.Equal(CharacterModelStreamTerminationStatus.Completed, result.Termination.Status);
        Assert.Same(request, result.Termination.Response?.Request);
        Assert.Equal(10, result.InputTokenCount);
        Assert.Equal(6, result.OutputTokenCount);
        Assert.Equal(16, result.TotalTokenCount);
        Assert.DoesNotContain("provider-id", result.ToString(), StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("length")]
    [InlineData("content_filter")]
    public async Task RejectsNonStopFinishes(string finishReason)
    {
        await using MemoryStream stream = Stream(Response(ResponseDocument, finishReason, null));

        OpenAiCharacterModelSseResult result =
            await OpenAiChatCompletionJsonCompletion.CompleteWithUsageAsync(
                CreateRequest(),
                stream);

        Assert.Equal(CharacterModelFailureCode.InvalidResponse, result.Termination.Failure?.Code);
    }

    [Fact]
    public async Task RejectsANonAssistantChoice()
    {
        string response = JsonSerializer.Serialize(new
        {
            choices = new[]
            {
                new
                {
                    message = new { role = "user", content = ResponseDocument },
                    finish_reason = "stop",
                },
            },
            usage = (object?)null,
        });
        await using MemoryStream stream = Stream(response);

        OpenAiCharacterModelSseResult result =
            await OpenAiChatCompletionJsonCompletion.CompleteWithUsageAsync(
                CreateRequest(),
                stream);

        Assert.Equal(CharacterModelFailureCode.InvalidResponse, result.Termination.Failure?.Code);
    }

    [Fact]
    public async Task RejectsMalformedInvalidOrExcessiveResponses()
    {
        string[] responses =
        [
            "not-json",
            "{}",
            Response("{}", "stop", null),
            Response(ResponseDocument, "stop", new
            {
                prompt_tokens = 10,
                completion_tokens = 6,
                total_tokens = 99,
            }),
            new string('x', OpenAiChatCompletionJsonCompletion.MaximumDocumentLength + 1),
        ];

        foreach (string response in responses)
        {
            await using MemoryStream stream = Stream(response);
            OpenAiCharacterModelSseResult result =
                await OpenAiChatCompletionJsonCompletion.CompleteWithUsageAsync(
                    CreateRequest(),
                    stream);
            Assert.Equal(
                CharacterModelFailureCode.InvalidResponse,
                result.Termination.Failure?.Code);
        }
    }

    private static string Response(string content, string finishReason, object? usage) =>
        JsonSerializer.Serialize(new
        {
            id = "provider-id",
            choices = new[]
            {
                new
                {
                    message = new { role = "assistant", content },
                    finish_reason = finishReason,
                },
            },
            usage,
        });

    private static MemoryStream Stream(string value) => new(Encoding.UTF8.GetBytes(value));

    private static CharacterTurnRequest CreateRequest()
    {
        CharacterId seekerId = new("seeker");
        DialogueUtterance utterance = new(seekerId, "Please wait here.");
        return new CharacterTurnRequest(
            new CharacterId("healer"),
            new DialogueEvent(
                new DialogueDelivery(utterance, DialogueDeliveryMode.OrdinarySpeech),
                new DialogueAudienceSnapshot(seekerId, [new CharacterId("healer")])),
            new DialogueAct(utterance, DialogueActType.Request));
    }
}
