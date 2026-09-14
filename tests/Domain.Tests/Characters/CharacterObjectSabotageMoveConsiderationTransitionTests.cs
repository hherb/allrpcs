using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Locations;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterObjectSabotageMoveConsiderationTransitionTests
{
    [Fact]
    public void AppliesExactAcceptedSelectionThroughExistingTransition()
    {
        TransitionInput input = Input();
        CharacterObjectSabotageMoveConsiderationResult considerationResult = new(
            input.Request,
            new CharacterObjectSabotageMoveDecision(
                input.Character.Identity.Id,
                input.Candidate.Evidence.Evidence.EvidenceId));

        CharacterObjectSabotageMoveResult? result =
            CharacterObjectSabotageMoveConsiderationTransition.Apply(considerationResult);

        Assert.NotNull(result);
        Assert.Same(input.Candidate.Evidence, result.EvidenceResult.Candidate);
        Assert.Same(input.Candidate.Destination.Destination, Assert.Single(result.Placements.Entries).Location);
        Assert.Same(input.Candidate.Evidence.Evidence, Assert.Single(result.EvidenceResult.Evidence.Entries));
    }

    [Fact]
    public void AcceptedAbstentionReturnsNoApplicationAndLeavesInputsUnchanged()
    {
        TransitionInput input = Input();
        CharacterObjectSabotageMoveConsiderationResult considerationResult = new(
            input.Request,
            new CharacterObjectSabotageMoveDecision(input.Character.Identity.Id, null));

        CharacterObjectSabotageMoveResult? result =
            CharacterObjectSabotageMoveConsiderationTransition.Apply(considerationResult);

        Assert.Null(result);
        Assert.Same(input.SourcePlacement, Assert.Single(input.Request.Placements.Entries));
        Assert.Empty(input.Request.Evidence.Entries);
        Assert.Same(input.Candidate, Assert.Single(input.Request.Candidates));
        Assert.Null(considerationResult.SelectedCandidate);
    }

    [Fact]
    public void CannotConstructStaleSelectionForApplicationAndLeavesInputsUnchanged()
    {
        TransitionInput input = Input();
        WorldObjectPlacement atDestination = new(input.Target, input.Candidate.Destination.Destination);
        CharacterObjectSabotageMoveConsiderationRequest staleRequest = new(
            input.Character,
            input.Request.PhysicalAccess,
            input.Request.WorldObjects,
            new WorldObjectPlacementSnapshot([atDestination]),
            input.Request.Evidence,
            [input.Candidate]);
        CharacterObjectSabotageMoveDecision decision = new(
            input.Character.Identity.Id,
            input.Candidate.Evidence.Evidence.EvidenceId);

        Assert.Throws<ArgumentException>(() =>
            new CharacterObjectSabotageMoveConsiderationResult(staleRequest, decision));
        Assert.Same(input.SourcePlacement, Assert.Single(input.Request.Placements.Entries));
        Assert.Empty(input.Request.Evidence.Entries);
    }

    [Fact]
    public void RejectsNullAndExposesOnlyOnePureApplicationOperation()
    {
        Assert.Throws<ArgumentNullException>(() =>
            CharacterObjectSabotageMoveConsiderationTransition.Apply(null!));

        Type transitionType = typeof(CharacterObjectSabotageMoveConsiderationTransition);
        System.Reflection.MethodInfo method = Assert.Single(transitionType.GetMethods().Where(
            candidate => candidate.DeclaringType == transitionType));

        Assert.Equal("Apply", method.Name);
        Assert.True(method.IsStatic);
        Assert.Equal(typeof(CharacterObjectSabotageMoveResult), method.ReturnType);
        Assert.Equal(
            [typeof(CharacterObjectSabotageMoveConsiderationResult)],
            method.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Empty(transitionType.GetProperties());
        Assert.Empty(transitionType.GetConstructors());
        Assert.True(transitionType.IsAbstract && transitionType.IsSealed);
    }

    private static TransitionInput Input()
    {
        Character character = RepresentativeEnemyAgentCharacterSource.Create();
        WorldObject target = new(
            new WorldObjectIdentity(new ObjectId("village-water-cask"), "Water cask"),
            new WorldObjectDescription("A covered cask beside the village well."));
        Location source = Location("village-square");
        Location destination = Location("abandoned-wagon");
        CharacterObjectSabotageOccurrence occurrence = new(
            new CharacterObjectSabotageProposal(
                character.Identity.Id,
                new CharacterActionAccessRequirement(
                    target.Identity.Id,
                    "assess-local-influence",
                    character.Knowledge.KnowledgeReferences.Single()),
                CharacterSabotageActionKind.Move),
            target);
        CharacterObjectSabotageMoveCandidate candidate = new(
            new CharacterObjectSabotageMoveDestination(occurrence, destination),
            new CharacterObjectSabotageEvidence(
                occurrence,
                new DiscoveryEvidence(
                    new EvidenceId("cask-drag-marks"),
                    target.Identity.Id,
                    VisibilityState.Noticed,
                    false)));
        CharacterPhysicalAccessSnapshot access = new(character.Identity.Id, [target.Identity.Id]);
        WorldObjectSnapshot worldObjects = new([target]);
        WorldObjectPlacement sourcePlacement = new(target, source);
        WorldObjectPlacementSnapshot placements = new([sourcePlacement]);
        DiscoveryEvidenceSnapshot evidence = new([]);
        CharacterObjectSabotageMoveConsiderationRequest request = new(
            character,
            access,
            worldObjects,
            placements,
            evidence,
            [candidate]);

        return new(character, target, sourcePlacement, candidate, request);
    }

    private static Location Location(string id) =>
        new(
            new LocationIdentity(new LocationId(id), id),
            new LocationDescription($"The authored {id} location."));

    private sealed record TransitionInput(
        Character Character,
        WorldObject Target,
        WorldObjectPlacement SourcePlacement,
        CharacterObjectSabotageMoveCandidate Candidate,
        CharacterObjectSabotageMoveConsiderationRequest Request);
}
