using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Coordinates one explicit recovery start from exact current influence state.</summary>
public partial class CharacterTargetSabotageInfluenceRecoveryStartCoordinator : Node
{
    private CharacterTargetSabotageInfluenceController influence = null!;
    private CharacterTargetSabotageInfluenceRecoveryCandidateController candidates = null!;
    private CharacterTargetSabotageInfluenceRecoveryCandidateRequestController requests = null!;
    private CharacterTargetSabotageInfluenceRecoveryController recovery = null!;
    private CharacterTargetSabotageInfluenceRecoveryTaskController task = null!;
    private CharacterTargetSabotageInfluenceRecoveryCandidateConsiderationFlowController flow =
        null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        influence = GetNode<CharacterTargetSabotageInfluenceController>(
            "../CharacterTargetSabotageInfluenceController");
        candidates = GetNode<CharacterTargetSabotageInfluenceRecoveryCandidateController>(
            "../CharacterTargetSabotageInfluenceRecoveryCandidateController");
        requests = GetNode<CharacterTargetSabotageInfluenceRecoveryCandidateRequestController>(
            "../CharacterTargetSabotageInfluenceRecoveryCandidateRequestController");
        recovery = GetNode<CharacterTargetSabotageInfluenceRecoveryController>(
            "../CharacterTargetSabotageInfluenceRecoveryController");
        task = GetNode<CharacterTargetSabotageInfluenceRecoveryTaskController>(
            "../CharacterTargetSabotageInfluenceRecoveryTaskController");
        flow = GetNode<
            CharacterTargetSabotageInfluenceRecoveryCandidateConsiderationFlowController>(
                "../CharacterTargetSabotageInfluenceRecoveryCandidateConsiderationFlowController");
    }

    /// <summary>Preflights, publishes, initializes, and starts one exact recovery graph.</summary>
    public bool TryStart(
        CharacterTargetSabotageInfluenceResult expectedInfluenceResult,
        CharacterRelationshipChange recoveryChange)
    {
        ArgumentNullException.ThrowIfNull(expectedInfluenceResult);
        ArgumentNullException.ThrowIfNull(recoveryChange);

        if (!CanStart(expectedInfluenceResult, recoveryChange))
        {
            return false;
        }

        if (!candidates.TryCreate(expectedInfluenceResult, recoveryChange)
            || candidates.Candidate is not { } candidate
            || !requests.TryCreate(candidate)
            || requests.Request is not { } request
            || !recovery.TryInitialize(request.Relationship)
            || !flow.TryStart(request))
        {
            throw new InvalidOperationException(
                "A preflighted influence-recovery graph must start atomically on the scene thread.");
        }

        return true;
    }

    private bool CanStart(
        CharacterTargetSabotageInfluenceResult expectedInfluenceResult,
        CharacterRelationshipChange recoveryChange)
    {
        if (candidates.Candidate is not null
            || requests.Request is not null
            || recovery.Relationship is not null
            || recovery.LatestAppliedResult is not null
            || task.Status != CharacterTargetSabotageInfluenceRecoveryTaskStatus.Idle
            || task.Result is not null
            || !ReferenceEquals(influence.LatestAppliedResult, expectedInfluenceResult)
            || !ReferenceEquals(influence.Relationship, expectedInfluenceResult.Relationship))
        {
            return false;
        }

        CharacterTargetSabotageInfluenceRecoveryCandidate prospectiveCandidate = new(
            expectedInfluenceResult,
            recoveryChange);
        if (!CharacterTargetSabotageInfluenceRecoveryPolicy.CanRecover(
                expectedInfluenceResult.Relationship,
                prospectiveCandidate))
        {
            return false;
        }

        Character target = expectedInfluenceResult.Candidate.Occurrence.Target;
        CharacterTargetSabotageInfluenceRecoveryConsiderationRequest prospectiveRequest = new(
            target,
            expectedInfluenceResult.Relationship,
            [prospectiveCandidate]);
        return CharacterTargetSabotageInfluenceRecoveryDecisionPolicy.CanAccept(
            prospectiveRequest,
            new CharacterTargetSabotageInfluenceRecoveryDecision(target.Identity.Id, null));
    }
}
