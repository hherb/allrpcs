using System.Reflection;

namespace Allrpcs.Domain.Tests;

public sealed class ListIdentityContractTests
{
    public static IEnumerable<object[]> Types =>
    [
        [typeof(Allrpcs.Domain.Dialogue.CharacterTurnParticipationConsiderationRoundResult)],
        [typeof(Allrpcs.Domain.Dialogue.CharacterTurnResponse)],
        [typeof(Allrpcs.Domain.Ingredients.IngredientPreparationPlan)],
        [typeof(Allrpcs.Domain.Characters.CharacterDialogueMisquotationEvidenceConsiderationRequest)],
        [typeof(Allrpcs.Domain.Characters.CharacterConversationSummary)],
        [typeof(Allrpcs.Domain.Characters.CharacterTargetSabotageInfluenceConsiderationRequest)],
        [typeof(Allrpcs.Domain.Characters.CharacterObjectSabotageMoveConsiderationRequest)],
        [typeof(Allrpcs.Domain.Characters.CharacterSabotageActionConsiderationRequest)],
        [typeof(Allrpcs.Domain.Characters.CharacterObjectSabotageSubstitutionConsiderationRequest)],
        [typeof(Allrpcs.Domain.Characters.CharacterBeliefAssessment)],
        [typeof(Allrpcs.Domain.Characters.CharacterObjectSabotageContaminationConsiderationRequest)],
        [typeof(Allrpcs.Domain.Characters.CharacterEpisodicMemory)],
        [typeof(Allrpcs.Domain.Characters.CharacterMemoryIndexEntry)],
        [typeof(Allrpcs.Domain.Characters.CharacterObjectSabotageConcealmentConsiderationRequest)],
        [typeof(Allrpcs.Domain.Characters.CharacterRelationshipMemory)],
        [typeof(Allrpcs.Domain.Characters.CharacterTargetSabotageInfluenceRecoveryConsiderationRequest)],
        [typeof(Allrpcs.Domain.Characters.CharacterTargetSabotageDelayConsiderationRequest)],
        [typeof(Allrpcs.Domain.Characters.CharacterTargetSabotageDelayRecoveryConsiderationRequest)],
    ];

    [Fact]
    public void InventoryContainsEighteenContainersAndTwentyTwoListProperties()
    {
        Type[] types = Types.Select(row => (Type)row[0]).ToArray();
        Assert.Equal(18, types.Length);
        Assert.Equal(18, types.Distinct().Count());
        Assert.Equal(22, types.Sum(type => type.GetProperties().Count(property =>
            property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(IReadOnlyList<>))));
    }

    [Theory]
    [MemberData(nameof(Types))]
    public void CollectionContainersExplicitlyUseObjectIdentity(Type type)
    {
        Assert.True(type.IsSealed);
        Assert.Equal(typeof(object), type.GetMethod(nameof(Equals), [typeof(object)])!.DeclaringType);
        Assert.Equal(typeof(object), type.GetMethod(nameof(GetHashCode), Type.EmptyTypes)!.DeclaringType);
    }
    internal static void AssertIdentity<T>(T original) where T : class
    {
        // Reconstruct from valid existing inputs, replacing every collection with an independent array.
        ConstructorInfo constructor = Assert.Single(typeof(T).GetConstructors());
        object?[] inputs = constructor.GetParameters().Select(parameter =>
        {
            PropertyInfo property = typeof(T).GetProperties().Single(property =>
                string.Equals(property.Name, parameter.Name, StringComparison.OrdinalIgnoreCase));
            object? value = property.GetValue(original);
            if (property.PropertyType.IsGenericType
                && property.PropertyType.GetGenericTypeDefinition() == typeof(IReadOnlyList<>))
            {
                object[] elements = ((System.Collections.IEnumerable)value!).Cast<object>().ToArray();
                Array copy = Array.CreateInstance(property.PropertyType.GetGenericArguments()[0], elements.Length);
                Array.Copy(elements, copy, elements.Length);
                return copy;
            }
            return value;
        }).ToArray();
        T independent = (T)constructor.Invoke(inputs);
        T shallowCopy = (T)typeof(object).GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(original, null)!;
        Assert.True(original.Equals(original));
        Assert.False(original.Equals(independent));
        Assert.False(original == independent);
        Assert.False(original.Equals(shallowCopy));
        Assert.False(EqualityComparer<T>.Default.Equals(original, independent));
        HashSet<T> set = [original, independent, shallowCopy];
        Assert.Equal(3, set.Count);
        Assert.Contains(original, set);
        Assert.Equal(original.GetHashCode(), original.GetHashCode());
        foreach (PropertyInfo property in typeof(T).GetProperties().Where(property =>
            property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(IReadOnlyList<>)))
        {
            Assert.NotSame(property.GetValue(original), property.GetValue(independent));
            Assert.Equal(((System.Collections.IEnumerable)property.GetValue(original)!).Cast<object>(),
                ((System.Collections.IEnumerable)property.GetValue(independent)!).Cast<object>());
        }
    }
}
