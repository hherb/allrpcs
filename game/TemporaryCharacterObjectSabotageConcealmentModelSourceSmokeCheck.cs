using Allrpcs.Application.Characters;
using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks configured, one-shot, and offline concealment-decision model supply.</summary>
public partial class TemporaryCharacterObjectSabotageConcealmentModelSourceSmokeCheck : Node
{
    /// <inheritdoc />
    public override void _Ready()
    {
        TemporaryCharacterObjectSabotageConcealmentModelSource source = new();
        RecordingModel first = new();
        RecordingModel overlap = new();
        RecordingModel configured = new();
        RecordingModel replacement = new();
        CharacterModelSettings settings = new(CharacterModelProvider.OpenAi, "gpt-test");
        CharacterObjectSabotageConcealmentModelBinding binding = new(settings, configured);
        CharacterObjectSabotageConcealmentModelBinding mismatchedBinding = new(
            settings,
            new RecordingModel());
        CharacterObjectSabotageConcealmentModelBinding replacementBinding = new(
            new CharacterModelSettings(CharacterModelProvider.OpenAi, "replacement-test"),
            replacement);
        ICharacterObjectSabotageConcealmentDecisionModel offline = source.TakeModel();
        bool stableOffline = ReferenceEquals(source.TakeModel(), offline);
        bool initialized = source.TryInitializeBinding(binding);
        bool rejectedRepeatedInitialization = !source.TryInitializeBinding(binding);
        bool stableConfigured = ReferenceEquals(source.TakeModel(), configured)
            && ReferenceEquals(source.TakeModel(), configured);
        bool rejectedMismatchedReplacement = !source.TryReplaceBinding(
                mismatchedBinding,
                replacementBinding)
            && ReferenceEquals(source.TakeModel(), configured);
        bool supplied = source.TrySupplyModel(first);
        bool rejectedOverlap = !source.TrySupplyModel(overlap);
        ICharacterObjectSabotageConcealmentDecisionModel taken = source.TakeModel();
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
            || !rejectedMismatchedReplacement
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
            Fail("Concealment model source failed configured, one-shot, or offline selection.");
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

        if (first.CallCount != 0
            || overlap.CallCount != 0
            || configured.CallCount != 0
            || replacement.CallCount != 0
            || !RequestIsUnchanged(request))
        {
            Fail("Concealment model selection invoked a model or changed its request.");
            return;
        }

        GD.Print(
            "Concealment model source smoke passed: exact initialized and replaced binding, one "
            + "exact one-shot model, exact stale-safe clearing, one stable fail-closed offline "
            + "model, cancellation, closed surface, and no model input.");
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
