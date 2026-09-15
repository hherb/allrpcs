using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Projects exact world-object contamination state onto one scene marker.</summary>
public partial class CharacterObjectSabotageContaminationPresentationController : Node
{
    private CanvasItem marker = null!;

    /// <summary>Gets the exact world-object identifier represented by the marker, if initialized.</summary>
    public ObjectId? TargetObjectId { get; private set; }

    /// <summary>Gets the exact most recently presented contamination snapshot, if initialized.</summary>
    public WorldObjectContaminationSnapshot? LastPresentedContamination { get; private set; }

    /// <summary>Initializes one exact marker and immediately presents its contamination once.</summary>
    public bool TryInitialize(
        ObjectId targetObjectId,
        CanvasItem contaminationMarker,
        WorldObjectContaminationSnapshot contamination)
    {
        ArgumentNullException.ThrowIfNull(targetObjectId);
        ArgumentNullException.ThrowIfNull(contaminationMarker);
        ArgumentNullException.ThrowIfNull(contamination);

        if (TargetObjectId is not null || LastPresentedContamination is not null)
        {
            return false;
        }

        TargetObjectId = targetObjectId;
        marker = contaminationMarker;
        marker.Visible = contamination.IsContaminated(targetObjectId);
        LastPresentedContamination = contamination;
        return true;
    }

    /// <summary>Projects one fresh exact contamination snapshot from the last presented snapshot.</summary>
    public bool TryPresent(
        WorldObjectContaminationSnapshot expectedContamination,
        WorldObjectContaminationSnapshot currentContamination)
    {
        ArgumentNullException.ThrowIfNull(expectedContamination);
        ArgumentNullException.ThrowIfNull(currentContamination);

        if (TargetObjectId is not { } targetObjectId
            || !ReferenceEquals(LastPresentedContamination, expectedContamination)
            || ReferenceEquals(expectedContamination, currentContamination))
        {
            return false;
        }

        marker.Visible = currentContamination.IsContaminated(targetObjectId);
        LastPresentedContamination = currentContamination;
        return true;
    }
}
