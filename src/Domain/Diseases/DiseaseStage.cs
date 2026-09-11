namespace Allrpcs.Domain.Diseases;

/// <summary>Identifies one closed authoritative disease stage.</summary>
public sealed class DiseaseStage : IEquatable<DiseaseStage>
{
    private DiseaseStage(string value)
    {
        Value = value;
    }

    /// <summary>Gets the stage after exposure and before observable illness.</summary>
    public static DiseaseStage Exposed { get; } = new("exposed");

    /// <summary>Gets the first stage of observable illness.</summary>
    public static DiseaseStage EarlyIllness { get; } = new("early-illness");

    /// <summary>Gets the stage in which growing drowsiness is observable.</summary>
    public static DiseaseStage Drowsing { get; } = new("drowsing");

    /// <summary>Gets the stage in which the infected character is deeply asleep.</summary>
    public static DiseaseStage DeepSleep { get; } = new("deep-sleep");

    /// <summary>Gets the stage in which untreated disease causes wasting.</summary>
    public static DiseaseStage Wasting { get; } = new("wasting");

    /// <summary>Gets the final stage before an unresolved terminal outcome.</summary>
    public static DiseaseStage FinalDecline { get; } = new("final-decline");

    /// <summary>Gets all defined stages in stable progression order.</summary>
    public static IReadOnlyList<DiseaseStage> All { get; } = Array.AsReadOnly(
        new[] { Exposed, EarlyIllness, Drowsing, DeepSleep, Wasting, FinalDecline });

    /// <summary>Gets the canonical stage text.</summary>
    public string Value { get; }

    /// <summary>Parses canonical text into one defined stage.</summary>
    public static DiseaseStage Parse(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return value switch
        {
            "exposed" => Exposed,
            "early-illness" => EarlyIllness,
            "drowsing" => Drowsing,
            "deep-sleep" => DeepSleep,
            "wasting" => Wasting,
            "final-decline" => FinalDecline,
            _ => throw new ArgumentException("Unknown disease stage.", nameof(value)),
        };
    }

    /// <inheritdoc />
    public bool Equals(DiseaseStage? other) => ReferenceEquals(this, other);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is DiseaseStage other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Value);

    /// <summary>Reports whether both values are the same defined stage.</summary>
    public static bool operator ==(DiseaseStage? left, DiseaseStage? right) =>
        ReferenceEquals(left, right);

    /// <summary>Reports whether both values are different defined stages.</summary>
    public static bool operator !=(DiseaseStage? left, DiseaseStage? right) => !(left == right);

    /// <inheritdoc />
    public override string ToString() => Value;
}
