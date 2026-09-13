using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterActionAccessPolicyTests
{
    [Fact]
    public void RequirementRetainsExactTargetCapabilityAndKnowledge()
    {
        ObjectId targetObjectId = new("village-water-cask");
        KnowledgeId knowledgeId = new("water-cask-contents");

        CharacterActionAccessRequirement requirement = new(
            targetObjectId,
            "assess-local-influence",
            knowledgeId);

        Assert.Same(targetObjectId, requirement.TargetObjectId);
        Assert.Equal("assess-local-influence", requirement.RequiredCapabilityKey);
        Assert.Same(knowledgeId, requirement.RequiredKnowledgeId);
        Assert.All(
            typeof(CharacterActionAccessRequirement).GetProperties(),
            property => Assert.False(property.CanWrite));
    }

    [Fact]
    public void PolicyRequiresPhysicalCapabilityAndInformationalAccessTogether()
    {
        Character character = RepresentativeEnemyAgentCharacterSource.Create();
        ObjectId targetObjectId = new("village-water-cask");
        CharacterPhysicalAccessSnapshot physicalAccess = new(
            character.Identity.Id,
            [targetObjectId]);
        CharacterActionAccessRequirement requirement = new(
            targetObjectId,
            "assess-local-influence",
            character.Knowledge.KnowledgeReferences.Single());

        Assert.True(CharacterActionAccessPolicy.IsSatisfied(
            character,
            physicalAccess,
            requirement));
    }

    [Fact]
    public void PolicyFailsClosedWhenAnyRequiredAccessIsMissing()
    {
        Character character = RepresentativeEnemyAgentCharacterSource.Create();
        ObjectId targetObjectId = new("village-water-cask");
        CharacterPhysicalAccessSnapshot physicalAccess = new(
            character.Identity.Id,
            [targetObjectId]);
        KnowledgeId knowledgeId = character.Knowledge.KnowledgeReferences.Single();

        Assert.False(CharacterActionAccessPolicy.IsSatisfied(
            character,
            new CharacterPhysicalAccessSnapshot(new CharacterId("other-agent"), [targetObjectId]),
            Requirement(targetObjectId, "assess-local-influence", knowledgeId)));
        Assert.False(CharacterActionAccessPolicy.IsSatisfied(
            character,
            new CharacterPhysicalAccessSnapshot(character.Identity.Id, []),
            Requirement(targetObjectId, "assess-local-influence", knowledgeId)));
        Assert.False(CharacterActionAccessPolicy.IsSatisfied(
            character,
            physicalAccess,
            Requirement(targetObjectId, "contaminate-supplies", knowledgeId)));
        Assert.False(CharacterActionAccessPolicy.IsSatisfied(
            character,
            physicalAccess,
            Requirement(
                targetObjectId,
                "assess-local-influence",
                new KnowledgeId("unknown-water-contents"))));
    }

    [Theory]
    [InlineData("")]
    [InlineData("Assess-local-influence")]
    [InlineData("assess_local_influence")]
    [InlineData("assess--local-influence")]
    public void RequirementRejectsInvalidCapabilityKeys(string capabilityKey)
    {
        Assert.Throws<ArgumentException>(() => new CharacterActionAccessRequirement(
            new ObjectId("village-water-cask"),
            capabilityKey,
            new KnowledgeId("water-cask-contents")));
    }

    [Fact]
    public void ContractsRejectNullsAndPolicyExposesOnlyOnePureOperation()
    {
        Character character = RepresentativeEnemyAgentCharacterSource.Create();
        ObjectId targetObjectId = new("village-water-cask");
        KnowledgeId knowledgeId = character.Knowledge.KnowledgeReferences.Single();
        CharacterPhysicalAccessSnapshot physicalAccess = new(
            character.Identity.Id,
            [targetObjectId]);
        CharacterActionAccessRequirement requirement = Requirement(
            targetObjectId,
            "assess-local-influence",
            knowledgeId);

        Assert.Throws<ArgumentNullException>(() =>
            new CharacterActionAccessRequirement(null!, "assess-local-influence", knowledgeId));
        Assert.Throws<ArgumentNullException>(() =>
            new CharacterActionAccessRequirement(targetObjectId, null!, knowledgeId));
        Assert.Throws<ArgumentNullException>(() =>
            new CharacterActionAccessRequirement(targetObjectId, "assess-local-influence", null!));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterActionAccessPolicy.IsSatisfied(null!, physicalAccess, requirement));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterActionAccessPolicy.IsSatisfied(character, null!, requirement));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterActionAccessPolicy.IsSatisfied(character, physicalAccess, null!));

        Type policyType = typeof(CharacterActionAccessPolicy);
        System.Reflection.MethodInfo method = Assert.Single(policyType.GetMethods().Where(
            candidate => candidate.DeclaringType == policyType));
        Assert.Equal("IsSatisfied", method.Name);
        Assert.True(method.IsStatic);
        Assert.Empty(policyType.GetConstructors());
        Assert.True(policyType.IsAbstract && policyType.IsSealed);
    }

    private static CharacterActionAccessRequirement Requirement(
        ObjectId targetObjectId,
        string capabilityKey,
        KnowledgeId knowledgeId) =>
        new(targetObjectId, capabilityKey, knowledgeId);
}
