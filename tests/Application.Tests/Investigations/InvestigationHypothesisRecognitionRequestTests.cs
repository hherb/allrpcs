using Allrpcs.Application.Investigations;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Investigations;

namespace Allrpcs.Application.Tests.Investigations;

public sealed class InvestigationHypothesisRecognitionRequestTests
{
    [Fact]
    public void RetainsTheExactUtteranceAndAuthoredCandidateState()
    {
        DialogueUtterance utterance = Utterance();
        InvestigationQuestion question = Question();
        InvestigationHypothesis hypothesis = Hypothesis(question);
        OpenInvestigationQuestionSnapshot questions = new([question]);
        InvestigationHypothesisSnapshot hypotheses = new([hypothesis]);

        InvestigationHypothesisRecognitionRequest request = new(
            utterance,
            questions,
            hypotheses);

        Assert.Same(utterance, request.SourceUtterance);
        Assert.Same(questions, request.OpenQuestions);
        Assert.Same(hypotheses, request.Hypotheses);
    }

    [Fact]
    public void RejectsNullsAndExposesOnlyTheImmutableRequestSurface()
    {
        DialogueUtterance utterance = Utterance();
        OpenInvestigationQuestionSnapshot questions = new([]);
        InvestigationHypothesisSnapshot hypotheses = new([]);

        Assert.Throws<ArgumentNullException>(() =>
            new InvestigationHypothesisRecognitionRequest(null!, questions, hypotheses));
        Assert.Throws<ArgumentNullException>(() =>
            new InvestigationHypothesisRecognitionRequest(utterance, null!, hypotheses));
        Assert.Throws<ArgumentNullException>(() =>
            new InvestigationHypothesisRecognitionRequest(utterance, questions, null!));

        Type requestType = typeof(InvestigationHypothesisRecognitionRequest);
        System.Reflection.PropertyInfo[] properties = requestType.GetProperties();
        System.Reflection.ConstructorInfo constructor = Assert.Single(requestType.GetConstructors());

        Assert.Equal(
            ["Hypotheses", "OpenQuestions", "SourceUtterance"],
            properties.Select(property => property.Name).Order());
        Assert.All(properties, property => Assert.False(property.CanWrite));
        Assert.Equal(
            [
                typeof(DialogueUtterance),
                typeof(OpenInvestigationQuestionSnapshot),
                typeof(InvestigationHypothesisSnapshot),
            ],
            constructor.GetParameters().Select(parameter => parameter.ParameterType).ToArray());
        Assert.Empty(requestType.GetEvents());
        Assert.Empty(requestType.GetMethods().Where(
            method => method.DeclaringType == requestType && !method.IsSpecialName));
        Assert.True(requestType.IsSealed);
    }

    internal static DialogueUtterance Utterance() => new(
        new CharacterId("seeker"),
        "Could cooling the tonic preserve what helps?");

    internal static InvestigationQuestion Question() => new(
        new InvestigationQuestionId("what-stabilizes-grey-sleep"),
        "What preparation can stabilize the sleeping illness?");

    internal static InvestigationHypothesis Hypothesis(InvestigationQuestion question) => new(
        new InvestigationHypothesisId("cooling-preserves-remedy"),
        question,
        "Cooling the preparation may preserve what stabilizes the patient.");
}
