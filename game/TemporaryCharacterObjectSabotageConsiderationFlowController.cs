using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Starts one explicit authored concealment consideration from exact scene state.</summary>
public partial class TemporaryCharacterObjectSabotageConsiderationFlowController : Node
{
    private TemporaryCharacterObjectSabotageSource source = null!;
    private CharacterObjectSabotageConcealmentController state = null!;
    private TemporaryCharacterObjectSabotageConcealmentModelSource modelSource = null!;
    private CharacterObjectSabotageConcealmentTaskController task = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        source = GetNode<TemporaryCharacterObjectSabotageSource>(
            "../TemporaryCharacterObjectSabotageSource");
        state = GetNode<CharacterObjectSabotageConcealmentController>(
            "../CharacterObjectSabotageConcealmentController");
        modelSource = GetNode<TemporaryCharacterObjectSabotageConcealmentModelSource>(
            "../TemporaryCharacterObjectSabotageConcealmentModelSource");
        task = GetNode<CharacterObjectSabotageConcealmentTaskController>(
            "../CharacterObjectSabotageConcealmentTaskController");
    }

    /// <summary>Starts one decision from the exact current state only when explicitly requested.</summary>
    public bool TryStart()
    {
        if (task.Status != CharacterObjectSabotageConcealmentTaskStatus.Idle
            || state.Concealment is not { } concealment
            || state.Evidence is not { } evidence)
        {
            return false;
        }

        CharacterObjectSabotageConcealmentConsiderationRequest request = new(
            source.Character,
            source.PhysicalAccess,
            source.WorldObjects,
            concealment,
            evidence,
            source.Candidates);
        ICharacterObjectSabotageConcealmentDecisionModel model = modelSource.TakeModel();
        return task.TryStart(model, request, state);
    }
}
