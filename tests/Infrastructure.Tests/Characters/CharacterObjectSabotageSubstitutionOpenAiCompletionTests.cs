using System.Text;
using System.Text.Json;
using Allrpcs.Domain.Characters;
using Allrpcs.Infrastructure.Characters;
using static Allrpcs.Infrastructure.Tests.StructuredDecisionTestModels;

namespace Allrpcs.Infrastructure.Tests.Characters;

public sealed class CharacterObjectSabotageSubstitutionOpenAiCompletionTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CompletesTheExactDecisionFromFramedSemanticEvents(bool abstain)
    {
        CharacterObjectSabotageSubstitutionConsiderationRequest request =
            CharacterObjectSabotageSubstitutionOpenAiRequestTests.CreateRequest();
        string? evidenceId = abstain
            ? null
            : Assert.Single(request.Candidates).Evidence.Evidence.EvidenceId.Value;
        string document = CharacterObjectSabotageSubstitutionOpenAiStreamTests.DecisionDocument(
            request.Character.Identity.Id.Value,
            evidenceId);
        string events = Frame(new { type = "response.created" })
            + Frame(new { type = "response.output_text.delta", delta = document })
            + Frame(new { type = "response.output_text.done", text = document })
            + Frame(new { type = "response.completed" });
        using MemoryStream stream = Utf8Stream(events);

        CharacterObjectSabotageSubstitutionDecision decision =
            await CompleteCharacterObjectSabotageSubstitutionAsync(request, stream);

        Assert.Same(request.Character.Identity.Id, decision.CharacterId);
        Assert.Equal(evidenceId, decision.SelectedEvidenceId?.Value);
        Assert.True(stream.CanRead);
    }

    [Theory]
    [InlineData("")]
    [InlineData(": heartbeat\n\n")]
    [InlineData("data: {\"type\":\"response.created\"}\n\n")]
    [InlineData("data: not-json\n\n")]
    [InlineData("data: {\"type\":\"response.completed\"}\n\n")]
    [InlineData("data: {\"type\":\"response.failed\",\"detail\":\"secret\"}\n\n")]
    [InlineData("data: {}")]
    public async Task RejectsIncompleteMalformedFailedAndUnframedInput(string events)
    {
        using MemoryStream stream = Utf8Stream(events);

        InvalidDataException exception = await Assert.ThrowsAsync<InvalidDataException>(() =>
            CompleteCharacterObjectSabotageSubstitutionAsync(
                CharacterObjectSabotageSubstitutionOpenAiRequestTests.CreateRequest(),
                stream));

        Assert.DoesNotContain("secret", exception.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task ReturnsAtCompletionBeforeTrailingEvents()
    {
        CharacterObjectSabotageSubstitutionConsiderationRequest request =
            CharacterObjectSabotageSubstitutionOpenAiRequestTests.CreateRequest();
        string document = CharacterObjectSabotageSubstitutionOpenAiStreamTests.DecisionDocument(
            request.Character.Identity.Id.Value,
            null);
        string events = Frame(new { type = "response.output_text.done", text = document })
            + Frame(new { type = "response.completed" })
            + Frame(new { type = "response.created" });
        using MemoryStream stream = Utf8Stream(events);

        Assert.NotNull(await CompleteCharacterObjectSabotageSubstitutionAsync(request, stream));
    }

    [Fact]
    public async Task PreservesIoFailureAndCallerCancellationAndRejectsNulls()
    {
        CharacterObjectSabotageSubstitutionConsiderationRequest request =
            CharacterObjectSabotageSubstitutionOpenAiRequestTests.CreateRequest();
        using ThrowingReadStream throwing = new("secret transport detail");
        using MemoryStream empty = Utf8Stream(string.Empty);
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();

        IOException exception = await Assert.ThrowsAsync<IOException>(() =>
            CompleteCharacterObjectSabotageSubstitutionAsync(request, throwing));
        Assert.Equal("secret transport detail", exception.Message);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            CompleteCharacterObjectSabotageSubstitutionAsync(
                request,
                empty,
                cancellation.Token));
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            CompleteCharacterObjectSabotageSubstitutionAsync(null!, empty));
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            CompleteCharacterObjectSabotageSubstitutionAsync(request, null!));
    }



    private static string Frame(object semanticEvent) =>
        "data: " + JsonSerializer.Serialize(semanticEvent) + "\n\n";

    private static MemoryStream Utf8Stream(string value) =>
        new(Encoding.UTF8.GetBytes(value));

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
