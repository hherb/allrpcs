using Allrpcs.Application.Navigation;
using Allrpcs.Domain.Ingredients;
using Godot;

namespace Allrpcs.Game;

/// <summary>Immutable presentation token; only the producing coordinator's retained identity is admissible.</summary>
internal sealed record HeatingPreparationCommit(
    OpeningRegionRemedyPreparationInputCoordinator Owner,
    OpeningRegionRemedyPreparationSceneBinding Binding,
    IngredientPreparationActivity Before,
    IngredientPreparationActivity After,
    IngredientPreparationStep Step,
    Vector2 ActualPosition);
