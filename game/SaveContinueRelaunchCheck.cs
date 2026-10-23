using System.Text.Json;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Persistence;
using Allrpcs.Infrastructure.Persistence;
using Godot;

namespace Allrpcs.Game;

/// <summary>
/// Proves save → relaunch → continue: a fresh Main offers Continue, and continuing restores the
/// transcript and character memory, the Seeker profile with its route access, and both body
/// positions, without reopening customization. A save taken before customization continues into
/// customization, and an unreadable save keeps the panel open until New game.
/// </summary>
public partial class SaveContinueRelaunchCheck : Node
{
    private const string EnvironmentVariable = "ALLRPCS_SAVE_CONTINUE_CHECK";
    private const string ReplyText = "I will remember that you asked about the blue fern.";
    private static readonly Vector2 SeekerMoved = new(2260, 1010);
    private static readonly Vector2 HealerMoved = new(2210, 990);

    /// <inheritdoc />
    public override async void _Ready()
    {
        if (OS.GetEnvironment(EnvironmentVariable) != "1") { return; }
        string path = SaveDocumentStorageController.CurrentFilePath();
        try
        {
            File.Delete(path);
            Node first = await Launch();
            Customize(first);
            CompleteTurn(first);
            first.GetNode<Node2D>("Seeker").GlobalPosition = SeekerMoved;
            first.GetNode<Node2D>("Healer").GlobalPosition = HealerMoved;
            InitializeMap(first);
            first.GetNode<Button>("CommandOverlay/SaveLoadPanel/Save").EmitSignal(BaseButton.SignalName.Pressed);
            Require(first.GetNode<Label>("CommandOverlay/SaveLoadPanel/Status").Text == SaveLoadInputController.SavedStatusText,
                "The first session saved.");
            string saved = File.ReadAllText(path);
            first.QueueFree();
            await Frames(4);

            Node second = await Launch();
            (NewGameStartPanelController start, SeekerCustomizationMenuController menu, Seeker seeker) = StartPanel(second);
            start.Begin();
            Require(start.Visible && !menu.Visible && !seeker.IsPhysicsProcessing(), "A relaunch with a save offers Continue first.");
            second.GetNode<Button>("CommandOverlay/SaveLoadPanel/Save").EmitSignal(BaseButton.SignalName.Pressed);
            Require(File.ReadAllText(path) == saved, "The Save button cannot overwrite the save while Continue is offered.");
            start.GetNode<Button>("Continue").EmitSignal(BaseButton.SignalName.Pressed);
            await Frames(2);

            Require(!start.Visible && !menu.Visible && seeker.IsPhysicsProcessing(), "Continue resumed play without reopening customization.");
            SeekerCustomizationProfile? profile = second.GetNode<SeekerCustomizationSelectionSceneSource>(
                "SeekerCustomizationSelectionSceneSource").CurrentState.SelectedProfile;
            Require(profile?.Identity.DisplayName == "Mara" && profile.Background.Key == "forester" && !menu.TryOpen(),
                "Continue restored Mara's Forester profile.");
            OpeningRegionClosedForestPathSceneSource closedPath =
                second.GetNode<OpeningRegionClosedForestPathSceneSource>("OpeningRegionClosedForestPathSceneSource");
            Require(closedPath.CurrentAccess is not null
                && closedPath.GetNode<StaticBody2D>("ClosedGate").GetChild<CollisionShape2D>(0).Disabled,
                "Continue restored the Forester's open forest path.");
            Require(seeker.GlobalPosition == SeekerMoved && second.GetNode<Node2D>("Healer").GlobalPosition == HealerMoved,
                "Continue restored both body positions.");
            CharacterMemoryAggregate healer = Assert(second.GetNode<CharacterMemoryController>("CharacterMemoryController")
                .Memory.Characters.SingleOrDefault(), "Continue restored the healer's memory.");
            Require(healer.RecentConversation.Turns.SingleOrDefault()?.Activity.Response?.Speech?.Text == ReplyText
                && healer.EpisodicMemories.Memories.SingleOrDefault()?.Summary.Contains(ReplyText, StringComparison.Ordinal) == false,
                "Continue restored the transcript turn and its speech-free episode.");
            DialogueUtterance next = new(new CharacterId(SeekerCustomizationProfile.SeekerCharacterIdValue), "Healer?");
            DialogueEvent current = new(new DialogueDelivery(next, DialogueDeliveryMode.OrdinarySpeech),
                new DialogueAudienceSnapshot(next.SpeakerCharacterId, [healer.CharacterId]));
            Require(second.GetNode<DialogueEventController>("DialogueEventController").History.RecentFor(healer.CharacterId, current)
                .Select(entry => entry.Utterance.Text).SequenceEqual(["Healer, remember the blue fern.", ReplyText]),
                "Continue offered the saved conversation to the healer's next turn as recent speech.");
            string resaved = second.GetNode<SaveSnapshotJsonController>("SaveSnapshotJsonController").CaptureSerialized()
                ?? throw new InvalidOperationException("The continued session could not be captured.");
            foreach (string section in new[] { "characterMemory", "continuation" })
            {
                Require(Section(saved, section) == Section(resaved, section), $"The continued {section} matches the save exactly.");
            }

            second.QueueFree();
            await Frames(2);

            Node early = await Launch();
            InitializeMap(early);
            early.GetNode<Button>("CommandOverlay/SaveLoadPanel/Save").EmitSignal(BaseButton.SignalName.Pressed);
            Require(early.GetNode<Label>("CommandOverlay/SaveLoadPanel/Status").Text == SaveLoadInputController.SavedStatusText,
                "A session saved before customization.");
            early.QueueFree();
            await Frames(4);
            Node uncustomized = await Launch();
            (start, menu, seeker) = StartPanel(uncustomized);
            start.Begin();
            start.GetNode<Button>("Continue").EmitSignal(BaseButton.SignalName.Pressed);
            await Frames(2);
            Require(!start.Visible && menu.Visible && !seeker.IsPhysicsProcessing(),
                "Continuing a save taken before customization opens customization.");
            uncustomized.QueueFree();
            await Frames(2);

            File.WriteAllText(path, "{");
            Node corrupt = await Launch();
            (start, menu, seeker) = StartPanel(corrupt);
            start.Begin();
            start.GetNode<Button>("Continue").EmitSignal(BaseButton.SignalName.Pressed);
            await Frames(2);
            Require(start.Visible && !menu.Visible && !seeker.IsPhysicsProcessing()
                && start.GetNode<Label>("Status").Text == NewGameStartPanelController.FailedStatusText,
                "An unreadable save keeps the start panel open with the failure status.");
            // New game asks again, filled in; an appearance this build lacks leaves its prompt.
            File.WriteAllText(NewGameStartPanelController.RememberedProfilePath(), RememberedSeekerProfileJson.Serialize(
                new SeekerCustomizationSaveState(new CharacterIdentity(new CharacterId(SeekerCustomizationProfile.SeekerCharacterIdValue), "Mara"),
                    new SeekerPronouns("they", "them", "their", "theirs", "themself"), "retired-appearance", "forester")));
            start.GetNode<Button>("NewGame").EmitSignal(BaseButton.SignalName.Pressed);
            Require(!start.Visible && menu.Visible, "New game after a failed Continue opens customization.");
            Require(menu.GetNode<LineEdit>("DisplayName").Text == "Mara" && menu.GetNode<LineEdit>("Reflexive").Text == "themself"
                && menu.GetNode<OptionButton>("Appearance").Selected == 0 && menu.GetNode<OptionButton>("Background").Selected == 1,
                "New game prefilled the remembered Mara, Forester, and left the unknown appearance unchosen.");
            corrupt.GetNode<Button>("CommandOverlay/SaveLoadPanel/Save").EmitSignal(BaseButton.SignalName.Pressed);
            Require(File.ReadAllText(path) == "{", "The Save button cannot overwrite the save before a new Seeker is confirmed.");
            corrupt.QueueFree();
            await Frames(2);
            File.Delete(path);
            File.Delete(NewGameStartPanelController.RememberedProfilePath());
            GD.Print("PASS: save continue relaunch");
            GetTree().Quit();
        }
        catch (Exception error)
        {
            GD.PushError(error.ToString());
            File.Delete(path);
            File.Delete(NewGameStartPanelController.RememberedProfilePath());
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

    private static (NewGameStartPanelController Start, SeekerCustomizationMenuController Menu, Seeker Seeker) StartPanel(
        Node main) => (
        main.GetNode<NewGameStartPanelController>("SeekerCustomizationOverlay/StartPanel"),
        main.GetNode<SeekerCustomizationMenuController>("SeekerCustomizationOverlay/Panel"),
        main.GetNode<Seeker>("Seeker"));

    private static void Customize(Node main)
    {
        SeekerCustomizationMenuController menu = main.GetNode<SeekerCustomizationMenuController>("SeekerCustomizationOverlay/Panel");
        Require(menu.TryOpen(), "The first session opened customization.");
        menu.GetNode<LineEdit>("DisplayName").Text = "Mara";
        menu.GetNode<LineEdit>("Subject").Text = "they";
        menu.GetNode<LineEdit>("Objective").Text = "them";
        menu.GetNode<LineEdit>("PossessiveDeterminer").Text = "their";
        menu.GetNode<LineEdit>("PossessivePronoun").Text = "theirs";
        menu.GetNode<LineEdit>("Reflexive").Text = "themself";
        menu.GetNode<OptionButton>("Appearance").Select(1);
        // Item 0 is the disabled prompt; item 1 is the first background, Forester.
        menu.GetNode<OptionButton>("Background").Select(1);
        menu.GetNode<Button>("Confirm").EmitSignal(BaseButton.SignalName.Pressed);
        Require(!menu.Visible, "The first session selected Forester.");
    }

    private static void CompleteTurn(Node main)
    {
        CharacterTurnActivityController turns = main.GetNode<CharacterTurnActivityController>("CharacterTurnActivityController");
        CharacterId healerId = main.GetNode<TemporaryOrdinaryCharacterParticipantSource>(
            "TemporaryOrdinaryCharacterParticipantSource").Participants[0].Identity.Id;
        CharacterId seekerId = new(SeekerCustomizationProfile.SeekerCharacterIdValue);
        DialogueUtterance heard = new(seekerId, "Healer, remember the blue fern.");
        CharacterTurnRequest request = new(
            healerId,
            new DialogueEvent(new DialogueDelivery(heard, DialogueDeliveryMode.OrdinarySpeech), new DialogueAudienceSnapshot(seekerId, [healerId])),
            new DialogueAct(heard, DialogueActType.Request));
        CharacterTurnResponse response = new(
            request,
            CharacterTurnDisposition.Accept,
            new DialogueDelivery(new DialogueUtterance(healerId, ReplyText), DialogueDeliveryMode.OrdinarySpeech),
            new CharacterIntention(CharacterIntentionType.Wait, null),
            [],
            []);
        Require(turns.TryStart(request) && turns.TryReceiveResponse(response) && turns.TryComplete(),
            "The first session completed one dialogue turn.");
    }

    private static void InitializeMap(Node main)
    {
        SeekerMapOpenCloseInputController map = main.GetNode<SeekerMapOpenCloseInputController>("SeekerMapOpenCloseInputController");
        map._UnhandledInput(new InputEventAction { Action = "map_toggle", Pressed = true });
        map._UnhandledInput(new InputEventAction { Action = "map_toggle", Pressed = true });
        Require(main.GetNode<SeekerMapController>("SeekerMapController").Map is not null, "The first session initialized the map.");
    }

    private static string Section(string document, string name)
    {
        using JsonDocument parsed = JsonDocument.Parse(document);
        return parsed.RootElement.GetProperty(name).GetRawText();
    }

    private static T Assert<T>(T? value, string message) where T : class =>
        value ?? throw new InvalidOperationException("Failed: " + message);

    private static void Require(bool condition, string message)
    {
        if (!condition) { throw new InvalidOperationException("Failed: " + message); }
    }

    private async Task Frames(int count)
    {
        for (int index = 0; index < count; index++) { await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame); }
    }
}
