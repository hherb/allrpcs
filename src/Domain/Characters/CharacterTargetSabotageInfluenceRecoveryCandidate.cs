namespace Allrpcs.Domain.Characters;

/// <summary>Binds one exact influence result to one exact inverse relationship change.</summary>
public sealed class CharacterTargetSabotageInfluenceRecoveryCandidate
{
    /// <summary>Creates one immutable recovery candidate without applying recovery.</summary>
    public CharacterTargetSabotageInfluenceRecoveryCandidate(
        CharacterTargetSabotageInfluenceResult influenceResult,
        CharacterRelationshipChange recoveryChange)
    {
        ArgumentNullException.ThrowIfNull(influenceResult);
        ArgumentNullException.ThrowIfNull(recoveryChange);

        InfluenceResult = influenceResult;
        RecoveryChange = recoveryChange;
    }

    /// <summary>Gets the exact applied influence result to recover.</summary>
    public CharacterTargetSabotageInfluenceResult InfluenceResult { get; }

    /// <summary>Gets the exact proposed inverse relationship change.</summary>
    public CharacterRelationshipChange RecoveryChange { get; }
}
