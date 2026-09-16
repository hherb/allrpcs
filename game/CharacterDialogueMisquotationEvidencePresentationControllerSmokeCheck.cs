using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks exact scene ownership of truth-neutral misquotation-evidence presentation.</summary>
public partial class CharacterDialogueMisquotationEvidencePresentationControllerSmokeCheck : Node
{
    private static readonly string[] ExpectedMethodNames = ["TryAcknowledge", "TryPresent", "_Ready"];

    /// <inheritdoc />
    public override void _Ready()
    {
        TemporaryCharacterDialogueMisquotationEvidenceSource source =
            GetNode<TemporaryCharacterDialogueMisquotationEvidenceSource>(
                "../TemporaryCharacterDialogueMisquotationEvidenceSource");
        CharacterDialogueMisquotationEvidenceViewController viewController =
            GetNode<CharacterDialogueMisquotationEvidenceViewController>(
                "../CharacterDialogueMisquotationEvidenceViewController");
        CharacterDialogueMisquotationEvidencePresentationSelectionController selectionController =
            GetNode<CharacterDialogueMisquotationEvidencePresentationSelectionController>(
                "../CharacterDialogueMisquotationEvidencePresentationSelectionController");
        CharacterDialogueMisquotationEvidencePresentationController controller =
            GetNode<CharacterDialogueMisquotationEvidencePresentationController>(
                "../CharacterDialogueMisquotationEvidencePresentationController");
        CharacterDialogueMisquotationEvidenceExposureSnapshot initialExposure = new([]);
        CharacterDialogueMisquotationEvidenceExposureSnapshot selectedExposure = new(
            [source.Candidate]);

        CharacterDialogueMisquotationEvidenceViewSnapshot unownedView = new(
            source.WitnessCharacterId,
            []);
        CharacterDialogueMisquotationEvidencePresentationSelection unownedSelection = new(
            unownedView,
            null);
        bool inert = controller.Presentation is null
            && !controller.TryPresent(unownedView, unownedSelection)
            && controller.Presentation is null;

        if (!viewController.TryInitialize(initialExposure, source.WitnessCharacterId)
            || viewController.View is not { } initialView
            || !selectionController.TryChoose(initialView, null)
            || selectionController.Selection is not { } abstention
            || !controller.TryPresent(initialView, abstention)
            || controller.Presentation is not null
            || controller.TryPresent(initialView, abstention))
        {
            Fail("Misquotation-evidence presentation did not retain one explicit abstention.");
            return;
        }

        if (!viewController.TryRefresh(initialExposure, selectedExposure)
            || viewController.View is not { } selectedView
            || controller.Presentation is not null
            || !selectionController.TryChoose(selectedView, source.Candidate)
            || selectionController.Selection is not { } selection)
        {
            Fail("Misquotation-evidence presentation could not arrange one fresh selection.");
            return;
        }

        CharacterDialogueMisquotationEvidenceViewSnapshot distinctEquivalentView = new(
            source.WitnessCharacterId,
            [source.Candidate]);
        CharacterDialogueMisquotationEvidencePresentationSelection distinctSelection = new(
            selectedView,
            source.Candidate);
        CharacterDialogueMisquotationEvidencePresentationSelection selectedViewAbstention = new(
            selectedView,
            null);
        bool rejected = !controller.TryPresent(initialView, abstention)
            && !controller.TryPresent(distinctEquivalentView, selection)
            && !controller.TryPresent(selectedView, distinctSelection)
            && controller.Presentation is null
            && ThrowsArgumentNull(() => controller.TryPresent(null!, selection))
            && ThrowsArgumentNull(() => controller.TryPresent(selectedView, null!));
        bool presented = controller.TryPresent(selectedView, selection)
            && controller.Presentation is { } presentation
            && ReferenceEquals(presentation.Selection, selection)
            && ReferenceEquals(presentation.WitnessCharacterId, source.WitnessCharacterId)
            && ReferenceEquals(presentation.SourceEvent, source.SourceEvent)
            && ReferenceEquals(presentation.DeliveredEvent, source.DeliveredEvent)
            && ReferenceEquals(selectedView.Entries.Single(), source.Candidate)
            && !controller.TryPresent(selectedView, selection);

        CharacterDialogueMisquotationEvidencePresentationSnapshot presentedSnapshot =
            controller.Presentation!;
        CharacterDialogueMisquotationEvidencePresentationSnapshot distinctPresentation = new(
            distinctSelection);
        bool acknowledgementRejected = !controller.TryAcknowledge(
                initialView,
                selection,
                presentedSnapshot)
            && !controller.TryAcknowledge(
                distinctEquivalentView,
                selection,
                presentedSnapshot)
            && !controller.TryAcknowledge(
                selectedView,
                distinctSelection,
                presentedSnapshot)
            && !controller.TryAcknowledge(
                selectedView,
                selectedViewAbstention,
                presentedSnapshot)
            && !controller.TryAcknowledge(
                selectedView,
                selection,
                distinctPresentation)
            && ThrowsArgumentNull(() => controller.TryAcknowledge(
                null!,
                selection,
                presentedSnapshot))
            && ThrowsArgumentNull(() => controller.TryAcknowledge(
                selectedView,
                null!,
                presentedSnapshot))
            && ThrowsArgumentNull(() => controller.TryAcknowledge(
                selectedView,
                selection,
                null!));
        bool acknowledged = controller.TryAcknowledge(
                selectedView,
                selection,
                presentedSnapshot)
            && controller.Presentation is null
            && ReferenceEquals(viewController.View, selectedView)
            && ReferenceEquals(selectionController.Selection, selection)
            && ReferenceEquals(selectedView.Entries.Single(), source.Candidate)
            && ReferenceEquals(source.Candidate.Occurrence.Proposal.SourceEvent, source.SourceEvent)
            && ReferenceEquals(source.Candidate.Occurrence.DeliveredEvent, source.DeliveredEvent)
            && !controller.TryAcknowledge(selectedView, selection, presentedSnapshot)
            && !controller.TryPresent(selectedView, selection);
        CharacterDialogueMisquotationEvidenceExposureSnapshot laterExposure = new(
            [source.Candidate]);
        CharacterDialogueMisquotationEvidenceExposureSnapshot clearedExposure = new([]);
        bool laterStaleHidden = viewController.TryRefresh(selectedExposure, laterExposure)
            && viewController.View is { } laterView
            && selectionController.TryChoose(laterView, source.Candidate)
            && selectionController.Selection is { } laterSelection
            && controller.TryPresent(laterView, laterSelection)
            && controller.Presentation is not null
            && viewController.TryRefresh(laterExposure, clearedExposure)
            && controller.Presentation is null;

        if (!inert
            || !rejected
            || !presented
            || !acknowledgementRejected
            || !acknowledged
            || !laterStaleHidden
            || !HasClosedSurface())
        {
            Fail("Misquotation-evidence presentation failed exact scene ownership.");
            return;
        }

        GD.Print(
            "Misquotation-evidence presentation controller smoke passed: inert startup, exact "
            + "selection projection, explicit abstention, stale and value-equal rejection, "
            + "exact acknowledgement, later fresh presentation, stale hiding, repeated rejection, "
            + "unchanged source state, exact nested identity retention, null guards, and closed "
            + "surface.");
        GetTree().Quit();
    }

    private static bool HasClosedSurface()
    {
        Type type = typeof(CharacterDialogueMisquotationEvidencePresentationController);
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
            && property.Name == "Presentation"
            && property.PropertyType
                == typeof(CharacterDialogueMisquotationEvidencePresentationSnapshot)
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
