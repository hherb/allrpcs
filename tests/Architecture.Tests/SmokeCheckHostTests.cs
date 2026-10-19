using System.Text.RegularExpressions;
using Xunit;

namespace Allrpcs.Architecture.Tests;

/// <summary>
/// The shipping Main scene holds no engine check: game/SmokeCheckHost.cs attaches only the checks a
/// launch requests. A hosted check whose gate the host does not list never attaches: a smoke launch
/// then runs plain Main until its run_check deadline fails, and a manual or quality.py launch runs
/// without it. Gates are found as ALLRPCS_*_CHECK (or live _ACTION/_WAIT) literals and
/// Type.SmokeCheckEnvironmentVariable constants in every partial file of the check.
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
        Dictionary<string, string> sources = GameClassSources();
        List<string> missing = [];
        foreach ((string name, string arguments, _) in Entries())
        {
            string source = sources.GetValueOrDefault(name) ?? throw new InvalidOperationException($"No game source for {name}.");
            IEnumerable<string> gates = GateLiteralPattern().Matches(source).Select(match => match.Value)
                .Concat(GateConstantPattern().Matches(source).Select(match => match.Value));
            missing.AddRange(gates
                .Where(gate => !arguments.Contains(gate, StringComparison.Ordinal))
                .Select(gate => $"{name}: {gate}"));
        }

        Assert.True(missing.Count == 0, "SmokeCheckHost entries missing their check's gate:\n" + string.Join('\n', missing));
    }

    [Fact]
    public void EveryGatedGameClassIsHostedOrInAScene()
    {
        HashSet<string> hosted = Entries().Select(entry => entry.Name).ToHashSet(StringComparer.Ordinal);
        string scenes = string.Join('\n', Directory.EnumerateFiles(GameDirectory(), "*.tscn").Select(File.ReadAllText));
        string[] unhosted = GameClassSources()
            .Where(source => GateLiteralPattern().IsMatch(source.Value) || GateConstantPattern().IsMatch(source.Value))
            .Select(source => source.Key)
            .Where(name => !hosted.Contains(name) && !scenes.Contains($"res://{name}.cs\"", StringComparison.Ordinal))
            .ToArray();

        Assert.True(
            unhosted.Length == 0,
            "Gated game classes neither in a scene nor in SmokeCheckHost (add a host entry):\n" + string.Join('\n', unhosted));
    }

    [Fact]
    public void EveryHostAnchorIsAMainChildInMainOrder()
    {
        List<string> children = MainChildPattern().Matches(ReadGameFile("Main.tscn"))
            .Select(match => match.Groups["name"].Value)
            .ToList();
        List<(string Name, string Arguments, bool IsScene)> entries = Entries();

        Assert.NotEmpty(entries);
        Assert.Equal(entries.Count, entries.Select(entry => entry.Name).Distinct(StringComparer.Ordinal).Count());
        List<string> problems = [];
        int previous = -1;
        foreach ((string name, string arguments, _) in entries)
        {
            string anchor = AnchorPattern().Match(arguments).Groups["anchor"].Value;
            int index = anchor == "null" ? -1 : children.IndexOf(anchor.Trim('"'));
            if (anchor != "null" && index < 0)
            {
                problems.Add($"{name}: anchor {anchor} is not a Main child");
                continue;
            }

            // Ready order is entry order, so it matches index order only while anchors never go back.
            if (index < previous)
            {
                problems.Add($"{name}: anchor {anchor} precedes the previous entry's anchor");
            }

            previous = Math.Max(previous, index);
        }

        Assert.True(problems.Count == 0, "SmokeCheckHost anchors out of place:\n" + string.Join('\n', problems));
    }

    [Fact]
    public void EveryHostedCheckSceneExists()
    {
        string[] missing = Entries()
            .Where(entry => entry.IsScene && !File.Exists(Path.Combine(GameDirectory(), entry.Name + ".tscn")))
            .Select(entry => entry.Name)
            .ToArray();

        Assert.True(missing.Length == 0, "SmokeCheckHost scene entries without a scene:\n" + string.Join('\n', missing));
    }

    private static List<(string Name, string Arguments, bool IsScene)> Entries()
    {
        string host = ReadGameFile("SmokeCheckHost.cs");
        int start = host.IndexOf("Entries =", StringComparison.Ordinal);
        string entries = host[start..host.IndexOf("];", start, StringComparison.Ordinal)];
        List<(string Name, string Arguments, bool IsScene)> parsed = EntryPattern().Matches(entries)
            .Select(match => (
                match.Groups["check"].Success ? match.Groups["check"].Value : match.Groups["scene"].Value,
                match.Groups["arguments"].Value,
                !match.Groups["check"].Success))
            .ToList();

        // An entry the pattern cannot parse would otherwise escape every test silently.
        Assert.Equal(EntryStartPattern().Matches(entries).Count, parsed.Count);
        return parsed;
    }

    /// <summary>Each game class's text across all of its partial files (game/ is flat).</summary>
    private static Dictionary<string, string> GameClassSources()
    {
        return Directory.EnumerateFiles(GameDirectory(), "*.cs")
            .Select(File.ReadAllText)
            .Select(text => (Name: ClassPattern().Match(text).Groups["name"].Value, Text: text))
            .Where(source => source.Name.Length > 0)
            .GroupBy(source => source.Name, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => string.Join('\n', group.Select(source => source.Text)), StringComparer.Ordinal);
    }

    private static IEnumerable<string> ExternalResourcePaths(string scene)
    {
        return ExternalResourcePattern().Matches(scene).Select(match => match.Groups["path"].Value);
    }

    private static string GameDirectory()
    {
        return Path.Combine(ProjectReferenceTests.FindRepositoryRoot(), "game");
    }

    private static string ReadGameFile(string name)
    {
        return File.ReadAllText(Path.Combine(GameDirectory(), name));
    }

    // Entry arguments are string literals and constants, so they contain no parentheses.
    [GeneratedRegex(@"(?:Check<(?<check>\w+)>\(|Scene\(""(?<scene>\w+)"",)(?<arguments>[^()]*)\)")]
    private static partial Regex EntryPattern();

    [GeneratedRegex(@"\b(?:Check<|Scene\()")]
    private static partial Regex EntryStartPattern();

    [GeneratedRegex(@"^\s*(?<anchor>null|""\w+"")")]
    private static partial Regex AnchorPattern();

    [GeneratedRegex(@"""ALLRPCS_[A-Z0-9_]+_(?:CHECK|ACTION|WAIT)""")]
    private static partial Regex GateLiteralPattern();

    [GeneratedRegex(@"\b\w+\.SmokeCheckEnvironmentVariable\b")]
    private static partial Regex GateConstantPattern();

    [GeneratedRegex(@"\bpartial class (?<name>\w+)")]
    private static partial Regex ClassPattern();

    [GeneratedRegex(@"\[ext_resource [^\]]*path=""(?<path>[^""]+)""")]
    private static partial Regex ExternalResourcePattern();

    [GeneratedRegex(@"\[node name=""(?<name>[^""]+)""[^\]]*parent=""\.""")]
    private static partial Regex MainChildPattern();

    [GeneratedRegex(@"Check|Proof|Probe")]
    private static partial Regex HarnessPattern();
}
