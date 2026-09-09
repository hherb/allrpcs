namespace Allrpcs.Infrastructure.Dialogue;

/// <summary>Names the closed redacted outcomes of saving an OpenAI credential.</summary>
public enum OpenAiCredentialWriteStatus
{
    /// <summary>The API key was saved or replaced.</summary>
    Saved,

    /// <summary>The API key was not saved.</summary>
    Failed,
}
