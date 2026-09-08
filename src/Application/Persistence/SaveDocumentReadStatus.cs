namespace Allrpcs.Application.Persistence;

/// <summary>Describes the outcome of one save-document read.</summary>
public enum SaveDocumentReadStatus
{
    /// <summary>A complete non-empty document was loaded.</summary>
    Loaded,

    /// <summary>No save document currently exists.</summary>
    Missing,

    /// <summary>The store could not determine or return the current document.</summary>
    Failed,
}
