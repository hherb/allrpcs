using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Diseases;

/// <summary>Retains one character's hidden authoritative state for one disease.</summary>
public sealed record CharacterDiseaseState
{
    /// <summary>Creates an immutable character disease state.</summary>
    public CharacterDiseaseState(
        CharacterId characterId,
        DiseaseDefinition disease,
        DiseaseStage stage,
        WorldEventId? lastProgressionEventId)
    {
        ArgumentNullException.ThrowIfNull(characterId);
        ArgumentNullException.ThrowIfNull(disease);
        ArgumentNullException.ThrowIfNull(stage);

        CharacterId = characterId;
        Disease = disease;
        Stage = stage;
        LastProgressionEventId = lastProgressionEventId;
    }

    /// <summary>Gets the exact infected character identity.</summary>
    public CharacterId CharacterId { get; }

    /// <summary>Gets the exact authoritative disease definition.</summary>
    public DiseaseDefinition Disease { get; }

    /// <summary>Gets the exact current authoritative stage.</summary>
    public DiseaseStage Stage { get; }

    /// <summary>Gets the exact meaningful-time event last consumed for progression, if any.</summary>
    public WorldEventId? LastProgressionEventId { get; }
}
