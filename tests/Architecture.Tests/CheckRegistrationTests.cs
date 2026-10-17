using System.Text.RegularExpressions;
using Xunit;

namespace Allrpcs.Architecture.Tests;

/// <summary>
/// Every engine check must actually run somewhere: its ALLRPCS_*_CHECK gate and its check or
/// proof scene must appear on a non-comment line of scripts/smoke-godot.sh, or in
/// ManualCheckRegistrations.txt with a reason. scripts/smoke-godot.sh is the only parsed launch
/// source: quality.py runs many checks only inside frozen historical snapshots (their HEAD code
/// never runs), so the checks it does run at HEAD are allowlisted with that stage named, and
/// frozen scripts/quality.sh is historical evidence.
/// </summary>
public sealed partial class CheckRegistrationTests
{
    private const string AllowlistPath = "tests/Architecture.Tests/ManualCheckRegistrations.txt";

    private const string LaunchSourcePath = "scripts/smoke-godot.sh";

    private static readonly string[] ExcludedSegments = [".godot", "bin", "obj"];

    [Fact]
    public void EveryCheckGateIsLaunchedOrAllowlisted()
    {
        string repositoryRoot = ProjectReferenceTests.FindRepositoryRoot();
        string launchSources = ReadLaunchSources(repositoryRoot);
        HashSet<string> allowlisted = ReadAllowlist(repositoryRoot).Select(entry => entry.Key).ToHashSet(StringComparer.Ordinal);
        SortedSet<string> gates = FindGates(repositoryRoot);

        Assert.NotEmpty(gates);
        string[] unregistered = gates
            .Where(gate => !IsLaunched(launchSources, gate) && !allowlisted.Contains(gate))
            .ToArray();

        Assert.True(
            unregistered.Length == 0,
            $"Check gates neither launched by {LaunchSourcePath} nor listed in {AllowlistPath}:\n"
                + string.Join('\n', unregistered));
    }

    [Fact]
    public void EveryCheckSceneIsLaunchedOrAllowlisted()
    {
        string repositoryRoot = ProjectReferenceTests.FindRepositoryRoot();
        string launchSources = ReadLaunchSources(repositoryRoot);
        HashSet<string> allowlisted = ReadAllowlist(repositoryRoot).Select(entry => entry.Key).ToHashSet(StringComparer.Ordinal);
        SortedSet<string> scenes = FindScenes(repositoryRoot);

        Assert.NotEmpty(scenes);
        string[] unregistered = scenes
            .Where(scene => !IsLaunched(launchSources, scene) && !allowlisted.Contains(scene))
            .ToArray();

        Assert.True(
            unregistered.Length == 0,
            $"Check scenes neither launched by {LaunchSourcePath} nor listed in {AllowlistPath}:\n"
                + string.Join('\n', unregistered));
    }

    [Fact]
    public void AllowlistHasNoStaleEntries()
    {
        string repositoryRoot = ProjectReferenceTests.FindRepositoryRoot();
        string launchSources = ReadLaunchSources(repositoryRoot);
        SortedSet<string> known = FindGates(repositoryRoot);
        known.UnionWith(FindScenes(repositoryRoot));

        string[] stale = ReadAllowlist(repositoryRoot)
            .Select(entry => entry.Key)
            .Where(entry => !known.Contains(entry) || IsLaunched(launchSources, entry))
            .ToArray();

        Assert.True(
            stale.Length == 0,
            $"{AllowlistPath} entries that no longer exist in game/ or are now launched by {LaunchSourcePath} (remove them):\n"
                + string.Join('\n', stale));
    }

    [Fact]
    public void AllowlistEntriesAreWellFormed()
    {
        string repositoryRoot = ProjectReferenceTests.FindRepositoryRoot();

        string[] problems = ParseAllowlist(File.ReadAllLines(Path.Combine(repositoryRoot, AllowlistPath)))
            .Problems
            .ToArray();

        Assert.True(problems.Length == 0, $"Malformed {AllowlistPath} lines:\n" + string.Join('\n', problems));
    }

    [Theory]
    [InlineData("ALLRPCS_EXAMPLE_CHECK")]
    [InlineData("ALLRPCS_EXAMPLE_CHECK  #")]
    [InlineData("ALLRPCS_EXAMPLE_CHECK  #   ")]
    [InlineData("ALLRPCS_EXAMPLE_MODE  # not a gate")]
    [InlineData("res://Example.gd  # not a scene")]
    [InlineData("res://Example.tscn extra  # two tokens")]
    [InlineData("ALLRPCS_EXAMPLE_CHECK extra  # two tokens")]
    public void AllowlistParserRejectsEntriesWithoutGateSceneOrReason(string line)
    {
        AllowlistParse parse = ParseAllowlist([line]);

        Assert.Empty(parse.Entries);
        Assert.Single(parse.Problems);
    }

    [Fact]
    public void AllowlistParserAcceptsGatesScenesAndComments()
    {
        AllowlistParse parse = ParseAllowlist(
        [
            "# comment",
            "",
            "ALLRPCS_EXAMPLE_CHECK  # owner-operated manual check",
            "res://ExampleProof.tscn # superseded proof",
            "ALLRPCS_EXAMPLE_CHECK  # duplicate",
        ]);

        Assert.Equal(["ALLRPCS_EXAMPLE_CHECK", "res://ExampleProof.tscn"], parse.Entries.Keys.Order(StringComparer.Ordinal));
        Assert.Equal(["line 5: duplicate entry ALLRPCS_EXAMPLE_CHECK"], parse.Problems);
    }

    [Fact]
    public void LaunchSourceIgnoresCommentedOutLaunches()
    {
        string launchSources = LaunchLines(
        [
            "# run_check 30 ALLRPCS_COMMENTED_CHECK=1 godot --headless --path game",
            "    #run_check - godot --headless --path game res://CommentedProof.tscn --quit-after 30",
            "run_check 30 ALLRPCS_LAUNCHED_CHECK=1 godot --headless --path game  # trailing note",
            "run_check - godot --headless --path game res://LaunchedProof.tscn --quit-after 30",
        ]);

        Assert.False(IsLaunched(launchSources, "ALLRPCS_COMMENTED_CHECK"));
        Assert.False(IsLaunched(launchSources, "res://CommentedProof.tscn"));
        Assert.True(IsLaunched(launchSources, "ALLRPCS_LAUNCHED_CHECK"));
        Assert.True(IsLaunched(launchSources, "res://LaunchedProof.tscn"));
    }

    private static string ReadLaunchSources(string repositoryRoot)
    {
        return LaunchLines(File.ReadAllLines(Path.Combine(repositoryRoot, LaunchSourcePath)));
    }

    // Drops shell comment lines, so a commented-out launch does not count as registered.
    private static string LaunchLines(IEnumerable<string> lines)
    {
        return string.Join('\n', lines.Where(line => !CommentLinePattern().IsMatch(line)));
    }

    private static bool IsLaunched(string launchSources, string entry)
    {
        return Regex.IsMatch(launchSources, $@"(?<![A-Za-z0-9_]){Regex.Escape(entry)}(?![A-Za-z0-9_])");
    }

    private static SortedSet<string> FindGates(string repositoryRoot)
    {
        SortedSet<string> gates = new(StringComparer.Ordinal);
        string gameDirectory = Path.Combine(repositoryRoot, "game");

        foreach (string file in Directory.EnumerateFiles(gameDirectory, "*", SearchOption.AllDirectories))
        {
            string extension = Path.GetExtension(file);
            if ((extension != ".cs" && extension != ".gd") || IsExcluded(gameDirectory, file))
            {
                continue;
            }

            foreach (Match match in GatePattern().Matches(File.ReadAllText(file)))
            {
                gates.Add(match.Value);
            }
        }

        return gates;
    }

    private static SortedSet<string> FindScenes(string repositoryRoot)
    {
        string gameDirectory = Path.Combine(repositoryRoot, "game");

        return new SortedSet<string>(
            Directory.EnumerateFiles(gameDirectory, "*Check*.tscn", SearchOption.TopDirectoryOnly)
                .Concat(Directory.EnumerateFiles(gameDirectory, "*Proof*.tscn", SearchOption.TopDirectoryOnly))
                .Select(file => "res://" + Path.GetFileName(file)),
            StringComparer.Ordinal);
    }

    private static bool IsExcluded(string gameDirectory, string file)
    {
        return Path.GetRelativePath(gameDirectory, file)
            .Split(Path.DirectorySeparatorChar)
            .Any(segment => ExcludedSegments.Contains(segment, StringComparer.Ordinal));
    }

    private static IReadOnlyDictionary<string, string> ReadAllowlist(string repositoryRoot)
    {
        return ParseAllowlist(File.ReadAllLines(Path.Combine(repositoryRoot, AllowlistPath))).Entries;
    }

    private static AllowlistParse ParseAllowlist(IReadOnlyList<string> lines)
    {
        Dictionary<string, string> entries = new(StringComparer.Ordinal);
        List<string> problems = [];

        for (int index = 0; index < lines.Count; index++)
        {
            string line = lines[index].Trim();
            if (line.Length == 0 || line.StartsWith('#'))
            {
                continue;
            }

            int hash = line.IndexOf('#', StringComparison.Ordinal);
            string entry = (hash < 0 ? line : line[..hash]).Trim();
            string reason = hash < 0 ? string.Empty : line[(hash + 1)..].Trim();

            if (!GateEntryPattern().IsMatch(entry) && !ScenePattern().IsMatch(entry))
            {
                problems.Add($"line {index + 1}: not an ALLRPCS_*_CHECK gate or res://<Scene>.tscn: {line}");
            }
            else if (reason.Length == 0)
            {
                problems.Add($"line {index + 1}: missing \"# <reason>\": {line}");
            }
            else if (!entries.TryAdd(entry, reason))
            {
                problems.Add($"line {index + 1}: duplicate entry {entry}");
            }
        }

        return new AllowlistParse(entries, problems);
    }

    [GeneratedRegex(@"^\s*#")]
    private static partial Regex CommentLinePattern();

    [GeneratedRegex(@"\bALLRPCS_[A-Z0-9_]+_CHECK\b")]
    private static partial Regex GatePattern();

    [GeneratedRegex(@"^ALLRPCS_[A-Z0-9_]+_CHECK$")]
    private static partial Regex GateEntryPattern();

    [GeneratedRegex(@"^res://[A-Za-z0-9_]+\.tscn$")]
    private static partial Regex ScenePattern();

    private sealed record AllowlistParse(IReadOnlyDictionary<string, string> Entries, IReadOnlyList<string> Problems);
}
