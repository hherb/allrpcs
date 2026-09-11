using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Investigations;

namespace Allrpcs.Domain.Tests.Investigations;

public sealed class InvestigationHypothesisRecognitionPolicyTests
{
    [Fact]
    public void CanonicalizesAProposedMatchForAnExactOpenQuestion()
    {
        InvestigationQuestion question = Question();
        InvestigationHypothesis hypothesis = Hypothesis(question);
        DialogueUtterance utterance = Utterance();

        InvestigationHypothesisRecognition? result =
            InvestigationHypothesisRecognitionPolicy.TryRecognize(
                utterance,
                new InvestigationQuestionId(question.QuestionId.Value),
                new InvestigationHypothesisId(hypothesis.HypothesisId.Value),
                new OpenInvestigationQuestionSnapshot([question]),
                new InvestigationHypothesisSnapshot([hypothesis]));

        InvestigationHypothesisRecognition recognition = Assert.IsType<
            InvestigationHypothesisRecognition>(result);
        Assert.Same(utterance, recognition.SourceUtterance);
        Assert.Same(hypothesis, recognition.Hypothesis);
        Assert.Same(question, recognition.Hypothesis.Question);
    }

    [Fact]
    public void RejectsUnknownClosedOrQuestionMismatchedProposals()
    {
        InvestigationQuestion question = Question();
        InvestigationHypothesis hypothesis = Hypothesis(question);
        InvestigationQuestion replacementQuestion = Question();
        InvestigationHypothesis replacementQuestionHypothesis = Hypothesis(replacementQuestion);
        DialogueUtterance utterance = Utterance();

        Assert.Null(InvestigationHypothesisRecognitionPolicy.TryRecognize(
            utterance,
            new InvestigationQuestionId("unknown-question"),
            hypothesis.HypothesisId,
            new OpenInvestigationQuestionSnapshot([question]),
            new InvestigationHypothesisSnapshot([hypothesis])));
        Assert.Null(InvestigationHypothesisRecognitionPolicy.TryRecognize(
            utterance,
            question.QuestionId,
            new InvestigationHypothesisId("unknown-hypothesis"),
            new OpenInvestigationQuestionSnapshot([question]),
            new InvestigationHypothesisSnapshot([hypothesis])));
        Assert.Null(InvestigationHypothesisRecognitionPolicy.TryRecognize(
            utterance,
            question.QuestionId,
            hypothesis.HypothesisId,
            new OpenInvestigationQuestionSnapshot([]),
            new InvestigationHypothesisSnapshot([hypothesis])));
        Assert.Null(InvestigationHypothesisRecognitionPolicy.TryRecognize(
            utterance,
            question.QuestionId,
            replacementQuestionHypothesis.HypothesisId,
            new OpenInvestigationQuestionSnapshot([question]),
            new InvestigationHypothesisSnapshot([replacementQuestionHypothesis])));
    }

    [Fact]
    public void LeavesAllInputsUnchangedAndNeverPromotesAConclusion()
    {
        InvestigationQuestion question = Question();
        InvestigationHypothesis hypothesis = Hypothesis(question);
        DialogueUtterance utterance = Utterance();
        OpenInvestigationQuestionSnapshot questions = new([question]);
        InvestigationHypothesisSnapshot hypotheses = new([hypothesis]);

        _ = InvestigationHypothesisRecognitionPolicy.TryRecognize(
            utterance,
            question.QuestionId,
            hypothesis.HypothesisId,
            questions,
            hypotheses);

        Assert.Same(question, Assert.Single(questions.Questions));
        Assert.Same(hypothesis, Assert.Single(hypotheses.Hypotheses));
        Assert.Equal("Could cooling the tonic preserve what helps?", utterance.Text);
    }

    [Fact]
    public void RejectsNullInputsAndExposesOnlyOnePureOperation()
    {
        InvestigationQuestion question = Question();
        InvestigationHypothesis hypothesis = Hypothesis(question);
        DialogueUtterance utterance = Utterance();
        OpenInvestigationQuestionSnapshot questions = new([question]);
        InvestigationHypothesisSnapshot hypotheses = new([hypothesis]);

        Assert.Throws<ArgumentNullException>(() =>
            InvestigationHypothesisRecognitionPolicy.TryRecognize(
                null!, question.QuestionId, hypothesis.HypothesisId, questions, hypotheses));
        Assert.Throws<ArgumentNullException>(() =>
            InvestigationHypothesisRecognitionPolicy.TryRecognize(
                utterance, null!, hypothesis.HypothesisId, questions, hypotheses));
        Assert.Throws<ArgumentNullException>(() =>
            InvestigationHypothesisRecognitionPolicy.TryRecognize(
                utterance, question.QuestionId, null!, questions, hypotheses));
        Assert.Throws<ArgumentNullException>(() =>
            InvestigationHypothesisRecognitionPolicy.TryRecognize(
                utterance, question.QuestionId, hypothesis.HypothesisId, null!, hypotheses));
        Assert.Throws<ArgumentNullException>(() =>
            InvestigationHypothesisRecognitionPolicy.TryRecognize(
                utterance, question.QuestionId, hypothesis.HypothesisId, questions, null!));

        Type policyType = typeof(InvestigationHypothesisRecognitionPolicy);
        System.Reflection.MethodInfo method = Assert.Single(policyType.GetMethods().Where(
            candidate => candidate.DeclaringType == policyType));

        Assert.Equal("TryRecognize", method.Name);
        Assert.Equal(typeof(InvestigationHypothesisRecognition), method.ReturnType);
        Assert.Equal(
            [
                typeof(DialogueUtterance),
                typeof(InvestigationQuestionId),
                typeof(InvestigationHypothesisId),
                typeof(OpenInvestigationQuestionSnapshot),
                typeof(InvestigationHypothesisSnapshot),
            ],
            method.GetParameters().Select(parameter => parameter.ParameterType).ToArray());
        Assert.Empty(policyType.GetConstructors());
        Assert.Empty(policyType.GetProperties());
        Assert.Empty(policyType.GetEvents());
        Assert.True(policyType.IsAbstract && policyType.IsSealed);
    }

    internal static DialogueUtterance Utterance() => new(
        new CharacterId("seeker"),
        "Could cooling the tonic preserve what helps?");

    internal static InvestigationQuestion Question() =>
        OpenInvestigationQuestionSnapshotTests.Question("what-stabilizes-grey-sleep");

    internal static InvestigationHypothesis Hypothesis() => Hypothesis(Question());

    internal static InvestigationHypothesis Hypothesis(InvestigationQuestion question) =>
        InvestigationHypothesisSnapshotTests.Hypothesis("cooling-preserves-remedy", question);
}
