using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Investigations;

namespace Allrpcs.Domain.Tests.Investigations;

public sealed class InvestigationEvidenceLinkTests
{
    [Theory]
    [MemberData(nameof(Relationships))]
    public void RetainsExactHypothesisEvidenceAndRelationship(
        InvestigationEvidenceRelationship relationship)
    {
        InvestigationQuestion question = OpenInvestigationQuestionSnapshotTests.Question("question");
        InvestigationHypothesis hypothesis =
            InvestigationHypothesisSnapshotTests.Hypothesis("hypothesis", question);
        EvidenceId evidenceId = new("cooling-observation");

        InvestigationEvidenceLink link = new(hypothesis, evidenceId, relationship);

        Assert.Same(hypothesis, link.Hypothesis);
        Assert.Same(evidenceId, link.EvidenceId);
        Assert.Same(relationship, link.Relationship);
    }

    [Fact]
    public void RejectsNullsAndExposesOnlyTheImmutableLinkSurface()
    {
        InvestigationQuestion question = OpenInvestigationQuestionSnapshotTests.Question("question");
        InvestigationHypothesis hypothesis =
            InvestigationHypothesisSnapshotTests.Hypothesis("hypothesis", question);
        EvidenceId evidenceId = new("cooling-observation");

        Assert.Throws<ArgumentNullException>(() =>
            new InvestigationEvidenceLink(
                null!,
                evidenceId,
                InvestigationEvidenceRelationship.Supports));
        Assert.Throws<ArgumentNullException>(() =>
            new InvestigationEvidenceLink(
                hypothesis,
                null!,
                InvestigationEvidenceRelationship.Supports));
        Assert.Throws<ArgumentNullException>(() =>
            new InvestigationEvidenceLink(hypothesis, evidenceId, null!));

        Type linkType = typeof(InvestigationEvidenceLink);
        System.Reflection.PropertyInfo[] properties = linkType.GetProperties();
        System.Reflection.ConstructorInfo constructor = Assert.Single(linkType.GetConstructors());

        Assert.Equal(
            ["EvidenceId", "Hypothesis", "Relationship"],
            properties.Select(property => property.Name).Order());
        Assert.All(properties, property => Assert.False(property.CanWrite));
        Assert.Equal(
            [
                typeof(InvestigationHypothesis),
                typeof(EvidenceId),
                typeof(InvestigationEvidenceRelationship),
            ],
            constructor.GetParameters().Select(parameter => parameter.ParameterType).ToArray());
        Assert.Empty(linkType.GetEvents());
        Assert.Empty(linkType.GetMethods().Where(
            method => method.DeclaringType == linkType && !method.IsSpecialName));
        Assert.True(linkType.IsSealed);
    }

    public static TheoryData<InvestigationEvidenceRelationship> Relationships => new()
    {
        InvestigationEvidenceRelationship.Supports,
        InvestigationEvidenceRelationship.Contradicts,
    };
}
