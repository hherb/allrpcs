using Allrpcs.Application.Journal;
using Allrpcs.Application.Map;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Application.Tests.Map;

public sealed class SeekerMapTestimonyReportProjectionTests
{
    [Fact]
    public void ReportsOneExactCanonicalLocationFromAlreadySafeHeardTestimony()
    {
        OpeningRegionMapLocationCatalogue catalogue =
            RepresentativeOpeningRegionMapLocationSource.Create();
        CharacterId seekerId = new("seeker");
        SeekerMapSnapshot map = SeekerMapSnapshotProjection.Project(
            catalogue,
            seekerId,
            [],
            []);
        JournalTestimonySectionSnapshot testimony = Testimony(
            seekerId,
            "The abandoned laundry lies beyond the stream.");

        SeekerMapSnapshot result = SeekerMapTestimonyReportProjection.Project(
            catalogue,
            map,
            testimony,
            seekerId,
            testimony.Entries[0],
            catalogue.Locations[5]);

        Assert.Equal([catalogue.Locations[5]], result.ReportedLocations);
        Assert.Empty(result.VerifiedLocations);
        Assert.Empty(map.ReportedLocations);
        Assert.Same(seekerId, result.SeekerId);
    }

    [Fact]
    public void DoesNotInterpretTestimonyTextDeliveryModeOrSpeakerAsAuthority()
    {
        OpeningRegionMapLocationCatalogue catalogue =
            RepresentativeOpeningRegionMapLocationSource.Create();
        CharacterId seekerId = new("seeker");
        SeekerMapSnapshot map = SeekerMapSnapshotProjection.Project(
            catalogue,
            seekerId,
            [],
            []);
        JournalTestimonySectionSnapshot testimony = Testimony(
            seekerId,
            "This text does not name a canonical location.",
            DialogueDeliveryMode.Whispering);

        SeekerMapSnapshot result = SeekerMapTestimonyReportProjection.Project(
            catalogue,
            map,
            testimony,
            seekerId,
            testimony.Entries[0],
            catalogue.Locations[8]);

        Assert.Equal([catalogue.Locations[8]], result.ReportedLocations);
        Assert.Equal(
            "This text does not name a canonical location.",
            testimony.Entries[0].Text);
        Assert.Same(DialogueDeliveryMode.Whispering, testimony.Entries[0].DeliveryMode);
    }

    [Fact]
    public void RejectsForeignTestimonyOwnerEntryCatalogueLocationAndRepeatedReports()
    {
        OpeningRegionMapLocationCatalogue catalogue =
            RepresentativeOpeningRegionMapLocationSource.Create();
        OpeningRegionMapLocationCatalogue foreignCatalogue =
            RepresentativeOpeningRegionMapLocationSource.Create();
        CharacterId seekerId = new("seeker");
        SeekerMapSnapshot map = SeekerMapSnapshotProjection.Project(
            catalogue,
            seekerId,
            [],
            []);
        JournalTestimonySectionSnapshot testimony = Testimony(seekerId, "Use the stream path.");
        JournalTestimonySectionSnapshot foreignEntry = Testimony(seekerId, "Use the stream path.");
        JournalTestimonySectionSnapshot foreignOwner = Testimony(
            new CharacterId("seeker"),
            "Use the stream path.");

        Assert.Throws<ArgumentException>(() => SeekerMapTestimonyReportProjection.Project(
            catalogue,
            map,
            foreignOwner,
            seekerId,
            foreignOwner.Entries[0],
            catalogue.Locations[4]));
        Assert.Throws<ArgumentException>(() => SeekerMapTestimonyReportProjection.Project(
            catalogue,
            map,
            testimony,
            seekerId,
            foreignEntry.Entries[0],
            catalogue.Locations[4]));
        Assert.Throws<ArgumentException>(() => SeekerMapTestimonyReportProjection.Project(
            foreignCatalogue,
            map,
            testimony,
            seekerId,
            testimony.Entries[0],
            foreignCatalogue.Locations[4]));
        Assert.Throws<ArgumentException>(() => SeekerMapTestimonyReportProjection.Project(
            catalogue,
            map,
            testimony,
            seekerId,
            testimony.Entries[0],
            foreignCatalogue.Locations[4]));

        SeekerMapSnapshot reported = SeekerMapTestimonyReportProjection.Project(
            catalogue,
            map,
            testimony,
            seekerId,
            testimony.Entries[0],
            catalogue.Locations[4]);
        Assert.Throws<ArgumentException>(() => SeekerMapTestimonyReportProjection.Project(
            catalogue,
            reported,
            testimony,
            seekerId,
            testimony.Entries[0],
            catalogue.Locations[4]));
        Assert.Empty(map.ReportedLocations);
    }

    [Fact]
    public void RejectsNullInputs()
    {
        OpeningRegionMapLocationCatalogue catalogue =
            RepresentativeOpeningRegionMapLocationSource.Create();
        CharacterId seekerId = new("seeker");
        SeekerMapSnapshot map = SeekerMapSnapshotProjection.Project(
            catalogue,
            seekerId,
            [],
            []);
        JournalTestimonySectionSnapshot testimony = Testimony(seekerId, "Use the stream path.");
        JournalTestimonyAuthoredCopy entry = testimony.Entries[0];
        MapLocationDefinition location = catalogue.Locations[4];

        Assert.Throws<ArgumentNullException>(() =>
            SeekerMapTestimonyReportProjection.Project(
                null!, map, testimony, seekerId, entry, location));
        Assert.Throws<ArgumentNullException>(() =>
            SeekerMapTestimonyReportProjection.Project(
                catalogue, null!, testimony, seekerId, entry, location));
        Assert.Throws<ArgumentNullException>(() =>
            SeekerMapTestimonyReportProjection.Project(
                catalogue, map, null!, seekerId, entry, location));
        Assert.Throws<ArgumentNullException>(() =>
            SeekerMapTestimonyReportProjection.Project(
                catalogue, map, testimony, null!, entry, location));
        Assert.Throws<ArgumentNullException>(() =>
            SeekerMapTestimonyReportProjection.Project(
                catalogue, map, testimony, seekerId, null!, location));
        Assert.Throws<ArgumentNullException>(() =>
            SeekerMapTestimonyReportProjection.Project(
                catalogue, map, testimony, seekerId, entry, null!));
    }

    private static JournalTestimonySectionSnapshot Testimony(
        CharacterId seekerId,
        string text,
        DialogueDeliveryMode? mode = null)
    {
        JournalSectionCatalogue journalCatalogue =
            RepresentativeJournalSectionSource.Create();
        JournalObservationSectionBinding observations = JournalObservationSectionProjection.Bind(
            journalCatalogue,
            seekerId,
            new CharacterPerceptionSnapshot(seekerId, []));
        JournalObservationWorldBinding world = JournalObservationWorldProjection.Bind(
            observations,
            new WorldObjectSnapshot([]));
        JournalSnapshot journal = JournalSnapshotProjection.Compose(
            journalCatalogue,
            JournalObservationSectionSnapshotProjection.Project(world));
        CharacterId speakerId = new("healer");
        DialogueEvent dialogueEvent = new(
            new DialogueDelivery(
                new DialogueUtterance(speakerId, text),
                mode ?? DialogueDeliveryMode.OrdinarySpeech),
            new DialogueAudienceSnapshot(speakerId, [seekerId]));
        JournalTestimonySectionBinding binding =
            JournalTestimonySectionProjection.Bind(journal, [dialogueEvent]);
        return JournalTestimonySectionSnapshotProjection.Project(binding);
    }
}
