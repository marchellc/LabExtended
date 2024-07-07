using Common.Extensions;

using HarmonyLib;

using InventorySystem;
using InventorySystem.Items;

using LabExtended.Core;
using LabExtended.Extensions;

using System.Collections.Frozen;
using System.Reflection;

namespace LabExtended.Patches.Optimize
{
    /// <summary>
    /// A patch that replaces the base-game's item dictionary with a <see cref="FrozenDictionary{TKey, TValue}"/>.
    /// </summary>
    public static class FrozenItemsPatch
    {
        private static MethodInfo _defaultMethod;
        private static MethodInfo _replaceMethod;

        /// <summary>
        /// Gets a list of all types that implement the <see cref="ItemBase"/> type.
        /// </summary>
        public static FrozenSet<Type> ItemTypes { get; private set; }

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
                if (ItemTypes is null)
                {
                    var types = new List<Type>();

                    foreach (var type in typeof(ServerConsole).Assembly.GetTypes())
                    {
                        if (!type.InheritsType<ItemBase>())
                            continue;

                        types.Add(type);
                    }

                    ItemTypes = types.ToFrozenSet();
                }

                _defaultMethod ??= typeof(InventoryItemLoader).GetAllMethods().First(m => m.Name == "TryGetItem");
                _replaceMethod ??= typeof(FrozenItemsPatch).GetAllMethods().First(m => m.Name == "Prefix");

                foreach (var type in ItemTypes)
                    Harmony.Patch(_defaultMethod.MakeGenericMethod(type), new HarmonyMethod(_replaceMethod.MakeGenericMethod(type)));
            }
            catch (Exception ex)
            {
                ExLoader.Error("FrozenItemsPatch", ex);
            }
        }

        private static bool Prefix<T>(ItemType itemType, ref bool __result, out T result) where T : ItemBase
        {
            __result = itemType.TryGetItemPrefab(out result);
            return false;
        }
    }
}