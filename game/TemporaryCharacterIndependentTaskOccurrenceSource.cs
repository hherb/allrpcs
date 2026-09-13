using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.WorldEvents;
using Godot;

namespace Allrpcs.Game;

/// <summary>Authors one exact temporary occurrence for the scene independent task.</summary>
public partial class TemporaryCharacterIndependentTaskOccurrenceSource : Node
{
    private CharacterIndependentTaskAssignment assignment = null!;
    private WorldEvent worldEvent = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        CharacterIndependentTaskActivitySnapshot activities =
            GetNode<TemporaryCharacterIndependentTaskSource>(
                "../TemporaryCharacterIndependentTaskSource").Activities;
        assignment = activities.Activities.Single().Assignment;
        worldEvent = GetNode<TemporaryWorldEventSource>(
            "../TemporaryWorldEventSource").HerbPatchExaminedWorldEvent;
    }

    /// <summary>Creates the exact physical-action occurrence for the authored active task.</summary>
    public CharacterIndependentTaskWorldEventOccurrence CreateOccurrence(
        CharacterIndependentTaskActivity activeActivity)
    {
        ArgumentNullException.ThrowIfNull(activeActivity);

        if (!ReferenceEquals(activeActivity.Assignment, assignment)
            || activeActivity.State != CharacterIndependentTaskActivityState.Active)
        {
            throw new InvalidOperationException(
                "Only the exact authored active task has a temporary occurrence.");
        }

        CharacterTurnWorldEventOccurrence occurrence = new(
            worldEvent,
            new CharacterTurnTranscriptEntry(
                assignment.CharacterId,
                assignment.Intention));
        return new CharacterIndependentTaskWorldEventOccurrence(activeActivity, occurrence);
    }
}
