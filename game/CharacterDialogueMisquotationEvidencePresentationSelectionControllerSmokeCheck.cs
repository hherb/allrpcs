using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks explicit exact-view ownership of one truth-neutral presentation selection.</summary>
public partial class CharacterDialogueMisquotationEvidencePresentationSelectionControllerSmokeCheck
    : Node
{
    private static readonly string[] ExpectedMethodNames = ["TryChoose", "_Ready"];

    /// <inheritdoc />
    public override void _Ready()
    {
        TemporaryCharacterDialogueMisquotationEvidenceSource source =
            GetNode<TemporaryCharacterDialogueMisquotationEvidenceSource>(
                "../TemporaryCharacterDialogueMisquotationEvidenceSource");
        CharacterDialogueMisquotationEvidenceViewController viewController =
            GetNode<CharacterDialogueMisquotationEvidenceViewController>(
                "../CharacterDialogueMisquotationEvidenceViewController");
        CharacterDialogueMisquotationEvidencePresentationSelectionController controller =
            GetNode<CharacterDialogueMisquotationEvidencePresentationSelectionController>(
                "../CharacterDialogueMisquotationEvidencePresentationSelectionController");
        CharacterDialogueMisquotationEvidenceExposureSnapshot initialExposure = new([]);
        CharacterDialogueMisquotationEvidenceExposureSnapshot selectedExposure = new(
            [source.Candidate]);
        CharacterDialogueMisquotationEvidenceViewSnapshot unownedView = new(
            source.WitnessCharacterId,
            []);

        bool inert = controller.Selection is null
            && !controller.TryChoose(unownedView, null)
            && controller.Selection is null;
        if (!viewController.TryInitialize(initialExposure, source.WitnessCharacterId)
            || viewController.View is not { } initialView
            || !controller.TryChoose(initialView, null)
            || controller.Selection is not { } abstention
            || !ReferenceEquals(abstention.WitnessView, initialView)
            || abstention.SelectedEvidence is not null
            || controller.TryChoose(initialView, null))
        {
            Fail("Misquotation-evidence presentation selection did not retain one abstention.");
            return;
        }

        if (!viewController.TryRefresh(initialExposure, selectedExposure)
            || viewController.View is not { } selectedView)
        {
            Fail("Misquotation-evidence presentation selection could not arrange a fresh view.");
            return;
        }

        CharacterDialogueMisquotationEvidence replacement = new(
            source.Candidate.Occurrence,
            new CharacterId(source.WitnessCharacterId.Value));
        CharacterDialogueMisquotationEvidenceViewSnapshot distinctValueEqualView = new(
            source.WitnessCharacterId,
            [source.Candidate]);
        bool rejected = !CharacterDialogueMisquotationEvidencePresentationSelectionPolicy.CanAccept(
                selectedView,
                abstention)
            && !controller.TryChoose(initialView, null)
            && !controller.TryChoose(distinctValueEqualView, source.Candidate)
            && !controller.TryChoose(selectedView, replacement)
            && controller.Selection is null
            && ThrowsArgumentNull(() => controller.TryChoose(null!, null));
        bool selected = controller.TryChoose(selectedView, source.Candidate)
            && controller.Selection is { } selection
            && !ReferenceEquals(selection, abstention)
            && ReferenceEquals(selection.WitnessView, selectedView)
            && ReferenceEquals(selection.SelectedEvidence, source.Candidate)
            && ReferenceEquals(
                selection.SelectedEvidence.Occurrence.Proposal.SourceEvent,
                source.SourceEvent)
            && ReferenceEquals(
                selection.SelectedEvidence.Occurrence.DeliveredEvent,
                source.DeliveredEvent)
            && CharacterDialogueMisquotationEvidencePresentationSelectionPolicy.CanAccept(
                selectedView,
                selection)
            && !controller.TryChoose(selectedView, source.Candidate);

        if (!inert || !rejected || !selected || !HasClosedSurface())
        {
            Fail("Misquotation-evidence presentation selection failed exact-view ownership.");
            return;
        }

        GD.Print(
            "Misquotation-evidence presentation selection controller smoke passed: inert startup, "
            + "one explicit exact-view selection or abstention, stale and value-equal rejection, "
            + "exact nested identity retention, null guards, and closed surface.");
        GetTree().Quit();
    }

    private static bool HasClosedSurface()
    {
        Type type = typeof(CharacterDialogueMisquotationEvidencePresentationSelectionController);
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
            && property.Name == "Selection"
            && property.PropertyType
                == typeof(CharacterDialogueMisquotationEvidencePresentationSelection)
            && !property.CanWrite;
    }

    private static T AssertSingle<T>(IReadOnlyList<T> values)
    {
        if (values.Count != 1)
        {
            throw new InvalidOperationException("Expected exactly one value.");
        }

        return values[0];
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
