using Allrpcs.Domain.Characters;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Presents each exact completed model-selected concealment result at most once.</summary>
public partial class CharacterObjectSabotageConcealmentPresentationCoordinator : Node
{
    private CharacterObjectSabotageConcealmentConsiderationResult? observedResult;
    private CharacterObjectSabotageConcealmentPresentationController presentation = null!;
    private CharacterObjectSabotageConcealmentController state = null!;
    private CharacterObjectSabotageConcealmentTaskController task = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        state = GetNode<CharacterObjectSabotageConcealmentController>(
            "../CharacterObjectSabotageConcealmentController");
        task = GetNode<CharacterObjectSabotageConcealmentTaskController>(
            "../CharacterObjectSabotageConcealmentTaskController");
        presentation = GetNode<CharacterObjectSabotageConcealmentPresentationController>(
            "../CharacterObjectSabotageConcealmentPresentationController");
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        _ = TryPresentCompletedResult();
    }

    /// <summary>Observes one fresh exact terminal selection and presents its state transition.</summary>
    public bool TryPresentCompletedResult()
    {
        CharacterObjectSabotageConcealmentConsiderationResult? result = task.Result;
        if (task.Status != CharacterObjectSabotageConcealmentTaskStatus.Completed
            || result is null
            || ReferenceEquals(observedResult, result))
        {
            return false;
        }

        observedResult = result;
        if (result.SelectedCandidate is null
            || state.Concealment is not { } currentConcealment)
        {
            return false;
        }

        WorldObjectConcealmentSnapshot expectedConcealment = result.Request.Concealment;
        return presentation.TryPresent(expectedConcealment, currentConcealment);
    }
}
