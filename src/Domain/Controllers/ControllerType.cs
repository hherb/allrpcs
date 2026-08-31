namespace Allrpcs.Domain.Controllers;

/// <summary>
/// Identifies one of the closed set of character controller types without defining controller
/// instances or runtime behavior.
/// </summary>
public sealed class ControllerType : IEquatable<ControllerType>
{
    private ControllerType(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Gets the human-controlled type.
    /// </summary>
    public static ControllerType Human { get; } = new("human");

    /// <summary>
    /// Gets the large-language-model-controlled type.
    /// </summary>
    public static ControllerType Llm { get; } = new("llm");

    /// <summary>
    /// Gets the deterministic test-controlled type.
    /// </summary>
    public static ControllerType DeterministicTest { get; } = new("deterministic-test");

    /// <summary>
    /// Gets the dormant type.
    /// </summary>
    public static ControllerType Dormant { get; } = new("dormant");

    /// <summary>
    /// Gets every defined controller type in stable canonical order.
    /// </summary>
    public static IReadOnlyList<ControllerType> All { get; } = Array.AsReadOnly(
        new[] { Human, Llm, DeterministicTest, Dormant });

    /// <summary>
    /// Gets the stable canonical text for this controller type.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Parses canonical text into one of the defined controller types.
    /// </summary>
    public static ControllerType Parse(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return value switch
        {
            "human" => Human,
            "llm" => Llm,
            "deterministic-test" => DeterministicTest,
            "dormant" => Dormant,
            _ => throw new ArgumentException("Unknown controller type.", nameof(value)),
        };
    }

    /// <inheritdoc />
    public bool Equals(ControllerType? other) => ReferenceEquals(this, other);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is ControllerType other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Value);

    /// <summary>
    /// Reports whether two controller types are the same defined type.
    /// </summary>
    public static bool operator ==(ControllerType? left, ControllerType? right) =>
        ReferenceEquals(left, right);

    /// <summary>
    /// Reports whether two controller types are different defined types.
    /// </summary>
    public static bool operator !=(ControllerType? left, ControllerType? right) => !(left == right);

    /// <inheritdoc />
    public override string ToString() => Value;
}
