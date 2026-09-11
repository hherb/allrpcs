using Allrpcs.Domain.Ingredients;
using Allrpcs.Domain.Investigations;

namespace Allrpcs.Domain.Diseases;

/// <summary>Checks exact authored first-patient treatment prerequisites without changing them.</summary>
public static class FirstPatientScenarioStructuralPolicy
{
    /// <summary>Reports whether disease, ingredient, and critical-route structure is coherent.</summary>
    public static bool IsAcceptable(FirstPatientScenario scenario)
    {
        ArgumentNullException.ThrowIfNull(scenario);

        bool hasExactPatientInfection = scenario.DiseaseTruth.Infections.Any(infection =>
            ReferenceEquals(infection.CharacterId, scenario.PatientId)
            && ReferenceEquals(infection.Disease, scenario.Remedy.Disease));

        return hasExactPatientInfection
            && IngredientPreparationAvailabilityPolicy.HasRequiredInputs(
                scenario.Remedy.Recipe,
                scenario.Ingredients)
            && CriticalInvestigationConclusionScenarioPolicy.IsAcceptable(
                scenario.Investigation.Plans,
                scenario.Investigation.Sources);
    }
}
