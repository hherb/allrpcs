using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.Ingredients;

namespace Allrpcs.Application.Investigations;

/// <summary>Reads recorded doses to decide what they say about one hypothesis.</summary>
public static class FirstPatientHypothesisTestPolicy
{
    /// <summary>Returns untested until the test's recipe is dosed; only the remedy stabilizes.</summary>
    public static InvestigationHypothesisTestOutcome Evaluate(
        FirstPatientHypothesisTest test,
        FirstPatientScenario scenario,
        IngredientExperimentJournalSnapshot doses)
    {
        ArgumentNullException.ThrowIfNull(test);
        ArgumentNullException.ThrowIfNull(scenario);
        ArgumentNullException.ThrowIfNull(doses);

        if (!doses.Records.Any(record => ReferenceEquals(
            record.Administration.Dosage.Preparation.Activity.Recipe,
            test.Recipe)))
        {
            return InvestigationHypothesisTestOutcome.Untested;
        }

        // FirstPatientAdministrationOccurrenceTransition stabilizes only for the remedy recipe.
        return ReferenceEquals(test.Recipe, scenario.Remedy.Recipe)
            ? InvestigationHypothesisTestOutcome.Confirmed
            : InvestigationHypothesisTestOutcome.Refuted;
    }
}
