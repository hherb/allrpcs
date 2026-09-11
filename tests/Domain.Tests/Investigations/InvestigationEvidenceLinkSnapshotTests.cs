using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Investigations;

namespace Allrpcs.Domain.Tests.Investigations;

public sealed class InvestigationEvidenceLinkSnapshotTests
{
    [Fact]
    public void CopiesLinksInStableOrderAndRetainsExactInstances()
    {
        InvestigationHypothesis hypothesis = Hypothesis("hypothesis");
        InvestigationEvidenceLink first = Link(hypothesis, "first-evidence");
        InvestigationEvidenceLink second = Link(hypothesis, "second-evidence");
        InvestigationEvidenceLink replacement = Link(hypothesis, "replacement");
        InvestigationEvidenceLink[] supplied = [first, second];

        InvestigationEvidenceLinkSnapshot snapshot = new(supplied);
        supplied[0] = replacement;

        Assert.Collection(
            snapshot.Links,
            link => Assert.Same(first, link),
            link => Assert.Same(second, link));
        Assert.Throws<NotSupportedException>(() =>
            ((IList<InvestigationEvidenceLink>)snapshot.Links).Add(replacement));
    }

    [Fact]
    public void AllowsSharedEvidenceAcrossHypothesesAndMultipleEvidencePerHypothesis()
    {
        InvestigationHypothesis first = Hypothesis("first-hypothesis");
        InvestigationHypothesis second = Hypothesis("second-hypothesis");

        InvestigationEvidenceLinkSnapshot snapshot = new(
            [
                Link(first, "shared-evidence"),
                Link(first, "other-evidence"),
                Link(second, "shared-evidence"),
            ]);

        Assert.Equal(3, snapshot.Links.Count);
    }

    [Fact]
    public void AcceptsEmptyAndMaximumCountAndRejectsInvalidCollections()
    {
        InvestigationHypothesis hypothesis = Hypothesis("hypothesis");
        InvestigationEvidenceLink[] maximum = Enumerable
            .Range(0, InvestigationEvidenceLinkSnapshot.MaximumLinkCount)
            .Select(index => Link(hypothesis, $"evidence-{index}"))
            .ToArray();

        Assert.Empty(new InvestigationEvidenceLinkSnapshot([]).Links);
        Assert.Equal(
            InvestigationEvidenceLinkSnapshot.MaximumLinkCount,
            new InvestigationEvidenceLinkSnapshot(maximum).Links.Count);
        Assert.Throws<ArgumentNullException>(() => new InvestigationEvidenceLinkSnapshot(null!));
        Assert.Throws<ArgumentException>(() => new InvestigationEvidenceLinkSnapshot([null!]));
        Assert.Throws<ArgumentException>(() => new InvestigationEvidenceLinkSnapshot(
            [
                Link(hypothesis, "duplicate"),
                new InvestigationEvidenceLink(
                    hypothesis,
                    new EvidenceId("duplicate"),
                    InvestigationEvidenceRelationship.Contradicts),
            ]));
        Assert.Throws<ArgumentOutOfRangeException>(() => new InvestigationEvidenceLinkSnapshot(
            maximum.Append(Link(hypothesis, "extra-evidence"))));
    }

    [Fact]
    public void ExposesOnlyTheReadOnlySnapshotSurface()
    {
        Type snapshotType = typeof(InvestigationEvidenceLinkSnapshot);
        System.Reflection.PropertyInfo property = Assert.Single(snapshotType.GetProperties());
        System.Reflection.ConstructorInfo constructor = Assert.Single(snapshotType.GetConstructors());

        Assert.Equal("Links", property.Name);
        Assert.Equal(typeof(IReadOnlyList<InvestigationEvidenceLink>), property.PropertyType);
        Assert.False(property.CanWrite);
        Assert.Collection(
            constructor.GetParameters(),
            parameter => Assert.Equal(
                typeof(IEnumerable<InvestigationEvidenceLink>),
                parameter.ParameterType));
        Assert.Empty(snapshotType.GetEvents());
        Assert.Empty(snapshotType.GetMethods().Where(
            method => method.DeclaringType == snapshotType && !method.IsSpecialName));
        Assert.True(snapshotType.IsSealed);
    }

    internal static InvestigationHypothesis Hypothesis(string hypothesisId) =>
        InvestigationHypothesisSnapshotTests.Hypothesis(
            hypothesisId,
            OpenInvestigationQuestionSnapshotTests.Question("question"));

    internal static InvestigationEvidenceLink Link(
        InvestigationHypothesis hypothesis,
        string evidenceId) =>
        new(hypothesis, new EvidenceId(evidenceId), InvestigationEvidenceRelationship.Supports);
}
