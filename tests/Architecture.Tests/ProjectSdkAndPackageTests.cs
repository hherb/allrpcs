using System.Xml.Linq;
using Xunit;

namespace Allrpcs.Architecture.Tests;

public sealed class ProjectSdkAndPackageTests
{
    private static readonly string[] ForbiddenElements =
    [
        "PackageReference",
        "FrameworkReference",
    ];

    public static TheoryData<string> RuleLayerProjects => new()
    {
        "src/Domain/Allrpcs.Domain.csproj",
        "src/Application/Allrpcs.Application.csproj",
        "src/Infrastructure/Allrpcs.Infrastructure.csproj",
    };

    [Theory]
    [MemberData(nameof(RuleLayerProjects))]
    public void RuleLayerProjectsUsePlainSdk(string projectPath)
    {
        XDocument project = XDocument.Load(
            Path.Combine(ProjectReferenceTests.FindRepositoryRoot(), projectPath));

        Assert.Equal("Microsoft.NET.Sdk", project.Root?.Attribute("Sdk")?.Value);
        Assert.Empty(project.Descendants().Where(element => ForbiddenElements.Contains(element.Name.LocalName)));
    }

    [Fact]
    public void DirectoryBuildPropsAddsNoReferences()
    {
        XDocument props = XDocument.Load(
            Path.Combine(ProjectReferenceTests.FindRepositoryRoot(), "Directory.Build.props"));
        string[] forbidden = ["ProjectReference", .. ForbiddenElements];

        Assert.Empty(props.Descendants().Where(element => forbidden.Contains(element.Name.LocalName)));
    }
}
