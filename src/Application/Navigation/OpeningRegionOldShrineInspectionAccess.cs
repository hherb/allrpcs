using Allrpcs.Application.Discovery;

namespace Allrpcs.Application.Navigation;

/// <summary>Retains one exact additive Scribe authorization to inspect the old shrine.</summary>
public sealed class OpeningRegionOldShrineInspectionAccess
{
    internal OpeningRegionOldShrineInspectionAccess(
        SeekerBackgroundRouteAccess routeAccess,
        SeekerBackgroundRouteVariation scribeVariation,
        OpeningRegionOldShrineSceneBinding shrineSceneBinding,
        OpeningRegionLayeredInscriptionBinding inscriptionBinding)
    {
        RouteAccess = routeAccess;
        ScribeVariation = scribeVariation;
        ShrineSceneBinding = shrineSceneBinding;
        InscriptionBinding = inscriptionBinding;
    }

    /// <summary>Gets the exact current selected background-route access.</summary>
    public SeekerBackgroundRouteAccess RouteAccess { get; }

    /// <summary>Gets the exact selected Scribe route variation.</summary>
    public SeekerBackgroundRouteVariation ScribeVariation { get; }

    /// <summary>Gets the exact existing old-shrine scene binding.</summary>
    public OpeningRegionOldShrineSceneBinding ShrineSceneBinding { get; }

    /// <summary>Gets the exact concealed layered-inscription provenance.</summary>
    public OpeningRegionLayeredInscriptionBinding InscriptionBinding { get; }
}
