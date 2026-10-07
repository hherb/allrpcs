using System.Text;
using Allrpcs.Application.Audio;

namespace Allrpcs.Infrastructure.Audio;

/// <summary>Atomically stores one validated local audio document at a trusted composition path.</summary>
public sealed class FileAudioPreferencesStore : IAudioPreferencesStore
{
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);
    private readonly string filePath;

    /// <summary>Accepts an absolute composition-owned path, separate from provider and world documents.</summary>
    public FileAudioPreferencesStore(string filePath)
    {
        ArgumentException.ThrowIfNullOrEmpty(filePath);
        if (!Path.IsPathFullyQualified(filePath) || string.IsNullOrEmpty(Path.GetFileName(filePath)))
        {
            throw new ArgumentException("An absolute audio settings document path is required.", nameof(filePath));
        }
        this.filePath = filePath;
    }

    /// <inheritdoc />
    public AudioPreferencesLoadResult Load()
    {
        try
        {
            return new(AudioPreferencesLoadStatus.Loaded,
                AudioPreferencesJson.Deserialize(File.ReadAllText(filePath, StrictUtf8)));
        }
        catch (Exception exception) when (exception is FileNotFoundException or DirectoryNotFoundException)
        {
            return new(AudioPreferencesLoadStatus.Missing, AudioPreferences.Defaults());
        }
        catch (Exception exception) when (IsFailure(exception) || exception is ArgumentException)
        {
            return new(AudioPreferencesLoadStatus.Invalid, AudioPreferences.Defaults());
        }
    }

    /// <inheritdoc />
    public bool Save(AudioPreferences preferences)
    {
        string serialized = AudioPreferencesJson.Serialize(preferences);
        string directory = Path.GetDirectoryName(filePath)!;
        string temporary = Path.Combine(directory, $".{Path.GetFileName(filePath)}.{Guid.NewGuid():N}.tmp");
        try
        {
            Directory.CreateDirectory(directory);
            using (FileStream stream = new(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                using StreamWriter writer = new(stream, StrictUtf8, leaveOpen: true);
                writer.Write(serialized);
                writer.Flush();
                stream.Flush(flushToDisk: true);
            }
            File.Move(temporary, filePath, overwrite: true);
            return true;
        }
        catch (Exception exception) when (IsFailure(exception))
        {
            return false;
        }
        finally
        {
            try { File.Delete(temporary); }
            catch (Exception exception) when (IsFailure(exception)) { /* The save result reports failure. */ }
        }
    }

    private static bool IsFailure(Exception exception) => exception is IOException
        or UnauthorizedAccessException or EncoderFallbackException or DecoderFallbackException or NotSupportedException;
}
