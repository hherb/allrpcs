using Allrpcs.Application.Audio;
using Allrpcs.Application.Map;
using Allrpcs.Application.Navigation;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Persistence;
using Allrpcs.Domain.WorldEvents;
using Allrpcs.Infrastructure.Persistence;
using Godot;

namespace Allrpcs.Game;

/// <summary>Exercises ordinary material wiring, committed action rejection paths and accessible views.</summary>
public partial class MaterialAudioRuntimeSmokeCheck : Node
{
    /// <inheritdoc />
    public override async void _Ready()
    {
        if (OS.GetEnvironment("ALLRPCS_MATERIAL_MAIN_CHECK") != "1") { return; }
        try
        {
            await Frame(); await Frame();
            Seeker seeker = GetNode<Seeker>("../Seeker");
            seeker.SetPhysicsProcess(false);
            GetNode<Healer>("../Healer").SetPhysicsProcess(false);
            OpeningRegionMaterialAudioController audio = GetNode<OpeningRegionMaterialAudioController>("../OpeningRegionMaterialAudioController");
            AudioSettingsController settings = GetNode<AudioSettingsController>("../AudioSettingsController");
            MaterialSoundCaptionPanel captions = GetNode<MaterialSoundCaptionPanel>("../MaterialSoundCaptionPanel");
            OpeningRegionProjectionSource projection = GetNode<OpeningRegionProjectionSource>("../OpeningRegionProjectionSource");
            WorldTimeController time = GetNode<WorldTimeController>("../WorldTimeController");
            object initialTime = time.WorldTime;
            foreach (OpeningRegionAnchor anchor in projection.Projection.Anchors)
            {
                seeker.GlobalPosition = projection.ToGlobal(new Vector2(anchor.Position.X, anchor.Position.Y));
                await Frame(); await Frame();
                Require(audio.Current is not null, "Missing live regional selection.");
                Require(captions.GetNode<Label>("Panel/Lines/Region").Text == audio.Current!.Caption,
                    "Region caption did not follow live presentation.");
            }
            Require(audio.GetChildCount() == 6 && audio.GetChildren().All(child => child is AudioStreamPlayer),
                "Material audio exceeded its six players.");
            Require(audio.GetChildren().Cast<AudioStreamPlayer>().All(player => player.Stream is not null),
                "An accepted sound failed engine import.");
            Require(audio.GetChildren().Cast<AudioStreamPlayer>().All(player =>
                player.Stream is AudioStreamWav wave && wave.Format == AudioStreamWav.FormatEnum.Format16Bits
                && wave.MixRate == 48000 && wave.Stereo), "Material import added lossy compression or changed sample layout.");
            Require(ReferenceEquals(initialTime, time.WorldTime), "Regional playback changed time.");

            AudioSettingsPanel panel = GetNode<AudioSettingsPanel>("../AudioSettingsPanel");
            foreach (Vector2I size in new[] { new Vector2I(1280, 720), new Vector2I(1600, 900) })
            {
                GetWindow().Size = size;
                panel.Open(); await Frame(); await Frame();
                Require(GetViewport().GetVisibleRect().Encloses(panel.GetNode<Control>("Shade/Panel").GetGlobalRect()),
                    "Audio settings exceeded viewport after adding credits.");
                await Capture($"settings-{size.X}");
                Button entry = panel.FindChild("SourceCredits", true, false) as Button
                    ?? throw new InvalidOperationException("Missing credits entry.");
                entry.EmitSignal(BaseButton.SignalName.Pressed); await Frame();
                MaterialAudioCreditsPanel credits = panel.GetNode<MaterialAudioCreditsPanel>("MaterialAudioCreditsPanel");
                Require(credits.Visible && !panel.Visible, "Credits did not replace Audio surface.");
                Require(GetViewport().GetVisibleRect().Encloses(credits.GetNode<Control>("Shade/Panel").GetGlobalRect()),
                    "Credits exceeded viewport.");
                RichTextLabel notices = credits.FindChild("Credits", true, false) as RichTextLabel
                    ?? throw new InvalidOperationException("Missing source notice text.");
                Require(notices.Text.Contains("kurt", StringComparison.Ordinal)
                    && notices.Text.Contains("Alexander", StringComparison.Ordinal), "Missing attribution authors.");
                await Capture($"credits-{size.X}");
                panel._Input(new InputEventAction { Action = "ui_cancel", Pressed = true });
                Require(!credits.Visible && panel.Visible && entry.HasFocus(), "Credits did not restore entry focus.");
                panel.Close(false);
                Require(GetViewport().GetVisibleRect().Encloses(captions.GetNode<Control>("Panel").GetGlobalRect()),
                    "Caption surface exceeded viewport.");
            }
            settings.SetPreferences(new(settings.Preferences.WithChannel(AudioCategory.Master, 0, true).Channels,
                AudioMixPreset.ReducedRange, true, false));
            Require(!captions.Visible, "Caption preference did not hide captions.");
            Require(ReferenceEquals(initialTime, time.WorldTime), "Settings changed time.");
            settings.RestoreDefaults();

            JournalController journal = GetNode<JournalController>("../JournalController");
            if (journal.Journal is null)
            {
                Require(GetNode<RepresentativeJournalCompositionCoordinator>("../RepresentativeJournalCompositionCoordinator")
                    .TryComposeAndInitialize(), "Cannot arrange persistence journal fixture.");
            }
            SeekerMapController map = GetNode<SeekerMapController>("../SeekerMapController");
            if (map.Map is null)
            {
                MapSaveStateRestorationProjection.Restore(new MapSaveState(
                    GetNode<TemporarySeekerCharacterSource>("../TemporarySeekerCharacterSource").Character.Identity.Id,
                    [], [], [])).InstallInto(map);
            }
            OpeningRegionInvestigationRouteProgressController routes =
                GetNode<OpeningRegionInvestigationRouteProgressController>("../OpeningRegionInvestigationRouteProgressController");
            if (routes.Progress is null)
            {
                Require(SaveRouteLifecycleSmokeArrangement.TryInitializeEmpty(routes, map,
                    GetNode<OpeningRegionOldShrineSceneSource>("../OpeningRegionOldShrineSceneSource")),
                    "Cannot arrange persistence route fixture.");
            }
            SaveSnapshot snapshot = GetNode<SaveSnapshotCaptureController>("../SaveSnapshotCaptureController").Capture()
                ?? throw new InvalidOperationException("Missing arranged save root.");
            string actionMode = OS.GetEnvironment("ALLRPCS_MATERIAL_ACTION_MODE");
            if (actionMode == "muted")
            {
                settings.SetPreferences(settings.Preferences.WithChannel(AudioCategory.Effects, 0, true));
            }
            if (actionMode == "captions-off")
            {
                settings.SetPreferences(new(settings.Preferences.Channels, settings.Preferences.Preset,
                    settings.Preferences.Mono, false));
            }
            if (actionMode == "missing")
            {
                audio.GetNode<AudioStreamPlayer>("A04").Stream = null;
                audio.GetNode<AudioStreamPlayer>("A05").Stream = null;
            }

            OpeningRegionRemedyPreparationInputCoordinator prep = GetNode<OpeningRegionRemedyPreparationInputCoordinator>("../OpeningRegionRemedyPreparationInputCoordinator");
            OpeningRegionRemedyPreparationSceneBinding station = GetNode<OpeningRegionRemedyPreparationSceneSource>("../OpeningRegionRemedyPreparationSceneSource").Binding!;
            OpeningRegionFirstPatientAdministrationInputCoordinator treatment = GetNode<OpeningRegionFirstPatientAdministrationInputCoordinator>("../OpeningRegionFirstPatientAdministrationInputCoordinator");
            OpeningRegionFirstPatientSceneBinding patient = GetNode<OpeningRegionFirstPatientSceneSource>("../OpeningRegionFirstPatientSceneSource").Binding!;
            int grinding = 0; int administered = 0; int restored = 0;
            prep.GrindingCompleted += (_, _, _) => throw new InvalidOperationException("expected isolated subscriber probe");
            prep.GrindingCompleted += (_, _, _) => grinding++;
            treatment.TreatmentCompleted += _ => throw new InvalidOperationException("expected isolated subscriber probe");
            treatment.TreatmentCompleted += _ => administered++;
            seeker.GlobalPosition = new Vector2(station.StationPosition.X + 49, station.StationPosition.Y);
            prep._UnhandledInput(Action("remedy_prepare"));
            Require(grinding == 0, "Remote request emitted grinding.");
            seeker.GlobalPosition = new Vector2(station.StationPosition.X, station.StationPosition.Y);
            prep._UnhandledInput(Action("remedy_prepare"));
            Require(grinding == 0, "Recipe start emitted grinding.");
            prep._UnhandledInput(Action("remedy_prepare"));
            Require(grinding == 1 && audio.GetNode<AudioStreamPlayer>("A04").Playing == (actionMode != "missing"),
                "Exact grinding feedback did not follow availability.");
            for (int index = 0; index < station.Recipe.Plan.Steps.Count + 2; index++) { prep._UnhandledInput(Action("remedy_prepare")); }
            Require(grinding == 1, "Other steps/completion/repeat emitted grinding.");
            seeker.GlobalPosition = new Vector2(patient.PatientPosition.X + 65, patient.PatientPosition.Y);
            treatment._UnhandledInput(Action("remedy_administer"));
            Require(administered == 0, "Remote treatment emitted feedback.");
            seeker.GlobalPosition = new Vector2(patient.PatientPosition.X, patient.PatientPosition.Y);
            treatment._UnhandledInput(Action("remedy_administer"));
            Require(administered == 1 && audio.GetNode<AudioStreamPlayer>("A05").Playing == (actionMode != "missing"),
                "Committed treatment feedback did not follow availability.");
            object committedTime = time.WorldTime;
            treatment._UnhandledInput(Action("remedy_administer"));
            Require(administered == 1 && ReferenceEquals(committedTime, time.WorldTime), "Repeat treatment replayed feedback or changed time.");
            await Frame(); await Frame();
            GetWindow().Size = new Vector2I(1280, 720);
            await Capture("treatment-1280");
            string resultPath = OS.GetEnvironment("ALLRPCS_MATERIAL_RESULT_PATH");
            if (resultPath.Length > 0)
            {
                FirstPatientStateController patientState = GetNode<FirstPatientStateController>("../FirstPatientStateController");
                WorldEventController world = GetNode<WorldEventController>("../WorldEventController");
                File.WriteAllText(resultPath, string.Join('\n',
                    patientState.DiseaseTruth!.Infections.Single().CareState.ToString(),
                    patientState.PreparedBatches!.Batches.Count.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    patientState.AdministrationExperiments!.Records.Count.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    string.Join(',', world.EventLog!.Entries.Select(entry => entry.Value)),
                    world.WorldTime!.CurrentEventId?.Value ?? "none"));
                SaveSnapshot committedSnapshot = GetNode<SaveSnapshotCaptureController>("../SaveSnapshotCaptureController")
                    .Capture() ?? throw new InvalidOperationException("Missing committed action snapshot.");
                File.WriteAllText(resultPath + ".json", SaveSnapshotJsonSerializer.Serialize(committedSnapshot));
            }
            settings.SetPreferences(settings.Preferences.WithChannel(AudioCategory.Effects, 1, true));
            Require(captions.Visible == settings.Preferences.Captions, "Mute changed semantic caption preference.");
            await ToSignal(GetTree().CreateTimer(0.8), SceneTreeTimer.SignalName.Timeout);
            Require(settings.Ducking.Gain(AudioCategory.Ambience) == 1, "Mute retained a stale foreground lease.");
            settings.RestoreDefaults();
            SaveSnapshotRestorationController restore = GetNode<SaveSnapshotRestorationController>("../SaveSnapshotRestorationController");
            restore.PresentationReset += () => throw new InvalidOperationException("expected isolated subscriber probe");
            restore.PresentationReset += () => restored++;
            WorldEventId unknown = new("unknown-material-probe");
            SaveSnapshot rejected = new(snapshot.SchemaVersion, snapshot.Discovery,
                new WorldEventSaveState(new WorldEventLogSnapshot([unknown]), new WorldTimeSnapshot(unknown)),
                snapshot.CharacterMemory, snapshot.FirstPatient, snapshot.Journal, snapshot.Map,
                snapshot.InvestigationRoutes, snapshot.FirstPatientRecovery);
            Require(!restore.TryRestore(rejected) && restored == 0 && ReferenceEquals(committedTime, time.WorldTime),
                "Failed restoration emitted reset or changed committed state.");
            Require(restore.TryRestore(snapshot), "Live restore failed.");
            Require(restored == 1 && grinding == 1 && administered == 1, "Restore replayed action history.");
            Require(!audio.GetNode<AudioStreamPlayer>("A04").Playing && !audio.GetNode<AudioStreamPlayer>("A05").Playing,
                "Restore did not stop transient actions.");
            Require(captions.GetNode<Label>("Panel/Lines/Action").Text.Length == 0, "Restore retained action caption.");
            AudioStreamPlayer treatmentPlayer = audio.GetNode<AudioStreamPlayer>("A05");
            AudioStream? treatmentStream = treatmentPlayer.Stream;
            treatmentPlayer.Stream = GD.Load<AudioStreamWav>("res://assets/audio/material/A05.wav");
            audio.PlayCue("A05");
            await ToSignal(GetTree().CreateTimer(0.8), SceneTreeTimer.SignalName.Timeout);
            Require(settings.Ducking.Gain(AudioCategory.Ambience) < 1, "Audible treatment acquired no lease.");
            treatmentPlayer.Stream = null;
            audio.PlayCue("A05");
            await ToSignal(GetTree().CreateTimer(0.8), SceneTreeTimer.SignalName.Timeout);
            Require(settings.Ducking.Gain(AudioCategory.Ambience) == 1, "Missing playback retained a lease.");
            treatmentPlayer.Stream = treatmentStream ?? GD.Load<AudioStreamWav>("res://assets/audio/material/A05.wav");
            audio.PlayCue("A05");
            await ToSignal(GetTree().CreateTimer(0.8), SceneTreeTimer.SignalName.Timeout);
            Require(settings.Ducking.Gain(AudioCategory.Ambience) < 1, "Teardown arrangement acquired no lease.");
            audio.GetParent().RemoveChild(audio);
            audio.QueueFree();
            await ToSignal(GetTree().CreateTimer(0.8), SceneTreeTimer.SignalName.Timeout);
            Require(settings.Ducking.Gain(AudioCategory.Ambience) == 1, "Teardown retained a lease.");
            GD.Print("Material Main smoke passed: 11 live anchors, six imported players, base/expanded captions/settings/credits, focus return, mute/caption isolation, ordinary action negative paths, failing-subscriber isolation, missing/teardown lease cleanup and successful restore without replay.");
            GetTree().Quit();
        }
        catch (Exception exception) { GD.PushError(exception.ToString()); GetTree().Quit(1); }
    }

    private static InputEventAction Action(string action) => new() { Action = action, Pressed = true };
    private async Task Frame() => await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
    private async Task Capture(string name)
    {
        string folder = OS.GetEnvironment("ALLRPCS_MATERIAL_PREVIEW_PATH");
        if (folder.Length == 0) { return; }
        await Frame();
        await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
        Require(GetViewport().GetTexture().GetImage().SavePng(Path.Combine(folder, name + ".png")) == Error.Ok,
            "Cannot retain material runtime view.");
    }
    private static void Require(bool value, string message)
    {
        if (!value) { throw new InvalidOperationException(message); }
    }
}
