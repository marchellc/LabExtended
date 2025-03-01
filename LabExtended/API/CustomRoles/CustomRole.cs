using System.Reflection;

using HarmonyLib;

using LabExtended.API.Containers;

using LabExtended.Utilities.Unity;
using LabExtended.Attributes;
using LabExtended.Extensions;
using LabExtended.Utilities;

using LabExtended.Events;
using LabExtended.Events.Player;

using NorthwoodLib.Pools;

using PlayerRoles;

namespace LabExtended.API.CustomRoles;

using Attributes;

public class CustomRole
{
    private static readonly Dictionary<Type, CustomRoleAttribute> customRoles = new();
    private static readonly Dictionary<ExPlayer, List<CustomRole>> activeRoles = new();

    public static IReadOnlyDictionary<Type, CustomRoleAttribute> CustomRoles => customRoles;
    public static IReadOnlyDictionary<ExPlayer, List<CustomRole>> ActiveRoles => activeRoles;
    
    public bool IsEnabled { get; internal set; }
    
    public ExPlayer Player { get; internal set; }
    public CustomRoleAttribute Attribute { get; internal set; }

    public RoleContainer Role => Player?.Role;
    public AmmoContainer Ammo => Player?.Ammo;
    public StatsContainer Stats => Player?.Stats;
    public EffectContainer Effects => Player?.Effects;
    public RotationContainer Rotation => Player?.Rotation;
    public PositionContainer Position => Player?.Position;
    public InventoryContainer Inventory => Player?.Inventory;
    public SubroutineContainer Subroutines => Player?.Subroutines;
    
    public string Id => Attribute?.Id ?? string.Empty;
    public string Name => Attribute?.Name ?? string.Empty;
    public string Description => Attribute?.Description ?? string.Empty;

    public Type Type => Attribute?.roleType;

    public virtual void OnGrantingInventory(PlayerGrantingInventoryArgs args) { }
    public virtual void OnSelectingPosition(PlayerSelectingSpawnPositionArgs args) { }
    public virtual void OnSpawning(PlayerSpawningArgs args) { }
    
    public virtual void Enable() { }
    public virtual void Disable() { }
    
    public virtual void Start() { }
    public virtual void Destroy() { }
    
    public virtual void Update() { }

    public virtual bool EnableRole(RoleChangeReason reason, RoleTypeId currentRole, RoleTypeId newRole) => false;

    internal virtual void InternalEnable()
    {
        if (IsEnabled)
            return;
        
        IsEnabled = true;
        Enable();
    }

    internal virtual void InternalDisable()
    {
        if (!IsEnabled)
            return;

        IsEnabled = false;
        Disable();
    }
    
    internal virtual void InternalStart()
    {
        PlayerLoopHelper.AfterLoop += InternalUpdate;
        Start();
    }
    
    internal virtual void InternalDestroy()
    {
        PlayerLoopHelper.AfterLoop -= InternalUpdate;
        Destroy();
    }
    
    internal virtual void InternalUpdate()
    {
        if (Player is null || !Player || !IsEnabled)
            return;
        
        Update();
    }

    public override string ToString()
        => $"Custom Role (Id={Id}; Player={Player?.ToString() ?? "null"}; Type={Type?.FullName ?? "null"})";

    public static bool HasRole<T>(ExPlayer player) where T : CustomRole
        => player != null && GetRoles(player).Any(x => x is T);

    public static bool HasRole(ExPlayer player, Type roleType)
        => player != null && roleType != null && GetRoles(player).Any(x => x.Attribute.roleType == roleType);

    public static bool HasRole<T>(ExPlayer player, out T customRole) where T : CustomRole
    {
        if (player is null)
            throw new ArgumentNullException(nameof(player));

        customRole = null;

        if (!TryGetRoles(player, out var roles))
            return false;

        foreach (var role in roles)
        {
            if (role is T foundRole)
            {
                customRole = foundRole;
                return true;
            }
        }

        return false;
    }
    
    public static bool HasRole(ExPlayer player, Type roleType, out CustomRole customRole)
    {
        if (player is null)
            throw new ArgumentNullException(nameof(player));
        
        if (roleType is null)
            throw new ArgumentNullException(nameof(roleType));

        customRole = null;

        if (!TryGetRoles(player, out var roles))
            return false;

        foreach (var role in roles)
        {
            if (role.Attribute.roleType == roleType)
            {
                customRole = role;
                return true;
            }
        }

        return false;
    }
    
    public static bool RemoveRole<T>(ExPlayer player) where T : CustomRole
        => RemoveRole(player, typeof(T));

    public static bool RemoveRole(ExPlayer player, Type roleType)
    {
        if (player is null)
            throw new ArgumentNullException(nameof(player));
        
        if (roleType is null)
            throw new ArgumentNullException(nameof(roleType));

        if (!TryGetRoles(player, out var roles))
            return false;

        if (!roles.TryGetFirst(x => x.Attribute.roleType == roleType, out var role))
            return false;
        
        role.InternalDisable();
        role.InternalDestroy();
        
        roles.Remove(role);
        role.Attribute.roles.Remove(role);
        return true;
    }

    public static T GrantRole<T>(ExPlayer player) where T : CustomRole
    {
        if (!TryGrantRole<T>(player, out var customRole))
            throw new Exception("Could not grant custom role");

        return customRole;
    }
    
    public static CustomRole GrantRole(ExPlayer player, Type roleType)
    {
        if (!TryGrantRole(player, roleType, out var customRole))
            throw new Exception("Could not grant custom role");

        return customRole;
    }

    public static bool TryGrantRole<T>(ExPlayer player, out T customRole) where T : CustomRole
    {
        customRole = null;
        
        if (!TryGrantRole(player, typeof(T), out var role))
            return false;
        
        customRole = (T)role;
        return true;
    }

    public static bool TryGrantRole(ExPlayer player, Type roleType, out CustomRole grantedRole)
    {
        if (roleType is null)
            throw new ArgumentNullException(nameof(roleType));
        
        if (player is null)
            throw new ArgumentNullException(nameof(player));

        grantedRole = null;

        if (!customRoles.TryGetValue(roleType, out var customRole))
            return false;

        var activeRoles = GetRoles(player);

        if (activeRoles.Any(x => x.Attribute == customRole))
            return false;
        
        grantedRole = customRole.constructor(Array.Empty<object>()) as CustomRole;

        if (grantedRole is null)
            return false;
        
        grantedRole.Player = player;
        grantedRole.Attribute = customRole;
        
        grantedRole.InternalStart();
        
        if (grantedRole.EnableRole(RoleChangeReason.RoundStart, player.Role, player.Role))
            grantedRole.InternalEnable();
        
        activeRoles.Add(grantedRole);
        customRole.roles.Add(grantedRole);
        return true;
    }

    public static bool TryGetRoles(ExPlayer player, out List<CustomRole> roles)
    {
        if (player is null)
            throw new ArgumentNullException(nameof(player));
        
        return activeRoles.TryGetValue(player, out roles);
    }

    public static List<CustomRole> GetRoles(ExPlayer player)
    {
        if (player is null)
            throw new ArgumentNullException(nameof(player));
        
        if (!activeRoles.TryGetValue(player, out var roles))
            activeRoles.Add(player, roles = ListPool<CustomRole>.Shared.Rent());
        
        return roles;
    }
    
    public static void RegisterRoles()
        => RegisterRoles(Assembly.GetCallingAssembly());

    public static void RegisterRoles(Assembly assembly)
    {        
        if (assembly is null)
            throw new ArgumentNullException(nameof(assembly));
        
        foreach (var type in assembly.GetTypes())
        {
            if (!type.IsPublic || (type.IsAbstract && type.IsSealed))
                continue;
            
            if (type == typeof(CustomRole))
                continue;
            
            if (!type.HasAttribute<CustomRoleAttribute>(out var customRoleAttribute))
                continue;
            
            if (!type.InheritsType<CustomRole>())
                continue;
            
            if (string.IsNullOrWhiteSpace(customRoleAttribute.Id) || string.IsNullOrWhiteSpace(customRoleAttribute.Name))
                continue;
            
            if (customRoles.ContainsKey(type))
                continue;

            if (string.IsNullOrWhiteSpace(customRoleAttribute.Description))
                customRoleAttribute.Description = "No description";

            customRoleAttribute.roleType = type;
            customRoleAttribute.roles = ListPool<CustomRole>.Shared.Rent();
            customRoleAttribute.constructor = FastReflection.ForConstructor(type.Constructor());
            
            if (customRoleAttribute.AssignOnJoin && !string.IsNullOrWhiteSpace(customRoleAttribute.PredicateName))
            {
                var method = type.FindMethod(x => x.Name == customRoleAttribute.PredicateName);

                if (method != null && method.IsStatic && method.ReturnType == typeof(bool))
                {
                    var parameters = method.GetAllParameters();

                    if (parameters?.Length == 1 && parameters[0].ParameterType == typeof(ExPlayer))
                    {
                        customRoleAttribute.predicate = method.CreateDelegate(typeof(Func<ExPlayer, bool>)) as Func<ExPlayer, bool>;
                    }
                }
            }
            
            customRoles.Add(type, customRoleAttribute);
        }
    }
    
    public static void UnregisterRoles()
        => UnregisterRoles(Assembly.GetCallingAssembly());

    public static void UnregisterRoles(Assembly assembly)
    {
        if (assembly is null)
            throw new ArgumentNullException(nameof(assembly));

        foreach (var type in assembly.GetTypes())
        {
            if (!customRoles.TryGetValue(type, out var role))
                continue;

            if (role.roles != null)
            {
                role.roles.ForEach(x =>
                {
                    x.InternalDisable();
                    x.InternalDestroy();
                });
                
                ListPool<CustomRole>.Shared.Return(role.roles);

                role.roles = null;
            }
            
            customRoles.Remove(type);
        }
    }

    private static void OnJoin(ExPlayer player)
    {
        var list = ListPool<CustomRole>.Shared.Rent();
        
        activeRoles.Add(player, list);
        
        foreach (var pair in customRoles)
        {
            if (pair.Value.AssignOnJoin)
            {
                if (pair.Value.predicate != null && !pair.Value.predicate(player))
                    continue;
                
                var role = pair.Value.constructor(Array.Empty<object>()) as CustomRole;
                
                if (role is null)
                    continue;
                
                list.Add(role);
                pair.Value.roles.Add(role);
                
                role.Player = player;
                role.Attribute = pair.Value;
                
                role.InternalStart();
                
                if (role.EnableRole(RoleChangeReason.RoundStart, player.Role, player.Role))
                    role.InternalEnable();
            }
        }
    }
    
    private static void OnLeft(ExPlayer player)
    {
        if (activeRoles.TryGetValue(player, out var roles))
        {
            roles.ForEach(x => x.InternalDestroy());
            ListPool<CustomRole>.Shared.Return(roles);
        }

        activeRoles.Remove(player);
    }

    [LoaderInitialize(1)]
    private static void Init()
    {
        InternalEvents.OnPlayerLeft += OnLeft;
        InternalEvents.OnPlayerJoined += OnJoin;
    }
}