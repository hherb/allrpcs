namespace Allrpcs.Application.Persistence;

/// <summary>Describes whether a save-document write completed.</summary>
public enum SaveDocumentWriteResult
{
    /// <summary>The complete supplied document replaced the prior document.</summary>
    Saved,

    /// <summary>The document was not written and any prior document remains current.</summary>
    Failed,
}
