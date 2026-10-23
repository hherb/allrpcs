using System.Runtime.CompilerServices;
using System.Text.Json;
using Allrpcs.Application.Dialogue;
using Allrpcs.Application.Navigation;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.Identifiers;
using Godot;
using static Allrpcs.Game.MainFlowCheckSteps;

namespace Allrpcs.Game;

/// <summary>
/// Proves slice steps 5 and 7 in Main: the station's unidentified leaves make a wrong remedy that
/// changes nothing; a conditional request makes the Healer examine the herb patch, which
/// identifies moonwort, and gather it on the normal path; a distant Healer's herb never turns a
/// press into the look-alike; the nearby Healer gives it to the Seeker; the inventory survives
/// save and Continue; the Seeker uses it in the remedy that stabilizes; and the cure survives a
/// second save and Continue with the station closed.
/// </summary>
public partial class IngredientRemedyFlowCheck : Node
{
    private const string EnvironmentVariable = "ALLRPCS_INGREDIENT_REMEDY_CHECK";
    private const string Request = "Healer, examine the herb patch and gather the herbs if they are safe.";
    private const string Reply = "I will look closely, and gather them only if they are safe.";
    // Known-good open ground beside the herb patch (also used by the herb-collection audio check).
    private static readonly Vector2 HealerBesidePatch = new(952, 350);
    private static readonly Vector2 SeekerBesidePatch = new(920, 350);

    /// <inheritdoc />
    public override async void _Ready()
    {
        if (OS.GetEnvironment(EnvironmentVariable) != "1") { return; }
        string path = SaveDocumentStorageController.CurrentFilePath();
        try
        {
            Engine.MaxFps = 60;
            File.Delete(path);
            Node first = await Launch();
            Customize(first);
            CharacterId seekerId = Seeker(first);
            CharacterId healerId = Healer(first);
            InventoryController inventory = first.GetNode<InventoryController>("InventoryController");
            Require(inventory.InventoryOf(seekerId).ObjectReferences.Select(id => id.Value).SequenceEqual(["field-lens"])
                && inventory.InventoryOf(healerId).ObjectReferences.Count == 0,
                "The Seeker starts with the field lens and the Healer with nothing.");

            // Wrong remedy: with nothing gathered, the station uses its unidentified dried leaves.
            PrepareAtStation(first, 7);
            FirstPatientStateController state = first.GetNode<FirstPatientStateController>("FirstPatientStateController");
            FirstPatientTreatmentContext context = first.GetNode<TemporaryFirstPatientTreatmentContextSource>(
                "TemporaryFirstPatientTreatmentContextSource").Context;
            Require(ReferenceEquals(
                    state.PreparedBatches!.Batches.SingleOrDefault()?.Preparation.Activity.Recipe,
                    context.Scenario.AlternativeRecipes[0]),
                "The station prepared the look-alike batch.");
            Administer(first);
            Require(state.DiseaseTruth!.Infections.Single().CareState == DiseaseCareState.Unassisted
                && state.AdministrationExperiments!.Records.Count == 1
                && state.Experiments!.Records.Single().ObservedOutcome.Contains("does not change", StringComparison.Ordinal)
                && Status(first) == OpeningRegionFirstPatientAdministrationInputCoordinator.NoChangeStatusText,
                "The wrong remedy was given, recorded, and changed nothing.");

            // Conditional collection on the normal path.
            Healer healer = first.GetNode<Healer>("Healer");
            await Until(() => healer.Visible, 60_000, "The Healer returned from the authored errand");
            healer.CommandWait();
            healer.GlobalPosition = HealerBesidePatch;
            first.GetNode<Node2D>("Seeker").GlobalPosition = SeekerBesidePatch;
            await Frames(2);
            ScriptedModel model = new();
            Require(first.GetNode<TemporaryCharacterModelSource>("TemporaryCharacterModelSource").TrySupplyModel(model),
                "The scripted model was supplied.");
            first.GetNode<Button>("SettingsOverlay/DialogueEntry").EmitSignal(BaseButton.SignalName.Pressed);
            LineEdit input = first.GetNode<LineEdit>("DialogueOverlay/Panel/Input");
            input.Text = Request;
            input.EmitSignal(LineEdit.SignalName.TextSubmitted, Request);
            HerbExaminationActionController action =
                first.GetNode<HerbExaminationActionController>("HerbExaminationActionController");
            CharacterTurnActivityController turns =
                first.GetNode<CharacterTurnActivityController>("CharacterTurnActivityController");
            await Until(() => action.LastResult is not null || (!action.Running && action.Status.Contains("cannot", StringComparison.Ordinal)),
                30_000, () => $"The Healer examined the herb patch (model calls {model.CallCount}, turn "
                    + $"{turns.Activity?.State.Value ?? "none"}, action '{action.Status}', healer {healer.GlobalPosition})");
            Require(model.CallCount == 1 && action.LastResult?.Safety == CharacterExaminationSafety.SafeToCollect,
                "The conditional request produced one safe examination: " + action.Status);
            Require(action.LastCollectionOccurrence is not null
                && action.Status.Contains("moonwort", StringComparison.Ordinal)
                && action.Status.Contains("Gathered", StringComparison.Ordinal)
                && inventory.Carries(healerId, InventoryController.MoonwortLeaves),
                "The examination identified moonwort and the Healer gathered it: " + action.Status);
            first.GetNode<CanvasLayer>("DialogueOverlay").Visible = false;

            // A distant Healer's moonwort: the press waits for the Healer instead of the look-alike.
            OpeningRegionRemedyPreparationSceneBinding station = first.GetNode<OpeningRegionRemedyPreparationSceneSource>(
                "OpeningRegionRemedyPreparationSceneSource").Binding!;
            first.GetNode<Node2D>("Seeker").GlobalPosition = new(station.StationPosition.X, station.StationPosition.Y);
            healer.GlobalPosition = new(station.StationPosition.X + 300, station.StationPosition.Y);
            Press(first, "OpeningRegionRemedyPreparationInputCoordinator", "remedy_prepare");
            Require(first.GetNode<FirstPatientPreparationController>("FirstPatientPreparationController").Activity is null
                && inventory.Carries(healerId, InventoryController.MoonwortLeaves)
                && Status(first) == OpeningRegionRemedyPreparationInputCoordinator.HealerCarryingStatusText,
                "A distant Healer's moonwort kept the station waiting: " + Status(first));

            // Give: at the station, the nearby Healer hands the moonwort to the Seeker.
            healer.GlobalPosition = new(station.StationPosition.X + 60, station.StationPosition.Y);
            Press(first, "OpeningRegionRemedyPreparationInputCoordinator", "remedy_prepare");
            Require(inventory.Carries(seekerId, InventoryController.MoonwortLeaves)
                && !inventory.Carries(healerId, InventoryController.MoonwortLeaves)
                && Status(first) == OpeningRegionRemedyPreparationInputCoordinator.CarryingStatusText,
                "The Healer gave the moonwort to the Seeker.");

            InitializeMap(first);
            first.GetNode<Button>("CommandOverlay/SaveLoadPanel/Save").EmitSignal(BaseButton.SignalName.Pressed);
            Require(first.GetNode<Label>("CommandOverlay/SaveLoadPanel/Status").Text == SaveLoadInputController.SavedStatusText,
                "The first session saved.");
            using (JsonDocument saved = JsonDocument.Parse(File.ReadAllText(path)))
            {
                Require(saved.RootElement.GetProperty("schemaVersion").GetInt32() == 11
                    && saved.RootElement.GetProperty("inventory").GetRawText().Contains(
                        "{\"objectId\":\"moonwort-leaves\",\"ownerCharacterId\":\"" + seekerId.Value + "\"}",
                        StringComparison.Ordinal),
                    "The save records that the Seeker carries the moonwort.");
            }

            first.QueueFree();
            await Frames(4);

            // Continue, then use the moonwort in the remedy.
            Node second = await Launch();
            NewGameStartPanelController start = second.GetNode<NewGameStartPanelController>("SeekerCustomizationOverlay/StartPanel");
            start.Begin();
            start.GetNode<Button>("Continue").EmitSignal(BaseButton.SignalName.Pressed);
            await Frames(2);
            inventory = second.GetNode<InventoryController>("InventoryController");
            state = second.GetNode<FirstPatientStateController>("FirstPatientStateController");
            Require(!start.Visible
                && !second.GetNode<Control>("SeekerCustomizationOverlay/Panel").Visible
                && inventory.Carries(seekerId, InventoryController.MoonwortLeaves)
                && inventory.InventoryOf(seekerId).ObjectReferences.Any(id => id.Value == "field-lens")
                && state.AdministrationExperiments!.Records.Count == 1,
                "Continue restored the inventory and the wrong dose.");
            PrepareAtStation(second, 7);
            context = second.GetNode<TemporaryFirstPatientTreatmentContextSource>(
                "TemporaryFirstPatientTreatmentContextSource").Context;
            Require(ReferenceEquals(
                    state.PreparedBatches!.Batches.SingleOrDefault()?.Preparation.Activity.Recipe,
                    context.Scenario.Remedy.Recipe)
                && !inventory.Carries(seekerId, InventoryController.MoonwortLeaves),
                "The remedy batch used up the moonwort: " + Status(second) + " batches="
                    + state.PreparedBatches.Batches.Count + " recipe="
                    + second.GetNode<FirstPatientPreparationController>("FirstPatientPreparationController")
                        .Activity?.Recipe.RecipeId.Value);
            Administer(second);
            Require(state.DiseaseTruth!.Infections.Single().CareState == DiseaseCareState.Stabilized
                && state.AdministrationExperiments!.Records.Count == 2
                && Status(second) == OpeningRegionFirstPatientAdministrationInputCoordinator.AdministeredStatusText,
                "The second dose, the remedy, stabilized the patient.");
            PrepareAtStation(second, 1);
            Require(Status(second) == OpeningRegionRemedyPreparationInputCoordinator.RemedyGivenStatusText,
                "After the cure the station says the remedy was given: " + Status(second));
            second.GetNode<Button>("CommandOverlay/SaveLoadPanel/Save").EmitSignal(BaseButton.SignalName.Pressed);
            Require(second.GetNode<Label>("CommandOverlay/SaveLoadPanel/Status").Text == SaveLoadInputController.SavedStatusText,
                "The cured session saved.");
            second.QueueFree();
            await Frames(4);

            // Continue the cured save: the stabilization, both doses and the used-up herb restore.
            Node third = await Launch();
            start = third.GetNode<NewGameStartPanelController>("SeekerCustomizationOverlay/StartPanel");
            start.Begin();
            start.GetNode<Button>("Continue").EmitSignal(BaseButton.SignalName.Pressed);
            await Frames(2);
            inventory = third.GetNode<InventoryController>("InventoryController");
            state = third.GetNode<FirstPatientStateController>("FirstPatientStateController");
            Require(!start.Visible
                && state.DiseaseTruth!.Infections.Single().CareState == DiseaseCareState.Stabilized
                && state.AdministrationExperiments!.Records.Count == 2
                && !inventory.Carries(seekerId, InventoryController.MoonwortLeaves)
                && !inventory.Carries(healerId, InventoryController.MoonwortLeaves),
                "Continue restored the cure, both doses and the used-up moonwort.");
            PrepareAtStation(third, 1);
            Require(Status(third) == OpeningRegionRemedyPreparationInputCoordinator.RemedyGivenStatusText,
                "The restored station stays closed after the cure: " + Status(third));
            third.QueueFree();
            await Frames(2);
            File.Delete(path);
            GD.Print("PASS: ingredient remedy flow");
            GetTree().Quit();
        }
        catch (Exception error)
        {
            GD.PushError(error.ToString());
            File.Delete(path);
            GetTree().Quit(1);
        }
    }

    private async Task<Node> Launch()
    {
        Node main = GD.Load<PackedScene>("res://Main.tscn").Instantiate();
        AddChild(main);
        await Frames(4);
        return main;
    }

    private static CharacterId Seeker(Node main) =>
        main.GetNode<TemporarySeekerCharacterSource>("TemporarySeekerCharacterSource").Character.Identity.Id;

    private static CharacterId Healer(Node main) =>
        main.GetNode<TemporaryHealerCharacterSource>("TemporaryHealerCharacterSource").Character.Identity.Id;

    private static string Status(Node main) => main.GetNode<Label>("CommandOverlay/Panel/RecoveryStatus").Text;

    private Task Until(Func<bool> condition, int milliseconds, string message) =>
        Until(condition, milliseconds, () => message);

    private async Task Until(Func<bool> condition, int milliseconds, Func<string> message)
    {
        ulong deadline = Time.GetTicksMsec() + (ulong)milliseconds;
        while (!condition())
        {
            Require(Time.GetTicksMsec() < deadline, message());
            await Frames(1);
        }
    }

    private async Task Frames(int count)
    {
        for (int index = 0; index < count; index++) { await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame); }
    }

    // Answers the conditional request with a conditionally accepted examination of the herb patch.
    private sealed class ScriptedModel : ICharacterModel
    {
        public int CallCount { get; private set; }

        public async IAsyncEnumerable<CharacterModelStreamUpdate> StreamResponseAsync(
            CharacterModelGenerationRequest request,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            CallCount++;
            CharacterTurnResponse response = new(
                request.TurnRequest,
                CharacterTurnDisposition.ConditionallyAccept,
                new DialogueDelivery(
                    new DialogueUtterance(request.TurnRequest.RespondingCharacterId, Reply),
                    DialogueDeliveryMode.OrdinarySpeech),
                new CharacterIntention(CharacterIntentionType.ExamineObject, new ObjectId("herb-patch")),
                [],
                []);
            await Task.CompletedTask;
            yield return new CharacterModelTextDelta(Reply);
            yield return new CharacterModelStreamTermination(
                CharacterModelStreamTerminationStatus.Completed,
                response,
                null);
        }
    }
}
