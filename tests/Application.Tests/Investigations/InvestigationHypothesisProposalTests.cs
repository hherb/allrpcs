using Allrpcs.Application.Investigations;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Application.Tests.Investigations;

public sealed class InvestigationHypothesisProposalTests
{
    [Fact]
    public void RetainsTheExactProposedIdentifiers()
    {
        InvestigationQuestionId questionId = new("what-stabilizes-grey-sleep");
        InvestigationHypothesisId hypothesisId = new("cooling-preserves-remedy");

        InvestigationHypothesisProposal proposal = new(questionId, hypothesisId);

        Assert.Same(questionId, proposal.QuestionId);
        Assert.Same(hypothesisId, proposal.HypothesisId);
    }

    [Fact]
    public void RejectsNullsAndExposesOnlyTheImmutableProposalSurface()
    {
        InvestigationQuestionId questionId = new("what-stabilizes-grey-sleep");
        InvestigationHypothesisId hypothesisId = new("cooling-preserves-remedy");

        Assert.Throws<ArgumentNullException>(() => new InvestigationHypothesisProposal(null!, hypothesisId));
        Assert.Throws<ArgumentNullException>(() => new InvestigationHypothesisProposal(questionId, null!));

        Type proposalType = typeof(InvestigationHypothesisProposal);
        System.Reflection.PropertyInfo[] properties = proposalType.GetProperties();
        System.Reflection.ConstructorInfo constructor = Assert.Single(proposalType.GetConstructors());

        Assert.Equal(["HypothesisId", "QuestionId"], properties.Select(property => property.Name).Order());
        Assert.All(properties, property => Assert.False(property.CanWrite));
        Assert.Equal(
            [typeof(InvestigationQuestionId), typeof(InvestigationHypothesisId)],
            constructor.GetParameters().Select(parameter => parameter.ParameterType).ToArray());
        Assert.Empty(proposalType.GetEvents());
        Assert.Empty(proposalType.GetMethods().Where(
            method => method.DeclaringType == proposalType && !method.IsSpecialName));
        Assert.True(proposalType.IsSealed);
    }
}
