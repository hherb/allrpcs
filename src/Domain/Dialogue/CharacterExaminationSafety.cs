namespace Allrpcs.Domain.Dialogue;

/// <summary>
/// Identifies one closed authoritative safety result for collecting an examined object.
/// </summary>
public sealed class CharacterExaminationSafety : IEquatable<CharacterExaminationSafety>
{
    private CharacterExaminationSafety(string value)
    {
        Value = value;
    }

    /// <summary>Gets the result that permits a later collection decision.</summary>
    public static CharacterExaminationSafety SafeToCollect { get; } =
        new("safe-to-collect");

    /// <summary>Gets the result that prohibits a later collection decision.</summary>
    public static CharacterExaminationSafety UnsafeToCollect { get; } =
        new("unsafe-to-collect");

    /// <summary>Gets every defined safety result in stable decision order.</summary>
    public static IReadOnlyList<CharacterExaminationSafety> All { get; } = Array.AsReadOnly(
        new[] { SafeToCollect, UnsafeToCollect });

    /// <summary>Gets the stable canonical text for this safety result.</summary>
    public string Value { get; }

    /// <summary>Parses canonical text into one of the defined safety result instances.</summary>
    public static CharacterExaminationSafety Parse(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return value switch
        {
            "safe-to-collect" => SafeToCollect,
            "unsafe-to-collect" => UnsafeToCollect,
            _ => throw new ArgumentException(
                "Unknown character-examination safety result.",
                nameof(value)),
        };
    }

    /// <inheritdoc />
    public bool Equals(CharacterExaminationSafety? other) => ReferenceEquals(this, other);

    /// <inheritdoc />
    public override bool Equals(object? obj) =>
        obj is CharacterExaminationSafety other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Value);

    /// <summary>Reports whether two safety results are the same defined instance.</summary>
    public static bool operator ==(
        CharacterExaminationSafety? left,
        CharacterExaminationSafety? right) => ReferenceEquals(left, right);

    /// <summary>Reports whether two safety results are different defined instances.</summary>
    public static bool operator !=(
        CharacterExaminationSafety? left,
        CharacterExaminationSafety? right) => !(left == right);

    /// <inheritdoc />
    public override string ToString() => Value;
}
