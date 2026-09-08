namespace Allrpcs.Application.Persistence;

/// <summary>Contains one explicit storage-independent save-document read outcome.</summary>
public sealed record SaveDocumentReadResult
{
    private SaveDocumentReadResult(SaveDocumentReadStatus status, string? serializedDocument)
    {
        if (status == SaveDocumentReadStatus.Loaded)
        {
            ArgumentNullException.ThrowIfNull(serializedDocument);
            if (serializedDocument.Length == 0)
            {
                throw new ArgumentException(
                    "A loaded save document must contain serialized data.",
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
    public SaveDocumentReadStatus Status { get; }

    /// <summary>Gets the exact loaded document, or null when no document was returned.</summary>
    public string? SerializedDocument { get; }

    /// <summary>Creates a successful result retaining the exact non-empty document.</summary>
    public static SaveDocumentReadResult Loaded(string serializedDocument) =>
        new(SaveDocumentReadStatus.Loaded, serializedDocument);

    /// <summary>Creates a result reporting that no document exists.</summary>
    public static SaveDocumentReadResult Missing() =>
        new(SaveDocumentReadStatus.Missing, null);

    /// <summary>Creates a result reporting that the current document could not be read.</summary>
    public static SaveDocumentReadResult Failed() =>
        new(SaveDocumentReadStatus.Failed, null);
}
