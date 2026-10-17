using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Locations;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterObjectSabotageMoveConsiderationRequestTests
{
    [Fact]
    public void RetainsExactProviderNeutralInputAndStableCandidates()
    {
        RequestInput input = Input();
        CharacterObjectSabotageMoveCandidate second = Candidate(
            input.Character,
            input.Target,
            input.Destination,
            "second-evidence");
        CharacterObjectSabotageMoveCandidate[] supplied = [input.Candidate, second];

        CharacterObjectSabotageMoveConsiderationRequest request = new(
            input.Character,
            input.PhysicalAccess,
            input.WorldObjects,
            input.Placements,
            input.Evidence,
            supplied);
        ListIdentityContractTests.AssertIdentity(request);

        supplied[0] = second;

        Assert.Same(input.Character, request.Character);
        Assert.Same(input.PhysicalAccess, request.PhysicalAccess);
        Assert.Same(input.WorldObjects, request.WorldObjects);
        Assert.Same(input.Placements, request.Placements);
        Assert.Same(input.Evidence, request.Evidence);
        Assert.Collection(
            request.Candidates,
            candidate => Assert.Same(input.Candidate, candidate),
            candidate => Assert.Same(second, candidate));
        Assert.IsType<System.Collections.ObjectModel.ReadOnlyCollection<
            CharacterObjectSabotageMoveCandidate>>(request.Candidates);
    }

    [Fact]
    public void AcceptsEmptyAndMaximumBoundedCandidateCollections()
    {
        RequestInput input = Input();
        CharacterObjectSabotageMoveCandidate[] maximum = Enumerable
            .Range(0, CharacterObjectSabotageMoveConsiderationRequest.MaximumCandidateCount)
            .Select(index => Candidate(
                input.Character,
                input.Target,
                input.Destination,
                $"evidence-{index}"))
            .ToArray();

        CharacterObjectSabotageMoveConsiderationRequest empty = Request(input, []);
        CharacterObjectSabotageMoveConsiderationRequest full = Request(input, maximum);

        Assert.Empty(empty.Candidates);
        Assert.Equal(
            CharacterObjectSabotageMoveConsiderationRequest.MaximumCandidateCount,
            full.Candidates.Count);
        Assert.Same(maximum[0], full.Candidates[0]);
        Assert.Same(maximum[^1], full.Candidates[^1]);
    }

    [Fact]
    public void RejectsNullDuplicateAndOverBoundCandidatesAfterFullMaterialization()
    {
        RequestInput input = Input();
        CharacterObjectSabotageMoveCandidate duplicate = Candidate(
            input.Character,
            input.Target,
            input.Destination,
            input.Candidate.Evidence.Evidence.EvidenceId.Value);
        CharacterObjectSabotageMoveCandidate[] excessive = Enumerable
            .Range(0, CharacterObjectSabotageMoveConsiderationRequest.MaximumCandidateCount + 1)
            .Select(index => Candidate(
                input.Character,
                input.Target,
                input.Destination,
                $"excessive-{index}"))
            .ToArray();

        Assert.Throws<ArgumentException>(() => Request(input, [input.Candidate, duplicate]));
        Assert.Throws<ArgumentException>(() => Request(input, [null!]));
        Assert.Throws<ArgumentOutOfRangeException>(() => Request(input, excessive));
        Assert.NotSame(input.Candidate, duplicate);
        Assert.Equal(
            input.Candidate.Evidence.Evidence.EvidenceId,
            duplicate.Evidence.Evidence.EvidenceId);
    }

    [Fact]
    public void RejectsEveryNullInput()
    {
        RequestInput input = Input();

        Assert.Throws<ArgumentNullException>(() => new CharacterObjectSabotageMoveConsiderationRequest(
            null!, input.PhysicalAccess, input.WorldObjects, input.Placements,
            input.Evidence, [input.Candidate]));
        Assert.Throws<ArgumentNullException>(() => new CharacterObjectSabotageMoveConsiderationRequest(
            input.Character, null!, input.WorldObjects, input.Placements,
            input.Evidence, [input.Candidate]));
        Assert.Throws<ArgumentNullException>(() => new CharacterObjectSabotageMoveConsiderationRequest(
            input.Character, input.PhysicalAccess, null!, input.Placements,
            input.Evidence, [input.Candidate]));
        Assert.Throws<ArgumentNullException>(() => new CharacterObjectSabotageMoveConsiderationRequest(
            input.Character, input.PhysicalAccess, input.WorldObjects, null!,
            input.Evidence, [input.Candidate]));
        Assert.Throws<ArgumentNullException>(() => new CharacterObjectSabotageMoveConsiderationRequest(
            input.Character, input.PhysicalAccess, input.WorldObjects, input.Placements,
            null!, [input.Candidate]));
        Assert.Throws<ArgumentNullException>(() => new CharacterObjectSabotageMoveConsiderationRequest(
            input.Character, input.PhysicalAccess, input.WorldObjects, input.Placements,
            input.Evidence, null!));
    }

    [Fact]
    public void ExposesOnlyTheImmutableProviderNeutralRequestSurface()
    {
        Type requestType = typeof(CharacterObjectSabotageMoveConsiderationRequest);
        string[] properties = requestType.GetProperties()
            .Select(property => property.Name)
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(
            ["Candidates", "Character", "Evidence", "PhysicalAccess", "Placements", "WorldObjects"],
            properties);
        Assert.All(requestType.GetProperties(), property => Assert.False(property.CanWrite));
        Assert.Single(requestType.GetConstructors());
        Assert.Empty(requestType.GetEvents());
        Assert.True(requestType.IsSealed);
    }

    private static CharacterObjectSabotageMoveConsiderationRequest Request(
        RequestInput input,
        IEnumerable<CharacterObjectSabotageMoveCandidate> candidates) =>
        new(
            input.Character,
            input.PhysicalAccess,
            input.WorldObjects,
            input.Placements,
            input.Evidence,
            candidates);

    private static RequestInput Input()
    {
        Character character = RepresentativeEnemyAgentCharacterSource.Create();
        WorldObject target = new(
            new WorldObjectIdentity(new ObjectId("village-water-cask"), "Water cask"),
            new WorldObjectDescription("A covered cask beside the village well."));
        Location source = Location("village-square");
        Location destination = Location("abandoned-wagon");
        CharacterObjectSabotageMoveCandidate candidate = Candidate(
            character,
            target,
            destination,
            "cask-drag-marks");

        return new RequestInput(
            character,
            target,
            destination,
            new CharacterPhysicalAccessSnapshot(character.Identity.Id, [target.Identity.Id]),
            new WorldObjectSnapshot([target]),
            new WorldObjectPlacementSnapshot([new WorldObjectPlacement(target, source)]),
            new DiscoveryEvidenceSnapshot([]),
            candidate);
    }

    private static CharacterObjectSabotageMoveCandidate Candidate(
        Character character,
        WorldObject target,
        Location destination,
        string evidenceId)
    {
        CharacterObjectSabotageOccurrence occurrence = new(
            new CharacterObjectSabotageProposal(
                character.Identity.Id,
                new CharacterActionAccessRequirement(
                    target.Identity.Id,
                    "assess-local-influence",
                    character.Knowledge.KnowledgeReferences.Single()),
                CharacterSabotageActionKind.Move),
            target);
        return new CharacterObjectSabotageMoveCandidate(
            new CharacterObjectSabotageMoveDestination(occurrence, destination),
            new CharacterObjectSabotageEvidence(
                occurrence,
                new DiscoveryEvidence(
                    new EvidenceId(evidenceId),
                    target.Identity.Id,
                    VisibilityState.Noticed,
                    false)));
    }

    private static Location Location(string id) =>
        new(
            new LocationIdentity(new LocationId(id), id),
            new LocationDescription($"The authored {id} location."));

    private sealed record RequestInput(
        Character Character,
        WorldObject Target,
        Location Destination,
        CharacterPhysicalAccessSnapshot PhysicalAccess,
        WorldObjectSnapshot WorldObjects,
        WorldObjectPlacementSnapshot Placements,
        DiscoveryEvidenceSnapshot Evidence,
        CharacterObjectSabotageMoveCandidate Candidate);
}
