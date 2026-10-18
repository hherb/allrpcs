using System.Text;
using System.Text.Json;
using Allrpcs.Domain.Characters;
using Allrpcs.Infrastructure.Characters;

namespace Allrpcs.Infrastructure.Tests.Characters;

public sealed class OpenAiCharacterObjectSabotageSubstitutionSseCompletionTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CompletesTheExactDecisionFromFramedSemanticEvents(bool abstain)
    {
        CharacterObjectSabotageSubstitutionConsiderationRequest request =
            OpenAiCharacterObjectSabotageSubstitutionRequestSerializerTests.CreateRequest();
        string? evidenceId = abstain
            ? null
            : Assert.Single(request.Candidates).Evidence.Evidence.EvidenceId.Value;
        string document = OpenAiCharacterObjectSabotageSubstitutionStreamAccumulatorTests.DecisionDocument(
            request.Character.Identity.Id.Value,
            evidenceId);
        string events = Frame(new { type = "response.created" })
            + Frame(new { type = "response.output_text.delta", delta = document })
            + Frame(new { type = "response.output_text.done", text = document })
            + Frame(new { type = "response.completed" });
        using MemoryStream stream = Utf8Stream(events);

        CharacterObjectSabotageSubstitutionDecision decision =
            await OpenAiCharacterObjectSabotageSubstitutionSseCompletion.CompleteAsync(request, stream);

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
            OpenAiCharacterObjectSabotageSubstitutionSseCompletion.CompleteAsync(
                OpenAiCharacterObjectSabotageSubstitutionRequestSerializerTests.CreateRequest(),
                stream));

        Assert.DoesNotContain("secret", exception.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task ReturnsAtCompletionBeforeTrailingEvents()
    {
        CharacterObjectSabotageSubstitutionConsiderationRequest request =
            OpenAiCharacterObjectSabotageSubstitutionRequestSerializerTests.CreateRequest();
        string document = OpenAiCharacterObjectSabotageSubstitutionStreamAccumulatorTests.DecisionDocument(
            request.Character.Identity.Id.Value,
            null);
        string events = Frame(new { type = "response.output_text.done", text = document })
            + Frame(new { type = "response.completed" })
            + Frame(new { type = "response.created" });
        using MemoryStream stream = Utf8Stream(events);

        Assert.NotNull(await OpenAiCharacterObjectSabotageSubstitutionSseCompletion.CompleteAsync(request, stream));
    }

    [Fact]
    public async Task PreservesIoFailureAndCallerCancellationAndRejectsNulls()
    {
        CharacterObjectSabotageSubstitutionConsiderationRequest request =
            OpenAiCharacterObjectSabotageSubstitutionRequestSerializerTests.CreateRequest();
        using ThrowingReadStream throwing = new("secret transport detail");
        using MemoryStream empty = Utf8Stream(string.Empty);
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();

        IOException exception = await Assert.ThrowsAsync<IOException>(() =>
            OpenAiCharacterObjectSabotageSubstitutionSseCompletion.CompleteAsync(request, throwing));
        Assert.Equal("secret transport detail", exception.Message);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            OpenAiCharacterObjectSabotageSubstitutionSseCompletion.CompleteAsync(
                request,
                empty,
                cancellation.Token));
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            OpenAiCharacterObjectSabotageSubstitutionSseCompletion.CompleteAsync(null!, empty));
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            OpenAiCharacterObjectSabotageSubstitutionSseCompletion.CompleteAsync(request, null!));
    }

    [Fact]
    public void ExposesOnlyOneBoundedAsyncCompletionOperation()
    {
        Type completionType = typeof(OpenAiCharacterObjectSabotageSubstitutionSseCompletion);
        System.Reflection.MethodInfo method = Assert.Single(completionType.GetMethods().Where(
            candidate => candidate.DeclaringType == completionType));

        Assert.Equal("CompleteAsync", method.Name);
        Assert.True(method.IsStatic);
        Assert.Equal(typeof(Task<CharacterObjectSabotageSubstitutionDecision>), method.ReturnType);
        Assert.Equal(
            [
                typeof(CharacterObjectSabotageSubstitutionConsiderationRequest),
                typeof(Stream),
                typeof(CancellationToken),
            ],
            method.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Empty(completionType.GetConstructors());
        Assert.Empty(completionType.GetProperties());
        Assert.Empty(completionType.GetEvents());
        Assert.True(completionType.IsAbstract && completionType.IsSealed);
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
