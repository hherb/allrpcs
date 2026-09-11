using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Diseases;

/// <summary>Establishes one engine-owned disease identity independently of character knowledge.</summary>
public sealed record DiseaseDefinition
{
    /// <summary>Creates an immutable authoritative disease definition.</summary>
    public DiseaseDefinition(DiseaseId diseaseId)
    {
        ArgumentNullException.ThrowIfNull(diseaseId);
        DiseaseId = diseaseId;
    }

    /// <summary>Gets the exact canonical disease identity.</summary>
    public DiseaseId DiseaseId { get; }
}
