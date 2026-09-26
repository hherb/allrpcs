using Allrpcs.Application.Journal;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Experiments;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Investigations;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.Persistence;
using Allrpcs.Domain.WorldEvents;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Application.Tests.Journal;

public sealed class JournalSaveStateCaptureProjectionTests
{
    [Fact]
    public void CapturesEverySafeSectionInStableOrderWithExactSelectionKey()
    {
        JournalSnapshot journal = CompleteJournal(withContent: true);
        JournalSectionDefinition selection = journal.Catalogue.Sections[3];

        JournalSaveState state = JournalSaveStateCaptureProjection.Capture(journal, selection);

        Assert.Same(journal.SeekerId, state.SeekerId);
        Assert.Equal("experiments", state.SelectedSectionKey);
        JournalObservationSaveEntry observation = Assert.Single(state.Observations);
        Assert.Equal("old-shrine", observation.ObjectId.Value);
        Assert.Equal("Old Shrine", observation.DisplayName);
        Assert.Equal("Layered marks cross the weathered stone.", observation.Description);
        JournalTestimonySaveEntry testimony = Assert.Single(state.Testimony);
        Assert.Equal("healer", testimony.SpeakerCharacterId.Value);
        Assert.Equal("The stream changed after the rain.", testimony.Text);
        Assert.Same(DialogueDeliveryMode.Whispering, testimony.DeliveryMode);
        JournalHypothesisSaveEntry hypothesis = Assert.Single(state.Hypotheses);
        Assert.Equal("stream-water", hypothesis.HypothesisId.Value);
        Assert.Equal("grey-sleep-cause", hypothesis.QuestionId.Value);
        Assert.Equal("What causes the grey sleep?", hypothesis.QuestionPrompt);
        Assert.Equal("The stream water may carry it.", hypothesis.Statement);
        JournalExperimentSaveEntry experiment = Assert.Single(state.Experiments);
        Assert.Equal("covered-water", experiment.ExperimentId.Value);
        Assert.Equal("Compare covered water.", experiment.Procedure);
        Assert.Equal("The covered sample remained clear.", experiment.ObservedOutcome);
        JournalOpenQuestionSaveEntry question = Assert.Single(state.OpenQuestions);
        Assert.Equal("grey-sleep-slowing", question.QuestionId.Value);
        Assert.Equal("What slows the grey sleep?", question.Prompt);
    }

    [Fact]
    public void CapturesExplicitNoSelectionAndEmptySectionArms()
    {
        JournalSnapshot journal = CompleteJournal(withContent: false);

        JournalSaveState state = JournalSaveStateCaptureProjection.Capture(journal, null);

        Assert.Null(state.SelectedSectionKey);
        Assert.Empty(state.Observations);
        Assert.Empty(state.Testimony);
        Assert.Empty(state.Hypotheses);
        Assert.Empty(state.Experiments);
        Assert.Empty(state.OpenQuestions);
    }

    [Fact]
    public void ReturnsFreshIndependentImmutableCopies()
    {
        JournalSnapshot journal = CompleteJournal(withContent: true);

        JournalSaveState first = JournalSaveStateCaptureProjection.Capture(journal, null);
        JournalSaveState second = JournalSaveStateCaptureProjection.Capture(journal, null);

        Assert.NotSame(first, second);
        Assert.NotSame(first.Observations, second.Observations);
        Assert.NotSame(first.Observations[0], second.Observations[0]);
        Assert.NotSame(first.Testimony[0], second.Testimony[0]);
        Assert.NotSame(first.Hypotheses[0], second.Hypotheses[0]);
        Assert.NotSame(first.Experiments[0], second.Experiments[0]);
        Assert.NotSame(first.OpenQuestions[0], second.OpenQuestions[0]);
        Assert.Throws<NotSupportedException>(() =>
            ((IList<JournalObservationSaveEntry>)first.Observations).Clear());
    }

    [Fact]
    public void RejectsIncompleteForeignAndRebuiltSelectionWithoutPartialOutput()
    {
        JournalSnapshot complete = CompleteJournal(withContent: true);
        JournalSnapshot incomplete = JournalSnapshotProjection.Compose(
            complete.Catalogue,
            complete.Observations);
        JournalSnapshot foreign = CompleteJournal(withContent: true);
        JournalSectionDefinition rebuilt = new(
            complete.Catalogue.Sections[0].Key,
            complete.Catalogue.Sections[0].DisplayLabel);

        Assert.Throws<ArgumentException>(() =>
            JournalSaveStateCaptureProjection.Capture(incomplete, null));
        Assert.Throws<ArgumentException>(() =>
            JournalSaveStateCaptureProjection.Capture(
                complete,
                foreign.Catalogue.Sections[0]));
        Assert.Throws<ArgumentException>(() =>
            JournalSaveStateCaptureProjection.Capture(complete, rebuilt));
        Assert.Same(complete.Observations, incomplete.Observations);
        Assert.Equal(4, incomplete.PendingSections.Count);
        Assert.Same(complete.Catalogue.Sections[0], complete.Observations.Section);
    }

    [Fact]
    public void RejectsNullJournal()
    {
        Assert.Throws<ArgumentNullException>(() =>
            JournalSaveStateCaptureProjection.Capture(null!, null));
    }

    [Fact]
    public void ExposesOnlyClosedContentSafePersistenceSurfaces()
    {
        Assert.Equal(
            [
                "Experiments",
                "Hypotheses",
                "Observations",
                "OpenQuestions",
                "SeekerId",
                "SelectedSectionKey",
                "Testimony",
            ],
            typeof(JournalSaveState)
                .GetProperties()
                .Select(property => property.Name)
                .Order(StringComparer.Ordinal));
        Assert.All(
            typeof(JournalSaveState).GetProperties(),
            property => Assert.False(property.CanWrite));
        Assert.DoesNotContain(
            typeof(JournalSaveState).GetProperties(),
            property => property.PropertyType.Namespace?.StartsWith(
                "Allrpcs.Application",
                StringComparison.Ordinal) == true);
        Assert.Equal(
            ["Description", "DisplayName", "ObjectId"],
            typeof(JournalObservationSaveEntry)
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
