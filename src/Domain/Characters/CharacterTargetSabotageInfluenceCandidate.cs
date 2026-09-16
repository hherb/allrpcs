namespace Allrpcs.Domain.Characters;

/// <summary>Binds one exact eligible influence occurrence to one exact relationship change.</summary>
public sealed class CharacterTargetSabotageInfluenceCandidate
{
    /// <summary>Creates one immutable influence candidate without applying its change.</summary>
    public CharacterTargetSabotageInfluenceCandidate(
        CharacterTargetSabotageOccurrence occurrence,
        CharacterRelationshipChange relationshipChange)
    {
        ArgumentNullException.ThrowIfNull(occurrence);
        ArgumentNullException.ThrowIfNull(relationshipChange);

        Occurrence = occurrence;
        RelationshipChange = relationshipChange;
    }

    /// <summary>Gets the exact eligible character-targeted occurrence.</summary>
    public CharacterTargetSabotageOccurrence Occurrence { get; }

    /// <summary>Gets the exact directional relationship change candidate.</summary>
    public CharacterRelationshipChange RelationshipChange { get; }
}
