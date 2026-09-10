using System.Text.Json;
using Allrpcs.Application.Dialogue;

namespace Allrpcs.Infrastructure.Dialogue;

/// <summary>Strictly reads the supported secret-free provider-settings JSON schema.</summary>
public static class CharacterModelSettingsJsonDeserializer
{
    private const int MaximumJsonDepth = 4;

    /// <summary>Materializes one complete version-1 provider-settings selection.</summary>
    public static CharacterModelSettingsSelectionSnapshot Deserialize(string serialized)
    {
        ArgumentNullException.ThrowIfNull(serialized);

        try
        {
            using JsonDocument document = JsonDocument.Parse(
                serialized,
                new JsonDocumentOptions
                {
                    AllowTrailingCommas = false,
                    CommentHandling = JsonCommentHandling.Disallow,
                    MaxDepth = MaximumJsonDepth,
                });
            JsonElement root = document.RootElement;
            RequireObject(root, "schemaVersion", "selectedProvider", "settings");
            RequireVersion(root.GetProperty("schemaVersion"));

            CharacterModelProvider selectedProvider = CharacterModelProvider.Parse(
                ReadString(root.GetProperty("selectedProvider")));
            JsonElement settingsElement = root.GetProperty("settings");
            if (settingsElement.ValueKind != JsonValueKind.Array)
            {
                throw InvalidDocument();
            }

            List<CharacterModelSettings> settings = new();
            foreach (JsonElement entry in settingsElement.EnumerateArray())
            {
                RequireObject(entry, "provider", "modelName");
                settings.Add(
                    new CharacterModelSettings(
                        CharacterModelProvider.Parse(
                            ReadString(entry.GetProperty("provider"))),
                        ReadString(entry.GetProperty("modelName"))));
                if (settings.Count > CharacterModelSettingsSelectionSnapshot.MaximumSettingsCount)
                {
                    throw InvalidDocument();
                }
            }

            return new CharacterModelSettingsSelectionSnapshot(settings, selectedProvider);
        }
        catch (Exception exception) when (
            exception is JsonException
                or InvalidDataException
                or ArgumentException
                or InvalidOperationException)
        {
            throw new ArgumentException(
                "Value must be a complete supported character-model settings JSON document.",
                nameof(serialized),
                exception);
        }
    }

    private static void RequireVersion(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Number
            || !element.TryGetInt32(out int value)
            || value != 1
            || !string.Equals(element.GetRawText(), "1", StringComparison.Ordinal))
        {
            throw InvalidDocument();
        }
    }

    private static string ReadString(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.String)
        {
            throw InvalidDocument();
        }

        return element.GetString() ?? throw InvalidDocument();
    }

    private static void RequireObject(JsonElement element, params string[] propertyNames)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            throw InvalidDocument();
        }

        HashSet<string> expected = new(propertyNames, StringComparer.Ordinal);
        HashSet<string> actual = new(StringComparer.Ordinal);
        foreach (JsonProperty property in element.EnumerateObject())
        {
            if (!expected.Contains(property.Name) || !actual.Add(property.Name))
            {
                throw InvalidDocument();
            }
        }

        if (!actual.SetEquals(expected))
        {
            throw InvalidDocument();
        }
    }

    private static InvalidDataException InvalidDocument() =>
        new("Unexpected character-model settings JSON structure or value.");
}
