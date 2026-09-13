using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterObjectSabotageOccurrenceTests
{
    [Fact]
    public void CandidateRetainsExactProposalAndTarget()
    {
        CharacterObjectSabotageProposal proposal = Proposal();
        WorldObject target = Target();

        CharacterObjectSabotageOccurrence candidate = new(proposal, target);

        Assert.Same(proposal, candidate.Proposal);
        Assert.Same(target, candidate.Target);
        Assert.All(
            typeof(CharacterObjectSabotageOccurrence).GetProperties(),
            property => Assert.False(property.CanWrite));
    }

    [Fact]
    public void PolicyAllowsEligibleProposalForExactAuthoritativeTarget()
    {
        Character character = RepresentativeEnemyAgentCharacterSource.Create();
        WorldObject target = Target();
        CharacterObjectSabotageOccurrence candidate = new(
            Proposal(
                new CharacterId(character.Identity.Id.Value),
                new ObjectId(target.Identity.Id.Value),
                character.Knowledge.KnowledgeReferences.Single()),
            target);
        CharacterPhysicalAccessSnapshot physicalAccess = new(
            new CharacterId(character.Identity.Id.Value),
            [new ObjectId(target.Identity.Id.Value)]);

        Assert.True(CharacterObjectSabotageOccurrencePolicy.CanOccur(
            character,
            physicalAccess,
            new WorldObjectSnapshot([target]),
            candidate));
    }

    [Fact]
    public void PolicyRejectsMismatchedOrNonAuthoritativeTarget()
    {
        Character character = RepresentativeEnemyAgentCharacterSource.Create();
        WorldObject target = Target();
        CharacterPhysicalAccessSnapshot physicalAccess = new(
            character.Identity.Id,
            [target.Identity.Id]);
        KnowledgeId knowledgeId = character.Knowledge.KnowledgeReferences.Single();

        Assert.False(CanOccur(
            character,
            physicalAccess,
            new WorldObjectSnapshot([target]),
            new CharacterObjectSabotageOccurrence(
                Proposal(character.Identity.Id, new ObjectId("other-object"), knowledgeId),
                target)));
        Assert.False(CanOccur(
            character,
            physicalAccess,
            new WorldObjectSnapshot([]),
            new CharacterObjectSabotageOccurrence(
                Proposal(character.Identity.Id, target.Identity.Id, knowledgeId),
                target)));

        WorldObject valueEqualTarget = Target();
        Assert.Equal(target.Identity, valueEqualTarget.Identity);
        Assert.False(CanOccur(
            character,
            physicalAccess,
            new WorldObjectSnapshot([valueEqualTarget]),
            new CharacterObjectSabotageOccurrence(
                Proposal(character.Identity.Id, target.Identity.Id, knowledgeId),
                target)));
    }

    [Fact]
    public void PolicyDelegatesActorAndCompleteAccessEligibility()
    {
        Character character = RepresentativeEnemyAgentCharacterSource.Create();
        WorldObject target = Target();
        WorldObjectSnapshot worldObjects = new([target]);
        CharacterPhysicalAccessSnapshot physicalAccess = new(
            character.Identity.Id,
            [target.Identity.Id]);
        KnowledgeId knowledgeId = character.Knowledge.KnowledgeReferences.Single();

        Assert.False(CanOccur(
            character,
            physicalAccess,
            worldObjects,
            Candidate(new CharacterId("other-agent"), target, knowledgeId)));
        Assert.False(CanOccur(
            character,
            new CharacterPhysicalAccessSnapshot(character.Identity.Id, []),
            worldObjects,
            Candidate(character.Identity.Id, target, knowledgeId)));
        Assert.False(CanOccur(
            character,
            physicalAccess,
            worldObjects,
            Candidate(character.Identity.Id, target, knowledgeId, "contaminate-supplies")));
        Assert.False(CanOccur(
            character,
            physicalAccess,
            worldObjects,
            Candidate(character.Identity.Id, target, new KnowledgeId("unknown"))));
    }

    [Fact]
    public void ContractsRejectNullsAndPolicyExposesOnlyOnePureOperation()
    {
        Character character = RepresentativeEnemyAgentCharacterSource.Create();
        WorldObject target = Target();
        CharacterObjectSabotageOccurrence candidate = new(Proposal(), target);
        CharacterPhysicalAccessSnapshot physicalAccess = new(character.Identity.Id, []);
        WorldObjectSnapshot worldObjects = new([target]);

        Assert.Throws<ArgumentNullException>(() =>
            new CharacterObjectSabotageOccurrence(null!, target));
        Assert.Throws<ArgumentNullException>(() =>
            new CharacterObjectSabotageOccurrence(Proposal(), null!));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterObjectSabotageOccurrencePolicy.CanOccur(
                null!, physicalAccess, worldObjects, candidate));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterObjectSabotageOccurrencePolicy.CanOccur(
                character, null!, worldObjects, candidate));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterObjectSabotageOccurrencePolicy.CanOccur(
                character, physicalAccess, null!, candidate));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterObjectSabotageOccurrencePolicy.CanOccur(
                character, physicalAccess, worldObjects, null!));

        Type policyType = typeof(CharacterObjectSabotageOccurrencePolicy);
        System.Reflection.MethodInfo method = Assert.Single(policyType.GetMethods().Where(
            candidateMethod => candidateMethod.DeclaringType == policyType));
        Assert.Equal("CanOccur", method.Name);
        Assert.True(method.IsStatic);
        Assert.Empty(policyType.GetConstructors());
        Assert.True(policyType.IsAbstract && policyType.IsSealed);
    }

    private static bool CanOccur(
        Character character,
        CharacterPhysicalAccessSnapshot physicalAccess,
        WorldObjectSnapshot worldObjects,
        CharacterObjectSabotageOccurrence candidate) =>
        CharacterObjectSabotageOccurrencePolicy.CanOccur(
            character,
            physicalAccess,
            worldObjects,
            candidate);

    private static CharacterObjectSabotageOccurrence Candidate(
        CharacterId actorId,
        WorldObject target,
        KnowledgeId knowledgeId,
        string capabilityKey = "assess-local-influence") =>
        new(Proposal(actorId, target.Identity.Id, knowledgeId, capabilityKey), target);

    private static CharacterObjectSabotageProposal Proposal() =>
        Proposal(
            new CharacterId("enemy-agent"),
            new ObjectId("village-water-cask"),
            new KnowledgeId("water-cask-contents"));

    private static CharacterObjectSabotageProposal Proposal(
        CharacterId actorId,
        ObjectId targetObjectId,
        KnowledgeId knowledgeId,
        string capabilityKey = "assess-local-influence") =>
        new(
            actorId,
            new CharacterActionAccessRequirement(targetObjectId, capabilityKey, knowledgeId),
            CharacterSabotageActionKind.Conceal);

    private static WorldObject Target() =>
        new(
            new WorldObjectIdentity(new ObjectId("village-water-cask"), "Village water cask"),
            new WorldObjectDescription("A covered cask beside the village well."));
}
