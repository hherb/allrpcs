using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Discovery;

public sealed class ObservationReinterpretationCandidateTests
{
    [Fact]
    public void RetainsObserverTargetAndTriggerKnowledgeIdentifierInstances()
    {
        CharacterId observerCharacterId = new("seeker");
        ObjectId targetObjectId = new("weathered-inscription");
        KnowledgeId triggerKnowledgeId = new("ancient-script-key");

        ObservationReinterpretationCandidate candidate = new(
            observerCharacterId,
            targetObjectId,
            triggerKnowledgeId);

        Assert.Same(observerCharacterId, candidate.ObserverCharacterId);
        Assert.Same(targetObjectId, candidate.TargetObjectId);
        Assert.Same(triggerKnowledgeId, candidate.TriggerKnowledgeId);
    }

    [Fact]
    public void EqualityUsesExistingIdentifierValues()
    {
        CharacterId firstObserverCharacterId = new("seeker");
        CharacterId sameObserverCharacterId = new("seeker");
        ObjectId firstTargetObjectId = new("weathered-inscription");
        ObjectId sameTargetObjectId = new("weathered-inscription");
        KnowledgeId firstTriggerKnowledgeId = new("ancient-script-key");
        KnowledgeId sameTriggerKnowledgeId = new("ancient-script-key");

        ObservationReinterpretationCandidate first = new(
            firstObserverCharacterId,
            firstTargetObjectId,
            firstTriggerKnowledgeId);
        ObservationReinterpretationCandidate same = new(
            sameObserverCharacterId,
            sameTargetObjectId,
            sameTriggerKnowledgeId);

        Assert.NotSame(firstObserverCharacterId, sameObserverCharacterId);
        Assert.NotSame(firstTargetObjectId, sameTargetObjectId);
        Assert.NotSame(firstTriggerKnowledgeId, sameTriggerKnowledgeId);
        Assert.Equal(first, same);
        Assert.True(first == same);
        Assert.Same(firstObserverCharacterId, first.ObserverCharacterId);
        Assert.Same(firstTargetObjectId, first.TargetObjectId);
        Assert.Same(firstTriggerKnowledgeId, first.TriggerKnowledgeId);
        Assert.Same(sameObserverCharacterId, same.ObserverCharacterId);
        Assert.Same(sameTargetObjectId, same.TargetObjectId);
        Assert.Same(sameTriggerKnowledgeId, same.TriggerKnowledgeId);
    }

    [Fact]
    public void DistinguishesEachIdentifierValueIndependently()
    {
        ObservationReinterpretationCandidate candidate = CreateCandidate();

        Assert.NotEqual(
            candidate,
            new ObservationReinterpretationCandidate(
                new CharacterId("travelling-healer"),
                new ObjectId("weathered-inscription"),
                new KnowledgeId("ancient-script-key")));
        Assert.NotEqual(
            candidate,
            new ObservationReinterpretationCandidate(
                new CharacterId("seeker"),
                new ObjectId("concealed-healing-herb"),
                new KnowledgeId("ancient-script-key")));
        Assert.NotEqual(
            candidate,
            new ObservationReinterpretationCandidate(
                new CharacterId("seeker"),
                new ObjectId("weathered-inscription"),
                new KnowledgeId("shrine-history")));
    }

    [Fact]
    public void RejectsNullValues()
    {
        Assert.Throws<ArgumentNullException>(
            () => new ObservationReinterpretationCandidate(
                null!,
                new ObjectId("weathered-inscription"),
                new KnowledgeId("ancient-script-key")));
        Assert.Throws<ArgumentNullException>(
            () => new ObservationReinterpretationCandidate(
                new CharacterId("seeker"),
                null!,
                new KnowledgeId("ancient-script-key")));
        Assert.Throws<ArgumentNullException>(
            () => new ObservationReinterpretationCandidate(
                new CharacterId("seeker"),
                new ObjectId("weathered-inscription"),
                null!));
    }

    [Fact]
    public void LeavesIdentifierInputsUnchanged()
    {
        CharacterId observerCharacterId = new("seeker");
        ObjectId targetObjectId = new("weathered-inscription");
        KnowledgeId triggerKnowledgeId = new("ancient-script-key");

        _ = new ObservationReinterpretationCandidate(
            observerCharacterId,
            targetObjectId,
            triggerKnowledgeId);

        Assert.Equal("seeker", observerCharacterId.Value);
        Assert.Equal("weathered-inscription", targetObjectId.Value);
        Assert.Equal("ancient-script-key", triggerKnowledgeId.Value);
    }

    [Fact]
    public void ExposesOnlyTheImmutableCandidateSurface()
    {
        Type candidateType = typeof(ObservationReinterpretationCandidate);
        (string Name, Type Type)[] expectedProperties =
        [
            ("ObserverCharacterId", typeof(CharacterId)),
            ("TargetObjectId", typeof(ObjectId)),
            ("TriggerKnowledgeId", typeof(KnowledgeId)),
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
            parameter => Assert.Equal(typeof(ObjectId), parameter.ParameterType),
            parameter => Assert.Equal(typeof(KnowledgeId), parameter.ParameterType));
        Assert.Empty(candidateType.GetEvents());
        Assert.True(candidateType.IsSealed);
    }

    private static ObservationReinterpretationCandidate CreateCandidate() =>
        new(
            new CharacterId("seeker"),
            new ObjectId("weathered-inscription"),
            new KnowledgeId("ancient-script-key"));
}
