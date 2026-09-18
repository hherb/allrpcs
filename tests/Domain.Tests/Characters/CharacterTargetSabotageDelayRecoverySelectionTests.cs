using System.Collections;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterTargetSabotageDelayRecoverySelectionTests
{
    [Fact]
    public void SelectsExactFirstEligibleCandidateInStableOrderWithoutMutation()
    {
        SelectionInput input = Input();
        CharacterTargetSabotageDelayRecoveryCandidate second = EquivalentCandidate(input);
        CharacterTargetSabotageDelayRecoveryCandidate[] candidates = [input.Candidate, second];

        CharacterTargetSabotageDelayRecoveryCandidate? selected = Select(input, candidates);

        Assert.Same(input.Candidate, selected);
        Assert.Collection(
            candidates,
            candidate => Assert.Same(input.Candidate, candidate),
            candidate => Assert.Same(second, candidate));
        Assert.Same(input.DelayResult, input.Candidate.DelayResult);
        Assert.Same(input.RecoveryEvent, input.Candidate.RecoveryEvent);
        Assert.Same(input.DelayEvent.EventId, input.EventLog.Entries[^1]);
        Assert.Same(input.DelayEvent.EventId, input.WorldTime.CurrentEventId);
    }

    [Fact]
    public void SkipsIneligibleCandidateBeforeLaterEligibleCandidate()
    {
        SelectionInput input = Input();
        WorldEvent noAdvanceEvent = new(
            new WorldEventId("sage-delay-not-recovered"),
            WorldEventTimeEffect.NoAdvance);
        CharacterTargetSabotageDelayRecoveryCandidate ineligible = new(
            input.DelayResult,
            noAdvanceEvent);
        CharacterTargetSabotageDelayRecoveryCandidate eligible = EquivalentCandidate(input);
        WorldEventSnapshot worldEvents = new(
            [input.PriorEvent, input.DelayEvent, noAdvanceEvent, input.RecoveryEvent]);

        CharacterTargetSabotageDelayRecoveryCandidate? selected =
            CharacterTargetSabotageDelayRecoverySelection.SelectFirstEligible(
                worldEvents,
                input.EventLog,
                input.WorldTime,
                [ineligible, eligible]);

        Assert.Same(eligible, selected);
        Assert.Same(input.DelayResult, eligible.DelayResult);
        Assert.Same(input.RecoveryEvent, eligible.RecoveryEvent);
    }

    [Fact]
    public void ReturnsNullWhenNoCandidateIsEligibleWithoutChangingInputs()
    {
        SelectionInput input = Input();
        WorldEventLogSnapshot staleLog = new([input.PriorEvent.EventId]);
        WorldTimeSnapshot staleTime = new(input.PriorEvent.EventId);

        CharacterTargetSabotageDelayRecoveryCandidate? selected =
            CharacterTargetSabotageDelayRecoverySelection.SelectFirstEligible(
                input.WorldEvents,
                staleLog,
                staleTime,
                [input.Candidate]);

        Assert.Null(selected);
        Assert.Same(input.PriorEvent.EventId, staleLog.Entries.Single());
        Assert.Same(input.PriorEvent.EventId, staleTime.CurrentEventId);
        Assert.Same(input.DelayResult, input.Candidate.DelayResult);
        Assert.Same(input.RecoveryEvent, input.Candidate.RecoveryEvent);
    }

    [Fact]
    public void ValidatesCompleteCollectionBeforeSelecting()
    {
        SelectionInput input = Input();

        Assert.Throws<ArgumentException>(() => Select(input, [input.Candidate, null!]));
        Assert.Same(input.DelayEvent.EventId, input.EventLog.Entries[^1]);
        Assert.Same(input.DelayEvent.EventId, input.WorldTime.CurrentEventId);
    }

    [Fact]
    public void RejectsDuplicateExactReferencesButAcceptsDistinctEquivalentCandidates()
    {
        SelectionInput input = Input();
        CharacterTargetSabotageDelayRecoveryCandidate equivalent = EquivalentCandidate(input);

        Assert.Throws<ArgumentException>(() =>
            Select(input, [input.Candidate, input.Candidate]));
        Assert.Same(input.Candidate, Select(input, [input.Candidate, equivalent]));
        Assert.NotSame(input.Candidate, equivalent);
        Assert.Same(input.DelayResult, equivalent.DelayResult);
        Assert.Same(input.RecoveryEvent, equivalent.RecoveryEvent);
    }

    [Fact]
    public void MaterializesOnePassEnumerableOnceAndEnforcesMaximumCount()
    {
        SelectionInput input = Input();
        CharacterTargetSabotageDelayRecoveryCandidate[] maximum = Enumerable
            .Range(0, CharacterTargetSabotageDelayRecoverySelection.MaximumCandidateCount)
            .Select(_ => EquivalentCandidate(input))
            .ToArray();
        CharacterTargetSabotageDelayRecoveryCandidate[] excessive = maximum
            .Append(EquivalentCandidate(input))
            .ToArray();
        SingleUseEnumerable<CharacterTargetSabotageDelayRecoveryCandidate> onePass =
            new(maximum);

        Assert.Same(maximum[0], Select(input, onePass));
        Assert.Equal(1, onePass.EnumerationCount);
        Assert.Throws<ArgumentOutOfRangeException>(() => Select(input, excessive));
        Assert.Equal(
            CharacterTargetSabotageDelayRecoverySelection.MaximumCandidateCount,
            maximum.Length);
    }

    [Fact]
    public void RejectsNullInputsAndExposesOneClosedPureOperation()
    {
        SelectionInput input = Input();

        Assert.Throws<ArgumentNullException>(() =>
            CharacterTargetSabotageDelayRecoverySelection.SelectFirstEligible(
                null!, input.EventLog, input.WorldTime, [input.Candidate]));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterTargetSabotageDelayRecoverySelection.SelectFirstEligible(
                input.WorldEvents, null!, input.WorldTime, [input.Candidate]));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterTargetSabotageDelayRecoverySelection.SelectFirstEligible(
                input.WorldEvents, input.EventLog, null!, [input.Candidate]));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterTargetSabotageDelayRecoverySelection.SelectFirstEligible(
                input.WorldEvents, input.EventLog, input.WorldTime, null!));

        Type selectionType = typeof(CharacterTargetSabotageDelayRecoverySelection);
        System.Reflection.MethodInfo method = Assert.Single(selectionType.GetMethods().Where(
            candidate => candidate.DeclaringType == selectionType));
        Assert.Equal("SelectFirstEligible", method.Name);
        Assert.Equal(typeof(CharacterTargetSabotageDelayRecoveryCandidate), method.ReturnType);
        Assert.Equal(
            [
                typeof(WorldEventSnapshot),
                typeof(WorldEventLogSnapshot),
                typeof(WorldTimeSnapshot),
                typeof(IEnumerable<CharacterTargetSabotageDelayRecoveryCandidate>),
            ],
            method.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Empty(selectionType.GetConstructors());
        Assert.True(selectionType.IsAbstract && selectionType.IsSealed);
    }

    private static CharacterTargetSabotageDelayRecoveryCandidate? Select(
        SelectionInput input,
        IEnumerable<CharacterTargetSabotageDelayRecoveryCandidate> candidates) =>
        CharacterTargetSabotageDelayRecoverySelection.SelectFirstEligible(
            input.WorldEvents,
            input.EventLog,
            input.WorldTime,
            candidates);

    private static CharacterTargetSabotageDelayRecoveryCandidate EquivalentCandidate(
        SelectionInput input) =>
        new(input.DelayResult, input.RecoveryEvent);

    private static SelectionInput Input()
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
        WorldEvent priorEvent = new(
            new WorldEventId("sage-began-round"),
            WorldEventTimeEffect.NoAdvance);
        WorldEvent delayEvent = new(
            new WorldEventId("enemy-agent-delayed-sage"),
            WorldEventTimeEffect.Advance);
        WorldEvent recoveryEvent = new(
            new WorldEventId("sage-delay-recovered"),
            WorldEventTimeEffect.Advance);
        CharacterTargetSabotageDelayResult delayResult = new(
            new CharacterTargetSabotageDelayCandidate(occurrence, delayEvent),
            new WorldEventLogSnapshot([priorEvent.EventId, delayEvent.EventId]),
            new WorldTimeSnapshot(delayEvent.EventId));

        return new SelectionInput(
            priorEvent,
            delayEvent,
            recoveryEvent,
            new WorldEventSnapshot([priorEvent, delayEvent, recoveryEvent]),
            delayResult.EventLog,
            delayResult.WorldTime,
            delayResult,
            new CharacterTargetSabotageDelayRecoveryCandidate(delayResult, recoveryEvent));
    }

    private sealed record SelectionInput(
        WorldEvent PriorEvent,
        WorldEvent DelayEvent,
        WorldEvent RecoveryEvent,
        WorldEventSnapshot WorldEvents,
        WorldEventLogSnapshot EventLog,
        WorldTimeSnapshot WorldTime,
        CharacterTargetSabotageDelayResult DelayResult,
        CharacterTargetSabotageDelayRecoveryCandidate Candidate);

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
