using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Allrpcs.Application.Speech;
using Allrpcs.Infrastructure.Speech;

namespace Allrpcs.SpeechServiceProbe;

/// <summary>Exercises the actual C# process boundary without microphone, playback or game state.</summary>
internal static class Program
{
    private const string English = "Stay close. There is still hope for the village.";
    private const string German = "Bleib bei mir. Es gibt noch Hoffnung für das Dorf.";
    private static readonly JsonSerializerOptions ReportOptions = new() { WriteIndented = true };

    private static async Task<int> Main(string[] args)
    {
        if (args.Length != 5)
        {
            Console.Error.WriteLine("Usage: SpeechServiceProbe PYTHON WORKER ASSETS PROFILES OUTPUT_DIRECTORY");
            return 1;
        }
        string directory = Path.GetFullPath(args[4]);
        Directory.CreateDirectory(directory);
        ProbeFactory factory = new(new PocketSpeechWorkerProcessFactory(new(args[0], args[1], args[2], args[3])));
        List<object> results = [];
        await using (PocketSpeechSynthesizer synthesizer = new(factory))
        {
            results.Add(await Capture(synthesizer, directory, "english-cold", English, SpeechLanguage.English));
            results.Add(await Capture(synthesizer, directory, "german-warm", German, SpeechLanguage.German));
            foreach (SpeechLanguage language in new[] { SpeechLanguage.English, SpeechLanguage.German })
            {
                using CancellationTokenSource cancellation = new();
                Stopwatch watch = Stopwatch.StartNew();
                await using IAsyncEnumerator<SpeechAudioChunk> stream = synthesizer.SynthesizeAsync(
                    new(string.Join(' ', Enumerable.Repeat(language == SpeechLanguage.English ? English : German, 20)),
                        language, "default"), cancellation.Token).GetAsyncEnumerator();
                if (!await stream.MoveNextAsync())
                {
                    throw new InvalidOperationException("No PCM before the cancellation probe.");
                }
                int delivered = stream.Current.Samples.Count;
                watch.Restart();
                cancellation.Cancel();
                try
                {
                    await stream.MoveNextAsync();
                    throw new InvalidOperationException("Cancellation did not retire the utterance.");
                }
                catch (OperationCanceledException)
                {
                    results.Add(new
                    {
                        id = "cancel-" + language,
                        deliveredSamples = delivered,
                        cancellationSeconds = watch.Elapsed.TotalSeconds,
                        noPostCancellationPcm = true
                    });
                }
                results.Add(await Capture(synthesizer, directory, "after-cancel-" + language,
                    language == SpeechLanguage.English ? English : German, language));
            }
            await using (IAsyncEnumerator<SpeechAudioChunk> early = synthesizer.SynthesizeAsync(
                new(English, SpeechLanguage.English, "default"), default).GetAsyncEnumerator())
            {
                if (!await early.MoveNextAsync())
                {
                    throw new InvalidOperationException("No PCM for early consumer disposal.");
                }
            }
            results.Add(await Capture(synthesizer, directory, "after-early-disposal", German, SpeechLanguage.German));
            if (factory.Starts != 1)
            {
                throw new InvalidOperationException("Healthy and cancelled utterances did not reuse one worker.");
            }
            results.Add(new { id = "healthy-reuse", workerStarts = factory.Starts });
            try
            {
                await foreach (SpeechAudioChunk ignored in synthesizer.SynthesizeAsync(
                    new(English, SpeechLanguage.English, "missing-profile"), default))
                {
                    throw new InvalidOperationException("Unsupported profile produced audio.");
                }
                throw new InvalidOperationException("Unsupported profile did not report failure.");
            }
            catch (SpeechSynthesisException)
            {
                results.Add(new { id = "unsupported-profile", failureReported = true, textRetained = English });
            }
            results.Add(await Capture(synthesizer, directory, "after-worker-error", English, SpeechLanguage.English));
            if (factory.Starts != 1)
            {
                throw new InvalidOperationException("A request error replaced the warm worker.");
            }
        }
        bool allExited = factory.Processes.All(process => HasExited(process));
        if (!allExited)
        {
            throw new InvalidOperationException("The disposed service left a worker running.");
        }
        object report = new
        {
            os = RuntimeInformation.OSDescription,
            architecture = RuntimeInformation.ProcessArchitecture.ToString(),
            logicalProcessors = Environment.ProcessorCount,
            workerStarts = factory.Starts,
            readySeconds = factory.ReadySeconds,
            allWorkersExited = allExited,
            workerRssSamplesBytes = factory.RssSamples,
            sampledWorkerRssPeakBytes = factory.RssSamples.Max(),
            limits = "Single trials; worker RSS sampled at protocol reads, not continuous startup sampling. No playback device or game/LLM coexistence. These results describe only the host running this probe.",
            cases = results
        };
        string json = JsonSerializer.Serialize(report, ReportOptions);
        await File.WriteAllTextAsync(Path.Combine(directory, "service-probe.json"), json);
        Console.WriteLine(json);
        return 0;
    }

    private static bool HasExited(int id)
    {
        try
        {
            using Process process = Process.GetProcessById(id);
            return process.HasExited;
        }
        catch (ArgumentException)
        {
            return true;
        }
    }

    private static async Task<object> Capture(PocketSpeechSynthesizer synthesizer, string directory,
        string id, string text, SpeechLanguage language)
    {
        List<float> samples = [];
        Stopwatch watch = Stopwatch.StartNew();
        double first = 0;
        await foreach (SpeechAudioChunk chunk in synthesizer.SynthesizeAsync(new(text, language, "default"), default))
        {
            if (samples.Count == 0)
            {
                first = watch.Elapsed.TotalSeconds;
            }
            if (chunk.SampleRate != 24000)
            {
                throw new InvalidOperationException("Unexpected sample rate.");
            }
            samples.AddRange(chunk.Samples);
        }
        double elapsed = watch.Elapsed.TotalSeconds;
        if (samples.Count == 0)
        {
            throw new InvalidOperationException("An accepted utterance produced no audio.");
        }
        string path = Path.Combine(directory, id + ".wav");
        WriteWave(path, samples);
        return new
        {
            id,
            language = language.ToString(),
            text,
            firstPcmSeconds = first,
            generationSeconds = elapsed,
            audioSeconds = samples.Count / 24000.0,
            samples = samples.Count,
            output = Path.GetFileName(path),
            sha256 = Convert.ToHexString(SHA256.HashData(await File.ReadAllBytesAsync(path))).ToLowerInvariant()
        };
    }

    private static void WriteWave(string path, List<float> samples)
    {
        using BinaryWriter writer = new(File.Create(path));
        writer.Write(Encoding.ASCII.GetBytes("RIFF"));
        writer.Write(36 + samples.Count * sizeof(short));
        writer.Write(Encoding.ASCII.GetBytes("WAVEfmt "));
        writer.Write(16);
        writer.Write((short)1);
        writer.Write((short)1);
        writer.Write(24000);
        writer.Write(48000);
        writer.Write((short)2);
        writer.Write((short)16);
        writer.Write(Encoding.ASCII.GetBytes("data"));
        writer.Write(samples.Count * sizeof(short));
        foreach (float sample in samples)
        {
            writer.Write((short)Math.Clamp((int)Math.Round(sample * 32767), short.MinValue, short.MaxValue));
        }
    }

    private sealed class ProbeFactory(IPocketSpeechWorkerFactory inner) : IPocketSpeechWorkerFactory
    {
        public int Starts { get; private set; }
        public List<int> Processes { get; } = [];
        public List<long> RssSamples { get; } = [];
        public List<double> ReadySeconds { get; } = [];

        public async ValueTask<IPocketSpeechWorker> StartAsync(CancellationToken cancellationToken)
        {
            Starts++;
            return new ProbeWorker(await inner.StartAsync(cancellationToken), this);
        }
    }

    private sealed class ProbeWorker(IPocketSpeechWorker inner, ProbeFactory owner) : IPocketSpeechWorker
    {
        private readonly Stopwatch _startup = Stopwatch.StartNew();
        private int _processId;

        public ValueTask SendAsync(string message, CancellationToken cancellationToken) =>
            inner.SendAsync(message, cancellationToken);

        public async ValueTask<string?> ReceiveAsync(CancellationToken cancellationToken)
        {
            string? line = await inner.ReceiveAsync(cancellationToken);
            if (line is not null)
            {
                using JsonDocument document = JsonDocument.Parse(line);
                if (document.RootElement.TryGetProperty("processId", out JsonElement process))
                {
                    _processId = process.GetInt32();
                    owner.Processes.Add(_processId);
                    owner.ReadySeconds.Add(_startup.Elapsed.TotalSeconds);
                }
                if (_processId != 0)
                {
                    using Process child = Process.GetProcessById(_processId);
                    owner.RssSamples.Add(child.WorkingSet64);
                }
            }
            return line;
        }

        public ValueTask DisposeAsync() => inner.DisposeAsync();
    }
}
