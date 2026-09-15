using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.WorldObjects;

/// <summary>Retains bounded authoritative world-object contamination without changing world objects.</summary>
public sealed class WorldObjectContaminationSnapshot
{
    /// <summary>Gets the maximum number of contaminated objects retained in one snapshot.</summary>
    public const int MaximumContaminatedObjectCount = 64;

    /// <summary>Creates an immutable stable copy of contaminated object identifiers.</summary>
    public WorldObjectContaminationSnapshot(IEnumerable<ObjectId> contaminatedObjectIds)
    {
        ArgumentNullException.ThrowIfNull(contaminatedObjectIds);

        List<ObjectId> retainedObjectIds = [];
        HashSet<ObjectId> distinctObjectIds = [];
        foreach (ObjectId? objectId in contaminatedObjectIds)
        {
            if (objectId is null)
            {
                throw new ArgumentException(
                    "World-object contamination cannot contain null object identifiers.",
                    nameof(contaminatedObjectIds));
            }

            if (!distinctObjectIds.Add(objectId))
            {
                throw new ArgumentException(
                    "World-object contamination cannot contain duplicate object identifiers.",
                    nameof(contaminatedObjectIds));
            }

            retainedObjectIds.Add(objectId);
            if (retainedObjectIds.Count > MaximumContaminatedObjectCount)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(contaminatedObjectIds),
                    retainedObjectIds.Count,
                    $"World-object contamination accepts at most {MaximumContaminatedObjectCount} objects.");
            }
        }

        ContaminatedObjectIds = retainedObjectIds.AsReadOnly();
    }

    /// <summary>Gets exact contaminated object identifiers in stable supplied order.</summary>
    public IReadOnlyList<ObjectId> ContaminatedObjectIds { get; }

    /// <summary>Returns whether the supplied identifier names a contaminated object.</summary>
    public bool IsContaminated(ObjectId objectId)
    {
        ArgumentNullException.ThrowIfNull(objectId);
        return ContaminatedObjectIds.Contains(objectId);
    }
}
