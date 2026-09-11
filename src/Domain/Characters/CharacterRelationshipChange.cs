using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Characters;

/// <summary>Proposes one bounded, reasoned relationship change from an ordinary exchange.</summary>
public sealed record CharacterRelationshipChange
{
    /// <summary>Gets the largest magnitude one exchange may change one relationship dimension.</summary>
    public const int MaximumAbsoluteDeltaPerExchange = 10;

    /// <summary>Creates one immutable bounded relationship change.</summary>
    public CharacterRelationshipChange(
        CharacterId characterId,
        CharacterId relatedCharacterId,
        int delta,
        CharacterRelationshipReason reason)
    {
        ArgumentNullException.ThrowIfNull(characterId);
        ArgumentNullException.ThrowIfNull(relatedCharacterId);
        ArgumentNullException.ThrowIfNull(reason);
        if (characterId == relatedCharacterId)
        {
            throw new ArgumentException(
                "A relationship change must refer to a different character.",
                nameof(relatedCharacterId));
        }

        if (delta is 0
            || delta < -MaximumAbsoluteDeltaPerExchange
            || delta > MaximumAbsoluteDeltaPerExchange)
        {
            throw new ArgumentOutOfRangeException(
                nameof(delta),
                delta,
                $"A relationship change must be non-zero and no greater than "
                    + $"{MaximumAbsoluteDeltaPerExchange} points in magnitude.");
        }

        CharacterId = characterId;
        RelatedCharacterId = relatedCharacterId;
        Delta = delta;
        Reason = reason;
    }

    /// <summary>Gets the exact character whose relationship would change.</summary>
    public CharacterId CharacterId { get; }

    /// <summary>Gets the exact other character in the directional relationship.</summary>
    public CharacterId RelatedCharacterId { get; }

    /// <summary>Gets the non-zero bounded signed dimension change.</summary>
    public int Delta { get; }

    /// <summary>Gets the exact recorded reason, dimension, and source exchange.</summary>
    public CharacterRelationshipReason Reason { get; }
}
