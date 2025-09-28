using Mirror;

using HarmonyLib;

using LabExtended.API;
using LabExtended.API.Settings;

using LabExtended.Core;
using LabExtended.Extensions;

using UserSettings.ServerSpecific;

namespace LabExtended.Patches.Functions.Settings
{
    /// <summary>
    /// Provides utilities and delegates for handling server-specific settings responses received from clients during
    /// the settings synchronization process.
    /// </summary>
    public static class SettingsProcessResponsePatch
    {
        /// <summary>
        /// Represents the delegate that is invoked when a server-specific setting is received for a player.
        /// </summary>
        public static Action<ReferenceHub, ServerSpecificSettingBase> OnSettingsListeners =
                Traverse.Create(typeof(ServerSpecificSettingsSync))
                            .Field(nameof(ServerSpecificSettingsSync.ServerOnSettingValueReceived))
                            .GetValue<Action<ReferenceHub, ServerSpecificSettingBase>>();

        [HarmonyPatch(typeof(ServerSpecificSettingsSync), nameof(ServerSpecificSettingsSync.ServerProcessClientResponseMsg))]
        private static bool Prefix(NetworkConnection conn, ref SSSClientResponse msg)
        {
            try
            {
                SettingsManager.OnResponseMessage(conn, msg);

                if (!ReferenceHub.TryGetHub(conn, out var hub) || !ExPlayer.TryGet(hub, out var player))
                    return false;

                var list = ServerSpecificSettingsSync.ReceivedUserSettings.GetOrAddNew(hub);
                var reader = NetworkReaderPool.Get(msg.Payload);

                for (var i = 0; i < list.Count; i++)
                {
                    var setting = list[i];

                    if (setting.SettingId == msg.Id && setting.GetType() == msg.SettingType)
                    {
                        ServerDeserializeClientResponse(player, setting, reader);
                        return false;
                    }
                }

                var newSetting = ServerSpecificSettingsSync.CreateInstance(msg.SettingType);

                list.Add(newSetting);

                newSetting.SetId(msg.Id, null);
                newSetting.ApplyDefaultValues();

                ServerDeserializeClientResponse(player, newSetting, reader);
            }
            catch (Exception e)
            {
                ApiLog.Error(e.ToColoredString());
            }

            return false;
        }

        private static void ServerDeserializeClientResponse(ExPlayer player, ServerSpecificSettingBase setting, NetworkReaderPooled reader)
        {
            if (setting.ResponseMode == 0 || OnSettingsListeners == null)
            {
                reader.Dispose();
                return;
            }

            setting.DeserializeValue(reader);

            var msgId = setting.SettingId;
            var msgType = setting.GetType();

            if (OnSettingsListeners != null)
            {
                foreach (var del in OnSettingsListeners.GetInvocationList())
                {

                    var assembly = (del.Method.DeclaringType ?? del.Method.ReflectedType).Assembly;

                    if (player.settingsByAssembly.Any(ps => ps.Value.Any(s => s.SettingId == msgId && s.GetType() == msgType))
                        || SettingsManager.GlobalSettingsByAssembly.Any(ps => ps.Value.Any(s => s.SettingId == msgId && s.GetType() == msgType)))
                    {
                        del.DynamicInvoke(player.ReferenceHub, setting);
                    }
                }
            }

            reader.Dispose();
        }
    }
}
