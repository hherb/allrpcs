using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterInteractionAccessTests
{
    [Fact]
    public void SnapshotRetainsExactActorAndStableCopiedReachableTargets()
    {
        CharacterId actorId = new("enemy-agent");
        CharacterId first = new("village-sage");
        CharacterId second = new("village-hermit");
        CharacterId[] source = [first, second];

        CharacterInteractionAccessSnapshot snapshot = new(actorId, source);
        source[0] = new CharacterId("replacement");

        Assert.Same(actorId, snapshot.ActorId);
        Assert.Equal(2, snapshot.ReachableTargetCharacterIds.Count);
        Assert.Same(first, snapshot.ReachableTargetCharacterIds[0]);
        Assert.Same(second, snapshot.ReachableTargetCharacterIds[1]);
        Assert.Throws<NotSupportedException>(() =>
            ((IList<CharacterId>)snapshot.ReachableTargetCharacterIds).Add(
                new CharacterId("extra")));
        Assert.All(
            typeof(CharacterInteractionAccessSnapshot).GetProperties(),
            property => Assert.False(property.CanWrite));
    }

    [Fact]
    public void SnapshotAcceptsEmptyAndMaximumBoundaries()
    {
        CharacterInteractionAccessSnapshot empty = new(new CharacterId("observer"), []);
        CharacterId[] maximum = Enumerable
            .Range(0, CharacterInteractionAccessSnapshot.MaximumReachableTargetCount)
            .Select(index => new CharacterId($"target-{index}"))
            .ToArray();
        CharacterInteractionAccessSnapshot full = new(new CharacterId("agent"), maximum);

        Assert.Empty(empty.ReachableTargetCharacterIds);
        Assert.Equal(
            CharacterInteractionAccessSnapshot.MaximumReachableTargetCount,
            full.ReachableTargetCharacterIds.Count);
    }

    [Fact]
    public void SnapshotRejectsNullSelfDuplicateAndOversizedInput()
    {
        CharacterId actorId = new("enemy-agent");
        CharacterId targetId = new("village-sage");
        CharacterId[] oversized = Enumerable
            .Range(0, CharacterInteractionAccessSnapshot.MaximumReachableTargetCount + 1)
            .Select(index => new CharacterId($"target-{index}"))
            .ToArray();

        Assert.Throws<ArgumentNullException>(() =>
            new CharacterInteractionAccessSnapshot(null!, []));
        Assert.Throws<ArgumentNullException>(() =>
            new CharacterInteractionAccessSnapshot(actorId, null!));
        Assert.Throws<ArgumentException>(() =>
            new CharacterInteractionAccessSnapshot(actorId, [targetId, null!]));
        Assert.Throws<ArgumentException>(() =>
            new CharacterInteractionAccessSnapshot(actorId, [new CharacterId(actorId.Value)]));
        Assert.Throws<ArgumentException>(() =>
            new CharacterInteractionAccessSnapshot(
                actorId,
                [targetId, new CharacterId(targetId.Value)]));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new CharacterInteractionAccessSnapshot(actorId, oversized));
    }

    [Fact]
    public void RequirementRetainsExactTargetCapabilityAndKnowledge()
    {
        CharacterId targetId = new("village-sage");
        KnowledgeId knowledgeId = new("agent-national-orders");

        CharacterTargetAccessRequirement requirement = new(
            targetId,
            "assess-local-influence",
            knowledgeId);

        Assert.Same(targetId, requirement.TargetCharacterId);
        Assert.Equal("assess-local-influence", requirement.RequiredCapabilityKey);
        Assert.Same(knowledgeId, requirement.RequiredKnowledgeId);
        Assert.All(
            typeof(CharacterTargetAccessRequirement).GetProperties(),
            property => Assert.False(property.CanWrite));
    }

    [Fact]
    public void PolicyRequiresActorTargetInteractionCapabilityAndKnowledgeTogether()
    {
        Character actor = RepresentativeEnemyAgentCharacterSource.Create();
        Character target = RepresentativeSageCharacterSource.Create();
        CharacterInteractionAccessSnapshot interactionAccess = new(
            new CharacterId(actor.Identity.Id.Value),
            [new CharacterId(target.Identity.Id.Value)]);
        CharacterTargetAccessRequirement requirement = new(
            new CharacterId(target.Identity.Id.Value),
            "assess-local-influence",
            new KnowledgeId(actor.Knowledge.KnowledgeReferences.Single().Value));

        Assert.True(CharacterTargetAccessPolicy.IsSatisfied(
            actor,
            target,
            interactionAccess,
            requirement));
    }

    [Fact]
    public void PolicyFailsClosedWhenAnyRequiredAccessIsMissingOrMismatched()
    {
        Character actor = RepresentativeEnemyAgentCharacterSource.Create();
        Character target = RepresentativeSageCharacterSource.Create();
        Character otherTarget = RepresentativeHermitCharacterSource.Create()[0];
        KnowledgeId knowledgeId = actor.Knowledge.KnowledgeReferences.Single();
        CharacterInteractionAccessSnapshot interactionAccess = new(
            actor.Identity.Id,
            [target.Identity.Id]);

        Assert.False(IsSatisfied(
            target,
            target,
            interactionAccess,
            Requirement(target.Identity.Id, "assess-local-influence", knowledgeId)));
        Assert.False(IsSatisfied(
            actor,
            target,
            new CharacterInteractionAccessSnapshot(new CharacterId("other-agent"), [target.Identity.Id]),
            Requirement(target.Identity.Id, "assess-local-influence", knowledgeId)));
        Assert.False(IsSatisfied(
            actor,
            target,
            new CharacterInteractionAccessSnapshot(actor.Identity.Id, []),
            Requirement(target.Identity.Id, "assess-local-influence", knowledgeId)));
        Assert.False(IsSatisfied(
            actor,
            otherTarget,
            interactionAccess,
            Requirement(target.Identity.Id, "assess-local-influence", knowledgeId)));
        Assert.False(IsSatisfied(
            actor,
            target,
            interactionAccess,
            Requirement(otherTarget.Identity.Id, "assess-local-influence", knowledgeId)));
        Assert.False(IsSatisfied(
            actor,
            target,
            interactionAccess,
            Requirement(target.Identity.Id, "compare-presented-evidence", knowledgeId)));
        Assert.False(IsSatisfied(
            actor,
            target,
            interactionAccess,
            Requirement(target.Identity.Id, "assess-local-influence", new KnowledgeId("unknown"))));
    }

    [Theory]
    [InlineData("")]
    [InlineData("Assess-local-influence")]
    [InlineData("assess_local_influence")]
    [InlineData("assess--local-influence")]
    public void RequirementRejectsInvalidCapabilityKeys(string capabilityKey)
    {
        Assert.Throws<ArgumentException>(() => new CharacterTargetAccessRequirement(
            new CharacterId("village-sage"),
            capabilityKey,
            new KnowledgeId("agent-national-orders")));
    }

    [Fact]
    public void ContractsRejectNullsAndPolicyExposesOnlyOnePureOperation()
    {
        Character actor = RepresentativeEnemyAgentCharacterSource.Create();
        Character target = RepresentativeSageCharacterSource.Create();
        KnowledgeId knowledgeId = actor.Knowledge.KnowledgeReferences.Single();
        CharacterInteractionAccessSnapshot interactionAccess = new(
            actor.Identity.Id,
            [target.Identity.Id]);
        CharacterTargetAccessRequirement requirement = Requirement(
            target.Identity.Id,
            "assess-local-influence",
            knowledgeId);

        Assert.Throws<ArgumentNullException>(() =>
            new CharacterTargetAccessRequirement(null!, "assess-local-influence", knowledgeId));
        Assert.Throws<ArgumentNullException>(() =>
            new CharacterTargetAccessRequirement(target.Identity.Id, null!, knowledgeId));
        Assert.Throws<ArgumentNullException>(() =>
            new CharacterTargetAccessRequirement(target.Identity.Id, "assess-local-influence", null!));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterTargetAccessPolicy.IsSatisfied(null!, target, interactionAccess, requirement));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterTargetAccessPolicy.IsSatisfied(actor, null!, interactionAccess, requirement));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterTargetAccessPolicy.IsSatisfied(actor, target, null!, requirement));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterTargetAccessPolicy.IsSatisfied(actor, target, interactionAccess, null!));

        Type snapshotType = typeof(CharacterInteractionAccessSnapshot);
        Assert.Equal(
            [
                ("ActorId", typeof(CharacterId)),
                ("ReachableTargetCharacterIds", typeof(IReadOnlyList<CharacterId>)),
            ],
            snapshotType.GetProperties()
                .OrderBy(property => property.Name, StringComparer.Ordinal)
                .Select(property => (property.Name, property.PropertyType))
                .ToArray());
        Assert.Empty(snapshotType.GetMethods().Where(method =>
            method.DeclaringType == snapshotType && !method.IsSpecialName));
        Assert.Empty(snapshotType.GetEvents());
        Assert.True(snapshotType.IsSealed);

        Type requirementType = typeof(CharacterTargetAccessRequirement);
        Assert.Equal(
            [
                ("RequiredCapabilityKey", typeof(string)),
                ("RequiredKnowledgeId", typeof(KnowledgeId)),
                ("TargetCharacterId", typeof(CharacterId)),
            ],
            requirementType.GetProperties()
                .OrderBy(property => property.Name, StringComparer.Ordinal)
                .Select(property => (property.Name, property.PropertyType))
                .ToArray());
        Assert.Empty(requirementType.GetEvents());
        Assert.True(requirementType.IsSealed);

        Type policyType = typeof(CharacterTargetAccessPolicy);
        System.Reflection.MethodInfo method = Assert.Single(policyType.GetMethods().Where(
            candidate => candidate.DeclaringType == policyType));
        Assert.Equal("IsSatisfied", method.Name);
        Assert.Equal(typeof(bool), method.ReturnType);
        Assert.Equal(
            [
                typeof(Character),
                typeof(Character),
                typeof(CharacterInteractionAccessSnapshot),
                typeof(CharacterTargetAccessRequirement),
            ],
            method.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.True(method.IsStatic);
        Assert.Empty(policyType.GetConstructors());
        Assert.True(policyType.IsAbstract && policyType.IsSealed);
    }

    private static bool IsSatisfied(
        Character actor,
        Character target,
        CharacterInteractionAccessSnapshot interactionAccess,
        CharacterTargetAccessRequirement requirement) =>
        CharacterTargetAccessPolicy.IsSatisfied(actor, target, interactionAccess, requirement);

    private static CharacterTargetAccessRequirement Requirement(
        CharacterId targetId,
        string capabilityKey,
        KnowledgeId knowledgeId) =>
        new(targetId, capabilityKey, knowledgeId);
}
