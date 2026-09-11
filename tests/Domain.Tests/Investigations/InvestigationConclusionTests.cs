using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Investigations;

namespace Allrpcs.Domain.Tests.Investigations;

public sealed class InvestigationConclusionTests
{
    [Fact]
    public void RetainsExactIdentityQuestionHypothesisAndAuthoredStatement()
    {
        InvestigationQuestion question = OpenInvestigationQuestionSnapshotTests.Question("question");
        InvestigationHypothesis hypothesis =
            InvestigationHypothesisSnapshotTests.Hypothesis("hypothesis", question);
        InvestigationConclusionId conclusionId = new("cooling-tonic-confirmed");
        const string Statement = "The cooled moonwort preparation stabilizes breathing.";

        InvestigationConclusion conclusion = new(
            conclusionId,
            question,
            hypothesis,
            Statement);

        Assert.Same(conclusionId, conclusion.ConclusionId);
        Assert.Same(question, conclusion.Question);
        Assert.Same(hypothesis, conclusion.Hypothesis);
        Assert.Equal(Statement, conclusion.Statement);
    }

    [Fact]
    public void RejectsMismatchedQuestionBlankNullAndOversizedValues()
    {
        InvestigationQuestion question = OpenInvestigationQuestionSnapshotTests.Question("question");
        InvestigationQuestion otherQuestion =
            OpenInvestigationQuestionSnapshotTests.Question("other-question");
        InvestigationHypothesis hypothesis =
            InvestigationHypothesisSnapshotTests.Hypothesis("hypothesis", question);
        InvestigationConclusionId conclusionId = new("conclusion");
        string excessive = new('c', InvestigationConclusion.MaximumStatementLength + 1);

        Assert.Throws<ArgumentException>(() =>
            new InvestigationConclusion(conclusionId, otherQuestion, hypothesis, "Conclusion"));
        Assert.Throws<ArgumentNullException>(() =>
            new InvestigationConclusion(null!, question, hypothesis, "Conclusion"));
        Assert.Throws<ArgumentNullException>(() =>
            new InvestigationConclusion(conclusionId, null!, hypothesis, "Conclusion"));
        Assert.Throws<ArgumentNullException>(() =>
            new InvestigationConclusion(conclusionId, question, null!, "Conclusion"));
        Assert.Throws<ArgumentNullException>(() =>
            new InvestigationConclusion(conclusionId, question, hypothesis, null!));
        Assert.Throws<ArgumentException>(() =>
            new InvestigationConclusion(conclusionId, question, hypothesis, " "));
        Assert.Throws<ArgumentException>(() =>
            new InvestigationConclusion(conclusionId, question, hypothesis, excessive));
    }

    [Fact]
    public void ExposesOnlyTheImmutableConclusionSurface()
    {
        Type conclusionType = typeof(InvestigationConclusion);
        System.Reflection.PropertyInfo[] properties = conclusionType.GetProperties();
        System.Reflection.ConstructorInfo constructor = Assert.Single(conclusionType.GetConstructors());

        Assert.Equal(
            ["ConclusionId", "Hypothesis", "Question", "Statement"],
            properties.Select(property => property.Name).Order());
        Assert.All(properties, property => Assert.False(property.CanWrite));
        Assert.Equal(
            [
                typeof(InvestigationConclusionId),
                typeof(InvestigationQuestion),
                typeof(InvestigationHypothesis),
                typeof(string),
            ],
            constructor.GetParameters().Select(parameter => parameter.ParameterType).ToArray());
        Assert.Empty(conclusionType.GetEvents());
        Assert.Empty(conclusionType.GetMethods().Where(
            method => method.DeclaringType == conclusionType && !method.IsSpecialName));
        Assert.True(conclusionType.IsSealed);
    }

    internal static InvestigationConclusion Conclusion()
    {
        InvestigationQuestion question = OpenInvestigationQuestionSnapshotTests.Question("question");
        InvestigationHypothesis hypothesis =
            InvestigationHypothesisSnapshotTests.Hypothesis("hypothesis", question);
        return new InvestigationConclusion(
            new InvestigationConclusionId("conclusion"),
            question,
            hypothesis,
            "The authored evidence confirms this conclusion.");
    }
}
