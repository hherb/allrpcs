using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Investigations;

namespace Allrpcs.Domain.Tests.Investigations;

public sealed class RepresentativeFirstPatientInvestigationSourceTests
{
    [Fact]
    public void AuthorsOneSafeCriticalConclusionWithTwoWorkingEvidenceRoutes()
    {
        CriticalInvestigationConclusionScenario scenario =
            RepresentativeFirstPatientInvestigationSource.Create();

        Assert.True(CriticalInvestigationConclusionScenarioPolicy.IsAcceptable(
            scenario.Plans,
            scenario.Sources));
        InvestigationConclusionEvidencePlan plan = Assert.Single(scenario.Plans.Plans);
        Assert.Equal("cooling-moonwort-stabilizes", plan.Conclusion.ConclusionId.Value);
        Assert.Equal(2, plan.Routes.Count);
        Assert.All(plan.Routes, route => Assert.True(
            InvestigationConclusionConfirmationPolicy.CanConfirm(
                plan,
                new CharacterEvidenceViewSnapshot(
                    new CharacterId("seeker"),
                    route.RequiredEvidenceIds))));
    }

    [Fact]
    public void UsesFourDistinctSourcesAndRetainsACompleteNonCooperationRoute()
    {
        CriticalInvestigationConclusionScenario scenario =
            RepresentativeFirstPatientInvestigationSource.Create();
        InvestigationConclusionEvidencePlan plan = Assert.Single(scenario.Plans.Plans);
        Dictionary<EvidenceId, InvestigationEvidenceSource> sourcesByEvidence =
            scenario.Sources.Sources.ToDictionary(source => source.EvidenceId);

        Assert.Equal(
            4,
            scenario.Sources.Sources.Select(source => source.SourceId).Distinct().Count());
        Assert.Contains(plan.Routes, route => route.RequiredEvidenceIds.All(
            evidenceId => !sourcesByEvidence[evidenceId].RequiresCooperation));
    }

    [Fact]
    public void ExposesOnlyOnePureScenarioSourceOperation()
    {
        Type sourceType = typeof(RepresentativeFirstPatientInvestigationSource);
        System.Reflection.MethodInfo method = Assert.Single(sourceType.GetMethods().Where(
            candidate => candidate.DeclaringType == sourceType));

        Assert.Equal("Create", method.Name);
        Assert.Equal(typeof(CriticalInvestigationConclusionScenario), method.ReturnType);
        Assert.Empty(method.GetParameters());
        Assert.Empty(sourceType.GetConstructors());
        Assert.Empty(sourceType.GetProperties());
        Assert.Empty(sourceType.GetEvents());
        Assert.True(sourceType.IsAbstract && sourceType.IsSealed);
    }
}
