using System.Collections.ObjectModel;
using System.Text;

namespace Allrpcs.Infrastructure.Dialogue;

/// <summary>Reads bounded newline-delimited JSON from a caller-owned UTF-8 stream.</summary>
public static class OllamaNewlineDelimitedJsonReader
{
    private const int ReadBufferSize = 4_096;

    /// <summary>Gets the maximum accepted length of one decoded line.</summary>
    public const int MaximumLineLength = 64_000;

    /// <summary>Gets the maximum accepted length of one JSON document.</summary>
    public const int MaximumDocumentLength = MaximumLineLength;

    /// <summary>Gets the maximum accepted number of JSON documents.</summary>
    public const int MaximumDocumentCount = 1_024;

    /// <summary>Reads complete non-blank documents in source order without closing the stream.</summary>
    public static async Task<IReadOnlyList<string>> ReadAsync(
        Stream stream,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        if (!stream.CanRead)
        {
            throw new ArgumentException("The NDJSON stream must be readable.", nameof(stream));
        }

        cancellationToken.ThrowIfCancellationRequested();

        List<string> documents = [];
        StringBuilder line = new();
        char[] buffer = new char[ReadBufferSize];
        bool awaitsLineFeed = false;

        using StreamReader reader = new(
            stream,
            new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true),
            detectEncodingFromByteOrderMarks: false,
            bufferSize: ReadBufferSize,
            leaveOpen: true);

        try
        {
            while (true)
            {
                int read = await reader.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
                if (read == 0)
                {
                    break;
                }

                for (int index = 0; index < read; index++)
                {
                    char character = buffer[index];
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
                        TerminateLine();
                        continue;
                    }

                    if (character == '\r')
                    {
                        awaitsLineFeed = true;
                    }
                    else if (character == '\n')
                    {
                        TerminateLine();
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
            }
        }
        catch (DecoderFallbackException exception)
        {
            throw new InvalidDataException("The NDJSON stream is not valid UTF-8.", exception);
        }

        if (awaitsLineFeed || line.Length != 0 || documents.Count == 0)
        {
            throw InvalidDocument();
        }

        return new ReadOnlyCollection<string>(documents);

        void TerminateLine()
        {
            if (line.Length == 0
                || line.Length > MaximumDocumentLength
                || IsWhiteSpaceOnly(line))
            {
                throw InvalidDocument();
            }

            if (documents.Count == MaximumDocumentCount)
            {
                throw InvalidDocument();
            }

            documents.Add(line.ToString());
            line.Clear();
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
