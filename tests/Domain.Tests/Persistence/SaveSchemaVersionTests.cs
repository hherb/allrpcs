using Allrpcs.Domain.Persistence;

namespace Allrpcs.Domain.Tests.Persistence;

public sealed class SaveSchemaVersionTests
{
    [Fact]
    public void DefinesAllCanonicalSchemaVersions()
    {
        Assert.Equal(1, SaveSchemaVersion.FirstSupported.Value);
        Assert.Equal(2, SaveSchemaVersion.CharacterMemory.Value);
        Assert.Equal(3, SaveSchemaVersion.FirstPatient.Value);
        Assert.Equal(4, SaveSchemaVersion.DialogueDelivery.Value);
        Assert.Equal(5, SaveSchemaVersion.Journal.Value);
        Assert.Equal(6, SaveSchemaVersion.Map.Value);
        Assert.Same(SaveSchemaVersion.FirstSupported, SaveSchemaVersion.FirstSupported);
        Assert.Same(SaveSchemaVersion.CharacterMemory, SaveSchemaVersion.CharacterMemory);
        Assert.Same(SaveSchemaVersion.FirstPatient, SaveSchemaVersion.FirstPatient);
        Assert.Same(SaveSchemaVersion.DialogueDelivery, SaveSchemaVersion.DialogueDelivery);
        Assert.Same(SaveSchemaVersion.Journal, SaveSchemaVersion.Journal);
        Assert.Same(SaveSchemaVersion.Map, SaveSchemaVersion.Map);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(27)]
    [InlineData(int.MaxValue)]
    public void ConstructsPositiveVersions(int value)
    {
        SaveSchemaVersion version = new(value);

        Assert.Equal(value, version.Value);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public void RejectsNonPositiveVersions(int value)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new SaveSchemaVersion(value));
    }

    [Theory]
    [InlineData("1", 1)]
    [InlineData("27", 27)]
    [InlineData("2147483647", int.MaxValue)]
    public void ParsesCanonicalVersionTextWithoutChangingTheInput(string text, int expectedValue)
    {
        string originalText = text;

        SaveSchemaVersion version = SaveSchemaVersion.Parse(text);

        Assert.Equal(expectedValue, version.Value);
        Assert.Equal(originalText, text);
        Assert.Equal(text, version.ToString());
    }

    [Theory]
    [InlineData(1)]
    [InlineData(27)]
    [InlineData(int.MaxValue)]
    public void CanonicalTextRoundTrips(int value)
    {
        SaveSchemaVersion version = new(value);

        Assert.Equal(version, SaveSchemaVersion.Parse(version.ToString()));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("01")]
    [InlineData("+1")]
    [InlineData("-1")]
    [InlineData("0")]
    [InlineData("1 ")]
    [InlineData(" 1")]
    [InlineData("1.0")]
    [InlineData("one")]
    [InlineData("2147483648")]
    public void RejectsNonCanonicalVersionText(string text)
    {
        Assert.Throws<ArgumentException>(() => SaveSchemaVersion.Parse(text));
    }

    [Fact]
    public void RejectsNullVersionText()
    {
        Assert.Throws<ArgumentNullException>(() => SaveSchemaVersion.Parse(null!));
    }

    [Fact]
    public void EqualityUsesTheVersionValue()
    {
        SaveSchemaVersion first = new(1);
        SaveSchemaVersion same = new(1);
        SaveSchemaVersion different = new(2);

        Assert.Equal(first, same);
        Assert.True(first == same);
        Assert.NotEqual(first, different);
    }

    [Fact]
    public void ExposesOnlyTheImmutableVersionSurface()
    {
        Type versionType = typeof(SaveSchemaVersion);
        System.Reflection.PropertyInfo[] properties = versionType.GetProperties();
        System.Reflection.MethodInfo[] declaredMethods = versionType
            .GetMethods(
                System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Static
                | System.Reflection.BindingFlags.DeclaredOnly)
            .Where(method => !method.IsSpecialName)
            .ToArray();

        Assert.Collection(
            properties.OrderBy(property => property.Name, StringComparer.Ordinal),
            property =>
            {
                Assert.Equal("CharacterMemory", property.Name);
                Assert.Equal(typeof(SaveSchemaVersion), property.PropertyType);
                Assert.False(property.CanWrite);
                Assert.True(property.GetMethod?.IsStatic);
            },
            property =>
            {
                Assert.Equal("DialogueDelivery", property.Name);
                Assert.Equal(typeof(SaveSchemaVersion), property.PropertyType);
                Assert.False(property.CanWrite);
                Assert.True(property.GetMethod?.IsStatic);
            },
            property =>
            {
                Assert.Equal("FirstPatient", property.Name);
                Assert.Equal(typeof(SaveSchemaVersion), property.PropertyType);
                Assert.False(property.CanWrite);
                Assert.True(property.GetMethod?.IsStatic);
            },
            property =>
            {
                Assert.Equal("FirstSupported", property.Name);
                Assert.Equal(typeof(SaveSchemaVersion), property.PropertyType);
                Assert.False(property.CanWrite);
                Assert.True(property.GetMethod?.IsStatic);
            },
            property =>
            {
                Assert.Equal("Journal", property.Name);
                Assert.Equal(typeof(SaveSchemaVersion), property.PropertyType);
                Assert.False(property.CanWrite);
                Assert.True(property.GetMethod?.IsStatic);
            },
            property =>
            {
                Assert.Equal("Map", property.Name);
                Assert.Equal(typeof(SaveSchemaVersion), property.PropertyType);
                Assert.False(property.CanWrite);
                Assert.True(property.GetMethod?.IsStatic);
            },
            property =>
            {
                Assert.Equal("Value", property.Name);
                Assert.Equal(typeof(int), property.PropertyType);
                Assert.False(property.CanWrite);
                Assert.False(property.GetMethod?.IsStatic);
            });
        Assert.Contains(declaredMethods, method => method.Name == nameof(SaveSchemaVersion.Parse));
        Assert.Empty(versionType.GetEvents());
        Assert.True(versionType.IsSealed);
    }
}
