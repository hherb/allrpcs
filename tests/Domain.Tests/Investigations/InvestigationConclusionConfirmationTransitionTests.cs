using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Investigations;

namespace Allrpcs.Domain.Tests.Investigations;

public sealed class InvestigationConclusionConfirmationTransitionTests
{
    [Fact]
    public void RemovesExactOpenQuestionAndAppendsExactConclusionAtomically()
    {
        ConfirmationInput input = Input();
        InvestigationQuestion unrelated = OpenInvestigationQuestionSnapshotTests.Question("unrelated");
        OpenInvestigationQuestionSnapshot openQuestions = new(
            [unrelated, input.Plan.Conclusion.Question]);
        InvestigationConclusion existing = InvestigationConclusionSnapshotTests.Conclusion("existing");
        InvestigationConclusionSnapshot conclusions = new([existing]);

        InvestigationConclusionConfirmationResult result =
            InvestigationConclusionConfirmationTransition.Confirm(
                input.Plan,
                input.Evidence,
                openQuestions,
                conclusions);

        Assert.Same(unrelated, Assert.Single(result.OpenQuestions.Questions));
        Assert.Collection(
            result.Conclusions.Conclusions,
            conclusion => Assert.Same(existing, conclusion),
            conclusion => Assert.Same(input.Plan.Conclusion, conclusion));
        Assert.Collection(
            openQuestions.Questions,
            question => Assert.Same(unrelated, question),
            question => Assert.Same(input.Plan.Conclusion.Question, question));
        Assert.Same(existing, Assert.Single(conclusions.Conclusions));
    }

    [Fact]
    public void RejectsIncompleteEvidenceAndValueEqualReplacementQuestion()
    {
        ConfirmationInput input = Input();
        OpenInvestigationQuestionSnapshot exactQuestions = new([input.Plan.Conclusion.Question]);
        InvestigationQuestion replacement = new(
            new InvestigationQuestionId(input.Plan.Conclusion.Question.QuestionId.Value),
            input.Plan.Conclusion.Question.Prompt);
        OpenInvestigationQuestionSnapshot replacementQuestions = new([replacement]);
        CharacterEvidenceViewSnapshot incomplete = new(
            input.Evidence.CharacterId,
            [input.Evidence.EvidenceIds[0]]);
        InvestigationConclusionSnapshot conclusions = new([]);

        Assert.Throws<InvalidOperationException>(() =>
            InvestigationConclusionConfirmationTransition.Confirm(
                input.Plan,
                incomplete,
                exactQuestions,
                conclusions));
        Assert.Throws<InvalidOperationException>(() =>
            InvestigationConclusionConfirmationTransition.Confirm(
                input.Plan,
                input.Evidence,
                replacementQuestions,
                conclusions));
        Assert.Same(input.Plan.Conclusion.Question, Assert.Single(exactQuestions.Questions));
        Assert.Same(replacement, Assert.Single(replacementQuestions.Questions));
        Assert.Empty(conclusions.Conclusions);
    }

    [Fact]
    public void RejectsDuplicateConclusionCapacityAndNullInputs()
    {
        ConfirmationInput input = Input();
        OpenInvestigationQuestionSnapshot openQuestions = new([input.Plan.Conclusion.Question]);
        InvestigationConclusionSnapshot duplicate = new([input.Plan.Conclusion]);
        InvestigationConclusionSnapshot full = new(
            Enumerable
                .Range(0, InvestigationConclusionSnapshot.MaximumConclusionCount)
                .Select(index => InvestigationConclusionSnapshotTests.Conclusion($"full-{index}")));
        InvestigationConclusionSnapshot empty = new([]);

        Assert.Throws<ArgumentException>(() =>
            InvestigationConclusionConfirmationTransition.Confirm(
                input.Plan,
                input.Evidence,
                openQuestions,
                duplicate));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            InvestigationConclusionConfirmationTransition.Confirm(
                input.Plan,
                input.Evidence,
                openQuestions,
                full));
        Assert.Throws<ArgumentNullException>(() =>
            InvestigationConclusionConfirmationTransition.Confirm(
                null!,
                input.Evidence,
                openQuestions,
                empty));
        Assert.Throws<ArgumentNullException>(() =>
            InvestigationConclusionConfirmationTransition.Confirm(
                input.Plan,
                null!,
                openQuestions,
                empty));
        Assert.Throws<ArgumentNullException>(() =>
            InvestigationConclusionConfirmationTransition.Confirm(
                input.Plan,
                input.Evidence,
                null!,
                empty));
        Assert.Throws<ArgumentNullException>(() =>
            InvestigationConclusionConfirmationTransition.Confirm(
                input.Plan,
                input.Evidence,
                openQuestions,
                null!));
    }

    [Fact]
    public void ExposesOnePureStaticOperation()
    {
        Type transitionType = typeof(InvestigationConclusionConfirmationTransition);
        System.Reflection.MethodInfo method = Assert.Single(transitionType.GetMethods().Where(
            candidate => candidate.DeclaringType == transitionType));

        Assert.Equal("Confirm", method.Name);
        Assert.True(method.IsStatic);
        Assert.Equal(typeof(InvestigationConclusionConfirmationResult), method.ReturnType);
        Assert.Equal(
            [
                typeof(InvestigationConclusionEvidencePlan),
                typeof(CharacterEvidenceViewSnapshot),
                typeof(OpenInvestigationQuestionSnapshot),
                typeof(InvestigationConclusionSnapshot),
            ],
            method.GetParameters().Select(parameter => parameter.ParameterType).ToArray());
        Assert.Empty(transitionType.GetProperties());
        Assert.Empty(transitionType.GetEvents());
        Assert.True(transitionType.IsAbstract && transitionType.IsSealed);
    }

    private static ConfirmationInput Input()
    {
        InvestigationConclusion conclusion = InvestigationConclusionTests.Conclusion();
        InvestigationConclusionEvidenceRoute route =
            InvestigationConclusionEvidencePlanTests.Route(
                conclusion,
                "first-evidence",
                "second-evidence");
        InvestigationConclusionEvidencePlan plan = new(conclusion, [route]);
        CharacterEvidenceViewSnapshot evidence = new(
            new CharacterId("healer"),
            [new EvidenceId("first-evidence"), new EvidenceId("second-evidence")]);
        return new ConfirmationInput(plan, evidence);
    }

    private sealed record ConfirmationInput(
        InvestigationConclusionEvidencePlan Plan,
        CharacterEvidenceViewSnapshot Evidence);
}
