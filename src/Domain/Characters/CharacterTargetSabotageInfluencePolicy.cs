namespace Allrpcs.Domain.Characters;

/// <summary>Requires an eligible influence occurrence and applicable target-to-actor change.</summary>
public static class CharacterTargetSabotageInfluencePolicy
{
    /// <summary>Returns whether the candidate may change the target's relationship to the actor.</summary>
    public static bool CanApply(
        Character actor,
        CharacterInteractionAccessSnapshot interactionAccess,
        CharacterRelationshipMemory relationship,
        CharacterTargetSabotageInfluenceCandidate candidate)
    {
        ArgumentNullException.ThrowIfNull(actor);
        ArgumentNullException.ThrowIfNull(interactionAccess);
        ArgumentNullException.ThrowIfNull(relationship);
        ArgumentNullException.ThrowIfNull(candidate);

        CharacterTargetSabotageOccurrence occurrence = candidate.Occurrence;
        CharacterRelationshipChange change = candidate.RelationshipChange;

        return occurrence.Proposal.ActionKind == CharacterSabotageActionKind.Influence
            && change.CharacterId == occurrence.Target.Identity.Id
            && change.RelatedCharacterId == occurrence.Proposal.ActorId
            && CharacterTargetSabotageOccurrencePolicy.CanOccur(
                actor,
                interactionAccess,
                occurrence)
            && CharacterRelationshipChangePolicy.CanApply(relationship, change);
    }
}
