using System.Diagnostics;
using System.Text;

namespace Allrpcs.Infrastructure.Speech;

/// <summary>Owns a worker process and bounded buffered protocol reader.</summary>
internal sealed class PocketSpeechWorkerProcess : IPocketSpeechWorker
{
    private readonly Process _process;
    private readonly OwnedSpeechWorkerLifetime _lifetime;
    private readonly char[] _buffer = new char[4096];
    private int _position;
    private int _length;

    internal PocketSpeechWorkerProcess(Process process)
    {
        _process = process;
        _process.StandardInput.AutoFlush = true;
        _lifetime = new(process);
    }

    public async ValueTask SendAsync(string message, CancellationToken cancellationToken) =>
        await _process.StandardInput.WriteLineAsync(message.AsMemory(), cancellationToken).ConfigureAwait(false);

    public async ValueTask<string?> ReceiveAsync(CancellationToken cancellationToken)
    {
        StringBuilder line = new();
        while (true)
        {
            if (_position == _length)
            {
                _length = await _process.StandardOutput.ReadAsync(_buffer.AsMemory(), cancellationToken).ConfigureAwait(false);
                _position = 0;
                if (_length == 0)
                {
                    if (line.Length > 0)
                    {
                        throw new IOException("Speech worker closed an incomplete protocol line.");
                    }
                    return null;
                }
            }
            int newline = Array.IndexOf(_buffer, '\n', _position, _length - _position);
            int end = newline < 0 ? _length : newline;
            line.Append(_buffer, _position, end - _position);
            _position = newline < 0 ? _length : newline + 1;
            if (line.Length > PocketSpeechProtocol.MaximumLineLength)
            {
                throw new IOException("Speech worker output exceeded the protocol bound.");
            }
            if (newline >= 0)
            {
                return line.ToString().TrimEnd('\r');
            }
        }
    }

    public ValueTask DisposeAsync() => _lifetime.DisposeAsync();
}
