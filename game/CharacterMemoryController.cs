using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;
using Godot;

namespace Allrpcs.Game;

/// <summary>Owns the scene's exact immutable provider-neutral character-memory snapshot.</summary>
public partial class CharacterMemoryController : Node
{
    // shortcut: every episode is placed in the village square; use the body's region once the scene maps positions to locations.
    private static readonly LocationId EpisodeLocationId = new("village-square");

    private CharacterTurnActivityController turns = null!;
    private HerbExaminationActionController? examination;
    private WorldEventController worldEvents = null!;

    /// <summary>Gets the exact installed canonical memory, initially an explicit empty snapshot.</summary>
    public CharacterMemorySnapshot Memory { get; private set; } = new([]);

    /// <summary>Installs one exact caller-supplied immutable replacement.</summary>
    public void Install(CharacterMemorySnapshot memory)
    {
        ArgumentNullException.ThrowIfNull(memory);

        Memory = memory;
    }

    /// <inheritdoc />
    public override void _Ready()
    {
        turns = GetNode<CharacterTurnActivityController>("../CharacterTurnActivityController");
        worldEvents = GetNode<WorldEventController>("../WorldEventController");
        turns.Completed += OnTurnCompleted;
    }

    /// <summary>
    /// Records each committed examination. Main calls this once, after its own _Ready adds the
    /// examination controller, which is later than this node's _Ready.
    /// </summary>
    internal void ObserveExaminations(HerbExaminationActionController action)
    {
        ArgumentNullException.ThrowIfNull(action);
        if (examination is not null) { throw new InvalidOperationException("Examinations are already observed."); }
        examination = action;
        examination.Examined += OnExamined;
    }

    /// <inheritdoc />
    public override void _ExitTree()
    {
        turns.Completed -= OnTurnCompleted;
        if (examination is not null) { examination.Examined -= OnExamined; }
    }

    private void OnTurnCompleted(CharacterTurnActivity completed) =>
        Record("turn", completed.Request.RespondingCharacterId, time =>
            CharacterMemoryRecordingTransition.RecordCompletedTurn(Memory, completed, EpisodeLocationId, time));

    private void OnExamined(CharacterExaminationResult result) =>
        Record("examination", result.Occurrence.TranscriptEntry.CharacterId, time =>
            CharacterMemoryRecordingTransition.RecordExamination(
                Memory,
                result.Occurrence.TranscriptEntry.CharacterId,
                result.TargetObjectId,
                result.Safety,
                EpisodeLocationId,
                time));

    // A rejected memory write must never break the turn or examination that produced it.
    private void Record(
        string source,
        CharacterId characterId,
        Func<WorldTimeSnapshot, CharacterMemorySnapshot> transition)
    {
        try
        {
            Memory = transition(worldEvents.WorldTime
                ?? throw new InvalidOperationException("World time is not installed."));
        }
        catch (Exception exception) when (exception is ArgumentException or InvalidOperationException)
        {
            GD.PushError(
                $"Character memory for '{characterId.Value}' was not recorded from a {source}: {exception.Message}");
        }
    }
}
