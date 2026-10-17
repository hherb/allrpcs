using Allrpcs.Domain;

namespace Allrpcs.Domain.Tests;

public sealed class DomainAssemblyTests
{
    [Fact]
    public void LoadsWithoutGodot()
    {
        Assert.Equal("Allrpcs.Domain", DomainAssembly.Name);
    }

    // Secondary guard: Roslyn drops unused references from metadata, so the csproj-level
    // SDK/package test in Architecture.Tests is the primary enforcement.
    [Fact]
    public void ReferencesNoGodotAssembly()
    {
        string[] godotReferences = typeof(DomainAssembly).Assembly
            .GetReferencedAssemblies()
            .Select(name => name.Name ?? string.Empty)
            .Where(name => name.StartsWith("Godot", StringComparison.Ordinal))
            .ToArray();

        Assert.Empty(godotReferences);
    }
}
