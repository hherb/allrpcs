namespace Allrpcs.Application.Dialogue;

/// <summary>
/// Identifies one bounded provider-neutral reason that character-model generation failed.
/// </summary>
public enum CharacterModelFailureCode
{
    /// <summary>The configured model service is unavailable.</summary>
    Unavailable,

    /// <summary>The model service rejected authentication.</summary>
    Authentication,

    /// <summary>The model service rejected the request because of a rate limit.</summary>
    RateLimited,

    /// <summary>Transport failed while communicating with the model service.</summary>
    Transport,

    /// <summary>The model service returned an invalid response.</summary>
    InvalidResponse,

    /// <summary>The model service rejected an otherwise valid request.</summary>
    ProviderRejected,
}
