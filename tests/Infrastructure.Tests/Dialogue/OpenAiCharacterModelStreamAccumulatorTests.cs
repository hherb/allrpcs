using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Infrastructure.Dialogue;

namespace Allrpcs.Infrastructure.Tests.Dialogue;

public sealed class OpenAiCharacterModelStreamAccumulatorTests
{
    private const string FirstDocumentPart =
        "{\"disposition\":\"accept\",\"speech\":null,";
    private const string SecondDocumentPart =
        "\"immediateIntention\":{\"type\":\"wait\",\"targetObjectId\":null},"
        + "\"commitments\":[],\"stateProposals\":[]}";

    [Fact]
    public void BuffersStructuredDeltasAndCompletesTheExactResponse()
    {
        CharacterTurnRequest request = CreateRequest();
        OpenAiCharacterModelStreamAccumulator accumulator = new(request);

        CharacterModelStreamTermination? created = accumulator.Apply(
            "{\"type\":\"response.created\"}");
        CharacterModelStreamTermination? first = accumulator.Apply(Delta(FirstDocumentPart));
        CharacterModelStreamTermination? second = accumulator.Apply(Delta(SecondDocumentPart));
        CharacterModelStreamTermination? done = accumulator.Apply(
            Done(FirstDocumentPart + SecondDocumentPart));
        CharacterModelStreamTermination? completed = accumulator.Apply(
            "{\"type\":\"response.completed\"}");

        Assert.Null(created);
        Assert.Null(first);
        Assert.Null(second);
        Assert.Null(done);
        Assert.Equal(CharacterModelStreamTerminationStatus.Completed, completed?.Status);
        Assert.Same(request, completed?.Response?.Request);
        Assert.Same(CharacterIntentionType.Wait, completed?.Response?.ImmediateIntention.Type);
        Assert.Null(completed?.Failure);
    }

    [Fact]
    public void IgnoresReasoningEventsAndUsesTheBoundedDoneTextAsAuthoritative()
    {
        CharacterTurnRequest request = CreateRequest();
        OpenAiCharacterModelStreamAccumulator accumulator = new(request);
        string response = FirstDocumentPart + SecondDocumentPart;

        Assert.Null(accumulator.Apply(
            "{\"type\":\"response.reasoning_summary_part.added\",\"secret\":\"private\"}"));
        Assert.Null(accumulator.Apply(
            "{\"type\":\"response.reasoning_summary_text.delta\",\"delta\":\"private\"}"));
        Assert.Null(accumulator.Apply(
            "{\"type\":\"response.reasoning_summary_text.done\",\"text\":\"private\"}"));
        Assert.Null(accumulator.Apply(
            "{\"type\":\"response.reasoning_summary_part.done\",\"secret\":\"private\"}"));
        Assert.Null(accumulator.Apply(Delta("non-authoritative transport rendering")));
        Assert.Null(accumulator.Apply(Done(response)));

        CharacterModelStreamTermination? completed = accumulator.Apply(
            "{\"type\":\"response.completed\"}");

        Assert.Equal(CharacterModelStreamTerminationStatus.Completed, completed?.Status);
        Assert.Same(request, completed?.Response?.Request);
        Assert.DoesNotContain("private", accumulator.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void RejectsExcessiveAuthoritativeDoneText()
    {
        OpenAiCharacterModelStreamAccumulator accumulator = new(CreateRequest());
        string excessive = new(
            'x',
            CharacterTurnResponseJsonDeserializer.MaximumDocumentLength + 1);

        CharacterModelStreamTermination? result = accumulator.Apply(Done(excessive));

        Assert.Equal(CharacterModelFailureCode.InvalidResponse, result?.Failure?.Code);
    }

    [Fact]
    public void RetainsOnlyBoundedCountsFromCompletedUsage()
    {
        OpenAiCharacterModelStreamAccumulator accumulator = CompletedAccumulator();

        CharacterModelStreamTermination? completed = accumulator.Apply(
            CompletedWithUsage(11, 7, 18, "secret-response-id"));

        Assert.Equal(CharacterModelStreamTerminationStatus.Completed, completed?.Status);
        Assert.Equal(11, accumulator.InputTokenCount);
        Assert.Equal(7, accumulator.OutputTokenCount);
        Assert.Equal(18, accumulator.TotalTokenCount);
        Assert.DoesNotContain(
            "secret-response-id",
            accumulator.ToString(),
            StringComparison.Ordinal);
    }

    [Fact]
    public void AcceptsAbsentOrNullCompletedUsageAsUnknown()
    {
        OpenAiCharacterModelStreamAccumulator absent = CompletedAccumulator();
        OpenAiCharacterModelStreamAccumulator nullUsage = CompletedAccumulator();

        CharacterModelStreamTermination? absentResult = absent.Apply(
            "{\"type\":\"response.completed\",\"response\":{}}");
        CharacterModelStreamTermination? nullResult = nullUsage.Apply(
            "{\"type\":\"response.completed\",\"response\":{\"usage\":null}}");

        Assert.Equal(CharacterModelStreamTerminationStatus.Completed, absentResult?.Status);
        Assert.Equal(CharacterModelStreamTerminationStatus.Completed, nullResult?.Status);
        Assert.Null(absent.InputTokenCount);
        Assert.Null(absent.OutputTokenCount);
        Assert.Null(absent.TotalTokenCount);
        Assert.Null(nullUsage.InputTokenCount);
        Assert.Null(nullUsage.OutputTokenCount);
        Assert.Null(nullUsage.TotalTokenCount);
    }

    [Theory]
    [InlineData("{}")]
    [InlineData("{\"input_tokens\":1,\"output_tokens\":2}")]
    [InlineData("{\"input_tokens\":1,\"output_tokens\":2,\"total_tokens\":4}")]
    [InlineData("{\"input_tokens\":-1,\"output_tokens\":0,\"total_tokens\":-1}")]
    [InlineData("{\"input_tokens\":1.5,\"output_tokens\":2,\"total_tokens\":3.5}")]
    [InlineData("{\"input_tokens\":1000000,\"output_tokens\":1,\"total_tokens\":1000001}")]
    [InlineData("{\"input_tokens\":1,\"input_tokens\":1,\"output_tokens\":2,\"total_tokens\":3}")]
    public void RejectsMalformedInconsistentOrExcessiveCompletedUsage(string usage)
    {
        OpenAiCharacterModelStreamAccumulator accumulator = CompletedAccumulator();
        string completed = "{\"type\":\"response.completed\",\"response\":{\"usage\":"
            + usage
            + "}}";

        CharacterModelStreamTermination? result = accumulator.Apply(completed);

        Assert.Equal(CharacterModelFailureCode.InvalidResponse, result?.Failure?.Code);
        Assert.Null(accumulator.InputTokenCount);
        Assert.Null(accumulator.OutputTokenCount);
        Assert.Null(accumulator.TotalTokenCount);
    }

    [Fact]
    public void RejectsCompletionWithoutVerifiedDoneText()
    {
        OpenAiCharacterModelStreamAccumulator accumulator = new(CreateRequest());
        Assert.Null(accumulator.Apply(Delta(FirstDocumentPart + SecondDocumentPart)));

        CharacterModelStreamTermination? result = accumulator.Apply(
            "{\"type\":\"response.completed\"}");

        Assert.Equal(CharacterModelFailureCode.InvalidResponse, result?.Failure?.Code);
        Assert.Null(result?.Response);
    }

    [Theory]
    [InlineData("{\"type\":\"error\",\"message\":\"secret provider detail\"}")]
    [InlineData("{\"type\":\"response.refusal.done\",\"refusal\":\"secret refusal\"}")]
    [InlineData("{\"type\":\"response.failed\",\"response\":{}}")]
    public void MapsProviderTerminalEventsWithoutRetainingProviderDetails(string serializedEvent)
    {
        OpenAiCharacterModelStreamAccumulator accumulator = new(CreateRequest());

        CharacterModelStreamTermination? result = accumulator.Apply(serializedEvent);

        Assert.Equal(CharacterModelStreamTerminationStatus.Failed, result?.Status);
        Assert.Equal(CharacterModelFailureCode.ProviderRejected, result?.Failure?.Code);
        Assert.Null(result?.Response);
        Assert.DoesNotContain("secret", result?.ToString() ?? string.Empty, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-json")]
    [InlineData("{}")]
    [InlineData("{\"type\":\"unknown.event\"}")]
    [InlineData("{\"type\":\"response.output_text.delta\"}")]
    [InlineData("{\"type\":\"response.output_text.delta\",\"delta\":5}")]
    public void FailsClosedForMalformedOrUnexpectedEvents(string serializedEvent)
    {
        OpenAiCharacterModelStreamAccumulator accumulator = new(CreateRequest());

        CharacterModelStreamTermination? result = accumulator.Apply(serializedEvent);

        Assert.Equal(CharacterModelFailureCode.InvalidResponse, result?.Failure?.Code);
        Assert.Null(result?.Response);
    }

    [Fact]
    public void RejectsExcessiveInputNullsAndUpdatesAfterTermination()
    {
        CharacterTurnRequest request = CreateRequest();
        OpenAiCharacterModelStreamAccumulator accumulator = new(request);
        string excessiveEvent = new(
            'x',
            OpenAiCharacterModelStreamAccumulator.MaximumEventLength + 1);

        CharacterModelStreamTermination? excessive = accumulator.Apply(excessiveEvent);

        Assert.Equal(CharacterModelFailureCode.InvalidResponse, excessive?.Failure?.Code);
        Assert.Throws<ArgumentNullException>(() => new OpenAiCharacterModelStreamAccumulator(null!));
        Assert.Throws<ArgumentNullException>(() => accumulator.Apply(null!));
        Assert.Throws<InvalidOperationException>(() => accumulator.Apply(
            "{\"type\":\"response.created\"}"));
    }

    [Fact]
    public void RejectsCombinedDeltasBeyondTheResponseDocumentBound()
    {
        OpenAiCharacterModelStreamAccumulator accumulator = new(CreateRequest());
        string first = new('a', CharacterTurnResponseJsonDeserializer.MaximumDocumentLength);
        Assert.Null(accumulator.Apply(Delta(first)));

        CharacterModelStreamTermination? result = accumulator.Apply(Delta("b"));

        Assert.Equal(CharacterModelFailureCode.InvalidResponse, result?.Failure?.Code);
    }

    private static string Delta(string value) =>
        System.Text.Json.JsonSerializer.Serialize(new
        {
            type = "response.output_text.delta",
            delta = value,
        });

    private static string Done(string value) =>
        System.Text.Json.JsonSerializer.Serialize(new
        {
            type = "response.output_text.done",
            text = value,
        });

    private static string CompletedWithUsage(
        int inputTokenCount,
        int outputTokenCount,
        int totalTokenCount,
        string responseId) =>
        System.Text.Json.JsonSerializer.Serialize(new
        {
            type = "response.completed",
            response = new
            {
                id = responseId,
                usage = new
                {
                    input_tokens = inputTokenCount,
                    output_tokens = outputTokenCount,
                    total_tokens = totalTokenCount,
                },
            },
        });

    private static OpenAiCharacterModelStreamAccumulator CompletedAccumulator()
    {
        OpenAiCharacterModelStreamAccumulator accumulator = new(CreateRequest());
        Assert.Null(accumulator.Apply(Delta(FirstDocumentPart + SecondDocumentPart)));
        Assert.Null(accumulator.Apply(Done(FirstDocumentPart + SecondDocumentPart)));
        return accumulator;
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
