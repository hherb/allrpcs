using Allrpcs.Application.Characters;
using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks configured, one-shot, and offline influence-recovery model supply.</summary>
public partial class TemporaryCharacterTargetSabotageInfluenceRecoveryModelSourceSmokeCheck : Node
{
    /// <inheritdoc />
    public override void _Ready()
    {
        TemporaryCharacterTargetSabotageInfluenceRecoveryModelSource source = new();
        RecordingModel first = new();
        RecordingModel overlap = new();
        RecordingModel configured = new();
        RecordingModel replacement = new();
        CharacterModelSettings settings = new(CharacterModelProvider.OpenAi, "gpt-test");
        CharacterTargetSabotageInfluenceRecoveryModelBinding binding = new(settings, configured);
        CharacterTargetSabotageInfluenceRecoveryModelBinding valueEqualBinding = new(
            settings,
            configured);
        CharacterTargetSabotageInfluenceRecoveryModelBinding replacementBinding = new(
            new CharacterModelSettings(CharacterModelProvider.OpenAi, "replacement-test"),
            replacement);
        ICharacterTargetSabotageInfluenceRecoveryDecisionModel offline = source.TakeModel();
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
        ICharacterTargetSabotageInfluenceRecoveryDecisionModel taken = source.TakeModel();
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
            Fail("Influence-recovery model source failed configured, one-shot, or offline selection.");
            return;
        }

        Character actor = RepresentativeSageCharacterSource.Create();
        Character enemy = RepresentativeEnemyAgentCharacterSource.Create();
        CharacterTargetSabotageInfluenceRecoveryConsiderationRequest request = new(
            actor,
            new CharacterRelationshipMemory(
                actor.Identity.Id,
                enemy.Identity.Id,
                [new CharacterRelationshipDimensionValue(CharacterRelationshipDimension.Trust, 10)],
                []),
            []);
        try
        {
            _ = offline.DecideAsync(request, CancellationToken.None).AsTask().GetAwaiter().GetResult();
            Fail("The offline influence-recovery model returned a decision.");
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
            Fail("The offline influence-recovery model ignored caller cancellation.");
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
            || !ReferenceEquals(request.Actor, actor)
            || !ReferenceEquals(request.Relationship.CharacterId, actor.Identity.Id)
            || request.Candidates.Count != 0)
        {
            Fail("Influence-recovery model selection invoked a model or changed its request.");
            return;
        }

        GD.Print(
            "Influence-recovery model source smoke passed: exact initialized and replaced binding, "
            + "one exact one-shot model, exact stale-safe clearing, one stable fail-closed offline "
            + "model, cancellation, closed surface, and no model input.");
        source.Free();
        GetTree().Quit();
    }

    private static bool HasClosedSurface()
    {
        Type type = typeof(TemporaryCharacterTargetSabotageInfluenceRecoveryModelSource);
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

    private sealed class RecordingModel : ICharacterTargetSabotageInfluenceRecoveryDecisionModel
    {
        public int CallCount { get; private set; }

        public ValueTask<CharacterTargetSabotageInfluenceRecoveryDecision> DecideAsync(
            CharacterTargetSabotageInfluenceRecoveryConsiderationRequest request,
            CancellationToken cancellationToken)
        {
            _ = cancellationToken;
            CallCount++;
            return ValueTask.FromResult(
                new CharacterTargetSabotageInfluenceRecoveryDecision(
                    request.Actor.Identity.Id,
                    null));
        }
    }
}
