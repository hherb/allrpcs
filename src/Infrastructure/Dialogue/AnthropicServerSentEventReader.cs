namespace Allrpcs.Infrastructure.Dialogue;

/// <summary>Reads bounded Anthropic Messages server-sent-event data documents.</summary>
public static class AnthropicServerSentEventReader
{
    /// <summary>Gets the maximum accepted length of one decoded SSE line.</summary>
    public const int MaximumLineLength = OpenAiServerSentEventReader.MaximumLineLength;

    /// <summary>Gets the maximum accepted number of lines in one stream.</summary>
    public const int MaximumLineCount = OpenAiServerSentEventReader.MaximumLineCount;

    /// <summary>Gets the maximum accepted number of terminated SSE frames.</summary>
    public const int MaximumEventCount = OpenAiServerSentEventReader.MaximumEventCount;

    /// <summary>Gets the maximum number of returned data documents.</summary>
    public const int MaximumDocumentCount = OpenAiServerSentEventReader.MaximumDocumentCount;

    /// <summary>Gets the maximum accepted length of one joined data document.</summary>
    public const int MaximumDocumentLength = OpenAiServerSentEventReader.MaximumDocumentLength;

    /// <summary>Reads complete SSE data documents without closing the supplied stream.</summary>
    public static Task<IReadOnlyList<string>> ReadAsync(
        Stream stream,
        CancellationToken cancellationToken = default) =>
        OpenAiServerSentEventReader.ReadAsync(stream, cancellationToken);
    /// <summary>Yields complete data documents without closing the stream.</summary>
    public static IAsyncEnumerable<string> ReadDocumentsAsync(
        Stream stream,
        CancellationToken cancellationToken = default) =>
        OpenAiServerSentEventReader.ReadDocumentsAsync(stream, cancellationToken);
}
