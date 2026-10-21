using Allrpcs.Application.Investigations;
using Allrpcs.Application.Journal;
using Allrpcs.Application.Tests.Investigations;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.Experiments;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Investigations;
using Allrpcs.Domain.Persistence;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Application.Tests.Journal;

public sealed class JournalInvestigationTransitionTests
{
    private static readonly CharacterId Seeker = new("seeker");

    [Fact]
    public void ProposesOnlyOpenHypothesesAndAddsEachOnceUntested()
    {
        (_, IReadOnlyList<FirstPatientHypothesisTest> tests) = Authored();
        JournalSnapshot journal = EmptyJournal();

        InvestigationHypothesisRecognitionRequest request =
            JournalHypothesisProposalFlow.CreateRequest(journal, tests, "  the dried leaves will wake her  ")!;
        Assert.Equal("the dried leaves will wake her", request.SourceUtterance.Text);
        Assert.Same(Seeker, request.SourceUtterance.SpeakerCharacterId);
        Assert.Same(tests[0].Hypothesis.Question, Assert.Single(request.OpenQuestions.Questions));
        Assert.Equal(tests.Select(test => test.Hypothesis), request.Hypotheses.Hypotheses);
        Assert.Null(JournalHypothesisProposalFlow.CreateRequest(journal, tests, "   "));
        Assert.Null(JournalHypothesisProposalFlow.CreateRequest(
            journal, tests, new string('x', DialogueUtterance.MaximumTextLength + 1)));

        InvestigationHypothesisRecognition leaves = Recognize(request, tests[1]);
        (JournalHypothesisProposalResult added, JournalSnapshot withLeaves) = JournalHypothesisProposalFlow.Apply(journal, leaves);
        (JournalHypothesisProposalResult again, JournalSnapshot unchanged) = JournalHypothesisProposalFlow.Apply(withLeaves, leaves);
        (JournalHypothesisProposalResult none, JournalSnapshot same) = JournalHypothesisProposalFlow.Apply(withLeaves, null);

        Assert.Equal(JournalHypothesisProposalResult.Added, added);
        JournalHypothesisAuthoredCopy entry = Assert.Single(withLeaves.Hypotheses!.Entries);
        Assert.Same(tests[1].Hypothesis.HypothesisId, entry.HypothesisId);
        Assert.Equal(tests[1].Hypothesis.Statement, entry.Statement);
        Assert.Equal(InvestigationHypothesisTestOutcome.Untested, entry.Outcome);
        Assert.Same(journal.OpenQuestions, withLeaves.OpenQuestions);
        Assert.Same(journal.Catalogue, withLeaves.Catalogue);
        Assert.Equal(JournalHypothesisProposalResult.AlreadyNoted, again);
        Assert.Same(withLeaves, unchanged);
        Assert.Equal(JournalHypothesisProposalResult.NoMatch, none);
        Assert.Same(withLeaves, same);
    }

    [Fact]
    public void RefreshShowsExperimentsRefutesTheLookAlikeAndConfirmingClosesTheQuestion()
    {
        (FirstPatientScenario scenario, IReadOnlyList<FirstPatientHypothesisTest> tests) = Authored();
        JournalSnapshot journal = EmptyJournal();
        journal = JournalInvestigationTransition.Refresh(journal, tests, scenario, new([]), new([]));
        Assert.Equal("what-stabilizes-grey-sleep", Assert.Single(journal.OpenQuestions!.Entries).QuestionId.Value);

        foreach (FirstPatientHypothesisTest test in tests)
        {
            InvestigationHypothesisRecognitionRequest request =
                JournalHypothesisProposalFlow.CreateRequest(journal, tests, "a guess")!;
            journal = JournalHypothesisProposalFlow.Apply(journal, Recognize(request, test)).Journal;
        }

        ExperimentJournalSnapshot oneRecord = new(
        [
            new ExperimentJournalRecord(
                new ExperimentId("dose-0"),
                new WorldEvent(new WorldEventId("dose-event-0"), WorldEventTimeEffect.Advance),
                "Administer the look-alike.",
                "The patient's breathing does not change."),
        ]);
        JournalSnapshot refuted = JournalInvestigationTransition.Refresh(
            journal, tests, scenario,
            FirstPatientHypothesisTestPolicyTests.Doses(scenario, scenario.AlternativeRecipes[0]),
            oneRecord);

        Assert.Equal(
            [InvestigationHypothesisTestOutcome.Untested, InvestigationHypothesisTestOutcome.Refuted],
            refuted.Hypotheses!.Entries.Select(entry => entry.Outcome));
        Assert.Equal("Administer the look-alike.", Assert.Single(refuted.Experiments!.Entries).Procedure);
        Assert.Single(refuted.OpenQuestions!.Entries);
        Assert.Same(refuted, JournalInvestigationTransition.Refresh(
            refuted, tests, scenario,
            FirstPatientHypothesisTestPolicyTests.Doses(scenario, scenario.AlternativeRecipes[0]),
            oneRecord));
        Assert.Contains("Refuted by experiment.", JournalSectionPresentationTextProjection.Project(
            JournalSectionPresentationProjection.Project(refuted, refuted.Catalogue.Sections[2])).Content,
            StringComparison.Ordinal);

        JournalSnapshot confirmed = JournalInvestigationTransition.Refresh(
            refuted, tests, scenario,
            FirstPatientHypothesisTestPolicyTests.Doses(scenario, scenario.AlternativeRecipes[0], scenario.Remedy.Recipe),
            oneRecord);

        Assert.Equal(InvestigationHypothesisTestOutcome.Confirmed, confirmed.Hypotheses!.Entries[0].Outcome);
        Assert.Empty(confirmed.OpenQuestions!.Entries);
        Assert.Empty(JournalInvestigationTransition.OpenQuestions(confirmed, tests));
        Assert.Null(JournalHypothesisProposalFlow.CreateRequest(confirmed, tests, "another idea"));
    }

    [Fact]
    public void RefreshLeavesAnUnknownSavedHypothesisUntestedAndRejectsAnIncompleteJournal()
    {
        (FirstPatientScenario scenario, IReadOnlyList<FirstPatientHypothesisTest> tests) = Authored();
        JournalSnapshot journal = JournalSaveStateRestorationProjection.Restore(new JournalSaveState(
            Seeker, [], [],
            [new JournalHypothesisSaveEntry(new InvestigationHypothesisId("retired-idea"), new InvestigationQuestionId("what-stabilizes-grey-sleep"), tests[0].Hypothesis.Question.Prompt, "An idea.")],
            [], [], null)).Journal;

        JournalSnapshot refreshed = JournalInvestigationTransition.Refresh(
            journal, tests, scenario, FirstPatientHypothesisTestPolicyTests.Doses(scenario, scenario.Remedy.Recipe), new([]));

        Assert.Equal(InvestigationHypothesisTestOutcome.Untested, Assert.Single(refreshed.Hypotheses!.Entries).Outcome);
        Assert.Single(refreshed.OpenQuestions!.Entries);
        JournalSnapshot pending = JournalSnapshotProjection.Compose(journal.Catalogue, journal.Observations);
        Assert.Throws<ArgumentException>(() => JournalInvestigationTransition.Refresh(pending, tests, scenario, new([]), new([])));
    }

    private static (FirstPatientScenario Scenario, IReadOnlyList<FirstPatientHypothesisTest> Tests) Authored()
    {
        FirstPatientScenario scenario = RepresentativeFirstPatientScenarioSource.Create();
        return (scenario, RepresentativeFirstPatientHypothesisSource.Create(scenario));
    }

    private static JournalSnapshot EmptyJournal() => JournalSaveStateRestorationProjection.Restore(
        new JournalSaveState(Seeker, [], [], [], [], [], null)).Journal;

    private static InvestigationHypothesisRecognition Recognize(
        InvestigationHypothesisRecognitionRequest request,
        FirstPatientHypothesisTest test) => InvestigationHypothesisRecognitionPolicy.TryRecognize(
            request.SourceUtterance,
            test.Hypothesis.Question.QuestionId,
            test.Hypothesis.HypothesisId,
            request.OpenQuestions,
            request.Hypotheses)!;
}
