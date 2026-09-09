namespace Allrpcs.Application.Dialogue;

/// <summary>Identifies one of the closed set of configurable character-model providers.</summary>
public sealed class CharacterModelProvider : IEquatable<CharacterModelProvider>
{
    private CharacterModelProvider(string value)
    {
        Value = value;
    }

    /// <summary>Gets the OpenAI provider.</summary>
    public static CharacterModelProvider OpenAi { get; } = new("openai");

    /// <summary>Gets the Anthropic provider.</summary>
    public static CharacterModelProvider Anthropic { get; } = new("anthropic");

    /// <summary>Gets the local Ollama provider.</summary>
    public static CharacterModelProvider Ollama { get; } = new("ollama");

    /// <summary>Gets every provider in stable canonical order.</summary>
    public static IReadOnlyList<CharacterModelProvider> All { get; } = Array.AsReadOnly(
        new[] { OpenAi, Anthropic, Ollama });

    /// <summary>Gets the stable canonical provider text.</summary>
    public string Value { get; }

    /// <summary>Parses canonical provider text into the defined instance.</summary>
    public static CharacterModelProvider Parse(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return value switch
        {
            "openai" => OpenAi,
            "anthropic" => Anthropic,
            "ollama" => Ollama,
            _ => throw new ArgumentException("Unknown character-model provider.", nameof(value)),
        };
    }

    /// <inheritdoc />
    public bool Equals(CharacterModelProvider? other) => ReferenceEquals(this, other);

    /// <inheritdoc />
    public override bool Equals(object? obj) =>
        obj is CharacterModelProvider provider && Equals(provider);

    /// <inheritdoc />
    public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Value);

    /// <summary>Reports whether two values are the same defined provider.</summary>
    public static bool operator ==(CharacterModelProvider? left, CharacterModelProvider? right) =>
        ReferenceEquals(left, right);

    /// <summary>Reports whether two values are different defined providers.</summary>
    public static bool operator !=(CharacterModelProvider? left, CharacterModelProvider? right) =>
        !(left == right);

    /// <inheritdoc />
    public override string ToString() => Value;
}
