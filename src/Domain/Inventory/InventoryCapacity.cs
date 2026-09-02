using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Inventory;

/// <summary>
/// Defines the maximum number of distinct owned world objects associated with one character
/// without enforcing inventory policy.
/// </summary>
public sealed record InventoryCapacity
{
    /// <summary>
    /// Creates an immutable per-character inventory capacity.
    /// </summary>
    public InventoryCapacity(CharacterId characterId, int maximumObjectCount)
    {
        ArgumentNullException.ThrowIfNull(characterId);
        ArgumentOutOfRangeException.ThrowIfNegative(maximumObjectCount);

        CharacterId = characterId;
        MaximumObjectCount = maximumObjectCount;
    }

    /// <summary>
    /// Gets the stable identifier of the character whose inventory capacity is described.
    /// </summary>
    public CharacterId CharacterId { get; }

    /// <summary>
    /// Gets the maximum number of distinct world objects the character may own.
    /// </summary>
    public int MaximumObjectCount { get; }
}
