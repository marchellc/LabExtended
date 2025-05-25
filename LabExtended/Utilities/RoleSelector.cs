using LabExtended.API;
using LabExtended.Core.Pooling.Pools;

using NorthwoodLib.Pools;

using PlayerRoles;
using PlayerRoles.RoleAssign;

using UnityEngine;

using Random = UnityEngine.Random;

// ReSharper disable ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace LabExtended.Utilities;

/// <summary>
/// Utility that selects roles for players.
/// </summary>
public static class RoleSelector
{
    /// <summary>
    /// Context for role selection.
    /// </summary>
    public class RoleSelectorContext : IDisposable
    {
        /// <summary>
        /// Gets the human team queue.
        /// </summary>
        public Team[] HumanQueue;

        /// <summary>
        /// Gets the total role queue.
        /// </summary>
        public Team[] TotalQueue;

        /// <summary>
        /// The human role queue.
        /// </summary>
        public RoleTypeId[] HumanRoles;

        /// <summary>
        /// Whether or not SCPs are overflowing.
        /// </summary>
        public bool ScpsOverflowing;

        /// <summary>
        /// Whether or not to register human role history.
        /// </summary>
        public bool RegisterRoleHistory;

        /// <summary>
        /// Whether or not to modify SCP tickets.
        /// </summary>
        public bool ModifyScpTickets = false;

        /// <summary>
        /// The overflow Hume Shield multiplier.
        /// </summary>
        public float ScpOverflowHsMultiplier = 0f;
        
        /// <summary>
        /// The source player list.
        /// </summary>
        public List<ExPlayer> Players = ListPool<ExPlayer>.Shared.Rent();

        /// <summary>
        /// List of generated SCP players.
        /// </summary>
        public List<ExPlayer> ScpPlayers = ListPool<ExPlayer>.Shared.Rent();

        /// <summary>
        /// List of human player candidates.
        /// </summary>
        public List<ExPlayer> HumanPlayers = ListPool<ExPlayer>.Shared.Rent();
        
        /// <summary>
        /// List of enqueued SCP roles.
        /// </summary>
        public List<RoleTypeId> EnqueuedScps = ListPool<RoleTypeId>.Shared.Rent();

        /// <summary>
        /// The target result dictionary.
        /// </summary>
        public IDictionary<ExPlayer, RoleTypeId> Target;

        /// <summary>
        /// Buffer for SCP player chances.
        /// </summary>
        public Dictionary<ExPlayer, float> ScpChancesBuffer = DictionaryPool<ExPlayer, float>.Shared.Rent();

        /// <summary>
        /// Buffer for selected SCP player chances.
        /// </summary>
        public Dictionary<ExPlayer, float> SelectedScpChancesBuffer = DictionaryPool<ExPlayer, float>.Shared.Rent();

        /// <inheritdoc cref="IDisposable.Dispose"/>
        public void Dispose()
        {
            if (Players != null)
                ListPool<ExPlayer>.Shared.Return(Players);
            
            if (ScpPlayers != null)
                ListPool<ExPlayer>.Shared.Return(ScpPlayers);
            
            if (HumanPlayers != null)
                ListPool<ExPlayer>.Shared.Return(HumanPlayers);

            if (EnqueuedScps != null)
                ListPool<RoleTypeId>.Shared.Return(EnqueuedScps);
            
            if (ScpChancesBuffer != null)
                DictionaryPool<ExPlayer, float>.Shared.Return(ScpChancesBuffer);
            
            if (SelectedScpChancesBuffer != null)
                DictionaryPool<ExPlayer, float>.Shared.Return(SelectedScpChancesBuffer);

            Target = null;
            Players = null;
            ScpPlayers = null;
            HumanRoles = null;
            TotalQueue = null;
            HumanQueue = null;
            HumanPlayers = null;
            EnqueuedScps = null;
            ScpChancesBuffer = null;
            SelectedScpChancesBuffer = null;
            
            ScpsOverflowing = false;
            ModifyScpTickets = false;
            RegisterRoleHistory = false;
            
            ScpOverflowHsMultiplier = 0f;
        }
    }
    
    /// <summary>
    /// Fills the dictionary with a round-start role selection.
    /// </summary>
    /// <param name="target">The target dictionary.</param>
    /// <param name="sourcePlayers">List of players to select from.</param>
    /// <param name="allowScpOverflow">Whether or not to allow SCP roles to overflow.</param>
    /// <param name="registerHumanHistory">Whether or not to register selected human roles to role history.</param>
    /// <param name="modifyScpTickets">Whether or not to modify player SCP tickets.</param>
    /// <param name="respawnQueue">Team respawn queue string.</param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="Exception"></exception>
    public static void GetRolesNonAlloc(IDictionary<ExPlayer, RoleTypeId> target, IEnumerable<ExPlayer> sourcePlayers, bool allowScpOverflow = false, 
        bool registerHumanHistory = false, bool modifyScpTickets = false, string respawnQueue = "4014314031441404134041434414")
    {
        if (target is null)
            throw new ArgumentNullException(nameof(target));

        if (sourcePlayers is null)
            throw new ArgumentNullException(nameof(sourcePlayers));
        
        if (string.IsNullOrWhiteSpace(respawnQueue))
            throw new ArgumentNullException(nameof(respawnQueue));

        using (var context = new RoleSelectorContext())
        {
            context.Target = target;
            context.RegisterRoleHistory = registerHumanHistory;
            
            SelectPlayers(context, sourcePlayers);

            context.TotalQueue = new Team[respawnQueue.Length];
            context.HumanQueue = new Team[respawnQueue.Length];

            var totalIndex = 0;
            var humanIndex = 0;

            for (var i = 0; i < respawnQueue.Length; i++)
            {
                var teamNum = (Team)(respawnQueue[i] - '0');

                if (Enum.IsDefined(typeof(Team), teamNum))
                {
                    if (teamNum != Team.SCPs)
                    {
                        context.HumanQueue[humanIndex++] = teamNum;
                    }
                    else
                    {
                        context.TotalQueue[totalIndex++] = teamNum;
                    }
                }
            }

            if (totalIndex == 0)
                throw new Exception("Failed to assign roles, queue has failed to load.");

            var maxScps = ScpSpawner.MaxSpawnableScps;
            var scpCount = 0;
            var playerCount = context.Players.Count;

            for (var i = 0; i < playerCount; i++)
            {
                if (context.TotalQueue[i % totalIndex] == Team.SCPs)
                {
                    scpCount++;

                    if (scpCount == maxScps && !allowScpOverflow)
                    {
                        context.ScpOverflowHsMultiplier = 1f + (playerCount - i) * 0.05f;
                        context.ScpsOverflowing = true;

                        break;
                    }
                }
            }

            SelectScps(context, scpCount);
            SelectHumans(context, humanIndex);
        }
    }

    private static void SelectPlayers(RoleSelectorContext ctx, IEnumerable<ExPlayer> sourcePlayers)
    {
        ctx.Players.Clear();
        
        foreach (var player in sourcePlayers)
        {
            if (player is null || player.IsUnverified)
                continue;
            
            if (!RoleAssigner.CheckPlayer(player.ReferenceHub))
                continue;
            
            if (ctx.Players.Contains(player))
                continue;
            
            ctx.Players.Add(player);
        }
    }

    private static void SelectHumans(RoleSelectorContext ctx, int humanCount)
    {
        HumanSpawner._humanQueue = ctx.HumanQueue;
        
        HumanSpawner._queueClock = 0;
        HumanSpawner._queueLength = humanCount;

        ctx.HumanRoles = new RoleTypeId[ctx.Players.Count];

        for (var i = 0; i < ctx.Players.Count; i++)
            ctx.HumanRoles[i] = HumanSpawner.NextHumanRoleToSpawn;
        
        ctx.HumanRoles.ShuffleList();

        var count = ctx.Players.Count;
        
        for (var i = 0; i < count; i++)
            AssignHuman(ctx, ctx.HumanRoles[i]);
    }

    private static void AssignHuman(RoleSelectorContext ctx, RoleTypeId role)
    {
        ctx.HumanPlayers.Clear();

        var num = int.MaxValue;

        for (var i = 0; i < ctx.Players.Count; i++)
        {
            var player = ctx.Players[i];
            var history = HumanSpawner.History.GetOrAdd(player.UserId, () => new());
            var count = 0;

            for (var x = 0; x < 5; x++)
            {
                if (history.History[x] == role)
                {
                    count++;
                }
            }

            if (count <= num)
            {
                if (count < num)
                    ctx.HumanPlayers.Clear();
                
                ctx.HumanPlayers.Add(player);

                num = count;
            }
        }

        if (ctx.HumanPlayers.Count == 0)
            return;

        var randomPlayer = ctx.HumanPlayers.RandomItem();

        ctx.Players.Remove(randomPlayer);
        ctx.Target[randomPlayer] = role;
        
        if (ctx.RegisterRoleHistory)
            HumanSpawner.History[randomPlayer.UserId].RegisterRole(role);
    }

    #region Scp Selection
    private static void SelectScps(RoleSelectorContext ctx, int scpCount)
    {
        ctx.EnqueuedScps.Clear();

        for (var i = 0; i < scpCount; i++)
            ctx.EnqueuedScps.Add(ScpSpawner.NextScp);
        
        SelectScpPlayers(ctx, scpCount);

        while (ctx.EnqueuedScps.Count > 0)
        {
            var scp = ctx.EnqueuedScps[0];
            
            ctx.EnqueuedScps.RemoveAt(0);
            
            AssignScp(ctx, scp);
        }
    }
    
    private static void SelectScpPlayers(RoleSelectorContext ctx, int scpCount)
    {
	    using (var tickerLoader = new ScpTicketsLoader())
	    {
		    GenerateScps(ctx, tickerLoader, scpCount);

            if (ctx.ModifyScpTickets)
            {
                for (var i = 0; i < ExPlayer.Count; i++)
                {
                    var player = ExPlayer.Players[i];

                    if (RoleAssigner.CheckPlayer(player.ReferenceHub) &&
                        !ScpPlayerPicker.IsOptedOutOfScp(player.ReferenceHub))
                        tickerLoader.ModifyTickets(player.ReferenceHub,
                            tickerLoader.GetTickets(player.ReferenceHub, 10) + 2);
                }
            }
        }
	    
	    if (scpCount != ScpPlayerPicker.ScpsToSpawn.Count)
		    throw new InvalidOperationException("Failed to meet target number of SCPs.");
    }

    private static void GenerateScps(RoleSelectorContext ctx, ScpTicketsLoader loader, int scpCount)
    {
        ctx.ScpPlayers.Clear();

        if (scpCount < 1)
            return;

        var num = 0;
        
        for (var i = 0; i < ctx.Players.Count; i++)
        {
            var player = ctx.Players[i];
            var tickets = loader.GetTickets(player.ReferenceHub, 10);

            if (tickets >= num)
            {
                if (tickets > num)
                    ctx.ScpPlayers.Clear();

                num = tickets;
                
                ctx.ScpPlayers.Add(player);
            }
        }

        if (ctx.ScpPlayers.Count > 1)
        {
            var randomPlayer = ctx.ScpPlayers.RandomItem();
            
            ctx.ScpPlayers.Clear();
            ctx.ScpPlayers.Add(randomPlayer);
        }

        scpCount -= ctx.ScpPlayers.Count;

        if (scpCount < 1)
            return;

        var potentialScps = ListPool<KeyValuePair<ExPlayer, long>>.Shared.Rent();
        var weight = 0L;

        for (var i = 0; i < ctx.Players.Count; i++)
        {
            var player = ctx.Players[i];

            if (!ctx.ScpPlayers.Contains(player))
            {
                var playerWeight = 1L;
                var playerTickets = loader.GetTickets(player.ReferenceHub, 10);

                for (var x = 0; x < scpCount; x++)
                    playerWeight *= playerTickets;
                
                potentialScps.Add(new(player, playerWeight));
                
                weight += playerWeight;
            }
        }

        if (scpCount < 1)
        {
            ListPool<KeyValuePair<ExPlayer, long>>.Shared.Return(potentialScps);
            return;
        }

        var randomWeight = weight * Random.value;

        for (var i = 0; i < potentialScps.Count; i++)
        {
            var pair = potentialScps[i];

            randomWeight -= pair.Value;

            if (randomWeight <= 0.0)
            {
                scpCount--;
                
                ctx.ScpPlayers.Add(pair.Key);
                
                potentialScps.RemoveAt(i);

                weight -= pair.Value;
                break;
            }
        }
        
        ListPool<KeyValuePair<ExPlayer, long>>.Shared.Return(potentialScps);
    }

    private static void AssignScp(RoleSelectorContext ctx, RoleTypeId scpRole)
    {
        ctx.ScpChancesBuffer.Clear();

        var num = 1;
        var num2 = 0;

        for (var i = 0; i < ctx.ScpPlayers.Count; i++)
        {
            var player = ctx.ScpPlayers[i];
            var rolePreference = ScpSpawner.GetPreferenceOfPlayer(player.ReferenceHub, scpRole);

            for (var x = 0; x < ctx.EnqueuedScps.Count; x++)
                rolePreference -= ScpSpawner.GetPreferenceOfPlayer(player.ReferenceHub, ctx.EnqueuedScps[x]);

            num2++;

            ctx.ScpChancesBuffer[player] = rolePreference;
            
            num = Mathf.Min(rolePreference, num);
        }
        
        ctx.SelectedScpChancesBuffer.Clear();

        var totalChance = 0f;
        
        foreach (var pair in ctx.ScpChancesBuffer)
        {
            var chance = Mathf.Pow(pair.Value - num + 1f, num2);

            ctx.SelectedScpChancesBuffer[pair.Key] = chance;

            totalChance += chance;
        }

        var randomChance = totalChance * Random.value;
        var weight = 0f;

        foreach (var pair in ctx.SelectedScpChancesBuffer)
        {
            weight += pair.Value;

            if (pair.Value >= randomChance)
            {
                ctx.Players.Remove(pair.Key);
                ctx.ScpPlayers.Remove(pair.Key);

                ctx.Target[pair.Key] = scpRole;
                break;
            }
        }
    }
    #endregion
}