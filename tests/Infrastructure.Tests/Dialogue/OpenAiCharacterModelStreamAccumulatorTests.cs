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
