using System.Text.Json;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Persistence;
using static Allrpcs.Infrastructure.Persistence.SaveSnapshotJsonDeserializer;

namespace Allrpcs.Infrastructure.Persistence;

/// <summary>Strictly reads the continuation section of a save document.</summary>
internal static class SaveContinuationJsonReader
{
    /// <summary>Reads the exact profile and position structure, rejecting anything else.</summary>
    internal static SaveContinuationState Read(JsonElement element)
    {
        RequireObject(element, "seekerProfile", "characterPositions");
        JsonElement profileElement = element.GetProperty("seekerProfile");
        SeekerCustomizationSaveState? profile = profileElement.ValueKind == JsonValueKind.Null
            ? null
            : ReadProfile(profileElement);
        JsonElement positionsElement = element.GetProperty("characterPositions");
        RequireArray(positionsElement);
        List<CharacterPositionSaveState> positions = new();
        foreach (JsonElement position in positionsElement.EnumerateArray())
        {
            RequireObject(position, "characterId", "x", "y");
            positions.Add(new CharacterPositionSaveState(
                new CharacterId(ReadString(position.GetProperty("characterId"))),
                ReadSingle(position.GetProperty("x")),
                ReadSingle(position.GetProperty("y"))));
        }

        return new SaveContinuationState(profile, positions);
    }

    private static SeekerCustomizationSaveState ReadProfile(JsonElement element)
    {
        RequireObject(
            element,
            "displayName",
            "subject",
            "objective",
            "possessiveDeterminer",
            "possessivePronoun",
            "reflexive",
            "appearanceKey",
            "backgroundKey");
        string Text(string name) => ReadString(element.GetProperty(name));
        return new SeekerCustomizationSaveState(
            new CharacterIdentity(
                new CharacterId(SeekerCustomizationProfile.SeekerCharacterIdValue),
                Text("displayName")),
            new SeekerPronouns(
                Text("subject"),
                Text("objective"),
                Text("possessiveDeterminer"),
                Text("possessivePronoun"),
                Text("reflexive")),
            Text("appearanceKey"),
            Text("backgroundKey"));
    }

    private static float ReadSingle(JsonElement element) =>
        element.ValueKind == JsonValueKind.Number && element.TryGetSingle(out float value)
            ? value
            : throw InvalidDocument();
}
