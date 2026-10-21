using Allrpcs.Application.Journal;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Experiments;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Investigations;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldEvents;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Application.Tests.Journal;

public sealed class JournalSectionPresentationTextProjectionTests
{
    [Theory]
    [InlineData(0, "Observations", "Old Shrine\nLayered marks cross the weathered stone.")]
    [InlineData(1, "Testimony", "healer · whispering\nThe stream changed after the rain.")]
    [InlineData(2, "Hypotheses", "What causes the grey sleep?\nThe stream water may carry it.\nUntested: try it on the patient.")]
    [InlineData(
        3,
        "Experiments",
        "Procedure: Compare covered water.\nObserved outcome: The covered sample remained clear.")]
    [InlineData(4, "Open Questions", "What slows the grey sleep?")]
    public void ProjectsStableSafeTextForEveryContentArm(
        int sectionIndex,
        string expectedLabel,
        string expectedContent)
    {
        JournalSnapshot journal = CompleteJournal(withContent: true);
        JournalSectionPresentationSnapshot presentation =
            JournalSectionPresentationProjection.Project(
                journal,
                journal.Catalogue.Sections[sectionIndex]);

        JournalSectionPresentationText result =
            JournalSectionPresentationTextProjection.Project(presentation);

        Assert.Equal(expectedLabel, result.SectionLabel);
        Assert.Equal(expectedContent, result.Content);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void ProjectsOneNeutralEmptyStateForEveryEmptyContentArm(int sectionIndex)
    {
        JournalSnapshot journal = CompleteJournal(withContent: false);
        JournalSectionPresentationSnapshot presentation =
            JournalSectionPresentationProjection.Project(
                journal,
                journal.Catalogue.Sections[sectionIndex]);

        JournalSectionPresentationText result =
            JournalSectionPresentationTextProjection.Project(presentation);

        Assert.Equal(JournalSectionPresentationTextProjection.EmptyContent, result.Content);
    }

    [Fact]
    public void RejectsNullAndExposesOnlyPlainTextSurface()
    {
        JournalSnapshot journal = CompleteJournal(withContent: true);
        JournalSectionPresentationSnapshot observations =
            JournalSectionPresentationProjection.Project(
                journal,
                journal.Catalogue.Sections[0]);

        JournalSectionPresentationText result =
            JournalSectionPresentationTextProjection.Project(observations);

        Assert.StartsWith("Old Shrine\n", result.Content, StringComparison.Ordinal);
        Assert.Throws<ArgumentNullException>(
            () => JournalSectionPresentationTextProjection.Project(null!));
        Assert.Equal(
            ["Content", "SectionLabel"],
            typeof(JournalSectionPresentationText)
                .GetProperties()
                .Select(property => property.Name)
                .Order(StringComparer.Ordinal));
    }

    private static JournalSnapshot CompleteJournal(bool withContent)
    {
        JournalSectionCatalogue catalogue = RepresentativeJournalSectionSource.Create();
        CharacterId seekerId = new("seeker");
        WorldObject shrine = new(
            new WorldObjectIdentity(new ObjectId("old-shrine"), "Old Shrine"),
            new WorldObjectDescription("Layered marks cross the weathered stone."));
        CharacterPerceptionSnapshot perception = new(
            seekerId,
            withContent
                ? [new WorldObjectVisibility(shrine.Identity.Id, VisibilityState.Identified)]
                : []);
        JournalObservationSectionBinding observationBinding =
            JournalObservationSectionProjection.Bind(catalogue, seekerId, perception);
        JournalObservationWorldBinding observationWorld = JournalObservationWorldProjection.Bind(
            observationBinding,
            new WorldObjectSnapshot(withContent ? [shrine] : []));
        JournalSnapshot journal = JournalSnapshotProjection.Compose(
            catalogue,
            JournalObservationSectionSnapshotProjection.Project(observationWorld));

        CharacterId healerId = new("healer");
        DialogueEvent testimonyEvent = new(
            new DialogueDelivery(
                new DialogueUtterance(healerId, "The stream changed after the rain."),
                DialogueDeliveryMode.Whispering),
            new DialogueAudienceSnapshot(healerId, [seekerId]));
        JournalTestimonySectionSnapshot testimony =
            JournalTestimonySectionSnapshotProjection.Project(
                JournalTestimonySectionProjection.Bind(
                    journal,
                    withContent ? [testimonyEvent] : []));
        journal = JournalTestimonySnapshotProjection.Compose(journal, testimony);

        InvestigationQuestion hypothesisQuestion = new(
            new InvestigationQuestionId("grey-sleep-cause"),
            "What causes the grey sleep?");
        InvestigationHypothesis hypothesis = new(
            new InvestigationHypothesisId("stream-water"),
            hypothesisQuestion,
            "The stream water may carry it.");
        JournalHypothesisSectionSnapshot hypotheses =
            JournalHypothesisSectionSnapshotProjection.Project(
                JournalHypothesisSectionProjection.Bind(
                    journal,
                    new InvestigationHypothesisSnapshot(withContent ? [hypothesis] : [])));
        journal = JournalHypothesisSnapshotProjection.Compose(journal, hypotheses);

        ExperimentJournalRecord experiment = new(
            new ExperimentId("covered-water"),
            new WorldEvent(
                new WorldEventId("covered-water-occurred"),
                WorldEventTimeEffect.Advance),
            "Compare covered water.",
            "The covered sample remained clear.");
        JournalExperimentSectionSnapshot experiments =
            JournalExperimentSectionSnapshotProjection.Project(
                JournalExperimentSectionProjection.Bind(
                    journal,
                    new ExperimentJournalSnapshot(withContent ? [experiment] : [])));
        journal = JournalExperimentSnapshotProjection.Compose(journal, experiments);

        InvestigationQuestion openQuestion = new(
            new InvestigationQuestionId("grey-sleep-slowing"),
            "What slows the grey sleep?");
        JournalOpenQuestionSectionSnapshot openQuestions =
            JournalOpenQuestionSectionSnapshotProjection.Project(
                JournalOpenQuestionSectionProjection.Bind(
                    journal,
                    new OpenInvestigationQuestionSnapshot(withContent ? [openQuestion] : [])));
        return JournalOpenQuestionSnapshotProjection.Compose(journal, openQuestions);
    }
}
