using System.Numerics;

namespace Allrpcs.Domain.Actions;

/// <summary>The single authorized in-place communal mallet/block, absent from inventory and saves.</summary>
public sealed class HandToolTapSite
{
    /// <summary>The sole environmental handling key, distinct from discovery objects.</summary>
    public const string SiteKey = "village-edge-practice-block";
    /// <summary>Retains only the exact approved fixed declaration and finite local point.</summary>
    public HandToolTapSite(string key, Vector2 position)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        if (key != SiteKey || position != new Vector2(1700, 900))
        { throw new ArgumentException("Only the approved Village Edge practice site is authorized.", nameof(position)); }
        Key = key; Position = position;
    }
    /// <summary>The stable site key; no inventory identifier is manufactured.</summary>
    public string Key { get; }
    /// <summary>The retained projection-local handling point.</summary>
    public Vector2 Position { get; }
    /// <summary>The explicit owner-approved environmental material declaration.</summary>
    public string Description { get; } = "Communal wooden mallet and wooden practice block";
}
