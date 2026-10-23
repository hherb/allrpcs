using System.Text.Json.Nodes;
using Allrpcs.Application.Input;
using Allrpcs.Infrastructure.Input;

namespace Allrpcs.Infrastructure.Tests.Input;

public sealed class ControlPreferencesJsonTests
{
    [Fact]
    public void StableCompleteRoundTripPreservesAllEventKinds()
    {
        ControlPreferences value = ControlBindingEdit.Set(ControlPreferences.Defaults(), "save_current", new("mouseButton", 8));
        value = ControlBindingEdit.Set(value, "load_current", new("key", 4194332, "physical", ctrl: true), 0);
        string serialized = ControlPreferencesJson.Serialize(value);
        Assert.Equal(serialized, ControlPreferencesJson.Serialize(ControlPreferencesJson.Deserialize(serialized)));
        Assert.DoesNotContain("device", serialized); Assert.DoesNotContain("path", serialized);
        Assert.Equal(30, ControlPreferencesJson.Deserialize(serialized).Actions.Count);
    }
    [Fact]
    public void MissingActionsReceiveDefaultsWithoutChangingCustomRowsAndMigrationIsIdempotent()
    {
        ControlPreferences customized = ControlBindingEdit.Set(ControlPreferences.Defaults(), "save_current", new("mouseButton", 8));
        JsonNode root = JsonNode.Parse(ControlPreferencesJson.Serialize(customized))!;
        JsonArray rows = root["actions"]!.AsArray();
        rows.Remove(rows.Single(row => (string?)row!["id"] == "load_current"));
        string migrated = ControlPreferencesJson.Serialize(ControlPreferencesJson.Deserialize(root.ToJsonString()));
        Assert.Equal(ControlPreferencesJson.Serialize(customized), migrated);
        Assert.Equal(migrated, ControlPreferencesJson.Serialize(ControlPreferencesJson.Deserialize(migrated)));
    }

    [Fact]
    public void MissingDefaultConflictingWithCustomRowFailsWithoutOverwritingFile()
    {
        JsonNode root = JsonNode.Parse(ControlPreferencesJson.Serialize(ControlPreferences.Defaults()))!;
        JsonArray rows = root["actions"]!.AsArray();
        JsonNode load = rows.Single(row => (string?)row!["id"] == "load_current")!;
        rows.Single(row => (string?)row!["id"] == "save_current")!["bindings"] = load["bindings"]!.DeepClone();
        rows.Remove(load);
        string text = root.ToJsonString();
        Assert.Throws<ArgumentException>(() => ControlPreferencesJson.Deserialize(text));
        string path = Path.Combine(Path.GetTempPath(), "allrpcs-controls-migration-" + Guid.NewGuid().ToString("N"));
        try
        {
            File.WriteAllText(path, text);
            Assert.Equal(ControlPreferencesLoadStatus.Invalid, new FileControlPreferencesStore(path).Load().Status);
            Assert.Equal(text, File.ReadAllText(path));
        }
        finally { File.Delete(path); }
    }

    [Theory]
    [InlineData("version")]
    [InlineData("unknownRoot")]
    [InlineData("missingRoot")]
    [InlineData("duplicateAction")]
    [InlineData("unknownAction")]
    [InlineData("emptyEvents")]
    [InlineData("unknownKind")]
    [InlineData("wrongFields")]
    [InlineData("wrongBool")]
    [InlineData("badCode")]
    [InlineData("fractionCode")]
    [InlineData("stringCode")]
    [InlineData("badDeadzone")]
    [InlineData("conflict")]
    [InlineData("unknownMode")]
    public void RejectsMalformedWholeDocuments(string mode)
    {
        JsonNode root = JsonNode.Parse(ControlPreferencesJson.Serialize(ControlPreferences.Defaults()))!;
        JsonArray rows = root["actions"]!.AsArray(); JsonNode key = rows[0]!["bindings"]!.AsArray().Single(b => (string?)b!["kind"] == "key")!;
        switch (mode)
        {
            case "version": root["schemaVersion"] = 2; break;
            case "unknownRoot": root["extra"] = true; break;
            case "missingRoot": root.AsObject().Remove("schemaVersion"); break;
            case "duplicateAction": rows[1] = rows[0]!.DeepClone(); break;
            case "unknownAction": rows[0]!["id"] = "MOVE_LEFT"; break;
            case "emptyEvents": rows[0]!["bindings"] = new JsonArray(); break;
            case "unknownKind": key["kind"] = "resource"; break;
            case "wrongFields": key["device"] = 0; break;
            case "wrongBool": key["shift"] = "true"; break;
            case "badCode": key["code"] = 0; break;
            case "fractionCode": key["code"] = 65.5; break;
            case "stringCode": key["code"] = "65"; break;
            case "badDeadzone": rows[0]!["deadzone"] = 1; break;
            case "conflict": key["code"] = 68; break;
            case "unknownMode": key["codeMode"] = "unicode"; break;
        }
        Assert.Throws<ArgumentException>(() => ControlPreferencesJson.Deserialize(root.ToJsonString()));
    }
    [Theory]
    [InlineData("{\"schemaVersion\":1,\"schemaVersion\":1,\"actions\":[]}")]
    [InlineData("{\"schemaVersion\":1,\"actions\":[],\"actions\":[]}")]
    [InlineData("null")]
    [InlineData("[]")]
    [InlineData("{")]
    public void RejectsDuplicateFieldsAndBrokenJson(string text) => Assert.Throws<ArgumentException>(() => ControlPreferencesJson.Deserialize(text));
    [Fact]
    public void ByteLimitIsInclusiveAndStrictUtf8RejectsInvalidSurrogate()
    {
        string text = ControlPreferencesJson.Serialize(ControlPreferences.Defaults());
        Assert.NotNull(ControlPreferencesJson.Deserialize(text.PadRight(65536)));
        Assert.Throws<ArgumentException>(() => ControlPreferencesJson.Deserialize(text.PadRight(65537)));
        Assert.Throws<ArgumentException>(() => ControlPreferencesJson.Deserialize("\uD800"));
    }
}
