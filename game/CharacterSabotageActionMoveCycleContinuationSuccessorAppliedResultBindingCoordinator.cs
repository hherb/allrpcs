using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Binds and acknowledges one exact applied typed fifth-cycle Move result.</summary>
public partial class
    CharacterSabotageActionMoveCycleContinuationSuccessorAppliedResultBindingCoordinator : Node
{
    private CharacterSabotageActionMoveCycleContinuationSuccessorSpecificRequestCoordinator
        requests = null!;
    private CharacterSabotageActionMoveCycleContinuationSuccessorSpecificRequestStartCoordinator
        start = null!;
    private CharacterSabotageActionMoveCycleContinuationSuccessorAppliedResultObservationCoordinator
        observation = null!;
    private CharacterObjectSabotageMoveTaskController task = null!;
    private CharacterObjectSabotageMoveController state = null!;

    /// <summary>Gets the exact observed typed fifth-cycle Move result bound here.</summary>
    public CharacterObjectSabotageMoveConsiderationResult? BoundResult { get; private set; }

    /// <summary>Gets the exact acknowledged result retained after task reset.</summary>
    public CharacterObjectSabotageMoveConsiderationResult? AcknowledgedResult { get; private set; }

    /// <summary>Gets the exact fresh authoritative placements bound to the result.</summary>
    public WorldObjectPlacementSnapshot? Placements { get; private set; }

    /// <summary>Gets the exact fresh authoritative evidence bound to the result.</summary>
    public DiscoveryEvidenceSnapshot? Evidence { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        requests = GetNode<
            CharacterSabotageActionMoveCycleContinuationSuccessorSpecificRequestCoordinator>(
                "../CharacterSabotageActionMoveCycleContinuationSuccessorSpecificRequestCoordinator");
        start = GetNode<
            CharacterSabotageActionMoveCycleContinuationSuccessorSpecificRequestStartCoordinator>(
                "../CharacterSabotageActionMoveCycleContinuationSuccessorSpecificRequestStartCoordinator");
        observation = GetNode<
            CharacterSabotageActionMoveCycleContinuationSuccessorAppliedResultObservationCoordinator>(
                "../CharacterSabotageActionMoveCycleContinuationSuccessorAppliedResultObservationCoordinator");
        task = GetNode<CharacterObjectSabotageMoveTaskController>(
            "../SuccessorCharacterObjectSabotageMoveTaskController");
        state = GetNode<CharacterObjectSabotageMoveController>(
            "../CharacterObjectSabotageMoveController");
    }

    /// <summary>Binds only the exact observation and its current authoritative state.</summary>
    public bool TryBindObservedResult(
        CharacterObjectSabotageMoveConsiderationResult? expectedResult,
        WorldObjectPlacementSnapshot? expectedPlacements,
        DiscoveryEvidenceSnapshot? expectedEvidence)
    {
        if (BoundResult is not null
            || AcknowledgedResult is not null
            || Placements is not null
            || Evidence is not null
            || expectedResult is null
            || expectedPlacements is null
            || expectedEvidence is null
            || !ReferenceEquals(observation.ObservedResult, expectedResult)
            || !ReferenceEquals(observation.ObservedPlacements, expectedPlacements)
            || !ReferenceEquals(observation.ObservedEvidence, expectedEvidence)
            || !ReferenceEquals(state.Placements, expectedPlacements)
            || !ReferenceEquals(state.Evidence, expectedEvidence)
            || !start.IsCurrentCompletedResult(expectedResult)
            || !ReferenceEquals(task.Result, expectedResult))
        {
            return false;
        }

        BoundResult = expectedResult;
        Placements = expectedPlacements;
        Evidence = expectedEvidence;
        return true;
    }

    /// <summary>Acknowledges only the exact still-current result already bound here.</summary>
    public bool TryAcknowledgeBoundResult(
        CharacterObjectSabotageMoveConsiderationResult? expectedResult)
    {
        if (!ReferenceEquals(BoundResult, expectedResult)
            || Placements is not { } placements
            || Evidence is not { } evidence
            || !ReferenceEquals(state.Placements, placements)
            || !ReferenceEquals(state.Evidence, evidence)
            || !start.IsCurrentCompletedResult(expectedResult)
            || !observation.TryAcknowledgeCompletedResult(
                expectedResult,
                placements,
                evidence))
        {
            return false;
        }

        AcknowledgedResult = expectedResult;
        return true;
    }

    /// <summary>Checks the exact acknowledged result and outcome against retained provenance.</summary>
    internal bool IsCurrentAcknowledgedOutcome(
        CharacterObjectSabotageMoveConsiderationResult? expectedResult,
        WorldObjectPlacementSnapshot? expectedPlacements,
        DiscoveryEvidenceSnapshot? expectedEvidence) =>
        expectedResult is not null
        && expectedPlacements is not null
        && expectedEvidence is not null
        && ReferenceEquals(BoundResult, expectedResult)
        && ReferenceEquals(AcknowledgedResult, expectedResult)
        && ReferenceEquals(Placements, expectedPlacements)
        && ReferenceEquals(Evidence, expectedEvidence)
        && ReferenceEquals(state.Placements, expectedPlacements)
        && ReferenceEquals(state.Evidence, expectedEvidence)
        && task.Status == CharacterObjectSabotageMoveTaskStatus.Idle
        && task.Result is null
        && observation.ObservedResult is null
        && observation.ObservedPlacements is null
        && observation.ObservedEvidence is null
        && ReferenceEquals(expectedResult.Request, start.StartedRequest)
        && requests.IsRetainedRequest(expectedResult.Request)
        && expectedResult.SelectedCandidate is { } selectedCandidate
        && expectedResult.Request.Candidates.Count == 1
        && ReferenceEquals(expectedResult.Request.Candidates[0], selectedCandidate);
}
