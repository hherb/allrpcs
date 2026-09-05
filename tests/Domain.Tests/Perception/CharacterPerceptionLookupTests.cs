using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;

namespace Allrpcs.Domain.Tests.Perception;

public sealed class CharacterPerceptionLookupTests
{
    [Fact]
    public void ReturnsNullForEmptyOrUnrelatedSnapshots()
    {
        ObjectId objectId = new("weathered-inscription");
        CharacterPerceptionSnapshot empty = Snapshot("seeker");
        CharacterPerceptionSnapshot unrelated = Snapshot(
            "seeker",
            Visibility("concealed-healing-herb", VisibilityState.Noticed));

        WorldObjectVisibility? emptyResult = CharacterPerceptionLookup.FindByObject(
            objectId,
            empty);
        WorldObjectVisibility? unrelatedResult = CharacterPerceptionLookup.FindByObject(
            objectId,
            unrelated);

        Assert.Null(emptyResult);
        Assert.Null(unrelatedResult);
    }

    [Fact]
    public void MatchesAValueEqualDistinctObjectIdentifier()
    {
        ObjectId retainedObjectId = new("weathered-inscription");
        WorldObjectVisibility retainedEntry = new(
            retainedObjectId,
            VisibilityState.Understood);
        CharacterPerceptionSnapshot snapshot = Snapshot("seeker", retainedEntry);
        ObjectId lookupObjectId = new("weathered-inscription");

        WorldObjectVisibility? result = CharacterPerceptionLookup.FindByObject(
            lookupObjectId,
            snapshot);

        Assert.NotSame(lookupObjectId, retainedObjectId);
        Assert.Equal(lookupObjectId, retainedObjectId);
        Assert.Same(retainedEntry, result);
    }

    [Fact]
    public void ReturnsTheExactMatchAmongSurroundingUnrelatedEntries()
    {
        ObjectId objectId = new("weathered-inscription");
        WorldObjectVisibility before = Visibility(
            "wagon-residue",
            VisibilityState.Noticed);
        WorldObjectVisibility matching = new(objectId, VisibilityState.Identified);
        WorldObjectVisibility after = Visibility(
            "concealed-healing-herb",
            VisibilityState.Understood);
        CharacterPerceptionSnapshot snapshot = Snapshot(
            "seeker",
            before,
            matching,
            after);

        WorldObjectVisibility? result = CharacterPerceptionLookup.FindByObject(
            objectId,
            snapshot);

        Assert.Same(matching, result);
    }

    [Fact]
    public void RetainsTheExactEntryAndNestedInstances()
    {
        CharacterId retainedCharacterId = new("seeker");
        ObjectId retainedObjectId = new("weathered-inscription");
        VisibilityState retainedVisibilityState = VisibilityState.Understood;
        WorldObjectVisibility retainedEntry = new(
            retainedObjectId,
            retainedVisibilityState);
        CharacterPerceptionSnapshot snapshot = new(
            retainedCharacterId,
            new[] { retainedEntry });

        WorldObjectVisibility? result = CharacterPerceptionLookup.FindByObject(
            retainedObjectId,
            snapshot);

        Assert.NotNull(result);
        Assert.Same(retainedEntry, result);
        Assert.Same(retainedObjectId, result.ObjectId);
        Assert.Same(retainedVisibilityState, result.VisibilityState);
        Assert.Same(retainedCharacterId, snapshot.CharacterId);
    }

    [Fact]
    public void DoesNotMutateAnyInput()
    {
        CharacterId characterId = new("seeker");
        ObjectId objectId = new("weathered-inscription");
        VisibilityState visibilityState = VisibilityState.Identified;
        WorldObjectVisibility matching = new(objectId, visibilityState);
        WorldObjectVisibility unrelated = Visibility(
            "wagon-residue",
            VisibilityState.Noticed);
        CharacterPerceptionSnapshot snapshot = new(
            characterId,
            new[] { matching, unrelated });

        WorldObjectVisibility? result = CharacterPerceptionLookup.FindByObject(
            objectId,
            snapshot);

        Assert.Same(matching, result);
        Assert.Collection(
            snapshot.Entries,
            entry => Assert.Same(matching, entry),
            entry => Assert.Same(unrelated, entry));
        Assert.Same(characterId, snapshot.CharacterId);
        Assert.Same(objectId, matching.ObjectId);
        Assert.Same(visibilityState, matching.VisibilityState);
    }

    [Fact]
    public void RejectsNullInputs()
    {
        ObjectId objectId = new("weathered-inscription");
        CharacterPerceptionSnapshot snapshot = Snapshot("seeker");

        Assert.Throws<ArgumentNullException>(
            () => CharacterPerceptionLookup.FindByObject(null!, snapshot));
        Assert.Throws<ArgumentNullException>(
            () => CharacterPerceptionLookup.FindByObject(objectId, null!));
    }

    [Fact]
    public void ExposesOnlyTheSinglePureLookupOperation()
    {
        Type lookupType = typeof(CharacterPerceptionLookup);
        System.Reflection.MethodInfo method = Assert.Single(
            lookupType
                .GetMethods()
                .Where(candidate => candidate.DeclaringType == lookupType));

        Assert.Equal("FindByObject", method.Name);
        Assert.Equal(typeof(WorldObjectVisibility), method.ReturnType);
        Assert.True(method.IsStatic);
        Assert.Collection(
            method.GetParameters(),
            parameter => Assert.Equal(typeof(ObjectId), parameter.ParameterType),
            parameter => Assert.Equal(
                typeof(CharacterPerceptionSnapshot),
                parameter.ParameterType));
        Assert.Empty(lookupType.GetConstructors());
        Assert.Empty(lookupType.GetProperties());
        Assert.Empty(lookupType.GetEvents());
        Assert.True(lookupType.IsAbstract);
        Assert.True(lookupType.IsSealed);
    }

    private static CharacterPerceptionSnapshot Snapshot(
        string characterId,
        params WorldObjectVisibility[] entries) =>
        new(new CharacterId(characterId), entries);

    private static WorldObjectVisibility Visibility(
        string objectId,
        VisibilityState state) =>
        new(new ObjectId(objectId), state);
}
