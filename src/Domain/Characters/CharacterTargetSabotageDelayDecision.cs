using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Characters;

/// <summary>Retains one provider-neutral exact target-delay selection or abstention.</summary>
public sealed record CharacterTargetSabotageDelayDecision
{
    /// <summary>Creates one explicit selection or null-candidate abstention.</summary>
    public CharacterTargetSabotageDelayDecision(
        CharacterId actorId,
        CharacterTargetSabotageDelayCandidate? selectedCandidate)
    {
        ArgumentNullException.ThrowIfNull(actorId);

        ActorId = actorId;
        SelectedCandidate = selectedCandidate;
    }

    /// <summary>Gets the exact deciding actor identifier.</summary>
    public CharacterId ActorId { get; }

    /// <summary>Gets the exact selected target-delay candidate, or null to abstain.</summary>
    public CharacterTargetSabotageDelayCandidate? SelectedCandidate { get; }
}
