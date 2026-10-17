using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Allrpcs.Application.Speech;

namespace Allrpcs.Infrastructure.Speech;

/// <summary>Local Small Streaming recognition; bounded IPC is independent of dialogue authority.</summary>
public sealed class MoonshineSpeechRecognizer(IPlayerSpeechWorkerFactory factory) : ISpeechRecognizer
{
    /// <inheritdoc />
    public async IAsyncEnumerable<SpeechRecognitionEvent> RecognizeAsync(SpeechLanguage language,
        IAsyncEnumerable<SpeechAudioChunk> input, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        if (!Enum.IsDefined(language)) { throw new ArgumentOutOfRangeException(nameof(language)); }
        using CancellationTokenSource lifetime = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        await using IPlayerSpeechWorker worker = await factory.StartAsync(false, language, lifetime.Token).ConfigureAwait(false);
        using (JsonDocument ready = await PlayerSpeechProtocol.Read(worker, lifetime.Token).ConfigureAwait(false))
        { if (ready.RootElement.GetProperty("type").GetString() != "ready") { throw new IOException("Recognizer unavailable."); } }
        Task sending = Send();
        try
        {
            int sequence = 0;
            while (true)
            {
                Task<JsonDocument> reading = PlayerSpeechProtocol.Read(worker, lifetime.Token);
                if (!sending.IsCompleted)
                { await Task.WhenAny(reading, sending).ConfigureAwait(false); }
                if (sending.IsFaulted || sending.IsCanceled) { await sending.ConfigureAwait(false); }
                using JsonDocument message = await reading.ConfigureAwait(false);
                JsonElement value = message.RootElement;
                string? kind = value.GetProperty("type").GetString();
                if (kind == "heartbeat") { continue; }
                if (kind == "error") { throw new IOException("Recognition unavailable."); }
                if (kind == "finished") { await sending.ConfigureAwait(false); yield break; }
                if (kind != "event" || value.GetProperty("sequence").GetInt32() != sequence++) { throw new IOException("Malformed recognition sequence."); }
                SpeechRecognitionKind eventKind = value.GetProperty("kind").GetString() switch
                {
                    "start" => SpeechRecognitionKind.VoiceStarted,
                    "partial" => SpeechRecognitionKind.Partial,
                    "final" => SpeechRecognitionKind.Final,
                    _ => throw new IOException("Unknown recognition event.")
                };
                yield return new(eventKind, value.GetProperty("utterance").GetString()!, value.GetProperty("text").GetString()!);
            }
        }
        finally
        {
            lifetime.Cancel();
            try { await sending.ConfigureAwait(false); }
            catch (Exception error) when (error is OperationCanceledException or IOException or InvalidOperationException) { }
        }
        async Task Send()
        {
            try
            {
                int sequence = 0;
                await foreach (SpeechAudioChunk chunk in input.WithCancellation(lifetime.Token).ConfigureAwait(false))
                {
                    await worker.SendAsync(PlayerSpeechProtocol.Audio(chunk, sequence++), lifetime.Token).ConfigureAwait(false);
                }
                await worker.SendAsync("{\"type\":\"finish\"}", lifetime.Token).ConfigureAwait(false);
            }
            catch { throw; }
        }
    }
}

/// <summary>Captures default-device PCM only during explicit enumeration; one device owner at a time.</summary>
public sealed class PortAudioMicrophoneCapture(IPlayerSpeechWorkerFactory factory) : IMicrophoneCapture
{
    private static readonly SemaphoreSlim Gate = new(1, 1);
    /// <inheritdoc />
    public async IAsyncEnumerable<SpeechAudioChunk> CaptureAsync([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await Gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            await using IPlayerSpeechWorker worker = await factory.StartAsync(true, SpeechLanguage.English, cancellationToken).ConfigureAwait(false);
            using (JsonDocument ready = await PlayerSpeechProtocol.Read(worker, cancellationToken).ConfigureAwait(false))
            { if (ready.RootElement.GetProperty("type").GetString() != "ready") { throw new IOException("Microphone unavailable."); } }
            await worker.SendAsync("{\"type\":\"start\"}", cancellationToken).ConfigureAwait(false);
            int sequence = 0;
            while (true)
            {
                using JsonDocument message = await PlayerSpeechProtocol.Read(worker, cancellationToken).ConfigureAwait(false);
                JsonElement value = message.RootElement;
                if (value.GetProperty("type").GetString() == "finished" && value.EnumerateObject().Count() == 1) { yield break; }
                if (value.GetProperty("type").GetString() != "audio" || value.GetProperty("sequence").GetInt32() != sequence++)
                { throw new IOException("Microphone unavailable; check permission/device."); }
                yield return PlayerSpeechProtocol.Decode(value);
            }
        }
        finally { Gate.Release(); }
    }
}

internal static class PlayerSpeechProtocol
{
    internal static async Task<JsonDocument> Read(IPlayerSpeechWorker worker, CancellationToken token, TimeProvider? clock = null)
    {
        using CancellationTokenSource timeout = new(TimeSpan.FromSeconds(10), clock ?? TimeProvider.System);
        using CancellationTokenSource deadline = CancellationTokenSource.CreateLinkedTokenSource(token, timeout.Token);
        string? line;
        try { line = await worker.ReceiveAsync(deadline.Token).ConfigureAwait(false); }
        catch (OperationCanceledException) when (!token.IsCancellationRequested && timeout.IsCancellationRequested)
        { throw new TimeoutException("Input speech worker response timed out."); }
        if (line is null || line.Length > 32000) { throw new IOException("Input speech worker unavailable."); }
        return JsonDocument.Parse(line);
    }
    internal static string Audio(SpeechAudioChunk chunk, int sequence)
    {
        if (chunk.Samples.Count > 4096) { throw new IOException("Input PCM exceeded frame bound."); }
        byte[] bytes = new byte[chunk.Samples.Count * 4];
        for (int index = 0; index < chunk.Samples.Count; index++)
        { BinaryPrimitives.WriteSingleLittleEndian(bytes.AsSpan(index * 4), chunk.Samples[index]); }
        return JsonSerializer.Serialize(new { type = "audio", sequence, sampleRate = chunk.SampleRate, samples = chunk.Samples.Count, pcm = Convert.ToBase64String(bytes) });
    }
    internal static SpeechAudioChunk Decode(JsonElement value)
    {
        int count = value.GetProperty("samples").GetInt32();
        if (count is < 1 or > 4096) { throw new IOException("Input PCM exceeded frame bound."); }
        byte[] bytes = Convert.FromBase64String(value.GetProperty("pcm").GetString()!);
        if (bytes.Length != count * 4) { throw new IOException("Input PCM length mismatch."); }
        float[] samples = new float[count];
        for (int index = 0; index < count; index++) { samples[index] = BinaryPrimitives.ReadSingleLittleEndian(bytes.AsSpan(index * 4)); }
        return new(value.GetProperty("sampleRate").GetInt32(), samples);
    }
}
