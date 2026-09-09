namespace Allrpcs.Application.Dialogue;

/// <summary>Pairs one exact character-model provider with one bounded model name.</summary>
public sealed class CharacterModelSettings
{
    /// <summary>Gets the maximum accepted model-name length.</summary>
    public const int MaximumModelNameLength = 256;

    /// <summary>Creates immutable provider and model settings.</summary>
    public CharacterModelSettings(CharacterModelProvider provider, string modelName)
    {
        ArgumentNullException.ThrowIfNull(provider);
        ValidateModelName(modelName);
        Provider = provider;
        ModelName = modelName;
    }

    /// <summary>Gets the exact defined provider.</summary>
    public CharacterModelProvider Provider { get; }

    /// <summary>Gets the exact bounded model name.</summary>
    public string ModelName { get; }

    /// <summary>Rejects null, blank, whitespace-containing, control, or excessive model names.</summary>
    public static void ValidateModelName(string modelName)
    {
        ArgumentNullException.ThrowIfNull(modelName);
        if (string.IsNullOrWhiteSpace(modelName)
            || modelName.Length > MaximumModelNameLength
            || modelName.Any(character => char.IsWhiteSpace(character) || char.IsControl(character)))
        {
            throw new ArgumentException(
                $"A model name must contain 1 to {MaximumModelNameLength} non-whitespace characters.",
                nameof(modelName));
        }
    }
}
