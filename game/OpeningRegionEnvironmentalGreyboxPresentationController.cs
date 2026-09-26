using Allrpcs.Application.EnvironmentalStates;
using Allrpcs.Domain.EnvironmentalStates;
using Allrpcs.Domain.Locations;
using Godot;

namespace Allrpcs.Game;

/// <summary>Renders one exact old-shrine qualitative environment as canonical greybox text.</summary>
public partial class OpeningRegionEnvironmentalGreyboxPresentationController : Node
{
    private Label label = null!;
    private OpeningRegionEnvironmentalLocationPresentation? currentPresentation;

    /// <summary>Gets the exact scene-owned qualitative environmental source.</summary>
    public OpeningRegionEnvironmentalStateSource EnvironmentalSource { get; private set; } = null!;

    /// <summary>Gets the exact scene-owned old-shrine source.</summary>
    public OpeningRegionOldShrineSceneSource ShrineSource { get; private set; } = null!;

    /// <summary>Gets the exact rendered location presentation, or null while hidden.</summary>
    public OpeningRegionEnvironmentalLocationPresentation? CurrentPresentation =>
        currentPresentation is not null
        && ReferenceEquals(EnvironmentalSource.CurrentRegions, currentPresentation.Regions)
        && label.Visible
        && label.Text == currentPresentation.Text
            ? currentPresentation
            : null;

    /// <inheritdoc />
    public override void _Ready()
    {
        EnvironmentalSource = GetNode<OpeningRegionEnvironmentalStateSource>(
            "../OpeningRegionEnvironmentalStateSource");
        ShrineSource = GetNode<OpeningRegionOldShrineSceneSource>(
            "../OpeningRegionOldShrineSceneSource");
        label = ShrineSource.GetNode<Label>("EnvironmentalVariationStatus");
        label.Text = string.Empty;
        label.Hide();
        currentPresentation = null;
    }

    /// <summary>Renders one exact current old-shrine projection at most once.</summary>
    public bool TryPresent(
        OpeningRegionEnvironmentalRegionSet? currentRegions,
        Location? location,
        OpeningRegionEnvironmentalLocationPresentation? presentation)
    {
        if (!CanPresentAfterRegionReplacement(
                currentRegions,
                currentRegions,
                location,
                presentation))
        {
            return false;
        }

        currentPresentation = presentation;
        label.Text = presentation!.Text;
        label.Show();
        return true;
    }

    /// <summary>Checks one exact projection before or after replacing the current regions.</summary>
    public bool CanPresentAfterRegionReplacement(
        OpeningRegionEnvironmentalRegionSet? expectedCurrentRegions,
        OpeningRegionEnvironmentalRegionSet? replacementRegions,
        Location? location,
        OpeningRegionEnvironmentalLocationPresentation? presentation)
    {
        if (expectedCurrentRegions is null
            || replacementRegions is null
            || location is null
            || presentation is null
            || currentPresentation is not null
            || !ReferenceEquals(EnvironmentalSource.CurrentRegions, expectedCurrentRegions)
            || !ReferenceEquals(ShrineSource.Binding.OldShrineAnchor.Location, location)
            || !ReferenceEquals(presentation.Regions, replacementRegions)
            || !ReferenceEquals(presentation.Location, location)
            || !ReferenceEquals(
                OpeningRegionEnvironmentalRegionLookup.FindByLocation(replacementRegions, location),
                presentation.Region)
            || label.Visible
            || label.Text.Length != 0)
        {
            return false;
        }

        return true;
    }
}
