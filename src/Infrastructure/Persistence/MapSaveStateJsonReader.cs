using System.Text.Json;
using Allrpcs.Application.Map;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Persistence;

namespace Allrpcs.Infrastructure.Persistence;

/// <summary>Strictly reads one canonical visibility-safe map save-state JSON object.</summary>
internal static class MapSaveStateJsonReader
{
    private const int MaximumLocationEntries = 11;

    /// <summary>Reads one complete bounded map object with no unknown properties.</summary>
    internal static MapSaveState Read(JsonElement element)
    {
        SaveSnapshotJsonDeserializer.RequireObject(
            element,
            "seekerId",
            "reportedLocationIds",
            "verifiedLocationIds",
            "notes");
        return new MapSaveState(
            new CharacterId(ReadString(element, "seekerId")),
            ReadLocationIds(element.GetProperty("reportedLocationIds")),
            ReadLocationIds(element.GetProperty("verifiedLocationIds")),
            ReadNotes(element.GetProperty("notes")));
    }

    private static List<LocationId> ReadLocationIds(JsonElement element)
    {
        List<LocationId> locationIds = [];
        foreach (JsonElement entry in RequireBoundedArray(element))
        {
            locationIds.Add(new LocationId(SaveSnapshotJsonDeserializer.ReadString(entry)));
        }

        return locationIds;
    }

    private static List<MapNoteSaveEntry> ReadNotes(JsonElement element)
    {
        List<MapNoteSaveEntry> notes = [];
        foreach (JsonElement entry in RequireBoundedArray(element))
        {
            SaveSnapshotJsonDeserializer.RequireObject(entry, "locationId", "text");
            string text = ReadString(entry, "text");
            if (string.IsNullOrWhiteSpace(text) || text.Length > SeekerMapNote.MaximumTextLength)
            {
                throw SaveSnapshotJsonDeserializer.InvalidDocument();
            }

            notes.Add(new MapNoteSaveEntry(
                new LocationId(ReadString(entry, "locationId")),
                text));
        }

        return notes;
    }

    private static JsonElement.ArrayEnumerator RequireBoundedArray(JsonElement element)
    {
        SaveSnapshotJsonDeserializer.RequireArray(element);
        if (element.GetArrayLength() > MaximumLocationEntries)
        {
            throw SaveSnapshotJsonDeserializer.InvalidDocument();
        }

        return element.EnumerateArray();
    }

    private static string ReadString(JsonElement element, string propertyName) =>
        SaveSnapshotJsonDeserializer.ReadString(element.GetProperty(propertyName));
}
