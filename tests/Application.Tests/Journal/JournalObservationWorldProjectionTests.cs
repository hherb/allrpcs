using Allrpcs.Application.Journal;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Application.Tests.Journal;

public sealed class JournalObservationWorldProjectionTests
{
    [Fact]
    public void BindsExactObservationAndWorldSnapshotsWithValueEqualTargets()
    {
        JournalObservationSectionBinding observations = Observations(
            new WorldObjectVisibility(new ObjectId("old-shrine"), VisibilityState.Identified),
            new WorldObjectVisibility(new ObjectId("herb-patch"), VisibilityState.Noticed));
        WorldObject unrelated = WorldObject("unrelated");
        WorldObject shrine = WorldObject("old-shrine");
        WorldObject herbPatch = WorldObject("herb-patch");
        WorldObjectSnapshot worldObjects = new([unrelated, shrine, herbPatch]);

        JournalObservationWorldBinding binding = JournalObservationWorldProjection.Bind(
            observations,
            worldObjects);

        Assert.Same(observations, binding.Observations);
        Assert.Same(worldObjects, binding.WorldObjects);
        Assert.Collection(
            binding.Observations.Perception.Entries,
            entry => Assert.Equal(shrine.Identity.Id, entry.ObjectId),
            entry => Assert.Equal(herbPatch.Identity.Id, entry.ObjectId));
        Assert.Collection(
            binding.WorldObjects.Entries,
            entry => Assert.Same(unrelated, entry),
            entry => Assert.Same(shrine, entry),
            entry => Assert.Same(herbPatch, entry));
    }

    [Fact]
    public void AcceptsEmptyPerceptionAndAnUnrelatedAuthoritativeWorld()
    {
        JournalObservationSectionBinding observations = Observations();
        WorldObject retained = WorldObject("retained");
        WorldObjectSnapshot worldObjects = new([retained]);

        JournalObservationWorldBinding binding = JournalObservationWorldProjection.Bind(
            observations,
            worldObjects);

        Assert.Same(observations, binding.Observations);
        Assert.Same(worldObjects, binding.WorldObjects);
        Assert.Same(retained, Assert.Single(binding.WorldObjects.Entries));
    }

    [Fact]
    public void RejectsAnyVisibleTargetMissingFromTheAuthoritativeWorld()
    {
        WorldObjectVisibility retained = new(
            new ObjectId("retained"),
            VisibilityState.Identified);
        WorldObjectVisibility missing = new(
            new ObjectId("missing"),
            VisibilityState.Noticed);
        JournalObservationSectionBinding observations = Observations(retained, missing);
        WorldObject worldObject = WorldObject("retained");
        WorldObjectSnapshot worldObjects = new([worldObject]);

        Assert.Throws<ArgumentException>(() => JournalObservationWorldProjection.Bind(
            observations,
            worldObjects));
        Assert.Collection(
            observations.Perception.Entries,
            entry => Assert.Same(retained, entry),
            entry => Assert.Same(missing, entry));
        Assert.Same(worldObject, Assert.Single(worldObjects.Entries));
    }

    [Fact]
    public void RejectsNullInputs()
    {
        JournalObservationSectionBinding observations = Observations();
        WorldObjectSnapshot worldObjects = new([]);

        Assert.Throws<ArgumentNullException>(() => JournalObservationWorldProjection.Bind(
            null!,
            worldObjects));
        Assert.Throws<ArgumentNullException>(() => JournalObservationWorldProjection.Bind(
            observations,
            null!));
    }

    private static JournalObservationSectionBinding Observations(
        params WorldObjectVisibility[] entries)
    {
        JournalSectionCatalogue catalogue = RepresentativeJournalSectionSource.Create();
        CharacterId seekerId = new("seeker");
        return JournalObservationSectionProjection.Bind(
            catalogue,
            seekerId,
            new CharacterPerceptionSnapshot(seekerId, entries));
    }

    private static WorldObject WorldObject(string objectId) => new(
        new WorldObjectIdentity(new ObjectId(objectId), $"Object {objectId}"),
        new WorldObjectDescription($"Description {objectId}."));
}
