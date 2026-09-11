using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Diseases;

/// <summary>Retains authoritative provenance for one disease-transmission exposure.</summary>
public sealed class DiseaseExposureOutcome
{
    /// <summary>Creates an immutable exposure outcome from exact authored values.</summary>
    public DiseaseExposureOutcome(
        CharacterId characterId,
        DiseaseDefinition disease,
        WorldEvent worldEvent)
    {
        ArgumentNullException.ThrowIfNull(characterId);
        ArgumentNullException.ThrowIfNull(disease);
        ArgumentNullException.ThrowIfNull(worldEvent);

        CharacterId = characterId;
        Disease = disease;
        WorldEvent = worldEvent;
    }

    /// <summary>Gets the exposed character identity.</summary>
    public CharacterId CharacterId { get; }

    /// <summary>Gets the exact authored disease involved in the exposure.</summary>
    public DiseaseDefinition Disease { get; }

    /// <summary>Gets the exact authored event that caused the exposure.</summary>
    public WorldEvent WorldEvent { get; }
}
