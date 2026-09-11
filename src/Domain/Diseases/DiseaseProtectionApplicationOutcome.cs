using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Diseases;

/// <summary>Retains authoritative provenance for applying one discovered protection.</summary>
public sealed class DiseaseProtectionApplicationOutcome
{
    /// <summary>Creates an immutable protection-application outcome.</summary>
    public DiseaseProtectionApplicationOutcome(
        CharacterEvidenceViewSnapshot evidenceView,
        DiseaseProtectionDefinition definition,
        WorldEvent worldEvent)
    {
        ArgumentNullException.ThrowIfNull(evidenceView);
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(worldEvent);

        EvidenceView = evidenceView;
        Definition = definition;
        WorldEvent = worldEvent;
    }

    /// <summary>Gets the exact evidence view authorizing the character's discovery.</summary>
    public CharacterEvidenceViewSnapshot EvidenceView { get; }

    /// <summary>Gets the exact authored protective measure applied.</summary>
    public DiseaseProtectionDefinition Definition { get; }

    /// <summary>Gets the exact authored event recording the application.</summary>
    public WorldEvent WorldEvent { get; }
}
