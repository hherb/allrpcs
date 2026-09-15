using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Starts one explicit authored contamination consideration from exact scene state.</summary>
public partial class TemporaryCharacterObjectSabotageContaminationConsiderationFlowController : Node
{
    private TemporaryCharacterObjectSabotageContaminationSource source = null!;
    private CharacterObjectSabotageContaminationController state = null!;
    private TemporaryCharacterObjectSabotageContaminationModelSource modelSource = null!;
    private CharacterObjectSabotageContaminationTaskController task = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        source = GetNode<TemporaryCharacterObjectSabotageContaminationSource>(
            "../TemporaryCharacterObjectSabotageContaminationSource");
        state = GetNode<CharacterObjectSabotageContaminationController>(
            "../CharacterObjectSabotageContaminationController");
        modelSource = GetNode<TemporaryCharacterObjectSabotageContaminationModelSource>(
            "../TemporaryCharacterObjectSabotageContaminationModelSource");
        task = GetNode<CharacterObjectSabotageContaminationTaskController>(
            "../CharacterObjectSabotageContaminationTaskController");

        if (!state.TryInitialize(source.Contamination, source.Evidence))
        {
            throw new InvalidOperationException(
                "The temporary contamination flow requires uninitialized scene-owned state.");
        }
    }

    /// <summary>Starts one decision from the exact current state only when explicitly requested.</summary>
    public bool TryStart()
    {
        if (task.Status != CharacterObjectSabotageContaminationTaskStatus.Idle
            || state.Contamination is not { } contamination
            || state.Evidence is not { } evidence)
        {
            return false;
        }

        CharacterObjectSabotageContaminationConsiderationRequest request = new(
            source.Character,
            source.PhysicalAccess,
            source.WorldObjects,
            contamination,
            evidence,
            [source.Candidate]);
        ICharacterObjectSabotageContaminationDecisionModel model = modelSource.TakeModel();
        return task.TryStart(model, request, state);
    }
}
