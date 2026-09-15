using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Perception;
using Godot;

namespace Allrpcs.Game;

/// <summary>Initializes one exact temporary contamination marker from scene-owned state.</summary>
public partial class TemporaryCharacterObjectSabotageContaminationPresentationFlowController : Node
{
    private TemporaryCharacterObjectSabotageContaminationSource source = null!;
    private CharacterObjectSabotageContaminationController state = null!;
    private CharacterObjectSabotageContaminationPresentationController presentation = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        source = GetNode<TemporaryCharacterObjectSabotageContaminationSource>(
            "../TemporaryCharacterObjectSabotageContaminationSource");
        state = GetNode<CharacterObjectSabotageContaminationController>(
            "../CharacterObjectSabotageContaminationController");
        presentation = GetNode<CharacterObjectSabotageContaminationPresentationController>(
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

    /// <summary>Explicitly recovers and presents contamination through visible exact evidence.</summary>
    public bool TryRecover(CharacterPerceptionSnapshot perception)
    {
        ArgumentNullException.ThrowIfNull(perception);

        if (state.Contamination is not { } expectedContamination
            || !ReferenceEquals(
                presentation.LastPresentedContamination,
                expectedContamination)
            || state.Evidence is not { } evidence)
        {
            return false;
        }

        CharacterEvidenceViewSnapshot evidenceView =
            DiscoveryEvidenceProjection.ForCharacter(evidence, perception);
        if (!state.TryRecover(
                expectedContamination,
                evidence,
                evidenceView,
                source.Candidate)
            || state.Contamination is not { } currentContamination)
        {
            return false;
        }

        return presentation.TryPresent(expectedContamination, currentContamination);
    }
}
