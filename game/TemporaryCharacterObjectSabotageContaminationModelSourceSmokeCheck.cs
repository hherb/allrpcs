using Allrpcs.Application.Characters;
using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks configured, one-shot, and offline contamination-decision model supply.</summary>
public partial class TemporaryCharacterObjectSabotageContaminationModelSourceSmokeCheck : Node
{
    /// <inheritdoc />
    public override void _Ready()
    {
        TemporaryCharacterObjectSabotageContaminationModelSource source = new();
        RecordingModel first = new();
        RecordingModel overlap = new();
        RecordingModel configured = new();
        RecordingModel replacement = new();
        CharacterModelSettings settings = new(CharacterModelProvider.OpenAi, "gpt-test");
        CharacterObjectSabotageContaminationModelBinding binding = new(settings, configured);
        CharacterObjectSabotageContaminationModelBinding valueEqualBinding = new(
            settings,
            configured);
        CharacterObjectSabotageContaminationModelBinding replacementBinding = new(
            new CharacterModelSettings(CharacterModelProvider.OpenAi, "replacement-test"),
            replacement);
        ICharacterObjectSabotageContaminationDecisionModel offline = source.TakeModel();
        bool stableOffline = ReferenceEquals(source.TakeModel(), offline);
        bool initialized = source.TryInitializeBinding(binding);
        bool rejectedRepeatedInitialization = !source.TryInitializeBinding(binding);
        bool stableConfigured = ReferenceEquals(source.TakeModel(), configured)
            && ReferenceEquals(source.TakeModel(), configured);
        bool rejectedValueEqualReplacement = !source.TryReplaceBinding(
                valueEqualBinding,
                replacementBinding)
            && ReferenceEquals(source.TakeModel(), configured);
        bool supplied = source.TrySupplyModel(first);
        bool rejectedOverlap = !source.TrySupplyModel(overlap);
        ICharacterObjectSabotageContaminationDecisionModel taken = source.TakeModel();
        bool resumedConfigured = ReferenceEquals(source.TakeModel(), configured);
        bool replaced = source.TryReplaceBinding(binding, replacementBinding);
        bool stableReplacement = ReferenceEquals(source.TakeModel(), replacement)
            && ReferenceEquals(source.TakeModel(), replacement);
        bool rejectedStaleClear = !source.TryClearBinding(binding)
            && ReferenceEquals(source.TakeModel(), replacement);
        bool resupplied = source.TrySupplyModel(overlap);
        bool cleared = source.TryClearBinding(replacementBinding);
        bool rejectedRepeatedClear = !source.TryClearBinding(replacementBinding);
        bool clearPreservedOneShot = ReferenceEquals(source.TakeModel(), overlap)
            && ReferenceEquals(source.TakeModel(), offline);

        if (!stableOffline
            || !initialized
            || !rejectedRepeatedInitialization
            || !stableConfigured
            || !rejectedValueEqualReplacement
            || !supplied
            || !rejectedOverlap
            || !ReferenceEquals(taken, first)
            || ReferenceEquals(taken, overlap)
            || !resumedConfigured
            || !replaced
            || !stableReplacement
            || !rejectedStaleClear
            || !resupplied
            || !cleared
            || !rejectedRepeatedClear
            || !clearPreservedOneShot
            || !ThrowsArgumentNull(() => source.TrySupplyModel(null!))
            || !ThrowsArgumentNull(() => source.TryInitializeBinding(null!))
            || !ThrowsArgumentNull(() => source.TryReplaceBinding(null!, replacementBinding))
            || !ThrowsArgumentNull(() => source.TryReplaceBinding(binding, null!))
            || !ThrowsArgumentNull(() => source.TryClearBinding(null!))
            || !HasClosedSurface())
        {
            Fail("Contamination model source failed configured, one-shot, or offline selection.");
            return;
        }

        CharacterObjectSabotageContaminationConsiderationRequest request = CreateRequest();
        try
        {
            _ = offline.DecideAsync(request, CancellationToken.None).AsTask().GetAwaiter().GetResult();
            Fail("The offline contamination model returned a decision.");
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
            Fail("The offline contamination model ignored caller cancellation.");
            return;
        }
        catch (OperationCanceledException)
        {
        }

        if (!ThrowsArgumentNull(
                () => offline.DecideAsync(null!, CancellationToken.None).AsTask().GetAwaiter().GetResult())
            || first.CallCount != 0
            || overlap.CallCount != 0
            || configured.CallCount != 0
            || replacement.CallCount != 0
            || request.Contamination.ContaminatedObjectIds.Count != 0
            || request.Evidence.Entries.Count != 0
            || request.Candidates.Count != 1)
        {
            Fail("Contamination model selection invoked a model or changed its request.");
            return;
        }

        GD.Print(
            "Contamination model source smoke passed: exact initialized and replaced binding, "
            + "one exact one-shot model, exact stale-safe clearing, one stable fail-closed "
            + "offline model, cancellation, closed surface, and no model input.");
        source.Free();
        GetTree().Quit();
    }

    private static bool HasClosedSurface()
    {
        Type type = typeof(TemporaryCharacterObjectSabotageContaminationModelSource);
        string[] methods = type.GetMethods(
                System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.DeclaredOnly)
            .Where(method => !method.IsSpecialName)
            .Select(method => method.Name)
            .Order(StringComparer.Ordinal)
            .ToArray();

        return methods.SequenceEqual(
                [
                    "TakeModel",
                    "TryClearBinding",
                    "TryInitializeBinding",
                    "TryReplaceBinding",
                    "TrySupplyModel",
                ])
            && type.GetProperties(
                System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.DeclaredOnly).Length == 0;
    }

    private static CharacterObjectSabotageContaminationConsiderationRequest CreateRequest()
    {
        Character character = RepresentativeEnemyAgentCharacterSource.Create();
        WorldObject target = new(
            new WorldObjectIdentity(new ObjectId("village-water-cask"), "Village water cask"),
            new WorldObjectDescription("A covered cask beside the village well."));
        CharacterObjectSabotageOccurrence occurrence = new(
            new CharacterObjectSabotageProposal(
                character.Identity.Id,
                new CharacterActionAccessRequirement(
                    target.Identity.Id,
                    "assess-local-influence",
                    character.Knowledge.KnowledgeReferences.Single()),
                CharacterSabotageActionKind.Contaminate),
            target);
        CharacterObjectSabotageEvidence candidate = new(
            occurrence,
            new DiscoveryEvidence(
                new EvidenceId("bitter-cask-residue"),
                target.Identity.Id,
                VisibilityState.Noticed,
                false));

        return new CharacterObjectSabotageContaminationConsiderationRequest(
            character,
            new CharacterPhysicalAccessSnapshot(character.Identity.Id, [target.Identity.Id]),
            new WorldObjectSnapshot([target]),
            new WorldObjectContaminationSnapshot([]),
            new DiscoveryEvidenceSnapshot([]),
            [candidate]);
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

    private sealed class RecordingModel : ICharacterObjectSabotageContaminationDecisionModel
    {
        public int CallCount { get; private set; }

        public ValueTask<CharacterObjectSabotageContaminationDecision> DecideAsync(
            CharacterObjectSabotageContaminationConsiderationRequest request,
            CancellationToken cancellationToken)
        {
            _ = cancellationToken;
            CallCount++;
            return ValueTask.FromResult(
                new CharacterObjectSabotageContaminationDecision(request.Character.Identity.Id, null));
        }
    }
}
