using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterTargetSabotageInfluenceCandidateTests
{
    [Fact]
    public void CandidateRetainsExactOccurrenceAndRelationshipChange()
    {
        CharacterTargetSabotageOccurrence occurrence = Occurrence();
        CharacterRelationshipChange change = Change(occurrence);

        CharacterTargetSabotageInfluenceCandidate candidate = new(occurrence, change);

        Assert.Same(occurrence, candidate.Occurrence);
        Assert.Same(change, candidate.RelationshipChange);
        Assert.Same(occurrence.Target, candidate.Occurrence.Target);
        Assert.Same(change.Reason, candidate.RelationshipChange.Reason);
    }

    [Fact]
    public void PolicyAllowsEligibleDirectionalInfluenceWithoutMutation()
    {
        Character actor = RepresentativeEnemyAgentCharacterSource.Create();
        Character target = RepresentativeSageCharacterSource.Create();
        CharacterTargetSabotageOccurrence occurrence = Occurrence(actor, target);
        CharacterRelationshipChange change = Change(occurrence);
        CharacterTargetSabotageInfluenceCandidate candidate = new(occurrence, change);
        CharacterInteractionAccessSnapshot interactionAccess = Access(actor, target);
        CharacterRelationshipMemory relationship = Relationship(target, actor, 25);

        Assert.True(CharacterTargetSabotageInfluencePolicy.CanApply(
            actor,
            interactionAccess,
            relationship,
            candidate));
        Assert.Equal(25, relationship.Dimensions.Single().Value);
        Assert.Empty(relationship.Reasons);
        Assert.Same(occurrence, candidate.Occurrence);
        Assert.Same(change, candidate.RelationshipChange);
    }

    [Fact]
    public void PolicyRejectsDelayAndMismatchedActorOrTarget()
    {
        Character actor = RepresentativeEnemyAgentCharacterSource.Create();
        Character target = RepresentativeSageCharacterSource.Create();
        Character otherTarget = RepresentativeHermitCharacterSource.Create()[0];
        CharacterInteractionAccessSnapshot interactionAccess = new(
            actor.Identity.Id,
            [target.Identity.Id, otherTarget.Identity.Id]);
        CharacterTargetSabotageOccurrence influence = Occurrence(actor, target);

        Assert.False(CanApply(
            actor,
            interactionAccess,
            Relationship(target, actor),
            Candidate(Occurrence(actor, target, CharacterSabotageActionKind.Delay))));
        Assert.False(CanApply(
            otherTarget,
            new CharacterInteractionAccessSnapshot(otherTarget.Identity.Id, [target.Identity.Id]),
            Relationship(target, actor),
            Candidate(influence)));
        Assert.False(CanApply(
            actor,
            interactionAccess,
            Relationship(target, actor),
            Candidate(Occurrence(actor, target, requirementTargetId: otherTarget.Identity.Id))));
    }

    [Fact]
    public void PolicyRejectsInaccessibleOccurrence()
    {
        Character actor = RepresentativeEnemyAgentCharacterSource.Create();
        Character target = RepresentativeSageCharacterSource.Create();
        CharacterTargetSabotageOccurrence occurrence = Occurrence(actor, target);

        Assert.False(CanApply(
            actor,
            new CharacterInteractionAccessSnapshot(actor.Identity.Id, []),
            Relationship(target, actor),
            Candidate(occurrence)));
    }

    [Fact]
    public void PolicyRejectsChangesWithWrongDirection()
    {
        Character actor = RepresentativeEnemyAgentCharacterSource.Create();
        Character target = RepresentativeSageCharacterSource.Create();
        Character otherTarget = RepresentativeHermitCharacterSource.Create()[0];
        CharacterTargetSabotageOccurrence occurrence = Occurrence(actor, target);
        CharacterInteractionAccessSnapshot interactionAccess = Access(actor, target);

        Assert.False(CanApply(
            actor,
            interactionAccess,
            Relationship(target, actor),
            Candidate(occurrence, Change(occurrence, characterId: otherTarget.Identity.Id))));
        Assert.False(CanApply(
            actor,
            interactionAccess,
            Relationship(target, actor),
            Candidate(occurrence, Change(occurrence, relatedCharacterId: otherTarget.Identity.Id))));
    }

    [Fact]
    public void PolicyRejectsMismatchedRelationshipAndInapplicableChange()
    {
        Character actor = RepresentativeEnemyAgentCharacterSource.Create();
        Character target = RepresentativeSageCharacterSource.Create();
        Character otherTarget = RepresentativeHermitCharacterSource.Create()[0];
        CharacterTargetSabotageOccurrence occurrence = Occurrence(actor, target);
        CharacterTargetSabotageInfluenceCandidate candidate = Candidate(occurrence);
        CharacterInteractionAccessSnapshot interactionAccess = Access(actor, target);

        Assert.False(CanApply(
            actor,
            interactionAccess,
            Relationship(otherTarget, actor),
            candidate));
        Assert.False(CanApply(
            actor,
            interactionAccess,
            Relationship(target, actor, CharacterRelationshipDimensionValue.MaximumValue),
            candidate));
        Assert.Same(occurrence, candidate.Occurrence);
        Assert.Same(occurrence.Target, candidate.Occurrence.Target);
    }

    [Fact]
    public void ContractsRejectNullsAndExposeOnlyTheBoundedPureSurface()
    {
        Character actor = RepresentativeEnemyAgentCharacterSource.Create();
        Character target = RepresentativeSageCharacterSource.Create();
        CharacterTargetSabotageOccurrence occurrence = Occurrence(actor, target);
        CharacterRelationshipChange change = Change(occurrence);
        CharacterTargetSabotageInfluenceCandidate candidate = new(occurrence, change);
        CharacterInteractionAccessSnapshot interactionAccess = Access(actor, target);
        CharacterRelationshipMemory relationship = Relationship(target, actor);

        Assert.Throws<ArgumentNullException>(() =>
            new CharacterTargetSabotageInfluenceCandidate(null!, change));
        Assert.Throws<ArgumentNullException>(() =>
            new CharacterTargetSabotageInfluenceCandidate(occurrence, null!));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterTargetSabotageInfluencePolicy.CanApply(
                null!, interactionAccess, relationship, candidate));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterTargetSabotageInfluencePolicy.CanApply(
                actor, null!, relationship, candidate));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterTargetSabotageInfluencePolicy.CanApply(
                actor, interactionAccess, null!, candidate));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterTargetSabotageInfluencePolicy.CanApply(
                actor, interactionAccess, relationship, null!));

        Type candidateType = typeof(CharacterTargetSabotageInfluenceCandidate);
        (string Name, Type Type)[] expectedProperties =
        [
            ("Occurrence", typeof(CharacterTargetSabotageOccurrence)),
            ("RelationshipChange", typeof(CharacterRelationshipChange)),
        ];
        System.Reflection.PropertyInfo[] properties = candidateType.GetProperties()
            .OrderBy(property => property.Name, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(
            expectedProperties,
            properties.Select(property => (property.Name, property.PropertyType)).ToArray());
        Assert.All(properties, property => Assert.False(property.CanWrite));
        Assert.Empty(candidateType.GetMethods().Where(method =>
            method.DeclaringType == candidateType && !method.IsSpecialName));
        Assert.Empty(candidateType.GetEvents());
        Assert.True(candidateType.IsSealed);

        Type policyType = typeof(CharacterTargetSabotageInfluencePolicy);
        System.Reflection.MethodInfo method = Assert.Single(policyType.GetMethods().Where(
            candidateMethod => candidateMethod.DeclaringType == policyType));
        Assert.Equal("CanApply", method.Name);
        Assert.Equal(typeof(bool), method.ReturnType);
        Assert.Equal(
            [
                typeof(Character),
                typeof(CharacterInteractionAccessSnapshot),
                typeof(CharacterRelationshipMemory),
                typeof(CharacterTargetSabotageInfluenceCandidate),
            ],
            method.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Empty(policyType.GetConstructors());
        Assert.True(policyType.IsAbstract && policyType.IsSealed);
    }

    private static bool CanApply(
        Character actor,
        CharacterInteractionAccessSnapshot interactionAccess,
        CharacterRelationshipMemory relationship,
        CharacterTargetSabotageInfluenceCandidate candidate) =>
        CharacterTargetSabotageInfluencePolicy.CanApply(
            actor,
            interactionAccess,
            relationship,
            candidate);

    private static CharacterTargetSabotageInfluenceCandidate Candidate(
        CharacterTargetSabotageOccurrence occurrence,
        CharacterRelationshipChange? change = null) =>
        new(occurrence, change ?? Change(occurrence));

    private static CharacterTargetSabotageOccurrence Occurrence() =>
        Occurrence(
            RepresentativeEnemyAgentCharacterSource.Create(),
            RepresentativeSageCharacterSource.Create());

    private static CharacterTargetSabotageOccurrence Occurrence(
        Character actor,
        Character target,
        CharacterSabotageActionKind? actionKind = null,
        CharacterId? requirementTargetId = null) =>
        new(
            new CharacterTargetSabotageProposal(
                actor.Identity.Id,
                new CharacterTargetAccessRequirement(
                    requirementTargetId ?? target.Identity.Id,
                    "assess-local-influence",
                    actor.Knowledge.KnowledgeReferences.Single()),
                actionKind ?? CharacterSabotageActionKind.Influence),
            target);

    private static CharacterRelationshipChange Change(
        CharacterTargetSabotageOccurrence occurrence,
        CharacterId? characterId = null,
        CharacterId? relatedCharacterId = null) =>
        new(
            characterId ?? occurrence.Target.Identity.Id,
            relatedCharacterId ?? occurrence.Proposal.ActorId,
            1,
            CharacterRelationshipChangeTests.Reason(
                "influence-attempt",
                "enemy-agent-influence"));

    private static CharacterInteractionAccessSnapshot Access(Character actor, Character target) =>
        new(actor.Identity.Id, [target.Identity.Id]);

    private static CharacterRelationshipMemory Relationship(
        Character character,
        Character relatedCharacter,
        int trust = 0) =>
        new(
            character.Identity.Id,
            relatedCharacter.Identity.Id,
            [new CharacterRelationshipDimensionValue(CharacterRelationshipDimension.Trust, trust)],
            []);
}
