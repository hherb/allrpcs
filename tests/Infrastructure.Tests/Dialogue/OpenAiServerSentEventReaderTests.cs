using System.Text;
using Allrpcs.Infrastructure.Dialogue;

namespace Allrpcs.Infrastructure.Tests.Dialogue;

public sealed class OpenAiServerSentEventReaderTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task BoundsEncodedBytesBeforeDecodedCharacterLimit(bool ndjson)
    {
        string line = ndjson ? "{\"text\":\"" + new string('語', 1_024) + "\"}\n"
            : ":" + new string('語', 1_024) + "\n";
        using MemoryStream excessive = Utf8Stream(string.Concat(Enumerable.Repeat(line, 3_000)));
        await Assert.ThrowsAsync<InvalidDataException>(async () =>
        {
            if (ndjson) { await OllamaNewlineDelimitedJsonReader.ReadAsync(excessive); }
            else { await OpenAiServerSentEventReader.ReadAsync(excessive); }
        });
        Assert.True(excessive.Position <= 8_388_608 + 4_096);
        Assert.True(excessive.Position < excessive.Length);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task EnforcesFiniteTotalConsumptionAndPreservesSplitUtf8(bool ndjson)
    {
        using SplitReadStream split = new(Encoding.UTF8.GetBytes(ndjson ? "{\"text\":\"語😀\"}\n" : "data: 語😀\n\n"));
        IReadOnlyList<string> documents = ndjson
            ? await OllamaNewlineDelimitedJsonReader.ReadAsync(split)
            : await OpenAiServerSentEventReader.ReadAsync(split);
        Assert.Contains("語😀", Assert.Single(documents), StringComparison.Ordinal);
        string line = ndjson ? new string(' ', 1_024) + "{}\n" : ":" + new string('x', 1_024) + "\n";
        using MemoryStream excessive = Utf8Stream(string.Concat(Enumerable.Repeat(line, 4_100)));
        await Assert.ThrowsAsync<InvalidDataException>(async () =>
        {
            if (ndjson) { await OllamaNewlineDelimitedJsonReader.ReadAsync(excessive); }
            else { await OpenAiServerSentEventReader.ReadAsync(excessive); }
        });
        Assert.True(excessive.Position <= OpenAiServerSentEventReader.MaximumStreamLength + 4_096);
    }

    private sealed class SplitReadStream(byte[] bytes) : MemoryStream(bytes)
    {
        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) =>
            base.ReadAsync(buffer[..Math.Min(buffer.Length, 1)], cancellationToken);
    }

    [Fact]
    public async Task AcceptsOutputTokenBudgetWithLifecycleMargin()
    {
        using MemoryStream stream = Utf8Stream(string.Concat(Enumerable.Repeat("data: {}\n\n", 4_100)));
        Assert.Equal(4_100, (await OpenAiServerSentEventReader.ReadAsync(stream)).Count);
    }

    [Fact]
    public async Task ReadsCompleteDocumentsInSourceOrderAcrossLfAndCrLfFrames()
    {
        const string events =
            "event: response.created\n"
            + "data: {\"type\":\"response.created\"}\n"
            + "\n"
            + "event: response.output_text.delta\r\n"
            + "data: {\"type\":\"response.output_text.delta\",\r\n"
            + "data: \"delta\":\"hello\"}\r\n"
            + "\r\n";
        using MemoryStream stream = Utf8Stream(events);

        IReadOnlyList<string> documents =
            await OpenAiServerSentEventReader.ReadAsync(stream);

        Assert.Equal(
            [
                "{\"type\":\"response.created\"}",
                "{\"type\":\"response.output_text.delta\",\n\"delta\":\"hello\"}",
            ],
            documents);
    }

    [Fact]
    public async Task IgnoresTerminatedCommentHeartbeatsAndOneOptionalValueSpace()
    {
        const string events =
            ": keep-alive\n\n"
            + ":keep-alive\r\n\r\n"
            + "data:{\"type\":\"response.created\"}\n\n";
        using MemoryStream stream = Utf8Stream(events);

        IReadOnlyList<string> documents =
            await OpenAiServerSentEventReader.ReadAsync(stream);

        Assert.Equal(["{\"type\":\"response.created\"}"], documents);
    }

    [Fact]
    public async Task LeavesTheCallerOwnedStreamOpen()
    {
        using MemoryStream stream = Utf8Stream("data: {}\n\n");

        await OpenAiServerSentEventReader.ReadAsync(stream);

        Assert.True(stream.CanRead);
    }

    [Fact]
    public async Task HonorsCallerCancellation()
    {
        using MemoryStream stream = Utf8Stream("data: {}\n\n");
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => OpenAiServerSentEventReader.ReadAsync(stream, cancellation.Token));
    }

    [Theory]
    [InlineData("event: response.created\n\n")]
    [InlineData("unknown: value\ndata: {}\n\n")]
    [InlineData("event response.created\ndata: {}\n\n")]
    [InlineData("event: first\nevent: second\ndata: {}\n\n")]
    [InlineData(" \ndata: {}\n\n")]
    [InlineData("data: {}\rdata: {}\n\n")]
    public async Task RejectsMissingDataAndMalformedFields(string events)
    {
        using MemoryStream stream = Utf8Stream(events);

        await Assert.ThrowsAsync<InvalidDataException>(
            () => OpenAiServerSentEventReader.ReadAsync(stream));
    }

    [Theory]
    [InlineData("data: {\"type\":\"response.created\"}")]
    [InlineData("event: response.created\n")]
    [InlineData(": incomplete heartbeat")]
    public async Task RejectsTruncatedFinalFrames(string events)
    {
        using MemoryStream stream = Utf8Stream(events);

        await Assert.ThrowsAsync<InvalidDataException>(
            () => OpenAiServerSentEventReader.ReadAsync(stream));
    }

    [Fact]
    public async Task RejectsEmbeddedNullAndInvalidUtf8()
    {
        using MemoryStream embeddedNull = Utf8Stream("data: {\0}\n\n");
        using MemoryStream invalidUtf8 = new([0x64, 0x61, 0x74, 0x61, 0x3a, 0x20, 0xff]);

        await Assert.ThrowsAsync<InvalidDataException>(
            () => OpenAiServerSentEventReader.ReadAsync(embeddedNull));
        await Assert.ThrowsAsync<InvalidDataException>(
            () => OpenAiServerSentEventReader.ReadAsync(invalidUtf8));
    }

    [Fact]
    public async Task AcceptsExactDocumentAndLineBoundsAndRejectsExcess()
    {
        string exactDocument = new(
            'a',
            OpenAiServerSentEventReader.MaximumDocumentLength);
        string exactLine = "data: " + exactDocument;
        using MemoryStream exact = Utf8Stream(exactLine + "\n\n");
        using MemoryStream excessiveDocument = Utf8Stream(exactLine + "a\n\n");

        IReadOnlyList<string> documents =
            await OpenAiServerSentEventReader.ReadAsync(exact);

        Assert.Equal(exactDocument, Assert.Single(documents));
        await Assert.ThrowsAsync<InvalidDataException>(
            () => OpenAiServerSentEventReader.ReadAsync(excessiveDocument));
    }

    [Fact]
    public async Task EnforcesTheExactLineCountBound()
    {
        string exactInput = string.Concat(
            Enumerable.Repeat(": comment\n", OpenAiServerSentEventReader.MaximumLineCount - 1))
            + "\n";
        string excessiveInput = exactInput + "\n";
        using MemoryStream exact = Utf8Stream(exactInput);
        using MemoryStream excessive = Utf8Stream(excessiveInput);

        Assert.Empty(await OpenAiServerSentEventReader.ReadAsync(exact));
        await Assert.ThrowsAsync<InvalidDataException>(
            () => OpenAiServerSentEventReader.ReadAsync(excessive));
    }

    [Fact]
    public async Task EnforcesTheExactLineLengthBound()
    {
        string exactLine = ":" + new string(
            'a',
            OpenAiServerSentEventReader.MaximumLineLength - 1);
        using MemoryStream exact = Utf8Stream(exactLine + "\n\n");
        using MemoryStream excessive = Utf8Stream(exactLine + "a\n\n");

        Assert.Empty(await OpenAiServerSentEventReader.ReadAsync(exact));
        await Assert.ThrowsAsync<InvalidDataException>(
            () => OpenAiServerSentEventReader.ReadAsync(excessive));
    }

    [Fact]
    public async Task EnforcesEventAndDocumentCountBounds()
    {
        string exactEvents = string.Concat(
            Enumerable.Repeat(": heartbeat\n\n", OpenAiServerSentEventReader.MaximumEventCount));
        string excessiveEvents = exactEvents + ": heartbeat\n\n";
        string exactDocuments = string.Concat(
            Enumerable.Repeat("data: {}\n\n", OpenAiServerSentEventReader.MaximumDocumentCount));
        string excessiveDocuments = exactDocuments + "data: {}\n\n";
        using MemoryStream exactEventStream = Utf8Stream(exactEvents);
        using MemoryStream excessiveEventStream = Utf8Stream(excessiveEvents);
        using MemoryStream exactDocumentStream = Utf8Stream(exactDocuments);
        using MemoryStream excessiveDocumentStream = Utf8Stream(excessiveDocuments);

        Assert.Empty(await OpenAiServerSentEventReader.ReadAsync(exactEventStream));
        await Assert.ThrowsAsync<InvalidDataException>(
            () => OpenAiServerSentEventReader.ReadAsync(excessiveEventStream));
        Assert.Equal(
            OpenAiServerSentEventReader.MaximumDocumentCount,
            (await OpenAiServerSentEventReader.ReadAsync(exactDocumentStream)).Count);
        await Assert.ThrowsAsync<InvalidDataException>(
            () => OpenAiServerSentEventReader.ReadAsync(excessiveDocumentStream));
    }

    [Fact]
    public async Task RejectsExcessiveLinesEventsAndDocuments()
    {
        string excessiveLines = string.Concat(
            Enumerable.Repeat(
                ": heartbeat\n",
                OpenAiServerSentEventReader.MaximumLineCount + 1));
        string excessiveEvents = string.Concat(
            Enumerable.Repeat(
                ": heartbeat\n\n",
                OpenAiServerSentEventReader.MaximumEventCount + 1));
        string excessiveDocuments = string.Concat(
            Enumerable.Repeat(
                "data: {}\n\n",
                OpenAiServerSentEventReader.MaximumDocumentCount + 1));

        await AssertInvalidAsync(excessiveLines);
        await AssertInvalidAsync(excessiveEvents);
        await AssertInvalidAsync(excessiveDocuments);
    }

    [Fact]
    public async Task RejectsNullOrUnreadableStreams()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => OpenAiServerSentEventReader.ReadAsync(null!));
        await Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            using MemoryStream stream = Utf8Stream(string.Empty);
            stream.Close();
            await OpenAiServerSentEventReader.ReadAsync(stream);
        });
    }

    private static async Task AssertInvalidAsync(string events)
    {
        using MemoryStream stream = Utf8Stream(events);
        await Assert.ThrowsAsync<InvalidDataException>(
            () => OpenAiServerSentEventReader.ReadAsync(stream));
    }

    private static MemoryStream Utf8Stream(string value) =>
        new(Encoding.UTF8.GetBytes(value));
}
