namespace Allrpcs.Domain.Characters;

/// <summary>Identifies one closed epistemic-memory category.</summary>
public sealed class CharacterEpistemicMemoryCategory : IEquatable<CharacterEpistemicMemoryCategory>
{
    private CharacterEpistemicMemoryCategory(string value)
    {
        Value = value;
    }

    /// <summary>Gets the confirmed-fact category.</summary>
    public static CharacterEpistemicMemoryCategory ConfirmedFact { get; } = new("confirmed-fact");

    /// <summary>Gets the attributed-claim category.</summary>
    public static CharacterEpistemicMemoryCategory AttributedClaim { get; } = new("attributed-claim");

    /// <summary>Gets the uncertain-belief category.</summary>
    public static CharacterEpistemicMemoryCategory UncertainBelief { get; } = new("uncertain-belief");

    /// <summary>Gets every defined category in stable epistemic order.</summary>
    public static IReadOnlyList<CharacterEpistemicMemoryCategory> All { get; } = Array.AsReadOnly(
        new[]
        {
            ConfirmedFact,
            AttributedClaim,
            UncertainBelief,
        });

    /// <summary>Gets the canonical category text.</summary>
    public string Value { get; }

    /// <summary>Parses canonical text into one defined category.</summary>
    public static CharacterEpistemicMemoryCategory Parse(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return value switch
        {
            "confirmed-fact" => ConfirmedFact,
            "attributed-claim" => AttributedClaim,
            "uncertain-belief" => UncertainBelief,
            _ => throw new ArgumentException("Unknown epistemic-memory category.", nameof(value)),
        };
    }

    /// <inheritdoc />
    public bool Equals(CharacterEpistemicMemoryCategory? other) => ReferenceEquals(this, other);

    /// <inheritdoc />
    public override bool Equals(object? obj) =>
        obj is CharacterEpistemicMemoryCategory other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Value);

    /// <summary>Reports whether two values are the same defined category.</summary>
    public static bool operator ==(
        CharacterEpistemicMemoryCategory? left,
        CharacterEpistemicMemoryCategory? right) => ReferenceEquals(left, right);

    /// <summary>Reports whether two values are different defined categories.</summary>
    public static bool operator !=(
        CharacterEpistemicMemoryCategory? left,
        CharacterEpistemicMemoryCategory? right) => !(left == right);

    /// <inheritdoc />
    public override string ToString() => Value;
}
