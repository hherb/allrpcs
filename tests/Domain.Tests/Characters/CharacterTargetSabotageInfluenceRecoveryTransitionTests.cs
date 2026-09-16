using Allrpcs.Domain.Characters;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterTargetSabotageInfluenceRecoveryTransitionTests
{
    [Fact]
    public void RecoverReturnsExactCandidateAndFreshRestoredRelationship()
    {
        RecoveryInput input = Input();
        CharacterRelationshipReason influenceReason =
            input.InfluenceResult.Candidate.RelationshipChange.Reason;

        CharacterTargetSabotageInfluenceRecoveryResult result =
            CharacterTargetSabotageInfluenceRecoveryTransition.Recover(
                input.InfluenceResult.Relationship,
                input.Candidate);

        Assert.Same(input.Candidate, result.Candidate);
        Assert.NotSame(input.InfluenceResult.Relationship, result.Relationship);
        Assert.Same(
            input.InfluenceResult.Relationship.CharacterId,
            result.Relationship.CharacterId);
        Assert.Same(
            input.InfluenceResult.Relationship.RelatedCharacterId,
            result.Relationship.RelatedCharacterId);
        Assert.Equal(10, result.Relationship.Dimensions.Single().Value);
        Assert.Collection(
            result.Relationship.Reasons,
            reason => Assert.Same(influenceReason, reason),
            reason => Assert.Same(input.Candidate.RecoveryChange.Reason, reason));
        Assert.Equal(11, input.InfluenceResult.Relationship.Dimensions.Single().Value);
        Assert.Same(
            influenceReason,
            Assert.Single(input.InfluenceResult.Relationship.Reasons));
    }

    [Fact]
    public void RecoverRejectsStaleCurrentStateWithoutChangingInputs()
    {
        RecoveryInput input = Input();
        CharacterTargetSabotageInfluenceResult influenceResult =
            input.Candidate.InfluenceResult;
        CharacterRelationshipChange recoveryChange = input.Candidate.RecoveryChange;

        Assert.Throws<InvalidOperationException>(() =>
            CharacterTargetSabotageInfluenceRecoveryTransition.Recover(
                input.PriorRelationship,
                input.Candidate));

        Assert.Equal(10, input.PriorRelationship.Dimensions.Single().Value);
        Assert.Empty(input.PriorRelationship.Reasons);
        Assert.Equal(11, input.InfluenceResult.Relationship.Dimensions.Single().Value);
        Assert.Same(influenceResult, input.Candidate.InfluenceResult);
        Assert.Same(recoveryChange, input.Candidate.RecoveryChange);
    }

    [Fact]
    public void ContractsRejectNullsAndExposeOnlyClosedReadOnlyOperations()
    {
        RecoveryInput input = Input();
        CharacterTargetSabotageInfluenceRecoveryResult result =
            CharacterTargetSabotageInfluenceRecoveryTransition.Recover(
                input.InfluenceResult.Relationship,
                input.Candidate);

        Assert.Throws<ArgumentNullException>(() =>
            new CharacterTargetSabotageInfluenceRecoveryResult(
                null!, result.Relationship));
        Assert.Throws<ArgumentNullException>(() =>
            new CharacterTargetSabotageInfluenceRecoveryResult(
                input.Candidate, null!));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterTargetSabotageInfluenceRecoveryTransition.Recover(
                null!, input.Candidate));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterTargetSabotageInfluenceRecoveryTransition.Recover(
                input.InfluenceResult.Relationship, null!));

        Type resultType = typeof(CharacterTargetSabotageInfluenceRecoveryResult);
        (string Name, Type Type)[] expectedProperties =
        [
            ("Candidate", typeof(CharacterTargetSabotageInfluenceRecoveryCandidate)),
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

        Type transitionType = typeof(CharacterTargetSabotageInfluenceRecoveryTransition);
        System.Reflection.MethodInfo method = Assert.Single(transitionType.GetMethods().Where(
            candidateMethod => candidateMethod.DeclaringType == transitionType));
        Assert.Equal("Recover", method.Name);
        Assert.Equal(typeof(CharacterTargetSabotageInfluenceRecoveryResult), method.ReturnType);
        Assert.Equal(
            [
                typeof(CharacterRelationshipMemory),
                typeof(CharacterTargetSabotageInfluenceRecoveryCandidate),
            ],
            method.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Empty(transitionType.GetConstructors());
        Assert.True(transitionType.IsAbstract && transitionType.IsSealed);
    }

    private static RecoveryInput Input()
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
                CharacterSabotageActionKind.Influence),
            target);
        CharacterRelationshipChange influenceChange = new(
            target.Identity.Id,
            actor.Identity.Id,
            1,
            CharacterRelationshipChangeTests.Reason(
                "influence-attempt",
                "enemy-agent-influence"));
        CharacterTargetSabotageInfluenceCandidate influenceCandidate = new(
            occurrence,
            influenceChange);
        CharacterRelationshipMemory priorRelationship = new(
            target.Identity.Id,
            actor.Identity.Id,
            [new CharacterRelationshipDimensionValue(CharacterRelationshipDimension.Trust, 10)],
            []);
        CharacterTargetSabotageInfluenceResult influenceResult = new(
            influenceCandidate,
            CharacterRelationshipChangeTransition.Apply(
                priorRelationship,
                influenceChange));
        CharacterRelationshipChange recoveryChange = new(
            target.Identity.Id,
            actor.Identity.Id,
            -1,
            CharacterRelationshipChangeTests.Reason(
                "influence-recovered",
                "seeker-recovery"));

        return new RecoveryInput(
            priorRelationship,
            influenceResult,
            new CharacterTargetSabotageInfluenceRecoveryCandidate(
                influenceResult,
                recoveryChange));
    }

    private sealed record RecoveryInput(
        CharacterRelationshipMemory PriorRelationship,
        CharacterTargetSabotageInfluenceResult InfluenceResult,
        CharacterTargetSabotageInfluenceRecoveryCandidate Candidate);
}
