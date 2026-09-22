using Allrpcs.Domain.Characters;
using Allrpcs.Domain.WorldEvents;
using Godot;

namespace Allrpcs.Game;

/// <summary>Owns one recovery candidate for the exact current applied target-delay result.</summary>
public partial class CharacterTargetSabotageDelayRecoveryCandidateController : Node
{
    private CharacterTargetSabotageDelayController delay = null!;

    /// <summary>Gets the exact one-shot recovery candidate accepted by this controller.</summary>
    public CharacterTargetSabotageDelayRecoveryCandidate? Candidate { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        delay = GetNode<CharacterTargetSabotageDelayController>(
            "../CharacterTargetSabotageDelayController");
    }

    /// <summary>Creates one candidate from the exact current delay result and recovery event.</summary>
    public bool TryCreate(
        WorldEventSnapshot worldEvents,
        CharacterTargetSabotageDelayResult expectedDelayResult,
        WorldEvent recoveryEvent)
    {
        ArgumentNullException.ThrowIfNull(worldEvents);
        ArgumentNullException.ThrowIfNull(expectedDelayResult);
        ArgumentNullException.ThrowIfNull(recoveryEvent);

        if (Candidate is not null
            || !ReferenceEquals(delay.LatestAppliedResult, expectedDelayResult)
            || !ReferenceEquals(delay.EventLog, expectedDelayResult.EventLog)
            || !ReferenceEquals(delay.WorldTime, expectedDelayResult.WorldTime))
        {
            return false;
        }

        CharacterTargetSabotageDelayRecoveryCandidate candidate = new(
            expectedDelayResult,
            recoveryEvent);
        if (!CharacterTargetSabotageDelayRecoveryPolicy.CanRecover(
                worldEvents,
                delay.EventLog,
                delay.WorldTime,
                candidate))
        {
            return false;
        }

        Candidate = candidate;
        return true;
    }
}
