using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks one-shot and offline substitution-decision model supply.</summary>
public partial class TemporaryCharacterObjectSabotageSubstitutionModelSourceSmokeCheck : Node
{
    /// <inheritdoc />
    public override void _Ready()
    {
        TemporaryCharacterObjectSabotageSubstitutionModelSource source = new();
        RecordingModel first = new();
        RecordingModel overlap = new();
        ICharacterObjectSabotageSubstitutionDecisionModel offline = source.TakeModel();
        bool stableOffline = ReferenceEquals(source.TakeModel(), offline);
        bool supplied = source.TrySupplyModel(first);
        bool rejectedOverlap = !source.TrySupplyModel(overlap);
        ICharacterObjectSabotageSubstitutionDecisionModel taken = source.TakeModel();
        bool resumedOffline = ReferenceEquals(source.TakeModel(), offline);

        if (!stableOffline
            || !supplied
            || !rejectedOverlap
            || !ReferenceEquals(taken, first)
            || ReferenceEquals(taken, overlap)
            || !resumedOffline
            || !ThrowsArgumentNull(() => source.TrySupplyModel(null!))
            || !HasClosedSurface())
        {
            Fail("Substitution model source failed one-shot or offline selection.");
            return;
        }

        CharacterObjectSabotageSubstitutionConsiderationRequest request = CreateRequest();
        try
        {
            _ = offline.DecideAsync(request, CancellationToken.None).AsTask().GetAwaiter().GetResult();
            Fail("The offline substitution model returned a decision.");
            return;
        }
        catch (InvalidOperationException)
        {
        }

        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();
        try
        {
            _ = offline.DecideAsync(null!, cancellation.Token).AsTask().GetAwaiter().GetResult();
            Fail("The offline substitution model ignored caller cancellation.");
            return;
        }
        catch (OperationCanceledException)
        {
        }

        if (!ThrowsArgumentNull(
                () => offline.DecideAsync(null!, CancellationToken.None).AsTask().GetAwaiter().GetResult())
            || first.CallCount != 0
            || overlap.CallCount != 0
            || request.Substitutions.Entries.Count != 0
            || request.Evidence.Entries.Count != 0
            || request.Candidates.Count != 1)
        {
            Fail("Substitution model selection invoked a model or changed its request.");
            return;
        }

        GD.Print(
            "Substitution model source smoke passed: one exact one-shot model, one stable "
            + "fail-closed offline model, cancellation, closed surface, and no model input.");
        source.Free();
        GetTree().Quit();
    }

    private static bool HasClosedSurface()
    {
        Type type = typeof(TemporaryCharacterObjectSabotageSubstitutionModelSource);
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

    private static CharacterObjectSabotageSubstitutionConsiderationRequest CreateRequest()
    {
        Character character = RepresentativeEnemyAgentCharacterSource.Create();
        WorldObject target = WorldObject("village-water-cask", "Village water cask");
        CharacterObjectSabotageOccurrence occurrence = new(
            new CharacterObjectSabotageProposal(
                character.Identity.Id,
                new CharacterActionAccessRequirement(
                    target.Identity.Id,
                    "assess-local-influence",
                    character.Knowledge.KnowledgeReferences.Single()),
                CharacterSabotageActionKind.Substitute),
            target);
        CharacterObjectSabotageSubstitutionCandidate candidate = new(
            new CharacterObjectSabotageSubstitution(
                occurrence,
                WorldObject("tainted-water-cask", "Tainted water cask")),
            new CharacterObjectSabotageEvidence(
                occurrence,
                new DiscoveryEvidence(
                    new EvidenceId("water-cask-substitution-signs"),
                    target.Identity.Id,
                    VisibilityState.Noticed,
                    false)));

        return new CharacterObjectSabotageSubstitutionConsiderationRequest(
            character,
            new CharacterPhysicalAccessSnapshot(character.Identity.Id, [target.Identity.Id]),
            new WorldObjectSnapshot([target]),
            new CharacterObjectSabotageSubstitutionSnapshot([]),
            new DiscoveryEvidenceSnapshot([]),
            [candidate]);
    }

    private static WorldObject WorldObject(string id, string name) =>
        new(new WorldObjectIdentity(new ObjectId(id), name), new WorldObjectDescription(name));

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

    private sealed class RecordingModel : ICharacterObjectSabotageSubstitutionDecisionModel
    {
        public int CallCount { get; private set; }

        public ValueTask<CharacterObjectSabotageSubstitutionDecision> DecideAsync(
            CharacterObjectSabotageSubstitutionConsiderationRequest request,
            CancellationToken cancellationToken)
        {
            _ = cancellationToken;
            CallCount++;
            return ValueTask.FromResult(
                new CharacterObjectSabotageSubstitutionDecision(request.Character.Identity.Id, null));
        }
    }
}
