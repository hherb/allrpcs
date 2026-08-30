using Allrpcs.Domain;

namespace Allrpcs.Domain.Tests;

public sealed class DomainAssemblyTests
{
    [Fact]
    public void LoadsWithoutGodot()
    {
        Assert.Equal("Allrpcs.Domain", DomainAssembly.Name);
    }
}
