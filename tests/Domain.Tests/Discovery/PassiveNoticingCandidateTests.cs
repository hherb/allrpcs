using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Discovery;

public sealed class PassiveNoticingCandidateTests
{
    [Fact]
    public void RetainsObserverAndTargetIdentifierInstances()
    {
        CharacterId observerCharacterId = new("seeker");
        ObjectId targetObjectId = new("concealed-healing-herb");

        PassiveNoticingCandidate candidate = new(observerCharacterId, targetObjectId);

        Assert.Same(observerCharacterId, candidate.ObserverCharacterId);
        Assert.Same(targetObjectId, candidate.TargetObjectId);
    }

    [Fact]
    public void EqualityUsesExistingIdentifierValues()
    {
        CharacterId firstObserverCharacterId = new("seeker");
        CharacterId sameObserverCharacterId = new("seeker");
        ObjectId firstTargetObjectId = new("concealed-healing-herb");
        ObjectId sameTargetObjectId = new("concealed-healing-herb");

        PassiveNoticingCandidate first = new(firstObserverCharacterId, firstTargetObjectId);
        PassiveNoticingCandidate same = new(sameObserverCharacterId, sameTargetObjectId);
        PassiveNoticingCandidate differentObserver = new(
            new CharacterId("travelling-healer"),
            new ObjectId("concealed-healing-herb"));
        PassiveNoticingCandidate differentTarget = new(
            new CharacterId("seeker"),
            new ObjectId("weathered-inscription"));

        Assert.NotSame(firstObserverCharacterId, sameObserverCharacterId);
        Assert.NotSame(firstTargetObjectId, sameTargetObjectId);
        Assert.Equal(first, same);
        Assert.True(first == same);
        Assert.NotEqual(first, differentObserver);
        Assert.NotEqual(first, differentTarget);
        Assert.Same(firstObserverCharacterId, first.ObserverCharacterId);
        Assert.Same(firstTargetObjectId, first.TargetObjectId);
        Assert.Same(sameObserverCharacterId, same.ObserverCharacterId);
        Assert.Same(sameTargetObjectId, same.TargetObjectId);
    }

    [Fact]
    public void RejectsNullValues()
    {
        Assert.Throws<ArgumentNullException>(
            () => new PassiveNoticingCandidate(
                null!,
                new ObjectId("concealed-healing-herb")));
        Assert.Throws<ArgumentNullException>(
            () => new PassiveNoticingCandidate(
                new CharacterId("seeker"),
                null!));
    }

    [Fact]
    public void LeavesIdentifierInputsUnchanged()
    {
        CharacterId observerCharacterId = new("seeker");
        ObjectId targetObjectId = new("concealed-healing-herb");

        _ = new PassiveNoticingCandidate(observerCharacterId, targetObjectId);

        Assert.Equal("seeker", observerCharacterId.Value);
        Assert.Equal("concealed-healing-herb", targetObjectId.Value);
    }

    [Fact]
    public void ExposesOnlyTheImmutableCandidateSurface()
    {
        Type candidateType = typeof(PassiveNoticingCandidate);
        (string Name, Type Type)[] expectedProperties =
        [
            ("ObserverCharacterId", typeof(CharacterId)),
            ("TargetObjectId", typeof(ObjectId)),
        ];

        (string Name, Type Type, bool CanWrite)[] actualProperties = candidateType
            .GetProperties()
            .OrderBy(property => property.Name, StringComparer.Ordinal)
            .Select(property => (property.Name, property.PropertyType, property.CanWrite))
            .ToArray();
        System.Reflection.ConstructorInfo constructor = Assert.Single(
            candidateType.GetConstructors());

        Assert.Equal(
            expectedProperties,
            actualProperties.Select(property => (property.Name, property.Type)).ToArray());
        Assert.All(actualProperties, property => Assert.False(property.CanWrite));
        Assert.Collection(
            constructor.GetParameters(),
            parameter => Assert.Equal(typeof(CharacterId), parameter.ParameterType),
            parameter => Assert.Equal(typeof(ObjectId), parameter.ParameterType));
        Assert.Empty(candidateType.GetEvents());
        Assert.True(candidateType.IsSealed);
    }
}
