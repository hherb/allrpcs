using Allrpcs.Domain.Investigations;

namespace Allrpcs.Domain.Tests.Investigations;

public sealed class InvestigationConclusionConfirmationResultTests
{
    [Fact]
    public void RetainsExactOpenQuestionsAndConclusions()
    {
        OpenInvestigationQuestionSnapshot openQuestions = new([]);
        InvestigationConclusionSnapshot conclusions = new([]);

        InvestigationConclusionConfirmationResult result = new(openQuestions, conclusions);

        Assert.Same(openQuestions, result.OpenQuestions);
        Assert.Same(conclusions, result.Conclusions);
    }

    [Fact]
    public void RejectsNullsAndExposesOnlyImmutableProperties()
    {
        OpenInvestigationQuestionSnapshot openQuestions = new([]);
        InvestigationConclusionSnapshot conclusions = new([]);

        Assert.Throws<ArgumentNullException>(() =>
            new InvestigationConclusionConfirmationResult(null!, conclusions));
        Assert.Throws<ArgumentNullException>(() =>
            new InvestigationConclusionConfirmationResult(openQuestions, null!));

        Type resultType = typeof(InvestigationConclusionConfirmationResult);
        System.Reflection.PropertyInfo[] properties = resultType.GetProperties();
        System.Reflection.ConstructorInfo constructor = Assert.Single(resultType.GetConstructors());

        Assert.Equal(
            ["Conclusions", "OpenQuestions"],
            properties.Select(property => property.Name).Order());
        Assert.All(properties, property => Assert.False(property.CanWrite));
        Assert.Equal(
            [typeof(OpenInvestigationQuestionSnapshot), typeof(InvestigationConclusionSnapshot)],
            constructor.GetParameters().Select(parameter => parameter.ParameterType).ToArray());
        Assert.Empty(resultType.GetEvents());
        Assert.True(resultType.IsSealed);
    }
}
