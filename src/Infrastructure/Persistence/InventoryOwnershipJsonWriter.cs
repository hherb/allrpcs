using System.Text.Json;
using Allrpcs.Domain.Inventory;

namespace Allrpcs.Infrastructure.Persistence;

/// <summary>Writes who carries which world objects inside an inventory save document.</summary>
internal static class InventoryOwnershipJsonWriter
{
    /// <summary>Writes the ownership pairs; a missing inventory is written as null.</summary>
    internal static void Write(Utf8JsonWriter writer, InventoryOwnershipSnapshot? inventory)
    {
        writer.WritePropertyName("inventory");
        if (inventory is null)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStartArray();
        foreach (InventoryOwnership entry in inventory.Entries)
        {
            writer.WriteStartObject();
            writer.WriteString("objectId", entry.ObjectId.Value);
            writer.WriteString("ownerCharacterId", entry.OwnerCharacterId.Value);
            writer.WriteEndObject();
        }

        writer.WriteEndArray();
    }
}
