using Allrpcs.Domain.Actions;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Binds the newly authorized material to one exact existing snapshot and live marker.</summary>
internal sealed class OpeningRegionCaskCoverMaterialBinding
{
    /// <summary>Creates the sole cask binding after all retained scene owners are ready.</summary>
    internal OpeningRegionCaskCoverMaterialBinding(WorldObjectSnapshot snapshot, GreyboxInteractionTarget marker)
    {
        Snapshot = snapshot; Marker = marker;
        WorldObject target = snapshot.Entries.Single(o => o.Identity.Id.Value == CaskCoverHandlingMaterial.ObjectIdText);
        if (marker.ObjectId != target.Identity.Id) { throw new InvalidOperationException("Mismatched cover marker."); }
        Material = new(target); Original = marker.GetNode<Polygon2D>("Body");
        Substitute = marker.GetNode<Polygon2D>("SubstituteBody");
        Scale = marker.Scale; Rotation = marker.Rotation;
        ParentTransform = marker.GetParent<Node2D>().GlobalTransform;
    }
    /// <summary>The exact retained world snapshot.</summary>
    internal WorldObjectSnapshot Snapshot { get; }
    /// <summary>The specifically authorized material and exact target.</summary>
    internal CaskCoverHandlingMaterial Material { get; }
    /// <summary>The exact ordinary cask marker.</summary>
    internal GreyboxInteractionTarget Marker { get; }
    /// <summary>The existing original body; no material geometry is installed.</summary>
    internal Polygon2D Original { get; }
    /// <summary>The existing alternate body, which never inherits this permission.</summary>
    internal Polygon2D Substitute { get; }
    /// <summary>The retained target scale.</summary>
    internal Vector2 Scale { get; }
    /// <summary>The retained target rotation.</summary>
    internal float Rotation { get; }
    /// <summary>The retained parent transform; target translation follows existing move presentation.</summary>
    internal Transform2D ParentTransform { get; }
}
