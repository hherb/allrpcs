using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterObjectSabotageConcealmentDecisionPolicyTests
{
    [Fact]
    public void AcceptsExactAbstentionAndExactEligibleSelection()
    {
        PolicyInput input = Input();
        CharacterObjectSabotageConcealmentDecision abstention = new(input.Character.Identity.Id, null);
        CharacterObjectSabotageConcealmentDecision selection = new(
            input.Character.Identity.Id,
            input.Candidate.Evidence.EvidenceId);

        Assert.True(CharacterObjectSabotageConcealmentDecisionPolicy.CanAccept(input.Request, abstention));
        Assert.True(CharacterObjectSabotageConcealmentDecisionPolicy.CanAccept(input.Request, selection));
    }

    [Fact]
    public void RejectsDistinctValueEqualCharacterOrEvidenceIdentifiers()
    {
        PolicyInput input = Input();

        Assert.False(CharacterObjectSabotageConcealmentDecisionPolicy.CanAccept(
            input.Request,
            new CharacterObjectSabotageConcealmentDecision(
                new CharacterId(input.Character.Identity.Id.Value),
                null)));
        Assert.False(CharacterObjectSabotageConcealmentDecisionPolicy.CanAccept(
            input.Request,
            new CharacterObjectSabotageConcealmentDecision(
                input.Character.Identity.Id,
                new EvidenceId(input.Candidate.Evidence.EvidenceId.Value))));
    }

    [Fact]
    public void RejectsUnknownOrCurrentlyIneligibleSelection()
    {
        PolicyInput input = Input();
        CharacterObjectSabotageConcealmentDecision unknown = new(
            input.Character.Identity.Id,
            new EvidenceId("unknown-evidence"));
        CharacterObjectSabotageConcealmentConsiderationRequest inaccessible = new(
            input.Character,
            new CharacterPhysicalAccessSnapshot(input.Character.Identity.Id, []),
            input.WorldObjects,
            input.Concealment,
            input.Evidence,
            [input.Candidate]);
        CharacterObjectSabotageConcealmentConsiderationRequest concealed = new(
            input.Character,
            input.PhysicalAccess,
            input.WorldObjects,
            new WorldObjectConcealmentSnapshot([input.Target.Identity.Id]),
            input.Evidence,
            [input.Candidate]);
        CharacterObjectSabotageConcealmentDecision selection = new(
            input.Character.Identity.Id,
            input.Candidate.Evidence.EvidenceId);

        Assert.False(CharacterObjectSabotageConcealmentDecisionPolicy.CanAccept(input.Request, unknown));
        Assert.False(CharacterObjectSabotageConcealmentDecisionPolicy.CanAccept(inaccessible, selection));
        Assert.False(CharacterObjectSabotageConcealmentDecisionPolicy.CanAccept(concealed, selection));
    }

    [Fact]
    public void RejectsNullsAndExposesOnlyOnePureAcceptanceQuery()
    {
        PolicyInput input = Input();
        CharacterObjectSabotageConcealmentDecision decision = new(input.Character.Identity.Id, null);

        Assert.Throws<ArgumentNullException>(() =>
            CharacterObjectSabotageConcealmentDecisionPolicy.CanAccept(null!, decision));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterObjectSabotageConcealmentDecisionPolicy.CanAccept(input.Request, null!));

        Type policyType = typeof(CharacterObjectSabotageConcealmentDecisionPolicy);
        System.Reflection.MethodInfo method = Assert.Single(policyType.GetMethods().Where(
            candidate => candidate.DeclaringType == policyType));
        Assert.Equal("CanAccept", method.Name);
        Assert.True(method.IsStatic);
        Assert.Empty(policyType.GetProperties());
        Assert.Empty(policyType.GetConstructors());
        Assert.True(policyType.IsAbstract && policyType.IsSealed);
    }

    private static PolicyInput Input()
    {
        Character character = RepresentativeEnemyAgentCharacterSource.Create();
        WorldObject target = new(
            new WorldObjectIdentity(new ObjectId("village-water-cask"), "Water cask"),
            new WorldObjectDescription("A covered cask beside the village well."));
        CharacterPhysicalAccessSnapshot access = new(
            character.Identity.Id,
            [target.Identity.Id]);
        WorldObjectSnapshot worldObjects = new([target]);
        WorldObjectConcealmentSnapshot concealment = new([]);
        DiscoveryEvidenceSnapshot evidence = new([]);
        CharacterObjectSabotageEvidence candidate = new(
            new CharacterObjectSabotageOccurrence(
                new CharacterObjectSabotageProposal(
                    character.Identity.Id,
                    new CharacterActionAccessRequirement(
                        target.Identity.Id,
                        "assess-local-influence",
                        character.Knowledge.KnowledgeReferences.Single()),
                    CharacterSabotageActionKind.Conceal),
                target),
            new DiscoveryEvidence(
                new EvidenceId("disturbed-cask-cover"),
                target.Identity.Id,
                VisibilityState.Noticed,
                false));
        CharacterObjectSabotageConcealmentConsiderationRequest request = new(
            character,
            access,
            worldObjects,
            concealment,
            evidence,
            [candidate]);

        return new PolicyInput(
            character,
            target,
            access,
            worldObjects,
            concealment,
            evidence,
            candidate,
            request);
    }

    private sealed record PolicyInput(
        Character Character,
        WorldObject Target,
        CharacterPhysicalAccessSnapshot PhysicalAccess,
        WorldObjectSnapshot WorldObjects,
        WorldObjectConcealmentSnapshot Concealment,
        DiscoveryEvidenceSnapshot Evidence,
        CharacterObjectSabotageEvidence Candidate,
        CharacterObjectSabotageConcealmentConsiderationRequest Request);
}
