using System.Text;
using Allrpcs.Infrastructure.Dialogue;

namespace Allrpcs.Infrastructure.Tests.Dialogue;

public sealed class OllamaNewlineDelimitedJsonReaderTests
{
    [Fact]
    public async Task ReadsCompleteDocumentsInSourceOrderAcrossLfAndCrLf()
    {
        using MemoryStream stream = Utf8Stream("{\"done\":false}\n{\"done\":true}\r\n");

        IReadOnlyList<string> documents =
            await OllamaNewlineDelimitedJsonReader.ReadAsync(stream);

        Assert.Equal(["{\"done\":false}", "{\"done\":true}"], documents);
        Assert.True(stream.CanRead);
    }

    [Fact]
    public async Task HonorsCallerCancellation()
    {
        using MemoryStream stream = Utf8Stream("{}\n");
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            OllamaNewlineDelimitedJsonReader.ReadAsync(stream, cancellation.Token));
    }

    [Theory]
    [InlineData("")]
    [InlineData("{}")]
    [InlineData("{}\n\n")]
    [InlineData("\n")]
    [InlineData(" \t\n")]
    [InlineData("{}\r")]
    [InlineData("{}\r{}\n")]
    public async Task RejectsEmptyBlankTruncatedAndMalformedLineEndings(string input)
    {
        using MemoryStream stream = Utf8Stream(input);

        await Assert.ThrowsAsync<InvalidDataException>(() =>
            OllamaNewlineDelimitedJsonReader.ReadAsync(stream));
    }

    [Fact]
    public async Task RejectsEmbeddedNullAndInvalidUtf8()
    {
        using MemoryStream embeddedNull = Utf8Stream("{\"value\":\"a\0b\"}\n");
        using MemoryStream invalidUtf8 = new([0x7b, 0x22, 0xff, 0x22, 0x7d, 0x0a]);

        await Assert.ThrowsAsync<InvalidDataException>(() =>
            OllamaNewlineDelimitedJsonReader.ReadAsync(embeddedNull));
        await Assert.ThrowsAsync<InvalidDataException>(() =>
            OllamaNewlineDelimitedJsonReader.ReadAsync(invalidUtf8));
    }

    [Fact]
    public async Task EnforcesExactDocumentAndLineBounds()
    {
        string exactDocument = new(
            'a',
            OllamaNewlineDelimitedJsonReader.MaximumDocumentLength);
        using MemoryStream exact = Utf8Stream(exactDocument + "\n");
        using MemoryStream excessiveDocument = Utf8Stream(exactDocument + "a\n");

        IReadOnlyList<string> documents =
            await OllamaNewlineDelimitedJsonReader.ReadAsync(exact);

        Assert.Equal(exactDocument, Assert.Single(documents));
        await Assert.ThrowsAsync<InvalidDataException>(() =>
            OllamaNewlineDelimitedJsonReader.ReadAsync(excessiveDocument));
    }

    [Fact]
    public async Task EnforcesExactDocumentCountBound()
    {
        string exactInput = string.Concat(Enumerable.Repeat(
            "{}\n",
            OllamaNewlineDelimitedJsonReader.MaximumDocumentCount));
        using MemoryStream exact = Utf8Stream(exactInput);
        using MemoryStream excessive = Utf8Stream(exactInput + "{}\n");

        Assert.Equal(
            OllamaNewlineDelimitedJsonReader.MaximumDocumentCount,
            (await OllamaNewlineDelimitedJsonReader.ReadAsync(exact)).Count);
        await Assert.ThrowsAsync<InvalidDataException>(() =>
            OllamaNewlineDelimitedJsonReader.ReadAsync(excessive));
    }

    [Fact]
    public async Task RejectsNullOrUnreadableStreams()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            OllamaNewlineDelimitedJsonReader.ReadAsync(null!));
        await Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            using MemoryStream stream = Utf8Stream("{}\n");
            stream.Close();
            await OllamaNewlineDelimitedJsonReader.ReadAsync(stream);
        });
    }

    private static MemoryStream Utf8Stream(string value) =>
        new(Encoding.UTF8.GetBytes(value));
}
