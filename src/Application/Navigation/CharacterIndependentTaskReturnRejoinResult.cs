using Allrpcs.Domain.Characters;

namespace Allrpcs.Application.Navigation;

/// <summary>Contains one completed return advance and its resulting bounded party.</summary>
public sealed record CharacterIndependentTaskReturnRejoinResult
{
    /// <summary>Creates one immutable atomic return and rejoin result.</summary>
    public CharacterIndependentTaskReturnRejoinResult(
        CharacterIndependentTaskReturnTravelProgress returnProgress,
        CharacterPartySnapshot party)
    {
        ArgumentNullException.ThrowIfNull(returnProgress);
        ArgumentNullException.ThrowIfNull(party);

        ReturnProgress = returnProgress;
        Party = party;
    }

    /// <summary>Gets the fresh progress completed by the exact final return waypoint.</summary>
    public CharacterIndependentTaskReturnTravelProgress ReturnProgress { get; }

    /// <summary>Gets the fresh party containing the exact returned character.</summary>
    public CharacterPartySnapshot Party { get; }
}
