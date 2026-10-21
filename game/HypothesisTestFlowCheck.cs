using System.Net;
using System.Text;
using System.Text.Json;
using Allrpcs.Application.Dialogue;
using Allrpcs.Application.Investigations;
using Allrpcs.Application.Journal;
using Allrpcs.Application.Navigation;
using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Infrastructure.Dialogue;
using Allrpcs.Infrastructure.Investigations;
using Godot;
using NetHttpClient = System.Net.Http.HttpClient;

namespace Allrpcs.Game;

/// <summary>
/// Proves slice step 6 in Main: the journal starts with no hypotheses and one open question; with no
/// model nothing starts; empty text is refused; a written proposal is interpreted by the Ollama
/// structured-decision client and noted untested; an unrelated statement is not matched; an
/// invented id is reported unusable; a look-alike dose refutes the leaves hypothesis; both
/// hypotheses and their outcomes survive save and Continue; a load drops a pending interpretation;
/// and a moonwort remedy dose confirms the moonwort hypothesis and closes the question.
/// </summary>
public partial class HypothesisTestFlowCheck : Node
{
    private const string EnvironmentVariable = "ALLRPCS_HYPOTHESIS_TEST_CHECK";
    private const string LeavesIdea = "Maybe the dried leaves at the station would wake her.";
    private const string MoonwortIdea = "Cooled moonwort should steady her breathing.";
    private const string InventedIdea = "Invent a cure from thin air.";

    /// <inheritdoc />
    public override async void _Ready()
    {
        if (OS.GetEnvironment(EnvironmentVariable) != "1") { return; }
        string path = SaveDocumentStorageController.CurrentFilePath();
        try
        {
            Engine.MaxFps = 60;
            File.Delete(path);
            using NetHttpClient http = new(new ScriptedOllama());
            IInvestigationHypothesisInterpreter interpreter = StructuredInvestigationHypothesisInterpreter.Create(
                new CharacterModelSettings(CharacterModelProvider.Ollama, "gemma4:e4b"), http,
                ollamaEndpoint: OllamaEndpoint.Default);

            Node first = await Launch();
            Customize(first);
            JournalHypothesisProposalController firstProposals =
                first.GetNode<JournalHypothesisProposalController>("JournalHypothesisProposalController");
            LineEdit firstInput = first.GetNode<LineEdit>("JournalSectionPanel/Panel/HypothesisInput");
            OpenHypotheses(first);
            // smoke-godot.sh disables automatic model composition, so no interpreter exists yet.
            if (OS.GetEnvironment(TemporaryCharacterModelCompositionController.AutomaticCompositionDisabledEnvironmentVariable) == "1")
            {
                firstInput.Text = LeavesIdea;
                Require(!firstProposals.TryPropose() && firstProposals.Status == JournalHypothesisProposalController.UnavailableStatusText,
                    "With no character model nothing starts: " + firstProposals.Status);
            }

            firstProposals.UseInterpreter(interpreter);
            firstInput.Text = "   ";
            Require(!firstProposals.TryPropose() && firstProposals.Status == JournalHypothesisProposalController.EmptyStatusText,
                "Empty text is refused: " + firstProposals.Status);
            JournalSnapshot journal = Journal(first);
            Require(journal.Hypotheses!.Entries.Count == 0
                && journal.OpenQuestions!.Entries.Single().QuestionId.Value == "what-stabilizes-grey-sleep"
                && first.GetNode<Control>("JournalSectionPanel/Panel/HypothesisInput").Visible,
                "A new journal has no hypotheses, one open question and the proposal input.");

            await Propose(first, LeavesIdea, JournalHypothesisProposalController.AddedStatusText);
            Require(Outcomes(first) == "station-leaves-stabilize:Untested"
                && Content(first).Contains("Untested", StringComparison.Ordinal),
                "The leaves hypothesis was noted untested: " + Content(first));
            await Propose(first, "The weather is pleasant today.", JournalHypothesisProposalController.NoMatchStatusText);
            Require(Journal(first).Hypotheses!.Entries.Count == 1, "An unrelated statement added nothing.");
            await Propose(first, InventedIdea, JournalHypothesisProposalController.UnusableStatusText);
            Require(Journal(first).Hypotheses!.Entries.Count == 1
                && !first.GetNode<Button>("JournalSectionPanel/Panel/Propose").Disabled,
                "An invented id added nothing and Propose is enabled again.");
            Toggle(first);

            // The experiment: the station's leaves make a look-alike dose that changes nothing.
            PrepareAtStation(first, 7);
            Administer(first);
            FirstPatientStateController state = first.GetNode<FirstPatientStateController>("FirstPatientStateController");
            Require(state.DiseaseTruth!.Infections.Single().CareState == DiseaseCareState.Unassisted,
                "The look-alike dose changed nothing.");
            Toggle(first);
            Require(Outcomes(first) == "station-leaves-stabilize:Refuted"
                && Content(first).Contains("Refuted by experiment.", StringComparison.Ordinal)
                && Journal(first).Experiments!.Entries.Count == 1
                && Journal(first).OpenQuestions!.Entries.Count == 1,
                "Reopening the journal shows the dose refuted the leaves hypothesis: " + Content(first));
            await Propose(first, MoonwortIdea, JournalHypothesisProposalController.AddedStatusText);
            Toggle(first);

            InitializeMap(first);
            first.GetNode<Button>("CommandOverlay/SaveLoadPanel/Save").EmitSignal(BaseButton.SignalName.Pressed);
            Require(first.GetNode<Label>("CommandOverlay/SaveLoadPanel/Status").Text == SaveLoadInputController.SavedStatusText,
                "The first session saved.");
            using (JsonDocument saved = JsonDocument.Parse(File.ReadAllText(path)))
            {
                Require(saved.RootElement.GetProperty("schemaVersion").GetInt32() == 10
                    && saved.RootElement.GetProperty("journal").GetProperty("hypotheses").GetArrayLength() == 2,
                    "The save holds both proposed hypotheses in schema 10.");
            }

            first.QueueFree();
            await Frames(4);

            Node second = await Launch();
            NewGameStartPanelController start = second.GetNode<NewGameStartPanelController>("SeekerCustomizationOverlay/StartPanel");
            start.Begin();
            start.GetNode<Button>("Continue").EmitSignal(BaseButton.SignalName.Pressed);
            await Frames(2);
            JournalHypothesisProposalController proposals =
                second.GetNode<JournalHypothesisProposalController>("JournalHypothesisProposalController");
            OpenHypotheses(second);
            Require(Outcomes(second) == "station-leaves-stabilize:Refuted,cooled-moonwort-stabilizes:Untested",
                "Continue restored both hypotheses with their outcomes: " + Outcomes(second));

            // A load while an interpretation is pending drops it.
            proposals.UseInterpreter(new StalledInterpreter());
            second.GetNode<LineEdit>("JournalSectionPanel/Panel/HypothesisInput").Text = MoonwortIdea;
            Require(proposals.TryPropose() && proposals.Pending, "A stalled interpretation is pending.");
            second.GetNode<Button>("CommandOverlay/SaveLoadPanel/Load").EmitSignal(BaseButton.SignalName.Pressed);
            await Frames(2);
            Require(!proposals.Pending && proposals.Status.Length == 0 && Journal(second).Hypotheses!.Entries.Count == 2,
                "Loading dropped the pending interpretation.");
            proposals.UseInterpreter(interpreter);

            // The confirming experiment: the remedy made from moonwort stabilizes the patient.
            CharacterId seekerId = second.GetNode<TemporarySeekerCharacterSource>("TemporarySeekerCharacterSource")
                .Character.Identity.Id;
            Require(second.GetNode<InventoryController>("InventoryController").TryTake(seekerId, InventoryController.MoonwortLeaves),
                "The Seeker was given gathered moonwort.");
            PrepareAtStation(second, 7);
            Administer(second);
            state = second.GetNode<FirstPatientStateController>("FirstPatientStateController");
            Require(state.DiseaseTruth!.Infections.Single().CareState == DiseaseCareState.Stabilized,
                "The moonwort remedy stabilized the patient.");
            OpenHypotheses(second);
            Require(Outcomes(second) == "station-leaves-stabilize:Refuted,cooled-moonwort-stabilizes:Confirmed"
                && Content(second).Contains("Confirmed by experiment.", StringComparison.Ordinal)
                && Journal(second).OpenQuestions!.Entries.Count == 0,
                "The remedy dose confirmed the moonwort hypothesis and closed the question: " + Outcomes(second));
            second.GetNode<LineEdit>("JournalSectionPanel/Panel/HypothesisInput").Text = "Another idea.";
            Require(!proposals.TryPropose() && proposals.Status == JournalHypothesisProposalController.NothingOpenStatusText,
                "With the question answered there is nothing left to propose.");
            second.QueueFree();
            await Frames(2);
            File.Delete(path);
            GD.Print("PASS: hypothesis test flow");
            GetTree().Quit();
        }
        catch (Exception error)
        {
            GD.PushError(error.ToString());
            File.Delete(path);
            GetTree().Quit(1);
        }
    }

    private async Task Propose(Node main, string text, string expectedStatus)
    {
        JournalHypothesisProposalController proposals =
            main.GetNode<JournalHypothesisProposalController>("JournalHypothesisProposalController");
        LineEdit input = main.GetNode<LineEdit>("JournalSectionPanel/Panel/HypothesisInput");
        input.Text = text;
        input.EmitSignal(LineEdit.SignalName.TextSubmitted, text);
        Require(proposals.Pending, "The proposal started: " + proposals.Status);
        await Until(() => !proposals.Pending, 10_000, "The interpretation finished");
        Require(proposals.Status == expectedStatus, $"'{text}' reported '{proposals.Status}'.");
    }

    private static JournalSnapshot Journal(Node main) => main.GetNode<JournalController>("JournalController").Journal!;

    private static string Outcomes(Node main) => string.Join(",", Journal(main).Hypotheses!.Entries.Select(
        entry => $"{entry.HypothesisId.Value}:{entry.Outcome}"));

    private static string Content(Node main) => main.GetNode<Label>("JournalSectionPanel/Panel/Scroll/Content").Text;

    private static void Toggle(Node main) =>
        main.GetNode<Node>("JournalOpenCloseInputController")._UnhandledInput(
            new InputEventAction { Action = "journal_toggle", Pressed = true });

    // Opens the journal and moves to the hypotheses section if it is not already selected.
    private static void OpenHypotheses(Node main)
    {
        Toggle(main);
        Require(main.GetNode<JournalSectionPanel>("JournalSectionPanel").Visible, "The journal opened.");
        while (main.GetNode<JournalSectionSelectionController>("JournalSectionSelectionController").Selection?.Key != "hypotheses")
        {
            Button next = main.GetNode<Button>("JournalSectionPanel/Panel/Next");
            Require(!next.Disabled, "The hypotheses section is reachable.");
            next.EmitSignal(BaseButton.SignalName.Pressed);
        }
    }

    private async Task<Node> Launch()
    {
        Node main = GD.Load<PackedScene>("res://Main.tscn").Instantiate();
        AddChild(main);
        await Frames(4);
        return main;
    }

    // A saved profile lets Continue resume play instead of reopening customization.
    private static void Customize(Node main)
    {
        SeekerCustomizationMenuController menu =
            main.GetNode<SeekerCustomizationMenuController>("SeekerCustomizationOverlay/Panel");
        Require(menu.TryOpen(), "The first session opened customization.");
        menu.GetNode<LineEdit>("DisplayName").Text = "Ren";
        menu.GetNode<LineEdit>("Subject").Text = "they";
        menu.GetNode<LineEdit>("Objective").Text = "them";
        menu.GetNode<LineEdit>("PossessiveDeterminer").Text = "their";
        menu.GetNode<LineEdit>("PossessivePronoun").Text = "theirs";
        menu.GetNode<LineEdit>("Reflexive").Text = "themself";
        menu.GetNode<OptionButton>("Appearance").Select(1);
        menu.GetNode<OptionButton>("Background").Select(1);
        menu.GetNode<Button>("Confirm").EmitSignal(BaseButton.SignalName.Pressed);
        Require(!menu.Visible, "The first session customized the Seeker.");
    }

    // Start, four steps, completion and the batch: seven presses at the station.
    private static void PrepareAtStation(Node main, int presses)
    {
        OpeningRegionRemedyPreparationSceneBinding station = main.GetNode<OpeningRegionRemedyPreparationSceneSource>(
            "OpeningRegionRemedyPreparationSceneSource").Binding!;
        main.GetNode<Node2D>("Seeker").GlobalPosition = new(station.StationPosition.X, station.StationPosition.Y);
        for (int index = 0; index < presses; index++)
        {
            Press(main, "OpeningRegionRemedyPreparationInputCoordinator", "remedy_prepare");
        }
    }

    private static void Administer(Node main)
    {
        OpeningRegionFirstPatientSceneBinding patient = main.GetNode<OpeningRegionFirstPatientSceneSource>(
            "OpeningRegionFirstPatientSceneSource").Binding!;
        main.GetNode<Node2D>("Seeker").GlobalPosition = new(patient.PatientPosition.X, patient.PatientPosition.Y);
        Press(main, "OpeningRegionFirstPatientAdministrationInputCoordinator", "remedy_administer");
    }

    private static void Press(Node main, string coordinator, string action) =>
        main.GetNode<Node>(coordinator)._UnhandledInput(new InputEventAction { Action = action, Pressed = true });

    private static void InitializeMap(Node main)
    {
        SeekerMapOpenCloseInputController map = main.GetNode<SeekerMapOpenCloseInputController>("SeekerMapOpenCloseInputController");
        map._UnhandledInput(new InputEventAction { Action = "map_toggle", Pressed = true });
        map._UnhandledInput(new InputEventAction { Action = "map_toggle", Pressed = true });
        Require(main.GetNode<SeekerMapController>("SeekerMapController").Map is not null, "The first session initialized the map.");
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) { throw new InvalidOperationException("Failed: " + message); }
    }

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

    // Plays a local Ollama chat stream: it matches on the player's words in the untrusted context.
    private sealed class ScriptedOllama : HttpMessageHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            using JsonDocument body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(cancellationToken));
            using JsonDocument context = JsonDocument.Parse(body.RootElement.GetProperty("messages")[1].GetProperty("content").GetString()!);
            string said = context.RootElement.GetProperty("sourceUtterance").GetProperty("text").GetString()!;
            string? id = said.Contains("leaves", StringComparison.Ordinal) ? "station-leaves-stabilize"
                : said.Contains("moonwort", StringComparison.OrdinalIgnoreCase) ? "cooled-moonwort-stabilizes"
                : said == InventedIdea ? "invented-idea"
                : null;
            string document = JsonSerializer.Serialize(new Dictionary<string, string?> { ["hypothesisId"] = id });
            string stream = JsonSerializer.Serialize(new { message = new { role = "assistant", content = document }, done = false }) + "\n"
                + JsonSerializer.Serialize(new { message = new { role = "assistant", content = "" }, done = true, done_reason = "stop" }) + "\n";
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(stream, Encoding.UTF8, "application/x-ndjson"),
            };
        }
    }

    private sealed class StalledInterpreter : IInvestigationHypothesisInterpreter
    {
        public async ValueTask<InvestigationHypothesisProposal?> InterpretAsync(
            InvestigationHypothesisRecognitionRequest request, CancellationToken cancellationToken)
        {
            await Task.Delay(Timeout.Infinite, cancellationToken);
            return null;
        }
    }
}
