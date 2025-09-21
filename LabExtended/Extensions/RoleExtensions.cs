using GameObjectPools;

using LabExtended.API;
using LabExtended.Core;

using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using PlayerRoles.FirstPersonControl.Thirdperson;

using UnityEngine;

namespace LabExtended.Extensions;

/// <summary>
/// Extensions targeting in-game roles.
/// </summary>
public static class RoleExtensions
{
    /// <summary>
    /// Dictionary of all role prefabs.
    /// </summary>
    public static Dictionary<RoleTypeId, PlayerRoleBase> Prefabs { get; }

    /// <summary>
    /// Dictionary of all role names.
    /// </summary>
    public static Dictionary<RoleTypeId, string> Names { get; }

    static RoleExtensions()
    {
        PlayerRoleLoader.LoadRoles();

        Prefabs = PlayerRoleLoader.AllRoles;
        Names = PlayerRoleLoader.AllRoles.ToDictionary(
            keyPair => keyPair.Key,
            valuePair => valuePair.Value.RoleName ?? valuePair.Value.RoleTypeId.ToString().SpaceByUpperCase());
    }

    /// <summary>
    /// Gets the spawn position of a role.
    /// </summary>
    /// <param name="role">The role to get the spawn position of.</param>
    /// <returns>The spawn position as a tuple.</returns>
    public static (Vector3 position, float horizontalRotation) GetSpawnPosition(this RoleTypeId role)
    {
        if (!role.TryGetSpawnPosition(out var position, out var horizontalRotation))
            throw new Exception($"Could not get spawnpoint for role {role}");

        return (position, horizontalRotation);
    }

    /// <summary>
    /// Attempts to get the spawn position of a role.
    /// </summary>
    /// <param name="role">The role to get the spawn position of.</param>
    /// <param name="position">The spawn position.</param>
    /// <param name="horizontalRotation">The spawn horizontal rotation angle.</param>
    /// <returns>true if the spawn position was found</returns>
    public static bool TryGetSpawnPosition(this RoleTypeId role, out Vector3 position, out float horizontalRotation)
    {
        position = default;
        horizontalRotation = 0f;

        if (!role.TryGetPrefab(out var prefab))
            return false;

        if (prefab is not IFpcRole fpcRole || fpcRole.SpawnpointHandler is null)
            return false;

        return fpcRole.SpawnpointHandler.TryGetSpawnpoint(out position, out horizontalRotation);
    }

    /// <summary>
    /// Attempts to get a role's prefab.
    /// </summary>
    public static bool TryGetPrefab(this RoleTypeId roleType, out PlayerRoleBase prefab)
        => Prefabs.TryGetValue(roleType, out prefab);

    /// <summary>
    /// Attempts to get a role's prefab.
    /// </summary>
    public static bool TryGetPrefab<T>(this RoleTypeId roleType, out T prefab) where T : PlayerRoleBase
    {
        if (!Prefabs.TryGetValue(roleType, out var roleBase))
        {
            ApiLog.Warn("Role API", $"Unknown role: &1{roleType}&r");

            prefab = null;
            return false;
        }

        if (roleBase is not T castPrefab)
        {
            prefab = null;
            return false;
        }

        prefab = castPrefab;
        return true;
    }

    /// <summary>
    /// Gets a role's prefab.
    /// </summary>
    public static PlayerRoleBase GetPrefab(this RoleTypeId roleType)
        => TryGetPrefab(roleType, out var prefab) ? prefab : null;

    /// <summary>
    /// Gets a role's prefab.
    /// </summary>
    public static T GetPrefab<T>(this RoleTypeId roleType) where T : PlayerRoleBase
        => TryGetPrefab<T>(roleType, out var prefab) ? prefab : null;

    /// <summary>
    /// Gets the instance of a role from the pool.
    /// </summary>
    public static PlayerRoleBase GetInstance(this RoleTypeId roleType)
    {
        if (!TryGetPrefab(roleType, out var prefab))
            return null;

        if (!PoolManager.Singleton.TryGetPoolObject(prefab.gameObject, out var pooledRole, false) || pooledRole is not PlayerRoleBase roleBase)
            return null;

        return roleBase;
    }

    /// <summary>
    /// Gets the instance of a role from the pool.
    /// </summary>
    public static PlayerRoleBase GetInstance(this PlayerRoleBase prefab)
    {
        if (prefab is null)
            return null;

        if (!PoolManager.Singleton.TryGetPoolObject(prefab.gameObject, out var pooledRole, false) || pooledRole is not PlayerRoleBase roleBase)
            return null;

        return roleBase;
    }

    /// <summary>
    /// Gets the instance of a role from the pool.
    /// </summary>
    public static T GetInstance<T>(this RoleTypeId roleType) where T : PlayerRoleBase
    {
        if (!TryGetPrefab(roleType, out var prefab))
            return null;

        if (!PoolManager.Singleton.TryGetPoolObject(prefab.gameObject, out var pooledRole, false) || pooledRole is not T roleBase)
            return null;

        return roleBase;
    }

    /// <summary>
    /// Gets the instance of a role from the pool.
    /// </summary>
    public static T GetInstance<T>(this T prefab) where T : PlayerRoleBase
    {
        if (prefab is null)
            return null;

        if (!PoolManager.Singleton.TryGetPoolObject(prefab.gameObject, out var pooledRole, false) || pooledRole is not T roleBase)
            return null;

        return roleBase;
    }

    /// <summary>
    /// Gets the model of a role's prefab.
    /// </summary>
    /// <param name="role">The role to get the model of.</param>
    /// <returns>The model of the role's prefab.</returns>
    public static CharacterModel GetModel(this RoleTypeId role)
    {
        if (!TryGetPrefab(role, out var prefab) || prefab is not IFpcRole fpcRole)
            return null;

        return fpcRole.FpcModule.CharacterModelInstance;
    }

    /// <summary>
    /// Gets the settings of a role's prefab.
    /// </summary>
    /// <param name="role">The role to get the settings of.</param>
    /// <returns>The settings of the role's prefab.</returns>
    public static CharacterControllerSettingsPreset GetSettings(this RoleTypeId role)
    {
        if (!TryGetPrefab(role, out var prefab) || prefab is not IFpcRole fpcRole)
            return null;

        return fpcRole.FpcModule.CharacterControllerSettings;
    }

    /// <summary>
    /// Whether or not a role is a SCP role.
    /// </summary>
    public static bool IsScp(this RoleTypeId role, bool countZombies = true)
        => role is RoleTypeId.Scp049 || (role is RoleTypeId.Scp0492 && countZombies) || role is RoleTypeId.Scp079 || role is RoleTypeId.Scp096 || role is RoleTypeId.Scp106 || role is RoleTypeId.Scp173 || role is RoleTypeId.Scp3114 || role is RoleTypeId.Scp939;

    /// <summary>
    /// Whether or not a role is an NTF role.
    /// </summary>
    public static bool IsNtf(this RoleTypeId role)
        => role is RoleTypeId.NtfCaptain or RoleTypeId.NtfPrivate or RoleTypeId.NtfSergeant or RoleTypeId.NtfSpecialist;

    /// <summary>
    /// Whether or not a role is a Chaos Insurgency role.
    /// </summary>
    public static bool IsChaos(this RoleTypeId role)
        => role is RoleTypeId.ChaosConscript or RoleTypeId.ChaosMarauder or RoleTypeId.ChaosRepressor or RoleTypeId.ChaosRifleman;

    /// <summary>
    /// Whether or not one role is considered the enemy to another.
    /// </summary>
    public static bool IsEnemy(this RoleTypeId role, RoleTypeId otherRole)
    {
        if (role == otherRole)
            return false;

        if (role is RoleTypeId.Tutorial || otherRole is RoleTypeId.Tutorial)
            return false;

        if (!otherRole.IsAlive() || !role.IsAlive())
            return false;

        if (role.GetTeam() == otherRole.GetTeam())
            return false;

        if ((role.IsNtf() && otherRole.IsChaos()) || (role.IsChaos() || otherRole.IsNtf()))
            return true;

        if (role.IsScp(true) || otherRole.IsScp(true))
            return true;

        if ((role is RoleTypeId.ClassD && (otherRole.IsNtf() || otherRole is RoleTypeId.FacilityGuard))
            || (otherRole is RoleTypeId.ClassD && (role.IsNtf() || role is RoleTypeId.FacilityGuard)))
            return true;

        return false;
    }

    /// <summary>
    /// Whether or not one role is considered friendly to another.
    /// </summary>
    public static bool IsFriendly(this RoleTypeId role, RoleTypeId otherRole)
        => !IsEnemy(role, otherRole);

    /// <summary>
    /// Gets the name of a role.
    /// </summary>
    public static string GetName(this RoleTypeId roleType)
        => Names[roleType];

    /// <summary>
    /// Gets the colored name of a role.
    /// </summary>
    public static string GetColoredName(this RoleTypeId roleType)
    {
        var baseName = Names[roleType];

        if (TryGetPrefab(roleType, out var prefab))
            baseName = $"<color=#{prefab.RoleColor.ToHex()}>{baseName}</color>";

        return baseName;
    }

    /// <summary>
    /// Gets all players with a specific role.
    /// </summary>
    public static IEnumerable<ExPlayer> GetPlayers(this RoleTypeId role)
        => ExPlayer.Get(role);

    /// <summary>
    /// Executes a delegate for each layer with a specific role.
    /// </summary>
    public static void ForEach(this RoleTypeId role, Action<ExPlayer> action)
    {
        foreach (var player in GetPlayers(role))
            action(player);
    }

    /// <summary>
    /// Executes a delegate for each layer with a specific role.
    /// </summary>
    public static void ForEach<T>(this RoleTypeId role, Action<ExPlayer, T> action) where T : PlayerRoleBase
    {
        foreach (var player in GetPlayers(role))
        {
            if (player.Role.Is<T>(out var castRole))
                action(player, castRole);
        }
    }
}