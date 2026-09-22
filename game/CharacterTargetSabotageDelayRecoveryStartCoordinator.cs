using Allrpcs.Domain.Characters;
using Allrpcs.Domain.WorldEvents;
using Godot;

namespace Allrpcs.Game;

/// <summary>Coordinates one explicit recovery start from exact current delay state.</summary>
public partial class CharacterTargetSabotageDelayRecoveryStartCoordinator : Node
{
    private CharacterTargetSabotageDelayController delay = null!;
    private CharacterTargetSabotageDelayRecoveryCandidateController candidates = null!;
    private CharacterTargetSabotageDelayRecoveryCandidateRequestController requests = null!;
    private CharacterTargetSabotageDelayRecoveryController recovery = null!;
    private CharacterTargetSabotageDelayRecoveryTaskController task = null!;
    private CharacterTargetSabotageDelayRecoveryCandidateConsiderationFlowController flow = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        delay = GetNode<CharacterTargetSabotageDelayController>(
            "../CharacterTargetSabotageDelayController");
        candidates = GetNode<CharacterTargetSabotageDelayRecoveryCandidateController>(
            "../CharacterTargetSabotageDelayRecoveryCandidateController");
        requests = GetNode<CharacterTargetSabotageDelayRecoveryCandidateRequestController>(
            "../CharacterTargetSabotageDelayRecoveryCandidateRequestController");
        recovery = GetNode<CharacterTargetSabotageDelayRecoveryController>(
            "../CharacterTargetSabotageDelayRecoveryController");
        task = GetNode<CharacterTargetSabotageDelayRecoveryTaskController>(
            "../CharacterTargetSabotageDelayRecoveryTaskController");
        flow = GetNode<CharacterTargetSabotageDelayRecoveryCandidateConsiderationFlowController>(
            "../CharacterTargetSabotageDelayRecoveryCandidateConsiderationFlowController");
    }

    /// <summary>Preflights, publishes, initializes, and starts one exact recovery graph.</summary>
    public bool TryStart(
        WorldEventSnapshot worldEvents,
        CharacterTargetSabotageDelayResult expectedDelayResult,
        WorldEvent recoveryEvent)
    {
        ArgumentNullException.ThrowIfNull(worldEvents);
        ArgumentNullException.ThrowIfNull(expectedDelayResult);
        ArgumentNullException.ThrowIfNull(recoveryEvent);

        if (!CanStart(worldEvents, expectedDelayResult, recoveryEvent))
        {
            return false;
        }

        if (!candidates.TryCreate(worldEvents, expectedDelayResult, recoveryEvent)
            || candidates.Candidate is not { } candidate
            || !requests.TryCreate(worldEvents, candidate)
            || requests.Request is not { } request
            || !recovery.TryInitialize(request.EventLog, request.WorldTime)
            || !flow.TryStart(request))
        {
            throw new InvalidOperationException(
                "A preflighted delay-recovery graph must start atomically on the scene thread.");
        }

        return true;
    }

    private bool CanStart(
        WorldEventSnapshot worldEvents,
        CharacterTargetSabotageDelayResult expectedDelayResult,
        WorldEvent recoveryEvent)
    {
        if (candidates.Candidate is not null
            || requests.Request is not null
            || recovery.EventLog is not null
            || recovery.WorldTime is not null
            || recovery.LatestAppliedResult is not null
            || task.Status != CharacterTargetSabotageDelayRecoveryTaskStatus.Idle
            || task.Result is not null
            || !ReferenceEquals(delay.LatestAppliedResult, expectedDelayResult)
            || !ReferenceEquals(delay.EventLog, expectedDelayResult.EventLog)
            || !ReferenceEquals(delay.WorldTime, expectedDelayResult.WorldTime))
        {
            return false;
        }

        CharacterTargetSabotageDelayRecoveryCandidate prospectiveCandidate = new(
            expectedDelayResult,
            recoveryEvent);
        if (!CharacterTargetSabotageDelayRecoveryPolicy.CanRecover(
                worldEvents,
                expectedDelayResult.EventLog,
                expectedDelayResult.WorldTime,
                prospectiveCandidate))
        {
            return false;
        }

        Character target = expectedDelayResult.Candidate.Occurrence.Target;
        CharacterTargetSabotageDelayRecoveryConsiderationRequest prospectiveRequest = new(
            target,
            worldEvents,
            expectedDelayResult.EventLog,
            expectedDelayResult.WorldTime,
            [prospectiveCandidate]);
        return CharacterTargetSabotageDelayRecoveryDecisionPolicy.CanAccept(
            prospectiveRequest,
            new CharacterTargetSabotageDelayRecoveryDecision(target.Identity.Id, null));
    }
}
