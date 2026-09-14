using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Locations;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks one-shot and offline move-decision model supply.</summary>
public partial class TemporaryCharacterObjectSabotageMoveModelSourceSmokeCheck : Node
{
    /// <inheritdoc />
    public override void _Ready()
    {
        TemporaryCharacterObjectSabotageMoveModelSource source = new();
        RecordingModel first = new();
        RecordingModel overlap = new();
        ICharacterObjectSabotageMoveDecisionModel offline = source.TakeModel();
        bool stableOffline = ReferenceEquals(source.TakeModel(), offline);
        bool supplied = source.TrySupplyModel(first);
        bool rejectedOverlap = !source.TrySupplyModel(overlap);
        ICharacterObjectSabotageMoveDecisionModel taken = source.TakeModel();
        bool resumedOffline = ReferenceEquals(source.TakeModel(), offline);
        bool resupplied = source.TrySupplyModel(overlap);

        if (!stableOffline
            || !supplied
            || !rejectedOverlap
            || !ReferenceEquals(taken, first)
            || ReferenceEquals(taken, overlap)
            || !resumedOffline
            || !resupplied
            || !ReferenceEquals(source.TakeModel(), overlap)
            || !ReferenceEquals(source.TakeModel(), offline)
            || !ThrowsArgumentNull(() => source.TrySupplyModel(null!))
            || !HasClosedSurface())
        {
            Fail("Move model source failed exact one-shot or offline selection.");
            return;
        }

        CharacterObjectSabotageMoveConsiderationRequest request = CreateRequest();
        try
        {
            _ = offline.DecideAsync(request, CancellationToken.None).AsTask().GetAwaiter().GetResult();
            Fail("The offline move model returned a decision.");
            return;
        }
        catch (InvalidOperationException)
        {
        }

        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();
        try
        {
            _ = offline.DecideAsync(request, cancellation.Token).AsTask().GetAwaiter().GetResult();
            Fail("The offline move model ignored caller cancellation.");
            return;
        }
        catch (OperationCanceledException)
        {
        }

        try
        {
            _ = offline.DecideAsync(null!, cancellation.Token).AsTask().GetAwaiter().GetResult();
            Fail("The offline move model did not give caller cancellation precedence.");
            return;
        }
        catch (OperationCanceledException)
        {
        }

        if (!ThrowsArgumentNull(
                () => offline.DecideAsync(null!, CancellationToken.None).AsTask().GetAwaiter().GetResult())
            || first.CallCount != 0
            || overlap.CallCount != 0
            || !RequestIsUnchanged(request))
        {
            Fail("Move model selection invoked a model or changed its request.");
            return;
        }

        GD.Print(
            "Move model source smoke passed: one exact one-shot model, overlap rejection, later "
            + "resupply, one stable fail-closed offline model, cancellation, closed surface, and "
            + "no model invocation or request change.");
        source.Free();
        GetTree().Quit();
    }

    private static bool HasClosedSurface()
    {
        Type type = typeof(TemporaryCharacterObjectSabotageMoveModelSource);
        string[] methods = type.GetMethods(
                System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.DeclaredOnly)
            .Where(method => !method.IsSpecialName)
            .Select(method => method.Name)
            .Order(StringComparer.Ordinal)
            .ToArray();

        return methods.SequenceEqual(["TakeModel", "TrySupplyModel"])
            && type.GetProperties(
                System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.DeclaredOnly).Length == 0;
    }

    private static CharacterObjectSabotageMoveConsiderationRequest CreateRequest()
    {
        Character character = RepresentativeEnemyAgentCharacterSource.Create();
        WorldObject target = new(
            new WorldObjectIdentity(new ObjectId("village-water-cask"), "Village water cask"),
            new WorldObjectDescription("A covered cask beside the village well."));
        Location source = new(
            new LocationIdentity(new LocationId("village-square"), "Village square"),
            new LocationDescription("The village square."));
        Location destination = new(
            new LocationIdentity(new LocationId("abandoned-wagon"), "Abandoned wagon"),
            new LocationDescription("An abandoned wagon."));
        CharacterObjectSabotageOccurrence occurrence = new(
            new CharacterObjectSabotageProposal(
                character.Identity.Id,
                new CharacterActionAccessRequirement(
                    target.Identity.Id,
                    "assess-local-influence",
                    character.Knowledge.KnowledgeReferences.Single()),
                CharacterSabotageActionKind.Move),
            target);
        CharacterObjectSabotageMoveCandidate candidate = new(
            new CharacterObjectSabotageMoveDestination(occurrence, destination),
            new CharacterObjectSabotageEvidence(
                occurrence,
                new DiscoveryEvidence(
                    new EvidenceId("cask-drag-marks"),
                    target.Identity.Id,
                    VisibilityState.Noticed,
                    false)));

        return new CharacterObjectSabotageMoveConsiderationRequest(
            character,
            new CharacterPhysicalAccessSnapshot(character.Identity.Id, [target.Identity.Id]),
            new WorldObjectSnapshot([target]),
            new WorldObjectPlacementSnapshot([new WorldObjectPlacement(target, source)]),
            new DiscoveryEvidenceSnapshot([]),
            [candidate]);
    }

    private static bool RequestIsUnchanged(
        CharacterObjectSabotageMoveConsiderationRequest request) =>
        request.Placements.Entries.Count == 1
        && request.Evidence.Entries.Count == 0
        && request.Candidates.Count == 1;

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

    private sealed class RecordingModel : ICharacterObjectSabotageMoveDecisionModel
    {
        public int CallCount { get; private set; }

        public ValueTask<CharacterObjectSabotageMoveDecision> DecideAsync(
            CharacterObjectSabotageMoveConsiderationRequest request,
            CancellationToken cancellationToken)
        {
            CallCount++;
            return ValueTask.FromResult(
                new CharacterObjectSabotageMoveDecision(request.Character.Identity.Id, null));
        }
    }
}
