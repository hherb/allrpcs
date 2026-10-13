using System.Text;
using Allrpcs.Application.Input;

namespace Allrpcs.Infrastructure.Input;

/// <summary>Bounded strict-UTF8 local controls storage at an absolute composition-owned path.</summary>
public sealed class FileControlPreferencesStore : IControlPreferencesStore
{
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);
    private readonly string path;
    /// <summary>Accepts only an absolute file path; document data can never choose paths.</summary>
    public FileControlPreferencesStore(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);
        if (!Path.IsPathFullyQualified(path) || string.IsNullOrEmpty(Path.GetFileName(path)))
        { throw new ArgumentException("An absolute controls document path is required."); }
        this.path = path;
    }
    /// <inheritdoc />
    public ControlPreferencesLoadResult Load()
    {
        try
        {
            using FileStream file = new(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            if (file.Length > ControlPreferencesJson.MaximumBytes) { return Default(ControlPreferencesLoadStatus.Invalid); }
            byte[] buffer = new byte[ControlPreferencesJson.MaximumBytes + 1];
            int read = 0, count;
            while (read < buffer.Length && (count = file.Read(buffer, read, buffer.Length - read)) > 0) { read += count; }
            if (read > ControlPreferencesJson.MaximumBytes) { return Default(ControlPreferencesLoadStatus.Invalid); }
            return new(ControlPreferencesLoadStatus.Loaded, ControlPreferencesJson.Deserialize(StrictUtf8.GetString(buffer, 0, read)));
        }
        catch (Exception e) when (e is FileNotFoundException or DirectoryNotFoundException) { return Default(ControlPreferencesLoadStatus.Missing); }
        catch (Exception e) when (Failure(e) || e is ArgumentException) { return Default(ControlPreferencesLoadStatus.Invalid); }
    }
    /// <inheritdoc />
    public bool Save(ControlPreferences preferences)
    {
        string serialized = ControlPreferencesJson.Serialize(preferences);
        string directory = Path.GetDirectoryName(path)!;
        string temporary = Path.Combine(directory, $".{Path.GetFileName(path)}.{Guid.NewGuid():N}.tmp");
        try
        {
            Directory.CreateDirectory(directory);
            using (FileStream file = new(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                using StreamWriter writer = new(file, StrictUtf8, leaveOpen: true);
                writer.Write(serialized); writer.Flush(); file.Flush(true);
            }
            File.Move(temporary, path, overwrite: true); return true;
        }
        catch (Exception e) when (Failure(e)) { return false; }
        finally { try { File.Delete(temporary); } catch (Exception e) when (Failure(e)) { } }
    }
    private static ControlPreferencesLoadResult Default(ControlPreferencesLoadStatus status) => new(status, ControlPreferences.Defaults());
    private static bool Failure(Exception e) => e is IOException or UnauthorizedAccessException or NotSupportedException
        or EncoderFallbackException or DecoderFallbackException;
}
