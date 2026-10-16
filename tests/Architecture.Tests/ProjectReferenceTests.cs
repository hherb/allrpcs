using System.IO.Enumeration;
using System.Xml.Linq;
using Xunit;

namespace Allrpcs.Architecture.Tests;

public sealed class ProjectReferenceTests
{
    private const string SolutionFileName = "Allrpcs.sln";

    public static TheoryData<string, string[]> AllowedReferences => new()
    {
        { "src/Domain/Allrpcs.Domain.csproj", [] },
        { "src/Application/Allrpcs.Application.csproj", ["src/Domain/Allrpcs.Domain.csproj"] },
        {
            "src/Infrastructure/Allrpcs.Infrastructure.csproj",
            ["src/Application/Allrpcs.Application.csproj"]
        },
        {
            "game/Allrpcs.Game.csproj",
            [
                "src/Application/Allrpcs.Application.csproj",
                "src/Infrastructure/Allrpcs.Infrastructure.csproj",
            ]
        },
        {
            "tests/Application.Tests/Allrpcs.Application.Tests.csproj",
            ["src/Application/Allrpcs.Application.csproj"]
        },
        {
            "tests/Infrastructure.Tests/Allrpcs.Infrastructure.Tests.csproj",
            ["src/Infrastructure/Allrpcs.Infrastructure.csproj"]
        },
        { "tests/Domain.Tests/Allrpcs.Domain.Tests.csproj", ["src/Domain/Allrpcs.Domain.csproj"] },
        {
            "tools/ScenarioValidator/Allrpcs.ScenarioValidator.csproj",
            ["src/Domain/Allrpcs.Domain.csproj"]
        },
        {
            "tools/SpeechServiceProbe/Allrpcs.SpeechServiceProbe.csproj",
            ["src/Infrastructure/Allrpcs.Infrastructure.csproj"]
        },
        { "tests/Architecture.Tests/Allrpcs.Architecture.Tests.csproj", [] },
    };

    [Theory]
    [MemberData(nameof(AllowedReferences))]
    public void ProjectReferencesMatchAllowedGraph(string projectPath, string[] allowedReferences)
    {
        string repositoryRoot = FindRepositoryRoot();
        string absoluteProjectPath = Path.Combine(repositoryRoot, projectPath);
        string projectDirectory = Path.GetDirectoryName(absoluteProjectPath)
            ?? throw new InvalidOperationException($"Project has no parent directory: {projectPath}");
        XDocument project = XDocument.Load(absoluteProjectPath);

        string[] actualReferences = project
            .Descendants("ProjectReference")
            .Select(reference => reference.Attribute("Include")?.Value)
            .Where(include => !string.IsNullOrWhiteSpace(include))
            .Select(include => NormalizeReference(repositoryRoot, projectDirectory, include!))
            .Order(StringComparer.Ordinal)
            .ToArray();
        string[] expectedReferences = allowedReferences
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(expectedReferences, actualReferences);
    }

    [Fact]
    public void EveryRepositoryProjectIsInAllowedGraph()
    {
        string repositoryRoot = FindRepositoryRoot();
        string[] excludedSegments = ["bin", "obj", "artifacts", ".git", ".godot"];

        string[] actualProjects = EnumerateProjects(repositoryRoot, excludedSegments)
            .Select(path => Path.GetRelativePath(repositoryRoot, path).Replace('\\', '/'))
            .Order(StringComparer.Ordinal)
            .ToArray();
        string[] allowedProjects = AllowedReferences
            .Select(row => (string)row[0])
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(allowedProjects, actualProjects);
    }

    [Fact]
    public void EveryAllowedProjectIsInSolution()
    {
        string repositoryRoot = FindRepositoryRoot();
        string solution = File.ReadAllText(Path.Combine(repositoryRoot, SolutionFileName))
            .Replace('\\', '/');

        string[] missing = AllowedReferences
            .Select(row => (string)row[0])
            .Where(project => !solution.Contains($"\"{project}\"", StringComparison.Ordinal))
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.Empty(missing);
    }

    private static FileSystemEnumerable<string> EnumerateProjects(string repositoryRoot, string[] excludedSegments)
    {
        EnumerationOptions options = new()
        {
            RecurseSubdirectories = true,
            IgnoreInaccessible = true,
            AttributesToSkip = FileAttributes.ReparsePoint,
        };

        return new FileSystemEnumerable<string>(
            repositoryRoot,
            (ref FileSystemEntry entry) => entry.ToFullPath(),
            options)
        {
            ShouldIncludePredicate = (ref FileSystemEntry entry) =>
                !entry.IsDirectory
                && entry.FileName.EndsWith(".csproj", StringComparison.Ordinal),
            ShouldRecursePredicate = (ref FileSystemEntry entry) =>
                !IsExcludedDirectory(entry.FileName, excludedSegments),
        };
    }

    private static bool IsExcludedDirectory(ReadOnlySpan<char> name, string[] excludedSegments)
    {
        foreach (string segment in excludedSegments)
        {
            if (name.SequenceEqual(segment))
            {
                return true;
            }
        }

        return false;
    }

    internal static string FindRepositoryRoot()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);

        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, SolutionFileName)))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException($"Could not find {SolutionFileName} above the test output directory.");
    }

    private static string NormalizeReference(
        string repositoryRoot,
        string projectDirectory,
        string reference)
    {
        string platformReference = reference
            .Replace('\\', Path.DirectorySeparatorChar)
            .Replace('/', Path.DirectorySeparatorChar);
        string absoluteReference = Path.GetFullPath(platformReference, projectDirectory);

        return Path.GetRelativePath(repositoryRoot, absoluteReference).Replace('\\', '/');
    }
}
