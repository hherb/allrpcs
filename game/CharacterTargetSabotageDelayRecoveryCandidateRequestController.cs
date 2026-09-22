using Allrpcs.Domain.Characters;
using Allrpcs.Domain.WorldEvents;
using Godot;

namespace Allrpcs.Game;

/// <summary>Owns one consideration request for the exact current delay-recovery candidate.</summary>
public partial class CharacterTargetSabotageDelayRecoveryCandidateRequestController : Node
{
    private CharacterTargetSabotageDelayRecoveryCandidateController candidates = null!;

    /// <summary>Gets the exact one-shot consideration request accepted by this controller.</summary>
    public CharacterTargetSabotageDelayRecoveryConsiderationRequest? Request { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        candidates = GetNode<CharacterTargetSabotageDelayRecoveryCandidateController>(
            "../CharacterTargetSabotageDelayRecoveryCandidateController");
    }

    /// <summary>Creates one request for the exact current recoverable delay candidate.</summary>
    public bool TryCreate(
        WorldEventSnapshot worldEvents,
        CharacterTargetSabotageDelayRecoveryCandidate expectedCandidate)
    {
        ArgumentNullException.ThrowIfNull(worldEvents);
        ArgumentNullException.ThrowIfNull(expectedCandidate);

        if (Request is not null
            || !ReferenceEquals(candidates.Candidate, expectedCandidate))
        {
            return false;
        }

        CharacterTargetSabotageDelayResult delayResult = expectedCandidate.DelayResult;
        Character target = delayResult.Candidate.Occurrence.Target;
        if (!CharacterTargetSabotageDelayRecoveryPolicy.CanRecover(
                worldEvents,
                delayResult.EventLog,
                delayResult.WorldTime,
                expectedCandidate))
        {
            return false;
        }

        CharacterTargetSabotageDelayRecoveryConsiderationRequest request = new(
            target,
            worldEvents,
            delayResult.EventLog,
            delayResult.WorldTime,
            [expectedCandidate]);
        CharacterTargetSabotageDelayRecoveryDecision abstention = new(
            target.Identity.Id,
            null);
        if (!CharacterTargetSabotageDelayRecoveryDecisionPolicy.CanAccept(request, abstention))
        {
            return false;
        }

        Request = request;
        return true;
    }
}
