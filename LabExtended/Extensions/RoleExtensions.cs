using GameObjectPools;

using LabExtended.API;
using LabExtended.Core;

using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using PlayerRoles.FirstPersonControl.Thirdperson;

namespace LabExtended.Extensions
{
    public static class RoleExtensions
    {
        public static Dictionary<RoleTypeId, PlayerRoleBase> Prefabs { get; }
        public static Dictionary<RoleTypeId, string> Names { get; }

        static RoleExtensions()
        {
            PlayerRoleLoader.LoadRoles();

            Prefabs = PlayerRoleLoader.AllRoles;
            Names = PlayerRoleLoader.AllRoles.ToDictionary(
                keyPair => keyPair.Key,
                valuePair => valuePair.Value.RoleName ?? valuePair.Value.RoleTypeId.ToString().SpaceByUpperCase());
        }

        public static bool TryGetPrefab(this RoleTypeId roleType, out PlayerRoleBase prefab)
            => Prefabs.TryGetValue(roleType, out prefab);

        public static bool TryGetPrefab<T>(this RoleTypeId roleType, out T prefab) where T : PlayerRoleBase
        {
            if (!Prefabs.TryGetValue(roleType, out var roleBase))
            {
                ApiLoader.Warn("Frozen Roles", $"Unknown role: &1{roleType}&r");

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

        public static PlayerRoleBase GetPrefab(this RoleTypeId roleType)
            => TryGetPrefab(roleType, out var prefab) ? prefab : null;

        public static T GetPrefab<T>(this RoleTypeId roleType) where T : PlayerRoleBase
            => TryGetPrefab<T>(roleType, out var prefab) ? prefab : null;

        public static PlayerRoleBase GetInstance(this RoleTypeId roleType)
        {
            if (!TryGetPrefab(roleType, out var prefab))
                return null;

            if (!PoolManager.Singleton.TryGetPoolObject(prefab.gameObject, out var pooledRole, false) || pooledRole is not PlayerRoleBase roleBase)
                return null;

            return roleBase;
        }

        public static PlayerRoleBase GetInstance(this PlayerRoleBase prefab)
        {
            if (prefab is null)
                return null;

            if (!PoolManager.Singleton.TryGetPoolObject(prefab.gameObject, out var pooledRole, false) || pooledRole is not PlayerRoleBase roleBase)
                return null;

            return roleBase;
        }

        public static T GetInstance<T>(this RoleTypeId roleType) where T : PlayerRoleBase
        {
            if (!TryGetPrefab(roleType, out var prefab))
                return null;

            if (!PoolManager.Singleton.TryGetPoolObject(prefab.gameObject, out var pooledRole, false) || pooledRole is not T roleBase)
                return null;

            return roleBase;
        }

        public static T GetInstance<T>(this T prefab) where T : PlayerRoleBase
        {
            if (prefab is null)
                return null;

            if (!PoolManager.Singleton.TryGetPoolObject(prefab.gameObject, out var pooledRole, false) || pooledRole is not T roleBase)
                return null;

            return roleBase;
        }

        public static CharacterModel GetModel(this RoleTypeId role)
        {
            if (!TryGetPrefab(role, out var prefab) || prefab is not IFpcRole fpcRole)
                return null;

            return fpcRole.FpcModule.CharacterModelInstance;
        }

        public static CharacterControllerSettingsPreset GetSettings(this RoleTypeId role)
        {
            if (!TryGetPrefab(role, out var prefab) || prefab is not IFpcRole fpcRole)
                return null;

            return fpcRole.FpcModule.CharacterControllerSettings;
        }

        public static bool IsScp(this RoleTypeId role, bool countZombies = true)
            => role is RoleTypeId.Scp049 || (role is RoleTypeId.Scp0492 && countZombies) || role is RoleTypeId.Scp079 || role is RoleTypeId.Scp096 || role is RoleTypeId.Scp106 || role is RoleTypeId.Scp173 || role is RoleTypeId.Scp3114 || role is RoleTypeId.Scp939;

        public static bool IsNtf(this RoleTypeId role)
            => role is RoleTypeId.NtfCaptain || role is RoleTypeId.NtfPrivate || role is RoleTypeId.NtfSergeant || role is RoleTypeId.NtfSpecialist;

        public static bool IsChaos(this RoleTypeId role)
            => role is RoleTypeId.ChaosConscript || role is RoleTypeId.ChaosMarauder || role is RoleTypeId.ChaosRepressor || role is RoleTypeId.ChaosRifleman;

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

        public static bool IsFriendly(this RoleTypeId role, RoleTypeId otherRole)
            => !IsEnemy(role, otherRole);

        public static string GetName(this RoleTypeId roleType)
            => Names[roleType];

        public static string GetColoredName(this RoleTypeId roleType)
        {
            var baseName = Names[roleType];

            if (TryGetPrefab(roleType, out var prefab))
                baseName = $"<color=#{prefab.RoleColor.ToHex()}>{baseName}</color>";

            return baseName;
        }

        public static IEnumerable<ExPlayer> GetPlayers(this RoleTypeId role)
            => ExPlayer.Get(role);

        public static void ForEach(this RoleTypeId role, Action<ExPlayer> action)
        {
            foreach (var player in GetPlayers(role))
                action(player);
        }

        public static void ForEach<T>(this RoleTypeId role, Action<ExPlayer, T> action) where T : PlayerRoleBase
        {
            foreach (var player in GetPlayers(role))
            {
                if (player.Role.Is<T>(out var castRole))
                    action(player, castRole);
            }
        }
    }
}