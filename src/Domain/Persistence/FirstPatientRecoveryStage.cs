namespace Allrpcs.Domain.Persistence;

/// <summary>Identifies the durable one-shot boundary of first-patient recovery dialogue.</summary>
public sealed class FirstPatientRecoveryStage : IEquatable<FirstPatientRecoveryStage>
{
    /// <summary>Gets the stage before the stabilized patient testimony has been heard.</summary>
    public static FirstPatientRecoveryStage NotHeard { get; } = new("not-heard");

    /// <summary>Gets the stage after the patient testimony but before sage consultation.</summary>
    public static FirstPatientRecoveryStage PatientTestimonyHeard { get; } =
        new("patient-testimony-heard");

    /// <summary>Gets the stage after the sage consultation has occurred.</summary>
    public static FirstPatientRecoveryStage SageConsulted { get; } = new("sage-consulted");

    private FirstPatientRecoveryStage(string value)
    {
        Value = value;
    }

    /// <summary>Gets the canonical serialized stage value.</summary>
    public string Value { get; }

    /// <summary>Parses one exact supported stage value.</summary>
    public static FirstPatientRecoveryStage Parse(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return value switch
        {
            "not-heard" => NotHeard,
            "patient-testimony-heard" => PatientTestimonyHeard,
            "sage-consulted" => SageConsulted,
            _ => throw new ArgumentException("Value must identify a supported recovery stage.", nameof(value)),
        };
    }

    /// <inheritdoc />
    public bool Equals(FirstPatientRecoveryStage? other) =>
        other is not null && string.Equals(Value, other.Value, StringComparison.Ordinal);

    /// <inheritdoc />
    public override bool Equals(object? obj) => Equals(obj as FirstPatientRecoveryStage);

    /// <inheritdoc />
    public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Value);

    /// <inheritdoc />
    public override string ToString() => Value;
}
