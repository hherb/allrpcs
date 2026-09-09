using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks the temporary exact-target examination-result source when enabled.</summary>
public partial class TemporaryCharacterExaminationResultSourceSmokeCheck : Node
{
    private const string CheckEnvironmentVariable =
        "ALLRPCS_CHARACTER_EXAMINATION_RESULT_SOURCE_CHECK";
    private const string EnabledEnvironmentValue = "1";

    /// <inheritdoc />
    public override void _Ready()
    {
        if (OS.GetEnvironment(CheckEnvironmentVariable) != EnabledEnvironmentValue)
        {
            return;
        }

        TemporaryCharacterExaminationResultSource source =
            GetNode<TemporaryCharacterExaminationResultSource>(
                "../TemporaryCharacterExaminationResultSource");
        TemporaryWorldEventSource worldEvents =
            GetNode<TemporaryWorldEventSource>("../TemporaryWorldEventSource");
        ObjectId targetObjectId = GetNode<TemporaryWorldObjectSource>(
            "../TemporaryWorldObjectSource").WorldObjects.Entries[0].Identity.Id;
        CharacterIntention action = new(CharacterIntentionType.ExamineObject, targetObjectId);
        CharacterTurnWorldEventOccurrence occurrence = new(
            worldEvents.HerbPatchExaminedWorldEvent,
            new CharacterTurnTranscriptEntry(new CharacterId("healer"), action));

        CharacterExaminationResult? first = source.CreateResult(occurrence, targetObjectId);
        CharacterExaminationResult? second = source.CreateResult(occurrence, targetObjectId);
        if (first is null
            || second is null
            || ReferenceEquals(first, second)
            || !ReferenceEquals(first.Occurrence, occurrence)
            || !ReferenceEquals(first.TargetObjectId, targetObjectId)
            || !ReferenceEquals(first.Safety, CharacterExaminationSafety.SafeToCollect)
            || source.CreateResult(occurrence, new ObjectId(targetObjectId.Value)) is not null
            || !RejectsNullInputs(source, occurrence, targetObjectId))
        {
            GD.PushError("Temporary examination-result source failed exact safe selection.");
            GetTree().Quit(exitCode: 1);
            return;
        }

        GD.Print(
            "Temporary examination-result source smoke passed: fresh safe results retained only "
            + "the exact observed herb-patch examination and exact supplied target.");
        GetTree().Quit();
    }

    private static bool RejectsNullInputs(
        TemporaryCharacterExaminationResultSource source,
        CharacterTurnWorldEventOccurrence occurrence,
        ObjectId targetObjectId)
    {
        try
        {
            _ = source.CreateResult(null!, targetObjectId);
            return false;
        }
        catch (ArgumentNullException)
        {
        }

        try
        {
            _ = source.CreateResult(occurrence, null!);
            return false;
        }
        catch (ArgumentNullException)
        {
            return true;
        }
    }
}
