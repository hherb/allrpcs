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

    private CharacterTurnActivityController? turns;
    private HerbExaminationActionController? examination;
    private WorldEventController? worldEvents;

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
        turns = GetNodeOrNull<CharacterTurnActivityController>("../CharacterTurnActivityController");
        examination = GetNodeOrNull<HerbExaminationActionController>("../HerbExaminationActionController");
        worldEvents = GetNodeOrNull<WorldEventController>("../WorldEventController");
        if (turns is not null) { turns.Completed += OnTurnCompleted; }
        if (examination is not null) { examination.Examined += OnExamined; }
    }

    /// <inheritdoc />
    public override void _ExitTree()
    {
        if (turns is not null) { turns.Completed -= OnTurnCompleted; }
        if (examination is not null) { examination.Examined -= OnExamined; }
    }

    private void OnTurnCompleted(CharacterTurnActivity completed) => Record(time =>
        CharacterMemoryRecordingTransition.RecordCompletedTurn(Memory, completed, EpisodeLocationId, time));

    private void OnExamined(CharacterExaminationResult result) => Record(time =>
        CharacterMemoryRecordingTransition.RecordObservation(
            Memory,
            result.Occurrence.TranscriptEntry.CharacterId,
            [],
            EpisodeLocationId,
            time,
            $"I examined {result.TargetObjectId.Value}; collection safety: {result.Safety.Value}."));

    // A rejected memory write must never break the turn or examination that produced it.
    private void Record(Func<WorldTimeSnapshot, CharacterMemorySnapshot> transition)
    {
        try { Memory = transition(worldEvents?.WorldTime ?? new WorldTimeSnapshot(null)); }
        catch (Exception exception) when (exception is ArgumentException or InvalidOperationException)
        {
            GD.PushWarning($"Character memory was not recorded: {exception.Message}");
        }
    }
}
