using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Godot;

namespace Allrpcs.Game;

/// <summary>Starts one generic sabotage decision from exact ordinary-turn authoring provenance.</summary>
public partial class CharacterSabotageOrdinaryTurnConsiderationStartCoordinator : Node
{
    private CharacterTurnActivityController turns = null!;
    private CharacterSabotageOrdinaryTurnAuthoringCoordinator authoring = null!;
    private CharacterSabotageActionRequestComposer composer = null!;
    private CharacterSabotageActionRequestConsiderationFlowController flow = null!;
    private CharacterSabotageActionTaskController task = null!;

    /// <summary>Gets the exact ordinary turn that guarded the started decision.</summary>
    public CharacterTurnActivity? Turn { get; private set; }

    /// <summary>Gets the exact composed request started through the existing task flow.</summary>
    public CharacterSabotageActionConsiderationRequest? Request { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        turns = GetNode<CharacterTurnActivityController>("../CharacterTurnActivityController");
        authoring = GetNode<CharacterSabotageOrdinaryTurnAuthoringCoordinator>(
            "../CharacterSabotageOrdinaryTurnAuthoringCoordinator");
        composer = GetNode<CharacterSabotageActionRequestComposer>(
            "../CharacterSabotageActionRequestComposer");
        flow = GetNode<CharacterSabotageActionRequestConsiderationFlowController>(
            "../CharacterSabotageActionRequestConsiderationFlowController");
        task = GetNode<CharacterSabotageActionTaskController>(
            "../CharacterSabotageActionTaskController");
    }

    /// <summary>Starts once only while the complete exact authored ordinary-turn graph is current.</summary>
    public bool TryStart(
        CharacterTurnActivity? expectedTurn,
        CharacterSabotageActionConsiderationRequest? expectedRequest)
    {
        if (Turn is not null
            || Request is not null
            || expectedTurn is null
            || expectedRequest is null
            || !ReferenceEquals(turns.Activity, expectedTurn)
            || !ReferenceEquals(composer.Request, expectedRequest)
            || task.Status != CharacterSabotageActionTaskStatus.Idle
            || task.Result is not null
            || !authoring.IsCurrent(expectedTurn, expectedRequest)
            || !flow.TryStart(expectedRequest))
        {
            return false;
        }

        Turn = expectedTurn;
        Request = expectedRequest;
        return true;
    }
}
