using Allrpcs.Domain.Characters;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterTargetSabotageInfluenceTransitionTests
{
    [Fact]
    public void ApplyReturnsExactCandidateAndFreshChangedRelationship()
    {
        InfluenceInput input = Input();
        CharacterRelationshipDimensionValue trust = input.Relationship.Dimensions.Single();

        CharacterTargetSabotageInfluenceResult result =
            CharacterTargetSabotageInfluenceTransition.Apply(
                input.Actor,
                input.InteractionAccess,
                input.Relationship,
                input.Candidate);

        Assert.Same(input.Candidate, result.Candidate);
        Assert.NotSame(input.Relationship, result.Relationship);
        Assert.Same(input.Relationship.CharacterId, result.Relationship.CharacterId);
        Assert.Same(input.Relationship.RelatedCharacterId, result.Relationship.RelatedCharacterId);
        Assert.Equal(11, result.Relationship.Dimensions.Single().Value);
        Assert.Same(
            input.Candidate.RelationshipChange.Reason,
            Assert.Single(result.Relationship.Reasons));
        Assert.Same(trust, input.Relationship.Dimensions.Single());
        Assert.Empty(input.Relationship.Reasons);
    }

    [Fact]
    public void ApplyRejectsIneligibleCandidateWithoutChangingInputs()
    {
        InfluenceInput input = Input(CharacterSabotageActionKind.Delay);
        CharacterTargetSabotageOccurrence occurrence = input.Candidate.Occurrence;
        CharacterRelationshipChange change = input.Candidate.RelationshipChange;

        Assert.Throws<InvalidOperationException>(() =>
            CharacterTargetSabotageInfluenceTransition.Apply(
                input.Actor,
                input.InteractionAccess,
                input.Relationship,
                input.Candidate));

        Assert.Equal(10, input.Relationship.Dimensions.Single().Value);
        Assert.Empty(input.Relationship.Reasons);
        Assert.Same(occurrence, input.Candidate.Occurrence);
        Assert.Same(change, input.Candidate.RelationshipChange);
    }

    [Fact]
    public void ContractsRejectNullsAndExposeOnlyClosedReadOnlyOperations()
    {
        InfluenceInput input = Input();
        CharacterTargetSabotageInfluenceResult result =
            CharacterTargetSabotageInfluenceTransition.Apply(
                input.Actor,
                input.InteractionAccess,
                input.Relationship,
                input.Candidate);

        Assert.Throws<ArgumentNullException>(() =>
            new CharacterTargetSabotageInfluenceResult(null!, result.Relationship));
        Assert.Throws<ArgumentNullException>(() =>
            new CharacterTargetSabotageInfluenceResult(input.Candidate, null!));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterTargetSabotageInfluenceTransition.Apply(
                null!, input.InteractionAccess, input.Relationship, input.Candidate));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterTargetSabotageInfluenceTransition.Apply(
                input.Actor, null!, input.Relationship, input.Candidate));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterTargetSabotageInfluenceTransition.Apply(
                input.Actor, input.InteractionAccess, null!, input.Candidate));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterTargetSabotageInfluenceTransition.Apply(
                input.Actor, input.InteractionAccess, input.Relationship, null!));

        Type resultType = typeof(CharacterTargetSabotageInfluenceResult);
        (string Name, Type Type)[] expectedProperties =
        [
            ("Candidate", typeof(CharacterTargetSabotageInfluenceCandidate)),
            ("Relationship", typeof(CharacterRelationshipMemory)),
        ];
        System.Reflection.PropertyInfo[] properties = resultType.GetProperties()
            .OrderBy(property => property.Name, StringComparer.Ordinal)
            .ToArray();
        Assert.Equal(
            expectedProperties,
            properties.Select(property => (property.Name, property.PropertyType)).ToArray());
        Assert.All(properties, property => Assert.False(property.CanWrite));
        Assert.True(resultType.IsSealed);

        Type transitionType = typeof(CharacterTargetSabotageInfluenceTransition);
        System.Reflection.MethodInfo method = Assert.Single(transitionType.GetMethods().Where(
            candidateMethod => candidateMethod.DeclaringType == transitionType));
        Assert.Equal("Apply", method.Name);
        Assert.Equal(typeof(CharacterTargetSabotageInfluenceResult), method.ReturnType);
        Assert.Equal(
            [
                typeof(Character),
                typeof(CharacterInteractionAccessSnapshot),
                typeof(CharacterRelationshipMemory),
                typeof(CharacterTargetSabotageInfluenceCandidate),
            ],
            method.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Empty(transitionType.GetConstructors());
        Assert.True(transitionType.IsAbstract && transitionType.IsSealed);
    }

    private static InfluenceInput Input(CharacterSabotageActionKind? actionKind = null)
    {
        Character actor = RepresentativeEnemyAgentCharacterSource.Create();
        Character target = RepresentativeSageCharacterSource.Create();
        CharacterTargetSabotageOccurrence occurrence = new(
            new CharacterTargetSabotageProposal(
                actor.Identity.Id,
                new CharacterTargetAccessRequirement(
                    target.Identity.Id,
                    "assess-local-influence",
                    actor.Knowledge.KnowledgeReferences.Single()),
                actionKind ?? CharacterSabotageActionKind.Influence),
            target);
        CharacterRelationshipChange change = new(
            target.Identity.Id,
            actor.Identity.Id,
            1,
            CharacterRelationshipChangeTests.Reason(
                "influence-attempt",
                "enemy-agent-influence"));
        CharacterTargetSabotageInfluenceCandidate candidate = new(occurrence, change);

        return new InfluenceInput(
            actor,
            new CharacterInteractionAccessSnapshot(actor.Identity.Id, [target.Identity.Id]),
            new CharacterRelationshipMemory(
                target.Identity.Id,
                actor.Identity.Id,
                [new CharacterRelationshipDimensionValue(CharacterRelationshipDimension.Trust, 10)],
                []),
            candidate);
    }

    private sealed record InfluenceInput(
        Character Actor,
        CharacterInteractionAccessSnapshot InteractionAccess,
        CharacterRelationshipMemory Relationship,
        CharacterTargetSabotageInfluenceCandidate Candidate);
}
