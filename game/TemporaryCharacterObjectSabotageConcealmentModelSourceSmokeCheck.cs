using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks exact one-shot and stable offline concealment-decision model supply.</summary>
public partial class TemporaryCharacterObjectSabotageConcealmentModelSourceSmokeCheck : Node
{
    /// <inheritdoc />
    public override void _Ready()
    {
        TemporaryCharacterObjectSabotageConcealmentModelSource source = new();
        RecordingModel first = new();
        RecordingModel overlap = new();
        bool supplied = source.TrySupplyModel(first);
        bool rejectedOverlap = !source.TrySupplyModel(overlap);
        ICharacterObjectSabotageConcealmentDecisionModel taken = source.TakeModel();
        ICharacterObjectSabotageConcealmentDecisionModel offline = source.TakeModel();
        ICharacterObjectSabotageConcealmentDecisionModel repeatedOffline = source.TakeModel();

        if (!supplied
            || !rejectedOverlap
            || !ReferenceEquals(taken, first)
            || ReferenceEquals(taken, overlap)
            || !ReferenceEquals(offline, repeatedOffline)
            || !source.TrySupplyModel(overlap)
            || !ReferenceEquals(source.TakeModel(), overlap)
            || !ThrowsArgumentNull(() => source.TrySupplyModel(null!))
            || !HasClosedSurface())
        {
            Fail("Concealment model source failed exact one-shot or stable offline selection.");
            return;
        }

        CharacterObjectSabotageConcealmentConsiderationRequest request = CreateRequest();
        try
        {
            _ = offline.DecideAsync(request, CancellationToken.None).AsTask().GetAwaiter().GetResult();
            Fail("The offline concealment model returned a decision.");
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
            Fail("The offline concealment model ignored caller cancellation.");
            return;
        }
        catch (OperationCanceledException)
        {
        }

        if (first.CallCount != 0 || !RequestIsUnchanged(request))
        {
            Fail("Concealment model selection invoked a model or changed its request.");
            return;
        }

        GD.Print(
            "Concealment model source smoke passed: one exact one-shot model, overlap rejection, "
            + "one stable fail-closed offline model, cancellation, closed surface, and no input.");
        source.Free();
        GetTree().Quit();
    }

    private static bool HasClosedSurface()
    {
        Type type = typeof(TemporaryCharacterObjectSabotageConcealmentModelSource);
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

    private static CharacterObjectSabotageConcealmentConsiderationRequest CreateRequest()
    {
        Character character = RepresentativeEnemyAgentCharacterSource.Create();
        WorldObject target = new(
            new WorldObjectIdentity(new ObjectId("village-water-cask"), "Village water cask"),
            new WorldObjectDescription("A covered cask beside the village well."));
        CharacterObjectSabotageEvidence candidate = new(
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
                new EvidenceId("disturbed-cask-cover"),
                target.Identity.Id,
                VisibilityState.Noticed,
                false));

        return new CharacterObjectSabotageConcealmentConsiderationRequest(
            character,
            new CharacterPhysicalAccessSnapshot(character.Identity.Id, [target.Identity.Id]),
            new WorldObjectSnapshot([target]),
            new WorldObjectConcealmentSnapshot([]),
            new DiscoveryEvidenceSnapshot([]),
            [candidate]);
    }

    private static bool RequestIsUnchanged(
        CharacterObjectSabotageConcealmentConsiderationRequest request) =>
        request.Concealment.ConcealedObjectIds.Count == 0
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

    private sealed class RecordingModel : ICharacterObjectSabotageConcealmentDecisionModel
    {
        public int CallCount { get; private set; }

        public ValueTask<CharacterObjectSabotageConcealmentDecision> DecideAsync(
            CharacterObjectSabotageConcealmentConsiderationRequest request,
            CancellationToken cancellationToken)
        {
            CallCount++;
            return ValueTask.FromResult(
                new CharacterObjectSabotageConcealmentDecision(
                    request.Character.Identity.Id,
                    null));
        }
    }
}
