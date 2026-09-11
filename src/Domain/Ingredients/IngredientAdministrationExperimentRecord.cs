using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Ingredients;

/// <summary>Records one exact ingredient administration and its qualitative observation.</summary>
public sealed class IngredientAdministrationExperimentRecord
{
    /// <summary>Creates an immutable experiment record without interpreting its observation.</summary>
    public IngredientAdministrationExperimentRecord(
        ExperimentId experimentId,
        IngredientAdministrationOutcome administration,
        CharacterConditionSet observedConditions)
    {
        ArgumentNullException.ThrowIfNull(experimentId);
        ArgumentNullException.ThrowIfNull(administration);
        ArgumentNullException.ThrowIfNull(observedConditions);

        ExperimentId = experimentId;
        Administration = administration;
        ObservedConditions = observedConditions;
    }

    /// <summary>Gets the exact canonical experiment identity.</summary>
    public ExperimentId ExperimentId { get; }

    /// <summary>Gets the exact authored administration occurrence.</summary>
    public IngredientAdministrationOutcome Administration { get; }

    /// <summary>Gets the exact observed qualitative condition set.</summary>
    public CharacterConditionSet ObservedConditions { get; }
}
