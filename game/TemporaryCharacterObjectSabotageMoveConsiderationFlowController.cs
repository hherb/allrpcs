using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Starts one explicit authored move consideration from exact scene state.</summary>
public partial class TemporaryCharacterObjectSabotageMoveConsiderationFlowController : Node
{
    private TemporaryCharacterObjectSabotageMoveSource source = null!;
    private CharacterObjectSabotageMoveController state = null!;
    private TemporaryCharacterObjectSabotageMoveModelSource modelSource = null!;
    private CharacterObjectSabotageMoveTaskController task = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        source = GetNode<TemporaryCharacterObjectSabotageMoveSource>(
            "../TemporaryCharacterObjectSabotageMoveSource");
        state = GetNode<CharacterObjectSabotageMoveController>(
            "../CharacterObjectSabotageMoveController");
        modelSource = GetNode<TemporaryCharacterObjectSabotageMoveModelSource>(
            "../TemporaryCharacterObjectSabotageMoveModelSource");
        task = GetNode<CharacterObjectSabotageMoveTaskController>(
            "../CharacterObjectSabotageMoveTaskController");
    }

    /// <summary>Starts one decision from the exact current state only when explicitly requested.</summary>
    public bool TryStart()
    {
        if (task.Status != CharacterObjectSabotageMoveTaskStatus.Idle
            || state.Placements is not { } placements
            || state.Evidence is not { } evidence)
        {
            return false;
        }

        CharacterObjectSabotageMoveConsiderationRequest request = new(
            source.Character,
            source.PhysicalAccess,
            source.WorldObjects,
            placements,
            evidence,
            [source.Candidate]);
        ICharacterObjectSabotageMoveDecisionModel model = modelSource.TakeModel();
        return task.TryStart(model, request, state);
    }
}
