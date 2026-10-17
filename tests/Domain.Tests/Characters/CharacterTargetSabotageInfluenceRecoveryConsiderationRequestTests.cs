using System.Collections;
using Allrpcs.Domain.Characters;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterTargetSabotageInfluenceRecoveryConsiderationRequestTests
{
    [Fact]
    public void RetainsExactProviderNeutralInputAndStableCandidates()
    {
        RequestInput input = Input();
        CharacterTargetSabotageInfluenceRecoveryCandidate second = EquivalentCandidate(input);
        CharacterTargetSabotageInfluenceRecoveryCandidate[] supplied = [input.Candidate, second];

        CharacterTargetSabotageInfluenceRecoveryConsiderationRequest request = new(
            input.Actor,
            input.Relationship,
            supplied);
        ListIdentityContractTests.AssertIdentity(request);

        supplied[0] = second;

        Assert.Same(input.Actor, request.Actor);
        Assert.Same(input.Relationship, request.Relationship);
        Assert.Collection(
            request.Candidates,
            candidate => Assert.Same(input.Candidate, candidate),
            candidate => Assert.Same(second, candidate));
        Assert.IsType<System.Collections.ObjectModel.ReadOnlyCollection<
            CharacterTargetSabotageInfluenceRecoveryCandidate>>(request.Candidates);
        Assert.Same(input.InfluenceResult, request.Candidates[0].InfluenceResult);
        Assert.Same(input.RecoveryChange, request.Candidates[0].RecoveryChange);
    }

    [Fact]
    public void AcceptsEmptyAndMaximumCandidateCollectionsWithOneEnumeration()
    {
        RequestInput input = Input();
        CharacterTargetSabotageInfluenceRecoveryCandidate[] maximum = Enumerable
            .Range(0, CharacterTargetSabotageInfluenceRecoverySelection.MaximumCandidateCount)
            .Select(_ => EquivalentCandidate(input))
            .ToArray();
        SingleUseEnumerable<CharacterTargetSabotageInfluenceRecoveryCandidate> onePass =
            new(maximum);

        CharacterTargetSabotageInfluenceRecoveryConsiderationRequest empty =
            Request(input, []);
        CharacterTargetSabotageInfluenceRecoveryConsiderationRequest full =
            Request(input, onePass);

        Assert.Empty(empty.Candidates);
        Assert.Equal(
            CharacterTargetSabotageInfluenceRecoverySelection.MaximumCandidateCount,
            full.Candidates.Count);
        Assert.Same(maximum[0], full.Candidates[0]);
        Assert.Same(maximum[^1], full.Candidates[^1]);
        Assert.Equal(1, onePass.EnumerationCount);
    }

    [Fact]
    public void RejectsNullDuplicateExactAndOverBoundCandidates()
    {
        RequestInput input = Input();
        CharacterTargetSabotageInfluenceRecoveryCandidate equivalent = EquivalentCandidate(input);
        CharacterTargetSabotageInfluenceRecoveryCandidate[] excessive = Enumerable
            .Range(0, CharacterTargetSabotageInfluenceRecoverySelection.MaximumCandidateCount + 1)
            .Select(_ => EquivalentCandidate(input))
            .ToArray();

        Assert.Throws<ArgumentException>(() =>
            Request(input, [input.Candidate, input.Candidate]));
        Assert.Throws<ArgumentException>(() => Request(input, [null!]));
        Assert.Throws<ArgumentOutOfRangeException>(() => Request(input, excessive));

        CharacterTargetSabotageInfluenceRecoveryConsiderationRequest accepted =
            Request(input, [input.Candidate, equivalent]);
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
            new CharacterTargetSabotageInfluenceRecoveryConsiderationRequest(
                null!, input.Relationship, [input.Candidate]));
        Assert.Throws<ArgumentNullException>(() =>
            new CharacterTargetSabotageInfluenceRecoveryConsiderationRequest(
                input.Actor, null!, [input.Candidate]));
        Assert.Throws<ArgumentNullException>(() =>
            new CharacterTargetSabotageInfluenceRecoveryConsiderationRequest(
                input.Actor, input.Relationship, null!));
    }

    [Fact]
    public void ExposesOnlyImmutableProviderNeutralRequestSurface()
    {
        Type requestType = typeof(CharacterTargetSabotageInfluenceRecoveryConsiderationRequest);
        (string Name, Type Type)[] expectedProperties =
        [
            ("Actor", typeof(Character)),
            ("Candidates", typeof(IReadOnlyList<CharacterTargetSabotageInfluenceRecoveryCandidate>)),
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

    private static CharacterTargetSabotageInfluenceRecoveryConsiderationRequest Request(
        RequestInput input,
        IEnumerable<CharacterTargetSabotageInfluenceRecoveryCandidate> candidates) =>
        new(input.Actor, input.Relationship, candidates);

    private static CharacterTargetSabotageInfluenceRecoveryCandidate EquivalentCandidate(
        RequestInput input) =>
        new(input.InfluenceResult, input.RecoveryChange);

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
        CharacterRelationshipChange influenceChange = new(
            target.Identity.Id,
            actor.Identity.Id,
            1,
            CharacterRelationshipChangeTests.Reason(
                "influence-attempt",
                "enemy-agent-influence"));
        CharacterTargetSabotageInfluenceCandidate influenceCandidate = new(
            occurrence,
            influenceChange);
        CharacterRelationshipMemory priorRelationship = new(
            target.Identity.Id,
            actor.Identity.Id,
            [new CharacterRelationshipDimensionValue(CharacterRelationshipDimension.Trust, 10)],
            []);
        CharacterRelationshipMemory relationship =
            CharacterRelationshipChangeTransition.Apply(priorRelationship, influenceChange);
        CharacterTargetSabotageInfluenceResult influenceResult = new(
            influenceCandidate,
            relationship);
        CharacterRelationshipChange recoveryChange = new(
            target.Identity.Id,
            actor.Identity.Id,
            -1,
            CharacterRelationshipChangeTests.Reason(
                "influence-recovered",
                "seeker-recovery"));

        return new RequestInput(
            target,
            relationship,
            influenceResult,
            recoveryChange,
            new CharacterTargetSabotageInfluenceRecoveryCandidate(
                influenceResult,
                recoveryChange));
    }

    private sealed record RequestInput(
        Character Actor,
        CharacterRelationshipMemory Relationship,
        CharacterTargetSabotageInfluenceResult InfluenceResult,
        CharacterRelationshipChange RecoveryChange,
        CharacterTargetSabotageInfluenceRecoveryCandidate Candidate);

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
