using System.Collections;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterTargetSabotageDelayRecoveryConsiderationRequestTests
{
    [Fact]
    public void RetainsExactProviderNeutralInputAndStableCandidates()
    {
        RequestInput input = Input();
        CharacterTargetSabotageDelayRecoveryCandidate second = EquivalentCandidate(input);
        CharacterTargetSabotageDelayRecoveryCandidate[] supplied = [input.Candidate, second];

        CharacterTargetSabotageDelayRecoveryConsiderationRequest request = Request(input, supplied);

        supplied[0] = second;
        Assert.Same(input.Actor, request.Actor);
        Assert.Same(input.WorldEvents, request.WorldEvents);
        Assert.Same(input.EventLog, request.EventLog);
        Assert.Same(input.WorldTime, request.WorldTime);
        Assert.Collection(
            request.Candidates,
            candidate => Assert.Same(input.Candidate, candidate),
            candidate => Assert.Same(second, candidate));
        Assert.IsType<System.Collections.ObjectModel.ReadOnlyCollection<
            CharacterTargetSabotageDelayRecoveryCandidate>>(request.Candidates);
        Assert.Same(input.DelayResult, request.Candidates[0].DelayResult);
        Assert.Same(input.RecoveryEvent, request.Candidates[0].RecoveryEvent);
    }

    [Fact]
    public void AcceptsEmptyAndMaximumCandidateCollectionsWithOneEnumeration()
    {
        RequestInput input = Input();
        CharacterTargetSabotageDelayRecoveryCandidate[] maximum = Enumerable
            .Range(0, CharacterTargetSabotageDelayRecoverySelection.MaximumCandidateCount)
            .Select(_ => EquivalentCandidate(input))
            .ToArray();
        SingleUseEnumerable<CharacterTargetSabotageDelayRecoveryCandidate> onePass = new(maximum);

        CharacterTargetSabotageDelayRecoveryConsiderationRequest empty = Request(input, []);
        CharacterTargetSabotageDelayRecoveryConsiderationRequest full = Request(input, onePass);

        Assert.Empty(empty.Candidates);
        Assert.Equal(
            CharacterTargetSabotageDelayRecoverySelection.MaximumCandidateCount,
            full.Candidates.Count);
        Assert.Same(maximum[0], full.Candidates[0]);
        Assert.Same(maximum[^1], full.Candidates[^1]);
        Assert.Equal(1, onePass.EnumerationCount);
    }

    [Fact]
    public void RejectsNullDuplicateExactAndOverBoundCandidates()
    {
        RequestInput input = Input();
        CharacterTargetSabotageDelayRecoveryCandidate equivalent = EquivalentCandidate(input);
        CharacterTargetSabotageDelayRecoveryCandidate[] excessive = Enumerable
            .Range(0, CharacterTargetSabotageDelayRecoverySelection.MaximumCandidateCount + 1)
            .Select(_ => EquivalentCandidate(input))
            .ToArray();

        Assert.Throws<ArgumentException>(() => Request(
            input,
            [input.Candidate, input.Candidate]));
        Assert.Throws<ArgumentException>(() => Request(input, [null!]));
        Assert.Throws<ArgumentOutOfRangeException>(() => Request(input, excessive));

        CharacterTargetSabotageDelayRecoveryConsiderationRequest accepted = Request(
            input,
            [input.Candidate, equivalent]);
        Assert.Collection(
            accepted.Candidates,
            candidate => Assert.Same(input.Candidate, candidate),
            candidate => Assert.Same(equivalent, candidate));
        Assert.NotSame(input.Candidate, equivalent);
    }

    [Fact]
    public void RejectsEveryNullInput()
    {
        RequestInput input = Input();

        Assert.Throws<ArgumentNullException>(() => Request(input with { Actor = null! }, []));
        Assert.Throws<ArgumentNullException>(() => Request(input with { WorldEvents = null! }, []));
        Assert.Throws<ArgumentNullException>(() => Request(input with { EventLog = null! }, []));
        Assert.Throws<ArgumentNullException>(() => Request(input with { WorldTime = null! }, []));
        Assert.Throws<ArgumentNullException>(() => Request(input, null!));
    }

    [Fact]
    public void ExposesOnlyImmutableProviderNeutralRequestSurface()
    {
        Type requestType = typeof(CharacterTargetSabotageDelayRecoveryConsiderationRequest);
        (string Name, Type Type)[] expectedProperties =
        [
            ("Actor", typeof(Character)),
            ("Candidates", typeof(IReadOnlyList<CharacterTargetSabotageDelayRecoveryCandidate>)),
            ("EventLog", typeof(WorldEventLogSnapshot)),
            ("WorldEvents", typeof(WorldEventSnapshot)),
            ("WorldTime", typeof(WorldTimeSnapshot)),
        ];
        System.Reflection.PropertyInfo[] properties = requestType.GetProperties()
            .OrderBy(property => property.Name, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(
            expectedProperties,
            properties.Select(property => (property.Name, property.PropertyType)).ToArray());
        Assert.All(properties, property => Assert.False(property.CanWrite));
        Assert.Single(requestType.GetConstructors());
        Assert.Empty(requestType.GetEvents());
        Assert.True(requestType.IsSealed);
    }

    private static CharacterTargetSabotageDelayRecoveryConsiderationRequest Request(
        RequestInput input,
        IEnumerable<CharacterTargetSabotageDelayRecoveryCandidate> candidates) =>
        new(
            input.Actor,
            input.WorldEvents,
            input.EventLog,
            input.WorldTime,
            candidates);

    private static CharacterTargetSabotageDelayRecoveryCandidate EquivalentCandidate(
        RequestInput input) =>
        new(input.DelayResult, input.RecoveryEvent);

    private static RequestInput Input()
    {
        Character enemyAgent = RepresentativeEnemyAgentCharacterSource.Create();
        Character target = RepresentativeSageCharacterSource.Create();
        Character actor = RepresentativeHermitCharacterSource.Create()[0];
        CharacterTargetSabotageOccurrence occurrence = new(
            new CharacterTargetSabotageProposal(
                enemyAgent.Identity.Id,
                new CharacterTargetAccessRequirement(
                    target.Identity.Id,
                    "assess-local-influence",
                    enemyAgent.Knowledge.KnowledgeReferences.Single()),
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

        return new RequestInput(
            actor,
            recoveryEvent,
            new WorldEventSnapshot([priorEvent, delayEvent, recoveryEvent]),
            delayResult.EventLog,
            delayResult.WorldTime,
            delayResult,
            new CharacterTargetSabotageDelayRecoveryCandidate(delayResult, recoveryEvent));
    }

    private sealed record RequestInput(
        Character Actor,
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
