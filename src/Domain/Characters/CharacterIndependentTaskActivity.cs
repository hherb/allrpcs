namespace Allrpcs.Domain.Characters;

/// <summary>Retains one immutable independent-task assignment and lifecycle state.</summary>
public sealed record CharacterIndependentTaskActivity
{
    /// <summary>Creates one provider-neutral independent-task activity.</summary>
    public CharacterIndependentTaskActivity(
        CharacterIndependentTaskAssignment assignment,
        CharacterIndependentTaskActivityState state)
    {
        ArgumentNullException.ThrowIfNull(assignment);
        ArgumentNullException.ThrowIfNull(state);

        Assignment = assignment;
        State = state;
    }

    /// <summary>Gets the exact assignment represented by this activity.</summary>
    public CharacterIndependentTaskAssignment Assignment { get; }

    /// <summary>Gets the exact defined lifecycle state.</summary>
    public CharacterIndependentTaskActivityState State { get; }
}
