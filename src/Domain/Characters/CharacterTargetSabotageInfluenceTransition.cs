namespace Allrpcs.Domain.Characters;

/// <summary>Applies one eligible influence candidate to immutable relationship state.</summary>
public static class CharacterTargetSabotageInfluenceTransition
{
    /// <summary>Returns the exact candidate and its freshly changed relationship.</summary>
    public static CharacterTargetSabotageInfluenceResult Apply(
        Character actor,
        CharacterInteractionAccessSnapshot interactionAccess,
        CharacterRelationshipMemory relationship,
        CharacterTargetSabotageInfluenceCandidate candidate)
    {
        ArgumentNullException.ThrowIfNull(actor);
        ArgumentNullException.ThrowIfNull(interactionAccess);
        ArgumentNullException.ThrowIfNull(relationship);
        ArgumentNullException.ThrowIfNull(candidate);

        if (!CharacterTargetSabotageInfluencePolicy.CanApply(
                actor,
                interactionAccess,
                relationship,
                candidate))
        {
            throw new InvalidOperationException(
                "Only an eligible influence candidate may change the target's relationship.");
        }

        CharacterRelationshipMemory resultingRelationship =
            CharacterRelationshipChangeTransition.Apply(
                relationship,
                candidate.RelationshipChange);

        return new CharacterTargetSabotageInfluenceResult(candidate, resultingRelationship);
    }
}
