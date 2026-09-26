using Allrpcs.Application.Journal;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Investigations;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Application.Tests.Journal;

public sealed class JournalHypothesisSectionProjectionTests
{
    [Fact]
    public void BindsPostTestimonyJournalToExactCanonicalSectionAndHypotheses()
    {
        JournalSnapshot journal = JournalWithTestimony();
        InvestigationQuestion question = new(
            new InvestigationQuestionId("what-causes-grey-sleep"),
            "What causes the grey sleep?");
        InvestigationHypothesis first = new(
            new InvestigationHypothesisId("stream-water"),
            question,
            "The stream water may carry the illness.");
        InvestigationHypothesis second = new(
            new InvestigationHypothesisId("laundry-contact"),
            question,
            "Shared laundry may spread the illness.");
        InvestigationHypothesisSnapshot hypotheses = new([first, second]);

        JournalHypothesisSectionBinding binding =
            JournalHypothesisSectionProjection.Bind(journal, hypotheses);

        Assert.Same(journal, binding.Journal);
        Assert.Same(journal.Catalogue.Sections[2], binding.Section);
        Assert.Same(journal.SeekerId, binding.SeekerId);
        Assert.Same(hypotheses, binding.Hypotheses);
        Assert.Collection(
            binding.Hypotheses.Hypotheses,
            hypothesis => Assert.Same(first, hypothesis),
            hypothesis => Assert.Same(second, hypothesis));
    }

    [Fact]
    public void ReturnsFreshBindingsWithoutChangingEmptySourceState()
    {
        JournalSnapshot journal = JournalWithTestimony();
        InvestigationHypothesisSnapshot hypotheses = new([]);

        JournalHypothesisSectionBinding first =
            JournalHypothesisSectionProjection.Bind(journal, hypotheses);
        JournalHypothesisSectionBinding second =
            JournalHypothesisSectionProjection.Bind(journal, hypotheses);

        Assert.NotSame(first, second);
        Assert.Same(journal, first.Journal);
        Assert.Same(journal, second.Journal);
        Assert.Same(hypotheses, first.Hypotheses);
        Assert.Same(hypotheses, second.Hypotheses);
        Assert.Empty(hypotheses.Hypotheses);
    }

    [Fact]
    public void RejectsJournalBeforeTestimonyInstallationWithoutChangingIt()
    {
        JournalSectionCatalogue catalogue = RepresentativeJournalSectionSource.Create();
        JournalSnapshot journal = Journal(catalogue, new CharacterId("seeker"));
        InvestigationHypothesisSnapshot hypotheses = new([]);

        Assert.Throws<ArgumentException>(() =>
            JournalHypothesisSectionProjection.Bind(journal, hypotheses));
        Assert.Null(journal.Testimony);
        Assert.Same(catalogue.Sections[1], journal.PendingSections[0]);
        Assert.Empty(hypotheses.Hypotheses);
    }

    [Fact]
    public void RejectsNullInputs()
    {
        JournalSnapshot journal = JournalWithTestimony();
        InvestigationHypothesisSnapshot hypotheses = new([]);

        Assert.Throws<ArgumentNullException>(() =>
            JournalHypothesisSectionProjection.Bind(null!, hypotheses));
        Assert.Throws<ArgumentNullException>(() =>
            JournalHypothesisSectionProjection.Bind(journal, null!));
    }

    private static JournalSnapshot JournalWithTestimony()
    {
        JournalSectionCatalogue catalogue = RepresentativeJournalSectionSource.Create();
        JournalSnapshot journal = Journal(catalogue, new CharacterId("seeker"));
        JournalTestimonySectionBinding binding = JournalTestimonySectionProjection.Bind(journal, []);
        JournalTestimonySectionSnapshot testimony =
            JournalTestimonySectionSnapshotProjection.Project(binding);
        return JournalTestimonySnapshotProjection.Compose(journal, testimony);
    }

    private static JournalSnapshot Journal(
        JournalSectionCatalogue catalogue,
        CharacterId seekerId)
    {
        JournalObservationSectionBinding observations = JournalObservationSectionProjection.Bind(
            catalogue,
            seekerId,
            new CharacterPerceptionSnapshot(seekerId, []));
        JournalObservationWorldBinding world = JournalObservationWorldProjection.Bind(
            observations,
            new WorldObjectSnapshot([]));
        return JournalSnapshotProjection.Compose(
            catalogue,
            JournalObservationSectionSnapshotProjection.Project(world));
    }
}
