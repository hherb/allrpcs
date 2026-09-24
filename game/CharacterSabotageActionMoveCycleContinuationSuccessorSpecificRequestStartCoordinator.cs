using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Explicitly starts only one exact typed fifth-cycle Move request.</summary>
public partial class
    CharacterSabotageActionMoveCycleContinuationSuccessorSpecificRequestStartCoordinator : Node
{
    private CharacterSabotageActionMoveCycleContinuationSuccessorSpecificRequestCoordinator
        requests = null!;
    private TemporaryCharacterObjectSabotageMoveModelSource models = null!;
    private CharacterObjectSabotageMoveTaskController task = null!;
    private CharacterObjectSabotageMoveController state = null!;

    /// <summary>Gets the exact typed fifth-cycle Move request successfully started here.</summary>
    public CharacterObjectSabotageMoveConsiderationRequest? StartedRequest { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        requests = GetNode<
            CharacterSabotageActionMoveCycleContinuationSuccessorSpecificRequestCoordinator>(
                "../CharacterSabotageActionMoveCycleContinuationSuccessorSpecificRequestCoordinator");
        models = GetNode<TemporaryCharacterObjectSabotageMoveModelSource>(
            "../SuccessorTemporaryCharacterObjectSabotageMoveModelSource");
        task = GetNode<CharacterObjectSabotageMoveTaskController>(
            "../SuccessorCharacterObjectSabotageMoveTaskController");
        state = GetNode<CharacterObjectSabotageMoveController>(
            "../CharacterObjectSabotageMoveController");
    }

    /// <summary>Starts once after revalidating the exact typed request and current graph.</summary>
    public bool TryStart(CharacterObjectSabotageMoveConsiderationRequest? expectedRequest)
    {
        if (StartedRequest is not null
            || task.Status != CharacterObjectSabotageMoveTaskStatus.Idle
            || !requests.IsCurrentRequest(expectedRequest))
        {
            return false;
        }

        ICharacterObjectSabotageMoveDecisionModel model = models.TakeModel();
        if (!task.TryStart(model, expectedRequest!, state))
        {
            throw new InvalidOperationException(
                "A preflighted typed fifth-cycle task must accept its exact request.");
        }

        StartedRequest = expectedRequest;
        return true;
    }

    /// <summary>Checks one exact completed result against the retained start graph.</summary>
    internal bool IsCurrentCompletedResult(
        CharacterObjectSabotageMoveConsiderationResult? expectedResult) =>
        StartedRequest is not null
        && expectedResult is not null
        && task.Status == CharacterObjectSabotageMoveTaskStatus.Completed
        && ReferenceEquals(task.Result, expectedResult)
        && ReferenceEquals(expectedResult.Request, StartedRequest)
        && expectedResult.SelectedCandidate is { } selectedCandidate
        && StartedRequest.Candidates.Count == 1
        && ReferenceEquals(StartedRequest.Candidates[0], selectedCandidate)
        && requests.IsRetainedRequest(StartedRequest)
        && state.Placements is { } placements
        && state.Evidence is { } evidence
        && ReferenceEquals(task.ResolvedPlacements, placements)
        && ReferenceEquals(task.ResolvedEvidence, evidence)
        && !ReferenceEquals(placements, StartedRequest.Placements)
        && !ReferenceEquals(evidence, StartedRequest.Evidence);
}
