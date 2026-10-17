using System.Collections;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterTargetSabotageDelayConsiderationRequestTests
{
    [Fact]
    public void RetainsExactProviderNeutralInputAndStableCandidates()
    {
        RequestInput input = Input();
        CharacterTargetSabotageDelayCandidate second = Candidate(input, 2);
        CharacterTargetSabotageDelayCandidate[] supplied = [input.Candidate, second];

        CharacterTargetSabotageDelayConsiderationRequest request = Request(input, supplied);
        ListIdentityContractTests.AssertIdentity(request);

        supplied[0] = second;
        Assert.Same(input.Actor, request.Actor);
        Assert.Same(input.InteractionAccess, request.InteractionAccess);
        Assert.Same(input.WorldEvents, request.WorldEvents);
        Assert.Same(input.EventLog, request.EventLog);
        Assert.Same(input.WorldTime, request.WorldTime);
        Assert.Collection(
            request.Candidates,
            candidate => Assert.Same(input.Candidate, candidate),
            candidate => Assert.Same(second, candidate));
        Assert.IsType<System.Collections.ObjectModel.ReadOnlyCollection<
            CharacterTargetSabotageDelayCandidate>>(request.Candidates);
        Assert.Same(input.Occurrence, request.Candidates[0].Occurrence);
        Assert.Same(input.WorldEvent, request.Candidates[0].WorldEvent);
    }

    [Fact]
    public void AcceptsEmptyAndMaximumCandidateCollectionsWithOneEnumeration()
    {
        RequestInput input = Input();
        CharacterTargetSabotageDelayCandidate[] maximum = Enumerable
            .Range(1, CharacterTargetSabotageDelaySelection.MaximumCandidateCount)
            .Select(index => Candidate(input, index))
            .ToArray();
        SingleUseEnumerable<CharacterTargetSabotageDelayCandidate> onePass = new(maximum);

        CharacterTargetSabotageDelayConsiderationRequest empty = Request(input, []);
        CharacterTargetSabotageDelayConsiderationRequest full = Request(input, onePass);

        Assert.Empty(empty.Candidates);
        Assert.Equal(CharacterTargetSabotageDelaySelection.MaximumCandidateCount, full.Candidates.Count);
        Assert.Same(maximum[0], full.Candidates[0]);
        Assert.Same(maximum[^1], full.Candidates[^1]);
        Assert.Equal(1, onePass.EnumerationCount);
    }

    [Fact]
    public void RejectsNullDuplicateExactAndOverBoundCandidates()
    {
        RequestInput input = Input();
        CharacterTargetSabotageDelayCandidate equivalent = new(
            input.Occurrence,
            input.WorldEvent);
        CharacterTargetSabotageDelayCandidate[] excessive = Enumerable
            .Range(1, CharacterTargetSabotageDelaySelection.MaximumCandidateCount + 1)
            .Select(index => Candidate(input, index))
            .ToArray();

        Assert.Throws<ArgumentException>(() => Request(
            input,
            [input.Candidate, input.Candidate]));
        Assert.Throws<ArgumentException>(() => Request(input, [null!]));
        Assert.Throws<ArgumentOutOfRangeException>(() => Request(input, excessive));

        CharacterTargetSabotageDelayConsiderationRequest accepted = Request(
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
        Assert.Throws<ArgumentNullException>(() =>
            Request(input with { InteractionAccess = null! }, []));
        Assert.Throws<ArgumentNullException>(() => Request(input with { WorldEvents = null! }, []));
        Assert.Throws<ArgumentNullException>(() => Request(input with { EventLog = null! }, []));
        Assert.Throws<ArgumentNullException>(() => Request(input with { WorldTime = null! }, []));
        Assert.Throws<ArgumentNullException>(() => Request(input, null!));
    }

    [Fact]
    public void ExposesOnlyImmutableProviderNeutralRequestSurface()
    {
        Type requestType = typeof(CharacterTargetSabotageDelayConsiderationRequest);
        (string Name, Type Type)[] expectedProperties =
        [
            ("Actor", typeof(Character)),
            ("Candidates", typeof(IReadOnlyList<CharacterTargetSabotageDelayCandidate>)),
            ("EventLog", typeof(WorldEventLogSnapshot)),
            ("InteractionAccess", typeof(CharacterInteractionAccessSnapshot)),
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

    private static CharacterTargetSabotageDelayConsiderationRequest Request(
        RequestInput input,
        IEnumerable<CharacterTargetSabotageDelayCandidate> candidates) =>
        new(
            input.Actor,
            input.InteractionAccess,
            input.WorldEvents,
            input.EventLog,
            input.WorldTime,
            candidates);

    private static RequestInput Input()
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
        WorldEvent worldEvent = Event(1);
        CharacterTargetSabotageDelayCandidate candidate = new(occurrence, worldEvent);

        return new(
            actor,
            new CharacterInteractionAccessSnapshot(actor.Identity.Id, [target.Identity.Id]),
            new WorldEventSnapshot([worldEvent]),
            new WorldEventLogSnapshot([]),
            new WorldTimeSnapshot(null),
            occurrence,
            worldEvent,
            candidate);
    }

    private static CharacterTargetSabotageDelayCandidate Candidate(RequestInput input, int ordinal) =>
        new(input.Occurrence, Event(ordinal));

    private static WorldEvent Event(int ordinal) =>
        new(
            new WorldEventId($"enemy-agent-delayed-sage-{ordinal}"),
            WorldEventTimeEffect.Advance);

    private sealed record RequestInput(
        Character Actor,
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
