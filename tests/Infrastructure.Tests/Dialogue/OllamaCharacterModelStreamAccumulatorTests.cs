using System.Text.Json;
using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Infrastructure.Dialogue;

namespace Allrpcs.Infrastructure.Tests.Dialogue;

public sealed class OllamaCharacterModelStreamAccumulatorTests
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
    };

    private const string ResponseDocument =
        "{\"disposition\":\"accept\",\"speech\":null,"
        + "\"immediateIntention\":{\"type\":\"wait\",\"targetObjectId\":null},"
        + "\"commitments\":[],\"stateProposals\":[]}";

    [Fact]
    public void CompletesExactRequestAndRetainsWhollyPresentBoundedUsage()
    {
        CharacterTurnRequest request = CreateRequest();
        OllamaCharacterModelStreamAccumulator accumulator = new(request);

        Assert.Null(accumulator.Apply(ChatChunk(ResponseDocument[..40], done: false)));
        Assert.Null(accumulator.Apply(ChatChunk(ResponseDocument[40..], done: false)));
        CharacterModelStreamTermination? completed = accumulator.Apply(
            ChatChunk(string.Empty, done: true, "stop", 11, 7));

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
        OllamaCharacterModelStreamAccumulator accumulator = new(CreateRequest());

        Assert.Null(accumulator.Apply(ChatChunk(ResponseDocument, done: false)));
        CharacterModelStreamTermination? completed = accumulator.Apply(
            ChatChunk(string.Empty, done: true, "stop"));

        Assert.Equal(CharacterModelStreamTerminationStatus.Completed, completed?.Status);
        Assert.Null(accumulator.InputTokenCount);
        Assert.Null(accumulator.OutputTokenCount);
        Assert.Null(accumulator.TotalTokenCount);
    }

    [Fact]
    public void MapsMidStreamErrorToRedactedProviderFailure()
    {
        OllamaCharacterModelStreamAccumulator accumulator = new(CreateRequest());
        Assert.Null(accumulator.Apply(ChatChunk("{", done: false)));

        CharacterModelStreamTermination? failed = accumulator.Apply(
            "{\"error\":\"secret provider detail\"}");

        Assert.Equal(CharacterModelFailureCode.ProviderRejected, failed?.Failure?.Code);
        Assert.Null(failed?.Response);
        Assert.DoesNotContain("secret", failed?.ToString() ?? string.Empty, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-json")]
    [InlineData("{}")]
    [InlineData("[]")]
    [InlineData("{\"error\":null}")]
    [InlineData("{\"error\":\"\"}")]
    [InlineData("{\"error\":\"failure\",\"done\":false}")]
    public void RejectsMalformedAndUnknownErrorShapes(string serialized)
    {
        OllamaCharacterModelStreamAccumulator accumulator = new(CreateRequest());

        CharacterModelStreamTermination? result = accumulator.Apply(serialized);

        Assert.Equal(CharacterModelFailureCode.InvalidResponse, result?.Failure?.Code);
        Assert.Null(result?.Response);
    }

    [Theory]
    [InlineData("model", "null")]
    [InlineData("created_at", "42")]
    [InlineData("total_duration", "-1")]
    [InlineData("eval_duration", "\"slow\"")]
    public void RejectsMalformedKnownMetadata(string name, string value)
    {
        OllamaCharacterModelStreamAccumulator accumulator = new(CreateRequest());
        string encodedContent = JsonSerializer.Serialize(ResponseDocument);
        string serialized =
            $"{{\"message\":{{\"role\":\"assistant\",\"content\":{encodedContent}}},"
            + $"\"done\":true,\"done_reason\":\"stop\",\"{name}\":{value}}}";

        CharacterModelStreamTermination? result = accumulator.Apply(serialized);

        Assert.Equal(CharacterModelFailureCode.InvalidResponse, result?.Failure?.Code);
    }

    [Theory]
    [InlineData("user", "content")]
    [InlineData("system", "content")]
    [InlineData("assistant", null)]
    public void RejectsNonAssistantRolesAndNonStringContent(string role, string? content)
    {
        OllamaCharacterModelStreamAccumulator accumulator = new(CreateRequest());
        string serialized = JsonSerializer.Serialize(new
        {
            message = new { role, content },
            done = false,
        });

        CharacterModelStreamTermination? result = accumulator.Apply(serialized);

        Assert.Equal(CharacterModelFailureCode.InvalidResponse, result?.Failure?.Code);
    }

    [Theory]
    [InlineData("thinking", "secret reasoning")]
    [InlineData("tool_calls", "secret tool")]
    [InlineData("unknown", "secret field")]
    public void RejectsThinkingToolCallsAndUnknownMessageShapes(string name, string value)
    {
        OllamaCharacterModelStreamAccumulator accumulator = new(CreateRequest());
        string serialized =
            $"{{\"message\":{{\"role\":\"assistant\",\"content\":\"x\","
            + $"\"{name}\":\"{value}\"}},\"done\":false}}";

        CharacterModelStreamTermination? result = accumulator.Apply(serialized);

        Assert.Equal(CharacterModelFailureCode.InvalidResponse, result?.Failure?.Code);
        Assert.DoesNotContain("secret", result?.ToString() ?? string.Empty, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("length")]
    [InlineData("load")]
    [InlineData("")]
    public void RejectsNonSuccessfulStopReasons(string doneReason)
    {
        OllamaCharacterModelStreamAccumulator accumulator = new(CreateRequest());

        CharacterModelStreamTermination? result = accumulator.Apply(
            ChatChunk(ResponseDocument, done: true, doneReason));

        Assert.Equal(CharacterModelFailureCode.InvalidResponse, result?.Failure?.Code);
        Assert.Null(accumulator.InputTokenCount);
    }

    [Fact]
    public void RejectsPrematureDoneRepeatedDoneAndUpdatesAfterFailure()
    {
        OllamaCharacterModelStreamAccumulator premature = new(CreateRequest());
        CharacterModelStreamTermination? prematureResult = premature.Apply(
            ChatChunk("{", done: true, "stop"));

        Assert.Equal(CharacterModelFailureCode.InvalidResponse, prematureResult?.Failure?.Code);
        Assert.Throws<InvalidOperationException>(() =>
            premature.Apply(ChatChunk(string.Empty, done: true, "stop")));

        OllamaCharacterModelStreamAccumulator repeated = new(CreateRequest());
        Assert.Null(repeated.Apply(ChatChunk(ResponseDocument, done: false)));
        Assert.NotNull(repeated.Apply(ChatChunk(string.Empty, done: true, "stop")));
        Assert.Throws<InvalidOperationException>(() =>
            repeated.Apply(ChatChunk(string.Empty, done: true, "stop")));
    }

    [Theory]
    [InlineData(-1, 1)]
    [InlineData(1, -1)]
    [InlineData(1000000, 1)]
    public void RejectsMalformedOrExcessiveUsage(int promptCount, int outputCount)
    {
        OllamaCharacterModelStreamAccumulator accumulator = new(CreateRequest());

        CharacterModelStreamTermination? result = accumulator.Apply(
            ChatChunk(ResponseDocument, done: true, "stop", promptCount, outputCount));

        Assert.Equal(CharacterModelFailureCode.InvalidResponse, result?.Failure?.Code);
        Assert.Null(accumulator.InputTokenCount);
        Assert.Null(accumulator.OutputTokenCount);
        Assert.Null(accumulator.TotalTokenCount);
    }

    [Fact]
    public void RejectsPartiallyPresentUsageAndUsageBeforeTermination()
    {
        OllamaCharacterModelStreamAccumulator partial = new(CreateRequest());
        string partialUsage = JsonSerializer.Serialize(new
        {
            message = new { role = "assistant", content = ResponseDocument },
            done = true,
            done_reason = "stop",
            prompt_eval_count = 1,
        });

        OllamaCharacterModelStreamAccumulator early = new(CreateRequest());
        string earlyUsage = ChatChunk("x", done: false, promptCount: 1, outputCount: 1);

        Assert.Equal(
            CharacterModelFailureCode.InvalidResponse,
            partial.Apply(partialUsage)?.Failure?.Code);
        Assert.Equal(
            CharacterModelFailureCode.InvalidResponse,
            early.Apply(earlyUsage)?.Failure?.Code);
    }

    [Fact]
    public void RejectsExcessiveCombinedContentNullsAndDuplicateProperties()
    {
        OllamaCharacterModelStreamAccumulator excessive = new(CreateRequest());
        Assert.Null(excessive.Apply(ChatChunk(
            new string('a', CharacterTurnResponseJsonDeserializer.MaximumDocumentLength),
            done: false)));
        CharacterModelStreamTermination? excessiveResult = excessive.Apply(
            ChatChunk("b", done: false));

        OllamaCharacterModelStreamAccumulator duplicate = new(CreateRequest());
        CharacterModelStreamTermination? duplicateResult = duplicate.Apply(
            "{\"message\":{\"role\":\"assistant\",\"content\":\"x\"},"
            + "\"done\":false,\"done\":false}");

        Assert.Equal(CharacterModelFailureCode.InvalidResponse, excessiveResult?.Failure?.Code);
        Assert.Equal(CharacterModelFailureCode.InvalidResponse, duplicateResult?.Failure?.Code);
        Assert.Throws<ArgumentNullException>(() =>
            new OllamaCharacterModelStreamAccumulator(null!));
        Assert.Throws<ArgumentNullException>(() => excessive.Apply(null!));
    }

    private static string ChatChunk(
        string content,
        bool done,
        string? doneReason = null,
        int? promptCount = null,
        int? outputCount = null) => JsonSerializer.Serialize(new
        {
            model = "model:test",
            created_at = "2026-09-10T00:00:00Z",
            message = new { role = "assistant", content },
            done,
            done_reason = doneReason,
            prompt_eval_count = promptCount,
            eval_count = outputCount,
        }, SerializerOptions);

    private static CharacterTurnRequest CreateRequest()
    {
        CharacterId seekerId = new("seeker");
        DialogueUtterance utterance = new(seekerId, "Please inspect the herb.");
        DialogueEvent dialogueEvent = new(
            utterance,
            new DialogueAudienceSnapshot(seekerId, [new CharacterId("healer")]));
        return new CharacterTurnRequest(
            new CharacterId("healer"),
            dialogueEvent,
            new DialogueAct(utterance, DialogueActType.Request));
    }
}
