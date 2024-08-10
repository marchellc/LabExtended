using LabExtended.API.Collections.Locked;

using Mirror;

using NorthwoodLib.Pools;

using PluginAPI.Core;

using System.Collections;

namespace LabExtended.API.Collections
{
    /// <summary>
    /// A class that helps with managing a list of players.
    /// </summary>
    public class PlayerCollection : IDisposable, IEnumerable<ExPlayer>
    {
        public struct PlayerEnumerator : IEnumerator<ExPlayer>
        {
            private int m_Index;
            private List<ExPlayer> m_PlayerList;
            private IEnumerable<uint> m_NetIdList;

            public PlayerEnumerator(IEnumerable<uint> networkIds)
            {
                m_Index = 0;
                m_NetIdList = networkIds;
                m_PlayerList = null;
            }

            public ExPlayer Current
            {
                get
                {
                    if (m_Index < 0 || m_Index >= m_PlayerList.Count)
                        return null;

                    return m_PlayerList[m_Index];
                }
            }

            object IEnumerator.Current => Current;

            public void Reset()
                => m_Index = 0;

            public void Dispose()
            {
                ListPool<ExPlayer>.Shared.Return(m_PlayerList);

                m_PlayerList = null;
                m_NetIdList = null;

                m_Index = 0;
            }

            public bool MoveNext()
            {
                if (m_PlayerList is null)
                {
                    var netIds = m_NetIdList;

                    m_PlayerList = ListPool<ExPlayer>.Shared.Rent(ExPlayer.Players.Where(x => netIds.Contains(x.NetId)));
                    m_Index = 0;

                    return true;
                }

                if (m_Index + 1 >= m_PlayerList.Count)
                    return false;

                m_Index++;
                return true;
            }
        }

        internal static readonly LockedHashSet<PlayerCollection> m_Handlers = new LockedHashSet<PlayerCollection>(); // A list of all handlers, used for player leave.

        public PlayerCollection()
        {
            m_List = new LockedHashSet<uint>(50);
            m_Handlers.Add(this);
        }

        internal LockedHashSet<uint> m_List;

        /// <summary>
        /// Adds a specific network ID.
        /// </summary>
        /// <param name="netId">The network ID to add.</param>
        /// <returns><see langword="true"/> if it was succesfully added, otherwise <see langword="false"/>.</returns>
        public bool Add(uint netId)
            => m_List.Add(netId);

        public bool Add(ReferenceHub hub)
            => hub != null && m_List.Add(hub.netId);

        public bool Add(Player player)
            => player != null && m_List.Add(player.NetworkId);

        public bool Add(ExPlayer player)
            => player != null && m_List.Add(player.NetId);

        public bool Add(NetworkIdentity identity)
            => identity != null && m_List.Add(identity.netId);

        public void AddAll()
            => m_List.AddRange(ExPlayer.Players.Select(p => p.NetId));

        public void AddRange(IEnumerable<ExPlayer> players)
            => m_List.AddRange(players.Select(p => p.NetId));

        public int AddWhere(Func<ExPlayer, bool> predicate)
        {
            var count = 0;

            foreach (var player in ExPlayer.Players)
            {
                if (Contains(player))
                    continue;

                if (!predicate(player))
                    continue;

                if (m_List.Add(player.NetId))
                    count++;
            }

            return count;
        }

        public bool Remove(uint netId)
            => m_List.Remove(netId);

        public bool Remove(ReferenceHub hub)
            => hub != null && m_List.Remove(hub.netId);

        public bool Remove(Player player)
            => player != null && m_List.Remove(player.NetworkId);

        public bool Remove(ExPlayer player)
            => player != null && m_List.Remove(player.NetId);

        public bool Remove(NetworkIdentity identity)
            => identity != null && m_List.Remove(identity.netId);

        public int RemoveWhere(Func<ExPlayer, bool> predicate)
        {
            var count = 0;

            foreach (var player in ExPlayer.Players)
            {
                if (!Contains(player))
                    continue;

                if (!predicate(player))
                    continue;

                if (m_List.Remove(player.NetId))
                    count++;
            }

            return count;
        }

        public bool Contains(uint netId)
            => m_List.Contains(netId);

        public bool Contains(ReferenceHub hub)
            => hub != null && m_List.Contains(hub.netId);

        public bool Contains(Player player)
            => player != null && m_List.Contains(player.NetworkId);

        public bool Contains(ExPlayer player)
            => player != null && m_List.Contains(player.NetId);

        public bool Contains(NetworkIdentity identity)
            => identity != null && m_List.Contains(identity.netId);

        public void ForEach(Action<ExPlayer> action)
        {
            if (action is null)
                return;

            if (m_List.Count < 1)
                return;

            foreach (var player in ExPlayer.Players)
            {
                if (!m_List.Contains(player.NetId))
                    continue;

                action(player);
            }
        }

        public void ForEach(Predicate<ExPlayer> predicate, Action<ExPlayer> action)
        {
            if (action is null || predicate is null)
                return;

            if (m_List.Count < 1)
                return;

            foreach (var player in ExPlayer.Players)
            {
                if (!m_List.Contains(player.NetId))
                    continue;

                if (!predicate(player))
                    continue;

                action(player);
            }
        }

        public void Clear()
            => m_List.Clear();

        public void Dispose()
        {
            m_List.Clear();
            m_Handlers.Remove(this);
        }

        public IEnumerator<ExPlayer> GetEnumerator()
            => new PlayerEnumerator(m_List);

        IEnumerator IEnumerable.GetEnumerator()
            => new PlayerEnumerator(m_List);
    }
}