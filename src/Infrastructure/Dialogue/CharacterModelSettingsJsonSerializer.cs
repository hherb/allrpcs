using System.Text;
using System.Text.Json;
using Allrpcs.Application.Dialogue;

namespace Allrpcs.Infrastructure.Dialogue;

/// <summary>Writes stable secret-free JSON for one provider-settings selection.</summary>
public static class CharacterModelSettingsJsonSerializer
{
    /// <summary>Serializes canonical provider identifiers and bounded model names only.</summary>
    public static string Serialize(CharacterModelSettingsSelectionSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        using MemoryStream stream = new();
        using (Utf8JsonWriter writer = new(stream))
        {
            writer.WriteStartObject();
            writer.WriteNumber("schemaVersion", 1);
            writer.WriteString("selectedProvider", snapshot.SelectedProvider.Value);
            writer.WritePropertyName("settings");
            writer.WriteStartArray();
            foreach (CharacterModelSettings settings in snapshot.Settings)
            {
                writer.WriteStartObject();
                writer.WriteString("provider", settings.Provider.Value);
                writer.WriteString("modelName", settings.ModelName);
                writer.WriteEndObject();
            }

            writer.WriteEndArray();
            writer.WriteEndObject();
        }

        return Encoding.UTF8.GetString(stream.ToArray());
    }
}
