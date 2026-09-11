using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Diseases;

/// <summary>Retains the exact infection and authored event for one successful cure.</summary>
public sealed class SuccessfulDiseaseCureOutcome
{
    /// <summary>Creates an immutable successful-cure outcome from authoritative provenance.</summary>
    public SuccessfulDiseaseCureOutcome(
        CharacterDiseaseState infection,
        WorldEvent worldEvent)
    {
        ArgumentNullException.ThrowIfNull(infection);
        ArgumentNullException.ThrowIfNull(worldEvent);

        Infection = infection;
        WorldEvent = worldEvent;
    }

    /// <summary>Gets the exact authoritative infection that was cured.</summary>
    public CharacterDiseaseState Infection { get; }

    /// <summary>Gets the exact authored world event that records the successful cure.</summary>
    public WorldEvent WorldEvent { get; }
}
