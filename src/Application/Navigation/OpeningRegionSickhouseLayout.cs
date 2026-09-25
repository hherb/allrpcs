using System.Numerics;

namespace Allrpcs.Application.Navigation;

/// <summary>Defines one collision-aware sickhouse room with a single traversable entrance.</summary>
public sealed class OpeningRegionSickhouseLayout
{
    /// <summary>Creates a bounded room retaining the exact first-patient scene provenance.</summary>
    public OpeningRegionSickhouseLayout(
        OpeningRegionFirstPatientSceneBinding firstPatientBinding,
        NavigationWorldBounds roomBounds,
        Vector2 entranceCenter,
        float entranceWidthPixels,
        float wallThicknessPixels,
        Vector2 exteriorApproach,
        Vector2 patientApproach)
    {
        ArgumentNullException.ThrowIfNull(firstPatientBinding);
        ValidateRoomBounds(firstPatientBinding.Projection.WorldBounds, roomBounds);
        ValidatePositiveFinite(entranceWidthPixels, nameof(entranceWidthPixels));
        ValidatePositiveFinite(wallThicknessPixels, nameof(wallThicknessPixels));

        float clearance = firstPatientBinding.Projection.MinimumPathClearancePixels;
        if (entranceWidthPixels < clearance * 2.0f)
        {
            throw new ArgumentOutOfRangeException(
                nameof(entranceWidthPixels),
                entranceWidthPixels,
                "The sickhouse entrance must retain path clearance on both sides.");
        }

        _ = NavigationRequestValidator.Create(
            firstPatientBinding.Projection.WorldBounds,
            exteriorApproach,
            patientApproach);
        if (entranceCenter.Y != roomBounds.Top
            || entranceCenter.X - (entranceWidthPixels / 2.0f) < roomBounds.Left + wallThicknessPixels
            || entranceCenter.X + (entranceWidthPixels / 2.0f) > roomBounds.Right - wallThicknessPixels)
        {
            throw new ArgumentException(
                "The sickhouse entrance must be one bounded opening in the north wall.",
                nameof(entranceCenter));
        }

        if (exteriorApproach.X != entranceCenter.X
            || exteriorApproach.Y > roomBounds.Top - clearance)
        {
            throw new ArgumentException(
                "The sickhouse exterior approach must align north of the entrance with path clearance.",
                nameof(exteriorApproach));
        }

        if (patientApproach != firstPatientBinding.SickhouseAnchor.Position
            || !HasInteriorClearance(roomBounds, patientApproach, clearance)
            || !HasInteriorClearance(
                roomBounds,
                firstPatientBinding.PatientPosition,
                clearance))
        {
            throw new ArgumentException(
                "The sickhouse patient approach must retain the exact anchor and interior clearance.",
                nameof(patientApproach));
        }

        FirstPatientBinding = firstPatientBinding;
        RoomBounds = roomBounds;
        EntranceCenter = entranceCenter;
        EntranceWidthPixels = entranceWidthPixels;
        WallThicknessPixels = wallThicknessPixels;
        ExteriorApproach = exteriorApproach;
        PatientApproach = patientApproach;
    }

    /// <summary>Gets the exact first-patient scene binding retained by this layout.</summary>
    public OpeningRegionFirstPatientSceneBinding FirstPatientBinding { get; }

    /// <summary>Gets the exterior edges of the room walls.</summary>
    public NavigationWorldBounds RoomBounds { get; }

    /// <summary>Gets the center of the single north-wall entrance.</summary>
    public Vector2 EntranceCenter { get; }

    /// <summary>Gets the width of the single entrance.</summary>
    public float EntranceWidthPixels { get; }

    /// <summary>Gets the physical wall thickness.</summary>
    public float WallThicknessPixels { get; }

    /// <summary>Gets the navigation point immediately outside the entrance.</summary>
    public Vector2 ExteriorApproach { get; }

    /// <summary>Gets the exact sickhouse anchor used as the patient approach.</summary>
    public Vector2 PatientApproach { get; }

    private static void ValidateRoomBounds(
        NavigationWorldBounds worldBounds,
        NavigationWorldBounds roomBounds)
    {
        if (!float.IsFinite(roomBounds.Left)
            || !float.IsFinite(roomBounds.Top)
            || !float.IsFinite(roomBounds.Right)
            || !float.IsFinite(roomBounds.Bottom)
            || roomBounds.Right <= roomBounds.Left
            || roomBounds.Bottom <= roomBounds.Top
            || roomBounds.Left < worldBounds.Left
            || roomBounds.Top < worldBounds.Top
            || roomBounds.Right > worldBounds.Right
            || roomBounds.Bottom > worldBounds.Bottom)
        {
            throw new ArgumentOutOfRangeException(
                nameof(roomBounds),
                roomBounds,
                "The sickhouse room must be finite, positive, and inside the opening region.");
        }
    }

    private static void ValidatePositiveFinite(float value, string parameterName)
    {
        if (!float.IsFinite(value) || value <= 0.0f)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                "Sickhouse dimensions must be finite and positive.");
        }
    }

    private static bool HasInteriorClearance(
        NavigationWorldBounds roomBounds,
        Vector2 position,
        float clearance) =>
        position.X >= roomBounds.Left + clearance
        && position.X <= roomBounds.Right - clearance
        && position.Y >= roomBounds.Top + clearance
        && position.Y <= roomBounds.Bottom - clearance;
}
