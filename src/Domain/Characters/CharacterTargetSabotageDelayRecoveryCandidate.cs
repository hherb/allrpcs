using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Characters;

/// <summary>Binds one exact applied delay result to one exact recovery world event.</summary>
public sealed class CharacterTargetSabotageDelayRecoveryCandidate
{
    /// <summary>Creates one immutable recovery candidate without applying its event.</summary>
    public CharacterTargetSabotageDelayRecoveryCandidate(
        CharacterTargetSabotageDelayResult delayResult,
        WorldEvent recoveryEvent)
    {
        ArgumentNullException.ThrowIfNull(delayResult);
        ArgumentNullException.ThrowIfNull(recoveryEvent);

        DelayResult = delayResult;
        RecoveryEvent = recoveryEvent;
    }

    /// <summary>Gets the exact applied delay result to recover.</summary>
    public CharacterTargetSabotageDelayResult DelayResult { get; }

    /// <summary>Gets the exact caller-authored recovery world event.</summary>
    public WorldEvent RecoveryEvent { get; }
}
