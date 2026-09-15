using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterObjectSabotageContaminationConsiderationRequestTests
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

        CharacterObjectSabotageContaminationConsiderationRequest request = new(
            input.Character,
            input.PhysicalAccess,
            input.WorldObjects,
            input.Contamination,
            input.Evidence,
            supplied);

        supplied[0] = second;

        Assert.Same(input.Character, request.Character);
        Assert.Same(input.PhysicalAccess, request.PhysicalAccess);
        Assert.Same(input.WorldObjects, request.WorldObjects);
        Assert.Same(input.Contamination, request.Contamination);
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
            .Range(0, CharacterObjectSabotageContaminationSelection.MaximumCandidateCount)
            .Select(index => Candidate(input.Character, input.Target, $"evidence-{index}"))
            .ToArray();

        CharacterObjectSabotageContaminationConsiderationRequest empty = Request(input, []);
        CharacterObjectSabotageContaminationConsiderationRequest full = Request(input, maximum);

        Assert.Empty(empty.Candidates);
        Assert.Equal(
            CharacterObjectSabotageContaminationSelection.MaximumCandidateCount,
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
            .Range(0, CharacterObjectSabotageContaminationSelection.MaximumCandidateCount + 1)
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

        Assert.Throws<ArgumentNullException>(() => New(
            null!, input.PhysicalAccess, input.WorldObjects, input.Contamination,
            input.Evidence, [input.Candidate]));
        Assert.Throws<ArgumentNullException>(() => New(
            input.Character, null!, input.WorldObjects, input.Contamination,
            input.Evidence, [input.Candidate]));
        Assert.Throws<ArgumentNullException>(() => New(
            input.Character, input.PhysicalAccess, null!, input.Contamination,
            input.Evidence, [input.Candidate]));
        Assert.Throws<ArgumentNullException>(() => New(
            input.Character, input.PhysicalAccess, input.WorldObjects, null!,
            input.Evidence, [input.Candidate]));
        Assert.Throws<ArgumentNullException>(() => New(
            input.Character, input.PhysicalAccess, input.WorldObjects, input.Contamination,
            null!, [input.Candidate]));
        Assert.Throws<ArgumentNullException>(() => New(
            input.Character, input.PhysicalAccess, input.WorldObjects, input.Contamination,
            input.Evidence, null!));
    }

    [Fact]
    public void ExposesOnlyTheImmutableProviderNeutralRequestSurface()
    {
        Type requestType = typeof(CharacterObjectSabotageContaminationConsiderationRequest);
        string[] properties = requestType.GetProperties()
            .Select(property => property.Name)
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(
            ["Candidates", "Character", "Contamination", "Evidence", "PhysicalAccess", "WorldObjects"],
            properties);
        Assert.All(requestType.GetProperties(), property => Assert.False(property.CanWrite));
        Assert.Single(requestType.GetConstructors());
        Assert.Empty(requestType.GetEvents());
        Assert.True(requestType.IsSealed);
    }

    private static CharacterObjectSabotageContaminationConsiderationRequest Request(
        RequestInput input,
        IEnumerable<CharacterObjectSabotageEvidence> candidates) =>
        New(
            input.Character,
            input.PhysicalAccess,
            input.WorldObjects,
            input.Contamination,
            input.Evidence,
            candidates);

    private static CharacterObjectSabotageContaminationConsiderationRequest New(
        Character character,
        CharacterPhysicalAccessSnapshot physicalAccess,
        WorldObjectSnapshot worldObjects,
        WorldObjectContaminationSnapshot contamination,
        DiscoveryEvidenceSnapshot evidence,
        IEnumerable<CharacterObjectSabotageEvidence> candidates) =>
        new(character, physicalAccess, worldObjects, contamination, evidence, candidates);

    private static RequestInput Input()
    {
        Character character = RepresentativeEnemyAgentCharacterSource.Create();
        WorldObject target = new(
            new WorldObjectIdentity(new ObjectId("village-water-cask"), "Water cask"),
            new WorldObjectDescription("A covered cask beside the village well."));
        CharacterObjectSabotageEvidence candidate = Candidate(
            character,
            target,
            "bitter-cask-residue");

        return new RequestInput(
            character,
            target,
            new CharacterPhysicalAccessSnapshot(character.Identity.Id, [target.Identity.Id]),
            new WorldObjectSnapshot([target]),
            new WorldObjectContaminationSnapshot([]),
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
                    CharacterSabotageActionKind.Contaminate),
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
        WorldObjectContaminationSnapshot Contamination,
        DiscoveryEvidenceSnapshot Evidence,
        CharacterObjectSabotageEvidence Candidate);
}
