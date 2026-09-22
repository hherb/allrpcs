using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Explicitly observes and acknowledges each exact sabotage-action result once.</summary>
public partial class CharacterSabotageActionSelectionCoordinator : Node
{
    private CharacterSabotageActionTaskController task = null!;

    /// <summary>Gets the exact completed result most recently observed by this coordinator.</summary>
    public CharacterSabotageActionConsiderationResult? ObservedResult { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        task = GetNode<CharacterSabotageActionTaskController>(
            "../CharacterSabotageActionTaskController");
    }

    /// <summary>Observes one fresh exact completed result without dispatching its selection.</summary>
    public bool TryObserveCompletedResult()
    {
        CharacterSabotageActionConsiderationResult? result = task.Result;
        if (task.Status != CharacterSabotageActionTaskStatus.Completed
            || result is null
            || ReferenceEquals(ObservedResult, result))
        {
            return false;
        }

        ObservedResult = result;
        return true;
    }

    /// <summary>Resets and clears only the same exact completed result already observed.</summary>
    public bool TryAcknowledgeCompletedResult()
    {
        CharacterSabotageActionConsiderationResult? result = task.Result;
        if (task.Status != CharacterSabotageActionTaskStatus.Completed
            || result is null
            || !ReferenceEquals(ObservedResult, result)
            || !task.TryReset())
        {
            return false;
        }

        ObservedResult = null;
        return true;
    }
}
