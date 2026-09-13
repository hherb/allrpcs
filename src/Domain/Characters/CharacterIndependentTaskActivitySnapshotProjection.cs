namespace Allrpcs.Domain.Characters;

/// <summary>Projects bounded assignments into their initial lifecycle activities.</summary>
public static class CharacterIndependentTaskActivitySnapshotProjection
{
    /// <summary>Returns assigned activities retaining every exact supplied assignment.</summary>
    public static CharacterIndependentTaskActivitySnapshot Project(
        CharacterIndependentTaskAssignmentSnapshot assignments)
    {
        ArgumentNullException.ThrowIfNull(assignments);

        return new CharacterIndependentTaskActivitySnapshot(assignments.Assignments.Select(
            assignment => new CharacterIndependentTaskActivity(
                assignment,
                CharacterIndependentTaskActivityState.Assigned)));
    }
}
