using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Characters;

/// <summary>Retains bounded authoritative physical access to world objects for one character.</summary>
public sealed class CharacterPhysicalAccessSnapshot
{
    /// <summary>Gets the maximum number of accessible objects retained for one character.</summary>
    public const int MaximumAccessibleObjectCount = 64;

    /// <summary>Creates an immutable stable copy of accessible object identifiers.</summary>
    public CharacterPhysicalAccessSnapshot(
        CharacterId characterId,
        IEnumerable<ObjectId> accessibleObjectIds)
    {
        ArgumentNullException.ThrowIfNull(characterId);
        ArgumentNullException.ThrowIfNull(accessibleObjectIds);

        List<ObjectId> retainedObjectIds = [];
        HashSet<ObjectId> distinctObjectIds = [];
        foreach (ObjectId? objectId in accessibleObjectIds)
        {
            if (objectId is null)
            {
                throw new ArgumentException(
                    "Physical-access snapshots cannot contain null object identifiers.",
                    nameof(accessibleObjectIds));
            }

            if (!distinctObjectIds.Add(objectId))
            {
                throw new ArgumentException(
                    "Physical-access snapshots cannot contain duplicate object identifiers.",
                    nameof(accessibleObjectIds));
            }

            retainedObjectIds.Add(objectId);
            if (retainedObjectIds.Count > MaximumAccessibleObjectCount)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(accessibleObjectIds),
                    retainedObjectIds.Count,
                    $"Physical access accepts at most {MaximumAccessibleObjectCount} objects.");
            }
        }

        CharacterId = characterId;
        AccessibleObjectIds = retainedObjectIds.AsReadOnly();
    }

    /// <summary>Gets the exact character identifier represented by this snapshot.</summary>
    public CharacterId CharacterId { get; }

    /// <summary>Gets exact accessible object identifiers in stable supplied order.</summary>
    public IReadOnlyList<ObjectId> AccessibleObjectIds { get; }
}
