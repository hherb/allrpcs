using System.Text.Json;
using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Infrastructure.Dialogue;

namespace Allrpcs.Infrastructure.Tests.Dialogue;

public sealed class AnthropicCharacterModelStreamAccumulatorTests
{
    private const string FirstDocumentPart =
        "{\"disposition\":\"accept\",\"speech\":null,";
    private const string SecondDocumentPart =
        "\"immediateIntention\":{\"type\":\"wait\",\"targetObjectId\":null},"
        + "\"commitments\":[],\"stateProposals\":[]}";

    [Fact]
    public void BuffersOneTextBlockAndCompletesTheExactResponse()
    {
        CharacterTurnRequest request = CreateRequest();
        AnthropicCharacterModelStreamAccumulator accumulator = new(request);

        Assert.Null(accumulator.Apply(MessageStart(inputTokens: 11)));
        Assert.Null(accumulator.Apply("{\"type\":\"ping\"}"));
        Assert.Null(accumulator.Apply(ContentStart()));
        Assert.Null(accumulator.Apply(TextDelta(FirstDocumentPart)));
        Assert.Null(accumulator.Apply(TextDelta(SecondDocumentPart)));
        Assert.Null(accumulator.Apply(ContentStop()));
        Assert.Null(accumulator.Apply(MessageDelta("end_turn", outputTokens: 7)));
        CharacterModelStreamTermination? completed = accumulator.Apply(
            "{\"type\":\"message_stop\"}");

        Assert.Equal(CharacterModelStreamTerminationStatus.Completed, completed?.Status);
        Assert.Same(request, completed?.Response?.Request);
        Assert.Same(CharacterIntentionType.Wait, completed?.Response?.ImmediateIntention.Type);
        Assert.Null(completed?.Failure);
        Assert.Equal(11, accumulator.InputTokenCount);
        Assert.Equal(7, accumulator.OutputTokenCount);
        Assert.Equal(18, accumulator.TotalTokenCount);
    }

    [Fact]
    public void AcceptsWhollyAbsentUsageAsUnknown()
    {
        AnthropicCharacterModelStreamAccumulator accumulator = StartedAccumulator(
            includeUsage: false);

        Assert.Null(accumulator.Apply(MessageDelta("end_turn", outputTokens: null)));
        CharacterModelStreamTermination? completed = accumulator.Apply(
            "{\"type\":\"message_stop\"}");

        Assert.Equal(CharacterModelStreamTerminationStatus.Completed, completed?.Status);
        Assert.Null(accumulator.InputTokenCount);
        Assert.Null(accumulator.OutputTokenCount);
        Assert.Null(accumulator.TotalTokenCount);
    }

    [Theory]
    [InlineData("refusal", CharacterModelFailureCode.ProviderRejected)]
    [InlineData("max_tokens", CharacterModelFailureCode.InvalidResponse)]
    [InlineData("tool_use", CharacterModelFailureCode.InvalidResponse)]
    public void MapsNonCompletedStopReasonsWithoutRetainingDetails(
        string stopReason,
        CharacterModelFailureCode expectedCode)
    {
        AnthropicCharacterModelStreamAccumulator accumulator = StartedAccumulator();

        CharacterModelStreamTermination? result = accumulator.Apply(
            MessageDelta(stopReason, outputTokens: 7, detail: "secret provider detail"));

        Assert.Equal(expectedCode, result?.Failure?.Code);
        Assert.Null(result?.Response);
        Assert.DoesNotContain("secret", result?.ToString() ?? string.Empty, StringComparison.Ordinal);
        Assert.Null(accumulator.InputTokenCount);
        Assert.Null(accumulator.OutputTokenCount);
        Assert.Null(accumulator.TotalTokenCount);
    }

    [Fact]
    public void MapsStreamErrorsToRedactedProviderRejection()
    {
        AnthropicCharacterModelStreamAccumulator accumulator = new(CreateRequest());

        CharacterModelStreamTermination? result = accumulator.Apply(
            "{\"type\":\"error\",\"error\":{\"message\":\"secret detail\"}}");

        Assert.Equal(CharacterModelFailureCode.ProviderRejected, result?.Failure?.Code);
        Assert.DoesNotContain("secret", result?.ToString() ?? string.Empty, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-json")]
    [InlineData("{}")]
    [InlineData("{\"type\":\"unknown_event\"}")]
    [InlineData("{\"type\":\"content_block_delta\",\"index\":0,\"delta\":{\"type\":\"text_delta\",\"text\":\"x\"}}")]
    [InlineData("{\"type\":\"message_stop\"}")]
    public void FailsClosedForMalformedUnexpectedOrOutOfOrderEvents(string serializedEvent)
    {
        AnthropicCharacterModelStreamAccumulator accumulator = new(CreateRequest());

        CharacterModelStreamTermination? result = accumulator.Apply(serializedEvent);

        Assert.Equal(CharacterModelFailureCode.InvalidResponse, result?.Failure?.Code);
        Assert.Null(result?.Response);
    }

    [Fact]
    public void RejectsNonTextBlocksWrongIndexesAndUpdatesAfterTermination()
    {
        AnthropicCharacterModelStreamAccumulator nonText = new(CreateRequest());
        Assert.Null(nonText.Apply(MessageStart(inputTokens: 1)));
        CharacterModelStreamTermination? nonTextResult = nonText.Apply(
            "{\"type\":\"content_block_start\",\"index\":0,"
            + "\"content_block\":{\"type\":\"thinking\",\"thinking\":\"secret\"}}");

        AnthropicCharacterModelStreamAccumulator wrongIndex = new(CreateRequest());
        Assert.Null(wrongIndex.Apply(MessageStart(inputTokens: 1)));
        CharacterModelStreamTermination? wrongIndexResult = wrongIndex.Apply(
            "{\"type\":\"content_block_start\",\"index\":1,"
            + "\"content_block\":{\"type\":\"text\",\"text\":\"\"}}");

        Assert.Equal(CharacterModelFailureCode.InvalidResponse, nonTextResult?.Failure?.Code);
        Assert.Equal(CharacterModelFailureCode.InvalidResponse, wrongIndexResult?.Failure?.Code);
        Assert.Throws<InvalidOperationException>(() => nonText.Apply("{\"type\":\"ping\"}"));
    }

    [Fact]
    public void RejectsExcessiveCombinedTextNullsAndMalformedUsage()
    {
        AnthropicCharacterModelStreamAccumulator excessive = new(CreateRequest());
        Assert.Null(excessive.Apply(MessageStart(inputTokens: 1)));
        Assert.Null(excessive.Apply(ContentStart()));
        Assert.Null(excessive.Apply(TextDelta(new string(
            'a',
            CharacterTurnResponseJsonDeserializer.MaximumDocumentLength))));
        CharacterModelStreamTermination? excessiveResult = excessive.Apply(TextDelta("b"));

        AnthropicCharacterModelStreamAccumulator malformedUsage = new(CreateRequest());
        CharacterModelStreamTermination? usageResult = malformedUsage.Apply(
            "{\"type\":\"message_start\",\"message\":{\"content\":[],"
            + "\"stop_reason\":null,\"usage\":{\"input_tokens\":-1}}}");

        Assert.Equal(CharacterModelFailureCode.InvalidResponse, excessiveResult?.Failure?.Code);
        Assert.Equal(CharacterModelFailureCode.InvalidResponse, usageResult?.Failure?.Code);
        Assert.Throws<ArgumentNullException>(() =>
            new AnthropicCharacterModelStreamAccumulator(null!));
        Assert.Throws<ArgumentNullException>(() => excessive.Apply(null!));
    }

    private static AnthropicCharacterModelStreamAccumulator StartedAccumulator(
        bool includeUsage = true)
    {
        AnthropicCharacterModelStreamAccumulator accumulator = new(CreateRequest());
        Assert.Null(accumulator.Apply(includeUsage
            ? MessageStart(inputTokens: 11)
            : MessageStart(inputTokens: null)));
        Assert.Null(accumulator.Apply(ContentStart()));
        Assert.Null(accumulator.Apply(TextDelta(FirstDocumentPart + SecondDocumentPart)));
        Assert.Null(accumulator.Apply(ContentStop()));
        return accumulator;
    }

    private static string MessageStart(int? inputTokens)
    {
        object? usage = inputTokens is null ? null : new { input_tokens = inputTokens.Value };
        return JsonSerializer.Serialize(new
        {
            type = "message_start",
            message = new
            {
                content = Array.Empty<object>(),
                stop_reason = (string?)null,
                usage,
            },
        });
    }

    private static string ContentStart() =>
        "{\"type\":\"content_block_start\",\"index\":0,"
        + "\"content_block\":{\"type\":\"text\",\"text\":\"\"}}";

    private static string TextDelta(string text) => JsonSerializer.Serialize(new
    {
        type = "content_block_delta",
        index = 0,
        delta = new { type = "text_delta", text },
    });

    private static string ContentStop() =>
        "{\"type\":\"content_block_stop\",\"index\":0}";

    private static string MessageDelta(
        string stopReason,
        int? outputTokens,
        string? detail = null)
    {
        object? usage = outputTokens is null ? null : new { output_tokens = outputTokens.Value };
        return JsonSerializer.Serialize(new
        {
            type = "message_delta",
            delta = new { stop_reason = stopReason, detail },
            usage,
        });
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
