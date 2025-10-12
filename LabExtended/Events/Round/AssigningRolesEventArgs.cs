using LabExtended.API;

using PlayerRoles;

namespace LabExtended.Events.Round
{
    /// <summary>
    /// Gets called when the server starts assigning roles when the round starts.
    /// </summary>
    public class AssigningRolesEventArgs : BooleanEventArgs
    {
        /// <summary>
        /// A dictionary of players and their decided roles.
        /// </summary>
        public Dictionary<ExPlayer, RoleTypeId> Roles { get; }

        /// <summary>
        /// Creates a new <see cref="AssigningRolesEventArgs"/> instance.
        /// </summary>
        /// <param name="roles">The assigned roles.</param>
        public AssigningRolesEventArgs(Dictionary<ExPlayer, RoleTypeId> roles)
            => Roles = roles;

        /// <summary>
        /// Sets the role for the specified player if the player exists in the collection.
        /// </summary>
        /// <param name="player">The player whose role is to be set. Cannot be null.</param>
        /// <param name="role">The role to assign to the specified player.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="player"/> is null.</exception>
        public void Set(ExPlayer player, RoleTypeId role)
        {
            if (player is null)
                throw new ArgumentNullException(nameof(player));

            if (!Roles.ContainsKey(player))
                return;

            Roles[player] = role;
        }

        /// <summary>
        /// Sets the role for all players by applying the specified selector function to each player.
        /// </summary>
        /// <param name="selector">A function that takes an ExPlayer and returns the RoleTypeId to assign to that player.</param>
        /// <exception cref="ArgumentNullException">Thrown if selector is null.</exception>
        public void SetAll(Func<ExPlayer, RoleTypeId> selector)
        {
            if (selector is null)
                throw new ArgumentNullException(nameof(selector));

            foreach (var player in ExPlayer.Players)
            {
                if (!Roles.ContainsKey(player))
                    continue;

                Roles[player] = selector(player);
            }
        }

        /// <summary>
        /// Sets the specified role for all players that match the given predicate.
        /// </summary>
        /// <param name="predicate">A predicate used to determine which players should have their role set. The method applies the role to each
        /// player for whom this predicate returns <see langword="true"/>.</param>
        /// <param name="role">The role to assign to each player that matches the predicate.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="predicate"/> is <see langword="null"/>.</exception>
        public void SetWhere(Predicate<ExPlayer> predicate, RoleTypeId role)
        {
            if (predicate is null)
                throw new ArgumentNullException(nameof(predicate));

            foreach (var player in ExPlayer.Players)
            {
                if (!Roles.ContainsKey(player))
                    continue;

                if (!predicate(player))
                    continue;

                Roles[player] = role;
            }
        }
    }
}