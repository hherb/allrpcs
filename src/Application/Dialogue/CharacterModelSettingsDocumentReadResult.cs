namespace Allrpcs.Application.Dialogue;

/// <summary>Contains one explicit storage-independent provider-settings document read outcome.</summary>
public sealed record CharacterModelSettingsDocumentReadResult
{
    private CharacterModelSettingsDocumentReadResult(
        CharacterModelSettingsDocumentReadStatus status,
        string? serializedDocument)
    {
        if (status == CharacterModelSettingsDocumentReadStatus.Loaded)
        {
            ArgumentNullException.ThrowIfNull(serializedDocument);
            if (serializedDocument.Length == 0)
            {
                throw new ArgumentException(
                    "A loaded provider-settings document must contain serialized data.",
                    nameof(serializedDocument));
            }
        }
        else if (serializedDocument is not null)
        {
            throw new ArgumentException(
                "Only a loaded result can contain a serialized document.",
                nameof(serializedDocument));
        }

        Status = status;
        SerializedDocument = serializedDocument;
    }

    /// <summary>Gets the explicit outcome of the read.</summary>
    public CharacterModelSettingsDocumentReadStatus Status { get; }

    /// <summary>Gets the exact loaded document, or null when no document was returned.</summary>
    public string? SerializedDocument { get; }

    /// <summary>Creates a successful result retaining the exact non-empty document.</summary>
    public static CharacterModelSettingsDocumentReadResult Loaded(string serializedDocument) =>
        new(CharacterModelSettingsDocumentReadStatus.Loaded, serializedDocument);

    /// <summary>Creates a result reporting that no document exists.</summary>
    public static CharacterModelSettingsDocumentReadResult Missing() =>
        new(CharacterModelSettingsDocumentReadStatus.Missing, null);

    /// <summary>Creates a result reporting that the current document could not be read.</summary>
    public static CharacterModelSettingsDocumentReadResult Failed() =>
        new(CharacterModelSettingsDocumentReadStatus.Failed, null);
}
