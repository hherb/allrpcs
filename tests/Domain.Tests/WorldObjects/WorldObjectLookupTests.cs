using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Domain.Tests.WorldObjects;

public sealed class WorldObjectLookupTests
{
    [Fact]
    public void ReturnsNullForEmptyOrUnrelatedSnapshots()
    {
        ObjectId objectId = new("weathered-inscription");
        WorldObjectSnapshot empty = Snapshot();
        WorldObjectSnapshot unrelated = Snapshot(
            Create("concealed-healing-herb", "Concealed Healing Herb", "A hidden plant."));

        WorldObject? emptyResult = WorldObjectLookup.FindById(objectId, empty);
        WorldObject? unrelatedResult = WorldObjectLookup.FindById(objectId, unrelated);

        Assert.Null(emptyResult);
        Assert.Null(unrelatedResult);
    }

    [Fact]
    public void MatchesAValueEqualDistinctObjectIdentifier()
    {
        ObjectId retainedObjectId = new("weathered-inscription");
        WorldObject retained = Create(
            retainedObjectId,
            "Weathered Inscription",
            "Faded marks cover the stone.");
        WorldObjectSnapshot snapshot = Snapshot(retained);
        ObjectId lookupObjectId = new("weathered-inscription");

        WorldObject? result = WorldObjectLookup.FindById(lookupObjectId, snapshot);

        Assert.NotSame(lookupObjectId, retainedObjectId);
        Assert.Equal(lookupObjectId, retainedObjectId);
        Assert.Same(retained, result);
    }

    [Fact]
    public void ReturnsTheExactMatchAmongSurroundingEntries()
    {
        ObjectId objectId = new("weathered-inscription");
        WorldObject before = Create("wagon-residue", "Wagon Residue", "A pale residue.");
        WorldObject matching = Create(
            objectId,
            "Weathered Inscription",
            "Faded marks cover the stone.");
        WorldObject after = Create(
            "concealed-healing-herb",
            "Concealed Healing Herb",
            "A hidden plant.");
        WorldObjectSnapshot snapshot = Snapshot(before, matching, after);

        WorldObject? result = WorldObjectLookup.FindById(objectId, snapshot);

        Assert.Same(matching, result);
    }

    [Fact]
    public void RetainsTheExactWorldObjectAndNestedInstances()
    {
        ObjectId retainedObjectId = new("weathered-inscription");
        WorldObjectIdentity retainedIdentity = new(
            retainedObjectId,
            "Weathered Inscription");
        WorldObjectDescription retainedDescription = new(
            "Faded marks cover the stone.");
        WorldObject retained = new(retainedIdentity, retainedDescription);
        WorldObjectSnapshot snapshot = Snapshot(retained);

        WorldObject? result = WorldObjectLookup.FindById(retainedObjectId, snapshot);

        Assert.NotNull(result);
        Assert.Same(retained, result);
        Assert.Same(retainedIdentity, result.Identity);
        Assert.Same(retainedObjectId, result.Identity.Id);
        Assert.Same(retainedDescription, result.Description);
    }

    [Fact]
    public void DoesNotMutateAnyInput()
    {
        ObjectId objectId = new("weathered-inscription");
        WorldObject matching = Create(
            objectId,
            "Weathered Inscription",
            "Faded marks cover the stone.");
        WorldObject unrelated = Create("wagon-residue", "Wagon Residue", "A pale residue.");
        WorldObjectSnapshot snapshot = Snapshot(matching, unrelated);
        WorldObjectIdentity identity = matching.Identity;
        WorldObjectDescription description = matching.Description;

        WorldObject? result = WorldObjectLookup.FindById(objectId, snapshot);

        Assert.Same(matching, result);
        Assert.Collection(
            snapshot.Entries,
            entry => Assert.Same(matching, entry),
            entry => Assert.Same(unrelated, entry));
        Assert.Same(identity, matching.Identity);
        Assert.Same(objectId, matching.Identity.Id);
        Assert.Same(description, matching.Description);
    }

    [Fact]
    public void RejectsNullInputs()
    {
        ObjectId objectId = new("weathered-inscription");
        WorldObjectSnapshot snapshot = Snapshot();

        Assert.Throws<ArgumentNullException>(() => WorldObjectLookup.FindById(null!, snapshot));
        Assert.Throws<ArgumentNullException>(() => WorldObjectLookup.FindById(objectId, null!));
    }

    [Fact]
    public void ExposesOnlyTheSinglePureLookupOperation()
    {
        Type lookupType = typeof(WorldObjectLookup);
        System.Reflection.MethodInfo method = Assert.Single(
            lookupType
                .GetMethods()
                .Where(candidate => candidate.DeclaringType == lookupType));

        Assert.Equal("FindById", method.Name);
        Assert.Equal(typeof(WorldObject), method.ReturnType);
        Assert.True(method.IsStatic);
        Assert.Collection(
            method.GetParameters(),
            parameter => Assert.Equal(typeof(ObjectId), parameter.ParameterType),
            parameter => Assert.Equal(typeof(WorldObjectSnapshot), parameter.ParameterType));
        Assert.Empty(lookupType.GetConstructors());
        Assert.Empty(lookupType.GetProperties());
        Assert.Empty(lookupType.GetEvents());
        Assert.True(lookupType.IsAbstract);
        Assert.True(lookupType.IsSealed);
    }

    private static WorldObject Create(
        string objectId,
        string displayName,
        string description) =>
        Create(new ObjectId(objectId), displayName, description);

    private static WorldObject Create(
        ObjectId objectId,
        string displayName,
        string description) =>
        new(
            new WorldObjectIdentity(objectId, displayName),
            new WorldObjectDescription(description));

    private static WorldObjectSnapshot Snapshot(params WorldObject[] entries) => new(entries);
}
