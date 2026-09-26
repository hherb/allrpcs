using Allrpcs.Application.Map;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Application.Tests.Map;

public sealed class SeekerMapNoteSnapshotTransitionTests
{
    [Fact]
    public void CreatesEmptySafeSeekerOwnedNoteState()
    {
        OpeningRegionMapLocationCatalogue catalogue = Catalogue();
        CharacterId seekerId = new("seeker");

        SeekerMapNoteSnapshot notes = SeekerMapNoteSnapshotProjection.CreateEmpty(
            catalogue,
            seekerId);

        Assert.Same(seekerId, notes.SeekerId);
        Assert.Empty(notes.Notes);
        Assert.Equal(
            ["Notes", "SeekerId"],
            typeof(SeekerMapNoteSnapshot)
                .GetProperties()
                .Select(property => property.Name)
                .Order());
    }

    [Fact]
    public void SetsNotesOnlyForReportedOrVerifiedLocationsInCanonicalOrder()
    {
        OpeningRegionMapLocationCatalogue catalogue = Catalogue();
        CharacterId seekerId = new("seeker");
        SeekerMapSnapshot map = SeekerMapSnapshotProjection.Project(
            catalogue,
            seekerId,
            [catalogue.Locations[8]],
            [catalogue.Locations[1]]);
        SeekerMapNoteSnapshot empty = SeekerMapNoteSnapshotProjection.CreateEmpty(
            catalogue,
            seekerId);

        SeekerMapNoteSnapshot first = SeekerMapNoteSnapshotTransition.Set(
            catalogue,
            map,
            empty,
            seekerId,
            catalogue.Locations[8],
            " Ask the traveller about the road. ");
        SeekerMapNoteSnapshot second = SeekerMapNoteSnapshotTransition.Set(
            catalogue,
            map,
            first,
            seekerId,
            catalogue.Locations[1],
            "Check the cultivated beds.");

        Assert.Collection(
            second.Notes,
            note => AssertNote(
                note,
                catalogue.Locations[1],
                "Check the cultivated beds."),
            note => AssertNote(
                note,
                catalogue.Locations[8],
                " Ask the traveller about the road. "));
        Assert.Empty(empty.Notes);
        Assert.Single(first.Notes);
        Assert.Equal(
            ["Location", "Text"],
            typeof(SeekerMapNote)
                .GetProperties()
                .Select(property => property.Name)
                .Order());
    }

    [Fact]
    public void ReplacesAndRemovesOneLocationNoteWithoutChangingInputs()
    {
        OpeningRegionMapLocationCatalogue catalogue = Catalogue();
        CharacterId seekerId = new("seeker");
        SeekerMapSnapshot map = SeekerMapSnapshotProjection.Project(
            catalogue,
            seekerId,
            [catalogue.Locations[4], catalogue.Locations[5]],
            []);
        SeekerMapNoteSnapshot empty = SeekerMapNoteSnapshotProjection.CreateEmpty(
            catalogue,
            seekerId);
        SeekerMapNoteSnapshot first = SeekerMapNoteSnapshotTransition.Set(
            catalogue, map, empty, seekerId, catalogue.Locations[4], "First note.");
        SeekerMapNoteSnapshot both = SeekerMapNoteSnapshotTransition.Set(
            catalogue, map, first, seekerId, catalogue.Locations[5], "Laundry note.");

        SeekerMapNoteSnapshot replaced = SeekerMapNoteSnapshotTransition.Set(
            catalogue,
            map,
            both,
            seekerId,
            catalogue.Locations[4],
            "Revised stream note.");
        SeekerMapNoteSnapshot removed = SeekerMapNoteSnapshotTransition.Remove(
            catalogue,
            map,
            replaced,
            seekerId,
            catalogue.Locations[5]);

        Assert.Collection(
            replaced.Notes,
            note => AssertNote(note, catalogue.Locations[4], "Revised stream note."),
            note => AssertNote(note, catalogue.Locations[5], "Laundry note."));
        Assert.Collection(
            removed.Notes,
            note => AssertNote(note, catalogue.Locations[4], "Revised stream note."));
        Assert.Collection(
            both.Notes,
            note => AssertNote(note, catalogue.Locations[4], "First note."),
            note => AssertNote(note, catalogue.Locations[5], "Laundry note."));
    }

    [Fact]
    public void CopiesCollectionsAndAcceptsTheMaximumBoundedText()
    {
        OpeningRegionMapLocationCatalogue catalogue = Catalogue();
        CharacterId seekerId = new("seeker");
        SeekerMapSnapshot map = SeekerMapSnapshotProjection.Project(
            catalogue,
            seekerId,
            [catalogue.Locations[0]],
            []);
        SeekerMapNoteSnapshot empty = SeekerMapNoteSnapshotProjection.CreateEmpty(
            catalogue,
            seekerId);
        string text = new('n', SeekerMapNote.MaximumTextLength);

        SeekerMapNoteSnapshot notes = SeekerMapNoteSnapshotTransition.Set(
            catalogue,
            map,
            empty,
            seekerId,
            catalogue.Locations[0],
            text);

        Assert.Equal(text, Assert.Single(notes.Notes).Text);
        Assert.Throws<NotSupportedException>(() =>
            ((IList<SeekerMapNote>)notes.Notes).Clear());
    }

    [Fact]
    public void RejectsHiddenForeignCrossOwnerInvalidAndRepeatedRemovalWithoutMutation()
    {
        OpeningRegionMapLocationCatalogue catalogue = Catalogue();
        OpeningRegionMapLocationCatalogue foreignCatalogue = Catalogue();
        CharacterId seekerId = new("seeker");
        SeekerMapSnapshot map = SeekerMapSnapshotProjection.Project(
            catalogue,
            seekerId,
            [catalogue.Locations[0]],
            []);
        SeekerMapNoteSnapshot empty = SeekerMapNoteSnapshotProjection.CreateEmpty(
            catalogue,
            seekerId);

        Assert.Throws<ArgumentException>(() => SeekerMapNoteSnapshotTransition.Set(
            catalogue,
            map,
            empty,
            seekerId,
            catalogue.Locations[1],
            "Hidden."));
        Assert.Throws<ArgumentException>(() => SeekerMapNoteSnapshotTransition.Set(
            foreignCatalogue,
            map,
            empty,
            seekerId,
            foreignCatalogue.Locations[0],
            "Foreign."));
        Assert.Throws<ArgumentException>(() => SeekerMapNoteSnapshotTransition.Set(
            catalogue,
            map,
            empty,
            new CharacterId("seeker"),
            catalogue.Locations[0],
            "Cross owner."));
        Assert.Throws<ArgumentException>(() => SeekerMapNoteSnapshotTransition.Set(
            catalogue,
            map,
            empty,
            seekerId,
            foreignCatalogue.Locations[0],
            "Foreign location."));
        Assert.Throws<ArgumentException>(() => SeekerMapNoteSnapshotTransition.Set(
            catalogue,
            map,
            empty,
            seekerId,
            catalogue.Locations[0],
            " \t\r\n"));
        Assert.Throws<ArgumentOutOfRangeException>(() => SeekerMapNoteSnapshotTransition.Set(
            catalogue,
            map,
            empty,
            seekerId,
            catalogue.Locations[0],
            new string('n', SeekerMapNote.MaximumTextLength + 1)));
        Assert.Throws<ArgumentException>(() => SeekerMapNoteSnapshotTransition.Remove(
            catalogue,
            map,
            empty,
            seekerId,
            catalogue.Locations[0]));
        Assert.Empty(empty.Notes);
    }

    [Fact]
    public void RejectsNotesRetainedForLocationsHiddenByAnUnrelatedMapSnapshot()
    {
        OpeningRegionMapLocationCatalogue catalogue = Catalogue();
        CharacterId seekerId = new("seeker");
        SeekerMapSnapshot bothKnown = SeekerMapSnapshotProjection.Project(
            catalogue,
            seekerId,
            [catalogue.Locations[0], catalogue.Locations[1]],
            []);
        SeekerMapNoteSnapshot empty = SeekerMapNoteSnapshotProjection.CreateEmpty(
            catalogue,
            seekerId);
        SeekerMapNoteSnapshot notes = SeekerMapNoteSnapshotTransition.Set(
            catalogue,
            bothKnown,
            empty,
            seekerId,
            catalogue.Locations[1],
            "Retained note.");
        SeekerMapSnapshot unrelated = SeekerMapSnapshotProjection.Project(
            catalogue,
            seekerId,
            [catalogue.Locations[0]],
            []);

        Assert.Throws<ArgumentException>(() => SeekerMapNoteSnapshotTransition.Set(
            catalogue,
            unrelated,
            notes,
            seekerId,
            catalogue.Locations[0],
            "New note."));
        Assert.Collection(
            notes.Notes,
            note => AssertNote(note, catalogue.Locations[1], "Retained note."));
    }

    [Fact]
    public void RejectsNullInputs()
    {
        OpeningRegionMapLocationCatalogue catalogue = Catalogue();
        CharacterId seekerId = new("seeker");
        SeekerMapSnapshot map = SeekerMapSnapshotProjection.Project(
            catalogue,
            seekerId,
            [catalogue.Locations[0]],
            []);
        SeekerMapNoteSnapshot notes = SeekerMapNoteSnapshotProjection.CreateEmpty(
            catalogue,
            seekerId);
        MapLocationDefinition location = catalogue.Locations[0];

        Assert.Throws<ArgumentNullException>(() =>
            SeekerMapNoteSnapshotProjection.CreateEmpty(null!, seekerId));
        Assert.Throws<ArgumentNullException>(() =>
            SeekerMapNoteSnapshotProjection.CreateEmpty(catalogue, null!));
        Assert.Throws<ArgumentNullException>(() => SeekerMapNoteSnapshotTransition.Set(
            null!, map, notes, seekerId, location, "Note."));
        Assert.Throws<ArgumentNullException>(() => SeekerMapNoteSnapshotTransition.Set(
            catalogue, null!, notes, seekerId, location, "Note."));
        Assert.Throws<ArgumentNullException>(() => SeekerMapNoteSnapshotTransition.Set(
            catalogue, map, null!, seekerId, location, "Note."));
        Assert.Throws<ArgumentNullException>(() => SeekerMapNoteSnapshotTransition.Set(
            catalogue, map, notes, null!, location, "Note."));
        Assert.Throws<ArgumentNullException>(() => SeekerMapNoteSnapshotTransition.Set(
            catalogue, map, notes, seekerId, null!, "Note."));
        Assert.Throws<ArgumentNullException>(() => SeekerMapNoteSnapshotTransition.Set(
            catalogue, map, notes, seekerId, location, null!));
    }

    private static OpeningRegionMapLocationCatalogue Catalogue() =>
        RepresentativeOpeningRegionMapLocationSource.Create();

    private static void AssertNote(
        SeekerMapNote note,
        MapLocationDefinition location,
        string text)
    {
        Assert.Same(location, note.Location);
        Assert.Equal(text, note.Text);
    }
}
