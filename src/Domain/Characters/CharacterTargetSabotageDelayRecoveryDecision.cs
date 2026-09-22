using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Characters;

/// <summary>Retains one provider-neutral exact delay-recovery selection or abstention.</summary>
public sealed record CharacterTargetSabotageDelayRecoveryDecision
{
    /// <summary>Creates one explicit selection or null-candidate abstention.</summary>
    public CharacterTargetSabotageDelayRecoveryDecision(
        CharacterId actorId,
        CharacterTargetSabotageDelayRecoveryCandidate? selectedCandidate)
    {
        ArgumentNullException.ThrowIfNull(actorId);

        ActorId = actorId;
        SelectedCandidate = selectedCandidate;
    }

    /// <summary>Gets the exact deciding actor identifier.</summary>
    public CharacterId ActorId { get; }

    /// <summary>Gets the exact selected recovery candidate, or null to abstain.</summary>
    public CharacterTargetSabotageDelayRecoveryCandidate? SelectedCandidate { get; }
}
