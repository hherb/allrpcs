using System.Text;
using System.Text.Json;
using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Infrastructure.Dialogue;

namespace Allrpcs.Infrastructure.Tests.Dialogue;

public sealed class AnthropicCharacterModelSseCompletionTests
{
    private const string ResponseDocument =
        "{\"disposition\":\"accept\",\"speech\":null,"
        + "\"immediateIntention\":{\"type\":\"wait\",\"targetObjectId\":null},"
        + "\"commitments\":[],\"stateProposals\":[]}";

    [Fact]
    public async Task CompletesTheExactRequestFromFramedMessagesEvents()
    {
        CharacterTurnRequest request = CreateRequest();
        string events = Frame("message_start", new
        {
            type = "message_start",
            message = new
            {
                content = Array.Empty<object>(),
                stop_reason = (string?)null,
                usage = new { input_tokens = 11 },
            },
        })
            + Frame("content_block_start", new
            {
                type = "content_block_start",
                index = 0,
                content_block = new { type = "text", text = string.Empty },
            })
            + Frame("content_block_delta", new
            {
                type = "content_block_delta",
                index = 0,
                delta = new { type = "text_delta", text = ResponseDocument },
            })
            + Frame("content_block_stop", new { type = "content_block_stop", index = 0 })
            + Frame("message_delta", new
            {
                type = "message_delta",
                delta = new { stop_reason = "end_turn" },
                usage = new { output_tokens = 7 },
            })
            + Frame("message_stop", new { type = "message_stop" });
        using MemoryStream stream = Utf8Stream(events);

        AnthropicCharacterModelSseResult result =
            await AnthropicCharacterModelSseCompletion.CompleteWithUsageAsync(request, stream);

        Assert.Equal(CharacterModelStreamTerminationStatus.Completed, result.Termination.Status);
        Assert.Same(request, result.Termination.Response?.Request);
        Assert.Equal(11, result.InputTokenCount);
        Assert.Equal(7, result.OutputTokenCount);
        Assert.Equal(18, result.TotalTokenCount);
        Assert.True(stream.CanRead);
    }

    [Theory]
    [InlineData("")]
    [InlineData(": ping\n\n")]
    [InlineData("data: {\"type\":\"message_stop\"}\n\n")]
    [InlineData("event: message_start\ndata: not-json\n\n")]
    [InlineData("event: message_start\ndata: {}")]
    public async Task MapsIncompleteMalformedAndTrailingInputToInvalidResponse(string events)
    {
        using MemoryStream stream = Utf8Stream(events);

        CharacterModelStreamTermination result =
            await AnthropicCharacterModelSseCompletion.CompleteAsync(CreateRequest(), stream);

        Assert.Equal(CharacterModelFailureCode.InvalidResponse, result.Failure?.Code);
        Assert.Null(result.Response);
    }

    [Fact]
    public async Task MapsIoFailureToTransportWithoutRetainingProviderDetails()
    {
        using ThrowingReadStream stream = new("secret transport detail");

        CharacterModelStreamTermination result =
            await AnthropicCharacterModelSseCompletion.CompleteAsync(CreateRequest(), stream);

        Assert.Equal(CharacterModelFailureCode.Transport, result.Failure?.Code);
        Assert.DoesNotContain("secret", result.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task PropagatesCallerCancellationAndRejectsNulls()
    {
        CharacterTurnRequest request = CreateRequest();
        using MemoryStream stream = Utf8Stream(string.Empty);
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            AnthropicCharacterModelSseCompletion.CompleteAsync(
                request,
                stream,
                cancellation.Token));
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            AnthropicCharacterModelSseCompletion.CompleteAsync(null!, stream));
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            AnthropicCharacterModelSseCompletion.CompleteAsync(request, null!));
    }

    private static string Frame(string eventName, object semanticEvent) =>
        $"event: {eventName}\ndata: {JsonSerializer.Serialize(semanticEvent)}\n\n";

    private static MemoryStream Utf8Stream(string value) =>
        new(Encoding.UTF8.GetBytes(value));

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
