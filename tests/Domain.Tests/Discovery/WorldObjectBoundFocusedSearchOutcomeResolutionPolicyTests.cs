using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Domain.Tests.Discovery;

public sealed class WorldObjectBoundFocusedSearchOutcomeResolutionPolicyTests
{
    [Fact]
    public void AllowsMatchingValueEqualWorldObjectWhenExistingResolutionIsEligible()
    {
        ObjectId outcomeTargetObjectId = new("weathered-inscription");
        ObjectId worldObjectId = new("weathered-inscription");
        FocusedSearchOutcome outcome = Outcome(
            "seeker",
            outcomeTargetObjectId,
            VisibilityState.Understood);
        WorldObject worldObject = CreateWorldObject(worldObjectId, "Weathered inscription");
        CharacterPerceptionSnapshot perception = Perception(
            "seeker",
            Visibility("weathered-inscription", VisibilityState.Noticed));

        bool result = WorldObjectBoundFocusedSearchOutcomeResolutionPolicy.CanResolve(
            outcome,
            worldObject,
            perception,
            Outcomes(outcome));

        Assert.NotSame(outcomeTargetObjectId, worldObjectId);
        Assert.Equal(outcomeTargetObjectId, worldObjectId);
        Assert.True(result);
    }

    [Fact]
    public void RejectsTargetMismatchedWorldObject()
    {
        FocusedSearchOutcome outcome = Outcome(
            "seeker",
            new ObjectId("weathered-inscription"),
            VisibilityState.Understood);
        WorldObject worldObject = CreateWorldObject(
            new ObjectId("wagon-residue"),
            "Wagon residue");
        CharacterPerceptionSnapshot perception = Perception(
            "seeker",
            Visibility("weathered-inscription", VisibilityState.Noticed));

        bool result = WorldObjectBoundFocusedSearchOutcomeResolutionPolicy.CanResolve(
            outcome,
            worldObject,
            perception,
            Outcomes(outcome));

        Assert.False(result);
    }

    [Theory]
    [InlineData("travelling-healer", "weathered-inscription", true, false)]
    [InlineData("seeker", "wagon-residue", true, false)]
    [InlineData("seeker", "weathered-inscription", true, true)]
    [InlineData("seeker", "weathered-inscription", false, false)]
    public void DelegatesExistingResolutionEligibilityUnchanged(
        string perceptionCharacterId,
        string perceptionObjectId,
        bool retainOutcome,
        bool visibilityIsUnchanged)
    {
        ObjectId targetObjectId = new("weathered-inscription");
        FocusedSearchOutcome outcome = Outcome(
            "seeker",
            targetObjectId,
            VisibilityState.Understood);
        WorldObject worldObject = CreateWorldObject(
            new ObjectId(targetObjectId.Value),
            "Weathered inscription");
        CharacterPerceptionSnapshot perception = Perception(
            perceptionCharacterId,
            Visibility(
                perceptionObjectId,
                visibilityIsUnchanged
                    ? VisibilityState.Understood
                    : VisibilityState.Noticed));
        FocusedSearchOutcomeSnapshot outcomes =
            retainOutcome ? Outcomes(outcome) : Outcomes();

        bool existingResult = FocusedSearchOutcomeResolutionPolicy.CanResolve(
            outcome,
            perception,
            outcomes);
        bool result = WorldObjectBoundFocusedSearchOutcomeResolutionPolicy.CanResolve(
            outcome,
            worldObject,
            perception,
            outcomes);

        Assert.False(existingResult);
        Assert.Equal(existingResult, result);
    }

    [Fact]
    public void LeavesEveryInputAndRetainedInstanceUnchanged()
    {
        CharacterId searcherCharacterId = new("seeker");
        ObjectId targetObjectId = new("weathered-inscription");
        FocusedSearchRequest request = new(searcherCharacterId, targetObjectId);
        VisibilityState resultingVisibilityState = VisibilityState.Understood;
        FocusedSearchOutcome outcome = new(request, resultingVisibilityState);
        ObjectId worldObjectId = new(targetObjectId.Value);
        WorldObjectIdentity worldObjectIdentity =
            new(worldObjectId, "Weathered inscription");
        WorldObjectDescription worldObjectDescription = new("Carved stone text.");
        WorldObject worldObject = new(worldObjectIdentity, worldObjectDescription);
        CharacterId perceptionCharacterId = new("seeker");
        WorldObjectVisibility unrelatedVisibility =
            Visibility("wagon-residue", VisibilityState.Identified);
        VisibilityState currentVisibilityState = VisibilityState.Noticed;
        WorldObjectVisibility targetVisibility =
            new(targetObjectId, currentVisibilityState);
        CharacterPerceptionSnapshot perception = new(
            perceptionCharacterId,
            [unrelatedVisibility, targetVisibility]);
        FocusedSearchOutcome unrelatedOutcome = Outcome(
            "sage",
            new ObjectId("concealed-healing-herb"),
            VisibilityState.Noticed);
        FocusedSearchOutcome retainedOutcome = Outcome(
            "seeker",
            new ObjectId(targetObjectId.Value),
            resultingVisibilityState);
        FocusedSearchOutcomeSnapshot outcomes =
            Outcomes(unrelatedOutcome, retainedOutcome);

        _ = WorldObjectBoundFocusedSearchOutcomeResolutionPolicy.CanResolve(
            outcome,
            worldObject,
            perception,
            outcomes);

        Assert.Same(request, outcome.Request);
        Assert.Same(searcherCharacterId, outcome.Request.SearcherCharacterId);
        Assert.Same(targetObjectId, outcome.Request.TargetObjectId);
        Assert.Same(resultingVisibilityState, outcome.ResultingVisibilityState);
        Assert.Same(worldObjectIdentity, worldObject.Identity);
        Assert.Same(worldObjectId, worldObject.Identity.Id);
        Assert.Same(worldObjectDescription, worldObject.Description);
        Assert.Same(perceptionCharacterId, perception.CharacterId);
        Assert.Collection(
            perception.Entries,
            entry => Assert.Same(unrelatedVisibility, entry),
            entry => Assert.Same(targetVisibility, entry));
        Assert.Same(targetObjectId, targetVisibility.ObjectId);
        Assert.Same(currentVisibilityState, targetVisibility.VisibilityState);
        Assert.Collection(
            outcomes.Entries,
            entry => Assert.Same(unrelatedOutcome, entry),
            entry => Assert.Same(retainedOutcome, entry));
    }

    [Fact]
    public void RejectsNullInputsBeforeDelegation()
    {
        FocusedSearchOutcome outcome = Outcome(
            "seeker",
            new ObjectId("weathered-inscription"),
            VisibilityState.Understood);
        WorldObject worldObject = CreateWorldObject(
            new ObjectId("weathered-inscription"),
            "Weathered inscription");
        CharacterPerceptionSnapshot perception = Perception(
            "seeker",
            Visibility("weathered-inscription", VisibilityState.Noticed));
        FocusedSearchOutcomeSnapshot outcomes = Outcomes(outcome);

        Assert.Throws<ArgumentNullException>(
            () => WorldObjectBoundFocusedSearchOutcomeResolutionPolicy.CanResolve(
                null!,
                worldObject,
                perception,
                outcomes));
        Assert.Throws<ArgumentNullException>(
            () => WorldObjectBoundFocusedSearchOutcomeResolutionPolicy.CanResolve(
                outcome,
                null!,
                perception,
                outcomes));
        Assert.Throws<ArgumentNullException>(
            () => WorldObjectBoundFocusedSearchOutcomeResolutionPolicy.CanResolve(
                outcome,
                worldObject,
                null!,
                outcomes));
        Assert.Throws<ArgumentNullException>(
            () => WorldObjectBoundFocusedSearchOutcomeResolutionPolicy.CanResolve(
                outcome,
                worldObject,
                perception,
                null!));
    }

    [Fact]
    public void ExposesOnlyTheSinglePureWorldObjectBoundResolutionOperation()
    {
        Type policyType = typeof(WorldObjectBoundFocusedSearchOutcomeResolutionPolicy);
        System.Reflection.MethodInfo method = Assert.Single(
            policyType
                .GetMethods()
                .Where(candidate => candidate.DeclaringType == policyType));

        Assert.Equal("CanResolve", method.Name);
        Assert.Equal(typeof(bool), method.ReturnType);
        Assert.True(method.IsStatic);
        Assert.Collection(
            method.GetParameters(),
            parameter => Assert.Equal(typeof(FocusedSearchOutcome), parameter.ParameterType),
            parameter => Assert.Equal(typeof(WorldObject), parameter.ParameterType),
            parameter => Assert.Equal(
                typeof(CharacterPerceptionSnapshot),
                parameter.ParameterType),
            parameter => Assert.Equal(
                typeof(FocusedSearchOutcomeSnapshot),
                parameter.ParameterType));
        Assert.Empty(policyType.GetConstructors());
        Assert.Empty(policyType.GetProperties());
        Assert.Empty(policyType.GetEvents());
        Assert.True(policyType.IsAbstract);
        Assert.True(policyType.IsSealed);
    }

    private static FocusedSearchOutcome Outcome(
        string characterId,
        ObjectId targetObjectId,
        VisibilityState resultingVisibilityState) =>
        new(
            new FocusedSearchRequest(new CharacterId(characterId), targetObjectId),
            resultingVisibilityState);

    private static CharacterPerceptionSnapshot Perception(
        string characterId,
        params WorldObjectVisibility[] entries) =>
        new(new CharacterId(characterId), entries);

    private static FocusedSearchOutcomeSnapshot Outcomes(
        params FocusedSearchOutcome[] entries) =>
        new(entries);

    private static WorldObjectVisibility Visibility(
        string objectId,
        VisibilityState visibilityState) =>
        new(new ObjectId(objectId), visibilityState);

    private static WorldObject CreateWorldObject(ObjectId objectId, string displayName) =>
        new(
            new WorldObjectIdentity(objectId, displayName),
            new WorldObjectDescription(string.Empty));
}
