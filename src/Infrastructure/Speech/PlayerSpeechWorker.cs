using System.Diagnostics;
using System.Text;
using Allrpcs.Application.Speech;

namespace Allrpcs.Infrastructure.Speech;

/// <summary>Bounded local speech IPC; implementations never log audio or recognized words.</summary>
public interface IPlayerSpeechWorker : IAsyncDisposable
{
    /// <summary>Sends one bounded JSON line.</summary>
    ValueTask SendAsync(string message, CancellationToken cancellationToken);
    /// <summary>Reads one bounded JSON line.</summary>
    ValueTask<string?> ReceiveAsync(CancellationToken cancellationToken);
}
/// <summary>Starts either a local recognizer or an explicitly requested microphone worker.</summary>
public interface IPlayerSpeechWorkerFactory
{
    /// <summary>Starts a configured local process using structured arguments.</summary>
    ValueTask<IPlayerSpeechWorker> StartAsync(bool microphone, SpeechLanguage language, CancellationToken cancellationToken);
}
/// <summary>Explicit developer composition with local model paths and a pinned model manifest.</summary>
public sealed record PlayerSpeechWorkerOptions
{
    /// <summary>Rejects relative executable, script, model and manifest paths.</summary>
    public PlayerSpeechWorkerOptions(string python, string worker, string english, string german, string manifest)
    {
        foreach (string path in new[] { python, worker, english, german, manifest })
        { if (!Path.IsPathFullyQualified(path)) { throw new ArgumentException("Local speech paths must be absolute."); } }
        Python = python; Worker = worker; English = english; German = german; Manifest = manifest;
    }
    /// <summary>Gets the explicit interpreter.</summary>
    public string Python { get; }
    /// <summary>Gets the worker script.</summary>
    public string Worker { get; }
    /// <summary>Gets the selected English model directory.</summary>
    public string English { get; }
    /// <summary>Gets the selected German model directory.</summary>
    public string German { get; }
    /// <summary>Gets the frozen asset manifest.</summary>
    public string Manifest { get; }
}
/// <summary>Owns local, shell-free, content-free-diagnostic worker lifetimes.</summary>
public sealed class PlayerSpeechWorkerFactory(PlayerSpeechWorkerOptions options) : IPlayerSpeechWorkerFactory
{
    /// <inheritdoc />
    public ValueTask<IPlayerSpeechWorker> StartAsync(bool microphone, SpeechLanguage language, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ProcessStartInfo info = new(options.Python)
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            StandardInputEncoding = new UTF8Encoding(false),
            StandardOutputEncoding = Encoding.UTF8
        };
        foreach (string argument in new[] { "-u", options.Worker, "--mode", microphone ? "capture" : "recognizer" })
        { info.ArgumentList.Add(argument); }
        if (!microphone)
        {
            foreach (string argument in new[] { "--language", language == SpeechLanguage.English ? "en" : "de",
                "--model", language == SpeechLanguage.English ? options.English : options.German, "--manifest", options.Manifest })
            { info.ArgumentList.Add(argument); }
        }
        foreach (string name in new[] { "HF_HUB_OFFLINE", "TRANSFORMERS_OFFLINE", "PYTHONDONTWRITEBYTECODE", "OMP_NUM_THREADS", "MKL_NUM_THREADS" })
        { info.Environment[name] = "1"; }
        try { return ValueTask.FromResult<IPlayerSpeechWorker>(new LocalProcess(Process.Start(info) ?? throw new IOException())); }
        catch (Exception error) when (error is IOException or System.ComponentModel.Win32Exception or InvalidOperationException)
        { throw new IOException("Local input speech unavailable."); }
    }
    private sealed class LocalProcess : IPlayerSpeechWorker
    {
        private readonly Process process;
        private readonly Task stderr;
        private readonly char[] buffer = new char[4096];
        private int offset; private int count;
        internal LocalProcess(Process process) { this.process = process; process.StandardInput.AutoFlush = true; stderr = Drain(); }
        public async ValueTask SendAsync(string message, CancellationToken cancellationToken)
        {
            if (message.Length > 32000) { throw new IOException("Input speech message exceeded bound."); }
            await process.StandardInput.WriteLineAsync(message.AsMemory(), cancellationToken).ConfigureAwait(false);
        }
        public async ValueTask<string?> ReceiveAsync(CancellationToken cancellationToken)
        {
            StringBuilder line = new();
            while (true)
            {
                if (offset == count)
                {
                    count = await process.StandardOutput.ReadAsync(buffer.AsMemory(), cancellationToken).ConfigureAwait(false); offset = 0;
                    if (count == 0) { if (line.Length > 0) { throw new IOException("Incomplete speech message."); } return null; }
                }
                int newline = Array.IndexOf(buffer, '\n', offset, count - offset);
                int end = newline < 0 ? count : newline; line.Append(buffer, offset, end - offset);
                offset = newline < 0 ? count : newline + 1;
                if (line.Length > 32000) { throw new IOException("Input speech message exceeded bound."); }
                if (newline >= 0) { return line.ToString().TrimEnd('\r'); }
            }
        }
        private async Task Drain()
        { char[] discarded = new char[4096]; while (await process.StandardError.ReadAsync(discarded.AsMemory()).ConfigureAwait(false) > 0) { } }
        public async ValueTask DisposeAsync()
        {
            try
            {
                if (!process.HasExited) { process.Kill(entireProcessTree: true); }
                await process.WaitForExitAsync().ConfigureAwait(false); await stderr.ConfigureAwait(false);
            }
            finally { process.Dispose(); }
        }
    }
}
