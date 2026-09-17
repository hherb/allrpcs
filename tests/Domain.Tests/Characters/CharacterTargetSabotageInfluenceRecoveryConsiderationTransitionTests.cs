using Allrpcs.Domain.Characters;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterTargetSabotageInfluenceRecoveryConsiderationTransitionTests
{
    [Fact]
    public void ApplyReturnsFreshRecoveryResultForExactAcceptedSelection()
    {
        TransitionInput input = Input();
        CharacterTargetSabotageInfluenceRecoveryConsiderationResult consideration =
            Result(input, input.Candidate);

        CharacterTargetSabotageInfluenceRecoveryResult? result =
            CharacterTargetSabotageInfluenceRecoveryConsiderationTransition.Apply(
                consideration);

        CharacterTargetSabotageInfluenceRecoveryResult recovered = Assert.IsType<
            CharacterTargetSabotageInfluenceRecoveryResult>(result);
        Assert.Same(input.Candidate, recovered.Candidate);
        Assert.NotSame(input.Relationship, recovered.Relationship);
        Assert.Same(input.Actor.Identity.Id, recovered.Relationship.CharacterId);
        Assert.Same(input.Enemy.Identity.Id, recovered.Relationship.RelatedCharacterId);
        Assert.Equal(10, recovered.Relationship.Dimensions.Single().Value);
        Assert.Collection(
            recovered.Relationship.Reasons,
            reason => Assert.Same(input.InfluenceChange.Reason, reason),
            reason => Assert.Same(input.RecoveryChange.Reason, reason));
        Assert.Equal(11, input.Relationship.Dimensions.Single().Value);
        Assert.Same(input.InfluenceChange.Reason, input.Relationship.Reasons.Single());
    }

    [Fact]
    public void ApplyReturnsNullForAcceptedAbstentionWithoutChangingInputs()
    {
        TransitionInput input = Input();
        CharacterTargetSabotageInfluenceRecoveryConsiderationResult consideration =
            Result(input, null);

        CharacterTargetSabotageInfluenceRecoveryResult? result =
            CharacterTargetSabotageInfluenceRecoveryConsiderationTransition.Apply(
                consideration);

        Assert.Null(result);
        Assert.Same(input.Request, consideration.Request);
        Assert.Null(consideration.SelectedCandidate);
        Assert.Same(input.Candidate, Assert.Single(input.Request.Candidates));
        Assert.Equal(11, input.Relationship.Dimensions.Single().Value);
        Assert.Same(input.InfluenceChange.Reason, input.Relationship.Reasons.Single());
    }

    [Fact]
    public void RejectsNullAndExposesOnlyOneClosedPureOperation()
    {
        Assert.Throws<ArgumentNullException>(() =>
            CharacterTargetSabotageInfluenceRecoveryConsiderationTransition.Apply(null!));

        Type transitionType = typeof(
            CharacterTargetSabotageInfluenceRecoveryConsiderationTransition);
        System.Reflection.MethodInfo method = Assert.Single(transitionType.GetMethods().Where(
            candidate => candidate.DeclaringType == transitionType));
        Assert.Equal("Apply", method.Name);
        Assert.Equal(typeof(CharacterTargetSabotageInfluenceRecoveryResult), method.ReturnType);
        Assert.Equal(
            [typeof(CharacterTargetSabotageInfluenceRecoveryConsiderationResult)],
            method.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Empty(transitionType.GetConstructors());
        Assert.True(transitionType.IsAbstract && transitionType.IsSealed);
    }

    private static CharacterTargetSabotageInfluenceRecoveryConsiderationResult Result(
        TransitionInput input,
        CharacterTargetSabotageInfluenceRecoveryCandidate? selectedCandidate) =>
        new(
            input.Request,
            new CharacterTargetSabotageInfluenceRecoveryDecision(
                input.Actor.Identity.Id,
                selectedCandidate));

    private static TransitionInput Input()
    {
        Character enemy = RepresentativeEnemyAgentCharacterSource.Create();
        Character actor = RepresentativeSageCharacterSource.Create();
        CharacterTargetSabotageOccurrence occurrence = new(
            new CharacterTargetSabotageProposal(
                enemy.Identity.Id,
                new CharacterTargetAccessRequirement(
                    actor.Identity.Id,
                    "assess-local-influence",
                    enemy.Knowledge.KnowledgeReferences.Single()),
                CharacterSabotageActionKind.Influence),
            actor);
        CharacterRelationshipChange influenceChange = new(
            actor.Identity.Id,
            enemy.Identity.Id,
            1,
            CharacterRelationshipChangeTests.Reason(
                "influence-attempt",
                "enemy-agent-influence"));
        CharacterTargetSabotageInfluenceCandidate influenceCandidate = new(
            occurrence,
            influenceChange);
        CharacterRelationshipMemory priorRelationship = new(
            actor.Identity.Id,
            enemy.Identity.Id,
            [new CharacterRelationshipDimensionValue(CharacterRelationshipDimension.Trust, 10)],
            []);
        CharacterRelationshipMemory relationship =
            CharacterRelationshipChangeTransition.Apply(priorRelationship, influenceChange);
        CharacterTargetSabotageInfluenceResult influenceResult = new(
            influenceCandidate,
            relationship);
        CharacterRelationshipChange recoveryChange = new(
            actor.Identity.Id,
            enemy.Identity.Id,
            -1,
            CharacterRelationshipChangeTests.Reason(
                "influence-recovered",
                "sage-recovery"));
        CharacterTargetSabotageInfluenceRecoveryCandidate candidate = new(
            influenceResult,
            recoveryChange);
        CharacterTargetSabotageInfluenceRecoveryConsiderationRequest request = new(
            actor,
            relationship,
            [candidate]);

        return new TransitionInput(
            enemy,
            actor,
            relationship,
            influenceChange,
            recoveryChange,
            candidate,
            request);
    }

    private sealed record TransitionInput(
        Character Enemy,
        Character Actor,
        CharacterRelationshipMemory Relationship,
        CharacterRelationshipChange InfluenceChange,
        CharacterRelationshipChange RecoveryChange,
        CharacterTargetSabotageInfluenceRecoveryCandidate Candidate,
        CharacterTargetSabotageInfluenceRecoveryConsiderationRequest Request);
}
