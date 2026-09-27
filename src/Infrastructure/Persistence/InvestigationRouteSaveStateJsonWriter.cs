using System.Text.Json;
using Allrpcs.Domain.Persistence;

namespace Allrpcs.Infrastructure.Persistence;

/// <summary>Writes the bounded ID-only investigation-route save-state JSON member.</summary>
internal static class InvestigationRouteSaveStateJsonWriter
{
    /// <summary>Writes one complete route-lifecycle property.</summary>
    internal static void Write(Utf8JsonWriter writer, InvestigationRouteSaveState state)
    {
        writer.WritePropertyName("investigationRoutes");
        writer.WriteStartObject();
        writer.WritePropertyName("progress");
        writer.WriteStartArray();
        foreach (InvestigationRouteProgressSaveEntry entry in state.Progress)
        {
            writer.WriteStartObject();
            writer.WriteString("routeKey", entry.RouteKey);
            WriteIdentifiers(writer, "recordedLocationIds", entry.RecordedLocationIds.Select(id => id.Value));
            WriteIdentifiers(writer, "recordedEvidenceIds", entry.RecordedEvidenceIds.Select(id => id.Value));
            writer.WriteEndObject();
        }

        writer.WriteEndArray();
        WriteIdentifiers(writer, "acceptedCompletionRouteKeys", state.AcceptedCompletionRouteKeys);
        writer.WriteString("consumedCompletionRouteKey", state.ConsumedCompletionRouteKey);
        writer.WriteEndObject();
    }

    private static void WriteIdentifiers(
        Utf8JsonWriter writer,
        string propertyName,
        IEnumerable<string> values)
    {
        writer.WritePropertyName(propertyName);
        writer.WriteStartArray();
        foreach (string value in values)
        {
            writer.WriteStringValue(value);
        }

        writer.WriteEndArray();
    }
}
