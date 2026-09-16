using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterTargetSabotageOccurrenceTests
{
    [Fact]
    public void CandidateRetainsExactProposalAndTarget()
    {
        Character target = RepresentativeSageCharacterSource.Create();
        CharacterTargetSabotageProposal proposal = Proposal(target.Identity.Id);

        CharacterTargetSabotageOccurrence candidate = new(proposal, target);

        Assert.Same(proposal, candidate.Proposal);
        Assert.Same(target, candidate.Target);
        Assert.All(
            typeof(CharacterTargetSabotageOccurrence).GetProperties(),
            property => Assert.False(property.CanWrite));
    }

    [Theory]
    [InlineData("delay")]
    [InlineData("influence")]
    public void PolicyAllowsEligibleProposalForItsExactRetainedTarget(string actionKind)
    {
        Character actor = RepresentativeEnemyAgentCharacterSource.Create();
        Character target = RepresentativeSageCharacterSource.Create();
        CharacterTargetSabotageOccurrence candidate = new(
            Proposal(
                new CharacterId(actor.Identity.Id.Value),
                new CharacterId(target.Identity.Id.Value),
                new KnowledgeId(actor.Knowledge.KnowledgeReferences.Single().Value),
                CharacterSabotageActionKind.Parse(actionKind)),
            target);
        CharacterInteractionAccessSnapshot interactionAccess = new(
            new CharacterId(actor.Identity.Id.Value),
            [new CharacterId(target.Identity.Id.Value)]);

        Assert.True(CharacterTargetSabotageOccurrencePolicy.CanOccur(
            actor,
            interactionAccess,
            candidate));
    }

    [Fact]
    public void PolicyRejectsMismatchedTarget()
    {
        Character actor = RepresentativeEnemyAgentCharacterSource.Create();
        Character target = RepresentativeSageCharacterSource.Create();
        Character otherTarget = RepresentativeHermitCharacterSource.Create()[0];
        CharacterInteractionAccessSnapshot interactionAccess = new(
            actor.Identity.Id,
            [target.Identity.Id, otherTarget.Identity.Id]);
        CharacterTargetSabotageOccurrence candidate = new(
            Proposal(
                actor.Identity.Id,
                otherTarget.Identity.Id,
                actor.Knowledge.KnowledgeReferences.Single()),
            target);

        Assert.False(CharacterTargetSabotageOccurrencePolicy.CanOccur(
            actor,
            interactionAccess,
            candidate));
        Assert.Same(target, candidate.Target);
        Assert.Same(otherTarget.Identity.Id, candidate.Proposal.AccessRequirement.TargetCharacterId);
    }

    [Fact]
    public void PolicyDelegatesActorAndCompleteAccessEligibility()
    {
        Character actor = RepresentativeEnemyAgentCharacterSource.Create();
        Character target = RepresentativeSageCharacterSource.Create();
        KnowledgeId knowledgeId = actor.Knowledge.KnowledgeReferences.Single();
        CharacterInteractionAccessSnapshot interactionAccess = new(
            actor.Identity.Id,
            [target.Identity.Id]);

        Assert.False(CanOccur(
            actor,
            interactionAccess,
            Candidate(new CharacterId("other-agent"), target, knowledgeId)));
        Assert.False(CanOccur(
            actor,
            new CharacterInteractionAccessSnapshot(actor.Identity.Id, []),
            Candidate(actor.Identity.Id, target, knowledgeId)));
        Assert.False(CanOccur(
            actor,
            interactionAccess,
            Candidate(actor.Identity.Id, target, knowledgeId, "compare-presented-evidence")));
        Assert.False(CanOccur(
            actor,
            interactionAccess,
            Candidate(actor.Identity.Id, target, new KnowledgeId("unknown"))));
    }

    [Fact]
    public void ContractsRejectNullsAndExposeOnlyTheBoundedPureSurface()
    {
        Character actor = RepresentativeEnemyAgentCharacterSource.Create();
        Character target = RepresentativeSageCharacterSource.Create();
        CharacterTargetSabotageProposal proposal = Proposal(target.Identity.Id);
        CharacterTargetSabotageOccurrence candidate = new(proposal, target);
        CharacterInteractionAccessSnapshot interactionAccess = new(
            actor.Identity.Id,
            [target.Identity.Id]);

        Assert.Throws<ArgumentNullException>(() =>
            new CharacterTargetSabotageOccurrence(null!, target));
        Assert.Throws<ArgumentNullException>(() =>
            new CharacterTargetSabotageOccurrence(proposal, null!));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterTargetSabotageOccurrencePolicy.CanOccur(
                null!, interactionAccess, candidate));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterTargetSabotageOccurrencePolicy.CanOccur(
                actor, null!, candidate));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterTargetSabotageOccurrencePolicy.CanOccur(
                actor, interactionAccess, null!));

        Type occurrenceType = typeof(CharacterTargetSabotageOccurrence);
        (string Name, Type Type)[] expectedProperties =
        [
            ("Proposal", typeof(CharacterTargetSabotageProposal)),
            ("Target", typeof(Character)),
        ];
        System.Reflection.PropertyInfo[] properties = occurrenceType.GetProperties()
            .OrderBy(property => property.Name, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(
            expectedProperties,
            properties.Select(property => (property.Name, property.PropertyType)).ToArray());
        Assert.All(properties, property => Assert.False(property.CanWrite));
        Assert.Empty(occurrenceType.GetMethods().Where(method =>
            method.DeclaringType == occurrenceType && !method.IsSpecialName));
        Assert.Empty(occurrenceType.GetEvents());
        Assert.True(occurrenceType.IsSealed);

        Type policyType = typeof(CharacterTargetSabotageOccurrencePolicy);
        System.Reflection.MethodInfo method = Assert.Single(policyType.GetMethods().Where(
            candidateMethod => candidateMethod.DeclaringType == policyType));
        Assert.Equal("CanOccur", method.Name);
        Assert.Equal(typeof(bool), method.ReturnType);
        Assert.Equal(
            [
                typeof(Character),
                typeof(CharacterInteractionAccessSnapshot),
                typeof(CharacterTargetSabotageOccurrence),
            ],
            method.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Empty(policyType.GetConstructors());
        Assert.True(policyType.IsAbstract && policyType.IsSealed);
    }

    private static bool CanOccur(
        Character actor,
        CharacterInteractionAccessSnapshot interactionAccess,
        CharacterTargetSabotageOccurrence candidate) =>
        CharacterTargetSabotageOccurrencePolicy.CanOccur(
            actor,
            interactionAccess,
            candidate);

    private static CharacterTargetSabotageOccurrence Candidate(
        CharacterId actorId,
        Character target,
        KnowledgeId knowledgeId,
        string capabilityKey = "assess-local-influence") =>
        new(
            Proposal(
                actorId,
                target.Identity.Id,
                knowledgeId,
                CharacterSabotageActionKind.Delay,
                capabilityKey),
            target);

    private static CharacterTargetSabotageProposal Proposal(CharacterId targetId) =>
        Proposal(
            new CharacterId("enemy-agent"),
            targetId,
            new KnowledgeId("agent-national-orders"),
            CharacterSabotageActionKind.Delay);

    private static CharacterTargetSabotageProposal Proposal(
        CharacterId actorId,
        CharacterId targetId,
        KnowledgeId knowledgeId,
        CharacterSabotageActionKind? actionKind = null,
        string capabilityKey = "assess-local-influence") =>
        new(
            actorId,
            new CharacterTargetAccessRequirement(targetId, capabilityKey, knowledgeId),
            actionKind ?? CharacterSabotageActionKind.Delay);
}
