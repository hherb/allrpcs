using System.Numerics;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Application.Discovery;

/// <summary>Retains one bounded Seeker access to the exact old-shrine inscription.</summary>
public sealed class OpeningRegionLayeredInscriptionAccess
{
    /// <summary>Gets the inclusive authored shrine-inscription access distance.</summary>
    public const float MaximumDistancePixels = 112.0f;

    /// <summary>Creates access without changing perception, discovery, or presentation state.</summary>
    public OpeningRegionLayeredInscriptionAccess(
        OpeningRegionLayeredInscriptionBinding inscriptionBinding,
        CharacterId seekerCharacterId,
        Vector2 seekerPosition)
    {
        ArgumentNullException.ThrowIfNull(inscriptionBinding);
        ArgumentNullException.ThrowIfNull(seekerCharacterId);

        if (!ReferenceEquals(
                seekerCharacterId,
                inscriptionBinding.DiscoveryScenario.ObserverCharacterId))
        {
            throw new ArgumentException(
                "Inscription access requires the scenario's exact Seeker identifier.",
                nameof(seekerCharacterId));
        }

        if (!IsInside(inscriptionBinding, seekerPosition)
            || Vector2.Distance(seekerPosition, inscriptionBinding.InscriptionPosition)
                > MaximumDistancePixels)
        {
            throw new ArgumentException(
                "The Seeker and inscription must be within the bounded shrine access distance.",
                nameof(seekerPosition));
        }

        InscriptionBinding = inscriptionBinding;
        SeekerCharacterId = seekerCharacterId;
        SeekerPosition = seekerPosition;
        InscriptionPosition = inscriptionBinding.InscriptionPosition;
    }

    /// <summary>Gets the exact inscription binding made accessible.</summary>
    public OpeningRegionLayeredInscriptionBinding InscriptionBinding { get; }

    /// <summary>Gets the exact scenario-owned Seeker identifier.</summary>
    public CharacterId SeekerCharacterId { get; }

    /// <summary>Gets the accepted Seeker position.</summary>
    public Vector2 SeekerPosition { get; }

    /// <summary>Gets the accepted inscription-marker position.</summary>
    public Vector2 InscriptionPosition { get; }

    private static bool IsInside(
        OpeningRegionLayeredInscriptionBinding binding,
        Vector2 position)
    {
        Allrpcs.Application.Navigation.NavigationWorldBounds bounds =
            binding.ShrineSceneBinding.OldShrineBounds;
        return float.IsFinite(position.X)
            && float.IsFinite(position.Y)
            && position.X >= bounds.Left
            && position.X <= bounds.Right
            && position.Y >= bounds.Top
            && position.Y <= bounds.Bottom;
    }
}
