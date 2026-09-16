using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterTargetSabotageInfluenceRecoveryCandidateTests
{
    [Fact]
    public void CandidateRetainsExactInfluenceResultAndRecoveryChange()
    {
        RecoveryInput input = Input();

        Assert.Same(input.InfluenceResult, input.Candidate.InfluenceResult);
        Assert.Same(input.RecoveryChange, input.Candidate.RecoveryChange);
        Assert.Same(
            input.InfluenceResult.Candidate,
            input.Candidate.InfluenceResult.Candidate);
        Assert.Same(input.RecoveryChange.Reason, input.Candidate.RecoveryChange.Reason);
    }

    [Fact]
    public void PolicyAllowsExactCurrentInverseChangeWithoutMutation()
    {
        RecoveryInput input = Input();

        Assert.True(CharacterTargetSabotageInfluenceRecoveryPolicy.CanRecover(
            input.InfluenceResult.Relationship,
            input.Candidate));
        Assert.Equal(11, input.InfluenceResult.Relationship.Dimensions.Single().Value);
        Assert.Same(
            input.InfluenceResult.Candidate.RelationshipChange.Reason,
            Assert.Single(input.InfluenceResult.Relationship.Reasons));
        Assert.Same(input.RecoveryChange, input.Candidate.RecoveryChange);
    }

    [Fact]
    public void PolicyRejectsDistinctValueEqualAndPriorRelationshipState()
    {
        RecoveryInput input = Input();
        CharacterRelationshipMemory current = input.InfluenceResult.Relationship;
        CharacterRelationshipMemory valueEqual = new(
            new CharacterId(current.CharacterId.Value),
            new CharacterId(current.RelatedCharacterId.Value),
            current.Dimensions.Select(dimension => new CharacterRelationshipDimensionValue(
                dimension.Dimension,
                dimension.Value)),
            current.Reasons);

        Assert.NotSame(current, valueEqual);
        Assert.Equal(current.CharacterId, valueEqual.CharacterId);
        Assert.Equal(current.RelatedCharacterId, valueEqual.RelatedCharacterId);
        Assert.Equal(
            current.Dimensions.Select(dimension => (dimension.Dimension, dimension.Value)),
            valueEqual.Dimensions.Select(dimension => (dimension.Dimension, dimension.Value)));
        Assert.Equal(
            current.Reasons.Select(reason => reason.ReasonId),
            valueEqual.Reasons.Select(reason => reason.ReasonId));
        Assert.False(CharacterTargetSabotageInfluenceRecoveryPolicy.CanRecover(
            valueEqual,
            input.Candidate));
        Assert.False(CharacterTargetSabotageInfluenceRecoveryPolicy.CanRecover(
            input.PriorRelationship,
            input.Candidate));
    }

    [Fact]
    public void PolicyRejectsWrongOwnerRelatedDimensionAndDelta()
    {
        RecoveryInput input = Input();
        CharacterRelationshipChange applied = input.InfluenceResult.Candidate.RelationshipChange;

        Assert.False(CanRecover(input, RecoveryChange(
            input,
            characterId: new CharacterId("hermit"))));
        Assert.False(CanRecover(input, RecoveryChange(
            input,
            relatedCharacterId: new CharacterId("sage"))));
        Assert.False(CanRecover(input, RecoveryChange(
            input,
            delta: applied.Delta)));
        Assert.False(CanRecover(input, RecoveryChange(
            input,
            dimension: CharacterRelationshipDimension.Respect)));
    }

    [Fact]
    public void PolicyRejectsOtherwiseInapplicableInverseChange()
    {
        RecoveryInput input = Input();
        CharacterRelationshipReason[] priorReasons = Enumerable
            .Range(0, CharacterRelationshipMemory.MaximumReasonCount - 1)
            .Select(index => CharacterRelationshipChangeTests.Reason(
                $"prior-{index}",
                $"prior-turn-{index}"))
            .ToArray();
        CharacterRelationshipReason appliedReason =
            input.InfluenceResult.Candidate.RelationshipChange.Reason;
        CharacterRelationshipMemory conflicting = new(
            input.InfluenceResult.Relationship.CharacterId,
            input.InfluenceResult.Relationship.RelatedCharacterId,
            input.InfluenceResult.Relationship.Dimensions,
            priorReasons.Append(appliedReason));
        CharacterTargetSabotageInfluenceResult staleResult = new(
            input.InfluenceResult.Candidate,
            conflicting);
        CharacterTargetSabotageInfluenceRecoveryCandidate candidate = new(
            staleResult,
            input.RecoveryChange);

        Assert.False(CharacterTargetSabotageInfluenceRecoveryPolicy.CanRecover(
            conflicting,
            candidate));
        Assert.Equal(CharacterRelationshipMemory.MaximumReasonCount, conflicting.Reasons.Count);
        Assert.Same(appliedReason, conflicting.Reasons[^1]);
    }

    [Fact]
    public void PolicyRejectsResultWithoutExactAppliedReasonAtHistoryTail()
    {
        RecoveryInput input = Input();
        CharacterRelationshipMemory fabricated = new(
            input.InfluenceResult.Relationship.CharacterId,
            input.InfluenceResult.Relationship.RelatedCharacterId,
            input.InfluenceResult.Relationship.Dimensions,
            [CharacterRelationshipChangeTests.Reason("other", "other-turn")]);
        CharacterTargetSabotageInfluenceResult fabricatedResult = new(
            input.InfluenceResult.Candidate,
            fabricated);
        CharacterTargetSabotageInfluenceRecoveryCandidate candidate = new(
            fabricatedResult,
            input.RecoveryChange);

        Assert.False(CharacterTargetSabotageInfluenceRecoveryPolicy.CanRecover(
            fabricated,
            candidate));
        Assert.Same(
            input.InfluenceResult.Candidate.RelationshipChange.Reason,
            input.InfluenceResult.Relationship.Reasons[^1]);
    }

    [Fact]
    public void ContractsRejectNullsAndExposeOnlyTheBoundedPureSurface()
    {
        RecoveryInput input = Input();

        Assert.Throws<ArgumentNullException>(() =>
            new CharacterTargetSabotageInfluenceRecoveryCandidate(
                null!, input.RecoveryChange));
        Assert.Throws<ArgumentNullException>(() =>
            new CharacterTargetSabotageInfluenceRecoveryCandidate(
                input.InfluenceResult, null!));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterTargetSabotageInfluenceRecoveryPolicy.CanRecover(
                null!, input.Candidate));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterTargetSabotageInfluenceRecoveryPolicy.CanRecover(
                input.InfluenceResult.Relationship, null!));

        Type candidateType = typeof(CharacterTargetSabotageInfluenceRecoveryCandidate);
        (string Name, Type Type)[] expectedProperties =
        [
            ("InfluenceResult", typeof(CharacterTargetSabotageInfluenceResult)),
            ("RecoveryChange", typeof(CharacterRelationshipChange)),
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

        Type policyType = typeof(CharacterTargetSabotageInfluenceRecoveryPolicy);
        System.Reflection.MethodInfo method = Assert.Single(policyType.GetMethods().Where(
            candidateMethod => candidateMethod.DeclaringType == policyType));
        Assert.Equal("CanRecover", method.Name);
        Assert.Equal(typeof(bool), method.ReturnType);
        Assert.Equal(
            [
                typeof(CharacterRelationshipMemory),
                typeof(CharacterTargetSabotageInfluenceRecoveryCandidate),
            ],
            method.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Empty(policyType.GetConstructors());
        Assert.True(policyType.IsAbstract && policyType.IsSealed);
    }

    private static bool CanRecover(
        RecoveryInput input,
        CharacterRelationshipChange recoveryChange) =>
        CharacterTargetSabotageInfluenceRecoveryPolicy.CanRecover(
            input.InfluenceResult.Relationship,
            new CharacterTargetSabotageInfluenceRecoveryCandidate(
                input.InfluenceResult,
                recoveryChange));

    private static RecoveryInput Input(int appliedDelta = 1, int priorTrust = 10)
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
            appliedDelta,
            CharacterRelationshipChangeTests.Reason(
                "influence-attempt",
                "enemy-agent-influence"));
        CharacterTargetSabotageInfluenceCandidate influenceCandidate = new(
            occurrence,
            influenceChange);
        CharacterRelationshipMemory priorRelationship = new(
            target.Identity.Id,
            actor.Identity.Id,
            [new CharacterRelationshipDimensionValue(
                CharacterRelationshipDimension.Trust,
                priorTrust)],
            []);
        CharacterTargetSabotageInfluenceResult influenceResult = new(
            influenceCandidate,
            CharacterRelationshipChangeTransition.Apply(
                priorRelationship,
                influenceChange));
        CharacterRelationshipChange recoveryChange = RecoveryChange(
            influenceResult,
            -appliedDelta);

        return new RecoveryInput(
            priorRelationship,
            influenceResult,
            recoveryChange,
            new CharacterTargetSabotageInfluenceRecoveryCandidate(
                influenceResult,
                recoveryChange));
    }

    private static CharacterRelationshipChange RecoveryChange(
        RecoveryInput input,
        CharacterId? characterId = null,
        CharacterId? relatedCharacterId = null,
        int? delta = null,
        CharacterRelationshipDimension? dimension = null) =>
        new(
            characterId ?? input.InfluenceResult.Relationship.CharacterId,
            relatedCharacterId ?? input.InfluenceResult.Relationship.RelatedCharacterId,
            delta ?? -input.InfluenceResult.Candidate.RelationshipChange.Delta,
            CharacterRelationshipChangeTests.Reason(
                "influence-recovered",
                "seeker-recovery",
                dimension));

    private static CharacterRelationshipChange RecoveryChange(
        CharacterTargetSabotageInfluenceResult influenceResult,
        int delta) =>
        new(
            influenceResult.Relationship.CharacterId,
            influenceResult.Relationship.RelatedCharacterId,
            delta,
            CharacterRelationshipChangeTests.Reason(
                "influence-recovered",
                "seeker-recovery",
                influenceResult.Candidate.RelationshipChange.Reason.Dimension));

    private sealed record RecoveryInput(
        CharacterRelationshipMemory PriorRelationship,
        CharacterTargetSabotageInfluenceResult InfluenceResult,
        CharacterRelationshipChange RecoveryChange,
        CharacterTargetSabotageInfluenceRecoveryCandidate Candidate);
}
