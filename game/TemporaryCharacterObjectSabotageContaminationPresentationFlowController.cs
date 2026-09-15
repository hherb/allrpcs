using Godot;

namespace Allrpcs.Game;

/// <summary>Initializes one exact temporary contamination marker from scene-owned state.</summary>
public partial class TemporaryCharacterObjectSabotageContaminationPresentationFlowController : Node
{
    /// <inheritdoc />
    public override void _Ready()
    {
        TemporaryCharacterObjectSabotageContaminationSource source =
            GetNode<TemporaryCharacterObjectSabotageContaminationSource>(
                "../TemporaryCharacterObjectSabotageContaminationSource");
        CharacterObjectSabotageContaminationController state =
            GetNode<CharacterObjectSabotageContaminationController>(
                "../CharacterObjectSabotageContaminationController");
        CharacterObjectSabotageContaminationPresentationController presentation =
            GetNode<CharacterObjectSabotageContaminationPresentationController>(
                "../CharacterObjectSabotageContaminationPresentationController");
        CanvasItem marker = GetNode<CanvasItem>(
            "../InteractionCandidates/WaterCask/ContaminationMarker");

        if (state.Contamination is not { } contamination
            || !presentation.TryInitialize(
                source.Candidate.Occurrence.Target.Identity.Id,
                marker,
                contamination))
        {
            throw new InvalidOperationException(
                "The temporary contamination presentation requires exact initialized scene state.");
        }
    }
}
