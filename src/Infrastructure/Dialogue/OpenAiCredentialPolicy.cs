namespace Allrpcs.Infrastructure.Dialogue;

/// <summary>Validates the bounded OpenAI API-key representation used at the adapter boundary.</summary>
public static class OpenAiCredentialPolicy
{
    /// <summary>Gets the maximum accepted OpenAI API-key length.</summary>
    public const int MaximumApiKeyLength = 512;

    /// <summary>Returns whether a value is a non-empty printable ASCII credential.</summary>
    public static bool IsValid(string? apiKey) =>
        apiKey is not null
        && apiKey.Length is > 0 and <= MaximumApiKeyLength
        && apiKey.All(character => character is >= '!' and <= '~');
}
