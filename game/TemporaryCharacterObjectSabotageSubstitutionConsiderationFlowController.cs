using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Starts one explicit authored substitution consideration from exact scene state.</summary>
public partial class TemporaryCharacterObjectSabotageSubstitutionConsiderationFlowController : Node
{
    private TemporaryCharacterObjectSabotageSubstitutionSource source = null!;
    private CharacterObjectSabotageSubstitutionController state = null!;
    private TemporaryCharacterObjectSabotageSubstitutionModelSource modelSource = null!;
    private CharacterObjectSabotageSubstitutionTaskController task = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        source = GetNode<TemporaryCharacterObjectSabotageSubstitutionSource>(
            "../TemporaryCharacterObjectSabotageSubstitutionSource");
        state = GetNode<CharacterObjectSabotageSubstitutionController>(
            "../CharacterObjectSabotageSubstitutionController");
        modelSource = GetNode<TemporaryCharacterObjectSabotageSubstitutionModelSource>(
            "../TemporaryCharacterObjectSabotageSubstitutionModelSource");
        task = GetNode<CharacterObjectSabotageSubstitutionTaskController>(
            "../CharacterObjectSabotageSubstitutionTaskController");
    }

    /// <summary>Starts one decision from the exact current state only when explicitly requested.</summary>
    public bool TryStart()
    {
        if (task.Status != CharacterObjectSabotageSubstitutionTaskStatus.Idle
            || state.Substitutions is not { } substitutions
            || state.Evidence is not { } evidence)
        {
            return false;
        }

        CharacterObjectSabotageSubstitutionConsiderationRequest request = new(
            source.Character,
            source.PhysicalAccess,
            source.WorldObjects,
            substitutions,
            evidence,
            [source.Candidate]);
        ICharacterObjectSabotageSubstitutionDecisionModel model = modelSource.TakeModel();
        return task.TryStart(model, request, state);
    }
}
