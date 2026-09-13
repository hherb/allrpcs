using System.Text;
using System.Text.Json;
using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Infrastructure.Dialogue;

namespace Allrpcs.Infrastructure.Tests.Dialogue;

public sealed class OllamaCharacterModelNdjsonCompletionTests
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
    };

    private const string ResponseDocument =
        "{\"disposition\":\"accept\",\"speech\":null,\"deliveryMode\":null,"
        + "\"immediateIntention\":{\"type\":\"wait\",\"targetObjectId\":null},"
        + "\"commitments\":[],\"stateProposals\":[]}";

    [Fact]
    public async Task CompletesTheExactRequestFromBoundedNativeNdjson()
    {
        CharacterTurnRequest request = CreateRequest();
        string ndjson = Line(ResponseDocument[..50], done: false)
            + Line(ResponseDocument[50..], done: false)
            + Line(string.Empty, done: true, "stop", 13, 5);
        using MemoryStream stream = Utf8Stream(ndjson);

        OllamaCharacterModelNdjsonResult result =
            await OllamaCharacterModelNdjsonCompletion.CompleteWithUsageAsync(request, stream);

        Assert.Equal(CharacterModelStreamTerminationStatus.Completed, result.Termination.Status);
        Assert.Same(request, result.Termination.Response?.Request);
        Assert.Equal(13, result.InputTokenCount);
        Assert.Equal(5, result.OutputTokenCount);
        Assert.Equal(18, result.TotalTokenCount);
        Assert.True(stream.CanRead);
    }

    [Theory]
    [InlineData("")]
    [InlineData("{}\n")]
    [InlineData("{\"error\":\"detail\"}")]
    [InlineData("{\"message\":{\"role\":\"assistant\",\"content\":\"x\"},\"done\":false}\n")]
    public async Task MapsEmptyMalformedTruncatedAndUnterminatedInputToInvalidResponse(
        string ndjson)
    {
        using MemoryStream stream = Utf8Stream(ndjson);

        CharacterModelStreamTermination result =
            await OllamaCharacterModelNdjsonCompletion.CompleteAsync(CreateRequest(), stream);

        Assert.Equal(CharacterModelFailureCode.InvalidResponse, result.Failure?.Code);
        Assert.Null(result.Response);
    }

    [Fact]
    public async Task RejectsDocumentsAfterTermination()
    {
        string ndjson = Line(ResponseDocument, done: true, "stop")
            + Line(string.Empty, done: true, "stop");
        using MemoryStream stream = Utf8Stream(ndjson);

        CharacterModelStreamTermination result =
            await OllamaCharacterModelNdjsonCompletion.CompleteAsync(CreateRequest(), stream);

        Assert.Equal(CharacterModelFailureCode.InvalidResponse, result.Failure?.Code);
        Assert.Null(result.Response);
    }

    [Fact]
    public async Task MapsIoFailureToTransportWithoutRetainingDetails()
    {
        using ThrowingReadStream stream = new("secret transport detail");

        CharacterModelStreamTermination result =
            await OllamaCharacterModelNdjsonCompletion.CompleteAsync(CreateRequest(), stream);

        Assert.Equal(CharacterModelFailureCode.Transport, result.Failure?.Code);
        Assert.DoesNotContain("secret", result.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task PropagatesCallerCancellationAndRejectsNulls()
    {
        CharacterTurnRequest request = CreateRequest();
        using MemoryStream stream = Utf8Stream("{}\n");
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            OllamaCharacterModelNdjsonCompletion.CompleteAsync(
                request,
                stream,
                cancellation.Token));
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            OllamaCharacterModelNdjsonCompletion.CompleteAsync(null!, stream));
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            OllamaCharacterModelNdjsonCompletion.CompleteAsync(request, null!));
    }

    private static string Line(
        string content,
        bool done,
        string? doneReason = null,
        int? promptCount = null,
        int? outputCount = null) => JsonSerializer.Serialize(new
        {
            message = new { role = "assistant", content },
            done,
            done_reason = doneReason,
            prompt_eval_count = promptCount,
            eval_count = outputCount,
        }, SerializerOptions) + "\n";

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
