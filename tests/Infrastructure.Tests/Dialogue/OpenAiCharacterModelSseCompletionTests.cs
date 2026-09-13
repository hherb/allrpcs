using System.Text;
using System.Text.Json;
using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Infrastructure.Dialogue;

namespace Allrpcs.Infrastructure.Tests.Dialogue;

public sealed class OpenAiCharacterModelSseCompletionTests
{
    private const string ResponseDocument =
        "{\"disposition\":\"accept\",\"speech\":null,"
        + "\"immediateIntention\":{\"type\":\"wait\",\"targetObjectId\":null},"
        + "\"commitments\":[],\"stateProposals\":[]}";

    [Fact]
    public async Task CompletesTheExactRequestFromFramedSemanticEvents()
    {
        CharacterTurnRequest request = CreateRequest();
        string events = Frame(new { type = "response.created" })
            + Frame(new { type = "response.output_text.delta", delta = ResponseDocument })
            + Frame(new { type = "response.output_text.done", text = ResponseDocument })
            + Frame(new { type = "response.completed" });
        using MemoryStream stream = Utf8Stream(events);

        CharacterModelStreamTermination result =
            await OpenAiCharacterModelSseCompletion.CompleteAsync(request, stream);

        Assert.Equal(CharacterModelStreamTerminationStatus.Completed, result.Status);
        Assert.Same(request, result.Response?.Request);
        Assert.Null(result.Failure);
        Assert.True(stream.CanRead);
    }

    [Fact]
    public async Task ReturnsOnlyTerminationAndBoundedUsageFromCompletedEvent()
    {
        CharacterTurnRequest request = CreateRequest();
        string events = Frame(new { type = "response.output_text.delta", delta = ResponseDocument })
            + Frame(new { type = "response.output_text.done", text = ResponseDocument })
            + Frame(new
            {
                type = "response.completed",
                response = new
                {
                    id = "secret-response-id",
                    usage = new
                    {
                        input_tokens = 11,
                        output_tokens = 7,
                        total_tokens = 18,
                    },
                },
            });
        using MemoryStream stream = Utf8Stream(events);

        OpenAiCharacterModelSseResult result =
            await OpenAiCharacterModelSseCompletion.CompleteWithUsageAsync(request, stream);

        Assert.Equal(CharacterModelStreamTerminationStatus.Completed, result.Termination.Status);
        Assert.Equal(11, result.InputTokenCount);
        Assert.Equal(7, result.OutputTokenCount);
        Assert.Equal(18, result.TotalTokenCount);
        Assert.DoesNotContain("secret-response-id", result.ToString(), StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("response.failed")]
    [InlineData("response.refusal.done")]
    public async Task RetainsOnlyARedactedProviderRejection(string eventType)
    {
        string events = Frame(new { type = eventType, detail = "secret provider detail" });
        using MemoryStream stream = Utf8Stream(events);

        CharacterModelStreamTermination result =
            await OpenAiCharacterModelSseCompletion.CompleteAsync(CreateRequest(), stream);

        Assert.Equal(CharacterModelFailureCode.ProviderRejected, result.Failure?.Code);
        Assert.Null(result.Response);
        Assert.DoesNotContain("secret", result.ToString(), StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("")]
    [InlineData(": heartbeat\n\n")]
    [InlineData("data: {\"type\":\"response.created\"}\n\n")]
    [InlineData("data: not-json\n\n")]
    [InlineData("data: {\"type\":\"response.completed\"}\n\n")]
    [InlineData("data: {\"type\":\"response.failed\"}\n\ndata: {}\n\n")]
    [InlineData("data: {}")]
    public async Task MapsIncompleteMalformedAndTrailingInputToInvalidResponse(string events)
    {
        using MemoryStream stream = Utf8Stream(events);

        CharacterModelStreamTermination result =
            await OpenAiCharacterModelSseCompletion.CompleteAsync(CreateRequest(), stream);

        Assert.Equal(CharacterModelFailureCode.InvalidResponse, result.Failure?.Code);
        Assert.Null(result.Response);
    }

    [Fact]
    public async Task MapsIoFailureToTransportWithoutRetainingProviderDetails()
    {
        using ThrowingReadStream stream = new("secret transport detail");

        CharacterModelStreamTermination result =
            await OpenAiCharacterModelSseCompletion.CompleteAsync(CreateRequest(), stream);

        Assert.Equal(CharacterModelFailureCode.Transport, result.Failure?.Code);
        Assert.Null(result.Response);
        Assert.DoesNotContain("secret", result.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task PropagatesCallerCancellationAndRejectsNulls()
    {
        CharacterTurnRequest request = CreateRequest();
        using MemoryStream stream = Utf8Stream(string.Empty);
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => OpenAiCharacterModelSseCompletion.CompleteAsync(
                request,
                stream,
                cancellation.Token));
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => OpenAiCharacterModelSseCompletion.CompleteAsync(null!, stream));
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => OpenAiCharacterModelSseCompletion.CompleteAsync(request, null!));
    }

    private static string Frame(object semanticEvent) =>
        "data: " + JsonSerializer.Serialize(semanticEvent) + "\n\n";

    private static MemoryStream Utf8Stream(string value) =>
        new(Encoding.UTF8.GetBytes(value));

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

    private sealed class ThrowingReadStream(string message) : Stream
    {
        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => throw new NotSupportedException();

        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override void Flush() => throw new NotSupportedException();

        public override int Read(byte[] buffer, int offset, int count) =>
            throw new IOException(message);

        public override ValueTask<int> ReadAsync(
            Memory<byte> buffer,
            CancellationToken cancellationToken = default) =>
            ValueTask.FromException<int>(new IOException(message));

        public override long Seek(long offset, SeekOrigin origin) =>
            throw new NotSupportedException();

        public override void SetLength(long value) => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count) =>
            throw new NotSupportedException();
    }
}
