using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterObjectSabotageContaminationPolicyTests
{
    [Fact]
    public void AllowsEligibleEvidenceBoundContaminationOfAnUncontaminatedTarget()
    {
        SabotageInput input = Input(CharacterSabotageActionKind.Contaminate);

        Assert.True(CanApply(input));
        Assert.False(input.Contamination.IsContaminated(input.Target.Identity.Id));
        Assert.Empty(input.ExistingEvidence.Entries);
    }

    [Fact]
    public void RejectsValueEqualRepeatedContaminationAndEveryOtherActionKind()
    {
        SabotageInput input = Input(CharacterSabotageActionKind.Contaminate);
        WorldObjectContaminationSnapshot contaminated = new(
            [new ObjectId(input.Target.Identity.Id.Value)]);

        Assert.False(CanApply(input with { Contamination = contaminated }));
        Assert.False(CanApply(Input(CharacterSabotageActionKind.Conceal)));
        Assert.False(CanApply(Input(CharacterSabotageActionKind.Move)));
        Assert.False(CanApply(Input(CharacterSabotageActionKind.Substitute)));
    }

    [Fact]
    public void DelegatesExactOccurrenceAccessAndFreshEvidenceEligibility()
    {
        SabotageInput input = Input(CharacterSabotageActionKind.Contaminate);
        CharacterPhysicalAccessSnapshot inaccessible = new(input.Character.Identity.Id, []);
        DiscoveryEvidence staleEvidence = new(
            new EvidenceId(input.Candidate.Evidence.EvidenceId.Value),
            input.Target.Identity.Id,
            VisibilityState.Unnoticed,
            false);
        WorldObject valueEqualTarget = Target();

        Assert.False(CanApply(input with { PhysicalAccess = inaccessible }));
        Assert.False(CanApply(input with
        {
            ExistingEvidence = new DiscoveryEvidenceSnapshot([staleEvidence]),
        }));
        Assert.False(CanApply(input with
        {
            WorldObjects = new WorldObjectSnapshot([valueEqualTarget]),
        }));
        Assert.NotSame(input.Target, valueEqualTarget);
        Assert.Equal(input.Target.Identity, valueEqualTarget.Identity);
    }

    [Fact]
    public void RejectsEveryNullInputAndExposesOnePureOperation()
    {
        SabotageInput input = Input(CharacterSabotageActionKind.Contaminate);

        Assert.Throws<ArgumentNullException>(() => CharacterObjectSabotageContaminationPolicy.CanApply(
            null!, input.PhysicalAccess, input.WorldObjects, input.Contamination,
            input.ExistingEvidence, input.Candidate));
        Assert.Throws<ArgumentNullException>(() => CharacterObjectSabotageContaminationPolicy.CanApply(
            input.Character, null!, input.WorldObjects, input.Contamination,
            input.ExistingEvidence, input.Candidate));
        Assert.Throws<ArgumentNullException>(() => CharacterObjectSabotageContaminationPolicy.CanApply(
            input.Character, input.PhysicalAccess, null!, input.Contamination,
            input.ExistingEvidence, input.Candidate));
        Assert.Throws<ArgumentNullException>(() => CharacterObjectSabotageContaminationPolicy.CanApply(
            input.Character, input.PhysicalAccess, input.WorldObjects, null!,
            input.ExistingEvidence, input.Candidate));
        Assert.Throws<ArgumentNullException>(() => CharacterObjectSabotageContaminationPolicy.CanApply(
            input.Character, input.PhysicalAccess, input.WorldObjects, input.Contamination,
            null!, input.Candidate));
        Assert.Throws<ArgumentNullException>(() => CharacterObjectSabotageContaminationPolicy.CanApply(
            input.Character, input.PhysicalAccess, input.WorldObjects, input.Contamination,
            input.ExistingEvidence, null!));

        System.Reflection.MethodInfo method = Assert.Single(
            typeof(CharacterObjectSabotageContaminationPolicy).GetMethods().Where(
                candidate => candidate.DeclaringType == typeof(CharacterObjectSabotageContaminationPolicy)));

        Assert.Equal("CanApply", method.Name);
        Assert.True(method.IsStatic);
        Assert.Empty(typeof(CharacterObjectSabotageContaminationPolicy).GetConstructors());
        Assert.True(typeof(CharacterObjectSabotageContaminationPolicy).IsAbstract);
        Assert.True(typeof(CharacterObjectSabotageContaminationPolicy).IsSealed);
    }

    private static bool CanApply(SabotageInput input) =>
        CharacterObjectSabotageContaminationPolicy.CanApply(
            input.Character,
            input.PhysicalAccess,
            input.WorldObjects,
            input.Contamination,
            input.ExistingEvidence,
            input.Candidate);

    private static SabotageInput Input(CharacterSabotageActionKind actionKind)
    {
        Character character = RepresentativeEnemyAgentCharacterSource.Create();
        WorldObject target = Target();
        CharacterObjectSabotageOccurrence occurrence = new(
            new CharacterObjectSabotageProposal(
                character.Identity.Id,
                new CharacterActionAccessRequirement(
                    target.Identity.Id,
                    "assess-local-influence",
                    character.Knowledge.KnowledgeReferences.Single()),
                actionKind),
            target);
        CharacterObjectSabotageEvidence candidate = new(
            occurrence,
            new DiscoveryEvidence(
                new EvidenceId("bitter-cask-residue"),
                target.Identity.Id,
                VisibilityState.Noticed,
                false));

        return new SabotageInput(
            character,
            target,
            new CharacterPhysicalAccessSnapshot(character.Identity.Id, [target.Identity.Id]),
            new WorldObjectSnapshot([target]),
            new WorldObjectContaminationSnapshot([]),
            new DiscoveryEvidenceSnapshot([]),
            candidate);
    }

    private static WorldObject Target() =>
        new(
            new WorldObjectIdentity(new ObjectId("village-water-cask"), "Village water cask"),
            new WorldObjectDescription("A covered cask beside the village well."));

    private sealed record SabotageInput(
        Character Character,
        WorldObject Target,
        CharacterPhysicalAccessSnapshot PhysicalAccess,
        WorldObjectSnapshot WorldObjects,
        WorldObjectContaminationSnapshot Contamination,
        DiscoveryEvidenceSnapshot ExistingEvidence,
        CharacterObjectSabotageEvidence Candidate);
}
