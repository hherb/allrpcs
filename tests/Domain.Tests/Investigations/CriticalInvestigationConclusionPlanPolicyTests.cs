using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Investigations;

namespace Allrpcs.Domain.Tests.Investigations;

public sealed class CriticalInvestigationConclusionPlanPolicyTests
{
    [Fact]
    public void AcceptsDistinctRoutesWithThreeSourcesNonCooperationAndSingleSourceRecovery()
    {
        PlanInput input = Input();

        Assert.True(CriticalInvestigationConclusionPlanPolicy.IsAcceptable(
            input.Plan,
            input.Sources));
    }

    [Fact]
    public void RejectsTooFewRoutesOrSourcesAndUnknownEvidence()
    {
        PlanInput input = Input();
        InvestigationConclusion conclusion = input.Plan.Conclusion;
        InvestigationConclusionEvidencePlan oneRoute = new(
            conclusion,
            [Route(conclusion, "laundry-residue", "sage-inscription")]);
        InvestigationConclusionEvidencePlan twoSources = new(
            conclusion,
            [
                Route(conclusion, "laundry-residue"),
                Route(conclusion, "sage-inscription"),
            ]);
        InvestigationConclusionEvidencePlan unknownEvidence = new(
            conclusion,
            [
                Route(conclusion, "laundry-residue", "unknown-evidence"),
                Route(conclusion, "healer-testimony", "failed-dose"),
            ]);

        Assert.False(CriticalInvestigationConclusionPlanPolicy.IsAcceptable(
            oneRoute,
            input.Sources));
        Assert.False(CriticalInvestigationConclusionPlanPolicy.IsAcceptable(
            twoSources,
            input.Sources));
        Assert.False(CriticalInvestigationConclusionPlanPolicy.IsAcceptable(
            unknownEvidence,
            input.Sources));
    }

    [Fact]
    public void RejectsNoNonCooperationRouteOrNoSingleSourceRecovery()
    {
        PlanInput input = Input();
        InvestigationConclusion conclusion = input.Plan.Conclusion;
        InvestigationEvidenceSourceSnapshot allCooperative = new(
            input.Sources.Sources.Select(source => new InvestigationEvidenceSource(
                source.SourceId,
                source.EvidenceId,
                true)));
        InvestigationConclusionEvidencePlan sharedSourceRoutes = new(
            conclusion,
            [
                Route(conclusion, "laundry-residue", "sage-inscription"),
                Route(conclusion, "laundry-water", "failed-dose"),
            ]);

        Assert.False(CriticalInvestigationConclusionPlanPolicy.IsAcceptable(
            input.Plan,
            allCooperative));
        Assert.False(CriticalInvestigationConclusionPlanPolicy.IsAcceptable(
            sharedSourceRoutes,
            input.Sources));
    }

    [Fact]
    public void LeavesInputsUnchangedAndExposesOnlyOnePureOperation()
    {
        PlanInput input = Input();
        InvestigationConclusionEvidenceRoute firstRoute = input.Plan.Routes[0];
        InvestigationEvidenceSource firstSource = input.Sources.Sources[0];

        _ = CriticalInvestigationConclusionPlanPolicy.IsAcceptable(input.Plan, input.Sources);

        Assert.Same(firstRoute, input.Plan.Routes[0]);
        Assert.Same(firstSource, input.Sources.Sources[0]);

        Type policyType = typeof(CriticalInvestigationConclusionPlanPolicy);
        System.Reflection.MethodInfo method = Assert.Single(policyType.GetMethods().Where(
            candidate => candidate.DeclaringType == policyType));

        Assert.Equal("IsAcceptable", method.Name);
        Assert.Equal(typeof(bool), method.ReturnType);
        Assert.Equal(
            [typeof(InvestigationConclusionEvidencePlan), typeof(InvestigationEvidenceSourceSnapshot)],
            method.GetParameters().Select(parameter => parameter.ParameterType).ToArray());
        Assert.Empty(policyType.GetConstructors());
        Assert.Empty(policyType.GetProperties());
        Assert.Empty(policyType.GetEvents());
        Assert.True(policyType.IsAbstract && policyType.IsSealed);
    }

    [Fact]
    public void RejectsNullInputs()
    {
        PlanInput input = Input();

        Assert.Throws<ArgumentNullException>(() =>
            CriticalInvestigationConclusionPlanPolicy.IsAcceptable(null!, input.Sources));
        Assert.Throws<ArgumentNullException>(() =>
            CriticalInvestigationConclusionPlanPolicy.IsAcceptable(input.Plan, null!));
    }

    internal static PlanInput Input()
    {
        InvestigationQuestion question = InvestigationHypothesisRecognitionPolicyTests.Question();
        InvestigationHypothesis hypothesis =
            InvestigationHypothesisRecognitionPolicyTests.Hypothesis(question);
        InvestigationConclusion conclusion = new(
            new InvestigationConclusionId("cooling-tonic-confirmed"),
            question,
            hypothesis,
            "Cooling preserves the stabilizing property.");
        InvestigationConclusionEvidencePlan plan = new(
            conclusion,
            [
                Route(conclusion, "laundry-residue", "sage-inscription"),
                Route(conclusion, "healer-testimony", "failed-dose"),
            ]);
        InvestigationEvidenceSourceSnapshot sources = new(
            [
                Source("laundry", "laundry-residue", false),
                Source("laundry", "laundry-water", false),
                Source("shrine", "sage-inscription", false),
                Source("healer", "healer-testimony", true),
                Source("patient-experiment", "failed-dose", false),
            ]);

        return new PlanInput(plan, sources);
    }

    private static InvestigationConclusionEvidenceRoute Route(
        InvestigationConclusion conclusion,
        params string[] evidenceIds) =>
        new(conclusion, evidenceIds.Select(evidenceId => new EvidenceId(evidenceId)));

    private static InvestigationEvidenceSource Source(
        string sourceId,
        string evidenceId,
        bool requiresCooperation) =>
        InvestigationEvidenceSourceSnapshotTests.Source(
            sourceId,
            evidenceId,
            requiresCooperation);

    internal sealed record PlanInput(
        InvestigationConclusionEvidencePlan Plan,
        InvestigationEvidenceSourceSnapshot Sources);
}
