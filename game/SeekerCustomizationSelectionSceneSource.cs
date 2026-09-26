using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Owns representative Seeker options and one exact one-shot selection state.</summary>
public partial class SeekerCustomizationSelectionSceneSource : Node
{
    /// <summary>Gets the exact representative options authored at ordinary startup.</summary>
    public SeekerCustomizationOptions Options { get; private set; } = null!;

    /// <summary>Gets the exact current empty or selected state.</summary>
    public SeekerCustomizationSelectionState CurrentState { get; private set; } = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        Options = RepresentativeSeekerCustomizationOptionsSource.Create();
        CurrentState = SeekerCustomizationSelectionStateTransition.Initialize(Options);
    }

    /// <summary>Installs one exact transition result through current-state compare-and-swap.</summary>
    public bool TryInstallSelection(
        SeekerCustomizationSelectionState? expectedCurrent,
        SeekerCustomizationSelectionState? selectedState)
    {
        if (!ReferenceEquals(CurrentState, expectedCurrent)
            || expectedCurrent.SelectedProfile is not null
            || selectedState?.SelectedProfile is null
            || !ReferenceEquals(expectedCurrent.Options, Options)
            || !ReferenceEquals(selectedState.Options, Options)
            || !ReferenceEquals(selectedState.PreviousState, expectedCurrent))
        {
            return false;
        }

        CurrentState = selectedState;
        return true;
    }
}
