using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Locations;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterObjectSabotageMoveRecoveryTests
{
    [Fact]
    public void PolicyAllowsExactMovedTargetWithAuthoritativeVisibleEvidence()
    {
        RecoveryInput input = Input();

        Assert.True(CanRecover(input));
        Assert.Same(input.MovedPlacement, Assert.Single(input.Placements.Entries));
        Assert.Same(input.EvidenceDefinition, Assert.Single(input.Evidence.Entries));
        Assert.Same(
            input.EvidenceDefinition.EvidenceId,
            Assert.Single(input.EvidenceView.EvidenceIds));
    }

    [Fact]
    public void PolicyRejectsMissingSourceOrValueEqualTargetPlacement()
    {
        RecoveryInput input = Input();
        WorldObject valueEqualTarget = Target();
        WorldObjectPlacement valueEqualTargetPlacement = new(
            valueEqualTarget,
            input.Destination);

        Assert.False(CanRecover(input with
        {
            Placements = new WorldObjectPlacementSnapshot([]),
        }));
        Assert.False(CanRecover(input with
        {
            Placements = new WorldObjectPlacementSnapshot([input.SourcePlacement]),
        }));
        Assert.False(CanRecover(input with
        {
            Placements = new WorldObjectPlacementSnapshot([valueEqualTargetPlacement]),
        }));
        Assert.NotSame(input.Target, valueEqualTarget);
        Assert.Equal(input.Target.Identity, valueEqualTarget.Identity);
    }

    [Fact]
    public void PolicyRejectsDistinctValueEqualDestinationPlacement()
    {
        RecoveryInput input = Input();
        Location valueEqualDestination = new(
            new LocationIdentity(
                new LocationId(input.Destination.Identity.Id.Value),
                input.Destination.Identity.DisplayName),
            new LocationDescription(input.Destination.Description.Text));
        WorldObjectPlacement replacementPlacement = new(
            input.Target,
            valueEqualDestination);

        Assert.False(CanRecover(input with
        {
            Placements = new WorldObjectPlacementSnapshot([replacementPlacement]),
        }));
        Assert.NotSame(input.Destination, valueEqualDestination);
        Assert.Equal(input.Destination.Identity, valueEqualDestination.Identity);
    }

    [Fact]
    public void PolicyRequiresExactAuthoritativeEvidenceAndVisibleEvidenceIdentity()
    {
        RecoveryInput input = Input();
        DiscoveryEvidence valueEqualEvidence = new(
            new EvidenceId(input.EvidenceDefinition.EvidenceId.Value),
            new ObjectId(input.Target.Identity.Id.Value),
            input.EvidenceDefinition.MinimumVisibilityState,
            input.EvidenceDefinition.IsCritical);
        CharacterEvidenceViewSnapshot valueEqualView = new(
            input.EvidenceView.CharacterId,
            [new EvidenceId(input.EvidenceDefinition.EvidenceId.Value)]);

        Assert.False(CanRecover(input with
        {
            Evidence = new DiscoveryEvidenceSnapshot([]),
        }));
        Assert.False(CanRecover(input with
        {
            Evidence = new DiscoveryEvidenceSnapshot([valueEqualEvidence]),
        }));
        Assert.False(CanRecover(input with
        {
            EvidenceView = new CharacterEvidenceViewSnapshot(
                input.EvidenceView.CharacterId,
                []),
        }));
        Assert.True(CanRecover(input with { EvidenceView = valueEqualView }));
    }

    [Fact]
    public void PolicyRejectsAuthoritativeVisibleEvidenceForAnotherObject()
    {
        RecoveryInput input = Input();
        CharacterObjectSabotageOccurrence occurrence =
            input.Candidate.Candidate.Destination.Occurrence;
        DiscoveryEvidence mismatchedEvidence = new(
            new EvidenceId("misdirected-drag-marks"),
            new ObjectId("storehouse-ledger"),
            VisibilityState.Noticed,
            false);
        CharacterObjectSabotageMoveCandidate mismatchedMoveCandidate = new(
            input.Candidate.Candidate.Destination,
            new CharacterObjectSabotageEvidence(occurrence, mismatchedEvidence));
        CharacterObjectSabotageMoveRecoveryCandidate mismatchedRecoveryCandidate = new(
            mismatchedMoveCandidate,
            input.SourcePlacement);

        Assert.False(CanRecover(input with
        {
            Evidence = new DiscoveryEvidenceSnapshot([mismatchedEvidence]),
            EvidenceView = new CharacterEvidenceViewSnapshot(
                input.EvidenceView.CharacterId,
                [mismatchedEvidence.EvidenceId]),
            Candidate = mismatchedRecoveryCandidate,
        }));
        Assert.Same(occurrence, mismatchedMoveCandidate.Evidence.Occurrence);
        Assert.Same(mismatchedEvidence, mismatchedMoveCandidate.Evidence.Evidence);
    }

    [Fact]
    public void RecoverRestoresExactSourceAtStableIndexAndRetainsExactAuthority()
    {
        RecoveryInput input = Input();
        WorldObject retainedObject = WorldObject("storehouse-ledger");
        WorldObjectPlacement retainedPlacement = new(
            retainedObject,
            Location("storehouse", "Storehouse"));
        WorldObjectPlacementSnapshot placements = new(
            [retainedPlacement, input.MovedPlacement]);

        CharacterObjectSabotageMoveRecoveryResult result =
            CharacterObjectSabotageMoveRecoveryTransition.Recover(
                placements,
                input.Evidence,
                input.EvidenceView,
                input.Candidate);

        Assert.NotSame(placements, result.Placements);
        Assert.Collection(
            result.Placements.Entries,
            placement => Assert.Same(retainedPlacement, placement),
            placement => Assert.Same(input.SourcePlacement, placement));
        Assert.Same(input.EvidenceView, result.EvidenceView);
        Assert.Same(input.Candidate, result.Candidate);
        Assert.Collection(
            placements.Entries,
            placement => Assert.Same(retainedPlacement, placement),
            placement => Assert.Same(input.MovedPlacement, placement));
        Assert.Same(input.EvidenceDefinition, Assert.Single(input.Evidence.Entries));
    }

    [Fact]
    public void RecoverRejectsIneligibleInputWithoutChangingAnything()
    {
        RecoveryInput input = Input();
        CharacterEvidenceViewSnapshot unseen = new(input.EvidenceView.CharacterId, []);

        Assert.Throws<InvalidOperationException>(() =>
            CharacterObjectSabotageMoveRecoveryTransition.Recover(
                input.Placements,
                input.Evidence,
                unseen,
                input.Candidate));

        Assert.Same(input.MovedPlacement, Assert.Single(input.Placements.Entries));
        Assert.Same(input.EvidenceDefinition, Assert.Single(input.Evidence.Entries));
        Assert.Empty(unseen.EvidenceIds);
        Assert.Same(input.SourcePlacement, input.Candidate.SourcePlacement);
    }

    [Fact]
    public void RecoveryContractsRejectNullsAndExposeClosedOperations()
    {
        RecoveryInput input = Input();
        CharacterObjectSabotageMoveRecoveryResult result =
            CharacterObjectSabotageMoveRecoveryTransition.Recover(
                input.Placements,
                input.Evidence,
                input.EvidenceView,
                input.Candidate);

        Assert.Throws<ArgumentNullException>(() =>
            CharacterObjectSabotageMoveRecoveryPolicy.CanRecover(
                null!,
                input.Evidence,
                input.EvidenceView,
                input.Candidate));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterObjectSabotageMoveRecoveryPolicy.CanRecover(
                input.Placements,
                null!,
                input.EvidenceView,
                input.Candidate));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterObjectSabotageMoveRecoveryPolicy.CanRecover(
                input.Placements,
                input.Evidence,
                null!,
                input.Candidate));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterObjectSabotageMoveRecoveryPolicy.CanRecover(
                input.Placements,
                input.Evidence,
                input.EvidenceView,
                null!));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterObjectSabotageMoveRecoveryTransition.Recover(
                null!,
                input.Evidence,
                input.EvidenceView,
                input.Candidate));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterObjectSabotageMoveRecoveryTransition.Recover(
                input.Placements,
                null!,
                input.EvidenceView,
                input.Candidate));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterObjectSabotageMoveRecoveryTransition.Recover(
                input.Placements,
                input.Evidence,
                null!,
                input.Candidate));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterObjectSabotageMoveRecoveryTransition.Recover(
                input.Placements,
                input.Evidence,
                input.EvidenceView,
                null!));
        Assert.Throws<ArgumentNullException>(() =>
            new CharacterObjectSabotageMoveRecoveryResult(
                null!,
                result.EvidenceView,
                result.Candidate));
        Assert.Throws<ArgumentNullException>(() =>
            new CharacterObjectSabotageMoveRecoveryResult(
                result.Placements,
                null!,
                result.Candidate));
        Assert.Throws<ArgumentNullException>(() =>
            new CharacterObjectSabotageMoveRecoveryResult(
                result.Placements,
                result.EvidenceView,
                null!));

        AssertSingleStaticOperation(
            typeof(CharacterObjectSabotageMoveRecoveryPolicy),
            "CanRecover");
        AssertSingleStaticOperation(
            typeof(CharacterObjectSabotageMoveRecoveryTransition),
            "Recover");
        Assert.All(
            typeof(CharacterObjectSabotageMoveRecoveryResult).GetProperties(),
            property => Assert.False(property.CanWrite));
    }

    private static bool CanRecover(RecoveryInput input) =>
        CharacterObjectSabotageMoveRecoveryPolicy.CanRecover(
            input.Placements,
            input.Evidence,
            input.EvidenceView,
            input.Candidate);

    private static void AssertSingleStaticOperation(Type type, string expectedName)
    {
        System.Reflection.MethodInfo method = Assert.Single(
            type.GetMethods().Where(candidate => candidate.DeclaringType == type));

        Assert.Equal(expectedName, method.Name);
        Assert.True(method.IsStatic);
        Assert.Empty(type.GetConstructors());
        Assert.True(type.IsAbstract && type.IsSealed);
    }

    private static RecoveryInput Input()
    {
        Character character = RepresentativeEnemyAgentCharacterSource.Create();
        WorldObject target = Target();
        Location sourceLocation = Location("village-square", "Village square");
        Location destination = Location("abandoned-wagon", "Abandoned wagon");
        CharacterObjectSabotageOccurrence occurrence = new(
            new CharacterObjectSabotageProposal(
                character.Identity.Id,
                new CharacterActionAccessRequirement(
                    target.Identity.Id,
                    "relocate-supplies",
                    character.Knowledge.KnowledgeReferences.Single()),
                CharacterSabotageActionKind.Move),
            target);
        DiscoveryEvidence evidenceDefinition = new(
            new EvidenceId("cask-drag-marks"),
            target.Identity.Id,
            VisibilityState.Noticed,
            false);
        CharacterObjectSabotageMoveCandidate moveCandidate = new(
            new CharacterObjectSabotageMoveDestination(occurrence, destination),
            new CharacterObjectSabotageEvidence(occurrence, evidenceDefinition));
        WorldObjectPlacement sourcePlacement = new(target, sourceLocation);
        CharacterObjectSabotageMoveRecoveryCandidate recoveryCandidate = new(
            moveCandidate,
            sourcePlacement);
        WorldObjectPlacement movedPlacement = new(target, destination);

        return new RecoveryInput(
            target,
            destination,
            sourcePlacement,
            movedPlacement,
            evidenceDefinition,
            new WorldObjectPlacementSnapshot([movedPlacement]),
            new DiscoveryEvidenceSnapshot([evidenceDefinition]),
            new CharacterEvidenceViewSnapshot(
                new CharacterId("seeker"),
                [evidenceDefinition.EvidenceId]),
            recoveryCandidate);
    }

    private static WorldObject Target() => WorldObject("village-water-cask");

    private static WorldObject WorldObject(string objectId) =>
        new(
            new WorldObjectIdentity(
                new ObjectId(objectId),
                $"Object {objectId}"),
            new WorldObjectDescription($"Description for {objectId}."));

    private static Location Location(string id, string displayName) =>
        new(
            new LocationIdentity(new LocationId(id), displayName),
            new LocationDescription($"Description for {id}."));

    private sealed record RecoveryInput(
        WorldObject Target,
        Location Destination,
        WorldObjectPlacement SourcePlacement,
        WorldObjectPlacement MovedPlacement,
        DiscoveryEvidence EvidenceDefinition,
        WorldObjectPlacementSnapshot Placements,
        DiscoveryEvidenceSnapshot Evidence,
        CharacterEvidenceViewSnapshot EvidenceView,
        CharacterObjectSabotageMoveRecoveryCandidate Candidate);
}
