using System.Text.Json.Nodes;
using Allrpcs.Application.Diseases;
using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.Identifiers;
using Godot;
using static Allrpcs.Game.MainFlowCheckSteps;

namespace Allrpcs.Game;

/// <summary>
/// Proves slice step 8 in Main with the patient's stakes on. Time-advancing events (a wrong dose
/// among them) worsen the patient, and the worsened stage survives save and Continue. Two wrong
/// doses lose the patient
/// and open the loss panel, which blocks world input. A save at final decline with a whole
/// interval passed loads as lost. The remedy, the testimony and the sage complete the slice; the
/// completion panel lets the player keep exploring and returns after save and Continue.
/// </summary>
public partial class SliceStakesEndingFlowCheck : Node
{
    private const string EnvironmentVariable = "ALLRPCS_SLICE_STAKES_CHECK";

    /// <inheritdoc />
    public override async void _Ready()
    {
        if (OS.GetEnvironment(EnvironmentVariable) != "1") { return; }
        string path = SaveDocumentStorageController.CurrentFilePath();
        try
        {
            Engine.MaxFps = 60;
            File.Delete(path);

            // Three time-advancing events (identification, the Healer's errand, a wrong dose): the
            // patient weakens from deep sleep to wasting.
            Node first = await Launch(fresh: true);
            WorldEventController world = first.GetNode<WorldEventController>("WorldEventController");
            foreach (string id in new[] { "herb-patch-identified", "healer-examined-herb-patch" })
            {
                Require(world.TryOccur(new WorldEventId(id)), "Occurred " + id);
                await Frames(2);
                Require(Stage(first) == DiseaseStage.DeepSleep, "Two events leave the patient in deep sleep.");
            }

            PrepareAtStation(first, 7);
            Administer(first);
            await Frames(2);
            Require(Stage(first) == DiseaseStage.Wasting
                && Status(first) == "Patient: Appears severely weakened."
                && Outcome(first) == FirstPatientSliceOutcome.InProgress,
                "The wrong dose, a third event, weakened the patient: " + Status(first));
            Save(first);
            string wastingSave = File.ReadAllText(path);
            first.QueueFree();
            await Frames(4);

            // Continue keeps the worsened stage; the second wrong dose then loses the patient.
            Node second = await Launch(fresh: false);
            Require(Stage(second) == DiseaseStage.Wasting && Outcome(second) == FirstPatientSliceOutcome.InProgress,
                "Continue kept the wasting patient.");
            PrepareAtStation(second, 7);
            Administer(second);
            await Frames(2);

            SliceEndPanelController panel = Panel(second);
            Require(Outcome(second) == FirstPatientSliceOutcome.LostAfterDoses
                && panel.Shown == FirstPatientSliceOutcome.LostAfterDoses
                && panel.Visible
                && !panel.GetNode<Button>("Box/KeepExploring").Visible
                && panel.GetNode<Label>("Box/Title").Text == FirstPatientStakesCopy.Ending(FirstPatientSliceOutcome.LostAfterDoses).Title
                && !ControlInputContext.WorldAllowed(second)
                && panel.MouseFilter == Control.MouseFilterEnum.Stop
                && panel.GetGlobalRect().Encloses(second.GetNode<Control>("CommandOverlay/SaveLoadPanel/Save").GetGlobalRect()),
                "Two wrong doses lost the patient and the loss panel holds the world and covers Save.");
            second.QueueFree();
            await Frames(4);

            // A save at final decline with a whole interval since its last progression loads as lost.
            File.WriteAllText(path, AtFinalDeclineWithIntervalPassed(wastingSave));
            Node third = await Launch(fresh: false);
            Require(Stage(third) == DiseaseStage.FinalDecline
                && Outcome(third) == FirstPatientSliceOutcome.LostToDecline
                && Panel(third).Shown == FirstPatientSliceOutcome.LostToDecline,
                "The final-decline save loaded as lost.");
            third.QueueFree();
            await Frames(4);

            // Completion: the remedy stabilizes, the patient testifies and the sage is consulted.
            File.Delete(path);
            Node fourth = await Launch(fresh: true);
            CharacterId seekerId = fourth.GetNode<TemporarySeekerCharacterSource>(
                "TemporarySeekerCharacterSource").Character.Identity.Id;
            // Gathering itself is proven by the ingredient check; here its event and herb are installed.
            Require(fourth.GetNode<WorldEventController>("WorldEventController").TryOccur(new WorldEventId("healer-collected-herb-patch"))
                && fourth.GetNode<InventoryController>("InventoryController").TryTake(seekerId, InventoryController.MoonwortLeaves),
                "The Seeker holds gathered moonwort.");
            PrepareAtStation(fourth, 7);
            Administer(fourth);
            await Frames(2);
            Require(fourth.GetNode<FirstPatientStateController>("FirstPatientStateController")
                    .DiseaseTruth!.Infections.Single().CareState == DiseaseCareState.Stabilized,
                "The remedy stabilized the patient: " + Status(fourth));
            OpeningRegionFirstPatientStabilizationTestimonyCoordinator testimony =
                fourth.GetNode<OpeningRegionFirstPatientStabilizationTestimonyCoordinator>(
                    "OpeningRegionFirstPatientStabilizationTestimonyCoordinator");
            await Until(() => testimony.Testimony is not null, 5_000, "The patient testified.");
            Require(Outcome(fourth) == FirstPatientSliceOutcome.InProgress && Panel(fourth).Shown is null,
                "The slice is not complete before the sage is consulted.");
            fourth.GetNode<Node2D>("Seeker").GlobalPosition = fourth.GetNode<Marker2D>("SageHearingPosition").Position;
            Press(fourth, "OpeningRegionSageConsultationInputCoordinator", "sage_consult");
            await Frames(2);
            panel = Panel(fourth);
            Require(Outcome(fourth) == FirstPatientSliceOutcome.Completed
                && panel.Shown == FirstPatientSliceOutcome.Completed
                && panel.GetNode<Button>("Box/KeepExploring").Visible,
                "Consulting the sage completed the slice: " + Status(fourth));
            panel.GetNode<Button>("Box/KeepExploring").EmitSignal(BaseButton.SignalName.Pressed);
            await Frames(2);
            Require(!panel.Visible && ControlInputContext.WorldAllowed(fourth),
                "Keep exploring closed the panel and released the world.");
            Save(fourth);
            fourth.QueueFree();
            await Frames(4);

            Node fifth = await Launch(fresh: false);
            Require(Outcome(fifth) == FirstPatientSliceOutcome.Completed
                && Panel(fifth).Shown == FirstPatientSliceOutcome.Completed,
                "Continue re-presented the completed slice.");
            fifth.QueueFree();
            await Frames(2);
            File.Delete(path);
            GD.Print("PASS: slice stakes and ending flow");
            GetTree().Quit();
        }
        catch (Exception error)
        {
            GD.PushError(error.ToString());
            File.Delete(path);
            GetTree().Quit(1);
        }
    }

    // Launches Main with the stakes on, then either customizes a new Seeker or Continues the save.
    private async Task<Node> Launch(bool fresh)
    {
        Node main = GD.Load<PackedScene>("res://Main.tscn").Instantiate();
        AddChild(main);
        await Frames(4);
        if (fresh)
        {
            Customize(main);
            // Saving requires the Seeker's map, which is created the first time it opens.
            InitializeMap(main);
        }
        else
        {
            NewGameStartPanelController start = main.GetNode<NewGameStartPanelController>("SeekerCustomizationOverlay/StartPanel");
            start.Begin();
            start.GetNode<Button>("Continue").EmitSignal(BaseButton.SignalName.Pressed);
            Require(!start.Visible, "Continue loaded the save.");
        }

        main.GetNode<FirstPatientStakesController>("FirstPatientStakesController").Active = true;
        await Frames(2);
        return main;
    }

    // Same Wasting save, but at final decline with three examinations logged after its progression.
    private static string AtFinalDeclineWithIntervalPassed(string json)
    {
        JsonNode root = JsonNode.Parse(json)!;
        string text = root.ToJsonString().Replace("\"stage\":\"wasting\"", "\"stage\":\"final-decline\"", StringComparison.Ordinal);
        root = JsonNode.Parse(text)!;
        JsonObject events = root["worldEvents"]!.AsObject();
        JsonArray log = events["eventLog"]!.AsArray();
        for (int index = 1; index <= FirstPatientDiseaseProgressionSchedule.StageInterval; index++)
        {
            log.Add($"herb-patch-examination-{index}");
        }

        events["currentTimeEventId"] = $"herb-patch-examination-{FirstPatientDiseaseProgressionSchedule.StageInterval}";
        Require(text.Contains("final-decline", StringComparison.Ordinal), "The save was moved to final decline.");
        return root.ToJsonString();
    }

    private static void Save(Node main)
    {
        main.GetNode<Button>("CommandOverlay/SaveLoadPanel/Save").EmitSignal(BaseButton.SignalName.Pressed);
        string status = main.GetNode<Label>("CommandOverlay/SaveLoadPanel/Status").Text;
        Require(status == SaveLoadInputController.SavedStatusText, "The session saved: " + status);
    }

    private static DiseaseStage Stage(Node main) =>
        main.GetNode<FirstPatientStateController>("FirstPatientStateController").DiseaseTruth!.Infections.Single().Stage;

    private static FirstPatientSliceOutcome Outcome(Node main) =>
        main.GetNode<FirstPatientStakesController>("FirstPatientStakesController").Outcome;

    private static SliceEndPanelController Panel(Node main) =>
        main.GetNode<SliceEndPanelController>("SliceEndOverlay/Panel");

    private static string Status(Node main) => main.GetNode<Label>("CommandOverlay/Panel/RecoveryStatus").Text;

    private async Task Until(Func<bool> condition, int milliseconds, string message)
    {
        ulong deadline = Time.GetTicksMsec() + (ulong)milliseconds;
        while (!condition())
        {
            Require(Time.GetTicksMsec() < deadline, message);
            await Frames(1);
        }
    }

    private async Task Frames(int count)
    {
        for (int index = 0; index < count; index++) { await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame); }
    }
}
