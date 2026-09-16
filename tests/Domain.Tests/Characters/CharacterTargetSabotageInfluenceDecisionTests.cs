using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterTargetSabotageInfluenceDecisionTests
{
    [Fact]
    public void DecisionRetainsExactSelectionOrAbstention()
    {
        DecisionInput input = Input();

        CharacterTargetSabotageInfluenceDecision selection = new(
            input.Actor.Identity.Id,
            input.Candidate);
        CharacterTargetSabotageInfluenceDecision abstention = new(
            input.Actor.Identity.Id,
            null);

        Assert.Same(input.Actor.Identity.Id, selection.ActorId);
        Assert.Same(input.Candidate, selection.SelectedCandidate);
        Assert.Same(input.Actor.Identity.Id, abstention.ActorId);
        Assert.Null(abstention.SelectedCandidate);
        CharacterTargetSabotageInfluenceCandidate selected = Assert.IsType<
            CharacterTargetSabotageInfluenceCandidate>(selection.SelectedCandidate);
        Assert.Same(input.Occurrence, selected.Occurrence);
        Assert.Same(input.Change, selected.RelationshipChange);
    }

    [Fact]
    public void PolicyAcceptsExactCurrentEligibleSelectionAndAbstention()
    {
        DecisionInput input = Input();
        CharacterTargetSabotageInfluenceDecision selection = new(
            input.Actor.Identity.Id,
            input.Candidate);
        CharacterTargetSabotageInfluenceDecision abstention = new(
            input.Actor.Identity.Id,
            null);

        Assert.True(CharacterTargetSabotageInfluenceDecisionPolicy.CanAccept(
            input.Request,
            selection));
        Assert.True(CharacterTargetSabotageInfluenceDecisionPolicy.CanAccept(
            input.Request,
            abstention));
        Assert.Same(input.Candidate, Assert.Single(input.Request.Candidates));
        Assert.Equal(10, input.Relationship.Dimensions.Single().Value);
        Assert.Empty(input.Relationship.Reasons);
    }

    [Fact]
    public void PolicyRejectsWrongActorForeignEquivalentAndIneligibleSelections()
    {
        DecisionInput input = Input();
        CharacterTargetSabotageInfluenceCandidate equivalent = new(
            input.Occurrence,
            input.Change);
        CharacterTargetSabotageOccurrence delayOccurrence = Occurrence(
            input.Actor,
            input.Target,
            CharacterSabotageActionKind.Delay);
        CharacterTargetSabotageInfluenceCandidate delay = new(
            delayOccurrence,
            Change(delayOccurrence, 2));
        CharacterTargetSabotageInfluenceConsiderationRequest requestWithDelay = new(
            input.Actor,
            input.InteractionAccess,
            input.Relationship,
            [delay]);

        Assert.False(CharacterTargetSabotageInfluenceDecisionPolicy.CanAccept(
            input.Request,
            new CharacterTargetSabotageInfluenceDecision(
                new CharacterId(input.Actor.Identity.Id.Value),
                input.Candidate)));
        Assert.False(CharacterTargetSabotageInfluenceDecisionPolicy.CanAccept(
            input.Request,
            new CharacterTargetSabotageInfluenceDecision(
                input.Actor.Identity.Id,
                equivalent)));
        Assert.False(CharacterTargetSabotageInfluenceDecisionPolicy.CanAccept(
            requestWithDelay,
            new CharacterTargetSabotageInfluenceDecision(
                input.Actor.Identity.Id,
                delay)));
        Assert.NotSame(input.Candidate, equivalent);
        Assert.Same(input.Occurrence, equivalent.Occurrence);
        Assert.Same(input.Change, equivalent.RelationshipChange);
    }

    [Fact]
    public void ResultRetainsExactAcceptedRequestDecisionAndSelection()
    {
        DecisionInput input = Input();
        CharacterTargetSabotageInfluenceDecision decision = new(
            input.Actor.Identity.Id,
            input.Candidate);

        CharacterTargetSabotageInfluenceConsiderationResult result = new(
            input.Request,
            decision);

        Assert.Same(input.Request, result.Request);
        Assert.Same(decision, result.Decision);
        Assert.Same(input.Candidate, result.SelectedCandidate);
        Assert.Same(input.Relationship, result.Request.Relationship);
        Assert.Same(input.InteractionAccess, result.Request.InteractionAccess);
    }

    [Fact]
    public void ResultRetainsAcceptedAbstentionWithoutSelection()
    {
        DecisionInput input = Input();
        CharacterTargetSabotageInfluenceDecision decision = new(
            input.Actor.Identity.Id,
            null);

        CharacterTargetSabotageInfluenceConsiderationResult result = new(
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
        CharacterTargetSabotageInfluenceCandidate equivalent = new(
            input.Occurrence,
            input.Change);
        CharacterTargetSabotageInfluenceDecision invalid = new(
            input.Actor.Identity.Id,
            equivalent);

        Assert.Throws<ArgumentException>(() =>
            new CharacterTargetSabotageInfluenceConsiderationResult(
                input.Request,
                invalid));
        Assert.Same(input.Candidate, Assert.Single(input.Request.Candidates));
        Assert.Same(input.Relationship, input.Request.Relationship);
        Assert.Empty(input.Relationship.Reasons);
    }

    [Fact]
    public void ContractsRejectNullsAndExposeOnlyClosedReadOnlySurfaces()
    {
        DecisionInput input = Input();
        CharacterTargetSabotageInfluenceDecision decision = new(
            input.Actor.Identity.Id,
            input.Candidate);

        Assert.Throws<ArgumentNullException>(() =>
            new CharacterTargetSabotageInfluenceDecision(null!, input.Candidate));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterTargetSabotageInfluenceDecisionPolicy.CanAccept(null!, decision));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterTargetSabotageInfluenceDecisionPolicy.CanAccept(input.Request, null!));
        Assert.Throws<ArgumentNullException>(() =>
            new CharacterTargetSabotageInfluenceConsiderationResult(null!, decision));
        Assert.Throws<ArgumentNullException>(() =>
            new CharacterTargetSabotageInfluenceConsiderationResult(input.Request, null!));

        AssertReadOnlySurface(
            typeof(CharacterTargetSabotageInfluenceDecision),
            [
                ("ActorId", typeof(CharacterId)),
                ("SelectedCandidate", typeof(CharacterTargetSabotageInfluenceCandidate)),
            ]);
        AssertReadOnlySurface(
            typeof(CharacterTargetSabotageInfluenceConsiderationResult),
            [
                ("Decision", typeof(CharacterTargetSabotageInfluenceDecision)),
                ("Request", typeof(CharacterTargetSabotageInfluenceConsiderationRequest)),
                ("SelectedCandidate", typeof(CharacterTargetSabotageInfluenceCandidate)),
            ]);

        Type policyType = typeof(CharacterTargetSabotageInfluenceDecisionPolicy);
        System.Reflection.MethodInfo method = Assert.Single(policyType.GetMethods().Where(
            candidate => candidate.DeclaringType == policyType));
        Assert.Equal("CanAccept", method.Name);
        Assert.Equal(typeof(bool), method.ReturnType);
        Assert.Equal(
            [
                typeof(CharacterTargetSabotageInfluenceConsiderationRequest),
                typeof(CharacterTargetSabotageInfluenceDecision),
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
        Character actor = RepresentativeEnemyAgentCharacterSource.Create();
        Character target = RepresentativeSageCharacterSource.Create();
        CharacterTargetSabotageOccurrence occurrence = Occurrence(actor, target);
        CharacterRelationshipChange change = Change(occurrence, 1);
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

        return new(
            actor,
            target,
            interactionAccess,
            relationship,
            occurrence,
            change,
            candidate,
            request);
    }

    private static CharacterTargetSabotageOccurrence Occurrence(
        Character actor,
        Character target,
        CharacterSabotageActionKind? actionKind = null) =>
        new(
            new CharacterTargetSabotageProposal(
                actor.Identity.Id,
                new CharacterTargetAccessRequirement(
                    target.Identity.Id,
                    "assess-local-influence",
                    actor.Knowledge.KnowledgeReferences.Single()),
                actionKind ?? CharacterSabotageActionKind.Influence),
            target);

    private static CharacterRelationshipChange Change(
        CharacterTargetSabotageOccurrence occurrence,
        int ordinal) =>
        new(
            occurrence.Target.Identity.Id,
            occurrence.Proposal.ActorId,
            1,
            CharacterRelationshipChangeTests.Reason(
                $"influence-attempt-{ordinal}",
                $"enemy-agent-influence-{ordinal}"));

    private sealed record DecisionInput(
        Character Actor,
        Character Target,
        CharacterInteractionAccessSnapshot InteractionAccess,
        CharacterRelationshipMemory Relationship,
        CharacterTargetSabotageOccurrence Occurrence,
        CharacterRelationshipChange Change,
        CharacterTargetSabotageInfluenceCandidate Candidate,
        CharacterTargetSabotageInfluenceConsiderationRequest Request);
}
