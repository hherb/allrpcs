using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Characters;

/// <summary>Contains bounded stable independent-task lifecycle activities.</summary>
public sealed class CharacterIndependentTaskActivitySnapshot
{
    /// <summary>Gets the maximum retained independent-task activity count.</summary>
    public const int MaximumActivityCount =
        CharacterIndependentTaskAssignmentSnapshot.MaximumAssignmentCount;

    /// <summary>Creates one immutable activity snapshot in supplied order.</summary>
    public CharacterIndependentTaskActivitySnapshot(
        IEnumerable<CharacterIndependentTaskActivity> activities)
    {
        ArgumentNullException.ThrowIfNull(activities);

        List<CharacterIndependentTaskActivity> retainedActivities = new();
        HashSet<CharacterIndependentTaskId> taskIds = new();
        HashSet<CharacterId> activeCharacterIds = new();
        foreach (CharacterIndependentTaskActivity activity in activities)
        {
            if (activity is null)
            {
                throw new ArgumentException(
                    "Independent-task activity snapshots cannot contain null values.",
                    nameof(activities));
            }

            if (!taskIds.Add(activity.Assignment.TaskId))
            {
                throw new ArgumentException(
                    "Independent-task identifiers must be unique within an activity snapshot.",
                    nameof(activities));
            }

            if (activity.State == CharacterIndependentTaskActivityState.Active
                && !activeCharacterIds.Add(activity.Assignment.CharacterId))
            {
                throw new ArgumentException(
                    "A character cannot have more than one active independent task.",
                    nameof(activities));
            }

            retainedActivities.Add(activity);
            if (retainedActivities.Count > MaximumActivityCount)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(activities),
                    retainedActivities.Count,
                    $"An activity snapshot accepts at most {MaximumActivityCount} tasks.");
            }
        }

        Activities = retainedActivities.AsReadOnly();
    }

    /// <summary>Gets exact activity instances in stable supplied order.</summary>
    public IReadOnlyList<CharacterIndependentTaskActivity> Activities { get; }
}
