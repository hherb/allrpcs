namespace Allrpcs.Domain.Dialogue;

/// <summary>
/// Identifies one explicit provider-neutral way in-world dialogue is delivered.
/// </summary>
public sealed class DialogueDeliveryMode : IEquatable<DialogueDeliveryMode>
{
    private DialogueDeliveryMode(string value)
    {
        Value = value;
    }

    /// <summary>Gets ordinary speech governed by the ordinary hearing boundary.</summary>
    public static DialogueDeliveryMode OrdinarySpeech { get; } = new("ordinary-speech");

    /// <summary>Gets explicitly whispered speech governed by a whisper hearing boundary.</summary>
    public static DialogueDeliveryMode Whispering { get; } = new("whispering");

    /// <summary>Gets every defined delivery mode in stable order.</summary>
    public static IReadOnlyList<DialogueDeliveryMode> All { get; } =
        Array.AsReadOnly(new[] { OrdinarySpeech, Whispering });

    /// <summary>Gets the stable canonical text for this delivery mode.</summary>
    public string Value { get; }

    /// <summary>Parses canonical text into one of the defined delivery modes.</summary>
    public static DialogueDeliveryMode Parse(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return value switch
        {
            "ordinary-speech" => OrdinarySpeech,
            "whispering" => Whispering,
            _ => throw new ArgumentException("Unknown dialogue delivery mode.", nameof(value)),
        };
    }

    /// <inheritdoc />
    public bool Equals(DialogueDeliveryMode? other) => ReferenceEquals(this, other);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is DialogueDeliveryMode other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Value);

    /// <summary>Reports whether two modes are the same defined instance.</summary>
    public static bool operator ==(DialogueDeliveryMode? left, DialogueDeliveryMode? right) =>
        ReferenceEquals(left, right);

    /// <summary>Reports whether two modes are different defined instances.</summary>
    public static bool operator !=(DialogueDeliveryMode? left, DialogueDeliveryMode? right) =>
        !(left == right);

    /// <inheritdoc />
    public override string ToString() => Value;
}
