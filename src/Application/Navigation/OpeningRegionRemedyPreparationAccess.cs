using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.Ingredients;

namespace Allrpcs.Application.Navigation;

/// <summary>Retains exact additive Apothecary access to the existing preparation station.</summary>
public sealed class OpeningRegionRemedyPreparationAccess
{
    internal OpeningRegionRemedyPreparationAccess(
        SeekerBackgroundRouteAccess routeAccess,
        SeekerBackgroundRouteVariation apothecaryVariation,
        OpeningRegionRemedyPreparationSceneBinding sceneBinding)
    {
        RouteAccess = routeAccess;
        ApothecaryVariation = apothecaryVariation;
        SceneBinding = sceneBinding;
    }

    /// <summary>Gets the exact current selected background-route access.</summary>
    public SeekerBackgroundRouteAccess RouteAccess { get; }

    /// <summary>Gets the exact selected Apothecary's Assistant route variation.</summary>
    public SeekerBackgroundRouteVariation ApothecaryVariation { get; }

    /// <summary>Gets the exact existing remedy-preparation scene binding.</summary>
    public OpeningRegionRemedyPreparationSceneBinding SceneBinding { get; }

    /// <summary>Gets the exact first-patient treatment context without changing it.</summary>
    public FirstPatientTreatmentContext TreatmentContext => SceneBinding.TreatmentContext;

    /// <summary>Gets the exact existing recipe represented by the station.</summary>
    public IngredientPreparationRecipe Recipe => SceneBinding.Recipe;
}
