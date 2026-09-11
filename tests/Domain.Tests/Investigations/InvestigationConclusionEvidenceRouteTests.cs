using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Investigations;

namespace Allrpcs.Domain.Tests.Investigations;

public sealed class InvestigationConclusionEvidenceRouteTests
{
    [Fact]
    public void CopiesAndRetainsExactEvidenceInAuthoredOrder()
    {
        InvestigationConclusion conclusion = InvestigationConclusionTests.Conclusion();
        EvidenceId first = new("covered-water-note");
        EvidenceId second = new("cooling-experiment");
        EvidenceId replacement = new("replacement");
        EvidenceId[] supplied = [first, second];

        InvestigationConclusionEvidenceRoute route = new(conclusion, supplied);
        supplied[0] = replacement;

        Assert.Same(conclusion, route.Conclusion);
        Assert.Collection(
            route.RequiredEvidenceIds,
            evidenceId => Assert.Same(first, evidenceId),
            evidenceId => Assert.Same(second, evidenceId));
        Assert.Throws<NotSupportedException>(() =>
            ((IList<EvidenceId>)route.RequiredEvidenceIds).Add(replacement));
    }

    [Fact]
    public void AcceptsTheMaximumDistinctEvidenceCount()
    {
        EvidenceId[] evidenceIds = Enumerable
            .Range(0, InvestigationConclusionEvidenceRoute.MaximumEvidenceCount)
            .Select(index => new EvidenceId($"evidence-{index}"))
            .ToArray();

        InvestigationConclusionEvidenceRoute route = new(
            InvestigationConclusionTests.Conclusion(),
            evidenceIds);

        Assert.Equal(
            InvestigationConclusionEvidenceRoute.MaximumEvidenceCount,
            route.RequiredEvidenceIds.Count);
    }

    [Fact]
    public void RejectsNullEmptyNullEntryExcessiveAndDuplicateEvidence()
    {
        InvestigationConclusion conclusion = InvestigationConclusionTests.Conclusion();
        EvidenceId[] excessive = Enumerable
            .Range(0, InvestigationConclusionEvidenceRoute.MaximumEvidenceCount + 1)
            .Select(index => new EvidenceId($"evidence-{index}"))
            .ToArray();

        Assert.Throws<ArgumentNullException>(() =>
            new InvestigationConclusionEvidenceRoute(null!, [new EvidenceId("evidence")]));
        Assert.Throws<ArgumentNullException>(() =>
            new InvestigationConclusionEvidenceRoute(conclusion, null!));
        Assert.Throws<ArgumentException>(() =>
            new InvestigationConclusionEvidenceRoute(conclusion, []));
        Assert.Throws<ArgumentException>(() =>
            new InvestigationConclusionEvidenceRoute(conclusion, [null!]));
        Assert.Throws<ArgumentException>(() =>
            new InvestigationConclusionEvidenceRoute(
                conclusion,
                [new EvidenceId("evidence"), new EvidenceId("evidence")]));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new InvestigationConclusionEvidenceRoute(conclusion, excessive));
    }

    [Fact]
    public void ExposesOnlyTheImmutableRouteSurface()
    {
        Type routeType = typeof(InvestigationConclusionEvidenceRoute);
        System.Reflection.PropertyInfo[] properties = routeType.GetProperties();
        System.Reflection.ConstructorInfo constructor = Assert.Single(routeType.GetConstructors());

        Assert.Equal(
            ["Conclusion", "RequiredEvidenceIds"],
            properties.Select(property => property.Name).Order());
        Assert.All(properties, property => Assert.False(property.CanWrite));
        Assert.Equal(
            [typeof(InvestigationConclusion), typeof(IEnumerable<EvidenceId>)],
            constructor.GetParameters().Select(parameter => parameter.ParameterType).ToArray());
        Assert.Empty(routeType.GetEvents());
        Assert.Empty(routeType.GetMethods().Where(
            method => method.DeclaringType == routeType && !method.IsSpecialName));
        Assert.True(routeType.IsSealed);
    }
}
