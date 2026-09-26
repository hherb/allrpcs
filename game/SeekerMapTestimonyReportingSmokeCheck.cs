using Allrpcs.Application.Journal;
using Allrpcs.Application.Map;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Experiments;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Investigations;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks one explicit exact safe testimony-to-location reporting flow.</summary>
public partial class SeekerMapTestimonyReportingSmokeCheck : Node
{
    private const string CheckEnvironmentVariable =
        "ALLRPCS_SEEKER_MAP_TESTIMONY_REPORTING_CHECK";
    private const string EnabledEnvironmentValue = "1";

    /// <inheritdoc />
    public override void _Ready()
    {
        SetProcess(OS.GetEnvironment(CheckEnvironmentVariable) == EnabledEnvironmentValue);
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        SetProcess(false);

        SeekerMapTestimonyReportingCoordinator coordinator =
            GetNode<SeekerMapTestimonyReportingCoordinator>(
                "../SeekerMapTestimonyReportingCoordinator");
        SeekerMapController map = GetNode<SeekerMapController>("../SeekerMapController");
        JournalController journal = GetNode<JournalController>("../JournalController");
        TemporarySeekerCharacterSource character = GetNode<TemporarySeekerCharacterSource>(
            "../TemporarySeekerCharacterSource");
        SeekerMapPresentationController presentations =
            GetNode<SeekerMapPresentationController>("../SeekerMapPresentationController");
        SeekerMapPanel panel = GetNode<SeekerMapPanel>("../SeekerMapPanel");
        CharacterPerceptionSnapshot? perception = GetNode<FocusedSearchActivityController>(
            "../Seeker/FocusedSearchActivityController").Perception;
        WorldObjectSnapshot? objects = GetNode<WorldObjectController>(
            "../WorldObjectController").WorldObjects;

        if (map.Map is not null
            || map.Catalogue is not null
            || map.Notes is not null
            || journal.Journal is not null
            || presentations.Presentation is not null
            || panel.Presentation is not null
            || panel.Visible)
        {
            Fail("Map testimony reporting changed state during ordinary startup.");
            return;
        }

        CharacterId seekerId = character.Character.Identity.Id;
        JournalSnapshot exactJournal = CompleteJournal(
            seekerId,
            "This does not name either chosen location.",
            DialogueDeliveryMode.Whispering);
        JournalSnapshot rebuiltJournal = CompleteJournal(
            seekerId,
            "This does not name either chosen location.",
            DialogueDeliveryMode.Whispering);
        if (!map.TryInitialize(seekerId)
            || map.Catalogue is not { } catalogue
            || map.Map is not { } emptyMap
            || map.Notes is not { } emptyNotes
            || exactJournal.Testimony is not { } testimony
            || rebuiltJournal.Testimony is not { } rebuiltTestimony
            || !journal.TryInitialize(exactJournal))
        {
            Fail("Map testimony reporting smoke could not arrange exact safe state.");
            return;
        }

        MapLocationDefinition stream = catalogue.Locations[4];
        MapLocationDefinition laundry = catalogue.Locations[5];
        OpeningRegionMapLocationCatalogue foreignCatalogue =
            RepresentativeOpeningRegionMapLocationSource.Create();
        if (coordinator.TryReport(
                emptyMap,
                rebuiltJournal,
                rebuiltTestimony,
                seekerId,
                rebuiltTestimony.Entries[0],
                laundry)
            || coordinator.TryReport(
                emptyMap,
                exactJournal,
                rebuiltTestimony,
                seekerId,
                rebuiltTestimony.Entries[0],
                laundry)
            || coordinator.TryReport(
                emptyMap,
                exactJournal,
                testimony,
                new CharacterId("seeker"),
                testimony.Entries[0],
                laundry)
            || coordinator.TryReport(
                emptyMap,
                exactJournal,
                testimony,
                seekerId,
                rebuiltTestimony.Entries[0],
                laundry)
            || coordinator.TryReport(
                emptyMap,
                exactJournal,
                testimony,
                seekerId,
                testimony.Entries[0],
                foreignCatalogue.Locations[5])
            || !Unchanged(map, emptyMap, emptyNotes))
        {
            Fail("Map testimony reporting accepted foreign or rebuilt safe provenance.");
            return;
        }

        if (!coordinator.TryReport(
                emptyMap,
                exactJournal,
                testimony,
                seekerId,
                testimony.Entries[0],
                laundry)
            || map.Map is null
            || !ReferenceEquals(map.Notes, emptyNotes))
        {
            Fail("Map testimony reporting failed its first explicit report.");
            return;
        }

        SeekerMapSnapshot laundryReported = map.Map
            ?? throw new InvalidOperationException("First report did not retain map state.");
        if (laundryReported.ReportedLocations.Count != 1
            || !ReferenceEquals(laundryReported.ReportedLocations[0], laundry)
            || laundryReported.VerifiedLocations.Count != 0
            || testimony.Entries[0].Text != "This does not name either chosen location."
            || !ReferenceEquals(
                testimony.Entries[0].DeliveryMode,
                DialogueDeliveryMode.Whispering)
            || coordinator.TryReport(
                emptyMap,
                exactJournal,
                testimony,
                seekerId,
                testimony.Entries[0],
                stream)
            || coordinator.TryReport(
                laundryReported,
                exactJournal,
                testimony,
                seekerId,
                testimony.Entries[0],
                laundry))
        {
            Fail("Map testimony reporting classified text or accepted stale or repeated state.");
            return;
        }

        if (!coordinator.TryReport(
                laundryReported,
                exactJournal,
                testimony,
                seekerId,
                testimony.Entries[0],
                stream)
            || map.Map is null)
        {
            Fail("Map testimony reporting failed a second explicit report.");
            return;
        }

        SeekerMapSnapshot bothReported = map.Map
            ?? throw new InvalidOperationException("Second report did not retain map state.");
        if (bothReported.ReportedLocations.Count != 2
            || !ReferenceEquals(bothReported.ReportedLocations[0], stream)
            || !ReferenceEquals(bothReported.ReportedLocations[1], laundry)
            || bothReported.VerifiedLocations.Count != 0
            || laundryReported.ReportedLocations.Count != 1
            || emptyMap.ReportedLocations.Count != 0
            || !ReferenceEquals(journal.Journal, exactJournal)
            || !ReferenceEquals(exactJournal.Testimony, testimony)
            || !ReferenceEquals(map.Notes, emptyNotes)
            || !RejectsNull(() => coordinator.TryReport(
                null!,
                exactJournal,
                testimony,
                seekerId,
                testimony.Entries[0],
                stream))
            || presentations.Presentation is not null
            || panel.Presentation is not null
            || panel.Visible
            || !ReferenceEquals(
                GetNode<FocusedSearchActivityController>(
                    "../Seeker/FocusedSearchActivityController").Perception,
                perception)
            || !ReferenceEquals(
                GetNode<WorldObjectController>("../WorldObjectController").WorldObjects,
                objects)
            || !HasClosedSurface())
        {
            Fail("Map testimony reporting lost order, immutability, or unrelated isolation.");
            return;
        }

        GD.Print(
            "Seeker map testimony reporting smoke passed: inert startup, exact current journal "
            + "and Seeker provenance, explicit content-neutral reporting, canonical order, stale, "
            + "rebuilt, foreign, repeated and null rejection, immutable sources, closed surface, "
            + "and unrelated isolation were correct.");
        GetTree().Quit();
    }

    private static JournalSnapshot CompleteJournal(
        CharacterId seekerId,
        string text,
        DialogueDeliveryMode deliveryMode)
    {
        JournalSectionCatalogue catalogue = RepresentativeJournalSectionSource.Create();
        JournalObservationSectionBinding observations = JournalObservationSectionProjection.Bind(
            catalogue,
            seekerId,
            new CharacterPerceptionSnapshot(seekerId, []));
        JournalObservationWorldBinding world = JournalObservationWorldProjection.Bind(
            observations,
            new WorldObjectSnapshot([]));
        JournalSnapshot observationsOnly = JournalSnapshotProjection.Compose(
            catalogue,
            JournalObservationSectionSnapshotProjection.Project(world));
        CharacterId speakerId = new("healer");
        DialogueEvent dialogueEvent = new(
            new DialogueDelivery(
                new DialogueUtterance(speakerId, text),
                deliveryMode),
            new DialogueAudienceSnapshot(speakerId, [seekerId]));
        JournalTestimonySectionSnapshot testimony =
            JournalTestimonySectionSnapshotProjection.Project(
                JournalTestimonySectionProjection.Bind(observationsOnly, [dialogueEvent]));
        JournalSnapshot snapshot = JournalTestimonySnapshotProjection.Compose(
            observationsOnly,
            testimony);
        JournalHypothesisSectionSnapshot hypotheses =
            JournalHypothesisSectionSnapshotProjection.Project(
                JournalHypothesisSectionProjection.Bind(
                    snapshot,
                    new InvestigationHypothesisSnapshot([])));
        snapshot = JournalHypothesisSnapshotProjection.Compose(snapshot, hypotheses);
        JournalExperimentSectionSnapshot experiments =
            JournalExperimentSectionSnapshotProjection.Project(
                JournalExperimentSectionProjection.Bind(
                    snapshot,
                    new ExperimentJournalSnapshot([])));
        snapshot = JournalExperimentSnapshotProjection.Compose(snapshot, experiments);
        JournalOpenQuestionSectionSnapshot questions =
            JournalOpenQuestionSectionSnapshotProjection.Project(
                JournalOpenQuestionSectionProjection.Bind(
                    snapshot,
                    new OpenInvestigationQuestionSnapshot([])));
        return JournalOpenQuestionSnapshotProjection.Compose(snapshot, questions);
    }

    private static bool Unchanged(
        SeekerMapController map,
        SeekerMapSnapshot expectedMap,
        SeekerMapNoteSnapshot expectedNotes) =>
        ReferenceEquals(map.Map, expectedMap)
        && ReferenceEquals(map.Notes, expectedNotes);

    private static bool RejectsNull(Action action)
    {
        try
        {
            action();
        }
        catch (ArgumentNullException)
        {
            return true;
        }

        return false;
    }

    private static bool HasClosedSurface()
    {
        Type type = typeof(SeekerMapTestimonyReportingCoordinator);
        string[] methods = type.GetMethods(
                System.Reflection.BindingFlags.Public
                    | System.Reflection.BindingFlags.Instance
                    | System.Reflection.BindingFlags.DeclaredOnly)
            .Where(method => !method.IsSpecialName)
            .Select(method => method.Name)
            .Order(StringComparer.Ordinal)
            .ToArray();
        return methods.SequenceEqual(["TryReport", "_Ready"])
            && type.GetProperties(
                System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.DeclaredOnly).Length == 0;
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }
}
