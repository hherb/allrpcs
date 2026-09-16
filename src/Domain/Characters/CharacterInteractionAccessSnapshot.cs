using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Characters;

/// <summary>Retains bounded authoritative ordinary-interaction access for one actor.</summary>
public sealed class CharacterInteractionAccessSnapshot
{
    /// <summary>Gets the maximum number of reachable target characters retained for one actor.</summary>
    public const int MaximumReachableTargetCount = 64;

    /// <summary>Creates an immutable stable copy of reachable target-character identifiers.</summary>
    public CharacterInteractionAccessSnapshot(
        CharacterId actorId,
        IEnumerable<CharacterId> reachableTargetCharacterIds)
    {
        ArgumentNullException.ThrowIfNull(actorId);
        ArgumentNullException.ThrowIfNull(reachableTargetCharacterIds);

        List<CharacterId> retainedTargetIds = [];
        HashSet<CharacterId> distinctTargetIds = [];
        foreach (CharacterId? targetId in reachableTargetCharacterIds)
        {
            if (targetId is null)
            {
                throw new ArgumentException(
                    "Interaction-access snapshots cannot contain null target identifiers.",
                    nameof(reachableTargetCharacterIds));
            }

            if (targetId == actorId)
            {
                throw new ArgumentException(
                    "Interaction-access snapshots cannot target the represented actor.",
                    nameof(reachableTargetCharacterIds));
            }

            if (!distinctTargetIds.Add(targetId))
            {
                throw new ArgumentException(
                    "Interaction-access snapshots cannot contain duplicate target identifiers.",
                    nameof(reachableTargetCharacterIds));
            }

            retainedTargetIds.Add(targetId);
            if (retainedTargetIds.Count > MaximumReachableTargetCount)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(reachableTargetCharacterIds),
                    retainedTargetIds.Count,
                    $"Interaction access accepts at most {MaximumReachableTargetCount} targets.");
            }
        }

        ActorId = actorId;
        ReachableTargetCharacterIds = retainedTargetIds.AsReadOnly();
    }

    /// <summary>Gets the exact actor identifier represented by this snapshot.</summary>
    public CharacterId ActorId { get; }

    /// <summary>Gets exact reachable target-character identifiers in stable supplied order.</summary>
    public IReadOnlyList<CharacterId> ReachableTargetCharacterIds { get; }
}
