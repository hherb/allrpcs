using System.Diagnostics;
using System.Text;
using Allrpcs.Application.Speech;

namespace Allrpcs.Infrastructure.Speech;

/// <summary>Starts the configured CPU worker directly, with local-only model resolution.</summary>
public sealed class PocketSpeechWorkerProcessFactory : IPocketSpeechWorkerFactory
{
    private readonly PocketSpeechWorkerOptions _options;

    /// <summary>Retains local deployment paths without exposing them to application voice profiles.</summary>
    public PocketSpeechWorkerProcessFactory(PocketSpeechWorkerOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _options = options;
    }

    /// <summary>Starts Python with structured arguments and drained, unlogged standard error.</summary>
    public ValueTask<IPocketSpeechWorker> StartAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ProcessStartInfo info = new(_options.PythonPath)
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            StandardInputEncoding = new UTF8Encoding(false),
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8,
        };
        foreach (string argument in new[] { "-u", _options.WorkerPath, "--assets", _options.AssetsPath,
            "--profiles", _options.ProfilesPath })
        {
            info.ArgumentList.Add(argument);
        }
        info.Environment["HF_HUB_OFFLINE"] = "1";
        info.Environment["TRANSFORMERS_OFFLINE"] = "1";
        info.Environment["PYTHONDONTWRITEBYTECODE"] = "1";
        info.Environment["OMP_NUM_THREADS"] = "1";
        info.Environment["MKL_NUM_THREADS"] = "1";
        try
        {
            Process process = Process.Start(info) ?? throw new SpeechSynthesisException("Speech worker could not start.");
            return ValueTask.FromResult<IPocketSpeechWorker>(new PocketSpeechWorkerProcess(process));
        }
        catch (Exception exception) when (exception is System.ComponentModel.Win32Exception or InvalidOperationException)
        {
            throw new SpeechSynthesisException("Local speech is unavailable; check the configured runtime and assets.");
        }
    }
}
