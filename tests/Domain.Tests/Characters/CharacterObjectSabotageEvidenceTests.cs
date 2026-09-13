using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterObjectSabotageEvidenceTests
{
    [Fact]
    public void CandidateRetainsExactOccurrenceAndEvidence()
    {
        SabotageInput input = Input();

        CharacterObjectSabotageEvidence candidate = new(input.Occurrence, input.Evidence);

        Assert.Same(input.Occurrence, candidate.Occurrence);
        Assert.Same(input.Evidence, candidate.Evidence);
        Assert.All(
            typeof(CharacterObjectSabotageEvidence).GetProperties(),
            property => Assert.False(property.CanWrite));
    }

    [Fact]
    public void PolicyAllowsEligibleOccurrenceWithFreshTargetBoundEvidence()
    {
        SabotageInput input = Input();

        Assert.True(CanLeave(input, new DiscoveryEvidenceSnapshot([])));
        Assert.Same(input.Occurrence, input.Candidate.Occurrence);
        Assert.Same(input.Evidence, input.Candidate.Evidence);
    }

    [Fact]
    public void PolicyRejectsEvidenceForAnotherTargetOrReusedIdentity()
    {
        SabotageInput input = Input();
        DiscoveryEvidence otherTargetEvidence = new(
            input.Evidence.EvidenceId,
            new ObjectId("other-object"),
            input.Evidence.MinimumVisibilityState,
            input.Evidence.IsCritical);
        DiscoveryEvidence valueEqualExisting = new(
            new EvidenceId(input.Evidence.EvidenceId.Value),
            new ObjectId(input.Evidence.ObjectId.Value),
            input.Evidence.MinimumVisibilityState,
            input.Evidence.IsCritical);

        Assert.False(CanLeave(
            input with
            {
                Candidate = new CharacterObjectSabotageEvidence(
                    input.Occurrence,
                    otherTargetEvidence),
            },
            new DiscoveryEvidenceSnapshot([])));
        Assert.False(CanLeave(input, new DiscoveryEvidenceSnapshot([valueEqualExisting])));
    }

    [Fact]
    public void PolicyDelegatesCompleteOccurrenceEligibility()
    {
        SabotageInput input = Input();
        CharacterPhysicalAccessSnapshot inaccessible = new(input.Character.Identity.Id, []);
        WorldObject equalTarget = Target();

        Assert.False(CanLeave(
            input with { PhysicalAccess = inaccessible },
            new DiscoveryEvidenceSnapshot([])));
        Assert.False(CanLeave(
            input with { WorldObjects = new WorldObjectSnapshot([equalTarget]) },
            new DiscoveryEvidenceSnapshot([])));
    }

    [Fact]
    public void ContractsRejectNullsAndPolicyExposesOnlyOnePureOperation()
    {
        SabotageInput input = Input();
        DiscoveryEvidenceSnapshot evidence = new([]);

        Assert.Throws<ArgumentNullException>(() =>
            new CharacterObjectSabotageEvidence(null!, input.Evidence));
        Assert.Throws<ArgumentNullException>(() =>
            new CharacterObjectSabotageEvidence(input.Occurrence, null!));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterObjectSabotageEvidencePolicy.CanLeave(
                null!, input.PhysicalAccess, input.WorldObjects, evidence, input.Candidate));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterObjectSabotageEvidencePolicy.CanLeave(
                input.Character, null!, input.WorldObjects, evidence, input.Candidate));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterObjectSabotageEvidencePolicy.CanLeave(
                input.Character, input.PhysicalAccess, null!, evidence, input.Candidate));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterObjectSabotageEvidencePolicy.CanLeave(
                input.Character, input.PhysicalAccess, input.WorldObjects, null!, input.Candidate));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterObjectSabotageEvidencePolicy.CanLeave(
                input.Character, input.PhysicalAccess, input.WorldObjects, evidence, null!));

        Type policyType = typeof(CharacterObjectSabotageEvidencePolicy);
        System.Reflection.MethodInfo method = Assert.Single(policyType.GetMethods().Where(
            candidate => candidate.DeclaringType == policyType));
        Assert.Equal("CanLeave", method.Name);
        Assert.True(method.IsStatic);
        Assert.Empty(policyType.GetConstructors());
        Assert.True(policyType.IsAbstract && policyType.IsSealed);
    }

    private static bool CanLeave(
        SabotageInput input,
        DiscoveryEvidenceSnapshot evidence) =>
        CharacterObjectSabotageEvidencePolicy.CanLeave(
            input.Character,
            input.PhysicalAccess,
            input.WorldObjects,
            evidence,
            input.Candidate);

    private static SabotageInput Input()
    {
        Character character = RepresentativeEnemyAgentCharacterSource.Create();
        WorldObject target = Target();
        CharacterActionAccessRequirement requirement = new(
            target.Identity.Id,
            "assess-local-influence",
            character.Knowledge.KnowledgeReferences.Single());
        CharacterObjectSabotageOccurrence occurrence = new(
            new CharacterObjectSabotageProposal(
                character.Identity.Id,
                requirement,
                CharacterSabotageActionKind.Conceal),
            target);
        DiscoveryEvidence evidence = new(
            new EvidenceId("disturbed-cask-cover"),
            target.Identity.Id,
            VisibilityState.Noticed,
            false);

        return new SabotageInput(
            character,
            new CharacterPhysicalAccessSnapshot(character.Identity.Id, [target.Identity.Id]),
            new WorldObjectSnapshot([target]),
            occurrence,
            evidence,
            new CharacterObjectSabotageEvidence(occurrence, evidence));
    }

    private static WorldObject Target() =>
        new(
            new WorldObjectIdentity(new ObjectId("village-water-cask"), "Village water cask"),
            new WorldObjectDescription("A covered cask beside the village well."));

    private sealed record SabotageInput(
        Character Character,
        CharacterPhysicalAccessSnapshot PhysicalAccess,
        WorldObjectSnapshot WorldObjects,
        CharacterObjectSabotageOccurrence Occurrence,
        DiscoveryEvidence Evidence,
        CharacterObjectSabotageEvidence Candidate);
}
