using System.Numerics;
using Allrpcs.Application.Navigation;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Application.Discovery;

/// <summary>Binds the exact noticed shrine inscription to the exact old-shrine scene.</summary>
public sealed class OpeningRegionLayeredInscriptionBinding
{
    private const string SeekerCharacterId = "seeker";
    private const string ShrineInscriptionObjectId = "shrine-inscription";

    /// <summary>Creates one inert binding without discovering or reinterpreting the inscription.</summary>
    public OpeningRegionLayeredInscriptionBinding(
        OpeningRegionOldShrineSceneBinding shrineSceneBinding,
        DiscoveryScenario discoveryScenario,
        WorldObject worldObject,
        WorldObjectVisibility initialVisibility,
        Vector2 inscriptionPosition)
    {
        ArgumentNullException.ThrowIfNull(shrineSceneBinding);
        ArgumentNullException.ThrowIfNull(discoveryScenario);
        ArgumentNullException.ThrowIfNull(worldObject);
        ArgumentNullException.ThrowIfNull(initialVisibility);

        if (discoveryScenario.ObserverCharacterId.Value != SeekerCharacterId
            || !ReferenceEquals(
                discoveryScenario.ObserverCharacterId,
                discoveryScenario.InitialPerception.CharacterId))
        {
            throw new ArgumentException(
                "The layered inscription requires the scenario's exact Seeker perception.",
                nameof(discoveryScenario));
        }

        if (worldObject.Identity.Id.Value != ShrineInscriptionObjectId
            || !discoveryScenario.WorldObjects.Entries.Any(candidate =>
                ReferenceEquals(candidate, worldObject)))
        {
            throw new ArgumentException(
                "The layered inscription requires the scenario's exact retained shrine object.",
                nameof(worldObject));
        }

        if (!ReferenceEquals(initialVisibility.ObjectId, worldObject.Identity.Id)
            || !ReferenceEquals(initialVisibility.VisibilityState, VisibilityState.Noticed)
            || !discoveryScenario.InitialPerception.Entries.Any(candidate =>
                ReferenceEquals(candidate, initialVisibility)))
        {
            throw new ArgumentException(
                "The layered inscription requires the exact initial noticed visibility.",
                nameof(initialVisibility));
        }

        NavigationWorldBounds bounds = shrineSceneBinding.OldShrineBounds;
        if (!float.IsFinite(inscriptionPosition.X)
            || !float.IsFinite(inscriptionPosition.Y)
            || inscriptionPosition.X < bounds.Left
            || inscriptionPosition.X > bounds.Right
            || inscriptionPosition.Y < bounds.Top
            || inscriptionPosition.Y > bounds.Bottom)
        {
            throw new ArgumentOutOfRangeException(
                nameof(inscriptionPosition),
                inscriptionPosition,
                "The layered inscription position must be finite and inside the shrine.");
        }

        ShrineSceneBinding = shrineSceneBinding;
        DiscoveryScenario = discoveryScenario;
        WorldObject = worldObject;
        InitialVisibility = initialVisibility;
        InscriptionPosition = inscriptionPosition;
    }

    /// <summary>Gets the exact old-shrine scene binding.</summary>
    public OpeningRegionOldShrineSceneBinding ShrineSceneBinding { get; }

    /// <summary>Gets the exact caller-owned discovery scenario.</summary>
    public DiscoveryScenario DiscoveryScenario { get; }

    /// <summary>Gets the scenario's exact retained shrine-inscription object.</summary>
    public WorldObject WorldObject { get; }

    /// <summary>Gets the scenario's exact initial noticed visibility.</summary>
    public WorldObjectVisibility InitialVisibility { get; }

    /// <summary>Gets the exact scene-authored inscription position.</summary>
    public Vector2 InscriptionPosition { get; }
}
