using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.WorldObjects;

/// <summary>Retains bounded authoritative concealment state without changing world objects.</summary>
public sealed class WorldObjectConcealmentSnapshot
{
    /// <summary>Gets the maximum number of concealed objects retained in one snapshot.</summary>
    public const int MaximumConcealedObjectCount = 64;

    /// <summary>Creates an immutable stable copy of concealed object identifiers.</summary>
    public WorldObjectConcealmentSnapshot(IEnumerable<ObjectId> concealedObjectIds)
    {
        ArgumentNullException.ThrowIfNull(concealedObjectIds);

        List<ObjectId> retainedObjectIds = [];
        HashSet<ObjectId> distinctObjectIds = [];
        foreach (ObjectId? objectId in concealedObjectIds)
        {
            if (objectId is null)
            {
                throw new ArgumentException(
                    "World-object concealment cannot contain null object identifiers.",
                    nameof(concealedObjectIds));
            }

            if (!distinctObjectIds.Add(objectId))
            {
                throw new ArgumentException(
                    "World-object concealment cannot contain duplicate object identifiers.",
                    nameof(concealedObjectIds));
            }

            retainedObjectIds.Add(objectId);
            if (retainedObjectIds.Count > MaximumConcealedObjectCount)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(concealedObjectIds),
                    retainedObjectIds.Count,
                    $"World-object concealment accepts at most {MaximumConcealedObjectCount} objects.");
            }
        }

        ConcealedObjectIds = retainedObjectIds.AsReadOnly();
    }

    /// <summary>Gets exact concealed object identifiers in stable supplied order.</summary>
    public IReadOnlyList<ObjectId> ConcealedObjectIds { get; }
}
