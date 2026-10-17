using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterObjectSabotageConcealmentConsiderationRequestTests
{
    [Fact]
    public void RetainsExactProviderNeutralInputAndStableCandidates()
    {
        RequestInput input = Input();
        CharacterObjectSabotageEvidence second = Candidate(
            input.Character,
            input.Target,
            "second-evidence");
        CharacterObjectSabotageEvidence[] supplied = [input.Candidate, second];

        CharacterObjectSabotageConcealmentConsiderationRequest request = new(
            input.Character,
            input.PhysicalAccess,
            input.WorldObjects,
            input.Concealment,
            input.Evidence,
            supplied);
        ListIdentityContractTests.AssertIdentity(request);

        supplied[0] = second;

        Assert.Same(input.Character, request.Character);
        Assert.Same(input.PhysicalAccess, request.PhysicalAccess);
        Assert.Same(input.WorldObjects, request.WorldObjects);
        Assert.Same(input.Concealment, request.Concealment);
        Assert.Same(input.Evidence, request.Evidence);
        Assert.Collection(
            request.Candidates,
            candidate => Assert.Same(input.Candidate, candidate),
            candidate => Assert.Same(second, candidate));
        Assert.IsType<System.Collections.ObjectModel.ReadOnlyCollection<
            CharacterObjectSabotageEvidence>>(request.Candidates);
    }

    [Fact]
    public void AcceptsEmptyAndMaximumBoundedCandidateCollections()
    {
        RequestInput input = Input();
        CharacterObjectSabotageEvidence[] maximum = Enumerable
            .Range(0, CharacterObjectSabotageConcealmentSelection.MaximumCandidateCount)
            .Select(index => Candidate(input.Character, input.Target, $"evidence-{index}"))
            .ToArray();

        CharacterObjectSabotageConcealmentConsiderationRequest empty = Request(input, []);
        CharacterObjectSabotageConcealmentConsiderationRequest full = Request(input, maximum);

        Assert.Empty(empty.Candidates);
        Assert.Equal(
            CharacterObjectSabotageConcealmentSelection.MaximumCandidateCount,
            full.Candidates.Count);
        Assert.Same(maximum[0], full.Candidates[0]);
        Assert.Same(maximum[^1], full.Candidates[^1]);
    }

    [Fact]
    public void RejectsNullDuplicateAndOverBoundCandidatesAfterFullMaterialization()
    {
        RequestInput input = Input();
        CharacterObjectSabotageEvidence duplicate = new(
            input.Candidate.Occurrence,
            new DiscoveryEvidence(
                new EvidenceId(input.Candidate.Evidence.EvidenceId.Value),
                input.Target.Identity.Id,
                VisibilityState.Noticed,
                false));
        CharacterObjectSabotageEvidence[] excessive = Enumerable
            .Range(0, CharacterObjectSabotageConcealmentSelection.MaximumCandidateCount + 1)
            .Select(index => Candidate(input.Character, input.Target, $"excessive-{index}"))
            .ToArray();

        Assert.Throws<ArgumentException>(() => Request(input, [input.Candidate, duplicate]));
        Assert.Throws<ArgumentException>(() => Request(input, [null!]));
        Assert.Throws<ArgumentOutOfRangeException>(() => Request(input, excessive));
        Assert.Same(input.Candidate.Occurrence, duplicate.Occurrence);
        Assert.NotSame(input.Candidate.Evidence, duplicate.Evidence);
    }

    [Fact]
    public void RejectsEveryNullInput()
    {
        RequestInput input = Input();

        Assert.Throws<ArgumentNullException>(() => new CharacterObjectSabotageConcealmentConsiderationRequest(
            null!, input.PhysicalAccess, input.WorldObjects, input.Concealment,
            input.Evidence, [input.Candidate]));
        Assert.Throws<ArgumentNullException>(() => new CharacterObjectSabotageConcealmentConsiderationRequest(
            input.Character, null!, input.WorldObjects, input.Concealment,
            input.Evidence, [input.Candidate]));
        Assert.Throws<ArgumentNullException>(() => new CharacterObjectSabotageConcealmentConsiderationRequest(
            input.Character, input.PhysicalAccess, null!, input.Concealment,
            input.Evidence, [input.Candidate]));
        Assert.Throws<ArgumentNullException>(() => new CharacterObjectSabotageConcealmentConsiderationRequest(
            input.Character, input.PhysicalAccess, input.WorldObjects, null!,
            input.Evidence, [input.Candidate]));
        Assert.Throws<ArgumentNullException>(() => new CharacterObjectSabotageConcealmentConsiderationRequest(
            input.Character, input.PhysicalAccess, input.WorldObjects, input.Concealment,
            null!, [input.Candidate]));
        Assert.Throws<ArgumentNullException>(() => new CharacterObjectSabotageConcealmentConsiderationRequest(
            input.Character, input.PhysicalAccess, input.WorldObjects, input.Concealment,
            input.Evidence, null!));
    }

    [Fact]
    public void ExposesOnlyTheImmutableProviderNeutralRequestSurface()
    {
        Type requestType = typeof(CharacterObjectSabotageConcealmentConsiderationRequest);
        string[] properties = requestType.GetProperties()
            .Select(property => property.Name)
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(
            ["Candidates", "Character", "Concealment", "Evidence", "PhysicalAccess", "WorldObjects"],
            properties);
        Assert.All(requestType.GetProperties(), property => Assert.False(property.CanWrite));
        Assert.Single(requestType.GetConstructors());
        Assert.Empty(requestType.GetEvents());
        Assert.True(requestType.IsSealed);
    }

    private static CharacterObjectSabotageConcealmentConsiderationRequest Request(
        RequestInput input,
        IEnumerable<CharacterObjectSabotageEvidence> candidates) =>
        new(
            input.Character,
            input.PhysicalAccess,
            input.WorldObjects,
            input.Concealment,
            input.Evidence,
            candidates);

    private static RequestInput Input()
    {
        Character character = RepresentativeEnemyAgentCharacterSource.Create();
        WorldObject target = new(
            new WorldObjectIdentity(new ObjectId("village-water-cask"), "Water cask"),
            new WorldObjectDescription("A covered cask beside the village well."));
        CharacterObjectSabotageEvidence candidate = Candidate(
            character,
            target,
            "disturbed-cask-cover");

        return new RequestInput(
            character,
            target,
            new CharacterPhysicalAccessSnapshot(character.Identity.Id, [target.Identity.Id]),
            new WorldObjectSnapshot([target]),
            new WorldObjectConcealmentSnapshot([]),
            new DiscoveryEvidenceSnapshot([]),
            candidate);
    }

    private static CharacterObjectSabotageEvidence Candidate(
        Character character,
        WorldObject target,
        string evidenceId) =>
        new(
            new CharacterObjectSabotageOccurrence(
                new CharacterObjectSabotageProposal(
                    character.Identity.Id,
                    new CharacterActionAccessRequirement(
                        target.Identity.Id,
                        "assess-local-influence",
                        character.Knowledge.KnowledgeReferences.Single()),
                    CharacterSabotageActionKind.Conceal),
                target),
            new DiscoveryEvidence(
                new EvidenceId(evidenceId),
                target.Identity.Id,
                VisibilityState.Noticed,
                false));

    private sealed record RequestInput(
        Character Character,
        WorldObject Target,
        CharacterPhysicalAccessSnapshot PhysicalAccess,
        WorldObjectSnapshot WorldObjects,
        WorldObjectConcealmentSnapshot Concealment,
        DiscoveryEvidenceSnapshot Evidence,
        CharacterObjectSabotageEvidence Candidate);
}
