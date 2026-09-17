using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterTargetSabotageInfluenceRecoveryDecisionTests
{
    [Fact]
    public void DecisionRetainsExactSelectionOrAbstention()
    {
        DecisionInput input = Input();

        CharacterTargetSabotageInfluenceRecoveryDecision selection = new(
            input.Actor.Identity.Id,
            input.Candidate);
        CharacterTargetSabotageInfluenceRecoveryDecision abstention = new(
            input.Actor.Identity.Id,
            null);

        Assert.Same(input.Actor.Identity.Id, selection.ActorId);
        Assert.Same(input.Candidate, selection.SelectedCandidate);
        Assert.Same(input.Actor.Identity.Id, abstention.ActorId);
        Assert.Null(abstention.SelectedCandidate);
        Assert.Same(input.InfluenceResult, selection.SelectedCandidate!.InfluenceResult);
        Assert.Same(input.RecoveryChange, selection.SelectedCandidate.RecoveryChange);
    }

    [Fact]
    public void PolicyAcceptsExactCurrentEligibleSelectionAndAbstention()
    {
        DecisionInput input = Input();
        CharacterTargetSabotageInfluenceRecoveryDecision selection = new(
            input.Actor.Identity.Id,
            input.Candidate);
        CharacterTargetSabotageInfluenceRecoveryDecision abstention = new(
            input.Actor.Identity.Id,
            null);

        Assert.True(CharacterTargetSabotageInfluenceRecoveryDecisionPolicy.CanAccept(
            input.Request,
            selection));
        Assert.True(CharacterTargetSabotageInfluenceRecoveryDecisionPolicy.CanAccept(
            input.Request,
            abstention));
        Assert.Same(input.Candidate, Assert.Single(input.Request.Candidates));
        Assert.Equal(11, input.Relationship.Dimensions.Single().Value);
        Assert.Same(input.InfluenceChange.Reason, input.Relationship.Reasons.Single());
    }

    [Fact]
    public void PolicyRejectsWrongOwnerActorForeignEquivalentAndIneligibleSelections()
    {
        DecisionInput input = Input();
        Character wrongOwner = RepresentativeHermitCharacterSource.Create()[0];
        CharacterTargetSabotageInfluenceRecoveryCandidate equivalent = new(
            input.InfluenceResult,
            input.RecoveryChange);
        CharacterRelationshipChange wrongDelta = new(
            input.RecoveryChange.CharacterId,
            input.RecoveryChange.RelatedCharacterId,
            input.InfluenceChange.Delta,
            CharacterRelationshipChangeTests.Reason("wrong-delta", "wrong-delta-turn"));
        CharacterTargetSabotageInfluenceRecoveryCandidate ineligible = new(
            input.InfluenceResult,
            wrongDelta);
        CharacterTargetSabotageInfluenceRecoveryConsiderationRequest ineligibleRequest = new(
            input.Actor,
            input.Relationship,
            [ineligible]);
        CharacterTargetSabotageInfluenceRecoveryConsiderationRequest wrongOwnerRequest = new(
            wrongOwner,
            input.Relationship,
            [input.Candidate]);

        Assert.False(CharacterTargetSabotageInfluenceRecoveryDecisionPolicy.CanAccept(
            input.Request,
            new CharacterTargetSabotageInfluenceRecoveryDecision(
                new CharacterId(input.Actor.Identity.Id.Value),
                input.Candidate)));
        Assert.False(CharacterTargetSabotageInfluenceRecoveryDecisionPolicy.CanAccept(
            wrongOwnerRequest,
            new CharacterTargetSabotageInfluenceRecoveryDecision(
                wrongOwner.Identity.Id,
                input.Candidate)));
        Assert.False(CharacterTargetSabotageInfluenceRecoveryDecisionPolicy.CanAccept(
            input.Request,
            new CharacterTargetSabotageInfluenceRecoveryDecision(
                input.Actor.Identity.Id,
                equivalent)));
        Assert.False(CharacterTargetSabotageInfluenceRecoveryDecisionPolicy.CanAccept(
            ineligibleRequest,
            new CharacterTargetSabotageInfluenceRecoveryDecision(
                input.Actor.Identity.Id,
                ineligible)));
        Assert.NotSame(input.Candidate, equivalent);
        Assert.NotEqual(input.Relationship.CharacterId, wrongOwner.Identity.Id);
        Assert.Same(input.InfluenceResult, equivalent.InfluenceResult);
        Assert.Same(input.RecoveryChange, equivalent.RecoveryChange);
    }

    [Fact]
    public void ResultRetainsExactAcceptedRequestDecisionAndSelection()
    {
        DecisionInput input = Input();
        CharacterTargetSabotageInfluenceRecoveryDecision decision = new(
            input.Actor.Identity.Id,
            input.Candidate);

        CharacterTargetSabotageInfluenceRecoveryConsiderationResult result = new(
            input.Request,
            decision);

        Assert.Same(input.Request, result.Request);
        Assert.Same(decision, result.Decision);
        Assert.Same(input.Candidate, result.SelectedCandidate);
        Assert.Same(input.Relationship, result.Request.Relationship);
    }

    [Fact]
    public void ResultRetainsAcceptedAbstentionWithoutSelection()
    {
        DecisionInput input = Input();
        CharacterTargetSabotageInfluenceRecoveryDecision decision = new(
            input.Actor.Identity.Id,
            null);

        CharacterTargetSabotageInfluenceRecoveryConsiderationResult result = new(
            input.Request,
            decision);

        Assert.Same(input.Request, result.Request);
        Assert.Same(decision, result.Decision);
        Assert.Null(result.SelectedCandidate);
        Assert.Same(input.Candidate, Assert.Single(result.Request.Candidates));
    }

    [Fact]
    public void ResultRejectsInvalidDecisionWithoutChangingRequestState()
    {
        DecisionInput input = Input();
        CharacterTargetSabotageInfluenceRecoveryCandidate equivalent = new(
            input.InfluenceResult,
            input.RecoveryChange);
        CharacterTargetSabotageInfluenceRecoveryDecision invalid = new(
            input.Actor.Identity.Id,
            equivalent);

        Assert.Throws<ArgumentException>(() =>
            new CharacterTargetSabotageInfluenceRecoveryConsiderationResult(
                input.Request,
                invalid));
        Assert.Same(input.Candidate, Assert.Single(input.Request.Candidates));
        Assert.Same(input.Relationship, input.Request.Relationship);
        Assert.Equal(11, input.Relationship.Dimensions.Single().Value);
    }

    [Fact]
    public void ContractsRejectNullsAndExposeOnlyClosedReadOnlySurfaces()
    {
        DecisionInput input = Input();
        CharacterTargetSabotageInfluenceRecoveryDecision decision = new(
            input.Actor.Identity.Id,
            input.Candidate);

        Assert.Throws<ArgumentNullException>(() =>
            new CharacterTargetSabotageInfluenceRecoveryDecision(null!, input.Candidate));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterTargetSabotageInfluenceRecoveryDecisionPolicy.CanAccept(null!, decision));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterTargetSabotageInfluenceRecoveryDecisionPolicy.CanAccept(
                input.Request,
                null!));
        Assert.Throws<ArgumentNullException>(() =>
            new CharacterTargetSabotageInfluenceRecoveryConsiderationResult(null!, decision));
        Assert.Throws<ArgumentNullException>(() =>
            new CharacterTargetSabotageInfluenceRecoveryConsiderationResult(
                input.Request,
                null!));

        AssertReadOnlySurface(
            typeof(CharacterTargetSabotageInfluenceRecoveryDecision),
            [
                ("ActorId", typeof(CharacterId)),
                ("SelectedCandidate", typeof(CharacterTargetSabotageInfluenceRecoveryCandidate)),
            ]);
        AssertReadOnlySurface(
            typeof(CharacterTargetSabotageInfluenceRecoveryConsiderationResult),
            [
                ("Decision", typeof(CharacterTargetSabotageInfluenceRecoveryDecision)),
                ("Request", typeof(CharacterTargetSabotageInfluenceRecoveryConsiderationRequest)),
                ("SelectedCandidate", typeof(CharacterTargetSabotageInfluenceRecoveryCandidate)),
            ]);

        Type policyType = typeof(CharacterTargetSabotageInfluenceRecoveryDecisionPolicy);
        System.Reflection.MethodInfo method = Assert.Single(policyType.GetMethods().Where(
            candidate => candidate.DeclaringType == policyType));
        Assert.Equal("CanAccept", method.Name);
        Assert.Equal(typeof(bool), method.ReturnType);
        Assert.Equal(
            [
                typeof(CharacterTargetSabotageInfluenceRecoveryConsiderationRequest),
                typeof(CharacterTargetSabotageInfluenceRecoveryDecision),
            ],
            method.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Empty(policyType.GetConstructors());
        Assert.True(policyType.IsAbstract && policyType.IsSealed);
    }

    private static void AssertReadOnlySurface(
        Type type,
        IReadOnlyList<(string Name, Type Type)> expectedProperties)
    {
        System.Reflection.PropertyInfo[] properties = type.GetProperties()
            .OrderBy(property => property.Name, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(
            expectedProperties,
            properties.Select(property => (property.Name, property.PropertyType)).ToArray());
        Assert.All(properties, property => Assert.False(property.CanWrite));
        Assert.Single(type.GetConstructors());
        Assert.Empty(type.GetEvents());
        Assert.True(type.IsSealed);
    }

    private static DecisionInput Input()
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

        return new DecisionInput(
            actor,
            relationship,
            influenceChange,
            influenceResult,
            recoveryChange,
            candidate,
            request);
    }

    private sealed record DecisionInput(
        Character Actor,
        CharacterRelationshipMemory Relationship,
        CharacterRelationshipChange InfluenceChange,
        CharacterTargetSabotageInfluenceResult InfluenceResult,
        CharacterRelationshipChange RecoveryChange,
        CharacterTargetSabotageInfluenceRecoveryCandidate Candidate,
        CharacterTargetSabotageInfluenceRecoveryConsiderationRequest Request);
}
