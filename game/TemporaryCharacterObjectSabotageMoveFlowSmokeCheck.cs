using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks one explicit authored move and evidence-gated recovery flow.</summary>
public partial class TemporaryCharacterObjectSabotageMoveFlowSmokeCheck : Node
{
    private static readonly string[] ExpectedMethodNames = ["TryApply", "TryRecover", "_Ready"];

    /// <inheritdoc />
    public override void _Ready() => SetProcess(true);

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        SetProcess(false);

        TemporaryCharacterObjectSabotageMoveSource source =
            GetNode<TemporaryCharacterObjectSabotageMoveSource>(
                "../TemporaryCharacterObjectSabotageMoveSource");
        CharacterObjectSabotageMoveController state =
            GetNode<CharacterObjectSabotageMoveController>(
                "../CharacterObjectSabotageMoveController");
        TemporaryCharacterObjectSabotageMoveFlowController flow =
            GetNode<TemporaryCharacterObjectSabotageMoveFlowController>(
                "../TemporaryCharacterObjectSabotageMoveFlowController");
        WorldObjectPlacementSnapshot initialPlacements = source.Placements;
        DiscoveryEvidenceSnapshot initialEvidence = source.Evidence;
        WorldObjectPlacement sourcePlacement = initialPlacements.Entries.Single();

        if (!ReferenceEquals(state.Placements, initialPlacements)
            || !ReferenceEquals(state.Evidence, initialEvidence)
            || !ReferenceEquals(sourcePlacement, source.RecoveryCandidate.SourcePlacement)
            || initialEvidence.Entries.Count != 0)
        {
            Fail("The temporary move flow did not initialize exact source state inertly.");
            return;
        }

        if (!flow.TryApply()
            || state.Placements is not { } movedPlacements
            || state.Evidence is not { } movedEvidence
            || ReferenceEquals(movedPlacements, initialPlacements)
            || ReferenceEquals(movedEvidence, initialEvidence)
            || !ReferenceEquals(movedPlacements.Entries.Single().WorldObject, sourcePlacement.WorldObject)
            || !ReferenceEquals(movedPlacements.Entries.Single().Location, source.Destination)
            || !ReferenceEquals(movedEvidence.Entries.Single(), source.Candidate.Evidence.Evidence))
        {
            Fail("The temporary move flow did not install one exact explicit move and evidence.");
            return;
        }

        if (flow.TryApply()
            || !ReferenceEquals(state.Placements, movedPlacements)
            || !ReferenceEquals(state.Evidence, movedEvidence)
            || !HasUnchangedSource(source, initialPlacements, initialEvidence, sourcePlacement))
        {
            Fail("The temporary move flow did not reject repeat application atomically.");
            return;
        }

        CharacterPerceptionSnapshot unseen = new(new CharacterId("seeker"), []);
        if (flow.TryRecover(unseen)
            || !ThrowsArgumentNull(() => flow.TryRecover(null!))
            || !ReferenceEquals(state.Placements, movedPlacements)
            || !ReferenceEquals(state.Evidence, movedEvidence))
        {
            Fail("The temporary move flow changed state without noticed exact evidence.");
            return;
        }

        CharacterPerceptionSnapshot noticed = new(
            unseen.CharacterId,
            [new WorldObjectVisibility(
                sourcePlacement.WorldObject.Identity.Id,
                VisibilityState.Noticed)]);
        if (!flow.TryRecover(noticed)
            || state.Placements is not { } recoveredPlacements
            || ReferenceEquals(recoveredPlacements, movedPlacements)
            || !ReferenceEquals(recoveredPlacements.Entries.Single(), sourcePlacement)
            || !ReferenceEquals(state.Evidence, movedEvidence)
            || flow.TryRecover(noticed)
            || !ReferenceEquals(state.Placements, recoveredPlacements)
            || !ReferenceEquals(state.Evidence, movedEvidence)
            || !HasUnchangedSource(source, initialPlacements, initialEvidence, sourcePlacement)
            || !HasClosedFlowSurface())
        {
            Fail("The temporary move flow did not recover only the exact retained source placement.");
            return;
        }

        GD.Print(
            "Temporary move sabotage flow smoke passed: inert exact initialization, explicit "
            + "atomic move, repeat and unseen rejection, perception-gated recovery, retained "
            + "evidence, unchanged source state, and closed behavior.");
        GetTree().Quit();
    }

    private static bool HasUnchangedSource(
        TemporaryCharacterObjectSabotageMoveSource source,
        WorldObjectPlacementSnapshot placements,
        DiscoveryEvidenceSnapshot evidence,
        WorldObjectPlacement sourcePlacement) =>
        ReferenceEquals(source.Placements, placements)
        && ReferenceEquals(source.Evidence, evidence)
        && ReferenceEquals(source.Placements.Entries.Single(), sourcePlacement)
        && source.Evidence.Entries.Count == 0;

    private static bool HasClosedFlowSurface()
    {
        Type type = typeof(TemporaryCharacterObjectSabotageMoveFlowController);
        string[] methodNames = type.GetMethods(
                System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.DeclaredOnly)
            .Where(method => !method.IsSpecialName)
            .Select(method => method.Name)
            .Order(StringComparer.Ordinal)
            .ToArray();
        System.Reflection.PropertyInfo[] properties = type.GetProperties(
            System.Reflection.BindingFlags.Public
            | System.Reflection.BindingFlags.Instance
            | System.Reflection.BindingFlags.DeclaredOnly);

        return methodNames.SequenceEqual(ExpectedMethodNames) && properties.Length == 0;
    }

    private static bool ThrowsArgumentNull(Action action)
    {
        try
        {
            action();
            return false;
        }
        catch (ArgumentNullException)
        {
            return true;
        }
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(1);
    }
}
