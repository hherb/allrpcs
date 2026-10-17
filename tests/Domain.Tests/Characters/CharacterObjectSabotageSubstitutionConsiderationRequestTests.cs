using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterObjectSabotageSubstitutionConsiderationRequestTests
{
    [Fact]
    public void RetainsExactProviderNeutralInputAndStableCandidates()
    {
        RequestInput input = Input();
        CharacterObjectSabotageSubstitutionCandidate second = Candidate(
            input.Character,
            input.Target,
            input.Replacement,
            "second-evidence");
        CharacterObjectSabotageSubstitutionCandidate[] supplied = [input.Candidate, second];

        CharacterObjectSabotageSubstitutionConsiderationRequest request = new(
            input.Character,
            input.PhysicalAccess,
            input.WorldObjects,
            input.Substitutions,
            input.Evidence,
            supplied);
        ListIdentityContractTests.AssertIdentity(request);

        supplied[0] = second;

        Assert.Same(input.Character, request.Character);
        Assert.Same(input.PhysicalAccess, request.PhysicalAccess);
        Assert.Same(input.WorldObjects, request.WorldObjects);
        Assert.Same(input.Substitutions, request.Substitutions);
        Assert.Same(input.Evidence, request.Evidence);
        Assert.Collection(
            request.Candidates,
            candidate => Assert.Same(input.Candidate, candidate),
            candidate => Assert.Same(second, candidate));
        Assert.IsType<System.Collections.ObjectModel.ReadOnlyCollection<
            CharacterObjectSabotageSubstitutionCandidate>>(request.Candidates);
    }

    [Fact]
    public void AcceptsEmptyAndMaximumBoundedCandidateCollections()
    {
        RequestInput input = Input();
        CharacterObjectSabotageSubstitutionCandidate[] maximum = Enumerable
            .Range(0, CharacterObjectSabotageSubstitutionConsiderationRequest.MaximumCandidateCount)
            .Select(index => Candidate(
                input.Character,
                input.Target,
                input.Replacement,
                $"evidence-{index}"))
            .ToArray();

        CharacterObjectSabotageSubstitutionConsiderationRequest empty = Request(input, []);
        CharacterObjectSabotageSubstitutionConsiderationRequest full = Request(input, maximum);

        Assert.Empty(empty.Candidates);
        Assert.Equal(
            CharacterObjectSabotageSubstitutionConsiderationRequest.MaximumCandidateCount,
            full.Candidates.Count);
        Assert.Same(maximum[0], full.Candidates[0]);
        Assert.Same(maximum[^1], full.Candidates[^1]);
    }

    [Fact]
    public void RejectsNullDuplicateAndOverBoundCandidatesAfterFullMaterialization()
    {
        RequestInput input = Input();
        CharacterObjectSabotageSubstitutionCandidate duplicate = Candidate(
            input.Character,
            input.Target,
            input.Replacement,
            input.Candidate.Evidence.Evidence.EvidenceId.Value);
        CharacterObjectSabotageSubstitutionCandidate[] excessive = Enumerable
            .Range(0, CharacterObjectSabotageSubstitutionConsiderationRequest.MaximumCandidateCount + 1)
            .Select(index => Candidate(
                input.Character,
                input.Target,
                input.Replacement,
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

        Assert.Throws<ArgumentNullException>(
            () => new CharacterObjectSabotageSubstitutionConsiderationRequest(
                null!, input.PhysicalAccess, input.WorldObjects, input.Substitutions,
                input.Evidence, [input.Candidate]));
        Assert.Throws<ArgumentNullException>(
            () => new CharacterObjectSabotageSubstitutionConsiderationRequest(
                input.Character, null!, input.WorldObjects, input.Substitutions,
                input.Evidence, [input.Candidate]));
        Assert.Throws<ArgumentNullException>(
            () => new CharacterObjectSabotageSubstitutionConsiderationRequest(
                input.Character, input.PhysicalAccess, null!, input.Substitutions,
                input.Evidence, [input.Candidate]));
        Assert.Throws<ArgumentNullException>(
            () => new CharacterObjectSabotageSubstitutionConsiderationRequest(
                input.Character, input.PhysicalAccess, input.WorldObjects, null!,
                input.Evidence, [input.Candidate]));
        Assert.Throws<ArgumentNullException>(
            () => new CharacterObjectSabotageSubstitutionConsiderationRequest(
                input.Character, input.PhysicalAccess, input.WorldObjects, input.Substitutions,
                null!, [input.Candidate]));
        Assert.Throws<ArgumentNullException>(
            () => new CharacterObjectSabotageSubstitutionConsiderationRequest(
                input.Character, input.PhysicalAccess, input.WorldObjects, input.Substitutions,
                input.Evidence, null!));
    }

    [Fact]
    public void ExposesOnlyTheImmutableProviderNeutralRequestSurface()
    {
        Type requestType = typeof(CharacterObjectSabotageSubstitutionConsiderationRequest);
        string[] properties = requestType.GetProperties()
            .Select(property => property.Name)
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(
            ["Candidates", "Character", "Evidence", "PhysicalAccess", "Substitutions", "WorldObjects"],
            properties);
        Assert.All(requestType.GetProperties(), property => Assert.False(property.CanWrite));
        Assert.Single(requestType.GetConstructors());
        Assert.Empty(requestType.GetEvents());
        Assert.True(requestType.IsSealed);
    }

    private static CharacterObjectSabotageSubstitutionConsiderationRequest Request(
        RequestInput input,
        IEnumerable<CharacterObjectSabotageSubstitutionCandidate> candidates) =>
        new(
            input.Character,
            input.PhysicalAccess,
            input.WorldObjects,
            input.Substitutions,
            input.Evidence,
            candidates);

    private static RequestInput Input()
    {
        Character character = RepresentativeEnemyAgentCharacterSource.Create();
        WorldObject target = WorldObject("village-water-cask");
        WorldObject replacement = WorldObject("tainted-water-cask");
        CharacterObjectSabotageSubstitutionCandidate candidate = Candidate(
            character,
            target,
            replacement,
            "water-cask-substitution-signs");

        return new RequestInput(
            character,
            target,
            replacement,
            new CharacterPhysicalAccessSnapshot(character.Identity.Id, [target.Identity.Id]),
            new WorldObjectSnapshot([target]),
            new CharacterObjectSabotageSubstitutionSnapshot([]),
            new DiscoveryEvidenceSnapshot([]),
            candidate);
    }

    private static CharacterObjectSabotageSubstitutionCandidate Candidate(
        Character character,
        WorldObject target,
        WorldObject replacement,
        string evidenceId)
    {
        CharacterObjectSabotageOccurrence occurrence = new(
            new CharacterObjectSabotageProposal(
                character.Identity.Id,
                new CharacterActionAccessRequirement(
                    target.Identity.Id,
                    "assess-local-influence",
                    character.Knowledge.KnowledgeReferences.Single()),
                CharacterSabotageActionKind.Substitute),
            target);
        return new CharacterObjectSabotageSubstitutionCandidate(
            new CharacterObjectSabotageSubstitution(occurrence, replacement),
            new CharacterObjectSabotageEvidence(
                occurrence,
                new DiscoveryEvidence(
                    new EvidenceId(evidenceId),
                    target.Identity.Id,
                    VisibilityState.Noticed,
                    false)));
    }

    private static WorldObject WorldObject(string id) =>
        new(
            new WorldObjectIdentity(new ObjectId(id), id),
            new WorldObjectDescription($"The authored {id} object."));

    private sealed record RequestInput(
        Character Character,
        WorldObject Target,
        WorldObject Replacement,
        CharacterPhysicalAccessSnapshot PhysicalAccess,
        WorldObjectSnapshot WorldObjects,
        CharacterObjectSabotageSubstitutionSnapshot Substitutions,
        DiscoveryEvidenceSnapshot Evidence,
        CharacterObjectSabotageSubstitutionCandidate Candidate);
}
