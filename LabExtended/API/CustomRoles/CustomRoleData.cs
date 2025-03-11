using LabExtended.API.Containers;
using LabExtended.API.Interfaces;
using PlayerRoles;

using UnityEngine;

namespace LabExtended.API.CustomRoles;

/// <summary>
/// Represents a custom role's configuration.
/// </summary>
public class CustomRoleData
{
    /// <summary>
    /// Gets the role's name.
    /// </summary>
    public string Name { get; internal set; } = string.Empty;
    
    /// <summary>
    /// Gets the role's description.
    /// </summary>
    public string Description { get; internal set; } = string.Empty;
    
    /// <summary>
    /// Gets the role's starting health.
    /// </summary>
    public float? StartingHealth { get; internal set; }
    
    /// <summary>
    /// Gets the role's starting AHP.
    /// </summary>
    public float? StartingAhp { get; internal set; }
    
    /// <summary>
    /// Gets the role's starting stamina.
    /// </summary>
    public float? StartingStamina { get; internal set; }
    
    /// <summary>
    /// Gets the role's maximum health.
    /// </summary>
    public float? MaxHealth { get; internal set; }
    
    /// <summary>
    /// Gets the role's maximum AHP.
    /// </summary>
    public float? MaxAhp { get; internal set; }
    
    /// <summary>
    /// Gets the role's maximum stamina.
    /// </summary>
    public float? MaxStamina { get; internal set; }

    /// <summary>
    /// Gets the role's type.
    /// </summary>
    public RoleTypeId RealType { get; internal set; } = RoleTypeId.None;
    
    /// <summary>
    /// Gets the role's fake appearance.
    /// </summary>
    public RoleTypeId? Appearance { get; internal set; }
    
    /// <summary>
    /// Gets the role's gravity.
    /// </summary>
    public Vector3? Gravity { get; internal set; }
    
    /// <summary>
    /// Gets the role's scale.
    /// </summary>
    public Vector3? Scale { get; internal set; }
    
    /// <summary>
    /// Gets the role's spawn message.
    /// </summary>
    public IMessage? SpawnMessage { get; internal set; }
    
    /// <summary>
    /// Gets the role's class.
    /// </summary>
    public Type? Type { get; internal set; }
    
    /// <summary>
    /// Gets custom toggles.
    /// </summary>
    public SwitchContainer? Toggles { get; internal set; }
    
    /// <summary>
    /// Gets a list of voice profiles.
    /// </summary>
    public List<Type> VoiceProfiles { get; } = new();

    /// <summary>
    /// Gets a list of items used for spawn inventory.
    /// </summary>
    public List<ItemType> SpawnInventory { get; } = new();

    /// <summary>
    /// Gets a list of delegates that will be called once this role spawns.
    /// </summary>
    public List<Action<ExPlayer, CustomRoleInstance>> SpawnHandlers { get; } = new();
    
    /// <summary>
    /// Gets a list of ammo used for spawn inventory.
    /// </summary>
    public Dictionary<ItemType, ushort> SpawnAmmo { get; } = new();
    
    /// <summary>
    /// Gets a list of effects used at spawn.
    /// </summary>
    public Dictionary<Type, KeyValuePair<byte, float>> SpawnEffects { get; } = new();
}