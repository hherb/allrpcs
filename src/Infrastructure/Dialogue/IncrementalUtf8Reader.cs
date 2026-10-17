using System.Text;

namespace Allrpcs.Infrastructure.Dialogue;

/// <summary>Decodes bounded UTF-8 on demand without interpreting a buffered terminal tail.</summary>
internal sealed class IncrementalUtf8Reader(Stream stream)
{
    internal const int MaximumByteCount = 8_388_608;
    private readonly byte[] bytes = new byte[4_096];
    private readonly char[] characters = new char[2];
    private readonly Decoder decoder = new UTF8Encoding(false, true).GetDecoder();
    private int byteIndex;
    private int byteCount;
    private int consumedBytes;
    private int characterIndex;
    private int characterCount;

    internal async ValueTask<int> ReadAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (characterIndex < characterCount)
        {
            return characters[characterIndex++];
        }
        try
        {
            while (true)
            {
                if (byteIndex == byteCount)
                {
                    byteCount = await stream.ReadAsync(bytes, cancellationToken).ConfigureAwait(false);
                    byteIndex = 0;
                    if (byteCount == 0)
                    {
                        decoder.Convert([], characters, true, out _, out int finalCount, out _);
                        if (finalCount != 0)
                        {
                            throw new InvalidDataException("Incomplete UTF-8 stream.");
                        }
                        return -1;
                    }
                }
                if (++consumedBytes > MaximumByteCount)
                {
                    throw new InvalidDataException("Excessive UTF-8 stream.");
                }
                decoder.Convert(bytes.AsSpan(byteIndex++, 1), characters, false,
                    out _, out characterCount, out _);
                characterIndex = 0;
                if (characterCount != 0)
                {
                    return characters[characterIndex++];
                }
            }
        }
        catch (DecoderFallbackException exception)
        {
            throw new InvalidDataException("The stream is not valid UTF-8.", exception);
        }
    }
}
