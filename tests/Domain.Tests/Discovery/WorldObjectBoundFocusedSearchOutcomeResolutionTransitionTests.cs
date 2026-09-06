using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Domain.Tests.Discovery;

public sealed class WorldObjectBoundFocusedSearchOutcomeResolutionTransitionTests
{
    [Fact]
    public void RejectsTargetMismatchedWorldObject()
    {
        FocusedSearchOutcome outcome =
            Outcome("seeker", "weathered-inscription", VisibilityState.Understood);
        WorldObject worldObject = CreateWorldObject("wagon-residue", "Wagon residue");
        CharacterPerceptionSnapshot perception = Perception(
            "seeker",
            Visibility("weathered-inscription", VisibilityState.Noticed));

        Assert.Throws<InvalidOperationException>(
            () => WorldObjectBoundFocusedSearchOutcomeResolutionTransition.Resolve(
                outcome,
                worldObject,
                perception,
                Outcomes(outcome)));
    }

    [Fact]
    public void RejectsEveryDelegatedPerceptionIneligibilityCategory()
    {
        FocusedSearchOutcome outcome =
            Outcome("seeker", "weathered-inscription", VisibilityState.Understood);
        WorldObject worldObject =
            CreateWorldObject("weathered-inscription", "Weathered inscription");
        FocusedSearchOutcomeSnapshot outcomes = Outcomes(outcome);
        CharacterPerceptionSnapshot[] ineligiblePerceptions =
        [
            Perception(
                "travelling-healer",
                Visibility("weathered-inscription", VisibilityState.Noticed)),
            Perception(
                "seeker",
                Visibility("wagon-residue", VisibilityState.Noticed)),
            Perception(
                "seeker",
                Visibility("weathered-inscription", VisibilityState.Understood)),
        ];

        foreach (CharacterPerceptionSnapshot perception in ineligiblePerceptions)
        {
            Assert.Throws<InvalidOperationException>(
                () => WorldObjectBoundFocusedSearchOutcomeResolutionTransition.Resolve(
                    outcome,
                    worldObject,
                    perception,
                    outcomes));
        }
    }

    [Fact]
    public void RejectsEveryDelegatedOutcomeConsumptionIneligibilityCategory()
    {
        FocusedSearchRequest request = Request("seeker", "weathered-inscription");
        FocusedSearchOutcome outcome = new(request, VisibilityState.Understood);
        WorldObject worldObject =
            CreateWorldObject("weathered-inscription", "Weathered inscription");
        CharacterPerceptionSnapshot perception = Perception(
            "seeker",
            Visibility("weathered-inscription", VisibilityState.Noticed));
        FocusedSearchOutcomeSnapshot[] ineligibleOutcomes =
        [
            Outcomes(),
            Outcomes(Outcome("seeker", "concealed-healing-herb", VisibilityState.Understood)),
            Outcomes(Outcome(
                "travelling-healer",
                "weathered-inscription",
                VisibilityState.Understood)),
            Outcomes(new FocusedSearchOutcome(request, VisibilityState.Identified)),
        ];

        foreach (FocusedSearchOutcomeSnapshot outcomes in ineligibleOutcomes)
        {
            Assert.Throws<InvalidOperationException>(
                () => WorldObjectBoundFocusedSearchOutcomeResolutionTransition.Resolve(
                    outcome,
                    worldObject,
                    perception,
                    outcomes));
        }
    }

    [Fact]
    public void ResolvesThroughValueEqualWorldObjectWithExactResultRetention()
    {
        CharacterId searcherCharacterId = new("seeker");
        ObjectId outcomeTargetObjectId = new("weathered-inscription");
        FocusedSearchRequest request = new(searcherCharacterId, outcomeTargetObjectId);
        VisibilityState resultingVisibilityState = VisibilityState.Understood;
        FocusedSearchOutcome outcome = new(request, resultingVisibilityState);
        ObjectId worldObjectId = new("weathered-inscription");
        WorldObjectIdentity worldObjectIdentity =
            new(worldObjectId, "Weathered inscription");
        WorldObjectDescription worldObjectDescription = new("Carved stone text.");
        WorldObject worldObject = new(worldObjectIdentity, worldObjectDescription);
        CharacterId perceptionCharacterId = new("seeker");
        WorldObjectVisibility firstVisibility =
            Visibility("wagon-residue", VisibilityState.Identified);
        ObjectId perceptionTargetObjectId = new("weathered-inscription");
        VisibilityState currentVisibilityState = VisibilityState.Noticed;
        WorldObjectVisibility targetVisibility =
            new(perceptionTargetObjectId, currentVisibilityState);
        WorldObjectVisibility lastVisibility =
            Visibility("concealed-healing-herb", VisibilityState.Unnoticed);
        CharacterPerceptionSnapshot perception = new(
            perceptionCharacterId,
            [firstVisibility, targetVisibility, lastVisibility]);
        FocusedSearchOutcome firstOutcome =
            Outcome("sage", "wagon-residue", VisibilityState.Noticed);
        FocusedSearchOutcome retainedOutcome =
            Outcome("seeker", "weathered-inscription", resultingVisibilityState);
        FocusedSearchOutcome lastOutcome = Outcome(
            "travelling-healer",
            "concealed-healing-herb",
            VisibilityState.Identified);
        FocusedSearchOutcomeSnapshot outcomes =
            Outcomes(firstOutcome, retainedOutcome, lastOutcome);

        FocusedSearchOutcomeResolutionResult result =
            WorldObjectBoundFocusedSearchOutcomeResolutionTransition.Resolve(
                outcome,
                worldObject,
                perception,
                outcomes);

        Assert.NotSame(outcomeTargetObjectId, worldObjectId);
        Assert.Equal(outcomeTargetObjectId, worldObjectId);
        Assert.NotSame(perception, result.Perception);
        Assert.Same(perceptionCharacterId, result.Perception.CharacterId);
        Assert.Collection(
            result.Perception.Entries,
            entry => Assert.Same(firstVisibility, entry),
            entry =>
            {
                Assert.NotSame(targetVisibility, entry);
                Assert.Same(perceptionTargetObjectId, entry.ObjectId);
                Assert.Same(resultingVisibilityState, entry.VisibilityState);
            },
            entry => Assert.Same(lastVisibility, entry));
        Assert.NotSame(outcomes, result.Outcomes);
        Assert.Collection(
            result.Outcomes.Entries,
            entry => Assert.Same(firstOutcome, entry),
            entry => Assert.Same(lastOutcome, entry));
        Assert.Same(worldObjectIdentity, worldObject.Identity);
        Assert.Same(worldObjectId, worldObject.Identity.Id);
        Assert.Same(worldObjectDescription, worldObject.Description);
    }

    [Fact]
    public void LeavesEverySuccessfulInputAndRetainedInstanceUnchanged()
    {
        CharacterId searcherCharacterId = new("seeker");
        ObjectId targetObjectId = new("weathered-inscription");
        FocusedSearchRequest request = new(searcherCharacterId, targetObjectId);
        VisibilityState resultingVisibilityState = VisibilityState.Understood;
        FocusedSearchOutcome outcome = new(request, resultingVisibilityState);
        ObjectId worldObjectId = new("weathered-inscription");
        WorldObject worldObject = new(
            new WorldObjectIdentity(worldObjectId, "Weathered inscription"),
            new WorldObjectDescription("Carved stone text."));
        CharacterId perceptionCharacterId = new("seeker");
        WorldObjectVisibility targetVisibility =
            new(targetObjectId, VisibilityState.Noticed);
        CharacterPerceptionSnapshot perception =
            new(perceptionCharacterId, [targetVisibility]);
        FocusedSearchOutcome retainedOutcome =
            Outcome("seeker", "weathered-inscription", resultingVisibilityState);
        FocusedSearchOutcomeSnapshot outcomes = Outcomes(retainedOutcome);

        _ = WorldObjectBoundFocusedSearchOutcomeResolutionTransition.Resolve(
            outcome,
            worldObject,
            perception,
            outcomes);

        Assert.Same(request, outcome.Request);
        Assert.Same(searcherCharacterId, outcome.Request.SearcherCharacterId);
        Assert.Same(targetObjectId, outcome.Request.TargetObjectId);
        Assert.Same(resultingVisibilityState, outcome.ResultingVisibilityState);
        Assert.Same(worldObjectId, worldObject.Identity.Id);
        Assert.Same(perceptionCharacterId, perception.CharacterId);
        Assert.Collection(perception.Entries, entry => Assert.Same(targetVisibility, entry));
        Assert.Same(targetObjectId, targetVisibility.ObjectId);
        Assert.Same(VisibilityState.Noticed, targetVisibility.VisibilityState);
        Assert.Collection(outcomes.Entries, entry => Assert.Same(retainedOutcome, entry));
    }

    [Fact]
    public void LeavesRejectedInputsAndRetainedInstancesUnchanged()
    {
        FocusedSearchOutcome outcome =
            Outcome("seeker", "weathered-inscription", VisibilityState.Understood);
        WorldObject worldObject = CreateWorldObject("wagon-residue", "Wagon residue");
        WorldObjectVisibility targetVisibility =
            Visibility("weathered-inscription", VisibilityState.Noticed);
        CharacterPerceptionSnapshot perception = Perception("seeker", targetVisibility);
        FocusedSearchOutcome retainedOutcome =
            Outcome("seeker", "weathered-inscription", VisibilityState.Understood);
        FocusedSearchOutcomeSnapshot outcomes = Outcomes(retainedOutcome);

        Assert.Throws<InvalidOperationException>(
            () => WorldObjectBoundFocusedSearchOutcomeResolutionTransition.Resolve(
                outcome,
                worldObject,
                perception,
                outcomes));

        Assert.Collection(perception.Entries, entry => Assert.Same(targetVisibility, entry));
        Assert.Collection(outcomes.Entries, entry => Assert.Same(retainedOutcome, entry));
    }

    [Fact]
    public void RejectsNullInputsThroughWorldObjectBoundPolicy()
    {
        FocusedSearchOutcome outcome =
            Outcome("seeker", "weathered-inscription", VisibilityState.Understood);
        WorldObject worldObject =
            CreateWorldObject("weathered-inscription", "Weathered inscription");
        CharacterPerceptionSnapshot perception = Perception(
            "seeker",
            Visibility("weathered-inscription", VisibilityState.Noticed));
        FocusedSearchOutcomeSnapshot outcomes = Outcomes(outcome);

        Assert.Throws<ArgumentNullException>(
            () => WorldObjectBoundFocusedSearchOutcomeResolutionTransition.Resolve(
                null!,
                worldObject,
                perception,
                outcomes));
        Assert.Throws<ArgumentNullException>(
            () => WorldObjectBoundFocusedSearchOutcomeResolutionTransition.Resolve(
                outcome,
                null!,
                perception,
                outcomes));
        Assert.Throws<ArgumentNullException>(
            () => WorldObjectBoundFocusedSearchOutcomeResolutionTransition.Resolve(
                outcome,
                worldObject,
                null!,
                outcomes));
        Assert.Throws<ArgumentNullException>(
            () => WorldObjectBoundFocusedSearchOutcomeResolutionTransition.Resolve(
                outcome,
                worldObject,
                perception,
                null!));
    }

    [Fact]
    public void ExposesOnlyTheSinglePureWorldObjectBoundResolutionOperation()
    {
        Type transitionType =
            typeof(WorldObjectBoundFocusedSearchOutcomeResolutionTransition);
        System.Reflection.MethodInfo method = Assert.Single(
            transitionType
                .GetMethods()
                .Where(candidate => candidate.DeclaringType == transitionType));

        Assert.Equal("Resolve", method.Name);
        Assert.Equal(typeof(FocusedSearchOutcomeResolutionResult), method.ReturnType);
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
        Assert.Empty(transitionType.GetConstructors());
        Assert.Empty(transitionType.GetProperties());
        Assert.Empty(transitionType.GetEvents());
        Assert.True(transitionType.IsAbstract);
        Assert.True(transitionType.IsSealed);
    }

    private static FocusedSearchOutcome Outcome(
        string characterId,
        string targetObjectId,
        VisibilityState resultingVisibilityState) =>
        new(Request(characterId, targetObjectId), resultingVisibilityState);

    private static CharacterPerceptionSnapshot Perception(
        string characterId,
        params WorldObjectVisibility[] entries) =>
        new(new CharacterId(characterId), entries);

    private static FocusedSearchOutcomeSnapshot Outcomes(
        params FocusedSearchOutcome[] entries) =>
        new(entries);

    private static FocusedSearchRequest Request(string characterId, string targetObjectId) =>
        new(new CharacterId(characterId), new ObjectId(targetObjectId));

    private static WorldObjectVisibility Visibility(
        string objectId,
        VisibilityState visibilityState) =>
        new(new ObjectId(objectId), visibilityState);

    private static WorldObject CreateWorldObject(string objectId, string displayName) =>
        new(
            new WorldObjectIdentity(new ObjectId(objectId), displayName),
            new WorldObjectDescription(string.Empty));
}
