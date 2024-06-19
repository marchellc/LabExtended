using Hints;

using LabExtended.API;

namespace LabExtended.Utilities
{
    public static class ProxyMethods
    {
        public static Action<ExPlayer, string, ushort> ShowHint { get; set; } = DefaultShowHint;
        public static Action<ExPlayer, string, ushort, bool> ShowBroadcast { get; set; } = DefaultShowBroadcast;

        private static void DefaultShowBroadcast(ExPlayer player, string content, ushort duration, bool clearPrevious)
        {
            if (clearPrevious)
                Broadcast.Singleton?.TargetClearElements(player.Connection);

            Broadcast.Singleton?.TargetAddElement(player.Connection, content, duration, Broadcast.BroadcastFlags.Normal);
        }

        private static void DefaultShowHint(ExPlayer player, string content, ushort duration)
            => player.Hub.hints.Show(new TextHint(content, null, null, duration));
    }
}