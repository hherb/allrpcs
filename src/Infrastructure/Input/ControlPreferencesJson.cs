using System.Text;
using System.Text.Json;
using Allrpcs.Application.Input;

namespace Allrpcs.Infrastructure.Input;

/// <summary>Strict bounded primitive-only version-one controls codec.</summary>
public static class ControlPreferencesJson
{
    /// <summary>Maximum UTF-8 document size.</summary>
    public const int MaximumBytes = 65536;
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);
    /// <summary>Writes complete canonical action/event order and exact per-kind fields.</summary>
    public static string Serialize(ControlPreferences preferences)
    {
        ArgumentNullException.ThrowIfNull(preferences);
        using MemoryStream bytes = new();
        using (Utf8JsonWriter writer = new(bytes))
        {
            writer.WriteStartObject(); writer.WriteNumber("schemaVersion", 1); writer.WriteStartArray("actions");
            foreach (ControlActionSetting row in preferences.Actions)
            {
                writer.WriteStartObject(); writer.WriteString("id", row.Id); writer.WriteNumber("deadzone", row.Deadzone);
                writer.WriteStartArray("bindings");
                foreach (ControlBinding binding in row.Bindings)
                {
                    writer.WriteStartObject(); writer.WriteString("kind", binding.Kind);
                    if (binding.Kind == "key")
                    {
                        writer.WriteString("codeMode", binding.CodeMode); writer.WriteNumber("code", binding.Code);
                        writer.WriteBoolean("shift", binding.Shift); writer.WriteBoolean("ctrl", binding.Ctrl);
                        writer.WriteBoolean("alt", binding.Alt); writer.WriteBoolean("meta", binding.Meta);
                    }
                    else if (binding.Kind == "joyAxis")
                    { writer.WriteNumber("axis", binding.Code); writer.WriteNumber("sign", binding.Sign); }
                    else { writer.WriteNumber("button", binding.Code); }
                    writer.WriteEndObject();
                }
                writer.WriteEndArray(); writer.WriteEndObject();
            }
            writer.WriteEndArray(); writer.WriteEndObject();
        }
        if (bytes.Length > MaximumBytes) { throw new ArgumentException("Controls document is too large."); }
        return StrictUtf8.GetString(bytes.ToArray());
    }
    /// <summary>Rejects all incomplete, duplicate, unknown or invalid data before materializing.</summary>
    public static ControlPreferences Deserialize(string serialized)
    {
        ArgumentNullException.ThrowIfNull(serialized);
        try
        {
            if (StrictUtf8.GetByteCount(serialized) > MaximumBytes) { throw new ArgumentException("Controls document is too large."); }
            using JsonDocument document = JsonDocument.Parse(serialized, new JsonDocumentOptions { MaxDepth = 8 });
            JsonElement root = document.RootElement; Fields(root, "schemaVersion", "actions");
            if (Integer(root.GetProperty("schemaVersion")) != 1) { throw new ArgumentException("Unknown controls schema."); }
            List<ControlActionSetting> rows = [];
            foreach (JsonElement row in root.GetProperty("actions").EnumerateArray())
            {
                if (rows.Count >= ControlActionCatalog.Actions.Count) { throw new ArgumentException("Too many actions."); }
                Fields(row, "id", "deadzone", "bindings"); List<ControlBinding> bindings = [];
                foreach (JsonElement binding in row.GetProperty("bindings").EnumerateArray())
                {
                    if (bindings.Count >= ControlActionCatalog.MaximumBindings) { throw new ArgumentException("Too many bindings."); }
                    string kind = binding.GetProperty("kind").GetString()!;
                    if (kind == "key")
                    {
                        Fields(binding, "kind", "codeMode", "code", "shift", "ctrl", "alt", "meta");
                        bindings.Add(new(kind, Integer(binding.GetProperty("code")), binding.GetProperty("codeMode").GetString()!,
                            shift: binding.GetProperty("shift").GetBoolean(), ctrl: binding.GetProperty("ctrl").GetBoolean(),
                            alt: binding.GetProperty("alt").GetBoolean(), meta: binding.GetProperty("meta").GetBoolean()));
                    }
                    else if (kind == "joyAxis")
                    { Fields(binding, "kind", "axis", "sign"); bindings.Add(new(kind, Integer(binding.GetProperty("axis")), sign: Integer(binding.GetProperty("sign")))); }
                    else
                    { Fields(binding, "kind", "button"); bindings.Add(new(kind, Integer(binding.GetProperty("button")))); }
                }
                rows.Add(new(row.GetProperty("id").GetString()!, row.GetProperty("deadzone").GetDouble(), bindings));
            }
            return new(rows);
        }
        catch (Exception e) when (e is JsonException or InvalidOperationException or FormatException or KeyNotFoundException
            or OverflowException or EncoderFallbackException)
        { throw new ArgumentException("Invalid controls document.", nameof(serialized), e); }
    }
    private static int Integer(JsonElement value)
    {
        string raw = value.GetRawText();
        if (value.ValueKind != JsonValueKind.Number || raw.Any(c => c is '.' or 'e' or 'E') || !value.TryGetInt32(out int result))
        { throw new ArgumentException("Controls codes/versions must be integers."); }
        return result;
    }
    private static void Fields(JsonElement value, params string[] expected)
    {
        string[] actual = value.EnumerateObject().Select(p => p.Name).ToArray();
        if (actual.Length != expected.Length || actual.Distinct(StringComparer.Ordinal).Count() != actual.Length
            || !actual.Order(StringComparer.Ordinal).SequenceEqual(expected.Order(StringComparer.Ordinal)))
        { throw new ArgumentException("Missing, duplicate or unknown controls fields."); }
    }
}
