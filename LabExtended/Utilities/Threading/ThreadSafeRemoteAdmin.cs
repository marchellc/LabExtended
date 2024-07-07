using LabExtended.API;

using Common.Extensions;

namespace LabExtended.Utilities.Threading
{
    public static class ThreadSafeRemoteAdmin
    {
        public static void Send(ExPlayer player, object message)
            => UnityThread.Thread.Run(() => player.RemoteAdminMessage(message), null);
    }
}
