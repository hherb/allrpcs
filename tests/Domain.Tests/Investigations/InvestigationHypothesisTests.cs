using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Investigations;

namespace Allrpcs.Domain.Tests.Investigations;

public sealed class InvestigationHypothesisTests
{
    [Theory]
    [InlineData("A cooled moonwort preparation may stabilize breathing.")]
    [InlineData("  Cooling, not steeping, may preserve the active property.  ")]
    public void RetainsExactIdentityQuestionAndAuthoredStatement(string statement)
    {
        InvestigationHypothesisId hypothesisId = new("cooling-tonic-stabilizes");
        InvestigationQuestion question = OpenInvestigationQuestionSnapshotTests.Question(
            "what-stabilizes-grey-sleep");

        InvestigationHypothesis hypothesis = new(hypothesisId, question, statement);

        Assert.Same(hypothesisId, hypothesis.HypothesisId);
        Assert.Same(question, hypothesis.Question);
        Assert.Equal(statement, hypothesis.Statement);
    }

    [Fact]
    public void AcceptsMaximumLengthAndRejectsBlankNullAndOversizedValues()
    {
        InvestigationHypothesisId hypothesisId = new("hypothesis");
        InvestigationQuestion question = OpenInvestigationQuestionSnapshotTests.Question("question");
        string maximum = new('h', InvestigationHypothesis.MaximumStatementLength);
        string excessive = new('h', InvestigationHypothesis.MaximumStatementLength + 1);

        Assert.Equal(maximum, new InvestigationHypothesis(hypothesisId, question, maximum).Statement);
        Assert.Throws<ArgumentNullException>(() =>
            new InvestigationHypothesis(null!, question, "Statement"));
        Assert.Throws<ArgumentNullException>(() =>
            new InvestigationHypothesis(hypothesisId, null!, "Statement"));
        Assert.Throws<ArgumentNullException>(() =>
            new InvestigationHypothesis(hypothesisId, question, null!));
        Assert.Throws<ArgumentException>(() =>
            new InvestigationHypothesis(hypothesisId, question, " "));
        Assert.Throws<ArgumentException>(() =>
            new InvestigationHypothesis(hypothesisId, question, excessive));
    }

    [Fact]
    public void ExposesOnlyTheImmutableHypothesisSurface()
    {
        Type hypothesisType = typeof(InvestigationHypothesis);
        System.Reflection.PropertyInfo[] properties = hypothesisType.GetProperties();
        System.Reflection.ConstructorInfo constructor = Assert.Single(hypothesisType.GetConstructors());

        Assert.Equal(
            ["HypothesisId", "Question", "Statement"],
            properties.Select(property => property.Name).Order());
        Assert.All(properties, property => Assert.False(property.CanWrite));
        Assert.Equal(
            [typeof(InvestigationHypothesisId), typeof(InvestigationQuestion), typeof(string)],
            constructor.GetParameters().Select(parameter => parameter.ParameterType).ToArray());
        Assert.Empty(hypothesisType.GetEvents());
        Assert.Empty(hypothesisType.GetMethods().Where(
            method => method.DeclaringType == hypothesisType && !method.IsSpecialName));
        Assert.True(hypothesisType.IsSealed);
    }
}
