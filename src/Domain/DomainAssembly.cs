namespace Allrpcs.Domain;

/// <summary>
/// Identifies the engine-independent domain assembly for smoke tests and composition roots.
/// </summary>
public static class DomainAssembly
{
    /// <summary>
    /// Gets the stable assembly name.
    /// </summary>
    public static string Name => typeof(DomainAssembly).Assembly.GetName().Name
        ?? throw new InvalidOperationException("The domain assembly has no name.");
}
