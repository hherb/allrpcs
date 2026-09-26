using System.Text.Json;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Persistence;

namespace Allrpcs.Infrastructure.Persistence;

/// <summary>Writes the canonical visibility-safe map save-state JSON object.</summary>
internal static class MapSaveStateJsonWriter
{
    /// <summary>Writes one complete map property in stable canonical order.</summary>
    internal static void Write(Utf8JsonWriter writer, MapSaveState map)
    {
        writer.WritePropertyName("map");
        writer.WriteStartObject();
        writer.WriteString("seekerId", map.SeekerId.Value);
        WriteLocationIds(writer, "reportedLocationIds", map.ReportedLocationIds);
        WriteLocationIds(writer, "verifiedLocationIds", map.VerifiedLocationIds);
        writer.WritePropertyName("notes");
        writer.WriteStartArray();
        foreach (MapNoteSaveEntry note in map.Notes)
        {
            writer.WriteStartObject();
            writer.WriteString("locationId", note.LocationId.Value);
            writer.WriteString("text", note.Text);
            writer.WriteEndObject();
        }

        writer.WriteEndArray();
        writer.WriteEndObject();
    }

    private static void WriteLocationIds(
        Utf8JsonWriter writer,
        string propertyName,
        IReadOnlyList<LocationId> locationIds)
    {
        writer.WritePropertyName(propertyName);
        writer.WriteStartArray();
        foreach (LocationId locationId in locationIds)
        {
            writer.WriteStringValue(locationId.Value);
        }

        writer.WriteEndArray();
    }
}
