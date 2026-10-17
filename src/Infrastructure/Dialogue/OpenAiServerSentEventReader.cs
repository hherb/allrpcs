using System.Runtime.CompilerServices;
using System.Text;

namespace Allrpcs.Infrastructure.Dialogue;

/// <summary>
/// Reads bounded OpenAI server-sent-event frames from a caller-owned UTF-8 stream.
/// </summary>
public static class OpenAiServerSentEventReader
{
    /// <summary>Gets the maximum decoded character budget for one stream.</summary>
    public const int MaximumStreamLength = 4_194_304;

    /// <summary>Gets the maximum accepted length of one decoded SSE line.</summary>
    public const int MaximumLineLength = 65_536;

    /// <summary>Gets the maximum accepted number of lines in one stream.</summary>
    public const int MaximumLineCount = 32_768;

    /// <summary>Gets the maximum accepted number of terminated SSE frames.</summary>
    public const int MaximumEventCount = 8_192;

    /// <summary>Gets the maximum number of returned data documents.</summary>
    public const int MaximumDocumentCount = 8_192;

    /// <summary>Gets the maximum accepted length of one joined data document.</summary>
    public const int MaximumDocumentLength =
        OpenAiCharacterModelStreamAccumulator.MaximumEventLength;

    /// <summary>
    /// Reads complete SSE data documents in source order without closing the supplied stream.
    /// </summary>
    public static async Task<IReadOnlyList<string>> ReadAsync(
        Stream stream,
        CancellationToken cancellationToken = default)
    {
        List<string> documents = [];
        await foreach (string document in ReadDocumentsAsync(stream, cancellationToken)
                .ConfigureAwait(false))
        {
            documents.Add(document);
        }
        return documents.AsReadOnly();
    }

    /// <summary>Yields each document as soon as its frame terminates, leaving the stream open.</summary>
    public static async IAsyncEnumerable<string> ReadDocumentsAsync(
        Stream stream,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        if (!stream.CanRead)
        {
            throw new ArgumentException("The SSE stream must be readable.", nameof(stream));
        }

        cancellationToken.ThrowIfCancellationRequested();

        int documentCount = 0;
        string? pendingDocument = null;
        int totalLength = 0;
        StringBuilder line = new();
        StringBuilder data = new();
        bool frameHasContent = false;
        bool hasData = false;
        bool hasEventName = false;
        bool awaitsLineFeed = false;
        int lineCount = 0;
        int eventCount = 0;

        IncrementalUtf8Reader reader = new(stream);

        while (true)
        {
            int nextCharacter = await reader.ReadAsync(cancellationToken).ConfigureAwait(false);
            if (nextCharacter == -1)
            {
                break;
            }

            if (++totalLength > MaximumStreamLength)

            {

                throw InvalidFrame();

            }
            char character = (char)nextCharacter;
            if (character == '\0')
            {
                throw InvalidFrame();
            }

            if (awaitsLineFeed)
            {
                if (character != '\n')
                {
                    throw InvalidFrame();
                }

                awaitsLineFeed = false;
                ProcessLine();
                if (pendingDocument is not null)
                {
                    yield return pendingDocument;
                    pendingDocument = null;
                }
                continue;
            }

            if (character == '\r')
            {
                awaitsLineFeed = true;
            }
            else if (character == '\n')
            {
                ProcessLine();
                if (pendingDocument is not null)
                {
                    yield return pendingDocument;
                    pendingDocument = null;
                }
            }
            else
            {
                if (line.Length == MaximumLineLength)
                {
                    throw InvalidFrame();
                }

                line.Append(character);
            }
        }
        if (awaitsLineFeed || line.Length != 0 || frameHasContent)
        {
            throw InvalidFrame();
        }

        void ProcessLine()
        {
            lineCount++;
            if (lineCount > MaximumLineCount)
            {
                throw InvalidFrame();
            }

            if (line.Length == 0)
            {
                TerminateFrame();
                return;
            }

            frameHasContent = true;
            if (line[0] == ':')
            {
                line.Clear();
                return;
            }

            int separatorIndex = IndexOfColon(line);
            if (separatorIndex <= 0)
            {
                throw InvalidFrame();
            }

            string fieldName = line.ToString(0, separatorIndex);
            int valueIndex = separatorIndex + 1;
            if (valueIndex < line.Length && line[valueIndex] == ' ')
            {
                valueIndex++;
            }

            if (fieldName == "event")
            {
                if (hasEventName || valueIndex == line.Length)
                {
                    throw InvalidFrame();
                }

                hasEventName = true;
            }
            else if (fieldName == "data")
            {
                int valueLength = line.Length - valueIndex;
                int separatorLength = hasData ? 1 : 0;
                if (data.Length + separatorLength + valueLength > MaximumDocumentLength)
                {
                    throw InvalidFrame();
                }

                if (hasData)
                {
                    data.Append('\n');
                }

                for (int index = valueIndex; index < line.Length; index++)
                {
                    data.Append(line[index]);
                }

                hasData = true;
            }
            else
            {
                throw InvalidFrame();
            }

            line.Clear();
        }

        void TerminateFrame()
        {
            line.Clear();
            if (!frameHasContent)
            {
                return;
            }

            eventCount++;
            if (eventCount > MaximumEventCount)
            {
                throw InvalidFrame();
            }

            if (!hasData)
            {
                if (hasEventName)
                {
                    throw InvalidFrame();
                }
            }
            else
            {
                if (documentCount == MaximumDocumentCount)
                {
                    throw InvalidFrame();
                }

                pendingDocument = data.ToString();
                documentCount++;
            }

            data.Clear();
            frameHasContent = false;
            hasData = false;
            hasEventName = false;
        }
    }

    private static int IndexOfColon(StringBuilder value)
    {
        for (int index = 0; index < value.Length; index++)
        {
            if (value[index] == ':')
            {
                return index;
            }
        }

        return -1;
    }

    private static InvalidDataException InvalidFrame() =>
        new("The SSE stream contains an invalid or excessive frame.");
}
