using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Ingredients;
using Allrpcs.Domain.Investigations;

namespace Allrpcs.Domain.Diseases;

/// <summary>Composes exact authored state for the first patient before stabilization.</summary>
public sealed class FirstPatientScenario
{
    /// <summary>Creates one immutable first-patient scenario without applying any outcome.</summary>
    public FirstPatientScenario(
        CharacterId patientId,
        DiseaseTruthSnapshot diseaseTruth,
        IngredientStateSnapshot ingredients,
        StabilizingRemedyDefinition remedy,
        CriticalInvestigationConclusionScenario investigation)
    {
        ArgumentNullException.ThrowIfNull(patientId);
        ArgumentNullException.ThrowIfNull(diseaseTruth);
        ArgumentNullException.ThrowIfNull(ingredients);
        ArgumentNullException.ThrowIfNull(remedy);
        ArgumentNullException.ThrowIfNull(investigation);

        PatientId = patientId;
        DiseaseTruth = diseaseTruth;
        Ingredients = ingredients;
        Remedy = remedy;
        Investigation = investigation;
    }

    /// <summary>Gets the exact authored first-patient identity.</summary>
    public CharacterId PatientId { get; }

    /// <summary>Gets the exact hidden authoritative disease truth.</summary>
    public DiseaseTruthSnapshot DiseaseTruth { get; }

    /// <summary>Gets the exact available ingredient state.</summary>
    public IngredientStateSnapshot Ingredients { get; }

    /// <summary>Gets the exact authored stabilizing-remedy definition.</summary>
    public StabilizingRemedyDefinition Remedy { get; }

    /// <summary>Gets the exact recoverable critical investigation.</summary>
    public CriticalInvestigationConclusionScenario Investigation { get; }
}
