using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Characters;

/// <summary>Contains bounded stable independent-task assignments.</summary>
public sealed class CharacterIndependentTaskAssignmentSnapshot
{
    /// <summary>Gets the maximum retained independent-task assignment count.</summary>
    public const int MaximumAssignmentCount = 32;

    /// <summary>Creates one immutable assignment snapshot in supplied order.</summary>
    public CharacterIndependentTaskAssignmentSnapshot(
        IEnumerable<CharacterIndependentTaskAssignment> assignments)
    {
        ArgumentNullException.ThrowIfNull(assignments);

        List<CharacterIndependentTaskAssignment> retainedAssignments = new();
        HashSet<CharacterIndependentTaskId> taskIds = new();
        foreach (CharacterIndependentTaskAssignment assignment in assignments)
        {
            if (assignment is null)
            {
                throw new ArgumentException(
                    "Independent-task assignment snapshots cannot contain null values.",
                    nameof(assignments));
            }

            if (!taskIds.Add(assignment.TaskId))
            {
                throw new ArgumentException(
                    "Independent-task identifiers must be unique within an assignment snapshot.",
                    nameof(assignments));
            }

            retainedAssignments.Add(assignment);
            if (retainedAssignments.Count > MaximumAssignmentCount)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(assignments),
                    retainedAssignments.Count,
                    $"An assignment snapshot accepts at most {MaximumAssignmentCount} tasks.");
            }
        }

        Assignments = retainedAssignments.AsReadOnly();
    }

    /// <summary>Gets exact assignment instances in stable supplied order.</summary>
    public IReadOnlyList<CharacterIndependentTaskAssignment> Assignments { get; }
}
