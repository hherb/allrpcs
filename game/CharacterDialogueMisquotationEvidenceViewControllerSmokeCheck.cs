using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks exact scene ownership of one witness-specific misquotation-evidence view.</summary>
public partial class CharacterDialogueMisquotationEvidenceViewControllerSmokeCheck : Node
{
    private static readonly string[] ExpectedMethodNames = ["TryInitialize", "TryRefresh"];

    /// <inheritdoc />
    public override void _Ready()
    {
        TemporaryCharacterDialogueMisquotationEvidenceSource source =
            GetNode<TemporaryCharacterDialogueMisquotationEvidenceSource>(
                "../TemporaryCharacterDialogueMisquotationEvidenceSource");
        CharacterDialogueMisquotationEvidenceExposureSnapshot initial = new([]);
        CharacterDialogueMisquotationEvidenceExposureSnapshot selected = new([source.Candidate]);
        CharacterDialogueMisquotationEvidenceViewController controller = new();

        bool initialized = controller.TryInitialize(initial, source.WitnessCharacterId);
        CharacterDialogueMisquotationEvidenceViewSnapshot? emptyView = controller.View;
        bool refreshed = controller.TryRefresh(initial, selected);
        CharacterDialogueMisquotationEvidenceViewSnapshot? selectedView = controller.View;
        bool accepted = initialized
            && emptyView is not null
            && ReferenceEquals(emptyView.WitnessCharacterId, source.WitnessCharacterId)
            && emptyView.Entries.Count == 0
            && refreshed
            && ReferenceEquals(controller.SourceExposure, selected)
            && selectedView is not null
            && !ReferenceEquals(selectedView, emptyView)
            && ReferenceEquals(selectedView.WitnessCharacterId, source.WitnessCharacterId)
            && ReferenceEquals(selectedView.Entries.Single(), source.Candidate)
            && ReferenceEquals(
                selectedView.Entries.Single().Occurrence.DeliveredEvent,
                source.DeliveredEvent);

        CharacterDialogueMisquotationEvidenceExposureSnapshot replacement = new([]);
        CharacterId valueEqualWitnessId = new(source.WitnessCharacterId.Value);
        bool rejected = !controller.TryInitialize(replacement, source.WitnessCharacterId)
            && !controller.TryRefresh(initial, replacement)
            && !controller.TryRefresh(selected, selected)
            && ReferenceEquals(controller.SourceExposure, selected)
            && ReferenceEquals(controller.View, selectedView)
            && ThrowsArgumentNull(() => controller.TryInitialize(null!, source.WitnessCharacterId))
            && ThrowsArgumentNull(() => controller.TryInitialize(replacement, null!))
            && ThrowsArgumentNull(() => controller.TryRefresh(null!, replacement))
            && ThrowsArgumentNull(() => controller.TryRefresh(selected, null!));

        CharacterDialogueMisquotationEvidenceViewController exactIdentityController = new();
        bool exactIdentity = exactIdentityController.TryInitialize(selected, valueEqualWitnessId)
            && exactIdentityController.View is
            {
                Entries.Count: 0,
                WitnessCharacterId: var retainedWitness,
            }
            && ReferenceEquals(retainedWitness, valueEqualWitnessId)
            && !ReferenceEquals(retainedWitness, source.WitnessCharacterId);

        if (!accepted || !rejected || !exactIdentity || !HasClosedSurface())
        {
            GD.PushError("Misquotation-evidence view controller failed exact scene ownership.");
            controller.Free();
            exactIdentityController.Free();
            GetTree().Quit(1);
            return;
        }

        GD.Print(
            "Misquotation-evidence view controller smoke passed: one-time initialization, exact "
            + "exposure CAS refresh, witness identity filtering, nested reference retention, "
            + "stale and same-snapshot rejection, null guards, and closed surface.");
        controller.Free();
        exactIdentityController.Free();
        GetTree().Quit();
    }

    private static bool HasClosedSurface()
    {
        Type type = typeof(CharacterDialogueMisquotationEvidenceViewController);
        string[] methods = type.GetMethods(
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
                | System.Reflection.BindingFlags.DeclaredOnly)
            .OrderBy(property => property.Name)
            .ToArray();

        return methods.SequenceEqual(ExpectedMethodNames)
            && properties.Select(property => property.Name).SequenceEqual(["SourceExposure", "View"])
            && properties.All(property => property.SetMethod is { IsPrivate: true });
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
}
