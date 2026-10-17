using System.Reflection;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Persistence;
using Godot;

namespace Allrpcs.Game;

public partial class SaveSnapshotRestorationControllerSmokeCheck
{
    private bool CheckPreparationFailurePreservesState(SaveSnapshot snapshot)
    {
        if (OS.GetEnvironment("ALLRPCS_RESTORE_REGRESSION") == "session") { return true; }
        FirstPatientPreparationController preparation = GetNode<FirstPatientPreparationController>(
            "../FirstPatientPreparationController");
        FieldInfo field = typeof(FirstPatientPreparationController).GetField(
            "initialIngredients", BindingFlags.Instance | BindingFlags.NonPublic)!;
        object? ingredients = field.GetValue(preparation);
        object? priorTruth = firstPatientController.DiseaseTruth;
        object? priorJournal = journalController.Journal;
        field.SetValue(preparation, null);
        bool rejected;
        try { rejected = !controller.TryRestore(snapshot); }
        catch (InvalidOperationException) { rejected = false; }
        finally { field.SetValue(preparation, ingredients); }
        if (!rejected || !ReferenceEquals(priorTruth, firstPatientController.DiseaseTruth)
            || !ReferenceEquals(priorJournal, journalController.Journal))
        {
            Fail("Restore ownership: failed preparation changed installed owners or threw after commit.");
            return false;
        }
        return true;
    }

    private bool CheckSessionReset(SaveSnapshot snapshot)
    {
        CharacterTurnTranscriptController transcript = GetNode<CharacterTurnTranscriptController>(
            "../CharacterTurnTranscriptController");
        _ = transcript.TryAppendSpokenStatement(new DialogueDelivery(
            new DialogueUtterance(discoveryController.SearcherCharacterId, "Session-only speech."),
            DialogueDeliveryMode.OrdinarySpeech));
        TemporaryResolvedPerceptionIndicator[] indicators =
            (TemporaryResolvedPerceptionIndicator[])typeof(Main).GetField(
                "resolvedPerceptionIndicators", BindingFlags.Instance | BindingFlags.NonPublic)!
                .GetValue(GetParent<Main>())!;
        indicators[0].Present(worldObjectController.WorldObjects!.Entries[1], snapshot.Discovery.Perception);
        JournalOpenCloseInputController journalInput = GetNode<JournalOpenCloseInputController>(
            "../JournalOpenCloseInputController");
        bool opened = (bool)typeof(JournalOpenCloseInputController).GetMethod(
            "TryOpen", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(journalInput, null)!;
        JournalSectionPanel panel = GetNode<JournalSectionPanel>("../JournalSectionPanel");
        if (!opened || !panel.Visible || !indicators[0].IsIndicatorVisible)
        {
            Fail("Restore ownership: modal/perception fixture was not populated.");
            return false;
        }
        if (transcript.Transcript is not { Entries.Count: > 0 })
        {
            Fail("Restore ownership: transcript fixture was not populated.");
            return false;
        }
        CharacterTurnActivityController turn = GetNode<CharacterTurnActivityController>(
            "../CharacterTurnActivityController");
        DialogueUtterance speech = new(new Allrpcs.Domain.Identifiers.CharacterId("healer"), "Old reply.");
        DialogueDelivery delivery = new(speech, DialogueDeliveryMode.OrdinarySpeech);
        DialogueUtterance playerSpeech = new(discoveryController.SearcherCharacterId, "Answer me.");
        DialogueEvent heard = new(new DialogueDelivery(playerSpeech, DialogueDeliveryMode.OrdinarySpeech),
            new DialogueAudienceSnapshot(playerSpeech.SpeakerCharacterId, [speech.SpeakerCharacterId]));
        CharacterTurnRequest request = new(speech.SpeakerCharacterId, heard,
            new DialogueAct(playerSpeech, DialogueActType.Request));
        CharacterTurnResponse response = new(request, CharacterTurnDisposition.Accept, delivery,
            new CharacterIntention(CharacterIntentionType.Wait, null), [], []);
        if (!turn.TryStart(request) || !turn.TryReceiveResponse(response))
        {
            Fail("Restore ownership: speaking fixture did not start.");
            return false;
        }
        TemporaryCharacterTurnResponseLabel responseLabel = GetNode<TemporaryCharacterTurnResponseLabel>(
            "../DialogueOverlay/Panel/Response");
        responseLabel._Process(0);
        responseLabel._Process(0);
        SaveSnapshot rejected = new(new SaveSchemaVersion(999), snapshot.Discovery, snapshot.WorldEvents);
        object? priorTranscript = transcript.Transcript;
        if (controller.TryRestore(rejected) || !ReferenceEquals(priorTranscript, transcript.Transcript)
            || !panel.Visible || !indicators[0].IsIndicatorVisible)
        {
            Fail("Restore ownership: rejected load cleared the transcript.");
            return false;
        }
        if (!controller.TryRestore(snapshot) || transcript.Transcript is not { Entries.Count: 0 }
            || panel.Visible || indicators.Any(indicator => indicator.IsIndicatorVisible))
        {
            Fail("Restore ownership: successful load retained the session transcript.");
            return false;
        }
        responseLabel._Process(0);
        responseLabel._Process(0);
        if (turn.Activity?.Response is not null || responseLabel.Text != "Character response: NONE")
        {
            Fail("Restore ownership: cancelled pre-load speech reappeared after resetting the response label.");
            return false;
        }
        SeekerMapOpenCloseInputController mapInput = GetNode<SeekerMapOpenCloseInputController>(
            "../SeekerMapOpenCloseInputController");
        bool mapOpened = (bool)typeof(SeekerMapOpenCloseInputController).GetMethod(
            "TryOpen", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(mapInput, null)!;
        SeekerMapPanel mapPanel = GetNode<SeekerMapPanel>("../SeekerMapPanel");
        if (!mapOpened || !mapPanel.Visible || controller.TryRestore(rejected) || !mapPanel.Visible
            || !controller.TryRestore(snapshot) || mapPanel.Visible)
        {
            Fail("Restore ownership: map modal reset or rejected-load preservation failed.");
            return false;
        }
        string[] expectedOwners =
        [
            "discovery",
            "world-events",
            "character-memory",
            "first-patient",
            "patient-testimony",
            "sage-consultation",
            "journal",
            "journal-selection",
            "map",
            "route-progress",
            "route-completions",
            "route-consumption",
            "participation-coordinator",
            "participation-round",
            "conversation-budget",
            "generation-input-fence",
            "generation-flow-and-stream",
            "character-turn",
            "transcript",
            "dialogue-input",
            "main-result-tracking",
            "character-response-presentation",
            "resolved-perception",
            "journal-modal",
            "map-modal",
            "first-patient-preparation",
        ];
        if (!controller.RegisteredOwners.SequenceEqual(expectedOwners))
        {
            Fail("Restore ownership: explicit owner registry changed without inventory review.");
            return false;
        }
        return true;
    }
}
