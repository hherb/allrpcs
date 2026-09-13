using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterObjectSabotageProposalTests
{
    [Theory]
    [MemberData(nameof(ObjectKinds))]
    public void ProposalRetainsExactActorRequirementAndObjectKind(
        CharacterSabotageActionKind actionKind)
    {
        CharacterId actorId = new("enemy-agent");
        ObjectId targetObjectId = new("village-water-cask");
        KnowledgeId knowledgeId = new("water-cask-contents");
        CharacterActionAccessRequirement requirement = new(
            targetObjectId,
            "assess-local-influence",
            knowledgeId);

        CharacterObjectSabotageProposal proposal = new(actorId, requirement, actionKind);

        Assert.Same(actorId, proposal.ActorId);
        Assert.Same(requirement, proposal.AccessRequirement);
        Assert.Same(actionKind, proposal.ActionKind);
        Assert.All(
            typeof(CharacterObjectSabotageProposal).GetProperties(),
            property => Assert.False(property.CanWrite));
    }

    [Theory]
    [MemberData(nameof(NonObjectKinds))]
    public void ProposalRejectsNonObjectKinds(CharacterSabotageActionKind actionKind)
    {
        Assert.Throws<ArgumentException>(() => new CharacterObjectSabotageProposal(
            new CharacterId("enemy-agent"),
            Requirement(),
            actionKind));
    }

    [Fact]
    public void EligibilityRequiresValueMatchingActorAndCompleteExistingAccess()
    {
        Character character = RepresentativeEnemyAgentCharacterSource.Create();
        ObjectId targetObjectId = new("village-water-cask");
        KnowledgeId knowledgeId = character.Knowledge.KnowledgeReferences.Single();
        CharacterActionAccessRequirement requirement = new(
            targetObjectId,
            "assess-local-influence",
            knowledgeId);
        CharacterPhysicalAccessSnapshot physicalAccess = new(
            new CharacterId(character.Identity.Id.Value),
            [new ObjectId(targetObjectId.Value)]);
        CharacterObjectSabotageProposal proposal = new(
            new CharacterId(character.Identity.Id.Value),
            requirement,
            CharacterSabotageActionKind.Conceal);

        Assert.True(CharacterObjectSabotageEligibilityPolicy.CanPropose(
            character,
            physicalAccess,
            proposal));
        Assert.Same(requirement, proposal.AccessRequirement);
        Assert.Same(targetObjectId, requirement.TargetObjectId);
        Assert.Same(knowledgeId, requirement.RequiredKnowledgeId);
    }

    [Fact]
    public void EligibilityFailsClosedForWrongActorOrMissingAccess()
    {
        Character character = RepresentativeEnemyAgentCharacterSource.Create();
        ObjectId targetObjectId = new("village-water-cask");
        KnowledgeId knowledgeId = character.Knowledge.KnowledgeReferences.Single();
        CharacterPhysicalAccessSnapshot physicalAccess = new(
            character.Identity.Id,
            [targetObjectId]);

        Assert.False(CanPropose(
            character,
            physicalAccess,
            new CharacterId("other-agent"),
            Requirement(targetObjectId, "assess-local-influence", knowledgeId)));
        Assert.False(CanPropose(
            character,
            new CharacterPhysicalAccessSnapshot(character.Identity.Id, []),
            character.Identity.Id,
            Requirement(targetObjectId, "assess-local-influence", knowledgeId)));
        Assert.False(CanPropose(
            character,
            physicalAccess,
            character.Identity.Id,
            Requirement(targetObjectId, "contaminate-supplies", knowledgeId)));
        Assert.False(CanPropose(
            character,
            physicalAccess,
            character.Identity.Id,
            Requirement(targetObjectId, "assess-local-influence", new KnowledgeId("unknown"))));
    }

    [Fact]
    public void ContractsRejectNullsAndPolicyExposesOnlyOnePureOperation()
    {
        Character character = RepresentativeEnemyAgentCharacterSource.Create();
        CharacterPhysicalAccessSnapshot physicalAccess = new(character.Identity.Id, []);
        CharacterObjectSabotageProposal proposal = new(
            character.Identity.Id,
            Requirement(),
            CharacterSabotageActionKind.Move);

        Assert.Throws<ArgumentNullException>(() =>
            new CharacterObjectSabotageProposal(null!, Requirement(), CharacterSabotageActionKind.Move));
        Assert.Throws<ArgumentNullException>(() =>
            new CharacterObjectSabotageProposal(character.Identity.Id, null!, CharacterSabotageActionKind.Move));
        Assert.Throws<ArgumentNullException>(() =>
            new CharacterObjectSabotageProposal(character.Identity.Id, Requirement(), null!));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterObjectSabotageEligibilityPolicy.CanPropose(null!, physicalAccess, proposal));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterObjectSabotageEligibilityPolicy.CanPropose(character, null!, proposal));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterObjectSabotageEligibilityPolicy.CanPropose(character, physicalAccess, null!));

        Type policyType = typeof(CharacterObjectSabotageEligibilityPolicy);
        System.Reflection.MethodInfo method = Assert.Single(policyType.GetMethods().Where(
            candidate => candidate.DeclaringType == policyType));
        Assert.Equal("CanPropose", method.Name);
        Assert.True(method.IsStatic);
        Assert.Empty(policyType.GetConstructors());
        Assert.True(policyType.IsAbstract && policyType.IsSealed);
    }

    public static TheoryData<CharacterSabotageActionKind> ObjectKinds() => new()
    {
        CharacterSabotageActionKind.Conceal,
        CharacterSabotageActionKind.Move,
        CharacterSabotageActionKind.Substitute,
        CharacterSabotageActionKind.Contaminate,
    };

    public static TheoryData<CharacterSabotageActionKind> NonObjectKinds() => new()
    {
        CharacterSabotageActionKind.Misquote,
        CharacterSabotageActionKind.Delay,
        CharacterSabotageActionKind.Influence,
    };

    private static bool CanPropose(
        Character character,
        CharacterPhysicalAccessSnapshot physicalAccess,
        CharacterId actorId,
        CharacterActionAccessRequirement requirement) =>
        CharacterObjectSabotageEligibilityPolicy.CanPropose(
            character,
            physicalAccess,
            new CharacterObjectSabotageProposal(
                actorId,
                requirement,
                CharacterSabotageActionKind.Contaminate));

    private static CharacterActionAccessRequirement Requirement() =>
        Requirement(
            new ObjectId("village-water-cask"),
            "assess-local-influence",
            new KnowledgeId("water-cask-contents"));

    private static CharacterActionAccessRequirement Requirement(
        ObjectId targetObjectId,
        string capabilityKey,
        KnowledgeId knowledgeId) =>
        new(targetObjectId, capabilityKey, knowledgeId);
}
