using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Ingredients;

/// <summary>Describes one authored ingredient-dosage administration occurrence.</summary>
public sealed class IngredientAdministrationOutcome
{
    /// <summary>Creates an immutable administration outcome with exact event provenance.</summary>
    public IngredientAdministrationOutcome(
        CharacterId administratorId,
        CharacterId patientId,
        IngredientDosage dosage,
        WorldEvent worldEvent)
    {
        ArgumentNullException.ThrowIfNull(administratorId);
        ArgumentNullException.ThrowIfNull(patientId);
        ArgumentNullException.ThrowIfNull(dosage);
        ArgumentNullException.ThrowIfNull(worldEvent);

        AdministratorId = administratorId;
        PatientId = patientId;
        Dosage = dosage;
        WorldEvent = worldEvent;
    }

    /// <summary>Gets the exact character identity administering the dosage.</summary>
    public CharacterId AdministratorId { get; }

    /// <summary>Gets the exact patient identity receiving the dosage.</summary>
    public CharacterId PatientId { get; }

    /// <summary>Gets the exact prepared dosage being administered.</summary>
    public IngredientDosage Dosage { get; }

    /// <summary>Gets the exact authored event recording the administration.</summary>
    public WorldEvent WorldEvent { get; }
}
