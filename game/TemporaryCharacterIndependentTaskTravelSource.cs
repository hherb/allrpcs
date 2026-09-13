using Allrpcs.Application.Navigation;
using Allrpcs.Domain.Characters;
using Godot;
using NumericsVector2 = System.Numerics.Vector2;

namespace Allrpcs.Game;

/// <summary>Authors one exact temporary scene route for the healer independent task.</summary>
public partial class TemporaryCharacterIndependentTaskTravelSource : Node
{
    private static readonly NavigationWorldBounds WorldBounds =
        new(0.0f, 0.0f, 1600.0f, 900.0f);

    private CharacterIndependentTaskAssignment assignment = null!;
    private NumericsVector2 destination;

    /// <inheritdoc />
    public override void _Ready()
    {
        assignment = GetNode<TemporaryCharacterIndependentTaskSource>(
            "../TemporaryCharacterIndependentTaskSource").Activities.Activities.Single().Assignment;
        Vector2 targetPosition = GetNode<GreyboxInteractionTarget>(
            "../InteractionCandidates/HerbPatch").GlobalPosition;
        destination = new NumericsVector2(targetPosition.X, targetPosition.Y);
    }

    /// <summary>Creates one fresh plan from the exact authored active task and supplied origin.</summary>
    public CharacterIndependentTaskTravelPlan CreatePlan(
        CharacterIndependentTaskActivity activeActivity,
        NumericsVector2 origin)
    {
        ArgumentNullException.ThrowIfNull(activeActivity);

        if (!ReferenceEquals(activeActivity.Assignment, assignment)
            || activeActivity.State != CharacterIndependentTaskActivityState.Active)
        {
            throw new InvalidOperationException(
                "Only the exact authored active task has a temporary travel route.");
        }

        return new CharacterIndependentTaskTravelPlan(
            activeActivity,
            WorldBounds,
            origin,
            [destination]);
    }
}
