using LabExtended.Events;
using LabExtended.Events.Mirror;

using LabExtended.Core;
using LabExtended.Utilities;
using LabExtended.Extensions;

using Mirror;

using HarmonyLib;

using NorthwoodLib.Pools;

using System.Reflection;

namespace LabExtended.Patches.Events.Mirror
{
    /// <summary>
    /// Implements the <see cref="MirrorEvents.SettingSyncVar"/> and <see cref="MirrorEvents.SetSyncVar"/> events.
    /// </summary>
    public static class MirrorSetSyncVarPatch
    {
        private static readonly MethodInfo patchMethod = typeof(MirrorSetSyncVarPatch).FindMethod(x => x.Name == "GeneratedSyncVarSetterPrefix");

        private static bool GeneratedSyncVarSetterPrefix<T>(NetworkBehaviour __instance, T value, ref T field, ulong dirtyBit, Action<T, T> OnChanged)
        {
            if (NetworkBehaviour.SyncVarEqual(value, ref field))
                return false;

            var settingSyncVarEventArgs = new MirrorSettingSyncVarEventArgs(__instance, typeof(T), dirtyBit, field, value);

            if (!MirrorEvents.OnSettingSyncVar(settingSyncVarEventArgs) || settingSyncVarEventArgs.NewValue is null)
                return false;

            var previousValue = field;

            __instance.SetSyncVar(value, ref field, dirtyBit);

            MirrorEvents.OnSetSyncVar(new MirrorSetSyncVarEventArgs(__instance, settingSyncVarEventArgs.Type, dirtyBit, previousValue, value));

            if (OnChanged != null && NetworkServer.activeHost && !__instance.GetSyncVarHookGuard(dirtyBit))
            {
                __instance.SetSyncVarHookGuard(dirtyBit, true);

                OnChanged(previousValue, value);

                __instance.SetSyncVarHookGuard(dirtyBit, false);
            }

            return false;
        }

        internal static void Internal_Init()
        {
            if (ApiLoader.ApiConfig.PatchSection.DisabledPatches.Contains("MirrorSetSyncVarPatch.GeneratedSyncVarSetterPrefix")
                || ApiLoader.ApiConfig.PatchSection.DisabledPatches.Contains("MirrorSetSyncVarPatch.*")
                || ApiLoader.ApiConfig.PatchSection.DisabledPatches.Contains("Mirror.*"))
                return;

            var targetMethod = typeof(NetworkBehaviour).FindMethod(x => x.Name == nameof(NetworkBehaviour.GeneratedSyncVarSetter));

            if (targetMethod is null)
            {
                ApiLog.Warn("MirrorSetSyncVarPatch", $"Could not find target method {nameof(NetworkBehaviour.GeneratedSyncVarSetter)}");
                return;
            }

            var patchedTypes = ListPool<Type>.Shared.Rent();

            try
            {
                foreach (var type in ReflectionUtils.GameAssembly.GetTypes())
                {
                    try
                    {
                        if (!type.InheritsType<NetworkBehaviour>())
                            continue;

                        foreach (var property in type.GetDeclaredProperties())
                        {
                            try
                            {
                                if (!property.Name.StartsWith("Network"))
                                    continue;

                                if (patchedTypes.Contains(property.PropertyType))
                                    continue;

                                var genericMethod = targetMethod.MakeGenericMethod(property.PropertyType);
                                var genericPatchMethod = patchMethod.MakeGenericMethod(property.PropertyType);

                                if (genericMethod != null)
                                {
                                    if (ApiPatcher.Harmony.Patch(genericMethod, new(genericPatchMethod)) != null)
                                    {
                                        patchedTypes.Add(property.PropertyType);
                                    }
                                    else
                                    {
                                        ApiLog.Warn("MirrorSetSyncVarPatch", $"Could not patch setter for type &3{property.PropertyType.FullName}&r");
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                ApiLog.Error("MirrorSetSyncVarPatch", $"Exception while patching property &3{property.Name}&r in type &3{type.FullName}&r:\n{ex}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ApiLog.Error("MirrorSetSyncVarPatch", $"Exception while patching in type &3{type.FullName}&r:\n{ex}");
                    }
                }
            }
            catch (Exception ex)
            {
                ApiLog.Error("MirrorSetSyncVarPatch", ex);
            }

            ApiPatcher.labExPatchCountOffset += patchedTypes.Count;

            ListPool<Type>.Shared.Return(patchedTypes);
        }
    }
}