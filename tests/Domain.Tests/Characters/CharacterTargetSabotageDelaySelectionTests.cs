using System.Collections;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterTargetSabotageDelaySelectionTests
{
    [Fact]
    public void SelectsExactFirstEligibleCandidateInStableOrder()
    {
        SelectionInput input = Input();
        CharacterTargetSabotageDelayCandidate second = Candidate(input.Occurrence, 2);
        CharacterTargetSabotageDelayCandidate[] candidates = [input.Candidate, second];

        CharacterTargetSabotageDelayCandidate? selected = Select(input, candidates);

        Assert.Same(input.Candidate, selected);
        Assert.Collection(
            candidates,
            candidate => Assert.Same(input.Candidate, candidate),
            candidate => Assert.Same(second, candidate));
        Assert.Same(input.Occurrence, input.Candidate.Occurrence);
        Assert.Same(input.WorldEvent, input.Candidate.WorldEvent);
        Assert.Empty(input.EventLog.Entries);
        Assert.Null(input.WorldTime.CurrentEventId);
    }

    [Fact]
    public void SkipsIneligibleCandidatesBeforeLaterEligibleCandidate()
    {
        SelectionInput input = Input();
        CharacterTargetSabotageDelayCandidate influence = Candidate(
            Occurrence(input.Actor, input.Target, CharacterSabotageActionKind.Influence));
        WorldEvent noAdvanceEvent = Event(2, WorldEventTimeEffect.NoAdvance);
        CharacterTargetSabotageDelayCandidate noAdvance =
            new(input.Occurrence, noAdvanceEvent);
        CharacterTargetSabotageDelayCandidate rebuilt =
            new(input.Occurrence, Event());
        CharacterTargetSabotageDelayCandidate eligible = Candidate(input.Occurrence, 3);
        WorldEventSnapshot worldEvents = new(
            [input.WorldEvent, noAdvanceEvent, eligible.WorldEvent]);

        CharacterTargetSabotageDelayCandidate? selected =
            CharacterTargetSabotageDelaySelection.SelectFirstEligible(
                input.Actor,
                input.InteractionAccess,
                worldEvents,
                input.EventLog,
                input.WorldTime,
                [influence, noAdvance, rebuilt, eligible]);

        Assert.Same(eligible, selected);
        Assert.Same(input.Occurrence, eligible.Occurrence);
        Assert.Same(eligible.WorldEvent, worldEvents.Entries[^1]);
    }

    [Fact]
    public void ReturnsNullWhenNoCandidateIsEligibleWithoutChangingInputs()
    {
        SelectionInput input = Input();
        CharacterInteractionAccessSnapshot inaccessible = new(input.Actor.Identity.Id, []);

        CharacterTargetSabotageDelayCandidate? selected =
            CharacterTargetSabotageDelaySelection.SelectFirstEligible(
                input.Actor,
                inaccessible,
                input.WorldEvents,
                input.EventLog,
                input.WorldTime,
                [input.Candidate]);

        Assert.Null(selected);
        Assert.Empty(inaccessible.ReachableTargetCharacterIds);
        Assert.Same(input.Occurrence, input.Candidate.Occurrence);
        Assert.Same(input.WorldEvent, input.Candidate.WorldEvent);
        Assert.Empty(input.EventLog.Entries);
        Assert.Null(input.WorldTime.CurrentEventId);
    }

    [Fact]
    public void ValidatesCompleteCollectionBeforeSelecting()
    {
        SelectionInput input = Input();

        Assert.Throws<ArgumentException>(() => Select(input, [input.Candidate, null!]));
        Assert.Same(input.Occurrence, input.Candidate.Occurrence);
        Assert.Same(input.WorldEvent, input.Candidate.WorldEvent);
    }

    [Fact]
    public void RejectsDuplicateExactReferencesButAcceptsDistinctEquivalentCandidates()
    {
        SelectionInput input = Input();
        CharacterTargetSabotageDelayCandidate equivalent = new(
            input.Occurrence,
            input.WorldEvent);

        Assert.Throws<ArgumentException>(() => Select(
            input,
            [input.Candidate, input.Candidate]));
        Assert.Same(input.Candidate, Select(input, [input.Candidate, equivalent]));
        Assert.NotSame(input.Candidate, equivalent);
        Assert.Same(input.Occurrence, equivalent.Occurrence);
        Assert.Same(input.WorldEvent, equivalent.WorldEvent);
    }

    [Fact]
    public void MaterializesOnePassEnumerableOnceAndEnforcesMaximumCount()
    {
        SelectionInput input = Input();
        CharacterTargetSabotageDelayCandidate[] maximum = Enumerable
            .Range(1, CharacterTargetSabotageDelaySelection.MaximumCandidateCount)
            .Select(index => Candidate(input.Occurrence, index))
            .ToArray();
        WorldEventSnapshot worldEvents = new(maximum.Select(candidate => candidate.WorldEvent));
        SelectionInput bounded = input with { WorldEvents = worldEvents };
        CharacterTargetSabotageDelayCandidate[] excessive = maximum
            .Append(Candidate(input.Occurrence, maximum.Length + 1))
            .ToArray();
        SingleUseEnumerable<CharacterTargetSabotageDelayCandidate> onePass = new(maximum);

        Assert.Same(maximum[0], Select(bounded, onePass));
        Assert.Equal(1, onePass.EnumerationCount);
        Assert.Throws<ArgumentOutOfRangeException>(() => Select(bounded, excessive));
        Assert.Equal(CharacterTargetSabotageDelaySelection.MaximumCandidateCount, maximum.Length);
    }

    [Fact]
    public void RejectsNullInputsAndExposesOneClosedPureOperation()
    {
        SelectionInput input = Input();

        Assert.Throws<ArgumentNullException>(() => Select(input with { Actor = null! }, []));
        Assert.Throws<ArgumentNullException>(() =>
            Select(input with { InteractionAccess = null! }, []));
        Assert.Throws<ArgumentNullException>(() => Select(input with { WorldEvents = null! }, []));
        Assert.Throws<ArgumentNullException>(() => Select(input with { EventLog = null! }, []));
        Assert.Throws<ArgumentNullException>(() => Select(input with { WorldTime = null! }, []));
        Assert.Throws<ArgumentNullException>(() => Select(input, null!));

        Type selectionType = typeof(CharacterTargetSabotageDelaySelection);
        System.Reflection.MethodInfo method = Assert.Single(selectionType.GetMethods().Where(
            candidate => candidate.DeclaringType == selectionType));
        Assert.Equal("SelectFirstEligible", method.Name);
        Assert.Equal(typeof(CharacterTargetSabotageDelayCandidate), method.ReturnType);
        Assert.Equal(
            [
                typeof(Character),
                typeof(CharacterInteractionAccessSnapshot),
                typeof(WorldEventSnapshot),
                typeof(WorldEventLogSnapshot),
                typeof(WorldTimeSnapshot),
                typeof(IEnumerable<CharacterTargetSabotageDelayCandidate>),
            ],
            method.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Empty(selectionType.GetConstructors());
        Assert.True(selectionType.IsAbstract && selectionType.IsSealed);
    }

    private static CharacterTargetSabotageDelayCandidate? Select(
        SelectionInput input,
        IEnumerable<CharacterTargetSabotageDelayCandidate> candidates) =>
        CharacterTargetSabotageDelaySelection.SelectFirstEligible(
            input.Actor,
            input.InteractionAccess,
            input.WorldEvents,
            input.EventLog,
            input.WorldTime,
            candidates);

    private static SelectionInput Input()
    {
        Character actor = RepresentativeEnemyAgentCharacterSource.Create();
        Character target = RepresentativeSageCharacterSource.Create();
        CharacterTargetSabotageOccurrence occurrence = Occurrence(actor, target);
        WorldEvent worldEvent = Event();
        return new(
            actor,
            target,
            new CharacterInteractionAccessSnapshot(actor.Identity.Id, [target.Identity.Id]),
            new WorldEventSnapshot([worldEvent]),
            new WorldEventLogSnapshot([]),
            new WorldTimeSnapshot(null),
            occurrence,
            worldEvent,
            new CharacterTargetSabotageDelayCandidate(occurrence, worldEvent));
    }

    private static CharacterTargetSabotageDelayCandidate Candidate(
        CharacterTargetSabotageOccurrence occurrence,
        int ordinal = 1) =>
        new(occurrence, Event(ordinal));

    private static WorldEvent Event(
        int ordinal = 1,
        WorldEventTimeEffect? timeEffect = null) =>
        new(
            new WorldEventId($"enemy-agent-delayed-sage-{ordinal}"),
            timeEffect ?? WorldEventTimeEffect.Advance);

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
                actionKind ?? CharacterSabotageActionKind.Delay),
            target);

    private sealed record SelectionInput(
        Character Actor,
        Character Target,
        CharacterInteractionAccessSnapshot InteractionAccess,
        WorldEventSnapshot WorldEvents,
        WorldEventLogSnapshot EventLog,
        WorldTimeSnapshot WorldTime,
        CharacterTargetSabotageOccurrence Occurrence,
        WorldEvent WorldEvent,
        CharacterTargetSabotageDelayCandidate Candidate);

    private sealed class SingleUseEnumerable<T>(IEnumerable<T> values) : IEnumerable<T>
    {
        public int EnumerationCount { get; private set; }

        public IEnumerator<T> GetEnumerator()
        {
            EnumerationCount++;
            if (EnumerationCount > 1)
            {
                throw new InvalidOperationException(
                    "The candidate source was enumerated more than once.");
            }

            return values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
