using InventorySystem.Items;

using LabExtended.Events;
using LabExtended.Extensions;
using LabExtended.Attributes;

using PlayerRoles;

using DelegateExtensions = LabExtended.Extensions.DelegateExtensions;

namespace LabExtended.API.CustomRoles;

/// <summary>
/// A class used to manage custom roles.
/// </summary>
public static class CustomRoleManager
{
    /// <summary>
    /// Gets all registered custom roles.
    /// </summary>
    public static Dictionary<Type, CustomRoleData> RegisteredRoles { get; } = new();

    /// <summary>
    /// Gets all active custom roles.
    /// </summary>
    public static Dictionary<Type, List<CustomRoleInstance>> ActiveRoles { get; } = new();

    /// <summary>
    /// Registers a new custom role.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static void RegisterRole<T>(Action<CustomRoleBuilder> builder) where T : CustomRoleInstance, new()
    {
        if (builder is null)
            throw new ArgumentNullException(nameof(builder));

        var instance = new CustomRoleBuilder()
            .WithClass<T>();
        
        builder(instance);
        
        RegisterRole(instance);
    }
    
    /// <summary>
    /// Registers a new custom role.
    /// </summary>
    /// <param name="builder">The builder to use.</param>
    public static void RegisterRole(CustomRoleBuilder builder)
        => RegisterRole(builder.Data);
    
    /// <summary>
    /// Registers a new custom role.
    /// </summary>
    /// <param name="data">Data of the custom role to register.</param>
    public static void RegisterRole(CustomRoleData data)
    {
        if (data is null)
            throw new ArgumentNullException(nameof(data));
        
        if (data.Type is null)
            throw new ArgumentException($"Type cannot be null.", nameof(data));
        
        if (string.IsNullOrWhiteSpace(data.Name))
            throw new ArgumentException($"Name cannot be null or whitespace.", nameof(data));
        
        if (string.IsNullOrWhiteSpace(data.Description))
            throw new ArgumentException($"Description cannot be null or whitespace.", nameof(data));
        
        if (data.RealType is RoleTypeId.None || data.RealType is RoleTypeId.Destroyed)
            throw new ArgumentException($"RealType is invalid.", nameof(data));
        
        if (RegisteredRoles.ContainsKey(data.Type))
            throw new ArgumentException($"Type {data.Type.FullName} already registered.", nameof(data));
        
        RegisteredRoles.Add(data.Type, data);
    }

    /// <summary>
    /// Unregisters a custom role.
    /// </summary>
    /// <typeparam name="T">Type of the custom role.</typeparam>
    public static void UnregisterRole<T>() where T : CustomRoleInstance, new()
        => UnregisterRole(typeof(T));

    /// <summary>
    /// Unregisters a custom role.
    /// </summary>
    /// <param name="type">Type of the custom role.</param>
    public static void UnregisterRole(Type type)
    {
        if (type is null)
            throw new ArgumentNullException(nameof(type));
        
        if (!RegisteredRoles.Remove(type))
            return;

        if (ActiveRoles.TryGetValue(type, out var activeRoles))
        {
            activeRoles.ForEach(role =>
            {
                role.Owner?.customRoles?.Remove(type);
                
                if (role.IsActive)
                {
                    role.IsActive = false;
                    role.OnDisabled();
                }
                
                role.Dispose();
            });
            
            activeRoles.Clear();
        }
        
        ActiveRoles.Remove(type);
    }

    /// <summary>
    /// Enables a custom role of a specific type.
    /// </summary>
    /// <param name="player">The player to enable this role for.</param>
    /// <param name="useSpawnPoint">Whether or not to use spawn point.</param>
    /// <typeparam name="T">The type of the role to enable.</typeparam>
    /// <returns>true if the role was successfully enabled.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static bool EnableCustomRole<T>(this ExPlayer player, bool useSpawnPoint = true) where T : CustomRoleInstance
    {
        return EnableCustomRole(player, typeof(T), useSpawnPoint);
    }
    
    /// <summary>
    /// Enables a custom role of a specific type.
    /// </summary>
    /// <param name="player">The player to enable this role for.</param>
    /// <param name="roleType">The type of the role to enable.</param>
    /// <param name="useSpawnPoint">Whether or not to use spawn point.</param>
    /// <returns>true if the role was successfully enabled.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static bool EnableCustomRole(this ExPlayer player, Type roleType, bool useSpawnPoint = true)
    {
        if (player is null)
            throw new ArgumentNullException(nameof(player));
        
        if (roleType is null)
            throw new ArgumentNullException(nameof(roleType));

        if (!player.customRoles.TryGetValue(roleType, out var role))
            return false;

        if (role.IsActive)
            return false;
        
        if (!ActiveRoles.TryGetValue(roleType, out var activeRoles))
            ActiveRoles.Add(roleType, activeRoles = new());
            
        if (!activeRoles.Contains(role))
            activeRoles.Add(role);
            
        SetToRole(player, role, useSpawnPoint);
            
        role.OnEnabled();
        role.IsActive = true;

        return true;
    }

    /// <summary>
    /// Disables a custom role for a player.
    /// </summary>
    /// <param name="player">The player to disable the custom role of.</param>
    /// <param name="setToSpectator">Whether or not to set the player's role to spectator if the custom role is active.</param>
    /// <typeparam name="T">The type of the custom role to disable.</typeparam>
    /// <returns>true if the custom role was successfully disabled.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static bool DisableCustomRole<T>(this ExPlayer player, bool setToSpectator = false) where T : CustomRoleInstance
    {
        return DisableCustomRole(player, typeof(T), setToSpectator);
    }
    
    /// <summary>
    /// Disables a custom role for a player.
    /// </summary>
    /// <param name="player">The player to disable the custom role of.</param>
    /// <param name="roleType">The type of the custom role to disable.</param>
    /// <param name="setToSpectator">Whether or not to set the player's role to spectator if the custom role is active.</param>
    /// <returns>true if the custom role was successfully disabled.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static bool DisableCustomRole(this ExPlayer player, Type roleType, bool setToSpectator = false)
    {
        if (player is null)
            throw new ArgumentNullException(nameof(player));
        
        if (roleType is null)
            throw new ArgumentNullException(nameof(roleType));
        
        if (!player.customRoles.TryGetValue(roleType, out var role))
            return false;

        if (!role.IsActive) 
            return false;
        
        if (ActiveRoles.TryGetValue(roleType, out var activeRoles))
            activeRoles.Remove(role);
            
        role.IsActive = false;
        role.OnDisabled();
            
        if (setToSpectator)
            player.Role.Set(RoleTypeId.Spectator);

        return true;

    }

    /// <summary>
    /// Adds a custom role to a specific player.
    /// </summary>
    /// <param name="player">The player to add the role to.</param>
    /// <param name="enableRole">Whether or not to enable the role as well.</param>
    /// <param name="useSpawnPoint">Whether or not to use spawn point.</param>
    /// <typeparam name="T">The type of the role to add.</typeparam>
    /// <returns>The instantiated custom role instance.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    public static T AddCustomRole<T>(this ExPlayer player, bool enableRole = false, bool useSpawnPoint = true) where T : CustomRoleInstance
    {
        return (T)AddCustomRole(player, typeof(T), enableRole, useSpawnPoint);
    }

    /// <summary>
    /// Adds a custom role to a specific player.
    /// </summary>
    /// <param name="player">The player to add the role to.</param>
    /// <param name="roleType">The type of the role to add.</param>
    /// <param name="enableRole">Whether or not to enable the role as well.</param>
    /// <param name="useSpawnPoint">Whether or not to use spawn point.</param>
    /// <returns>The instantiated custom role instance.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    public static CustomRoleInstance AddCustomRole(this ExPlayer player, Type roleType, bool enableRole = false, bool useSpawnPoint = true)
    {
        if (player is null)
            throw new ArgumentNullException(nameof(player));
        
        if (roleType is null)
            throw new ArgumentNullException(nameof(roleType));
        
        if (!RegisteredRoles.TryGetValue(roleType, out var customRoleData))
            throw new ArgumentException($"Role type {roleType} is not registered.", nameof(roleType));
        
        if (player.customRoles.ContainsKey(roleType))
            throw new ArgumentException($"Role type {roleType} is already added to player.", nameof(roleType));
        
        if (Activator.CreateInstance(roleType) is not CustomRoleInstance customRoleInstance)
            throw new ArgumentException($"Role type {roleType} is not a custom role instance.", nameof(roleType));

        customRoleInstance.CustomData = customRoleData;
        customRoleInstance.Owner = player;
        
        customRoleInstance.InvokeInstantiated();
        
        player.customRoles.Add(roleType, customRoleInstance);

        if (enableRole)
        {
            if (!ActiveRoles.TryGetValue(roleType, out var activeRoles))
                ActiveRoles.Add(roleType, activeRoles = new());
            
            activeRoles.Add(customRoleInstance);
            
            SetToRole(player, customRoleInstance, useSpawnPoint);
            
            customRoleInstance.OnEnabled();
            customRoleInstance.IsActive = true;
        }

        return customRoleInstance;
    }

    /// <summary>
    /// Removes a custom role from a player.
    /// </summary>
    /// <param name="player">The player to remove the custom role from.</param>
    /// <param name="setToSpectator">Whether or not to set the player's role to spectator if the custom role is active.</param>
    /// <typeparam name="T">The type of the custom role to remove.</typeparam>
    /// <returns>true if the custom role was successfully removed.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static bool RemoveCustomRole<T>(this ExPlayer player, bool setToSpectator = false) where T : CustomRoleInstance
    {
        return RemoveCustomRole(player, typeof(T), setToSpectator);
    }

    /// <summary>
    /// Removes a custom role from a player.
    /// </summary>
    /// <param name="player">The player to remove the custom role from.</param>
    /// <param name="roleType">The type of the custom role to remove.</param>
    /// <param name="setToSpectator">Whether or not to set the player's role to spectator if the custom role is active.</param>
    /// <returns>true if the custom role was successfully removed.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static bool RemoveCustomRole(this ExPlayer player, Type roleType, bool setToSpectator = false)
    {
        if (player is null)
            throw new ArgumentNullException(nameof(player));
        
        if (roleType is null)
            throw new ArgumentNullException(nameof(roleType));
        
        if (!player.customRoles.TryGetValue(roleType, out var role))
            return false;

        if (role.IsActive)
        {
            role.IsActive = false;
            role.OnDisabled();
            
            if (setToSpectator)
                player.Role.Set(RoleTypeId.Spectator);
        }
        
        role.Dispose();
        
        player.customRoles.Remove(roleType);
        
        if (ActiveRoles.TryGetValue(roleType, out var activeRoles))
            activeRoles.Remove(role);

        return true;
    }

    /// <summary>
    /// Tries to get a list of active custom roles of a specific type.
    /// </summary>
    /// <param name="type">Type of the role.</param>
    /// <param name="roles">A list of roles.</param>
    /// <returns>true if there are any active roles.</returns>
    public static bool TryGetActiveRoles(Type type, out List<CustomRoleInstance> roles)
        => ActiveRoles.TryGetValue(type, out roles);

    /// <summary>
    /// Tries to get a list of active custom roles of a specific type.
    /// </summary>
    /// <param name="roles">A list of roles.</param>
    /// <typeparam name="T">Type of the role.</typeparam>
    /// <returns>true if there are any active roles.</returns>
    public static bool TryGetActiveRoles<T>(out List<T>? roles) where T : CustomRoleInstance
    {
        if (!ActiveRoles.TryGetValue(typeof(T), out var activeRoles)
            || activeRoles.Count == 0)
        {
            roles = null;
            return false;
        }

        roles = new(activeRoles.Count);
        
        foreach (var role in activeRoles)
        {
            if (role is not T roleInstance)
                continue;
            
            roles.Add(roleInstance);
        }
        
        return true;
    }

    /// <summary>
    /// Executes the specified delegate on each active custom role instance.
    /// </summary>
    /// <param name="action">The delegate to invoke.</param>
    /// <param name="predicate">An optional predicate.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void ForEachActiveCustomRole(Action<CustomRoleInstance> action, Func<CustomRoleInstance, bool>? predicate = null)
        => ForEachCustomRole(action, role => role.IsActive && (predicate is null || predicate(role)));
    
    /// <summary>
    /// Executes the specified delegate on each active custom role instance.
    /// </summary>
    /// <param name="action">The delegate to invoke.</param>
    /// <param name="predicate">An optional predicate.</param>
    /// <typeparam name="T">Type of the custom role.</typeparam>
    /// <exception cref="ArgumentNullException"></exception>
    public static void ForEachActiveCustomRole<T>(Action<T> action, Func<T, bool>? predicate = null) where T : CustomRoleInstance
        => ForEachCustomRole(action, role => role.IsActive && (predicate is null || predicate(role)));
    
    /// <summary>
    /// Executes the specified delegate on each custom role instance.
    /// </summary>
    /// <param name="action">The delegate to invoke.</param>
    /// <param name="predicate">An optional predicate.</param>
    /// <typeparam name="T">Type of the custom role.</typeparam>
    /// <exception cref="ArgumentNullException"></exception>
    public static void ForEachCustomRole<T>(Action<T> action, Func<T, bool>? predicate = null) where T : CustomRoleInstance
    {
        if (action is null)
            throw new ArgumentNullException(nameof(action));

        foreach (var pair in ActiveRoles)
        {
            pair.Value.ForEach(role =>
            {
                if (role is not T customRoleInstance)
                    return;
                
                if (predicate != null && !predicate(customRoleInstance))
                    return;
                
                DelegateExtensions.InvokeSafe(action, customRoleInstance);
            });
        }
    }

    /// <summary>
    /// Executes the specified delegate on each custom role instance.
    /// </summary>
    /// <param name="action">The delegate to invoke.</param>
    /// <param name="predicate">An optional predicate.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void ForEachCustomRole(Action<CustomRoleInstance> action, Func<CustomRoleInstance, bool>? predicate = null)
    {
        if (action is null)
            throw new ArgumentNullException(nameof(action));

        foreach (var pair in ActiveRoles)
        {
            pair.Value.ForEach(role =>
            {
                if (predicate != null && !predicate(role))
                    return;
                
                DelegateExtensions.InvokeSafe(action, role);
            });
        }
    }

    /// <summary>
    /// Gets a value indicating whether or not a specific player has an active custom role of a specific type.
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <typeparam name="T">Type of the custom role.</typeparam>
    /// <returns>true if the custom role is found and active.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static bool HasActiveCustomRole<T>(this ExPlayer player) where T : CustomRoleInstance
    {
        if (player is null)
            throw new ArgumentNullException(nameof(player));
        
        return HasActiveCustomRole(player, typeof(T));
    }
    
    /// <summary>
    /// Gets a value indicating whether or not a specific player has an active custom role of a specific type.
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <param name="roleInstance">The found role instance.</param>
    /// <typeparam name="T">Type of the custom role.</typeparam>
    /// <returns>true if the custom role is found and active.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static bool HasActiveCustomRole<T>(this ExPlayer player, out T? roleInstance) where T : CustomRoleInstance
    {
        if (player is null)
            throw new ArgumentNullException(nameof(player));

        if (!HasActiveCustomRole(player, typeof(T), out var role))
        {
            roleInstance = null;
            return false;
        }
        
        roleInstance = (T)role!;
        return true;
    }
    
    /// <summary>
    /// Gets a value indicating whether or not a specific player has an active custom role of a specific type.
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <param name="roleType">Type of the custom role.</param>
    /// <returns>true if the custom role is found and active.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static bool HasActiveCustomRole(this ExPlayer player, Type roleType)
    {
        if (player is null)
            throw new ArgumentNullException(nameof(player));
        
        if (roleType is null)
            throw new ArgumentNullException(nameof(roleType));

        return player.customRoles.TryGetValue(roleType, out var role) && role.IsActive;
    }
    
    /// <summary>
    /// Gets a value indicating whether or not a specific player has an active custom role of a specific type.
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <param name="roleType">Type of the custom role.</param>
    /// <param name="roleInstance">The found role instance.</param>
    /// <returns>true if the custom role is found and active.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static bool HasActiveCustomRole(this ExPlayer player, Type roleType, out CustomRoleInstance? roleInstance)
    {
        if (player is null)
            throw new ArgumentNullException(nameof(player));
        
        if (roleType is null)
            throw new ArgumentNullException(nameof(roleType));

        if (!player.customRoles.TryGetValue(roleType, out var role) || !role.IsActive)
        {
            roleInstance = null;
            return false;
        }

        roleInstance = role;
        return true;
    }

    private static void SetToRole(ExPlayer player, CustomRoleInstance role, bool useSpawnPoint)
    {
        if (player.Role.Type != role.CustomData.RealType)
            player.Role.Set(role.CustomData.RealType, RoleChangeReason.RoundStart, useSpawnPoint ? RoleSpawnFlags.UseSpawnpoint : RoleSpawnFlags.None);
        
        role.CustomData.SpawnAmmo.ForEach(p => player.Ammo.SetAmmo(p.Key, p.Value));
        role.CustomData.SpawnEffects.ForEach(p => player.Effects.EnableEffect(p.Key, p.Value.Key, p.Value.Value));
        role.CustomData.SpawnInventory.ForEach(p => player.Inventory.AddOrSpawnItem(p, ItemAddReason.StartingItem));
        
        role.CustomData.VoiceProfiles.ForEach(p =>
        {
            if (!player.Voice.HasProfile(p))
                player.Voice.AddProfile(p, true);
        });
        
        if (role.CustomData.Toggles != null)
            player.Toggles.Copy(role.CustomData.Toggles);
        
        if (role.CustomData.Gravity.HasValue)
            player.Position.Gravity = role.CustomData.Gravity.Value;
        
        if (role.CustomData.Scale.HasValue)
            player.Scale = role.CustomData.Scale.Value;
        
        if (role.CustomData.Appearance.HasValue)
            player.Role.FakedList.GlobalValue = role.CustomData.Appearance.Value;

        if (role.CustomData.MaxHealth.HasValue)
            player.Stats.MaxHealth = role.CustomData.MaxHealth.Value;
        
        if (role.CustomData.MaxAhp.HasValue)
            player.Stats.MaxAhp = role.CustomData.MaxAhp.Value;
        
        if (role.CustomData.MaxStamina.HasValue)
            player.Stats.MaxStamina = role.CustomData.MaxStamina.Value;
        
        if (role.CustomData.StartingHealth.HasValue)
        {
            if (role.CustomData.StartingHealth.Value > player.Stats.MaxHealth)
                player.Stats.MaxHealth = role.CustomData.StartingHealth.Value;
            
            player.Stats.CurHealth = role.CustomData.StartingHealth.Value;
        }
        
        if (role.CustomData.StartingAhp.HasValue)
        {
            if (role.CustomData.StartingAhp.Value > player.Stats.MaxAhp)
                player.Stats.MaxAhp = role.CustomData.StartingAhp.Value;
            
            player.Stats.CurAhp = role.CustomData.StartingAhp.Value;
        }
        
        if (role.CustomData.StartingStamina.HasValue)
        {
            if (role.CustomData.StartingStamina.Value > player.Stats.MaxStamina)
                player.Stats.MaxStamina = role.CustomData.StartingStamina.Value;
            
            player.Stats.CurStamina = role.CustomData.StartingStamina.Value;
        }
        
        role.CustomData.SpawnMessage?.DisplayMessage(player);
        role.CustomData.SpawnHandlers.ForEach(handler => handler.InvokeSafe(player, role));
    }

    private static void OnPlayerLeft(ExPlayer? player)
    {
        if (player == null || !player) 
            return;
        
        player.customRoles.ForEach(p =>
        {
            if (ActiveRoles.TryGetValue(p.Key, out var activeRoles))
                activeRoles.Remove(p.Value);

            if (p.Value.IsActive)
            {
                p.Value.IsActive = false;
                p.Value.OnDisabled();
            }
                
            p.Value.Dispose();
        });
            
        player.customRoles.Clear();
    }

    [LoaderInitialize(1)]
    private static void OnInit()
    {
        InternalEvents.OnPlayerLeft += OnPlayerLeft;
    }
}