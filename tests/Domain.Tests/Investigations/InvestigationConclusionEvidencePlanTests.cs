using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Investigations;

namespace Allrpcs.Domain.Tests.Investigations;

public sealed class InvestigationConclusionEvidencePlanTests
{
    [Fact]
    public void CopiesAndRetainsExactAlternateRoutesInAuthoredOrder()
    {
        InvestigationConclusion conclusion = InvestigationConclusionTests.Conclusion();
        InvestigationConclusionEvidenceRoute first = Route(conclusion, "note", "experiment");
        InvestigationConclusionEvidenceRoute second = Route(conclusion, "witness", "sample");
        InvestigationConclusionEvidenceRoute replacement = Route(conclusion, "replacement");
        InvestigationConclusionEvidenceRoute[] supplied = [first, second];

        InvestigationConclusionEvidencePlan plan = new(conclusion, supplied);
        supplied[0] = replacement;

        Assert.Same(conclusion, plan.Conclusion);
        Assert.Collection(
            plan.Routes,
            route => Assert.Same(first, route),
            route => Assert.Same(second, route));
        Assert.Throws<NotSupportedException>(() =>
            ((IList<InvestigationConclusionEvidenceRoute>)plan.Routes).Add(replacement));
    }

    [Fact]
    public void AcceptsTheMaximumDistinctRouteCount()
    {
        InvestigationConclusion conclusion = InvestigationConclusionTests.Conclusion();
        InvestigationConclusionEvidenceRoute[] routes = Enumerable
            .Range(0, InvestigationConclusionEvidencePlan.MaximumRouteCount)
            .Select(index => Route(conclusion, $"evidence-{index}"))
            .ToArray();

        InvestigationConclusionEvidencePlan plan = new(conclusion, routes);

        Assert.Equal(InvestigationConclusionEvidencePlan.MaximumRouteCount, plan.Routes.Count);
    }

    [Fact]
    public void RejectsNullEmptyExcessiveMismatchedAndDuplicateRouteCombinations()
    {
        InvestigationConclusion conclusion = InvestigationConclusionTests.Conclusion();
        InvestigationConclusion otherConclusion = Conclusion("other");
        InvestigationConclusionEvidenceRoute[] excessive = Enumerable
            .Range(0, InvestigationConclusionEvidencePlan.MaximumRouteCount + 1)
            .Select(index => Route(conclusion, $"evidence-{index}"))
            .ToArray();

        Assert.Throws<ArgumentNullException>(() =>
            new InvestigationConclusionEvidencePlan(null!, [Route(conclusion, "evidence")]));
        Assert.Throws<ArgumentNullException>(() =>
            new InvestigationConclusionEvidencePlan(conclusion, null!));
        Assert.Throws<ArgumentException>(() =>
            new InvestigationConclusionEvidencePlan(conclusion, []));
        Assert.Throws<ArgumentException>(() =>
            new InvestigationConclusionEvidencePlan(conclusion, [null!]));
        Assert.Throws<ArgumentException>(() =>
            new InvestigationConclusionEvidencePlan(
                conclusion,
                [Route(otherConclusion, "evidence")]));
        Assert.Throws<ArgumentException>(() =>
            new InvestigationConclusionEvidencePlan(
                conclusion,
                [Route(conclusion, "first", "second"), Route(conclusion, "second", "first")]));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new InvestigationConclusionEvidencePlan(conclusion, excessive));
    }

    [Fact]
    public void ExposesOnlyTheImmutablePlanSurface()
    {
        Type planType = typeof(InvestigationConclusionEvidencePlan);
        System.Reflection.PropertyInfo[] properties = planType.GetProperties();
        System.Reflection.ConstructorInfo constructor = Assert.Single(planType.GetConstructors());

        Assert.Equal(["Conclusion", "Routes"], properties.Select(property => property.Name).Order());
        Assert.All(properties, property => Assert.False(property.CanWrite));
        Assert.Equal(
            [
                typeof(InvestigationConclusion),
                typeof(IEnumerable<InvestigationConclusionEvidenceRoute>),
            ],
            constructor.GetParameters().Select(parameter => parameter.ParameterType).ToArray());
        Assert.Empty(planType.GetEvents());
        Assert.Empty(planType.GetMethods().Where(
            method => method.DeclaringType == planType && !method.IsSpecialName));
        Assert.True(planType.IsSealed);
    }

    internal static InvestigationConclusionEvidenceRoute Route(
        InvestigationConclusion conclusion,
        params string[] evidenceIds) =>
        new(conclusion, evidenceIds.Select(evidenceId => new EvidenceId(evidenceId)));

    private static InvestigationConclusion Conclusion(string suffix)
    {
        InvestigationQuestion question = OpenInvestigationQuestionSnapshotTests.Question(
            $"question-{suffix}");
        InvestigationHypothesis hypothesis = InvestigationHypothesisSnapshotTests.Hypothesis(
            $"hypothesis-{suffix}",
            question);
        return new InvestigationConclusion(
            new InvestigationConclusionId($"conclusion-{suffix}"),
            question,
            hypothesis,
            "An authored conclusion.");
    }
}
