using Allrpcs.Domain.Persistence;

namespace Allrpcs.Domain.Tests.Persistence;

public sealed class SaveSchemaVersionSupportPolicyTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public void SupportsEachKnownVersionIncludingADistinctValueEqualInstance(int value)
    {
        SaveSchemaVersion suppliedVersion = new(value);
        SaveSchemaVersion canonicalVersion = value switch
        {
            1 => SaveSchemaVersion.FirstSupported,
            2 => SaveSchemaVersion.CharacterMemory,
            3 => SaveSchemaVersion.FirstPatient,
            4 => SaveSchemaVersion.DialogueDelivery,
            _ => SaveSchemaVersion.Journal,
        };

        bool supported = SaveSchemaVersionSupportPolicy.CanLoad(suppliedVersion);

        Assert.True(supported);
        Assert.NotSame(canonicalVersion, suppliedVersion);
        Assert.Equal(canonicalVersion, suppliedVersion);
    }

    [Theory]
    [InlineData(27)]
    [InlineData(int.MaxValue)]
    public void RejectsStructurallyValidFutureVersionsWithoutChangingThem(int value)
    {
        SaveSchemaVersion version = new(value);
        int originalValue = version.Value;

        bool supported = SaveSchemaVersionSupportPolicy.CanLoad(version);

        Assert.False(supported);
        Assert.Equal(originalValue, version.Value);
    }

    [Fact]
    public void RejectsNullVersion()
    {
        Assert.Throws<ArgumentNullException>(
            () => SaveSchemaVersionSupportPolicy.CanLoad(null!));
    }

    [Fact]
    public void ExposesOnlyTheStaticLoadDecision()
    {
        Type policyType = typeof(SaveSchemaVersionSupportPolicy);
        System.Reflection.MethodInfo method = Assert.Single(
            policyType.GetMethods(
                System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Static
                | System.Reflection.BindingFlags.DeclaredOnly));

        Assert.Equal(nameof(SaveSchemaVersionSupportPolicy.CanLoad), method.Name);
        Assert.Equal(typeof(bool), method.ReturnType);
        Assert.Collection(
            method.GetParameters(),
            parameter => Assert.Equal(typeof(SaveSchemaVersion), parameter.ParameterType));
        Assert.Empty(policyType.GetProperties());
        Assert.Empty(policyType.GetEvents());
    }
}
