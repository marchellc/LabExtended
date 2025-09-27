using HarmonyLib;
using LabExtended.API;
using LabExtended.API.Settings;
using LabExtended.Core;
using LabExtended.Extensions;
using Mirror;
using UserSettings.ServerSpecific;

namespace LabExtended.Patches.Functions.Settings
{
    static class SettingsProcessResponsePatch
    {
        [HarmonyPatch(typeof(ServerSpecificSettingsSync), nameof(ServerSpecificSettingsSync.ServerProcessClientResponseMsg))]
        private static bool Prefix(NetworkConnection conn, ref SSSClientResponse msg)
        {
            try {
                SettingsManager.OnResponseMessage(conn, msg);

                if (!ReferenceHub.TryGetHub(conn, out var hub) || !ExPlayer.TryGet(hub, out var player)) {
                    return false;
                }

                List<ServerSpecificSettingBase> orAddNew = ServerSpecificSettingsSync.ReceivedUserSettings.GetOrAddNew(hub);
                NetworkReaderPooled reader = NetworkReaderPool.Get(msg.Payload);
                foreach (ServerSpecificSettingBase item in orAddNew) {
                    if (item.SettingId == msg.Id && item.GetType() == msg.SettingType) {
                        ServerDeserializeClientResponse(player, item, reader);
                        return false;
                    }
                }
                ServerSpecificSettingBase serverSpecificSettingBase = ServerSpecificSettingsSync.CreateInstance(msg.SettingType);
                orAddNew.Add(serverSpecificSettingBase);
                serverSpecificSettingBase.SetId(msg.Id, null);
                serverSpecificSettingBase.ApplyDefaultValues();
                ServerDeserializeClientResponse(player, serverSpecificSettingBase, reader);
            } catch (Exception e) {
                ApiLog.Error(e.Message);
                ApiLog.Error(e.ToColoredString());
            }

            return false;
        }

        private static void ServerDeserializeClientResponse(ExPlayer player, ServerSpecificSettingBase setting, NetworkReaderPooled reader) {
            if (setting.ResponseMode == 0 || OnSettingsListeners == null) {
                reader.Dispose();
                return;
            }
            setting.DeserializeValue(reader);

            int msgId = setting.SettingId;
            Type msgType = setting.GetType();
            foreach (Delegate del in OnSettingsListeners.GetInvocationList()) {
                var assembly = del.Method.DeclaringType.Assembly;
                if ((VanillaSettingsAdapter.SssByAssemblyPersonal.TryGetValue(player, out var loadedPlayerSettings) &&
                   loadedPlayerSettings.Any(ps => ps.Value.Any(s => s.SettingId == msgId && s.GetType() == msgType))) ||
                   VanillaSettingsAdapter.SssByAssemblyGlobal.Any(ps => ps.Value.Any(s => s.SettingId == msgId && s.GetType() == msgType))) {
                    del.DynamicInvoke(player.ReferenceHub, setting);
                }

            }

            reader.Dispose();
        }

        private static Action<ReferenceHub, ServerSpecificSettingBase> OnSettingsListeners =
            Traverse.Create(typeof(ServerSpecificSettingsSync))
                .Field(nameof(ServerSpecificSettingsSync.ServerOnSettingValueReceived))
                .GetValue<Action<ReferenceHub, ServerSpecificSettingBase>>();
    }
}
