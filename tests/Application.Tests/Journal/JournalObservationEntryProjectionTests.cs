using Allrpcs.Application.Journal;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Application.Tests.Journal;

public sealed class JournalObservationEntryProjectionTests
{
    [Fact]
    public void ProjectsStableExactEntriesByValueEqualIdentifier()
    {
        WorldObjectVisibility shrineVisibility = new(
            new ObjectId("old-shrine"),
            VisibilityState.Identified);
        WorldObjectVisibility herbVisibility = new(
            new ObjectId("herb-patch"),
            VisibilityState.Noticed);
        WorldObject unrelated = WorldObject("unrelated");
        WorldObject shrine = WorldObject("old-shrine");
        WorldObject herbPatch = WorldObject("herb-patch");
        JournalObservationWorldBinding binding = Binding(
            [shrineVisibility, herbVisibility],
            [unrelated, herbPatch, shrine]);

        IReadOnlyList<JournalObservationEntry> entries =
            JournalObservationEntryProjection.Project(binding);

        Assert.Collection(
            entries,
            entry =>
            {
                Assert.Same(shrineVisibility, entry.Visibility);
                Assert.Same(shrine, entry.WorldObject);
            },
            entry =>
            {
                Assert.Same(herbVisibility, entry.Visibility);
                Assert.Same(herbPatch, entry.WorldObject);
            });
        Assert.Collection(
            binding.Observations.Perception.Entries,
            entry => Assert.Same(shrineVisibility, entry),
            entry => Assert.Same(herbVisibility, entry));
        Assert.Collection(
            binding.WorldObjects.Entries,
            entry => Assert.Same(unrelated, entry),
            entry => Assert.Same(herbPatch, entry),
            entry => Assert.Same(shrine, entry));
    }

    [Fact]
    public void ReturnsIndependentReadOnlyCollections()
    {
        WorldObjectVisibility visibility = new(
            new ObjectId("old-shrine"),
            VisibilityState.Identified);
        WorldObject worldObject = WorldObject("old-shrine");
        JournalObservationWorldBinding binding = Binding([visibility], [worldObject]);

        IReadOnlyList<JournalObservationEntry> first =
            JournalObservationEntryProjection.Project(binding);
        IReadOnlyList<JournalObservationEntry> second =
            JournalObservationEntryProjection.Project(binding);

        Assert.NotSame(first, second);
        Assert.NotSame(Assert.Single(first), Assert.Single(second));
        Assert.Same(visibility, first[0].Visibility);
        Assert.Same(worldObject, first[0].WorldObject);
        Assert.Throws<NotSupportedException>(() =>
            ((IList<JournalObservationEntry>)first).Clear());
    }

    [Fact]
    public void ReturnsAnEmptyReadOnlyCollectionForEmptyPerception()
    {
        JournalObservationWorldBinding binding = Binding([], [WorldObject("unrelated")]);

        IReadOnlyList<JournalObservationEntry> entries =
            JournalObservationEntryProjection.Project(binding);

        Assert.Empty(entries);
        Assert.Throws<NotSupportedException>(() =>
            ((IList<JournalObservationEntry>)entries).Clear());
    }

    [Fact]
    public void RejectsNullBinding()
    {
        Assert.Throws<ArgumentNullException>(() =>
            JournalObservationEntryProjection.Project(null!));
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
        return JournalObservationWorldProjection.Bind(observations, new WorldObjectSnapshot(worldObjects));
    }

    private static WorldObject WorldObject(string objectId) => new(
        new WorldObjectIdentity(new ObjectId(objectId), $"Object {objectId}"),
        new WorldObjectDescription($"Description {objectId}."));
}
