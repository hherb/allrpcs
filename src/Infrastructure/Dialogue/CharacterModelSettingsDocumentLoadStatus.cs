namespace Allrpcs.Infrastructure.Dialogue;

/// <summary>Describes one complete provider-settings document load attempt.</summary>
public enum CharacterModelSettingsDocumentLoadStatus
{
    /// <summary>A supported document was loaded and materialized.</summary>
    Loaded,

    /// <summary>No stored document exists.</summary>
    Missing,

    /// <summary>The document store could not return its current document.</summary>
    StorageFailed,

    /// <summary>The stored document does not conform to the supported schema.</summary>
    InvalidDocument,
}
