using Allrpcs.Application.Journal;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Investigations;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Application.Tests.Journal;

public sealed class JournalHypothesisAuthoredCopyProjectionTests
{
    [Fact]
    public void CopiesAuthoredHypothesesInStableOrderWithoutSourceAggregateExposure()
    {
        InvestigationQuestionId questionId = new("what-causes-grey-sleep");
        string prompt = "  What causes the grey sleep?  ";
        InvestigationQuestion question = new(questionId, prompt);
        InvestigationHypothesisId firstId = new("stream-water");
        InvestigationHypothesisId secondId = new("laundry-contact");
        string firstStatement = "The stream water may carry the illness.";
        string secondStatement = "Shared laundry may spread the illness.";
        InvestigationHypothesis first = new(firstId, question, firstStatement);
        InvestigationHypothesis second = new(secondId, question, secondStatement);
        JournalHypothesisSectionBinding binding = Binding(first, second);

        IReadOnlyList<JournalHypothesisAuthoredCopy> copies =
            JournalHypothesisAuthoredCopyProjection.Project(binding);

        Assert.Collection(
            copies,
            copy =>
            {
                Assert.Same(firstId, copy.HypothesisId);
                Assert.Same(questionId, copy.QuestionId);
                Assert.Same(prompt, copy.QuestionPrompt);
                Assert.Same(firstStatement, copy.Statement);
            },
            copy =>
            {
                Assert.Same(secondId, copy.HypothesisId);
                Assert.Same(questionId, copy.QuestionId);
                Assert.Same(prompt, copy.QuestionPrompt);
                Assert.Same(secondStatement, copy.Statement);
            });
        Assert.Equal(
            ["HypothesisId", "QuestionId", "QuestionPrompt", "Statement"],
            typeof(JournalHypothesisAuthoredCopy)
                .GetProperties()
                .Select(property => property.Name)
                .Order());
        Assert.Collection(
            binding.Hypotheses.Hypotheses,
            hypothesis => Assert.Same(first, hypothesis),
            hypothesis => Assert.Same(second, hypothesis));
    }

    [Fact]
    public void ReturnsFreshCopiesInIndependentReadOnlyCollections()
    {
        InvestigationQuestion question = new(
            new InvestigationQuestionId("question"),
            "What happened?");
        InvestigationHypothesis hypothesis = new(
            new InvestigationHypothesisId("hypothesis"),
            question,
            "The weather may have changed.");
        JournalHypothesisSectionBinding binding = Binding(hypothesis);

        IReadOnlyList<JournalHypothesisAuthoredCopy> first =
            JournalHypothesisAuthoredCopyProjection.Project(binding);
        IReadOnlyList<JournalHypothesisAuthoredCopy> second =
            JournalHypothesisAuthoredCopyProjection.Project(binding);

        Assert.NotSame(first, second);
        Assert.NotSame(Assert.Single(first), Assert.Single(second));
        Assert.Throws<NotSupportedException>(() =>
            ((IList<JournalHypothesisAuthoredCopy>)first).Clear());
    }

    [Fact]
    public void AcceptsNoAuthoredHypotheses()
    {
        JournalHypothesisSectionBinding binding = Binding();

        IReadOnlyList<JournalHypothesisAuthoredCopy> copies =
            JournalHypothesisAuthoredCopyProjection.Project(binding);

        Assert.Empty(copies);
        Assert.Throws<NotSupportedException>(() =>
            ((IList<JournalHypothesisAuthoredCopy>)copies).Clear());
    }

    [Fact]
    public void RejectsNullBinding()
    {
        Assert.Throws<ArgumentNullException>(() =>
            JournalHypothesisAuthoredCopyProjection.Project(null!));
    }

    private static JournalHypothesisSectionBinding Binding(
        params InvestigationHypothesis[] hypotheses)
    {
        JournalSectionCatalogue catalogue = RepresentativeJournalSectionSource.Create();
        CharacterId seekerId = new("seeker");
        JournalObservationSectionBinding observations = JournalObservationSectionProjection.Bind(
            catalogue,
            seekerId,
            new CharacterPerceptionSnapshot(seekerId, []));
        JournalObservationWorldBinding world = JournalObservationWorldProjection.Bind(
            observations,
            new WorldObjectSnapshot([]));
        JournalSnapshot journal = JournalSnapshotProjection.Compose(
            catalogue,
            JournalObservationSectionSnapshotProjection.Project(world));
        JournalTestimonySectionSnapshot testimony =
            JournalTestimonySectionSnapshotProjection.Project(
                JournalTestimonySectionProjection.Bind(journal, []));
        JournalSnapshot withTestimony =
            JournalTestimonySnapshotProjection.Compose(journal, testimony);
        return JournalHypothesisSectionProjection.Bind(
            withTestimony,
            new InvestigationHypothesisSnapshot(hypotheses));
    }
}
