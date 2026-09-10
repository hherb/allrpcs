namespace Allrpcs.Application.Dialogue;

/// <summary>Describes whether a provider-settings document write completed.</summary>
public enum CharacterModelSettingsDocumentWriteResult
{
    /// <summary>The complete supplied document replaced the prior document.</summary>
    Saved,

    /// <summary>The document was not written and any prior document remains current.</summary>
    Failed,
}
