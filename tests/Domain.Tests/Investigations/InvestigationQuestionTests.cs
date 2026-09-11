using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Investigations;

namespace Allrpcs.Domain.Tests.Investigations;

public sealed class InvestigationQuestionTests
{
    [Theory]
    [InlineData("What stabilizes the patient's breathing?")]
    [InlineData("  Does cooling preserve potency?  ")]
    [InlineData("どの調製法が患者を安定させますか？")]
    public void RetainsExactCanonicalIdentityAndAuthoredPrompt(string prompt)
    {
        InvestigationQuestionId questionId = new("what-stabilizes-grey-sleep");

        InvestigationQuestion question = new(questionId, prompt);

        Assert.Same(questionId, question.QuestionId);
        Assert.Equal(prompt, question.Prompt);
    }

    [Fact]
    public void AcceptsTheMaximumPromptLength()
    {
        string prompt = new('q', InvestigationQuestion.MaximumPromptLength);

        InvestigationQuestion question = new(
            new InvestigationQuestionId("maximum-question"),
            prompt);

        Assert.Equal(prompt, question.Prompt);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t\r\n")]
    public void RejectsBlankPrompt(string prompt)
    {
        Assert.Throws<ArgumentException>(() =>
            new InvestigationQuestion(new InvestigationQuestionId("question"), prompt));
    }

    [Fact]
    public void RejectsNullAndOversizedValues()
    {
        InvestigationQuestionId questionId = new("question");
        string oversized = new('q', InvestigationQuestion.MaximumPromptLength + 1);

        Assert.Throws<ArgumentNullException>(() => new InvestigationQuestion(null!, "Question?"));
        Assert.Throws<ArgumentNullException>(() => new InvestigationQuestion(questionId, null!));
        Assert.Throws<ArgumentException>(() => new InvestigationQuestion(questionId, oversized));
    }

    [Fact]
    public void ExposesOnlyTheImmutableQuestionSurface()
    {
        Type questionType = typeof(InvestigationQuestion);
        System.Reflection.PropertyInfo[] properties = questionType.GetProperties();
        System.Reflection.ConstructorInfo constructor = Assert.Single(questionType.GetConstructors());

        Assert.Equal(["Prompt", "QuestionId"], properties.Select(property => property.Name).Order());
        Assert.All(properties, property => Assert.False(property.CanWrite));
        Assert.Equal(
            [typeof(InvestigationQuestionId), typeof(string)],
            constructor.GetParameters().Select(parameter => parameter.ParameterType).ToArray());
        Assert.Empty(questionType.GetEvents());
        Assert.Empty(questionType.GetMethods().Where(
            method => method.DeclaringType == questionType && !method.IsSpecialName));
        Assert.True(questionType.IsSealed);
    }
}
