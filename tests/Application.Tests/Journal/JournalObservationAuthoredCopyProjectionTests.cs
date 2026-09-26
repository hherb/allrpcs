using Allrpcs.Application.Journal;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Application.Tests.Journal;

public sealed class JournalObservationAuthoredCopyProjectionTests
{
    [Fact]
    public void CopiesOnlyExactIdentifiedAuthoredTextInObservationOrder()
    {
        string identifiedName = " Old Shrine ";
        string identifiedDescription = "  Layered marks cross the weathered stone.  ";
        WorldObject noticed = WorldObject("herb-patch", "Hidden herb", "Hidden herb text.");
        WorldObject identified = WorldObject(
            "old-shrine",
            identifiedName,
            identifiedDescription);
        WorldObject understood = WorldObject(
            "wagon",
            "Hidden wagon",
            "Hidden wagon text.");
        IReadOnlyList<JournalObservationEntry> entries = Entries(
            [
                new WorldObjectVisibility(new ObjectId("herb-patch"), VisibilityState.Noticed),
                new WorldObjectVisibility(new ObjectId("old-shrine"), VisibilityState.Identified),
                new WorldObjectVisibility(new ObjectId("wagon"), VisibilityState.Understood),
            ],
            [understood, identified, noticed]);

        IReadOnlyList<JournalObservationAuthoredCopy> copies =
            JournalObservationAuthoredCopyProjection.Project(entries);

        JournalObservationAuthoredCopy copy = Assert.Single(copies);
        Assert.Same(entries[1].Visibility.ObjectId, copy.ObjectId);
        Assert.Same(identifiedName, copy.DisplayName);
        Assert.Same(identifiedDescription, copy.Description);
        Assert.Collection(
            entries,
            entry => Assert.Same(noticed, entry.WorldObject),
            entry => Assert.Same(identified, entry.WorldObject),
            entry => Assert.Same(understood, entry.WorldObject));
    }

    [Fact]
    public void RetainsAnIdentifiedObjectWithAnEmptyAuthoredDescription()
    {
        WorldObject worldObject = WorldObject("marker", "Stone Marker", string.Empty);
        IReadOnlyList<JournalObservationEntry> entries = Entries(
            [new WorldObjectVisibility(new ObjectId("marker"), VisibilityState.Identified)],
            [worldObject]);

        JournalObservationAuthoredCopy copy = Assert.Single(
            JournalObservationAuthoredCopyProjection.Project(entries));

        Assert.Equal("Stone Marker", copy.DisplayName);
        Assert.Same(worldObject.Description.Text, copy.Description);
        Assert.Empty(copy.Description);
    }

    [Fact]
    public void ExcludesEveryAdmissibleNonIdentifiedVisibilityWithoutCopyingHiddenText()
    {
        VisibilityState[] excludedStates = VisibilityState.All
            .Where(state => state != VisibilityState.Unnoticed
                && state != VisibilityState.Identified)
            .ToArray();
        WorldObjectVisibility[] visibilityEntries = excludedStates
            .Select((state, index) => new WorldObjectVisibility(
                new ObjectId($"object-{index}"),
                state))
            .ToArray();
        WorldObject[] worldObjects = excludedStates
            .Select((_, index) => WorldObject(
                $"object-{index}",
                $"Hidden name {index}",
                $"Hidden description {index}."))
            .ToArray();
        IReadOnlyList<JournalObservationEntry> entries = Entries(
            visibilityEntries,
            worldObjects);

        IReadOnlyList<JournalObservationAuthoredCopy> copies =
            JournalObservationAuthoredCopyProjection.Project(entries);

        Assert.Empty(copies);
        Assert.Equal(excludedStates.Length, entries.Count);
    }

    [Fact]
    public void ReturnsIndependentReadOnlyCollectionsAndFreshCopies()
    {
        IReadOnlyList<JournalObservationEntry> entries = Entries(
            [new WorldObjectVisibility(new ObjectId("old-shrine"), VisibilityState.Identified)],
            [WorldObject("old-shrine", "Old Shrine", "Weathered stone.")]);

        IReadOnlyList<JournalObservationAuthoredCopy> first =
            JournalObservationAuthoredCopyProjection.Project(entries);
        IReadOnlyList<JournalObservationAuthoredCopy> second =
            JournalObservationAuthoredCopyProjection.Project(entries);

        Assert.NotSame(first, second);
        Assert.NotSame(Assert.Single(first), Assert.Single(second));
        Assert.Throws<NotSupportedException>(() =>
            ((IList<JournalObservationAuthoredCopy>)first).Clear());
    }

    [Fact]
    public void AcceptsEmptyInputAndRejectsNulls()
    {
        IReadOnlyList<JournalObservationEntry> empty = [];
        IReadOnlyList<JournalObservationEntry> containingNull =
            new JournalObservationEntry[] { null! };

        Assert.Empty(JournalObservationAuthoredCopyProjection.Project(empty));
        Assert.Throws<ArgumentNullException>(() =>
            JournalObservationAuthoredCopyProjection.Project(null!));
        Assert.Throws<ArgumentException>(() =>
            JournalObservationAuthoredCopyProjection.Project(containingNull));
    }

    private static IReadOnlyList<JournalObservationEntry> Entries(
        WorldObjectVisibility[] visibilityEntries,
        WorldObject[] worldObjects)
    {
        JournalSectionCatalogue catalogue = RepresentativeJournalSectionSource.Create();
        CharacterId seekerId = new("seeker");
        JournalObservationSectionBinding observations = JournalObservationSectionProjection.Bind(
            catalogue,
            seekerId,
            new CharacterPerceptionSnapshot(seekerId, visibilityEntries));
        JournalObservationWorldBinding binding = JournalObservationWorldProjection.Bind(
            observations,
            new WorldObjectSnapshot(worldObjects));
        return JournalObservationEntryProjection.Project(binding);
    }

    private static WorldObject WorldObject(
        string objectId,
        string displayName,
        string description) => new(
        new WorldObjectIdentity(new ObjectId(objectId), displayName),
        new WorldObjectDescription(description));
}
