using System.Text.Json;
using Allrpcs.Application.Audio;

namespace Allrpcs.Infrastructure.Audio;

/// <summary>Strict versioned codec rejecting duplicate, unknown and partial preference fields.</summary>
public static class AudioPreferencesJson
{
    /// <summary>Serializes the complete stable-order snapshot with no runtime or world state.</summary>
    public static string Serialize(AudioPreferences preferences)
    {
        ArgumentNullException.ThrowIfNull(preferences);
        return JsonSerializer.Serialize(new
        {
            schemaVersion = 1,
            preset = preferences.Preset.ToString(),
            mono = preferences.Mono,
            captions = preferences.Captions,
            channels = preferences.Channels.Select(item => new
            {
                category = item.Category.ToString(),
                gain = item.Gain,
                muted = item.Muted,
            }),
        });
    }

    /// <summary>Materializes one complete snapshot; rejects malformed input without partial application.</summary>
    public static AudioPreferences Deserialize(string serialized)
    {
        ArgumentNullException.ThrowIfNull(serialized);
        try
        {
            using JsonDocument document = JsonDocument.Parse(serialized);
            JsonElement root = document.RootElement;
            Fields(root, "schemaVersion", "preset", "mono", "captions", "channels");
            if (!root.GetProperty("schemaVersion").TryGetInt32(out int version) || version != 1)
            {
                throw new ArgumentException("Unknown audio preferences version.", nameof(serialized));
            }
            AudioMixPreset preset = ExactEnum<AudioMixPreset>(root.GetProperty("preset"));
            List<AudioChannelSetting> channels = [];
            foreach (JsonElement item in root.GetProperty("channels").EnumerateArray())
            {
                Fields(item, "category", "gain", "muted");
                channels.Add(new AudioChannelSetting(ExactEnum<AudioCategory>(item.GetProperty("category")),
                    item.GetProperty("gain").GetDouble(), item.GetProperty("muted").GetBoolean()));
            }
            return new AudioPreferences(channels, preset,
                root.GetProperty("mono").GetBoolean(), root.GetProperty("captions").GetBoolean());
        }
        catch (Exception exception) when (exception is JsonException or InvalidOperationException or FormatException)
        {
            throw new ArgumentException("Invalid audio preferences document.", nameof(serialized), exception);
        }
    }

    private static T ExactEnum<T>(JsonElement element) where T : struct, Enum
    {
        string? text = element.GetString();
        if (text is null || !Enum.TryParse(text, out T value) || !Enum.IsDefined(value) || value.ToString() != text)
        {
            throw new ArgumentException("Unknown audio preference value.");
        }
        return value;
    }

    private static void Fields(JsonElement element, params string[] expected)
    {
        string[] actual = element.EnumerateObject().Select(property => property.Name).ToArray();
        if (actual.Length != expected.Length || actual.Distinct(StringComparer.Ordinal).Count() != actual.Length
            || !actual.Order(StringComparer.Ordinal).SequenceEqual(expected.Order(StringComparer.Ordinal)))
        {
            throw new ArgumentException("Missing, duplicate or unknown audio preference fields.");
        }
    }
}
