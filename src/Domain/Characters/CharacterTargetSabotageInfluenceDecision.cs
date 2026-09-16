using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Characters;

/// <summary>Retains one provider-neutral exact influence selection or abstention.</summary>
public sealed record CharacterTargetSabotageInfluenceDecision
{
    /// <summary>Creates one explicit selection or null-candidate abstention.</summary>
    public CharacterTargetSabotageInfluenceDecision(
        CharacterId actorId,
        CharacterTargetSabotageInfluenceCandidate? selectedCandidate)
    {
        ArgumentNullException.ThrowIfNull(actorId);

        ActorId = actorId;
        SelectedCandidate = selectedCandidate;
    }

    /// <summary>Gets the exact deciding actor identifier.</summary>
    public CharacterId ActorId { get; }

    /// <summary>Gets the exact selected influence candidate, or null to abstain.</summary>
    public CharacterTargetSabotageInfluenceCandidate? SelectedCandidate { get; }
}
