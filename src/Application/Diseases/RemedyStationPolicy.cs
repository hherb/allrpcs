using Allrpcs.Domain.Ingredients;
using Allrpcs.Domain.Persistence;

namespace Allrpcs.Application.Diseases;

/// <summary>What one press at an idle preparation station does.</summary>
public enum RemedyStationAction
{
    /// <summary>The Seeker uses the gathered herb they carry and starts the remedy.</summary>
    PrepareRemedy,

    /// <summary>The nearby Healer gives the Seeker the gathered herb it carries.</summary>
    ReceiveFromHealer,

    /// <summary>The Healer carries the gathered herb but is too far away to hand it over.</summary>
    WaitForHealer,

    /// <summary>Nothing gathered is carried, so the station's unidentified leaves are used.</summary>
    PrepareLookAlike,
}

/// <summary>Chooses the next action at an idle preparation station.</summary>
public static class RemedyStationPolicy
{
    /// <summary>
    /// Prefers the Seeker's own herb, then the nearby Healer's, and uses the look-alike leaves only
    /// when nobody carries the herb, so a distant Healer never turns a press into a wrong remedy.
    /// </summary>
    public static RemedyStationAction Choose(bool seekerCarriesHerb, bool healerCarriesHerb, bool healerNear) =>
        seekerCarriesHerb ? RemedyStationAction.PrepareRemedy
        : !healerCarriesHerb ? RemedyStationAction.PrepareLookAlike
        : healerNear ? RemedyStationAction.ReceiveFromHealer
        : RemedyStationAction.WaitForHealer;

    /// <summary>
    /// Reports whether durable history leaves the gathered herb in someone's hands: it was
    /// collected and no remedy batch or remedy dose has used it up yet.
    /// </summary>
    public static bool GatheredHerbIsHeld(
        bool collected,
        FirstPatientSaveState? state,
        IngredientPreparationRecipe remedy)
    {
        ArgumentNullException.ThrowIfNull(remedy);

        return collected
            && (state is null
                || !state.PreparedBatches.Batches.Select(batch => batch.Preparation)
                    .Concat(state.AdministrationExperiments.Records.Select(
                        record => record.Administration.Dosage.Preparation))
                    .Any(preparation => ReferenceEquals(preparation.Activity.Recipe, remedy)));
    }
}
