using Allrpcs.Application.Map;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Application.Tests.Map;

public sealed class SeekerMapPresentationTextProjectionTests
{
    [Fact]
    public void ProjectsStablePlainTextForKnownLocationsStatusAndNotes()
    {
        OpeningRegionMapLocationCatalogue catalogue =
            RepresentativeOpeningRegionMapLocationSource.Create();
        CharacterId seekerId = new("seeker");
        SeekerMapSnapshot map = SeekerMapSnapshotProjection.Project(
            catalogue,
            seekerId,
            [catalogue.Locations[4]],
            [catalogue.Locations[1]]);
        SeekerMapNoteSnapshot notes = SeekerMapNoteSnapshotProjection.CreateEmpty(
            catalogue,
            seekerId);
        notes = SeekerMapNoteSnapshotTransition.Set(
            catalogue,
            map,
            notes,
            seekerId,
            catalogue.Locations[1],
            "Return for cultivated herbs.");
        SeekerMapPresentationSnapshot presentation = SeekerMapPresentationProjection.Project(
            map,
            notes);

        SeekerMapPresentationText result = SeekerMapPresentationTextProjection.Project(
            presentation);

        Assert.Equal("Opening Region Map", result.Title);
        Assert.Equal(
            "Verified: Healer's Garden\nNote: Return for cultivated herbs.\n\nReported: Stream",
            result.Content);
    }

    [Fact]
    public void ProjectsNeutralEmptyTextWithoutHiddenLocations()
    {
        OpeningRegionMapLocationCatalogue catalogue =
            RepresentativeOpeningRegionMapLocationSource.Create();
        CharacterId seekerId = new("seeker");
        SeekerMapSnapshot map = SeekerMapSnapshotProjection.Project(catalogue, seekerId, [], []);
        SeekerMapNoteSnapshot notes = SeekerMapNoteSnapshotProjection.CreateEmpty(
            catalogue,
            seekerId);
        SeekerMapPresentationSnapshot presentation = SeekerMapPresentationProjection.Project(
            map,
            notes);

        SeekerMapPresentationText result = SeekerMapPresentationTextProjection.Project(
            presentation);

        Assert.Equal(SeekerMapPresentationTextProjection.EmptyContent, result.Content);
        Assert.DoesNotContain("Sickhouse", result.Content, StringComparison.Ordinal);
        Assert.DoesNotContain("Old Shrine", result.Content, StringComparison.Ordinal);
    }

    [Fact]
    public void RejectsNullAndExposesOnlyPlainText()
    {
        Assert.Throws<ArgumentNullException>(() =>
            SeekerMapPresentationTextProjection.Project(null!));
        Assert.Equal(
            ["Content", "Title"],
            typeof(SeekerMapPresentationText)
                .GetProperties()
                .Select(property => property.Name)
                .Order(StringComparer.Ordinal));
        Assert.All(
            typeof(SeekerMapPresentationText).GetProperties(),
            property => Assert.Null(property.SetMethod));
    }
}
