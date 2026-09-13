using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Projects exact world-object concealment state onto one scene body.</summary>
public partial class CharacterObjectSabotageConcealmentPresentationController : Node
{
    private CanvasItem body = null!;

    /// <summary>Gets the exact world-object identifier represented by the body, if initialized.</summary>
    public ObjectId? TargetObjectId { get; private set; }

    /// <summary>Gets the exact most recently presented concealment snapshot, if initialized.</summary>
    public WorldObjectConcealmentSnapshot? LastPresentedConcealment { get; private set; }

    /// <summary>Initializes one exact body and immediately presents its concealment once.</summary>
    public bool TryInitialize(
        ObjectId targetObjectId,
        CanvasItem targetBody,
        WorldObjectConcealmentSnapshot concealment)
    {
        ArgumentNullException.ThrowIfNull(targetObjectId);
        ArgumentNullException.ThrowIfNull(targetBody);
        ArgumentNullException.ThrowIfNull(concealment);

        if (TargetObjectId is not null || LastPresentedConcealment is not null)
        {
            return false;
        }

        TargetObjectId = targetObjectId;
        body = targetBody;
        body.Visible = !ContainsTarget(concealment, targetObjectId);
        LastPresentedConcealment = concealment;
        return true;
    }

    /// <summary>Projects one fresh exact concealment snapshot from the last presented snapshot.</summary>
    public bool TryPresent(
        WorldObjectConcealmentSnapshot expectedConcealment,
        WorldObjectConcealmentSnapshot currentConcealment)
    {
        ArgumentNullException.ThrowIfNull(expectedConcealment);
        ArgumentNullException.ThrowIfNull(currentConcealment);

        if (TargetObjectId is not { } targetObjectId
            || !ReferenceEquals(LastPresentedConcealment, expectedConcealment)
            || ReferenceEquals(expectedConcealment, currentConcealment))
        {
            return false;
        }

        body.Visible = !ContainsTarget(currentConcealment, targetObjectId);
        LastPresentedConcealment = currentConcealment;
        return true;
    }

    private static bool ContainsTarget(
        WorldObjectConcealmentSnapshot concealment,
        ObjectId targetObjectId) =>
        concealment.ConcealedObjectIds.Any(objectId => objectId == targetObjectId);
}
