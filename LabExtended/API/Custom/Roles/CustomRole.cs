using PlayerRoles;

using System.ComponentModel;
using System.Collections.ObjectModel;

using UnityEngine;

using LabApi.Events.Handlers;
using LabApi.Events.Arguments.PlayerEvents;

using LabExtended.Core;
using LabExtended.Utilities;
using LabExtended.Extensions;

using LabExtended.API.Containers;
using LabExtended.API.Custom.Items;

using LabExtended.Core.Configs.Objects;

using YamlDotNet.Serialization;

namespace LabExtended.API.Custom.Roles
{
    /// <summary>
    /// Base class for custom roles.
    /// </summary>
    public abstract class CustomRole : CustomObject<CustomRole>
    {
        #region Config Properties
        /// <summary>
        /// Gets or sets the name of the custom role.
        /// </summary>
        [Description("Sets the name of the custom role.")]
        public virtual string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the maximum health value for the role.
        /// </summary>
        /// <remarks>If the value is set below zero, the default maximum health for the role type will be
        /// used. This property allows customization of health limits for different role instances.</remarks>
        [Description("Sets the maximum health for this role - values below zero will use the default max health for the role type.")]
        public virtual float MaxHealth { get; set; } = -1f;

        /// <summary>
        /// Gets or sets the health value that a player will have upon spawning as this role.
        /// </summary>
        /// <remarks>If the value is less than zero, the default health for the role type will be used.
        /// Setting this property allows customization of the initial health for players when they assume this
        /// role.</remarks>
        [Description("Sets the health a player will spawn with when they spawn as this role - values below zero will use the default health for the role type.")]
        public virtual float SpawnHealth { get; set; } = -1f;

        /// <summary>
        /// Gets or sets the maximum artificial health value for the role.
        /// </summary>
        /// <remarks>If the value is less than zero, the default maximum artificial health for the role
        /// type is used.</remarks>
        [Description("Sets the maximum artificial health for this role - values below zero will use the default max artificial health for the role type.")]
        public virtual float MaxArtificialHealth { get; set; } = -1f;

        /// <summary>
        /// Gets or sets the artificial health value assigned to a player when spawning as this role.
        /// </summary>
        /// <remarks>If the value is less than zero, the default artificial health for the role type is
        /// used. This property allows customization of a player's starting artificial health upon spawn.</remarks>
        [Description("Sets the artificial health a player will spawn with when they spawn as this role - values below zero will use the default artificial health for the role type.")]
        public virtual float SpawnArtificialHealth { get; set; } = -1f;

        /// <summary>
        /// Gets or sets the maximum stamina value for the role.
        /// </summary>
        /// <remarks>If the value is set below zero, the default maximum stamina for the role type is
        /// used. This property allows customization of stamina limits for different roles.</remarks>
        [Description("Sets the maximum stamina for this role - values below zero will use the default max stamina for the role type.")]
        public virtual float MaxStamina { get; set; } = -1f;

        /// <summary>
        /// Gets or sets the stamina value that a player will have upon spawning as this role.
        /// </summary>
        /// <remarks>If the value is less than zero, the default stamina for the role type is
        /// used.</remarks>
        [Description("Sets the stamina a player will spawn with when they spawn as this role - values below zero will use the default stamina for the role type.")]
        public virtual float SpawnStamina { get; set; } = -1f;

        /// <summary>
        /// Gets or sets a value indicating whether the player's inventory is cleared when they spawn as this role.
        /// </summary>
        [Description("Whether or not the player's inventory should be cleared when they spawn as this role.")]
        public virtual bool ClearInventory { get; set; }

        /// <summary>
        /// Gets or sets the type identifier for this role.
        /// </summary>
        [Description("Sets the role the player who spawned as this role will see it as.")]
        public virtual RoleTypeId Type { get; set; } = RoleTypeId.None;

        /// <summary>
        /// Gets or sets the appearance of the role when it is spawned.
        /// </summary>
        [Description("Sets the role other players will see this role as.")]
        public virtual RoleTypeId Appearance { get; set; } = RoleTypeId.None;

        /// <summary>
        /// Gets or sets the scale of the player model when spawning as this role.
        /// </summary>
        [Description("Sets the scale of the player model when they spawn as this role.")]
        public virtual YamlVector3 Scale { get; set; } = new(Vector3.one);

        /// <summary>
        /// Gets or sets the gravity.
        /// </summary>
        [Description("Sets the player's gravity.")]
        public virtual YamlVector3 Gravity { get; set; } = new(PositionContainer.DefaultGravity);

        /// <summary>
        /// Gets or sets the collection of feature toggles that are specific to this role.
        /// </summary>
        [Description("Toggles specific to this role.")]
        public virtual SwitchContainer? Toggles { get; set; }

        /// <summary>
        /// Gets or sets the collection of inventory items assigned to the player when spawning.
        /// </summary>
        [Description("Inventory items to give to the player upon spawning.")]
        public virtual List<ItemType> Items { get; set; } = new();

        /// <summary>
        /// Gets or sets the collection of custom inventory items to assign to the player when spawning.
        /// </summary>
        [Description("Custom inventory items to give to the player upon spawning.")]
        public virtual List<string> CustomItems { get; set; } = new();

        /// <summary>
        /// Gets or sets the list of spawn locations associated with this role. If the list is empty, default spawn
        /// locations for the role type are used.
        /// </summary>
        [Description("A list of spawn locations for this role. If empty, the default spawn locations for the role type will be used.")]
        public virtual List<string> SpawnLocations { get; set; } = new();

        /// <summary>
        /// Gets or sets the collection of configuration effects applied by this instance.
        /// </summary>
        [Description("Effects to apply to the player upon spawning.")]
        public virtual List<ConfigEffect> Effects { get; set; } = new() { new() };

        /// <summary>
        /// Gets or sets the collection of ammunition counts for each item type.
        /// </summary>
        [Description("Base game ammo types and their corresponding amounts.")]
        public virtual Dictionary<ItemType, ushort> Ammo { get; set; } = new();

        /// <summary>
        /// Gets or sets a collection of custom ammunition types and their corresponding quantities.
        /// </summary>
        [Description("Custom ammo types and their corresponding amounts.")]
        public virtual Dictionary<string, int> CustomAmmo { get; set; } = new();
        #endregion

        private Dictionary<ExPlayer, object> players = new();

        /// <summary>
        /// Gets the read-only list of players currently assigned to this custom role.
        /// </summary>
        [YamlIgnore]
        public ReadOnlyDictionary<ExPlayer, object> Players => field ??= new ReadOnlyDictionary<ExPlayer, object>(players);

        /// <summary>
        /// Determines the role appearance that should be presented for a player from the perspective of a specific
        /// receiver.
        /// </summary>
        /// <param name="player">The player whose appearance is being evaluated.</param>
        /// <param name="receiver">The player who will perceive the appearance of the specified player.</param>
        /// <param name="appearance">The default role appearance to use if no override is specified.</param>
        /// <param name="data">An optional data object that can be used to provide additional context or receive extra information. This
        /// parameter is passed by reference and may be modified by the method.</param>
        /// <returns>A value indicating the role appearance that should be shown to the receiver for the specified player.</returns>
        public virtual RoleTypeId GetAppearance(ExPlayer player, ExPlayer receiver, RoleTypeId appearance, ref object? data)
        {
            if (Appearance == RoleTypeId.None)
                return appearance;

            return Appearance;
        }

        /// <summary>
        /// Determines whether the specified player currently has this custom role assigned.
        /// </summary>
        /// <param name="player">The player to check for the assigned custom role. Cannot be null.</param>
        /// <returns>true if the player has this custom role assigned; otherwise, false.</returns>
        public bool HasRole(ExPlayer player)
            => player?.ReferenceHub != null 
            && player.Role.CustomRole != null 
            && player.Role.CustomRole == this;

        /// <summary>
        /// Determines whether the specified player has an assigned role and provides associated custom role data.
        /// </summary>
        /// <param name="player">The player whose role membership is to be checked. Cannot be null.</param>
        /// <param name="data">When this method returns, contains the custom role data associated with the player's role, or null if no
        /// data is available.</param>
        /// <returns>true if the player has an assigned role; otherwise, false.</returns>
        public bool HasRole(ExPlayer player, out object? data)
        {
            data = player.Role.customRoleData;
            return HasRole(player);
        }

        /// <summary>
        /// Determines whether the specified player has a role that provides data of the given type.
        /// </summary>
        /// <typeparam name="T">The type of data to retrieve from the player's role.</typeparam>
        /// <param name="player">The player whose role is evaluated for the specified data type.</param>
        /// <param name="data">When this method returns, contains the data of type <typeparamref name="T"/> associated with the player's
        /// role, if available; otherwise, the default value for the type.</param>
        /// <returns>true if the player's role contains data of type <typeparamref name="T"/>; otherwise, false.</returns>
        public bool HasRole<T>(ExPlayer player, out T data)
        {
            data = default!;

            if (!HasRole(player))
                return false;

            if (player.Role.TryGetData<T>(out var customData))
                data = customData;

            return true;
        }

        /// <summary>
        /// Assigns this custom role to the specified player, optionally associating additional data with the role
        /// assignment.
        /// </summary>
        /// <remarks>If the player already has a custom role, it will be removed before assigning this
        /// role. The method will not assign the role if the player or their reference hub is null.</remarks>
        /// <param name="player">The player to whom the custom role will be assigned. Cannot be null and must have a valid reference hub.</param>
        /// <param name="data">Optional data to associate with the player's custom role. This can be any object relevant to the role's
        /// behavior, or null if no additional data is needed.</param>
        /// <returns>true if the custom role was successfully assigned to the player; otherwise, false.</returns>
        public bool Give(ExPlayer player, object? data = null)
        {
            if (player?.ReferenceHub == null)
                return false;

            if (player.Role.CustomRole != null)
                player.Role.CustomRole.Remove(player);

            player.Role.CustomRole = this;
            player.Role.customRoleData = data;

            players.Add(player, data!);

            OnAdded(player, ref player.Role.customRoleData);
            return true;
        }

        /// <summary>
        /// Removes the specified player from this custom role if they are currently assigned to it.
        /// </summary>
        /// <remarks>If the player does not have this custom role assigned or is invalid, the method
        /// returns false and no changes are made. After removal, the player's custom role data is cleared.</remarks>
        /// <param name="player">The player to remove from the custom role. Must not be null and must currently have this custom role
        /// assigned.</param>
        /// <returns>true if the player was successfully removed from the custom role; otherwise, false.</returns>
        public bool Remove(ExPlayer player)
        {
            if (player?.ReferenceHub == null)
                return false;

            if (player.Role.CustomRole == null || player.Role.CustomRole != this)
                return false;

            players.Remove(player);

            OnRemoved(player, ref player.Role.customRoleData);

            player.Role.CustomRole = null;
            player.Role.customRoleData = null;

            return true;
        }

        /// <summary>
        /// Spawns the specified player with the custom role, initializing their inventory, stats, position, and effects
        /// according to the role's configuration.
        /// </summary>
        /// <remarks>If no valid spawn locations are available, the player will be spawned using default
        /// settings. Inventory, stats, and effects are set based on the role's configuration. The method does not throw
        /// exceptions for invalid input; instead, it returns false if the player or their ReferenceHub is
        /// null.</remarks>
        /// <param name="player">The player to spawn with the custom role. Must not be null and must have a valid ReferenceHub.</param>
        /// <param name="useSpawnpoint">Whether or not to assign a spawn position.</param>
        /// <param name="addInventory">Whether or not to add vanilla inventory items to the player. Will be overriden by ClearInventory if both are true.</param>
        /// <param name="data">A reference to an optional data object that can be used to pass or receive custom spawn-related information.</param>
        /// <returns>true if the player was successfully spawned with the custom role; otherwise, false.</returns>
        public virtual bool Spawn(ExPlayer player, bool useSpawnpoint, bool addInventory, ref object? data)
        {
            if (player?.ReferenceHub == null)
                return false;

            var validSpawnPoints = useSpawnpoint ? null : SpawnLocations.Where(x => MapUtilities.TryGet(x, null, out _, out _));
            var useDefaultSpawn = useSpawnpoint || validSpawnPoints?.Count() < 1;

            player.Role.Set(Type, RoleChangeReason.RemoteAdmin, 
                useDefaultSpawn ? (ClearInventory || !addInventory
                                    ? RoleSpawnFlags.UseSpawnpoint
                                    : RoleSpawnFlags.All)
                                : (ClearInventory || !addInventory
                                    ? RoleSpawnFlags.None
                                    : RoleSpawnFlags.AssignInventory));

            void AddCustomItem(string customItem)
            {
                if (!CustomItem.TryGet(customItem, out var item))
                    return;

                item.AddItem(player);
            }

            TimingUtils.AfterFrames(() =>
            {
                if (Scale.Vector != Vector3.one)
                    player.Scale = Scale.Vector;

                if (Gravity.Vector != PositionContainer.DefaultGravity)
                    player.Position.Gravity = Gravity.Vector;

                if (MaxHealth > 0f)
                    player.Stats.MaxHealth = MaxHealth;

                if (MaxStamina > 0f)
                    player.Stats.MaxStamina = MaxStamina;

                if (MaxArtificialHealth > 0f)
                    player.Stats.MaxAhp = MaxArtificialHealth;

                if (SpawnHealth > 0f)
                {
                    if (player.Stats.MaxHealth < SpawnHealth)
                        player.Stats.MaxHealth = SpawnHealth;

                    player.Stats.CurHealth = SpawnHealth;
                }

                if (SpawnStamina > 0f)
                {
                    if (player.Stats.MaxStamina < SpawnStamina)
                        player.Stats.MaxStamina = SpawnStamina;

                    player.Stats.MaxStamina = SpawnStamina;
                }

                if (SpawnArtificialHealth < 0f)
                {
                    if (player.Stats.MaxAhp < SpawnArtificialHealth)
                        player.Stats.MaxAhp = SpawnArtificialHealth;

                    player.Stats.CurAhp = SpawnArtificialHealth;
                }

                if (ClearInventory)
                {
                    player.Inventory.Clear();

                    player.Ammo.ClearAmmo();
                    player.Ammo.ClearCustomAmmo();
                }

                Items.ForEach(item => player.Inventory.AddItem(item));
                Ammo.ForEach(pair => player.Ammo.AddAmmo(pair.Key, pair.Value));

                CustomItems.ForEach(item => AddCustomItem(item));
                CustomAmmo.ForEach(pair => player.Ammo.AddCustomAmmo(pair.Key, pair.Value));

                Effects.ForEach(effect => effect.Apply(player));

                if (!useDefaultSpawn && useSpawnpoint)
                {
                    var spawnPointName = validSpawnPoints!.GetRandomItem();

                    if (MapUtilities.TryGet(spawnPointName, null, out var position, out var rotation))
                    {
                        player.Position.Position = position;
                        player.Rotation.Rotation = rotation;
                    }
                    else
                    {
                        ApiLog.Warn("LabExtended", $"Could not find spawn-point &3{spawnPointName}&r while spawning custom role &3{Id}&r!");
                    }
                }

                OnSpawned(player, ref player.Role.customRoleData);
            }, 1);

            return true;
        }

        /// <summary>
        /// Gets called when the role is added to a player.
        /// </summary>
        /// <remarks>Make sure to call the base method (<c>base.OnAdded(player, ref data)</c>) in order to spawn the player and apply the config 
        /// properties of the role!</remarks>
        public virtual void OnAdded(ExPlayer player, ref object? data) 
            => Spawn(player, true, true, ref data);

        /// <summary>
        /// Gets called when the role is removed from a player.
        /// </summary>
        public virtual void OnRemoved(ExPlayer player, ref object? data) { }

        /// <summary>
        /// Gets called when the player gets fully spawned.
        /// </summary>
        public virtual void OnSpawned(ExPlayer player, ref object? data) { }

        /// <summary>
        /// Gets called when the player's info area is being built.
        /// </summary>
        public virtual void OnBuildingInfo(ExPlayer player, ref object? data) { }

        /// <summary>
        /// Gets called before the player's role is changed.
        /// </summary>
        public virtual void OnChangingRole(PlayerChangingRoleEventArgs args, ref object? data) { }

        /// <summary>
        /// Gets called after the player's role has been changed.
        /// </summary>
        /// <remarks>Make sure to call the base method (<c>base.OnChangedRole(args, ref data)</c>) in order to remove the role!</remarks>
        public virtual void OnChangedRole(PlayerChangedRoleEventArgs args, ref object? data)
        {
            if (args.Player is not ExPlayer player)
                return;

            Remove(player);
        }

        /// <summary>
        /// Gets called before this player deals damage to another player.
        /// </summary>
        public virtual void OnAttacking(PlayerHurtingEventArgs args, ref object? data) { }

        /// <summary>
        /// Gets called after this player has dealt damage to another player.
        /// </summary>
        public virtual void OnAttacked(PlayerHurtEventArgs args, ref object? data) { }

        /// <summary>
        /// Gets called before this player receives damage.
        /// </summary>
        public virtual void OnHurting(PlayerHurtingEventArgs args, ref object? data) { }

        /// <summary>
        /// Gets called after damage has been applied to this player.
        /// </summary>
        public virtual void OnHurt(PlayerHurtEventArgs args, ref object? data) { }

        /// <summary>
        /// Gets called before this player kills another player.
        /// </summary>
        public virtual void OnKilling(PlayerDyingEventArgs args, ref object? data) { }

        /// <summary>
        /// Gets called after this player has killed another player.
        /// </summary>
        public virtual void OnKilled(PlayerDeathEventArgs args, ref object? data) { }

        /// <summary>
        /// Gets called before this player dies.
        /// </summary>
        public virtual void OnDying(PlayerDyingEventArgs args, ref object? data) { }

        /// <summary>
        /// Gets called after this player has died.
        /// </summary>
        public virtual void OnDied(PlayerDeathEventArgs args, ref object? data) { }
        
        private static void _OnChangingRole(PlayerChangingRoleEventArgs args)
        {
            if (args.Player is ExPlayer player && player.Role.CustomRole is not null)
                player.Role.CustomRole.OnChangingRole(args, ref player.Role.customRoleData);
        }

        private static void _OnHurting(PlayerHurtingEventArgs args)
        {
            if (args.Player is ExPlayer player && player.Role.CustomRole is not null)
                player.Role.CustomRole.OnHurting(args, ref player.Role.customRoleData);

            if (args.Attacker is ExPlayer attacker && attacker.Role.CustomRole is not null)
                attacker.Role.CustomRole.OnAttacking(args, ref attacker.Role.customRoleData);
        }

        private static void _OnHurt(PlayerHurtEventArgs args)
        {
            if (args.Player is ExPlayer player && player.Role.CustomRole is not null)
                player.Role.CustomRole.OnHurt(args, ref player.Role.customRoleData);

            if (args.Attacker is ExPlayer attacker && attacker.Role.CustomRole is not null)
                attacker.Role.CustomRole.OnAttacked(args, ref attacker.Role.customRoleData);
        }

        private static void _OnDying(PlayerDyingEventArgs args)
        {
            if (args.Player is ExPlayer player && player.Role.CustomRole is not null)
                player.Role.CustomRole.OnDying(args, ref player.Role.customRoleData);

            if (args.Attacker is ExPlayer attacker && attacker.Role.CustomRole is not null)
                attacker.Role.CustomRole.OnKilling(args, ref attacker.Role.customRoleData);
        }

        private static void _OnDied(PlayerDeathEventArgs args)
        {
            if (args.Player is ExPlayer player && player.Role.CustomRole is not null)
                player.Role.CustomRole.OnDied(args, ref player.Role.customRoleData);

            if (args.Attacker is ExPlayer attacker && attacker.Role.CustomRole is not null)
                attacker.Role.CustomRole.OnKilled(args, ref attacker.Role.customRoleData);
        }
        
        internal static void Initialize()
        {
            PlayerEvents.ChangingRole += _OnChangingRole;

            PlayerEvents.Hurting += _OnHurting;
            PlayerEvents.Hurt += _OnHurt;

            PlayerEvents.Dying += _OnDying;
            PlayerEvents.Death += _OnDied;
        }
    }
}