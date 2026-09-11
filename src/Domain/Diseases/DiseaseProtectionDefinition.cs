using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Diseases;

/// <summary>Defines one evidence-linked protective measure for an exact disease.</summary>
public sealed class DiseaseProtectionDefinition
{
    /// <summary>Creates an immutable protective-measure definition.</summary>
    public DiseaseProtectionDefinition(DiseaseDefinition disease, EvidenceId evidenceId)
    {
        ArgumentNullException.ThrowIfNull(disease);
        ArgumentNullException.ThrowIfNull(evidenceId);

        Disease = disease;
        EvidenceId = evidenceId;
    }

    /// <summary>Gets the exact disease for which this measure is protective.</summary>
    public DiseaseDefinition Disease { get; }

    /// <summary>Gets the evidence identity through which this measure can be discovered.</summary>
    public EvidenceId EvidenceId { get; }
}
