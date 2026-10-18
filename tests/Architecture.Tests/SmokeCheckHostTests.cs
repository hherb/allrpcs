using System.Text.RegularExpressions;
using Xunit;

namespace Allrpcs.Architecture.Tests;

/// <summary>
/// The shipping Main scene holds no engine check: game/SmokeCheckHost.cs attaches only the checks a
/// launch requests. A hosted check whose gate the host does not list would never attach, so its
/// launch would hang until its deadline, or never run at all when it is not a smoke launch.
/// </summary>
public sealed partial class SmokeCheckHostTests
{
    [Fact]
    public void MainSceneReferencesNoHarnessResource()
    {
        string[] harness = ExternalResourcePaths(ReadGameFile("Main.tscn"))
            .Where(path => HarnessPattern().IsMatch(Path.GetFileName(path)))
            .ToArray();

        Assert.True(
            harness.Length == 0,
            "Main.tscn references harness resources (attach them through SmokeCheckHost):\n" + string.Join('\n', harness));
    }

    [Fact]
    public void EveryHostedCheckListsEachOfItsGates()
    {
        List<string> missing = [];
        foreach ((string name, string arguments) in Entries())
        {
            foreach (Match gate in GateLiteralPattern().Matches(ReadGameFile(name + ".cs")))
            {
                if (!arguments.Contains(gate.Value, StringComparison.Ordinal))
                {
                    missing.Add($"{name}: {gate.Value}");
                }
            }
        }

        Assert.True(missing.Count == 0, "SmokeCheckHost entries missing their check's gate:\n" + string.Join('\n', missing));
    }

    [Fact]
    public void EveryHostAnchorIsAMainChild()
    {
        HashSet<string> children = MainChildPattern().Matches(ReadGameFile("Main.tscn"))
            .Select(match => match.Groups["name"].Value)
            .ToHashSet(StringComparer.Ordinal);
        List<(string Name, string Arguments)> entries = Entries();

        Assert.NotEmpty(entries);
        Assert.Equal(entries.Count, entries.Select(entry => entry.Name).Distinct(StringComparer.Ordinal).Count());
        string[] unanchored = entries
            .Select(entry => (entry.Name, Anchor: AnchorPattern().Match(entry.Arguments).Groups["anchor"].Value))
            .Where(entry => entry.Anchor != "null" && !children.Contains(entry.Anchor.Trim('"')))
            .Select(entry => $"{entry.Name}: {entry.Anchor}")
            .ToArray();

        Assert.True(unanchored.Length == 0, "SmokeCheckHost anchors that are not Main children:\n" + string.Join('\n', unanchored));
    }

    private static List<(string Name, string Arguments)> Entries()
    {
        string host = ReadGameFile("SmokeCheckHost.cs");
        int start = host.IndexOf("Entries =", StringComparison.Ordinal);
        string entries = host[start..host.IndexOf("];", start, StringComparison.Ordinal)];

        return EntryPattern().Matches(entries)
            .Select(match => (match.Groups["check"].Success ? match.Groups["check"].Value : match.Groups["scene"].Value,
                match.Groups["arguments"].Value))
            .ToList();
    }

    private static IEnumerable<string> ExternalResourcePaths(string scene)
    {
        return ExternalResourcePattern().Matches(scene).Select(match => match.Groups["path"].Value);
    }

    private static string ReadGameFile(string name)
    {
        return File.ReadAllText(Path.Combine(ProjectReferenceTests.FindRepositoryRoot(), "game", name));
    }

    // Entry arguments are string literals and constants, so they contain no parentheses.
    [GeneratedRegex(@"(?:Check<(?<check>\w+)>\(|Scene\(""(?<scene>\w+)"",)(?<arguments>[^()]*)\)")]
    private static partial Regex EntryPattern();

    [GeneratedRegex(@"^\s*(?<anchor>null|""\w+"")")]
    private static partial Regex AnchorPattern();

    [GeneratedRegex(@"""ALLRPCS_[A-Z0-9_]+_CHECK""")]
    private static partial Regex GateLiteralPattern();

    [GeneratedRegex(@"\[ext_resource [^\]]*path=""(?<path>[^""]+)""")]
    private static partial Regex ExternalResourcePattern();

    [GeneratedRegex(@"\[node name=""(?<name>[^""]+)""[^\]]*parent=""\.""")]
    private static partial Regex MainChildPattern();

    [GeneratedRegex(@"Check|Proof|Probe")]
    private static partial Regex HarnessPattern();
}
