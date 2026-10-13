using System.Buffers.Binary;
using System.Text.Json;
using Allrpcs.Application.Speech;

namespace Allrpcs.Infrastructure.Speech;

/// <summary>Decodes bounded worker PCM and rejects corruption before it reaches playback.</summary>
internal static class PocketSpeechProtocol
{
    internal const int Version = 1;
    internal const int MaximumLineLength = 270000;

    internal static string StringField(JsonElement root, string name)
    {
        try
        {
            return root.GetProperty(name).GetString() ?? throw new SpeechSynthesisException("Speech worker field is null.");
        }
        catch (Exception exception) when (exception is KeyNotFoundException or InvalidOperationException)
        {
            throw new SpeechSynthesisException("Speech worker field is missing or malformed.");
        }
    }

    internal static int IntField(JsonElement root, string name)
    {
        try
        {
            return root.GetProperty(name).GetInt32();
        }
        catch (Exception exception) when (exception is KeyNotFoundException or InvalidOperationException or FormatException)
        {
            throw new SpeechSynthesisException("Speech worker field is missing or malformed.");
        }
    }

    internal static JsonDocument Parse(string? message)
    {
        if (message is null || message.Length > MaximumLineLength)
        {
            throw new SpeechSynthesisException("Speech worker output is missing or oversized.");
        }
        return JsonDocument.Parse(message, new JsonDocumentOptions { MaxDepth = 8 });
    }

    internal static SpeechAudioChunk DecodeAudio(JsonElement root, int expectedSequence)
    {
        int rate = root.GetProperty("sampleRate").GetInt32();
        int samples = root.GetProperty("samples").GetInt32();
        if (root.GetProperty("sequence").GetInt32() != expectedSequence || rate != 24000 ||
            samples is < 1 or > SpeechAudioChunk.MaximumSamples)
        {
            throw new SpeechSynthesisException("Speech worker PCM metadata is invalid.");
        }
        byte[] bytes = Convert.FromBase64String(root.GetProperty("pcm").GetString()!);
        if (bytes.Length != samples * sizeof(float))
        {
            throw new SpeechSynthesisException("Speech worker PCM length is invalid.");
        }
        float[] values = new float[samples];
        for (int index = 0; index < samples; index++)
        {
            values[index] = BinaryPrimitives.ReadSingleLittleEndian(bytes.AsSpan(index * sizeof(float), sizeof(float)));
        }
        return new SpeechAudioChunk(rate, values);
    }
}
