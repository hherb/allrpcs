using System.Text;
using System.Text.Json;
using Allrpcs.Domain.Persistence;
using static Allrpcs.Infrastructure.Persistence.SaveSnapshotJsonDeserializer;

namespace Allrpcs.Infrastructure.Persistence;

/// <summary>
/// Reads and writes the last confirmed Seeker profile as a small local settings document, so a
/// new game can start without asking again. The profile object has the same shape as in a save.
/// </summary>
public static class RememberedSeekerProfileJson
{
    private const int SchemaVersion = 1;

    /// <summary>Writes <c>{"schemaVersion":1,"seekerProfile":{...}}</c>.</summary>
    public static string Serialize(SeekerCustomizationSaveState profile)
    {
        ArgumentNullException.ThrowIfNull(profile);

        using MemoryStream stream = new();
        using (Utf8JsonWriter writer = new(stream))
        {
            writer.WriteStartObject();
            writer.WriteNumber("schemaVersion", SchemaVersion);
            writer.WritePropertyName("seekerProfile");
            SaveContinuationJsonWriter.WriteProfile(writer, profile);
            writer.WriteEndObject();
        }

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    /// <summary>Reads one exact document; anything else throws <see cref="ArgumentException"/>.</summary>
    public static SeekerCustomizationSaveState Deserialize(string serialized)
    {
        ArgumentNullException.ThrowIfNull(serialized);

        try
        {
            using JsonDocument document = JsonDocument.Parse(serialized);
            JsonElement root = document.RootElement;
            RequireObject(root, "schemaVersion", "seekerProfile");
            JsonElement version = root.GetProperty("schemaVersion");
            if (version.ValueKind != JsonValueKind.Number
                || !version.TryGetInt32(out int value)
                || value != SchemaVersion)
            {
                throw InvalidDocument();
            }

            return SaveContinuationJsonReader.ReadProfile(root.GetProperty("seekerProfile"));
        }
        catch (Exception exception) when (
            exception is JsonException
                or InvalidDataException
                or ArgumentException
                or InvalidOperationException
                or OverflowException)
        {
            throw new ArgumentException(
                "Value must be a complete remembered Seeker profile document.",
                nameof(serialized),
                exception);
        }
    }
}
