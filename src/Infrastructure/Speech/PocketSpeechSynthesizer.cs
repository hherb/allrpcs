using System.Runtime.CompilerServices;
using System.Text.Json;
using Allrpcs.Application.Speech;

namespace Allrpcs.Infrastructure.Speech;

/// <summary>Serializes speech requests through a reusable offline worker and discards obsolete PCM.</summary>
public sealed class PocketSpeechSynthesizer : ISpeechSynthesizer, IAsyncDisposable
{
    private readonly IPocketSpeechWorkerFactory _factory;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private readonly CancellationTokenSource _lifetime = new();
    private IPocketSpeechWorker? _worker;
    private bool _disposed;

    /// <summary>Creates the synthesizer with an injected local process boundary.</summary>
    public PocketSpeechSynthesizer(IPocketSpeechWorkerFactory factory)
    {
        ArgumentNullException.ThrowIfNull(factory);
        _factory = factory;
    }

    /// <summary>Streams only the current request; caller cancellation or disposal retires queued output.</summary>
    public async IAsyncEnumerable<SpeechAudioChunk> SynthesizeAsync(
        SpeechSynthesisRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ObjectDisposedException.ThrowIf(_disposed, this);
        using CancellationTokenSource linked = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken, _lifetime.Token);
        await _gate.WaitAsync(linked.Token).ConfigureAwait(false);
        string id = Guid.NewGuid().ToString("N");
        bool sent = false;
        bool completed = false;
        bool poisoned = true;
        try
        {
            await EnsureReadyAsync(linked.Token).ConfigureAwait(false);
            await SendAsync(JsonSerializer.Serialize(new
            {
                type = "synthesize",
                id,
                text = request.Text,
                language = request.Language == SpeechLanguage.English ? "en" : "de",
                voiceProfile = request.VoiceProfile,
            }), linked.Token).ConfigureAwait(false);
            sent = true;
            int sequence = 0;
            while (true)
            {
                linked.Token.ThrowIfCancellationRequested();
                poisoned = true;
                JsonDocument received;
                try { received = await ReceiveAsync(linked.Token).ConfigureAwait(false); }
                catch (OperationCanceledException) when (linked.IsCancellationRequested)
                {
                    poisoned = false;
                    throw;
                }
                using JsonDocument document = received;
                JsonElement root = document.RootElement;
                if (PocketSpeechProtocol.StringField(root, "id") != id)
                {
                    throw new SpeechSynthesisException("Speech worker returned an unrelated request.");
                }
                string type = PocketSpeechProtocol.StringField(root, "type");
                if (type == "audio")
                {
                    SpeechAudioChunk chunk = Decode(root, sequence++);
                    linked.Token.ThrowIfCancellationRequested();
                    poisoned = false;
                    yield return chunk;
                }
                else if (type == "finished" && PocketSpeechProtocol.StringField(root, "status") == "completed" && sequence > 0)
                {
                    completed = true;
                    yield break;
                }
                else
                {
                    if (type == "error" && !string.IsNullOrWhiteSpace(PocketSpeechProtocol.StringField(root, "code")))
                    {
                        // A request error is terminal: the worker will send no further completion.
                        completed = true;
                    }
                    throw new SpeechSynthesisException("Speech generation failed; text remains available.");
                }
            }
        }
        finally
        {
            if (!completed)
            {
                if (poisoned)
                {
                    await ResetAsync().ConfigureAwait(false);
                }
                else
                {
                    await RetireAsync(id, sent).ConfigureAwait(false);
                }
            }
            _gate.Release();
        }
    }

    private static SpeechAudioChunk Decode(JsonElement root, int sequence)
    {
        try
        {
            return PocketSpeechProtocol.DecodeAudio(root, sequence);
        }
        catch (Exception exception) when (exception is ArgumentException or FormatException or
            InvalidOperationException or KeyNotFoundException)
        {
            throw new SpeechSynthesisException("Speech worker PCM is malformed.");
        }
    }

    private async ValueTask<JsonDocument> ReceiveAsync(CancellationToken token)
    {
        try
        {
            using CancellationTokenSource deadline = CancellationTokenSource.CreateLinkedTokenSource(token);
            deadline.CancelAfter(TimeSpan.FromSeconds(30));
            return PocketSpeechProtocol.Parse(await _worker!.ReceiveAsync(deadline.Token).ConfigureAwait(false));
        }
        catch (Exception exception) when (exception is JsonException or IOException)
        {
            throw new SpeechSynthesisException("Speech worker output is malformed or unavailable.");
        }
        catch (OperationCanceledException) when (!token.IsCancellationRequested)
        {
            throw new SpeechSynthesisException("Speech worker response timed out.");
        }
    }

    private async ValueTask SendAsync(string message, CancellationToken token)
    {
        try
        {
            await _worker!.SendAsync(message, token).ConfigureAwait(false);
        }
        catch (IOException)
        {
            throw new SpeechSynthesisException("Speech worker input is unavailable.");
        }
    }

    private async ValueTask EnsureReadyAsync(CancellationToken token)
    {
        if (_worker is not null)
        {
            return;
        }
        _worker = await _factory.StartAsync(token).ConfigureAwait(false);
        using JsonDocument ready = await ReceiveAsync(token).ConfigureAwait(false);
        if (PocketSpeechProtocol.StringField(ready.RootElement, "type") != "ready" ||
            PocketSpeechProtocol.IntField(ready.RootElement, "protocol") != PocketSpeechProtocol.Version)
        {
            throw new SpeechSynthesisException("Speech worker is unavailable or incompatible.");
        }
    }

    private async ValueTask RetireAsync(string id, bool sent)
    {
        if (_worker is null)
        {
            return;
        }
        try
        {
            if (sent)
            {
                using CancellationTokenSource deadline = new(TimeSpan.FromSeconds(2));
                await _worker.SendAsync(JsonSerializer.Serialize(new { type = "cancel", id }), deadline.Token)
                    .ConfigureAwait(false);
                while (true)
                {
                    using JsonDocument document = PocketSpeechProtocol.Parse(
                        await _worker.ReceiveAsync(deadline.Token).ConfigureAwait(false));
                    JsonElement root = document.RootElement;
                    if (root.GetProperty("id").GetString() != id)
                    {
                        break;
                    }
                    string type = PocketSpeechProtocol.StringField(root, "type");
                    if ((type == "finished" && PocketSpeechProtocol.StringField(root, "status") is "completed" or "cancelled") ||
                        (type == "error" && !string.IsNullOrWhiteSpace(PocketSpeechProtocol.StringField(root, "code"))))
                    {
                        return;
                    }
                    if (type != "audio") { break; }
                }
            }
        }
        catch (Exception exception) when (exception is OperationCanceledException or IOException or
            JsonException or SpeechSynthesisException or KeyNotFoundException or InvalidOperationException)
        {
            // An interrupted line or failed acknowledgement poisons the channel; replace it on the next request.
        }
        await ResetAsync().ConfigureAwait(false);
    }

    private async ValueTask ResetAsync()
    {
        if (_worker is not null)
        {
            await _worker.DisposeAsync().ConfigureAwait(false);
            _worker = null;
        }
    }

    /// <summary>Cancels current work and terminates the process before releasing its owned resources.</summary>
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }
        _disposed = true;
        await _lifetime.CancelAsync().ConfigureAwait(false);
        await _gate.WaitAsync().ConfigureAwait(false);
        try
        {
            await ResetAsync().ConfigureAwait(false);
        }
        finally
        {
            _gate.Release();
        }
        _gate.Dispose();
        _lifetime.Dispose();
    }
}
