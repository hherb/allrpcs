using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterObjectSabotageContaminationDecisionPolicyTests
{
    [Fact]
    public void AcceptsExactAbstentionAndExactEligibleSelection()
    {
        PolicyInput input = Input();
        CharacterObjectSabotageContaminationDecision abstention = new(
            input.Character.Identity.Id,
            null);
        CharacterObjectSabotageContaminationDecision selection = new(
            input.Character.Identity.Id,
            input.Candidate.Evidence.EvidenceId);

        Assert.True(CharacterObjectSabotageContaminationDecisionPolicy.CanAccept(
            input.Request,
            abstention));
        Assert.True(CharacterObjectSabotageContaminationDecisionPolicy.CanAccept(
            input.Request,
            selection));
    }

    [Fact]
    public void RejectsDistinctValueEqualCharacterOrEvidenceIdentifiers()
    {
        PolicyInput input = Input();

        Assert.False(CharacterObjectSabotageContaminationDecisionPolicy.CanAccept(
            input.Request,
            new CharacterObjectSabotageContaminationDecision(
                new CharacterId(input.Character.Identity.Id.Value),
                null)));
        Assert.False(CharacterObjectSabotageContaminationDecisionPolicy.CanAccept(
            input.Request,
            new CharacterObjectSabotageContaminationDecision(
                input.Character.Identity.Id,
                new EvidenceId(input.Candidate.Evidence.EvidenceId.Value))));
    }

    [Fact]
    public void RejectsUnknownOrCurrentlyIneligibleSelection()
    {
        PolicyInput input = Input();
        CharacterObjectSabotageContaminationDecision unknown = new(
            input.Character.Identity.Id,
            new EvidenceId("unknown-evidence"));
        CharacterObjectSabotageContaminationConsiderationRequest inaccessible = Request(
            input,
            new CharacterPhysicalAccessSnapshot(input.Character.Identity.Id, []),
            input.Contamination);
        CharacterObjectSabotageContaminationConsiderationRequest contaminated = Request(
            input,
            input.PhysicalAccess,
            new WorldObjectContaminationSnapshot([input.Target.Identity.Id]));
        CharacterObjectSabotageContaminationDecision selection = new(
            input.Character.Identity.Id,
            input.Candidate.Evidence.EvidenceId);

        Assert.False(CharacterObjectSabotageContaminationDecisionPolicy.CanAccept(
            input.Request,
            unknown));
        Assert.False(CharacterObjectSabotageContaminationDecisionPolicy.CanAccept(
            inaccessible,
            selection));
        Assert.False(CharacterObjectSabotageContaminationDecisionPolicy.CanAccept(
            contaminated,
            selection));
        Assert.Empty(input.Contamination.ContaminatedObjectIds);
        Assert.Empty(input.Evidence.Entries);
    }

    [Fact]
    public void RejectsNullsAndExposesOnlyOnePureAcceptanceQuery()
    {
        PolicyInput input = Input();
        CharacterObjectSabotageContaminationDecision decision = new(
            input.Character.Identity.Id,
            null);

        Assert.Throws<ArgumentNullException>(() =>
            CharacterObjectSabotageContaminationDecisionPolicy.CanAccept(null!, decision));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterObjectSabotageContaminationDecisionPolicy.CanAccept(input.Request, null!));

        Type policyType = typeof(CharacterObjectSabotageContaminationDecisionPolicy);
        System.Reflection.MethodInfo method = Assert.Single(policyType.GetMethods().Where(
            candidate => candidate.DeclaringType == policyType));
        Assert.Equal("CanAccept", method.Name);
        Assert.True(method.IsStatic);
        Assert.Empty(policyType.GetProperties());
        Assert.Empty(policyType.GetConstructors());
        Assert.True(policyType.IsAbstract && policyType.IsSealed);
    }

    private static CharacterObjectSabotageContaminationConsiderationRequest Request(
        PolicyInput input,
        CharacterPhysicalAccessSnapshot physicalAccess,
        WorldObjectContaminationSnapshot contamination) =>
        new(
            input.Character,
            physicalAccess,
            input.WorldObjects,
            contamination,
            input.Evidence,
            [input.Candidate]);

    private static PolicyInput Input()
    {
        Character character = RepresentativeEnemyAgentCharacterSource.Create();
        WorldObject target = new(
            new WorldObjectIdentity(new ObjectId("village-water-cask"), "Water cask"),
            new WorldObjectDescription("A covered cask beside the village well."));
        CharacterPhysicalAccessSnapshot physicalAccess = new(
            character.Identity.Id,
            [target.Identity.Id]);
        WorldObjectSnapshot worldObjects = new([target]);
        WorldObjectContaminationSnapshot contamination = new([]);
        DiscoveryEvidenceSnapshot evidence = new([]);
        CharacterObjectSabotageEvidence candidate = new(
            new CharacterObjectSabotageOccurrence(
                new CharacterObjectSabotageProposal(
                    character.Identity.Id,
                    new CharacterActionAccessRequirement(
                        target.Identity.Id,
                        "assess-local-influence",
                        character.Knowledge.KnowledgeReferences.Single()),
                    CharacterSabotageActionKind.Contaminate),
                target),
            new DiscoveryEvidence(
                new EvidenceId("bitter-cask-residue"),
                target.Identity.Id,
                VisibilityState.Noticed,
                false));
        CharacterObjectSabotageContaminationConsiderationRequest request = new(
            character,
            physicalAccess,
            worldObjects,
            contamination,
            evidence,
            [candidate]);

        return new(
            character,
            target,
            physicalAccess,
            worldObjects,
            contamination,
            evidence,
            candidate,
            request);
    }

    private sealed record PolicyInput(
        Character Character,
        WorldObject Target,
        CharacterPhysicalAccessSnapshot PhysicalAccess,
        WorldObjectSnapshot WorldObjects,
        WorldObjectContaminationSnapshot Contamination,
        DiscoveryEvidenceSnapshot Evidence,
        CharacterObjectSabotageEvidence Candidate,
        CharacterObjectSabotageContaminationConsiderationRequest Request);
}
