using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Discovery;

/// <summary>Contains only evidence identifiers visible to one exact character.</summary>
public sealed class CharacterEvidenceViewSnapshot
{
    /// <summary>Creates an immutable, duplicate-free character evidence view.</summary>
    public CharacterEvidenceViewSnapshot(
        CharacterId characterId,
        IEnumerable<EvidenceId> evidenceIds)
    {
        ArgumentNullException.ThrowIfNull(characterId);
        ArgumentNullException.ThrowIfNull(evidenceIds);

        List<EvidenceId> retainedEvidenceIds = new();
        HashSet<EvidenceId> identifiers = new();
        foreach (EvidenceId evidenceId in evidenceIds)
        {
            if (evidenceId is null)
            {
                throw new ArgumentException(
                    "Evidence identifiers cannot contain null values.",
                    nameof(evidenceIds));
            }

            if (!identifiers.Add(evidenceId))
            {
                throw new ArgumentException(
                    "Evidence identifiers cannot contain duplicates.",
                    nameof(evidenceIds));
            }

            retainedEvidenceIds.Add(evidenceId);
        }

        CharacterId = characterId;
        EvidenceIds = retainedEvidenceIds.AsReadOnly();
    }

    /// <summary>Gets the exact character whose visibility produced this view.</summary>
    public CharacterId CharacterId { get; }

    /// <summary>Gets visible evidence identifiers in stable authored order.</summary>
    public IReadOnlyList<EvidenceId> EvidenceIds { get; }
}
