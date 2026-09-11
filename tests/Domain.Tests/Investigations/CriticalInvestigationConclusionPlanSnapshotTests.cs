using Allrpcs.Domain.Investigations;

namespace Allrpcs.Domain.Tests.Investigations;

public sealed class CriticalInvestigationConclusionPlanSnapshotTests
{
    [Fact]
    public void CopiesPlansInStableOrderAndRetainsExactInstances()
    {
        InvestigationConclusionEvidencePlan first = Plan("first-conclusion");
        InvestigationConclusionEvidencePlan second = Plan("second-conclusion");
        InvestigationConclusionEvidencePlan replacement = Plan("replacement-conclusion");
        InvestigationConclusionEvidencePlan[] supplied = [first, second];

        CriticalInvestigationConclusionPlanSnapshot snapshot = new(supplied);
        supplied[0] = replacement;

        Assert.Collection(
            snapshot.Plans,
            plan => Assert.Same(first, plan),
            plan => Assert.Same(second, plan));
        Assert.Throws<NotSupportedException>(() =>
            ((IList<InvestigationConclusionEvidencePlan>)snapshot.Plans).Add(replacement));
    }

    [Fact]
    public void AcceptsEmptyAndMaximumCountAndRejectsInvalidCollections()
    {
        InvestigationConclusionEvidencePlan[] maximum = Enumerable
            .Range(0, CriticalInvestigationConclusionPlanSnapshot.MaximumPlanCount)
            .Select(index => Plan($"conclusion-{index}"))
            .ToArray();

        Assert.Empty(new CriticalInvestigationConclusionPlanSnapshot([]).Plans);
        Assert.Equal(
            CriticalInvestigationConclusionPlanSnapshot.MaximumPlanCount,
            new CriticalInvestigationConclusionPlanSnapshot(maximum).Plans.Count);
        Assert.Throws<ArgumentNullException>(() =>
            new CriticalInvestigationConclusionPlanSnapshot(null!));
        Assert.Throws<ArgumentException>(() =>
            new CriticalInvestigationConclusionPlanSnapshot([null!]));
        Assert.Throws<ArgumentException>(() =>
            new CriticalInvestigationConclusionPlanSnapshot(
                [Plan("duplicate"), Plan("duplicate")]));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new CriticalInvestigationConclusionPlanSnapshot(
                maximum.Append(Plan("extra-conclusion"))));
    }

    [Fact]
    public void ExposesOnlyTheReadOnlyPlanSurface()
    {
        Type snapshotType = typeof(CriticalInvestigationConclusionPlanSnapshot);
        System.Reflection.PropertyInfo property = Assert.Single(snapshotType.GetProperties());
        System.Reflection.ConstructorInfo constructor = Assert.Single(snapshotType.GetConstructors());

        Assert.Equal("Plans", property.Name);
        Assert.Equal(typeof(IReadOnlyList<InvestigationConclusionEvidencePlan>), property.PropertyType);
        Assert.False(property.CanWrite);
        Assert.Equal(
            typeof(IEnumerable<InvestigationConclusionEvidencePlan>),
            Assert.Single(constructor.GetParameters()).ParameterType);
        Assert.Empty(snapshotType.GetEvents());
        Assert.Empty(snapshotType.GetMethods().Where(
            method => method.DeclaringType == snapshotType && !method.IsSpecialName));
        Assert.True(snapshotType.IsSealed);
    }

    internal static InvestigationConclusionEvidencePlan Plan(string conclusionId)
    {
        CriticalInvestigationConclusionPlanPolicyTests.PlanInput input =
            CriticalInvestigationConclusionPlanPolicyTests.Input();
        InvestigationConclusion source = input.Plan.Conclusion;
        InvestigationConclusion conclusion = new(
            new(conclusionId),
            source.Question,
            source.Hypothesis,
            source.Statement);
        return new InvestigationConclusionEvidencePlan(
            conclusion,
            input.Plan.Routes.Select(route => new InvestigationConclusionEvidenceRoute(
                conclusion,
                route.RequiredEvidenceIds)));
    }
}
