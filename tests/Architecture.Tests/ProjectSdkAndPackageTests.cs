using System.Xml.Linq;
using Xunit;

namespace Allrpcs.Architecture.Tests;

public sealed class ProjectSdkAndPackageTests
{
    private static readonly string[] ForbiddenElements =
    [
        "PackageReference",
        "FrameworkReference",
        "GlobalPackageReference",
    ];

    // Assembly references (e.g. a HintPath to GodotSharp.dll) and extra SDKs, whether as an
    // <Sdk> child or an <Import Sdk="..."> element, would bring an engine into the rule layer.
    private static readonly string[] RuleLayerOnlyForbiddenElements =
    [
        "Reference",
        "Sdk",
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
        Assert.Empty(RuleLayerViolations(project));
    }

    [Theory]
    [InlineData("<PackageReference Include=\"Example\" Version=\"1.0.0\" />")]
    [InlineData("<FrameworkReference Include=\"Microsoft.AspNetCore.App\" />")]
    [InlineData("<GlobalPackageReference Include=\"Example\" Version=\"1.0.0\" />")]
    [InlineData("<Reference Include=\"GodotSharp\"><HintPath>GodotSharp.dll</HintPath></Reference>")]
    [InlineData("<Sdk Name=\"Godot.NET.Sdk\" Version=\"4.6.3\" />")]
    public void RuleLayerCheckRejectsReferencesAndExtraSdks(string item)
    {
        XDocument project = XDocument.Parse(
            $"<Project Sdk=\"Microsoft.NET.Sdk\"><ItemGroup>{item}</ItemGroup></Project>");

        Assert.Single(RuleLayerViolations(project));
    }

    [Fact]
    public void RuleLayerCheckRejectsSdkImports()
    {
        XDocument project = XDocument.Parse(
            "<Project Sdk=\"Microsoft.NET.Sdk\">"
                + "<Import Project=\"Sdk.targets\" Sdk=\"Godot.NET.Sdk\" />"
                + "<Import Project=\"Local.targets\" />"
                + "</Project>");

        Assert.Equal(["Import"], RuleLayerViolations(project).Select(element => element.Name.LocalName));
    }

    [Theory]
    [InlineData("Directory.Build.props")]
    [InlineData("Directory.Build.targets")]
    [InlineData("Directory.Packages.props")]
    public void SharedBuildFilesAddNoReferences(string fileName)
    {
        string path = Path.Combine(ProjectReferenceTests.FindRepositoryRoot(), fileName);
        if (!File.Exists(path))
        {
            Assert.NotEqual("Directory.Build.props", fileName);
            return;
        }

        string[] forbidden = ["ProjectReference", .. ForbiddenElements];

        Assert.Empty(XDocument.Load(path).Descendants()
            .Where(element => forbidden.Contains(element.Name.LocalName)));
    }

    private static IEnumerable<XElement> RuleLayerViolations(XDocument project)
    {
        return project.Descendants().Where(element =>
            ForbiddenElements.Contains(element.Name.LocalName)
            || RuleLayerOnlyForbiddenElements.Contains(element.Name.LocalName)
            || (element.Name.LocalName == "Import" && element.Attribute("Sdk") is not null));
    }
}
