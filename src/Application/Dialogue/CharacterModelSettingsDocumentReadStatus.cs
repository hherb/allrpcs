namespace Allrpcs.Application.Dialogue;

/// <summary>Describes the outcome of one provider-settings document read.</summary>
public enum CharacterModelSettingsDocumentReadStatus
{
    /// <summary>A complete non-empty document was loaded.</summary>
    Loaded,

    /// <summary>No provider-settings document currently exists.</summary>
    Missing,

    /// <summary>The store could not determine or return the current document.</summary>
    Failed,
}
