using System.Text.Json;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Inventory;
using static Allrpcs.Infrastructure.Persistence.SaveSnapshotJsonDeserializer;

namespace Allrpcs.Infrastructure.Persistence;

/// <summary>Strictly reads the inventory section of a save document.</summary>
internal static class InventoryOwnershipJsonReader
{
    /// <summary>
    /// Reads exact ownership pairs, or null, which restores the authored starting inventory plus
    /// any gathered herb the saved history still holds.
    /// </summary>
    internal static InventoryOwnershipSnapshot? Read(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        RequireArray(element);
        List<InventoryOwnership> entries = new();
        foreach (JsonElement entry in element.EnumerateArray())
        {
            RequireObject(entry, "objectId", "ownerCharacterId");
            entries.Add(new InventoryOwnership(
                new ObjectId(ReadString(entry.GetProperty("objectId"))),
                new CharacterId(ReadString(entry.GetProperty("ownerCharacterId")))));
        }

        return new InventoryOwnershipSnapshot(entries);
    }
}
