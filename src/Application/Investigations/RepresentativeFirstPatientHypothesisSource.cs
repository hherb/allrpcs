using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Investigations;

namespace Allrpcs.Application.Investigations;

/// <summary>Authors the hypotheses a player can propose about the first patient.</summary>
public static class RepresentativeFirstPatientHypothesisSource
{
    /// <summary>Creates the plan's remedy hypothesis and the station look-alike hypothesis.</summary>
    public static IReadOnlyList<FirstPatientHypothesisTest> Create(FirstPatientScenario scenario)
    {
        ArgumentNullException.ThrowIfNull(scenario);

        InvestigationHypothesis remedy = scenario.Investigation.Plans.Plans.Single().Conclusion.Hypothesis;
        InvestigationHypothesis lookAlike = new(
            new InvestigationHypothesisId("station-leaves-stabilize"),
            remedy.Question,
            "A cooled preparation of the station's unlabelled dried leaves may stabilize the patient's breathing.");
        return
        [
            new(remedy, scenario.Remedy.Recipe),
            new(lookAlike, scenario.AlternativeRecipes.Single()),
        ];
    }
}
