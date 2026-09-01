using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Characters;

/// <summary>
/// Contains a character inventory's ordered, duplicate-free references to world objects.
/// </summary>
public sealed class CharacterInventory
{
    /// <summary>
    /// Creates an inventory. An empty sequence explicitly means the character references no objects.
    /// </summary>
    public CharacterInventory(IEnumerable<ObjectId> objectReferences)
    {
        ArgumentNullException.ThrowIfNull(objectReferences);

        List<ObjectId> orderedReferences = new();
        HashSet<ObjectId> identifiers = new();

        foreach (ObjectId objectReference in objectReferences)
        {
            if (objectReference is null)
            {
                throw new ArgumentException(
                    "Object references cannot contain null values.",
                    nameof(objectReferences));
            }

            if (!identifiers.Add(objectReference))
            {
                throw new ArgumentException(
                    "Object references cannot contain duplicate identifiers.",
                    nameof(objectReferences));
            }

            orderedReferences.Add(objectReference);
        }

        ObjectReferences = orderedReferences.AsReadOnly();
    }

    /// <summary>
    /// Gets stable object references in their authored order.
    /// </summary>
    public IReadOnlyList<ObjectId> ObjectReferences { get; }
}
