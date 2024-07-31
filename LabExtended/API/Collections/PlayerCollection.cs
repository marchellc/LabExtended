using LabExtended.API.Collections.Locked;

using Mirror;

using PluginAPI.Core;

namespace LabExtended.API.Collections
{
    /// <summary>
    /// A class that helps with managing a list of players.
    /// </summary>
    public class PlayerCollection : IDisposable
    {
        internal static readonly LockedHashSet<PlayerCollection> _handlers = new LockedHashSet<PlayerCollection>(); // A list of all handlers, used for player leave.

        internal PlayerCollection()
        {
            _netIdList = new LockedHashSet<uint>(20);
            _handlers.Add(this);
        }

        internal LockedHashSet<uint> _netIdList;

        /// <summary>
        /// Adds a specific network ID.
        /// </summary>
        /// <param name="netId">The network ID to add.</param>
        /// <returns><see langword="true"/> if it was succesfully added, otherwise <see langword="false"/>.</returns>
        public bool Add(uint netId)
            => _netIdList.Add(netId);

        public bool Add(ReferenceHub hub)
            => hub != null && _netIdList.Add(hub.netId);

        public bool Add(Player player)
            => player != null && _netIdList.Add(player.NetworkId);

        public bool Add(ExPlayer player)
            => player != null && _netIdList.Add(player.NetId);

        public bool Add(NetworkIdentity identity)
            => identity != null && _netIdList.Add(identity.netId);

        public void AddAll()
            => _netIdList.AddRange(ExPlayer.Players.Select(p => p.NetId));

        public void AddRange(IEnumerable<ExPlayer> players)
            => _netIdList.AddRange(players.Select(p => p.NetId));

        public int AddWhere(Func<ExPlayer, bool> predicate)
        {
            var count = 0;

            foreach (var player in ExPlayer.Players)
            {
                if (Contains(player))
                    continue;

                if (!predicate(player))
                    continue;

                if (_netIdList.Add(player.NetId))
                    count++;
            }

            return count;
        }

        public bool Remove(uint netId)
            => _netIdList.Remove(netId);

        public bool Remove(ReferenceHub hub)
            => hub != null && _netIdList.Remove(hub.netId);

        public bool Remove(Player player)
            => player != null && _netIdList.Remove(player.NetworkId);

        public bool Remove(ExPlayer player)
            => player != null && _netIdList.Remove(player.NetId);

        public bool Remove(NetworkIdentity identity)
            => identity != null && _netIdList.Remove(identity.netId);

        public int RemoveWhere(Func<ExPlayer, bool> predicate)
        {
            var count = 0;

            foreach (var player in ExPlayer.Players)
            {
                if (!Contains(player))
                    continue;

                if (!predicate(player))
                    continue;

                if (_netIdList.Remove(player.NetId))
                    count++;
            }

            return count;
        }

        public bool Contains(uint netId)
            => _netIdList.Contains(netId);

        public bool Contains(ReferenceHub hub)
            => hub != null && _netIdList.Contains(hub.netId);

        public bool Contains(Player player)
            => player != null && _netIdList.Contains(player.NetworkId);

        public bool Contains(ExPlayer player)
            => player != null && _netIdList.Contains(player.NetId);

        public bool Contains(NetworkIdentity identity)
            => identity != null && _netIdList.Contains(identity.netId);

        public void Clear()
            => _netIdList.Clear();

        public void Dispose()
        {
            _netIdList.Clear();
            _netIdList = null;

            _handlers.Remove(this);
        }
    }
}