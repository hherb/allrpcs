using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks exact scene ownership of exposed misquotation evidence.</summary>
public partial class CharacterDialogueMisquotationEvidenceExposureControllerSmokeCheck : Node
{
    private static readonly string[] ExpectedMethodNames = ["TryExpose", "TryInitialize"];

    /// <inheritdoc />
    public override void _Ready()
    {
        TemporaryCharacterDialogueMisquotationEvidenceSource source =
            GetNode<TemporaryCharacterDialogueMisquotationEvidenceSource>(
                "../TemporaryCharacterDialogueMisquotationEvidenceSource");
        CharacterDialogueMisquotationEvidenceExposureSnapshot initial = new([]);
        CharacterDialogueMisquotationEvidenceConsiderationResult selection = Result(
            source,
            source.Candidate);
        CharacterDialogueMisquotationEvidenceConsiderationResult abstention = Result(source, null);
        CharacterDialogueMisquotationEvidenceExposureController controller = new();

        bool initialized = controller.TryInitialize(initial);
        bool exposed = controller.TryExpose(initial, selection);
        CharacterDialogueMisquotationEvidenceExposureSnapshot? selected = controller.Exposure;
        bool accepted = initialized
            && exposed
            && selected is not null
            && !ReferenceEquals(selected, initial)
            && ReferenceEquals(selected.Entries.Single(), source.Candidate)
            && ReferenceEquals(
                selected.Entries.Single().Occurrence.Proposal.SourceEvent,
                source.SourceEvent)
            && ReferenceEquals(
                selected.Entries.Single().Occurrence.DeliveredEvent,
                source.DeliveredEvent)
            && !controller.TryExpose(initial, selection)
            && !controller.TryInitialize(new CharacterDialogueMisquotationEvidenceExposureSnapshot([]));

        CharacterDialogueMisquotationEvidenceExposureController abstainingController = new();
        bool abstained = abstainingController.TryInitialize(initial)
            && abstainingController.TryExpose(initial, abstention)
            && ReferenceEquals(abstainingController.Exposure, initial)
            && !abstainingController.TryExpose(initial, abstention);

        CharacterDialogueMisquotationEvidenceExposureController rejectingController = new();
        CharacterDialogueMisquotationEvidenceExposureSnapshot duplicate = new([source.Candidate]);
        CharacterDialogueMisquotationEvidenceExposureSnapshot replacement = new([]);
        bool rejected = !rejectingController.TryExpose(initial, selection)
            && rejectingController.TryInitialize(duplicate)
            && !rejectingController.TryExpose(replacement, selection)
            && !rejectingController.TryExpose(duplicate, selection)
            && ReferenceEquals(rejectingController.Exposure, duplicate)
            && ThrowsArgumentNull(() => rejectingController.TryInitialize(null!))
            && ThrowsArgumentNull(() => rejectingController.TryExpose(null!, selection))
            && ThrowsArgumentNull(() => rejectingController.TryExpose(duplicate, null!));

        if (!accepted || !abstained || !rejected || !HasClosedSurface())
        {
            GD.PushError("Misquotation-evidence exposure controller failed exact scene ownership.");
            controller.Free();
            abstainingController.Free();
            rejectingController.Free();
            GetTree().Quit(1);
            return;
        }

        GD.Print(
            "Misquotation-evidence exposure controller smoke passed: one-time initialization, "
            + "exact CAS selection, one-shot abstention, duplicate and stale rejection, nested "
            + "reference retention, closed surface, and atomic failure.");
        controller.Free();
        abstainingController.Free();
        rejectingController.Free();
        GetTree().Quit();
    }

    private static CharacterDialogueMisquotationEvidenceConsiderationResult Result(
        TemporaryCharacterDialogueMisquotationEvidenceSource source,
        CharacterDialogueMisquotationEvidence? selectedEvidence)
    {
        CharacterDialogueMisquotationEvidenceConsiderationRequest request = new(
            source.Actor,
            [source.Candidate]);
        return new(
            request,
            new CharacterDialogueMisquotationEvidenceDecision(
                source.Actor.Identity.Id,
                selectedEvidence));
    }

    private static bool HasClosedSurface()
    {
        Type type = typeof(CharacterDialogueMisquotationEvidenceExposureController);
        string[] methods = type.GetMethods(
                System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.DeclaredOnly)
            .Where(method => !method.IsSpecialName)
            .Select(method => method.Name)
            .Order(StringComparer.Ordinal)
            .ToArray();
        System.Reflection.PropertyInfo property = AssertSingle(type.GetProperties(
            System.Reflection.BindingFlags.Public
            | System.Reflection.BindingFlags.Instance
            | System.Reflection.BindingFlags.DeclaredOnly));

        return methods.SequenceEqual(ExpectedMethodNames)
            && property.Name == "Exposure"
            && property.SetMethod is { IsPrivate: true };
    }

    private static T AssertSingle<T>(IEnumerable<T> values) => values.Single();

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
