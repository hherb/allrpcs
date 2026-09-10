using System.Text;
using Allrpcs.Application.Dialogue;

namespace Allrpcs.Infrastructure.Dialogue;

/// <summary>Stores one provider-settings document at a composition-root-owned absolute path.</summary>
public sealed class FileCharacterModelSettingsDocumentStore
    : ICharacterModelSettingsDocumentStore
{
    private static readonly UTF8Encoding StrictUtf8 = new(
        encoderShouldEmitUTF8Identifier: false,
        throwOnInvalidBytes: true);

    private readonly string filePath;

    /// <summary>Creates a single-document store for one trusted absolute file path.</summary>
    public FileCharacterModelSettingsDocumentStore(string filePath)
    {
        ArgumentException.ThrowIfNullOrEmpty(filePath);
        if (!Path.IsPathFullyQualified(filePath)
            || string.IsNullOrEmpty(Path.GetFileName(filePath)))
        {
            throw new ArgumentException(
                "Value must be an absolute provider-settings document file path.",
                nameof(filePath));
        }

        this.filePath = filePath;
    }

    /// <inheritdoc />
    public CharacterModelSettingsDocumentWriteResult Save(string serializedDocument)
    {
        ArgumentException.ThrowIfNullOrEmpty(serializedDocument);

        string directoryPath = Path.GetDirectoryName(filePath)
            ?? throw new InvalidOperationException("The validated file path has no directory.");
        string temporaryPath = Path.Combine(
            directoryPath,
            $".{Path.GetFileName(filePath)}.{Guid.NewGuid():N}.tmp");

        try
        {
            Directory.CreateDirectory(directoryPath);
            using (FileStream stream = new(
                temporaryPath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 4096,
                FileOptions.WriteThrough))
            {
                using StreamWriter writer = new(
                    stream,
                    StrictUtf8,
                    bufferSize: 4096,
                    leaveOpen: true);
                writer.Write(serializedDocument);
                writer.Flush();
                stream.Flush(flushToDisk: true);
            }

            if (File.Exists(filePath))
            {
                File.Replace(temporaryPath, filePath, destinationBackupFileName: null);
            }
            else
            {
                File.Move(temporaryPath, filePath);
            }
            return CharacterModelSettingsDocumentWriteResult.Saved;
        }
        catch (Exception exception) when (IsStorageFailure(exception))
        {
            return CharacterModelSettingsDocumentWriteResult.Failed;
        }
        finally
        {
            TryDeleteTemporaryFile(temporaryPath);
        }
    }

    /// <inheritdoc />
    public CharacterModelSettingsDocumentReadResult Load()
    {
        try
        {
            string serializedDocument = File.ReadAllText(filePath, StrictUtf8);
            return string.IsNullOrEmpty(serializedDocument)
                ? CharacterModelSettingsDocumentReadResult.Failed()
                : CharacterModelSettingsDocumentReadResult.Loaded(serializedDocument);
        }
        catch (Exception exception) when (
            exception is FileNotFoundException or DirectoryNotFoundException)
        {
            return CharacterModelSettingsDocumentReadResult.Missing();
        }
        catch (Exception exception) when (IsStorageFailure(exception))
        {
            return CharacterModelSettingsDocumentReadResult.Failed();
        }
    }

    private static bool IsStorageFailure(Exception exception) =>
        exception is IOException
            or UnauthorizedAccessException
            or EncoderFallbackException
            or DecoderFallbackException
            or NotSupportedException;

    private static void TryDeleteTemporaryFile(string temporaryPath)
    {
        try
        {
            File.Delete(temporaryPath);
        }
        catch (Exception exception) when (IsStorageFailure(exception))
        {
            // The write result already reports failure; cleanup must not replace that outcome.
        }
    }
}
