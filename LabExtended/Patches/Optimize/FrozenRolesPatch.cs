using Common.Extensions;

using HarmonyLib;

using PlayerRoles;

using LabExtended.Core;
using LabExtended.Extensions;

using System.Collections.Frozen;
using System.Reflection;

namespace LabExtended.Patches.Optimize
{
    /// <summary>
    /// A patch that replaces the base-game's item dictionary with a <see cref="FrozenDictionary{TKey, TValue}"/>.
    /// </summary>
    public static class FrozenRolesPatch
    {
        private static MethodInfo _defaultMethod;
        private static MethodInfo _replaceMethod;

        /// <summary>
        /// Gets a list of all types that implement the <see cref="PlayerRoleBase"/> type.
        /// </summary>
        public static FrozenSet<Type> RoleTypes { get; private set; }

        /// <summary>
        /// Gets the active <see cref="HarmonyLib.Harmony"/> instance.
        /// </summary>
        public static Harmony Harmony => ExLoader.Loader.Harmony;

        /// <summary>
        /// Enables this patch.
        /// </summary>
        public static void Enable()
        {
            try
            {
                if (RoleTypes is null)
                {
                    var types = new List<Type>();

                    foreach (var type in typeof(ServerConsole).Assembly.GetTypes())
                    {
                        if (!type.InheritsType<PlayerRoleBase>())
                            continue;

                        types.Add(type);
                    }

                    RoleTypes = types.ToFrozenSet();
                }

                _defaultMethod ??= typeof(PlayerRoleLoader).GetAllMethods().First(m => m.Name == "TryGetRoleTemplate");
                _replaceMethod ??= typeof(FrozenRolesPatch).GetAllMethods().First(m => m.Name == "Prefix");

                foreach (var type in RoleTypes)
                    Harmony.Patch(_defaultMethod.MakeGenericMethod(type), new HarmonyMethod(_replaceMethod.MakeGenericMethod(type)));
            }
            catch (Exception ex)
            {
                ExLoader.Error("FrozenRolesPatch", ex);
            }
        }

        private static bool Prefix<T>(RoleTypeId roleType, ref bool __result, out T result) where T : PlayerRoleBase
        {
            __result = roleType.TryGetPrefab(out result);
            return false;
        }
    }
}