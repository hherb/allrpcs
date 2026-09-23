using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Godot;

namespace Allrpcs.Game;

/// <summary>Presents each exact completed model-selected substitution result at most once.</summary>
public partial class CharacterObjectSabotageSubstitutionPresentationCoordinator : Node
{
    private CharacterObjectSabotageSubstitutionConsiderationResult? observedResult;
    private CharacterObjectSabotageSubstitutionPresentationController presentation = null!;
    private CharacterObjectSabotageSubstitutionController state = null!;
    private CharacterObjectSabotageSubstitutionTaskController task = null!;

    internal CharacterObjectSabotageSubstitutionConsiderationResult? AcknowledgedResult
    {
        get;
        private set;
    }

    internal CharacterObjectSabotageSubstitutionSnapshot? AcknowledgedSubstitutions
    {
        get;
        private set;
    }

    internal DiscoveryEvidenceSnapshot? AcknowledgedEvidence { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        state = GetNode<CharacterObjectSabotageSubstitutionController>(
            "../CharacterObjectSabotageSubstitutionController");
        task = GetNode<CharacterObjectSabotageSubstitutionTaskController>(
            "../CharacterObjectSabotageSubstitutionTaskController");
        presentation = GetNode<CharacterObjectSabotageSubstitutionPresentationController>(
            "../CharacterObjectSabotageSubstitutionPresentationController");
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        _ = TryPresentCompletedResult();
    }

    /// <summary>Observes one fresh exact terminal selection and presents its state transition.</summary>
    public bool TryPresentCompletedResult()
    {
        CharacterObjectSabotageSubstitutionConsiderationResult? result = task.Result;
        if (task.Status != CharacterObjectSabotageSubstitutionTaskStatus.Completed
            || result is null
            || ReferenceEquals(observedResult, result))
        {
            return false;
        }

        observedResult = result;
        if (result.SelectedCandidate is null
            || state.Substitutions is not { } currentSubstitutions)
        {
            return false;
        }

        CharacterObjectSabotageSubstitutionSnapshot expectedSubstitutions =
            result.Request.Substitutions;
        return presentation.TryPresent(expectedSubstitutions, currentSubstitutions);
    }

    /// <summary>Explicitly resets only the exact completed result already observed in current state.</summary>
    public bool TryAcknowledgeCompletedResult()
    {
        CharacterObjectSabotageSubstitutionConsiderationResult? result = task.Result;
        if (task.Status != CharacterObjectSabotageSubstitutionTaskStatus.Completed
            || result is null
            || !ReferenceEquals(observedResult, result))
        {
            return false;
        }

        bool isCurrent = result.SelectedCandidate is null
            ? ReferenceEquals(state.Substitutions, result.Request.Substitutions)
                && ReferenceEquals(state.Evidence, result.Request.Evidence)
            : ReferenceEquals(
                state.Substitutions,
                presentation.LastPresentedSubstitutions);
        if (!isCurrent || !task.TryReset())
        {
            return false;
        }

        AcknowledgedResult = result;
        AcknowledgedSubstitutions = state.Substitutions;
        AcknowledgedEvidence = state.Evidence;
        observedResult = null;
        return true;
    }
}
