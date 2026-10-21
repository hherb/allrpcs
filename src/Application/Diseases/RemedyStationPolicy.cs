namespace Allrpcs.Application.Diseases;

/// <summary>What one press at an idle preparation station does.</summary>
public enum RemedyStationAction
{
    /// <summary>The Seeker uses the gathered herb they carry and starts the remedy.</summary>
    PrepareRemedy,

    /// <summary>The nearby Healer gives the Seeker the gathered herb it carries.</summary>
    ReceiveFromHealer,

    /// <summary>Nothing gathered is at hand, so the station's unidentified leaves are used.</summary>
    PrepareLookAlike,
}

/// <summary>Chooses the next action at an idle preparation station.</summary>
public static class RemedyStationPolicy
{
    /// <summary>Prefers the Seeker's own herb, then the Healer's, then the look-alike leaves.</summary>
    public static RemedyStationAction Choose(bool seekerCarriesHerb, bool nearbyHealerCarriesHerb) =>
        seekerCarriesHerb ? RemedyStationAction.PrepareRemedy
        : nearbyHealerCarriesHerb ? RemedyStationAction.ReceiveFromHealer
        : RemedyStationAction.PrepareLookAlike;
}
