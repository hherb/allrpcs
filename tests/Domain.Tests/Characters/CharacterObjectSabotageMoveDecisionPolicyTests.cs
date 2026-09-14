using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Locations;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterObjectSabotageMoveDecisionPolicyTests
{
    [Fact]
    public void AcceptsExactAbstentionAndExactEligibleSelection()
    {
        PolicyInput input = Input();
        CharacterObjectSabotageMoveDecision abstention = new(input.Character.Identity.Id, null);
        CharacterObjectSabotageMoveDecision selection = new(
            input.Character.Identity.Id,
            input.Candidate.Evidence.Evidence.EvidenceId);

        Assert.True(CharacterObjectSabotageMoveDecisionPolicy.CanAccept(input.Request, abstention));
        Assert.True(CharacterObjectSabotageMoveDecisionPolicy.CanAccept(input.Request, selection));
    }

    [Fact]
    public void RejectsDistinctValueEqualCharacterOrEvidenceIdentifiers()
    {
        PolicyInput input = Input();

        Assert.False(CharacterObjectSabotageMoveDecisionPolicy.CanAccept(
            input.Request,
            new CharacterObjectSabotageMoveDecision(
                new CharacterId(input.Character.Identity.Id.Value),
                null)));
        Assert.False(CharacterObjectSabotageMoveDecisionPolicy.CanAccept(
            input.Request,
            new CharacterObjectSabotageMoveDecision(
                input.Character.Identity.Id,
                new EvidenceId(input.Candidate.Evidence.Evidence.EvidenceId.Value))));
    }

    [Fact]
    public void RejectsUnknownOrCurrentlyIneligibleSelection()
    {
        PolicyInput input = Input();
        CharacterObjectSabotageMoveDecision unknown = new(
            input.Character.Identity.Id,
            new EvidenceId("unknown-evidence"));
        CharacterObjectSabotageMoveConsiderationRequest inaccessible = Request(
            input,
            new CharacterPhysicalAccessSnapshot(input.Character.Identity.Id, []),
            input.Placements);
        CharacterObjectSabotageMoveConsiderationRequest alreadyMoved = Request(
            input,
            input.PhysicalAccess,
            new WorldObjectPlacementSnapshot(
                [new WorldObjectPlacement(input.Target, input.Destination)]));
        CharacterObjectSabotageMoveDecision selection = new(
            input.Character.Identity.Id,
            input.Candidate.Evidence.Evidence.EvidenceId);

        Assert.False(CharacterObjectSabotageMoveDecisionPolicy.CanAccept(input.Request, unknown));
        Assert.False(CharacterObjectSabotageMoveDecisionPolicy.CanAccept(inaccessible, selection));
        Assert.False(CharacterObjectSabotageMoveDecisionPolicy.CanAccept(alreadyMoved, selection));
    }

    [Fact]
    public void RejectsNullsAndExposesOnlyOnePureAcceptanceQuery()
    {
        PolicyInput input = Input();
        CharacterObjectSabotageMoveDecision decision = new(input.Character.Identity.Id, null);

        Assert.Throws<ArgumentNullException>(() =>
            CharacterObjectSabotageMoveDecisionPolicy.CanAccept(null!, decision));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterObjectSabotageMoveDecisionPolicy.CanAccept(input.Request, null!));

        Type policyType = typeof(CharacterObjectSabotageMoveDecisionPolicy);
        System.Reflection.MethodInfo method = Assert.Single(policyType.GetMethods().Where(
            candidate => candidate.DeclaringType == policyType));
        Assert.Equal("CanAccept", method.Name);
        Assert.True(method.IsStatic);
        Assert.Empty(policyType.GetProperties());
        Assert.Empty(policyType.GetConstructors());
        Assert.True(policyType.IsAbstract && policyType.IsSealed);
    }

    private static CharacterObjectSabotageMoveConsiderationRequest Request(
        PolicyInput input,
        CharacterPhysicalAccessSnapshot access,
        WorldObjectPlacementSnapshot placements) =>
        new(
            input.Character,
            access,
            input.WorldObjects,
            placements,
            input.Evidence,
            [input.Candidate]);

    private static PolicyInput Input()
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
        CharacterPhysicalAccessSnapshot access = new(
            character.Identity.Id,
            [target.Identity.Id]);
        WorldObjectSnapshot worldObjects = new([target]);
        WorldObjectPlacementSnapshot placements = new(
            [new WorldObjectPlacement(target, source)]);
        DiscoveryEvidenceSnapshot evidence = new([]);
        CharacterObjectSabotageMoveConsiderationRequest request = new(
            character,
            access,
            worldObjects,
            placements,
            evidence,
            [candidate]);

        return new PolicyInput(
            character,
            target,
            destination,
            access,
            worldObjects,
            placements,
            evidence,
            candidate,
            request);
    }

    private static Location Location(string id) =>
        new(
            new LocationIdentity(new LocationId(id), id),
            new LocationDescription($"The authored {id} location."));

    private sealed record PolicyInput(
        Character Character,
        WorldObject Target,
        Location Destination,
        CharacterPhysicalAccessSnapshot PhysicalAccess,
        WorldObjectSnapshot WorldObjects,
        WorldObjectPlacementSnapshot Placements,
        DiscoveryEvidenceSnapshot Evidence,
        CharacterObjectSabotageMoveCandidate Candidate,
        CharacterObjectSabotageMoveConsiderationRequest Request);
}
