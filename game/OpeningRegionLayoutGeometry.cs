using Godot;

namespace Allrpcs.Game;

/// <summary>Names the explicitly approved opening-region proposal geometry.</summary>
internal static class OpeningRegionLayoutGeometry
{
    internal const string ProposalSha256 =
        "a5ebd5e23b5f41a8958477c953ba2763afffcb636f3bb6f69a969c0c1d57f8f1";
    internal const float WorldWidthPixels = 3200.0f;
    internal const float WorldHeightPixels = 1800.0f;
    internal const float PathWidthPixels = 96.0f;
    internal const float RouteCellSizePixels = 8.0f;

    internal static IReadOnlyDictionary<string, Vector2> Anchors { get; } =
        new Dictionary<string, Vector2>(StringComparer.Ordinal)
        {
            ["sickhouse"] = new(2640.0f, 1450.0f),
            ["healer-garden"] = new(2300.0f, 1300.0f),
            ["village-square"] = new(2200.0f, 900.0f),
            ["village-edge"] = new(1700.0f, 900.0f),
            ["stream"] = new(1800.0f, 500.0f),
            ["abandoned-laundry"] = new(1500.0f, 220.0f),
            ["woodland-clearing"] = new(1000.0f, 350.0f),
            ["old-shrine"] = new(380.0f, 300.0f),
            ["southern-road"] = new(1500.0f, 1400.0f),
            ["abandoned-wagon"] = new(1900.0f, 1640.0f),
            ["closed-deeper-forest-path"] = new(650.0f, 150.0f),
        };

    internal static IReadOnlyList<(string First, string Second)> Connections { get; } =
    [
        ("village-square", "sickhouse"),
        ("village-square", "healer-garden"),
        ("village-square", "village-edge"),
        ("village-square", "southern-road"),
        ("village-edge", "stream"),
        ("stream", "abandoned-laundry"),
        ("village-edge", "woodland-clearing"),
        ("abandoned-laundry", "woodland-clearing"),
        ("woodland-clearing", "old-shrine"),
        ("old-shrine", "southern-road"),
        ("southern-road", "abandoned-wagon"),
        ("woodland-clearing", "closed-deeper-forest-path"),
    ];
}
