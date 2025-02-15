using AdminToys;

using UnityEngine;

namespace LabExtended.Core.Optimization.Toys;

public static class ToyPositionSync
{
    public const ulong PositionDirtyBit = 1UL;
    public const ulong RotationDirtyBit = 2UL;
    public const ulong ScaleDirtyBit = 4UL;
    
    public static float DistanceSync
    {
        get => ApiLoader.ApiConfig.OptimizationSection.ToyDistanceSync;
        set => ApiLoader.ApiConfig.OptimizationSection.ToyDistanceSync = value;
    }

    public static float AngleSync
    {
        get => ApiLoader.ApiConfig.OptimizationSection.ToyAngleSync;
        set => ApiLoader.ApiConfig.OptimizationSection.ToyAngleSync = value;
    }
    
    public static bool ShouldSync(Vector3 current, Vector3 target)
    {
        if (DistanceSync > 0f && Vector3.Distance(current, target) > DistanceSync) return true;
        if (current != target) return true;

        return false;
    }

    public static bool ShouldSync(Quaternion current, Quaternion target)
    {
        if (AngleSync > 0f && Quaternion.Angle(current, target) > AngleSync) return true;
        if (current != target) return true;
        
        return false;
    }

    public static void SyncPosition(AdminToyBase toy)
    {
        toy.Position = toy.transform.position;
        toy.syncVarDirtyBits |= PositionDirtyBit;
    }

    public static void SyncRotation(AdminToyBase toy)
    {
        toy.Rotation = toy.transform.rotation;
        toy.syncVarDirtyBits |= RotationDirtyBit;
    }

    public static void SyncScale(AdminToyBase toy)
    {
        toy.Scale = toy.transform.localScale;
        toy.syncVarDirtyBits |= ScaleDirtyBit;
    }
}