using System.Collections.ObjectModel;
using System.Text.Json;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Persistence;

namespace Allrpcs.Infrastructure.Persistence;

/// <summary>Reads only the bounded closed investigation-route JSON structure.</summary>
internal static class InvestigationRouteSaveStateJsonReader
{
    private const int MaximumRouteCount = 3;
    private const int MaximumIdentifierCount = 3;

    /// <summary>Reads one complete ID-only route lifecycle.</summary>
    internal static InvestigationRouteSaveState Read(JsonElement element)
    {
        SaveSnapshotJsonDeserializer.RequireObject(
            element,
            "progress",
            "acceptedCompletionRouteKeys",
            "consumedCompletionRouteKey");
        JsonElement progressElement = element.GetProperty("progress");
        SaveSnapshotJsonDeserializer.RequireArray(progressElement);
        List<InvestigationRouteProgressSaveEntry> progress = [];
        foreach (JsonElement entry in progressElement.EnumerateArray())
        {
            if (progress.Count == MaximumRouteCount)
            {
                throw SaveSnapshotJsonDeserializer.InvalidDocument();
            }

            SaveSnapshotJsonDeserializer.RequireObject(
                entry,
                "routeKey",
                "recordedLocationIds",
                "recordedEvidenceIds");
            progress.Add(new InvestigationRouteProgressSaveEntry(
                SaveSnapshotJsonDeserializer.ReadString(entry.GetProperty("routeKey")),
                ReadIdentifiers(
                    entry.GetProperty("recordedLocationIds"),
                    value => new LocationId(value)),
                ReadIdentifiers(
                    entry.GetProperty("recordedEvidenceIds"),
                    value => new EvidenceId(value))));
        }

        IReadOnlyList<string> accepted = ReadIdentifiers(
            element.GetProperty("acceptedCompletionRouteKeys"),
            value => value);
        JsonElement consumedElement = element.GetProperty("consumedCompletionRouteKey");
        string? consumed = consumedElement.ValueKind switch
        {
            JsonValueKind.Null => null,
            JsonValueKind.String => SaveSnapshotJsonDeserializer.ReadString(consumedElement),
            _ => throw SaveSnapshotJsonDeserializer.InvalidDocument(),
        };
        return new InvestigationRouteSaveState(progress, accepted, consumed);
    }

    private static ReadOnlyCollection<T> ReadIdentifiers<T>(
        JsonElement element,
        Func<string, T> create)
    {
        SaveSnapshotJsonDeserializer.RequireArray(element);
        List<T> values = [];
        foreach (JsonElement item in element.EnumerateArray())
        {
            if (values.Count == MaximumIdentifierCount)
            {
                throw SaveSnapshotJsonDeserializer.InvalidDocument();
            }

            values.Add(create(SaveSnapshotJsonDeserializer.ReadString(item)));
        }

        return values.AsReadOnly();
    }
}
