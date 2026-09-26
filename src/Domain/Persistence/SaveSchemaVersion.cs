using System.Globalization;

namespace Allrpcs.Domain.Persistence;

/// <summary>
/// Identifies an immutable save-schema version without attaching serialization or storage behavior.
/// </summary>
public sealed record SaveSchemaVersion
{
    private const int FirstSupportedValue = 1;
    private const int CharacterMemoryValue = 2;
    private const int FirstPatientValue = 3;
    private const int DialogueDeliveryValue = 4;
    private const int JournalValue = 5;
    private const int MapValue = 6;

    /// <summary>Gets the first save-schema version supported by the game.</summary>
    public static SaveSchemaVersion FirstSupported { get; } = new(FirstSupportedValue);

    /// <summary>Gets the schema version that first retains canonical character memory.</summary>
    public static SaveSchemaVersion CharacterMemory { get; } = new(CharacterMemoryValue);

    /// <summary>Gets the schema version that first retains first-patient state.</summary>
    public static SaveSchemaVersion FirstPatient { get; } = new(FirstPatientValue);

    /// <summary>Gets the schema version that first preserves dialogue delivery modes.</summary>
    public static SaveSchemaVersion DialogueDelivery { get; } = new(DialogueDeliveryValue);

    /// <summary>Gets the schema version that first retains the safe journal.</summary>
    public static SaveSchemaVersion Journal { get; } = new(JournalValue);

    /// <summary>Gets the schema version that first retains the visibility-safe map.</summary>
    public static SaveSchemaVersion Map { get; } = new(MapValue);

    /// <summary>Creates a save-schema version from a positive integer.</summary>
    public SaveSchemaVersion(int value)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(value, FirstSupportedValue);

        Value = value;
    }

    /// <summary>Gets the positive integer version.</summary>
    public int Value { get; }

    /// <summary>Parses invariant canonical decimal text into a save-schema version.</summary>
    public static SaveSchemaVersion Parse(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (!int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out int parsed)
            || parsed < FirstSupportedValue
            || !string.Equals(
                value,
                parsed.ToString(CultureInfo.InvariantCulture),
                StringComparison.Ordinal))
        {
            throw new ArgumentException("Value must be canonical positive integer text.", nameof(value));
        }

        return new SaveSchemaVersion(parsed);
    }

    /// <inheritdoc />
    public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
}
