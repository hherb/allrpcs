using Allrpcs.Application.Navigation;
using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Authors one exact temporary scene return route for the completed healer task.</summary>
public partial class TemporaryCharacterIndependentTaskReturnTravelSource : Node
{
    private CharacterIndependentTaskAssignment assignment = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        assignment = GetNode<TemporaryCharacterIndependentTaskSource>(
            "../TemporaryCharacterIndependentTaskSource").Activities.Activities.Single().Assignment;
    }

    /// <summary>Creates one fresh return plan from the exact successful authored task result.</summary>
    public CharacterIndependentTaskReturnTravelPlan CreatePlan(
        CharacterIndependentTaskArrivalOccurrenceResult arrivalOccurrenceResult,
        CharacterIndependentTaskActivity completedActivity)
    {
        ArgumentNullException.ThrowIfNull(arrivalOccurrenceResult);
        ArgumentNullException.ThrowIfNull(completedActivity);

        CharacterIndependentTaskTravelPlan outboundPlan =
            arrivalOccurrenceResult.TravelProgress.Plan;
        if (!ReferenceEquals(outboundPlan.Activity.Assignment, assignment)
            || !ReferenceEquals(completedActivity.Assignment, assignment))
        {
            throw new InvalidOperationException(
                "Only the exact authored completed task has a temporary return route.");
        }

        return new CharacterIndependentTaskReturnTravelPlan(
            arrivalOccurrenceResult,
            completedActivity,
            outboundPlan.WorldBounds,
            outboundPlan.Waypoints[^1],
            [outboundPlan.Origin]);
    }
}
