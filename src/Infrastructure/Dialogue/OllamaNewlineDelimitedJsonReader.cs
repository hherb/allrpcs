using System.Runtime.CompilerServices;
using System.Text;

namespace Allrpcs.Infrastructure.Dialogue;

/// <summary>Reads bounded newline-delimited JSON from a caller-owned UTF-8 stream.</summary>
public static class OllamaNewlineDelimitedJsonReader
{
    /// <summary>Gets the maximum accepted length of one decoded line.</summary>
    public const int MaximumLineLength = 64_000;

    /// <summary>Gets the maximum accepted length of one JSON document.</summary>
    public const int MaximumDocumentLength = MaximumLineLength;

    /// <summary>Gets the maximum accepted number of JSON documents.</summary>
    public const int MaximumDocumentCount = 8_192;

    /// <summary>Reads complete non-blank documents in source order without closing the stream.</summary>
    public static async Task<IReadOnlyList<string>> ReadAsync(
        Stream stream, CancellationToken cancellationToken = default)
    {
        List<string> documents = [];
        await foreach (string document in ReadDocumentsAsync(stream, cancellationToken)
                .ConfigureAwait(false))
        {
            documents.Add(document);
        }
        return documents.AsReadOnly();
    }

    /// <summary>Yields each terminated document without closing the supplied stream.</summary>
    public static async IAsyncEnumerable<string> ReadDocumentsAsync(
        Stream stream,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        if (!stream.CanRead)
        {
            throw new ArgumentException("The NDJSON stream must be readable.", nameof(stream));
        }

        cancellationToken.ThrowIfCancellationRequested();

        int documentCount = 0;
        int totalLength = 0;
        StringBuilder line = new();
        bool awaitsLineFeed = false;

        IncrementalUtf8Reader reader = new(stream);

        while (true)
        {
            int nextCharacter = await reader.ReadAsync(cancellationToken).ConfigureAwait(false);
            if (nextCharacter == -1)
            {
                break;
            }

            if (++totalLength > OpenAiServerSentEventReader.MaximumStreamLength)

            {

                throw InvalidDocument();

            }
            char character = (char)nextCharacter;
            if (character == '\0')
            {
                throw InvalidDocument();
            }

            if (awaitsLineFeed)
            {
                if (character != '\n')
                {
                    throw InvalidDocument();
                }

                awaitsLineFeed = false;
                yield return TerminateLine();
                continue;
            }

            if (character == '\r')
            {
                awaitsLineFeed = true;
            }
            else if (character == '\n')
            {
                yield return TerminateLine();
            }
            else
            {
                if (line.Length == MaximumLineLength)
                {
                    throw InvalidDocument();
                }

                line.Append(character);
            }
        }
        if (awaitsLineFeed || line.Length != 0 || documentCount == 0)
        {
            throw InvalidDocument();
        }

        string TerminateLine()
        {
            if (line.Length == 0
                || line.Length > MaximumDocumentLength
                || IsWhiteSpaceOnly(line))
            {
                throw InvalidDocument();
            }

            if (documentCount == MaximumDocumentCount)
            {
                throw InvalidDocument();
            }

            string document = line.ToString();
            documentCount++;
            line.Clear();
            return document;
        }
    }

    private static bool IsWhiteSpaceOnly(StringBuilder value)
    {
        for (int index = 0; index < value.Length; index++)
        {
            if (!char.IsWhiteSpace(value[index]))
            {
                return false;
            }
        }

        return true;
    }

    private static InvalidDataException InvalidDocument() =>
        new("The NDJSON stream contains an invalid or excessive document.");
}
