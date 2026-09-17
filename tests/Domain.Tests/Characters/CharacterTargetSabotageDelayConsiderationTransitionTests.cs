using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterTargetSabotageDelayConsiderationTransitionTests
{
    [Fact]
    public void ApplyReturnsFreshDelayResultForExactAcceptedSelection()
    {
        TransitionInput input = Input();
        CharacterTargetSabotageDelayConsiderationResult consideration = Result(
            input,
            input.Candidate);

        CharacterTargetSabotageDelayResult? result =
            CharacterTargetSabotageDelayConsiderationTransition.Apply(consideration);

        CharacterTargetSabotageDelayResult applied = Assert.IsType<
            CharacterTargetSabotageDelayResult>(result);
        Assert.Same(input.Candidate, applied.Candidate);
        Assert.NotSame(input.EventLog, applied.EventLog);
        Assert.NotSame(input.WorldTime, applied.WorldTime);
        Assert.Same(input.WorldEvent.EventId, Assert.Single(applied.EventLog.Entries));
        Assert.Same(input.WorldEvent.EventId, applied.WorldTime.CurrentEventId);
        Assert.Empty(input.EventLog.Entries);
        Assert.Null(input.WorldTime.CurrentEventId);
    }

    [Fact]
    public void ApplyReturnsNullForAcceptedAbstentionWithoutChangingInputs()
    {
        TransitionInput input = Input();
        CharacterTargetSabotageDelayConsiderationResult consideration = Result(input, null);

        CharacterTargetSabotageDelayResult? result =
            CharacterTargetSabotageDelayConsiderationTransition.Apply(consideration);

        Assert.Null(result);
        Assert.Same(input.Request, consideration.Request);
        Assert.Null(consideration.SelectedCandidate);
        Assert.Same(input.Candidate, Assert.Single(input.Request.Candidates));
        Assert.Empty(input.EventLog.Entries);
        Assert.Null(input.WorldTime.CurrentEventId);
    }

    [Fact]
    public void RejectsNullAndExposesOnlyOneClosedPureOperation()
    {
        Assert.Throws<ArgumentNullException>(() =>
            CharacterTargetSabotageDelayConsiderationTransition.Apply(null!));

        Type transitionType = typeof(CharacterTargetSabotageDelayConsiderationTransition);
        System.Reflection.MethodInfo method = Assert.Single(transitionType.GetMethods().Where(
            candidate => candidate.DeclaringType == transitionType));
        Assert.Equal("Apply", method.Name);
        Assert.Equal(typeof(CharacterTargetSabotageDelayResult), method.ReturnType);
        Assert.Equal(
            [typeof(CharacterTargetSabotageDelayConsiderationResult)],
            method.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Empty(transitionType.GetConstructors());
        Assert.True(transitionType.IsAbstract && transitionType.IsSealed);
    }

    private static CharacterTargetSabotageDelayConsiderationResult Result(
        TransitionInput input,
        CharacterTargetSabotageDelayCandidate? selectedCandidate) =>
        new(
            input.Request,
            new CharacterTargetSabotageDelayDecision(
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
                CharacterSabotageActionKind.Delay),
            target);
        WorldEvent worldEvent = new(
            new WorldEventId("enemy-agent-delayed-sage"),
            WorldEventTimeEffect.Advance);
        CharacterTargetSabotageDelayCandidate candidate = new(occurrence, worldEvent);
        CharacterInteractionAccessSnapshot interactionAccess = new(
            actor.Identity.Id,
            [target.Identity.Id]);
        WorldEventSnapshot worldEvents = new([worldEvent]);
        WorldEventLogSnapshot eventLog = new([]);
        WorldTimeSnapshot worldTime = new(null);
        CharacterTargetSabotageDelayConsiderationRequest request = new(
            actor,
            interactionAccess,
            worldEvents,
            eventLog,
            worldTime,
            [candidate]);

        return new(actor, worldEvent, eventLog, worldTime, candidate, request);
    }

    private sealed record TransitionInput(
        Character Actor,
        WorldEvent WorldEvent,
        WorldEventLogSnapshot EventLog,
        WorldTimeSnapshot WorldTime,
        CharacterTargetSabotageDelayCandidate Candidate,
        CharacterTargetSabotageDelayConsiderationRequest Request);
}
