namespace Allrpcs.Domain.Dialogue;

/// <summary>
/// Identifies one of the closed set of in-world dialogue-act types without classifying text.
/// </summary>
public sealed class DialogueActType : IEquatable<DialogueActType>
{
    private DialogueActType(string value)
    {
        Value = value;
    }

    /// <summary>Gets the direct-command dialogue-act type.</summary>
    public static DialogueActType Command { get; } = new("command");

    /// <summary>Gets the request dialogue-act type.</summary>
    public static DialogueActType Request { get; } = new("request");

    /// <summary>Gets the question dialogue-act type.</summary>
    public static DialogueActType Question { get; } = new("question");

    /// <summary>Gets the speculative dialogue-act type.</summary>
    public static DialogueActType Speculation { get; } = new("speculation");

    /// <summary>Gets the warning dialogue-act type.</summary>
    public static DialogueActType Warning { get; } = new("warning");

    /// <summary>Gets the reported-claim dialogue-act type.</summary>
    public static DialogueActType ReportedClaim { get; } = new("reported-claim");

    /// <summary>Gets the promise dialogue-act type.</summary>
    public static DialogueActType Promise { get; } = new("promise");

    /// <summary>Gets the conditional-plan dialogue-act type.</summary>
    public static DialogueActType ConditionalPlan { get; } = new("conditional-plan");

    /// <summary>Gets every defined dialogue-act type in stable roadmap order.</summary>
    public static IReadOnlyList<DialogueActType> All { get; } = Array.AsReadOnly(
        new[]
        {
            Command,
            Request,
            Question,
            Speculation,
            Warning,
            ReportedClaim,
            Promise,
            ConditionalPlan,
        });

    /// <summary>Gets the stable canonical text for this dialogue-act type.</summary>
    public string Value { get; }

    /// <summary>Parses canonical text into one of the defined dialogue-act types.</summary>
    public static DialogueActType Parse(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return value switch
        {
            "command" => Command,
            "request" => Request,
            "question" => Question,
            "speculation" => Speculation,
            "warning" => Warning,
            "reported-claim" => ReportedClaim,
            "promise" => Promise,
            "conditional-plan" => ConditionalPlan,
            _ => throw new ArgumentException("Unknown dialogue-act type.", nameof(value)),
        };
    }

    /// <inheritdoc />
    public bool Equals(DialogueActType? other) => ReferenceEquals(this, other);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is DialogueActType other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Value);

    /// <summary>Reports whether two dialogue-act types are the same defined instance.</summary>
    public static bool operator ==(DialogueActType? left, DialogueActType? right) =>
        ReferenceEquals(left, right);

    /// <summary>Reports whether two dialogue-act types are different defined instances.</summary>
    public static bool operator !=(DialogueActType? left, DialogueActType? right) =>
        !(left == right);

    /// <inheritdoc />
    public override string ToString() => Value;
}
