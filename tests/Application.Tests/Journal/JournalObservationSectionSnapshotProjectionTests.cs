using Allrpcs.Application.Journal;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Application.Tests.Journal;

public sealed class JournalObservationSectionSnapshotProjectionTests
{
    [Fact]
    public void ProjectsTheExactSectionAndSeekerWithSafeIdentifiedEntries()
    {
        WorldObjectVisibility noticed = new(
            new ObjectId("herb-patch"),
            VisibilityState.Noticed);
        WorldObjectVisibility identified = new(
            new ObjectId("old-shrine"),
            VisibilityState.Identified);
        JournalObservationWorldBinding binding = Binding(
            [noticed, identified],
            [
                WorldObject("old-shrine", "Old Shrine", "Layered marks cross the stone."),
                WorldObject("herb-patch", "Hidden Herb", "Hidden herb text."),
            ]);

        JournalObservationSectionSnapshot snapshot =
            JournalObservationSectionSnapshotProjection.Project(binding);

        Assert.Same(binding.Observations.Section, snapshot.Section);
        Assert.Same(binding.Observations.SeekerId, snapshot.SeekerId);
        JournalObservationAuthoredCopy entry = Assert.Single(snapshot.Entries);
        Assert.Same(identified.ObjectId, entry.ObjectId);
        Assert.Equal("Old Shrine", entry.DisplayName);
        Assert.Equal("Layered marks cross the stone.", entry.Description);
        Assert.Collection(
            binding.Observations.Perception.Entries,
            entry => Assert.Same(noticed, entry),
            entry => Assert.Same(identified, entry));
    }

    [Fact]
    public void ReturnsIndependentSnapshotsWithIndependentReadOnlyEntries()
    {
        JournalObservationWorldBinding binding = Binding(
            [new WorldObjectVisibility(new ObjectId("old-shrine"), VisibilityState.Identified)],
            [WorldObject("old-shrine", "Old Shrine", "Weathered stone.")]);

        JournalObservationSectionSnapshot first =
            JournalObservationSectionSnapshotProjection.Project(binding);
        JournalObservationSectionSnapshot second =
            JournalObservationSectionSnapshotProjection.Project(binding);

        Assert.NotSame(first, second);
        Assert.NotSame(first.Entries, second.Entries);
        Assert.NotSame(Assert.Single(first.Entries), Assert.Single(second.Entries));
        Assert.Throws<NotSupportedException>(() =>
            ((IList<JournalObservationAuthoredCopy>)first.Entries).Clear());
    }

    [Fact]
    public void AcceptsEmptyPerceptionWithoutExposingAuthoritativeWorldState()
    {
        JournalObservationWorldBinding binding = Binding(
            [],
            [WorldObject("unrelated", "Unrelated", "Unrelated description.")]);

        JournalObservationSectionSnapshot snapshot =
            JournalObservationSectionSnapshotProjection.Project(binding);

        Assert.Empty(snapshot.Entries);
        Assert.Equal(
            ["Entries", "Section", "SeekerId"],
            typeof(JournalObservationSectionSnapshot)
                .GetProperties()
                .Select(property => property.Name)
                .Order());
    }

    [Fact]
    public void RejectsNullBinding()
    {
        Assert.Throws<ArgumentNullException>(() =>
            JournalObservationSectionSnapshotProjection.Project(null!));
    }

    private static JournalObservationWorldBinding Binding(
        WorldObjectVisibility[] visibilityEntries,
        WorldObject[] worldObjects)
    {
        JournalSectionCatalogue catalogue = RepresentativeJournalSectionSource.Create();
        CharacterId seekerId = new("seeker");
        JournalObservationSectionBinding observations = JournalObservationSectionProjection.Bind(
            catalogue,
            seekerId,
            new CharacterPerceptionSnapshot(seekerId, visibilityEntries));
        return JournalObservationWorldProjection.Bind(
            observations,
            new WorldObjectSnapshot(worldObjects));
    }

    private static WorldObject WorldObject(
        string objectId,
        string displayName,
        string description) => new(
        new WorldObjectIdentity(new ObjectId(objectId), displayName),
        new WorldObjectDescription(description));
}
