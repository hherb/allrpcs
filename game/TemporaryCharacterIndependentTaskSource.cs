using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Godot;

namespace Allrpcs.Game;

/// <summary>Authors one exact temporary independent task and initial party snapshot.</summary>
public partial class TemporaryCharacterIndependentTaskSource : Node
{
    private const string TaskIdText = "healer-inspect-herb-patch";

    /// <summary>Gets the exact authored departing character identifier.</summary>
    public CharacterId DepartingCharacterId { get; private set; } = null!;

    /// <summary>Gets the exact initial assigned-task snapshot.</summary>
    public CharacterIndependentTaskActivitySnapshot Activities { get; private set; } = null!;

    /// <summary>Gets the exact initial party snapshot.</summary>
    public CharacterPartySnapshot Party { get; private set; } = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        IReadOnlyList<Character> participants =
            GetNode<TemporaryOrdinaryCharacterParticipantSource>(
                "../TemporaryOrdinaryCharacterParticipantSource").Participants;
        ObjectId targetObjectId = GetNode<TemporaryWorldObjectSource>(
            "../TemporaryWorldObjectSource").WorldObjects.Entries[0].Identity.Id;
        DepartingCharacterId = participants[0].Identity.Id;
        CharacterId sageId = participants[1].Identity.Id;

        CharacterIndependentTaskAssignment assignment = new(
            new CharacterIndependentTaskId(TaskIdText),
            DepartingCharacterId,
            new CharacterIntention(CharacterIntentionType.ExamineObject, targetObjectId));
        Activities = new CharacterIndependentTaskActivitySnapshot(
            [new CharacterIndependentTaskActivity(
                assignment,
                CharacterIndependentTaskActivityState.Assigned)]);
        Party = new CharacterPartySnapshot([DepartingCharacterId, sageId]);
    }
}
