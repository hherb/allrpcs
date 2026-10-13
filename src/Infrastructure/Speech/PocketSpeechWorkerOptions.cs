namespace Allrpcs.Infrastructure.Speech;

/// <summary>Supplies infrastructure-owned local executable and model locations.</summary>
public sealed class PocketSpeechWorkerOptions
{
    /// <summary>Requires fully qualified local paths; no shell or provider endpoint is accepted.</summary>
    public PocketSpeechWorkerOptions(string pythonPath, string workerPath, string assetsPath, string profilesPath)
    {
        PythonPath = RequirePath(pythonPath);
        WorkerPath = RequirePath(workerPath);
        AssetsPath = RequirePath(assetsPath);
        ProfilesPath = RequirePath(profilesPath);
    }

    /// <summary>Gets the configured Python interpreter.</summary>
    public string PythonPath { get; }
    /// <summary>Gets the shipped worker script.</summary>
    public string WorkerPath { get; }
    /// <summary>Gets the local verified checkpoint directory.</summary>
    public string AssetsPath { get; }
    /// <summary>Gets infrastructure-owned voice mappings.</summary>
    public string ProfilesPath { get; }

    private static string RequirePath(string path)
    {
        ArgumentNullException.ThrowIfNull(path);
        if (!Path.IsPathFullyQualified(path))
        {
            throw new ArgumentException("Speech worker paths must be fully qualified.", nameof(path));
        }
        return path;
    }
}
