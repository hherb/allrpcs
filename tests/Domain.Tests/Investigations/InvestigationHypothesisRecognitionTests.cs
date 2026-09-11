using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Investigations;

namespace Allrpcs.Domain.Tests.Investigations;

public sealed class InvestigationHypothesisRecognitionTests
{
    [Fact]
    public void RetainsTheExactNaturalLanguageUtteranceAndAuthoredHypothesis()
    {
        DialogueUtterance utterance = InvestigationHypothesisRecognitionPolicyTests.Utterance();
        InvestigationHypothesis hypothesis =
            InvestigationHypothesisRecognitionPolicyTests.Hypothesis();

        InvestigationHypothesisRecognition recognition = new(utterance, hypothesis);

        Assert.Same(utterance, recognition.SourceUtterance);
        Assert.Same(hypothesis, recognition.Hypothesis);
    }

    [Fact]
    public void RejectsNullsAndExposesOnlyTheImmutableRecognitionSurface()
    {
        DialogueUtterance utterance = InvestigationHypothesisRecognitionPolicyTests.Utterance();
        InvestigationHypothesis hypothesis =
            InvestigationHypothesisRecognitionPolicyTests.Hypothesis();

        Assert.Throws<ArgumentNullException>(() =>
            new InvestigationHypothesisRecognition(null!, hypothesis));
        Assert.Throws<ArgumentNullException>(() =>
            new InvestigationHypothesisRecognition(utterance, null!));

        Type recognitionType = typeof(InvestigationHypothesisRecognition);
        System.Reflection.PropertyInfo[] properties = recognitionType.GetProperties();
        System.Reflection.ConstructorInfo constructor = Assert.Single(
            recognitionType.GetConstructors());

        Assert.Equal(
            ["Hypothesis", "SourceUtterance"],
            properties.Select(property => property.Name).Order());
        Assert.All(properties, property => Assert.False(property.CanWrite));
        Assert.Equal(
            [typeof(DialogueUtterance), typeof(InvestigationHypothesis)],
            constructor.GetParameters().Select(parameter => parameter.ParameterType).ToArray());
        Assert.Empty(recognitionType.GetEvents());
        Assert.Empty(recognitionType.GetMethods().Where(
            method => method.DeclaringType == recognitionType && !method.IsSpecialName));
        Assert.True(recognitionType.IsSealed);
    }
}
