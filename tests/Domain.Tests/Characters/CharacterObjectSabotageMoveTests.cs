using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Locations;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterObjectSabotageMoveTests
{
    [Fact]
    public void PolicyAllowsEligibleExactTargetMoveToDifferentAuthoredLocation()
    {
        SabotageInput input = Input();

        Assert.True(CanApply(input));
        Assert.Same(input.Target, input.TargetPlacement.WorldObject);
        Assert.Same(input.CurrentLocation, input.TargetPlacement.Location);
        Assert.Same(input.Destination, input.Candidate.Destination.Destination);
    }

    [Fact]
    public void PolicyRejectsMissingValueEqualOrAlreadyDestinationPlacement()
    {
        SabotageInput input = Input();
        WorldObject valueEqualTarget = Target();
        WorldObjectPlacement valueEqualPlacement = new(
            valueEqualTarget,
            input.CurrentLocation);
        WorldObjectPlacement alreadyAtDestination = new(
            input.Target,
            new Location(
                new LocationIdentity(
                    new LocationId(input.Destination.Identity.Id.Value),
                    input.Destination.Identity.DisplayName),
                new LocationDescription(input.Destination.Description.Text)));

        Assert.False(CanApply(input with
        {
            Placements = new WorldObjectPlacementSnapshot([]),
        }));
        Assert.False(CanApply(input with
        {
            Placements = new WorldObjectPlacementSnapshot([valueEqualPlacement]),
        }));
        Assert.False(CanApply(input with
        {
            Placements = new WorldObjectPlacementSnapshot([alreadyAtDestination]),
        }));
        Assert.NotSame(input.Target, valueEqualTarget);
        Assert.Equal(input.Target.Identity, valueEqualTarget.Identity);
        Assert.NotSame(input.Destination, alreadyAtDestination.Location);
        Assert.Equal(
            input.Destination.Identity,
            alreadyAtDestination.Location.Identity);
    }

    [Fact]
    public void PolicyDelegatesOccurrenceAccessAndFreshEvidenceEligibility()
    {
        SabotageInput input = Input();
        CharacterPhysicalAccessSnapshot inaccessible = new(
            input.Character.Identity.Id,
            []);
        DiscoveryEvidence staleEvidence = new(
            new EvidenceId(input.Candidate.Evidence.Evidence.EvidenceId.Value),
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
    }

    [Fact]
    public void ApplyReturnsFreshStablePlacementAndExactFreshEvidence()
    {
        SabotageInput input = Input();
        WorldObject retainedObject = WorldObject("storehouse-ledger");
        Location retainedLocation = Location("storehouse", "Storehouse");
        WorldObjectPlacement retainedPlacement = new(retainedObject, retainedLocation);
        WorldObjectPlacementSnapshot placements = new(
            [retainedPlacement, input.TargetPlacement]);
        DiscoveryEvidence retainedEvidence = new(
            new EvidenceId("retained-evidence"),
            retainedObject.Identity.Id,
            VisibilityState.Identified,
            false);
        DiscoveryEvidenceSnapshot evidence = new([retainedEvidence]);

        CharacterObjectSabotageMoveResult result =
            CharacterObjectSabotageMoveTransition.Apply(
                input.Character,
                input.PhysicalAccess,
                input.WorldObjects,
                placements,
                evidence,
                input.Candidate);

        Assert.NotSame(placements, result.Placements);
        Assert.Collection(
            result.Placements.Entries,
            placement => Assert.Same(retainedPlacement, placement),
            placement =>
            {
                Assert.NotSame(input.TargetPlacement, placement);
                Assert.Same(input.Target, placement.WorldObject);
                Assert.Same(input.Destination, placement.Location);
            });
        Assert.Same(input.Candidate.Evidence, result.EvidenceResult.Candidate);
        Assert.NotSame(evidence, result.EvidenceResult.Evidence);
        Assert.Collection(
            result.EvidenceResult.Evidence.Entries,
            entry => Assert.Same(retainedEvidence, entry),
            entry => Assert.Same(input.Candidate.Evidence.Evidence, entry));
        Assert.Collection(
            placements.Entries,
            placement => Assert.Same(retainedPlacement, placement),
            placement => Assert.Same(input.TargetPlacement, placement));
        Assert.Same(retainedEvidence, Assert.Single(evidence.Entries));
        Assert.Same(input.CurrentLocation, input.TargetPlacement.Location);
        Assert.All(
            typeof(CharacterObjectSabotageMoveResult).GetProperties(),
            property => Assert.False(property.CanWrite));
    }

    [Fact]
    public void ApplyRejectsIneligibleMoveWithoutChangingInputs()
    {
        SabotageInput input = Input();
        WorldObjectPlacement alreadyAtDestination = new(
            input.Target,
            input.Destination);
        WorldObjectPlacementSnapshot placements = new([alreadyAtDestination]);
        CharacterObjectSabotageMoveDestination destinationBinding =
            input.Candidate.Destination;
        CharacterObjectSabotageEvidence evidenceCandidate = input.Candidate.Evidence;

        Assert.Throws<InvalidOperationException>(() =>
            CharacterObjectSabotageMoveTransition.Apply(
                input.Character,
                input.PhysicalAccess,
                input.WorldObjects,
                placements,
                input.ExistingEvidence,
                input.Candidate));

        Assert.Same(alreadyAtDestination, Assert.Single(placements.Entries));
        Assert.Empty(input.ExistingEvidence.Entries);
        Assert.Same(destinationBinding, input.Candidate.Destination);
        Assert.Same(evidenceCandidate, input.Candidate.Evidence);
    }

    [Fact]
    public void ContractsRejectNullsAndExposeOnlyOnePureOperationEach()
    {
        SabotageInput input = Input();
        CharacterObjectSabotageEvidenceResult evidenceResult =
            CharacterObjectSabotageEvidenceTransition.Leave(
                input.Character,
                input.PhysicalAccess,
                input.WorldObjects,
                input.ExistingEvidence,
                input.Candidate.Evidence);

        Assert.Throws<ArgumentNullException>(() =>
            new CharacterObjectSabotageMoveResult(null!, evidenceResult));
        Assert.Throws<ArgumentNullException>(() =>
            new CharacterObjectSabotageMoveResult(input.Placements, null!));
        AssertNullRejection(
            input,
            (character, physicalAccess, worldObjects, placements, evidence, candidate) =>
                CharacterObjectSabotageMovePolicy.CanApply(
                    character,
                    physicalAccess,
                    worldObjects,
                    placements,
                    evidence,
                    candidate));
        AssertNullRejection(
            input,
            (character, physicalAccess, worldObjects, placements, evidence, candidate) =>
                CharacterObjectSabotageMoveTransition.Apply(
                    character,
                    physicalAccess,
                    worldObjects,
                    placements,
                    evidence,
                    candidate));

        AssertSingleStaticOperation(typeof(CharacterObjectSabotageMovePolicy), "CanApply");
        AssertSingleStaticOperation(typeof(CharacterObjectSabotageMoveTransition), "Apply");
    }

    private static void AssertNullRejection(
        SabotageInput input,
        Func<Character, CharacterPhysicalAccessSnapshot, WorldObjectSnapshot,
            WorldObjectPlacementSnapshot, DiscoveryEvidenceSnapshot,
            CharacterObjectSabotageMoveCandidate, object> operation)
    {
        Assert.Throws<ArgumentNullException>(() => operation(
            null!, input.PhysicalAccess, input.WorldObjects, input.Placements,
            input.ExistingEvidence, input.Candidate));
        Assert.Throws<ArgumentNullException>(() => operation(
            input.Character, null!, input.WorldObjects, input.Placements,
            input.ExistingEvidence, input.Candidate));
        Assert.Throws<ArgumentNullException>(() => operation(
            input.Character, input.PhysicalAccess, null!, input.Placements,
            input.ExistingEvidence, input.Candidate));
        Assert.Throws<ArgumentNullException>(() => operation(
            input.Character, input.PhysicalAccess, input.WorldObjects, null!,
            input.ExistingEvidence, input.Candidate));
        Assert.Throws<ArgumentNullException>(() => operation(
            input.Character, input.PhysicalAccess, input.WorldObjects, input.Placements,
            null!, input.Candidate));
        Assert.Throws<ArgumentNullException>(() => operation(
            input.Character, input.PhysicalAccess, input.WorldObjects, input.Placements,
            input.ExistingEvidence, null!));
    }

    private static bool CanApply(SabotageInput input) =>
        CharacterObjectSabotageMovePolicy.CanApply(
            input.Character,
            input.PhysicalAccess,
            input.WorldObjects,
            input.Placements,
            input.ExistingEvidence,
            input.Candidate);

    private static void AssertSingleStaticOperation(Type type, string expectedName)
    {
        System.Reflection.MethodInfo method = Assert.Single(type.GetMethods().Where(
            candidate => candidate.DeclaringType == type));

        Assert.Equal(expectedName, method.Name);
        Assert.True(method.IsStatic);
        Assert.Empty(type.GetConstructors());
        Assert.True(type.IsAbstract && type.IsSealed);
    }

    private static SabotageInput Input()
    {
        Character character = RepresentativeEnemyAgentCharacterSource.Create();
        WorldObject target = Target();
        Location currentLocation = Location("village-square", "Village square");
        Location destination = Location("abandoned-wagon", "Abandoned wagon");
        CharacterObjectSabotageOccurrence occurrence = new(
            new CharacterObjectSabotageProposal(
                character.Identity.Id,
                new CharacterActionAccessRequirement(
                    target.Identity.Id,
                    "assess-local-influence",
                    character.Knowledge.KnowledgeReferences.Single()),
                CharacterSabotageActionKind.Move),
            target);
        CharacterObjectSabotageEvidence evidence = new(
            occurrence,
            new DiscoveryEvidence(
                new EvidenceId("cask-drag-marks"),
                target.Identity.Id,
                VisibilityState.Noticed,
                false));
        CharacterObjectSabotageMoveCandidate candidate = new(
            new CharacterObjectSabotageMoveDestination(occurrence, destination),
            evidence);
        WorldObjectPlacement targetPlacement = new(target, currentLocation);

        return new SabotageInput(
            character,
            target,
            currentLocation,
            destination,
            targetPlacement,
            new CharacterPhysicalAccessSnapshot(character.Identity.Id, [target.Identity.Id]),
            new WorldObjectSnapshot([target]),
            new WorldObjectPlacementSnapshot([targetPlacement]),
            new DiscoveryEvidenceSnapshot([]),
            candidate);
    }

    private static WorldObject Target() => WorldObject("village-water-cask");

    private static WorldObject WorldObject(string objectId) =>
        new(
            new WorldObjectIdentity(new ObjectId(objectId), $"Object {objectId}"),
            new WorldObjectDescription($"Description for {objectId}."));

    private static Location Location(string locationId, string displayName) =>
        new(
            new LocationIdentity(new LocationId(locationId), displayName),
            new LocationDescription($"Description for {locationId}."));

    private sealed record SabotageInput(
        Character Character,
        WorldObject Target,
        Location CurrentLocation,
        Location Destination,
        WorldObjectPlacement TargetPlacement,
        CharacterPhysicalAccessSnapshot PhysicalAccess,
        WorldObjectSnapshot WorldObjects,
        WorldObjectPlacementSnapshot Placements,
        DiscoveryEvidenceSnapshot ExistingEvidence,
        CharacterObjectSabotageMoveCandidate Candidate);
}
