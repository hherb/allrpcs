using System.Text.Json;
using Allrpcs.Domain.Persistence;

namespace Allrpcs.Infrastructure.Persistence;

/// <summary>Writes the Seeker profile and body positions inside a continuation save document.</summary>
internal static class SaveContinuationJsonWriter
{
    /// <summary>Writes the state; a missing state is written as no profile and no positions.</summary>
    internal static void Write(Utf8JsonWriter writer, SaveContinuationState? state)
    {
        writer.WritePropertyName("continuation");
        writer.WriteStartObject();
        writer.WritePropertyName("seekerProfile");
        if (state?.SeekerProfile is { } profile)
        {
            writer.WriteStartObject();
            writer.WriteString("displayName", profile.Identity.DisplayName);
            writer.WriteString("subject", profile.Pronouns.Subject);
            writer.WriteString("objective", profile.Pronouns.Objective);
            writer.WriteString("possessiveDeterminer", profile.Pronouns.PossessiveDeterminer);
            writer.WriteString("possessivePronoun", profile.Pronouns.PossessivePronoun);
            writer.WriteString("reflexive", profile.Pronouns.Reflexive);
            writer.WriteString("appearanceKey", profile.AppearanceKey);
            writer.WriteString("backgroundKey", profile.BackgroundKey);
            writer.WriteEndObject();
        }
        else
        {
            writer.WriteNullValue();
        }

        writer.WritePropertyName("characterPositions");
        writer.WriteStartArray();
        foreach (CharacterPositionSaveState position in state?.Positions ?? [])
        {
            writer.WriteStartObject();
            writer.WriteString("characterId", position.CharacterId.Value);
            writer.WriteNumber("x", position.X);
            writer.WriteNumber("y", position.Y);
            writer.WriteEndObject();
        }

        writer.WriteEndArray();
        writer.WriteEndObject();
    }
}
