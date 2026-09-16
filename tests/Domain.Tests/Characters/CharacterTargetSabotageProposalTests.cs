using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterTargetSabotageProposalTests
{
    [Theory]
    [MemberData(nameof(CharacterTargetKinds))]
    public void ProposalRetainsExactActorRequirementAndCharacterTargetKind(
        CharacterSabotageActionKind actionKind)
    {
        CharacterId actorId = new("enemy-agent");
        CharacterId targetId = new("village-sage");
        CharacterTargetAccessRequirement requirement = Requirement(targetId);

        CharacterTargetSabotageProposal proposal = new(actorId, requirement, actionKind);

        Assert.Same(actorId, proposal.ActorId);
        Assert.Same(requirement, proposal.AccessRequirement);
        Assert.Same(actionKind, proposal.ActionKind);
        Assert.Same(targetId, proposal.AccessRequirement.TargetCharacterId);
    }

    [Theory]
    [MemberData(nameof(NonCharacterTargetKinds))]
    public void ProposalRejectsOtherKinds(CharacterSabotageActionKind actionKind)
    {
        Assert.Throws<ArgumentException>(() => new CharacterTargetSabotageProposal(
            new CharacterId("enemy-agent"),
            Requirement(new CharacterId("village-sage")),
            actionKind));
    }

    [Fact]
    public void ProposalRejectsSelfTarget()
    {
        CharacterId actorId = new("enemy-agent");

        Assert.Throws<ArgumentException>(() => new CharacterTargetSabotageProposal(
            actorId,
            Requirement(new CharacterId(actorId.Value)),
            CharacterSabotageActionKind.Influence));
    }

    [Theory]
    [MemberData(nameof(CharacterTargetKinds))]
    public void EligibilityRequiresValueMatchingActorTargetAndCompleteExistingAccess(
        CharacterSabotageActionKind actionKind)
    {
        Character actor = RepresentativeEnemyAgentCharacterSource.Create();
        Character target = RepresentativeSageCharacterSource.Create();
        CharacterTargetAccessRequirement requirement = Requirement(
            new CharacterId(target.Identity.Id.Value),
            new KnowledgeId(actor.Knowledge.KnowledgeReferences.Single().Value));
        CharacterInteractionAccessSnapshot interactionAccess = new(
            new CharacterId(actor.Identity.Id.Value),
            [new CharacterId(target.Identity.Id.Value)]);
        CharacterTargetSabotageProposal proposal = new(
            new CharacterId(actor.Identity.Id.Value),
            requirement,
            actionKind);

        Assert.True(CharacterTargetSabotageEligibilityPolicy.CanPropose(
            actor,
            target,
            interactionAccess,
            proposal));
        Assert.Same(requirement, proposal.AccessRequirement);
    }

    [Fact]
    public void EligibilityFailsClosedForMismatchedActorTargetOrAccess()
    {
        Character actor = RepresentativeEnemyAgentCharacterSource.Create();
        Character target = RepresentativeSageCharacterSource.Create();
        Character otherTarget = RepresentativeHermitCharacterSource.Create()[0];
        KnowledgeId knowledgeId = actor.Knowledge.KnowledgeReferences.Single();
        CharacterInteractionAccessSnapshot interactionAccess = new(
            actor.Identity.Id,
            [target.Identity.Id]);

        Assert.False(CanPropose(
            actor,
            target,
            interactionAccess,
            new CharacterId("other-agent"),
            Requirement(target.Identity.Id, knowledgeId)));
        Assert.False(CanPropose(
            actor,
            otherTarget,
            interactionAccess,
            actor.Identity.Id,
            Requirement(target.Identity.Id, knowledgeId)));
        Assert.False(CanPropose(
            actor,
            target,
            new CharacterInteractionAccessSnapshot(actor.Identity.Id, []),
            actor.Identity.Id,
            Requirement(target.Identity.Id, knowledgeId)));
        Assert.False(CanPropose(
            actor,
            target,
            interactionAccess,
            actor.Identity.Id,
            new CharacterTargetAccessRequirement(
                target.Identity.Id,
                "compare-presented-evidence",
                knowledgeId)));
        Assert.False(CanPropose(
            actor,
            target,
            interactionAccess,
            actor.Identity.Id,
            Requirement(target.Identity.Id, new KnowledgeId("unknown"))));
    }

    [Fact]
    public void ContractsRejectNullsAndExposeOnlyTheBoundedPureSurface()
    {
        Character actor = RepresentativeEnemyAgentCharacterSource.Create();
        Character target = RepresentativeSageCharacterSource.Create();
        CharacterInteractionAccessSnapshot interactionAccess = new(
            actor.Identity.Id,
            [target.Identity.Id]);
        CharacterTargetSabotageProposal proposal = new(
            actor.Identity.Id,
            Requirement(target.Identity.Id, actor.Knowledge.KnowledgeReferences.Single()),
            CharacterSabotageActionKind.Delay);

        Assert.Throws<ArgumentNullException>(() =>
            new CharacterTargetSabotageProposal(null!, proposal.AccessRequirement, proposal.ActionKind));
        Assert.Throws<ArgumentNullException>(() =>
            new CharacterTargetSabotageProposal(actor.Identity.Id, null!, proposal.ActionKind));
        Assert.Throws<ArgumentNullException>(() =>
            new CharacterTargetSabotageProposal(actor.Identity.Id, proposal.AccessRequirement, null!));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterTargetSabotageEligibilityPolicy.CanPropose(
                null!, target, interactionAccess, proposal));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterTargetSabotageEligibilityPolicy.CanPropose(
                actor, null!, interactionAccess, proposal));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterTargetSabotageEligibilityPolicy.CanPropose(
                actor, target, null!, proposal));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterTargetSabotageEligibilityPolicy.CanPropose(
                actor, target, interactionAccess, null!));

        Type proposalType = typeof(CharacterTargetSabotageProposal);
        (string Name, Type Type)[] expectedProperties =
        [
            ("AccessRequirement", typeof(CharacterTargetAccessRequirement)),
            ("ActionKind", typeof(CharacterSabotageActionKind)),
            ("ActorId", typeof(CharacterId)),
        ];
        System.Reflection.PropertyInfo[] properties = proposalType.GetProperties()
            .OrderBy(property => property.Name, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(
            expectedProperties,
            properties.Select(property => (property.Name, property.PropertyType)).ToArray());
        Assert.All(properties, property => Assert.False(property.CanWrite));
        Assert.Empty(proposalType.GetMethods().Where(method =>
            method.DeclaringType == proposalType && !method.IsSpecialName));
        Assert.Empty(proposalType.GetEvents());
        Assert.True(proposalType.IsSealed);

        Type policyType = typeof(CharacterTargetSabotageEligibilityPolicy);
        System.Reflection.MethodInfo method = Assert.Single(policyType.GetMethods().Where(
            candidate => candidate.DeclaringType == policyType));
        Assert.Equal("CanPropose", method.Name);
        Assert.Equal(typeof(bool), method.ReturnType);
        Assert.Equal(
            [
                typeof(Character),
                typeof(Character),
                typeof(CharacterInteractionAccessSnapshot),
                typeof(CharacterTargetSabotageProposal),
            ],
            method.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Empty(policyType.GetConstructors());
        Assert.True(policyType.IsAbstract && policyType.IsSealed);
    }

    public static TheoryData<CharacterSabotageActionKind> CharacterTargetKinds() => new()
    {
        CharacterSabotageActionKind.Delay,
        CharacterSabotageActionKind.Influence,
    };

    public static TheoryData<CharacterSabotageActionKind> NonCharacterTargetKinds() => new()
    {
        CharacterSabotageActionKind.Conceal,
        CharacterSabotageActionKind.Move,
        CharacterSabotageActionKind.Substitute,
        CharacterSabotageActionKind.Contaminate,
        CharacterSabotageActionKind.Misquote,
    };

    private static bool CanPropose(
        Character actor,
        Character target,
        CharacterInteractionAccessSnapshot interactionAccess,
        CharacterId proposalActorId,
        CharacterTargetAccessRequirement requirement) =>
        CharacterTargetSabotageEligibilityPolicy.CanPropose(
            actor,
            target,
            interactionAccess,
            new CharacterTargetSabotageProposal(
                proposalActorId,
                requirement,
                CharacterSabotageActionKind.Influence));

    private static CharacterTargetAccessRequirement Requirement(CharacterId targetId) =>
        Requirement(targetId, new KnowledgeId("agent-national-orders"));

    private static CharacterTargetAccessRequirement Requirement(
        CharacterId targetId,
        KnowledgeId knowledgeId) =>
        new(targetId, "assess-local-influence", knowledgeId);
}
