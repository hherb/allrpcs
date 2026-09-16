using Allrpcs.Domain.Characters;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterTargetSabotageInfluenceConsiderationTransitionTests
{
    [Fact]
    public void ApplyReturnsFreshInfluenceResultForExactAcceptedSelection()
    {
        TransitionInput input = Input();
        CharacterTargetSabotageInfluenceConsiderationResult consideration = Result(
            input,
            input.Candidate);

        CharacterTargetSabotageInfluenceResult? result =
            CharacterTargetSabotageInfluenceConsiderationTransition.Apply(consideration);

        CharacterTargetSabotageInfluenceResult applied = Assert.IsType<
            CharacterTargetSabotageInfluenceResult>(result);
        Assert.Same(input.Candidate, applied.Candidate);
        Assert.NotSame(input.Relationship, applied.Relationship);
        Assert.Same(input.Target.Identity.Id, applied.Relationship.CharacterId);
        Assert.Same(input.Actor.Identity.Id, applied.Relationship.RelatedCharacterId);
        Assert.Equal(8, applied.Relationship.Dimensions.Single().Value);
        Assert.Same(input.Change.Reason, Assert.Single(applied.Relationship.Reasons));
        Assert.Equal(10, input.Relationship.Dimensions.Single().Value);
        Assert.Empty(input.Relationship.Reasons);
    }

    [Fact]
    public void ApplyReturnsNullForAcceptedAbstentionWithoutChangingInputs()
    {
        TransitionInput input = Input();
        CharacterTargetSabotageInfluenceConsiderationResult consideration = Result(input, null);

        CharacterTargetSabotageInfluenceResult? result =
            CharacterTargetSabotageInfluenceConsiderationTransition.Apply(consideration);

        Assert.Null(result);
        Assert.Same(input.Request, consideration.Request);
        Assert.Null(consideration.SelectedCandidate);
        Assert.Same(input.Candidate, Assert.Single(input.Request.Candidates));
        Assert.Equal(10, input.Relationship.Dimensions.Single().Value);
        Assert.Empty(input.Relationship.Reasons);
    }

    [Fact]
    public void RejectsNullAndExposesOnlyOneClosedPureOperation()
    {
        Assert.Throws<ArgumentNullException>(() =>
            CharacterTargetSabotageInfluenceConsiderationTransition.Apply(null!));

        Type transitionType = typeof(CharacterTargetSabotageInfluenceConsiderationTransition);
        System.Reflection.MethodInfo method = Assert.Single(transitionType.GetMethods().Where(
            candidate => candidate.DeclaringType == transitionType));
        Assert.Equal("Apply", method.Name);
        Assert.Equal(typeof(CharacterTargetSabotageInfluenceResult), method.ReturnType);
        Assert.Equal(
            [typeof(CharacterTargetSabotageInfluenceConsiderationResult)],
            method.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Empty(transitionType.GetConstructors());
        Assert.True(transitionType.IsAbstract && transitionType.IsSealed);
    }

    private static CharacterTargetSabotageInfluenceConsiderationResult Result(
        TransitionInput input,
        CharacterTargetSabotageInfluenceCandidate? selectedCandidate) =>
        new(
            input.Request,
            new CharacterTargetSabotageInfluenceDecision(
                input.Actor.Identity.Id,
                selectedCandidate));

    private static TransitionInput Input()
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
        CharacterRelationshipChange change = new(
            target.Identity.Id,
            actor.Identity.Id,
            -2,
            CharacterRelationshipChangeTests.Reason(
                "influence-attempt",
                "enemy-agent-influence"));
        CharacterTargetSabotageInfluenceCandidate candidate = new(occurrence, change);
        CharacterInteractionAccessSnapshot interactionAccess = new(
            actor.Identity.Id,
            [target.Identity.Id]);
        CharacterRelationshipMemory relationship = new(
            target.Identity.Id,
            actor.Identity.Id,
            [new CharacterRelationshipDimensionValue(CharacterRelationshipDimension.Trust, 10)],
            []);
        CharacterTargetSabotageInfluenceConsiderationRequest request = new(
            actor,
            interactionAccess,
            relationship,
            [candidate]);

        return new(actor, target, relationship, change, candidate, request);
    }

    private sealed record TransitionInput(
        Character Actor,
        Character Target,
        CharacterRelationshipMemory Relationship,
        CharacterRelationshipChange Change,
        CharacterTargetSabotageInfluenceCandidate Candidate,
        CharacterTargetSabotageInfluenceConsiderationRequest Request);
}
