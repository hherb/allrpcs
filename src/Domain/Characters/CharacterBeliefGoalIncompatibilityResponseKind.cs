namespace Allrpcs.Domain.Characters;

/// <summary>Identifies one closed response kind without selecting or applying a response.</summary>
public sealed class CharacterBeliefGoalIncompatibilityResponseKind :
    IEquatable<CharacterBeliefGoalIncompatibilityResponseKind>
{
    private CharacterBeliefGoalIncompatibilityResponseKind(string value)
    {
        Value = value;
    }

    /// <summary>Gets the response that maintains the subject's declared position.</summary>
    public static CharacterBeliefGoalIncompatibilityResponseKind MaintainPosition { get; } =
        new("maintain-position");

    /// <summary>Gets the response that reconsiders the subject's declared belief.</summary>
    public static CharacterBeliefGoalIncompatibilityResponseKind ReconsiderBelief { get; } =
        new("reconsider-belief");

    /// <summary>Gets the response that reconsiders the subject's declared goal.</summary>
    public static CharacterBeliefGoalIncompatibilityResponseKind ReconsiderGoal { get; } =
        new("reconsider-goal");

    /// <summary>Gets the response that seeks reconciliation without preselecting a state change.</summary>
    public static CharacterBeliefGoalIncompatibilityResponseKind SeekReconciliation { get; } =
        new("seek-reconciliation");

    /// <summary>Gets every response kind in stable authored order.</summary>
    public static IReadOnlyList<CharacterBeliefGoalIncompatibilityResponseKind> All { get; } =
        Array.AsReadOnly(new[]
        {
            MaintainPosition,
            ReconsiderBelief,
            ReconsiderGoal,
            SeekReconciliation,
        });

    /// <summary>Gets the stable canonical text for this response kind.</summary>
    public string Value { get; }

    /// <summary>Parses canonical text into one exact defined response-kind instance.</summary>
    public static CharacterBeliefGoalIncompatibilityResponseKind Parse(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return value switch
        {
            "maintain-position" => MaintainPosition,
            "reconsider-belief" => ReconsiderBelief,
            "reconsider-goal" => ReconsiderGoal,
            "seek-reconciliation" => SeekReconciliation,
            _ => throw new ArgumentException(
                "Unknown belief-goal incompatibility response kind.",
                nameof(value)),
        };
    }

    /// <inheritdoc />
    public bool Equals(CharacterBeliefGoalIncompatibilityResponseKind? other) =>
        ReferenceEquals(this, other);

    /// <inheritdoc />
    public override bool Equals(object? obj) =>
        obj is CharacterBeliefGoalIncompatibilityResponseKind other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Value);

    /// <summary>Reports whether two values are the same defined instance.</summary>
    public static bool operator ==(
        CharacterBeliefGoalIncompatibilityResponseKind? left,
        CharacterBeliefGoalIncompatibilityResponseKind? right) => ReferenceEquals(left, right);

    /// <summary>Reports whether two values are different defined instances.</summary>
    public static bool operator !=(
        CharacterBeliefGoalIncompatibilityResponseKind? left,
        CharacterBeliefGoalIncompatibilityResponseKind? right) => !(left == right);

    /// <inheritdoc />
    public override string ToString() => Value;
}
