using Hints;

using LabExtended.API;

namespace LabExtended.Utilities
{
    /// <summary>
    /// A class used to allow plugins to change the default API methods.
    /// </summary>
    public static class ProxyMethods
    {
        /// <summary>
        /// An alternative method to show players a hint.
        /// <para>The first <see cref="ExPlayer"/> argument is the receiving player.</para>
        /// <para>The second <see langword="string"/> argument is the hint content.</para>
        /// <para>The third <see langword="ushort"/> argument is the hint duration.</para>
        /// </summary>
        public static Action<ExPlayer, string, ushort> ShowHint { get; set; } = DefaultShowHint;

        /// <summary>
        /// An alternative method to show players a broadcast.
        /// <para>The first <see cref="ExPlayer"/> argument is the receiving player.</para>
        /// <para>The second <see langword="string"/> argument is the broadcast content.</para>
        /// <para>The third <see langword="ushort"/> argument is the broadcast duration.</para>
        /// <para>The fourth <see langword="bool"/> argument is a value indicating whether or not to clear previous broadcasts.</para>
        /// </summary>
        public static Action<ExPlayer, string, ushort, bool> ShowBroadcast { get; set; } = DefaultShowBroadcast;

        private static void DefaultShowBroadcast(ExPlayer player, string content, ushort duration, bool clearPrevious)
        {
            if (clearPrevious)
                Broadcast.Singleton?.TargetClearElements(player.Connection);

            Broadcast.Singleton?.TargetAddElement(player.Connection, content, duration, Broadcast.BroadcastFlags.Normal);
        }

        private static void DefaultShowHint(ExPlayer player, string content, ushort duration)
        {
            if (player.Hints != null)
                player.Hints.Show(content, duration);
            else
                player.Hub.hints.Show(new TextHint(content, null, null, duration));
        }
    }
}