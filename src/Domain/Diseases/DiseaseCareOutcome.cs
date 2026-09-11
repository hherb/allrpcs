using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Diseases;

/// <summary>Describes one authored comfort or stabilization outcome.</summary>
public sealed class DiseaseCareOutcome
{
    /// <summary>Creates an outcome retaining exact authoritative state and event provenance.</summary>
    public DiseaseCareOutcome(
        CharacterDiseaseState infection,
        DiseaseCareState resultingCareState,
        WorldEvent worldEvent)
    {
        ArgumentNullException.ThrowIfNull(infection);
        ArgumentNullException.ThrowIfNull(resultingCareState);
        ArgumentNullException.ThrowIfNull(worldEvent);

        if (ReferenceEquals(resultingCareState, DiseaseCareState.Unassisted))
        {
            throw new ArgumentException(
                "A care outcome must provide comfort or stabilization.",
                nameof(resultingCareState));
        }

        Infection = infection;
        ResultingCareState = resultingCareState;
        WorldEvent = worldEvent;
    }

    /// <summary>Gets the exact authoritative infection receiving care.</summary>
    public CharacterDiseaseState Infection { get; }

    /// <summary>Gets the exact authored resulting care state.</summary>
    public DiseaseCareState ResultingCareState { get; }

    /// <summary>Gets the exact authored event recording the care outcome.</summary>
    public WorldEvent WorldEvent { get; }
}
