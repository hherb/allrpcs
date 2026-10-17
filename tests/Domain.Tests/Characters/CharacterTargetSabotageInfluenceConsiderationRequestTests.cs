using System.Collections;
using Allrpcs.Domain.Characters;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterTargetSabotageInfluenceConsiderationRequestTests
{
    [Fact]
    public void RetainsExactProviderNeutralInputAndStableCandidates()
    {
        RequestInput input = Input();
        CharacterTargetSabotageInfluenceCandidate second = Candidate(input, 2);
        CharacterTargetSabotageInfluenceCandidate[] supplied = [input.Candidate, second];

        CharacterTargetSabotageInfluenceConsiderationRequest request = new(
            input.Actor,
            input.InteractionAccess,
            input.Relationship,
            supplied);
        ListIdentityContractTests.AssertIdentity(request);

        supplied[0] = second;

        Assert.Same(input.Actor, request.Actor);
        Assert.Same(input.InteractionAccess, request.InteractionAccess);
        Assert.Same(input.Relationship, request.Relationship);
        Assert.Collection(
            request.Candidates,
            candidate => Assert.Same(input.Candidate, candidate),
            candidate => Assert.Same(second, candidate));
        Assert.IsType<System.Collections.ObjectModel.ReadOnlyCollection<
            CharacterTargetSabotageInfluenceCandidate>>(request.Candidates);
        Assert.Same(input.Occurrence, request.Candidates[0].Occurrence);
        Assert.Same(input.Change, request.Candidates[0].RelationshipChange);
    }

    [Fact]
    public void AcceptsEmptyAndMaximumCandidateCollectionsWithOneEnumeration()
    {
        RequestInput input = Input();
        CharacterTargetSabotageInfluenceCandidate[] maximum = Enumerable
            .Range(0, CharacterTargetSabotageInfluenceSelection.MaximumCandidateCount)
            .Select(index => Candidate(input, index + 1))
            .ToArray();
        SingleUseEnumerable<CharacterTargetSabotageInfluenceCandidate> onePass = new(maximum);

        CharacterTargetSabotageInfluenceConsiderationRequest empty = Request(input, []);
        CharacterTargetSabotageInfluenceConsiderationRequest full = Request(input, onePass);

        Assert.Empty(empty.Candidates);
        Assert.Equal(
            CharacterTargetSabotageInfluenceSelection.MaximumCandidateCount,
            full.Candidates.Count);
        Assert.Same(maximum[0], full.Candidates[0]);
        Assert.Same(maximum[^1], full.Candidates[^1]);
        Assert.Equal(1, onePass.EnumerationCount);
    }

    [Fact]
    public void RejectsNullDuplicateExactAndOverBoundCandidates()
    {
        RequestInput input = Input();
        CharacterTargetSabotageInfluenceCandidate equivalent = new(
            input.Occurrence,
            input.Change);
        CharacterTargetSabotageInfluenceCandidate[] excessive = Enumerable
            .Range(0, CharacterTargetSabotageInfluenceSelection.MaximumCandidateCount + 1)
            .Select(index => Candidate(input, index + 1))
            .ToArray();

        Assert.Throws<ArgumentException>(() => Request(
            input,
            [input.Candidate, input.Candidate]));
        Assert.Throws<ArgumentException>(() => Request(input, [null!]));
        Assert.Throws<ArgumentOutOfRangeException>(() => Request(input, excessive));

        CharacterTargetSabotageInfluenceConsiderationRequest accepted = Request(
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

        Assert.Throws<ArgumentNullException>(() =>
            new CharacterTargetSabotageInfluenceConsiderationRequest(
                null!, input.InteractionAccess, input.Relationship, [input.Candidate]));
        Assert.Throws<ArgumentNullException>(() =>
            new CharacterTargetSabotageInfluenceConsiderationRequest(
                input.Actor, null!, input.Relationship, [input.Candidate]));
        Assert.Throws<ArgumentNullException>(() =>
            new CharacterTargetSabotageInfluenceConsiderationRequest(
                input.Actor, input.InteractionAccess, null!, [input.Candidate]));
        Assert.Throws<ArgumentNullException>(() =>
            new CharacterTargetSabotageInfluenceConsiderationRequest(
                input.Actor, input.InteractionAccess, input.Relationship, null!));
    }

    [Fact]
    public void ExposesOnlyImmutableProviderNeutralRequestSurface()
    {
        Type requestType = typeof(CharacterTargetSabotageInfluenceConsiderationRequest);
        (string Name, Type Type)[] expectedProperties =
        [
            ("Actor", typeof(Character)),
            ("Candidates", typeof(IReadOnlyList<CharacterTargetSabotageInfluenceCandidate>)),
            ("InteractionAccess", typeof(CharacterInteractionAccessSnapshot)),
            ("Relationship", typeof(CharacterRelationshipMemory)),
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

    private static CharacterTargetSabotageInfluenceConsiderationRequest Request(
        RequestInput input,
        IEnumerable<CharacterTargetSabotageInfluenceCandidate> candidates) =>
        new(
            input.Actor,
            input.InteractionAccess,
            input.Relationship,
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
                CharacterSabotageActionKind.Influence),
            target);
        CharacterRelationshipChange change = Change(occurrence, 1);
        CharacterTargetSabotageInfluenceCandidate candidate = new(occurrence, change);

        return new(
            actor,
            new CharacterInteractionAccessSnapshot(actor.Identity.Id, [target.Identity.Id]),
            new CharacterRelationshipMemory(
                target.Identity.Id,
                actor.Identity.Id,
                [new CharacterRelationshipDimensionValue(CharacterRelationshipDimension.Trust, 10)],
                []),
            occurrence,
            change,
            candidate);
    }

    private static CharacterTargetSabotageInfluenceCandidate Candidate(
        RequestInput input,
        int ordinal) =>
        new(input.Occurrence, Change(input.Occurrence, ordinal));

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

    private sealed record RequestInput(
        Character Actor,
        CharacterInteractionAccessSnapshot InteractionAccess,
        CharacterRelationshipMemory Relationship,
        CharacterTargetSabotageOccurrence Occurrence,
        CharacterRelationshipChange Change,
        CharacterTargetSabotageInfluenceCandidate Candidate);

    private sealed class SingleUseEnumerable<T>(IEnumerable<T> values) : IEnumerable<T>
    {
        public int EnumerationCount { get; private set; }

        public IEnumerator<T> GetEnumerator()
        {
            EnumerationCount++;
            if (EnumerationCount > 1)
            {
                throw new InvalidOperationException("The candidate source was enumerated more than once.");
            }

            return values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
