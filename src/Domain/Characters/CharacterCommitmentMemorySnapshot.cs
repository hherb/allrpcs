using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Characters;

/// <summary>Contains bounded ordered unfinished commitments for one character.</summary>
public sealed class CharacterCommitmentMemorySnapshot
{
    /// <summary>Gets the maximum unfinished commitments retained per character.</summary>
    public const int MaximumCommitmentCount = 128;

    /// <summary>Creates one immutable per-character unfinished-commitment snapshot.</summary>
    public CharacterCommitmentMemorySnapshot(
        CharacterId characterId,
        IEnumerable<CharacterCommitmentMemory> commitments)
    {
        ArgumentNullException.ThrowIfNull(characterId);
        ArgumentNullException.ThrowIfNull(commitments);

        List<CharacterCommitmentMemory> retainedCommitments = new();
        HashSet<CharacterCommitmentId> commitmentIds = new();
        foreach (CharacterCommitmentMemory commitment in commitments)
        {
            if (commitment is null)
            {
                throw new ArgumentException(
                    "Commitment-memory snapshots cannot contain null values.",
                    nameof(commitments));
            }

            if (commitment.CharacterId != characterId)
            {
                throw new ArgumentException(
                    "Every commitment must belong to the snapshot character.",
                    nameof(commitments));
            }

            if (!commitmentIds.Add(commitment.CommitmentId))
            {
                throw new ArgumentException(
                    "Commitment identities must be unique within a character snapshot.",
                    nameof(commitments));
            }

            retainedCommitments.Add(commitment);
            if (retainedCommitments.Count > MaximumCommitmentCount)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(commitments),
                    retainedCommitments.Count,
                    $"A character accepts at most {MaximumCommitmentCount} unfinished commitments.");
            }
        }

        CharacterId = characterId;
        Commitments = retainedCommitments.AsReadOnly();
    }

    /// <summary>Gets the exact character identity supplied for this snapshot.</summary>
    public CharacterId CharacterId { get; }

    /// <summary>Gets exact unfinished commitment instances in stable supplied order.</summary>
    public IReadOnlyList<CharacterCommitmentMemory> Commitments { get; }
}
