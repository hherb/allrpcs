using System.Numerics;
using System.Reflection;
using Allrpcs.Application.Discovery;
using Allrpcs.Application.Navigation;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Controllers;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Application.Tests.Navigation;

public sealed class OpeningRegionOldShrineInspectionAccessPolicyTests
{
    [Fact]
    public void AuthorizesOneExactScribeInspectionWithoutChangingDiscovery()
    {
        TestInput input = CreateInput(backgroundIndex: 1);

        OpeningRegionOldShrineInspectionAccess? access =
            OpeningRegionOldShrineInspectionAccessPolicy.Authorize(
                input.RouteAccess,
                input.RouteAccess,
                input.ShrineBinding,
                input.ShrineBinding,
                input.InscriptionBinding,
                input.InscriptionBinding,
                issuedAccess: null);

        Assert.NotNull(access);
        Assert.Same(input.RouteAccess, access.RouteAccess);
        Assert.Same(input.RouteAccess.SelectedVariation, access.ScribeVariation);
        Assert.Same(input.ShrineBinding, access.ShrineSceneBinding);
        Assert.Same(input.InscriptionBinding, access.InscriptionBinding);
        Assert.Same(
            input.InscriptionBinding.InitialVisibility,
            input.InscriptionBinding.DiscoveryScenario.InitialPerception.Entries.Single(
                candidate => candidate.ObjectId.Value == "shrine-inscription"));
        Assert.Same(VisibilityState.Noticed, input.InscriptionBinding.InitialVisibility.VisibilityState);
    }

    [Fact]
    public void RejectsOtherBackgroundNonCurrentAndRepeatedInputs()
    {
        TestInput scribe = CreateInput(backgroundIndex: 1);
        TestInput forester = CreateInput(backgroundIndex: 0);
        TestInput apothecary = CreateInput(backgroundIndex: 2);
        TestInput traveller = CreateInput(backgroundIndex: 3);
        OpeningRegionOldShrineInspectionAccess issued = Assert.IsType<
            OpeningRegionOldShrineInspectionAccess>(
                OpeningRegionOldShrineInspectionAccessPolicy.Authorize(
                    scribe.RouteAccess,
                    scribe.RouteAccess,
                    scribe.ShrineBinding,
                    scribe.ShrineBinding,
                    scribe.InscriptionBinding,
                    scribe.InscriptionBinding,
                    issuedAccess: null));

        Assert.Null(Authorize(forester));
        Assert.Null(Authorize(apothecary));
        Assert.Null(Authorize(traveller));
        Assert.Null(OpeningRegionOldShrineInspectionAccessPolicy.Authorize(
            scribe.RouteAccess,
            forester.RouteAccess,
            scribe.ShrineBinding,
            scribe.ShrineBinding,
            scribe.InscriptionBinding,
            scribe.InscriptionBinding,
            issuedAccess: null));
        Assert.Null(OpeningRegionOldShrineInspectionAccessPolicy.Authorize(
            scribe.RouteAccess,
            scribe.RouteAccess,
            scribe.ShrineBinding,
            scribe.ShrineBinding,
            scribe.InscriptionBinding,
            scribe.InscriptionBinding,
            issued));
        Assert.Same(scribe.RouteAccess, issued.RouteAccess);
    }

    [Fact]
    public void RejectsStaleRebuiltForeignAndNonDestinationProvenance()
    {
        TestInput input = CreateInput(backgroundIndex: 1);
        TestInput rebuilt = CreateInput(
            backgroundIndex: 1,
            input.Options,
            input.Projection);
        TestInput foreign = CreateInput(backgroundIndex: 1);
        OpeningRegionOldShrineSceneBinding nonDestination = new(
            input.Projection,
            input.Projection.Anchors[7],
            new NavigationWorldBounds(24.0f, 24.0f, 216.0f, 136.0f));
        OpeningRegionLayeredInscriptionBinding foreignInscription =
            CreateInscriptionBinding(nonDestination);

        Assert.Null(OpeningRegionOldShrineInspectionAccessPolicy.Authorize(
            input.RouteAccess,
            rebuilt.RouteAccess,
            input.ShrineBinding,
            input.ShrineBinding,
            input.InscriptionBinding,
            input.InscriptionBinding,
            issuedAccess: null));
        Assert.Null(OpeningRegionOldShrineInspectionAccessPolicy.Authorize(
            input.RouteAccess,
            foreign.RouteAccess,
            input.ShrineBinding,
            input.ShrineBinding,
            input.InscriptionBinding,
            input.InscriptionBinding,
            issuedAccess: null));
        Assert.Null(OpeningRegionOldShrineInspectionAccessPolicy.Authorize(
            input.RouteAccess,
            input.RouteAccess,
            input.ShrineBinding,
            nonDestination,
            input.InscriptionBinding,
            foreignInscription,
            issuedAccess: null));
        Assert.Null(OpeningRegionOldShrineInspectionAccessPolicy.Authorize(
            input.RouteAccess,
            input.RouteAccess,
            input.ShrineBinding,
            input.ShrineBinding,
            input.InscriptionBinding,
            foreignInscription,
            issuedAccess: null));
    }

    [Fact]
    public void RejectsMalformedAndNullInputs()
    {
        TestInput input = CreateInput(backgroundIndex: 1);
        ConstructorInfo constructor = typeof(SeekerBackgroundRouteAccess)
            .GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)
            .Single();
        SeekerBackgroundRouteAccess malformed =
            Assert.IsType<SeekerBackgroundRouteAccess>(constructor.Invoke(
            [
                input.RouteAccess.RouteState,
                input.RouteAccess.SelectedVariation,
                input.RouteAccess.RouteState.OpeningRegion.Anchors[0],
            ]));

        Assert.Null(OpeningRegionOldShrineInspectionAccessPolicy.Authorize(
            malformed,
            malformed,
            input.ShrineBinding,
            input.ShrineBinding,
            input.InscriptionBinding,
            input.InscriptionBinding,
            issuedAccess: null));
        Assert.Throws<ArgumentNullException>(() =>
            OpeningRegionOldShrineInspectionAccessPolicy.Authorize(
                null!,
                input.RouteAccess,
                input.ShrineBinding,
                input.ShrineBinding,
                input.InscriptionBinding,
                input.InscriptionBinding,
                issuedAccess: null));
        Assert.Throws<ArgumentNullException>(() =>
            OpeningRegionOldShrineInspectionAccessPolicy.Authorize(
                input.RouteAccess,
                null!,
                input.ShrineBinding,
                input.ShrineBinding,
                input.InscriptionBinding,
                input.InscriptionBinding,
                issuedAccess: null));
        Assert.Throws<ArgumentNullException>(() =>
            OpeningRegionOldShrineInspectionAccessPolicy.Authorize(
                input.RouteAccess,
                input.RouteAccess,
                null!,
                input.ShrineBinding,
                input.InscriptionBinding,
                input.InscriptionBinding,
                issuedAccess: null));
        Assert.Throws<ArgumentNullException>(() =>
            OpeningRegionOldShrineInspectionAccessPolicy.Authorize(
                input.RouteAccess,
                input.RouteAccess,
                input.ShrineBinding,
                null!,
                input.InscriptionBinding,
                input.InscriptionBinding,
                issuedAccess: null));
        Assert.Throws<ArgumentNullException>(() =>
            OpeningRegionOldShrineInspectionAccessPolicy.Authorize(
                input.RouteAccess,
                input.RouteAccess,
                input.ShrineBinding,
                input.ShrineBinding,
                null!,
                input.InscriptionBinding,
                issuedAccess: null));
        Assert.Throws<ArgumentNullException>(() =>
            OpeningRegionOldShrineInspectionAccessPolicy.Authorize(
                input.RouteAccess,
                input.RouteAccess,
                input.ShrineBinding,
                input.ShrineBinding,
                input.InscriptionBinding,
                null!,
                issuedAccess: null));
    }

    private static OpeningRegionOldShrineInspectionAccess? Authorize(TestInput input) =>
        OpeningRegionOldShrineInspectionAccessPolicy.Authorize(
            input.RouteAccess,
            input.RouteAccess,
            input.ShrineBinding,
            input.ShrineBinding,
            input.InscriptionBinding,
            input.InscriptionBinding,
            issuedAccess: null);

    private static TestInput CreateInput(
        int backgroundIndex,
        SeekerCustomizationOptions? options = null,
        OpeningRegionProjection? projection = null)
    {
        options ??= RepresentativeSeekerCustomizationOptionsSource.Create();
        projection ??= OpeningRegionOldShrineSceneBindingTests.Projection();
        SeekerBackgroundRouteVariationState initial =
            SeekerBackgroundRouteVariationStateTransition.Initialize(options, projection);
        Character current = Character();
        SeekerCustomizationSelectionState selection =
            SeekerCustomizationSelectionStateTransition.Select(
                SeekerCustomizationSelectionStateTransition.Initialize(options),
                new CharacterIdentity(current.Identity.Id, "Mara"),
                new SeekerPronouns("they", "them", "their", "theirs", "themself"),
                options.AppearanceChoices[0],
                options.BackgroundChoices[backgroundIndex]);
        SeekerCustomizedCharacterApplication application =
            SeekerCustomizedCharacterProjection.Project(selection, current);
        SeekerBackgroundRouteVariationState routeState =
            SeekerBackgroundRouteVariationStateTransition.Select(
                initial,
                selection,
                application);
        SeekerBackgroundRouteAccess routeAccess =
            Assert.IsType<SeekerBackgroundRouteAccess>(
                SeekerBackgroundRouteAccessPolicy.Authorize(
                    routeState,
                    routeState,
                    routeState.SelectedVariation!.DestinationAnchor,
                    issuedAccess: null));
        OpeningRegionOldShrineSceneBinding shrine = new(
            projection,
            projection.Anchors[7],
            new NavigationWorldBounds(24.0f, 24.0f, 216.0f, 136.0f));
        return new TestInput(
            options,
            projection,
            routeAccess,
            shrine,
            CreateInscriptionBinding(shrine));
    }

    private static OpeningRegionLayeredInscriptionBinding CreateInscriptionBinding(
        OpeningRegionOldShrineSceneBinding shrine)
    {
        DiscoveryScenario scenario = RepresentativeDiscoveryScenarioSource.Create();
        WorldObject inscription = scenario.WorldObjects.Entries.Single(candidate =>
            candidate.Identity.Id.Value == "shrine-inscription");
        WorldObjectVisibility visibility = scenario.InitialPerception.Entries.Single(candidate =>
            candidate.ObjectId.Value == "shrine-inscription");
        return new OpeningRegionLayeredInscriptionBinding(
            shrine,
            scenario,
            inscription,
            visibility,
            new Vector2(110.0f, 90.0f));
    }

    private static Character Character()
    {
        CharacterId id = new("seeker");
        return new Character(
            new CharacterIdentity(id, "SEEKER"),
            new PublicCharacterBackground(string.Empty),
            new PrivateCharacterBackground(string.Empty),
            new CharacterCapabilitySet([]),
            new CharacterLimitationSet([]),
            new CharacterControllerAssignment(id, ControllerType.Human),
            new CharacterConditionSet([]),
            new CharacterInventory([]),
            new CharacterKnowledge([]),
            new CharacterBeliefSet([]),
            new CharacterRelationshipSet([]),
            new CharacterGoalSet([]));
    }

    private sealed record TestInput(
        SeekerCustomizationOptions Options,
        OpeningRegionProjection Projection,
        SeekerBackgroundRouteAccess RouteAccess,
        OpeningRegionOldShrineSceneBinding ShrineBinding,
        OpeningRegionLayeredInscriptionBinding InscriptionBinding);
}
