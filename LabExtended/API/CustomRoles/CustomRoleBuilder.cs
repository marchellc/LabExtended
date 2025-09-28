using LabExtended.API.Containers;
using LabExtended.API.Interfaces;
using LabExtended.API.Custom.Voice.Profiles;

using LabExtended.Extensions;

using PlayerRoles;

using UnityEngine;
using Utils.NonAllocLINQ;

namespace LabExtended.API.CustomRoles;

/// <summary>
/// Custom role configuration builder.
/// </summary>
public class CustomRoleBuilder
{
    /// <summary>
    /// Gets the active role data.
    /// </summary>
    public CustomRoleData Data { get; } = new();
    
    /// <summary>
    /// Sets the role's class.
    /// </summary>
    /// <typeparam name="T">The class.</typeparam>
    /// <returns>This builder instance.</returns>
    public CustomRoleBuilder WithClass<T>() where T : CustomRoleInstance, new()
    {
        Data.Type = typeof(T);
        return this;
    }

    /// <summary>
    /// Sets the role's class.
    /// </summary>
    /// <param name="roleClass">The class.</param>
    /// <returns>This builder instance.</returns>
    public CustomRoleBuilder WithClass(Type roleClass)
    {
        if (roleClass is null)
            throw new ArgumentNullException(nameof(roleClass));
        
        Data.Type = roleClass;
        return this;
    }

    /// <summary>
    /// Sets the role's name.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <returns>This builder instance.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public CustomRoleBuilder WithName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentNullException(nameof(name));
        
        Data.Name = name;
        return this;
    }

    /// <summary>
    /// Sets the role's description.
    /// </summary>
    /// <param name="description">The description.</param>
    /// <returns>This builder instance.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public CustomRoleBuilder WithDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentNullException(nameof(description));
        
        Data.Description = description;
        return this;
    }

    /// <summary>
    /// Sets the role's spawn class.
    /// </summary>
    /// <param name="realType">The real type to spawn as.</param>
    /// <param name="fakeType">The appeareance to show to other players.</param>
    /// <returns>This builder instance.</returns>
    public CustomRoleBuilder WithType(RoleTypeId realType, RoleTypeId? fakeType = null)
    {
        if (realType is RoleTypeId.None or RoleTypeId.Destroyed)
            throw new ArgumentException($"Invalid role type: {realType}", nameof(realType));
        
        Data.RealType = realType;
        Data.Appearance = fakeType;

        return this;
    }
    
    /// <summary>
    /// Sets the role's scale.
    /// </summary>
    /// <param name="scale">The scale.</param>
    /// <returns>This builder instance.</returns>
    public CustomRoleBuilder WithScale(Vector3 scale)
    {
        Data.Scale = scale;
        return this;
    }

    /// <summary>
    /// Sets the role's spawn message.
    /// </summary>
    /// <param name="spawnMessage">The spawn message.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public CustomRoleBuilder WithMessage(IMessage spawnMessage)
    {
        if (spawnMessage is null)
            throw new ArgumentNullException(nameof(spawnMessage));

        Data.SpawnMessage = spawnMessage;
        return this;
    }

    /// <summary>
    /// Sets the role's gravity.
    /// </summary>
    /// <param name="gravity">The gravity.</param>
    /// <returns>This builder instance.</returns>
    public CustomRoleBuilder WithGravity(Vector3 gravity)
    {
        Data.Gravity = gravity;
        return this;
    }
    
    /// <summary>
    /// Sets the role's starting and maximum AHP.
    /// </summary>
    /// <param name="startingAhp">The amount of AHP the role will spawn with.</param>
    /// <param name="maxAhp">The maximum amount of AHP.</param>
    /// <returns>This builder instance.</returns>
    public CustomRoleBuilder WithAhp(float? startingAhp, float? maxAhp = null)
    {
        Data.StartingAhp = startingAhp;
        Data.MaxAhp = maxAhp;
        
        return this;
    }

    /// <summary>
    /// Sets the role's starting and maximum health.
    /// </summary>
    /// <param name="startingHealth">The amount of health the role will spawn with.</param>
    /// <param name="maxHealth">The maximum amount of health.</param>
    /// <returns>This builder instance.</returns>
    public CustomRoleBuilder WithHealth(float? startingHealth, float? maxHealth = null)
    {
        Data.StartingHealth = startingHealth;
        Data.MaxHealth = maxHealth;
        
        return this;
    }
    
    /// <summary>
    /// Sets the role's starting and maximum stamina.
    /// </summary>
    /// <param name="startingStamina">The amount of stamina the role will spawn with.</param>
    /// <param name="maxStamina">The maximum amount of stamina.</param>
    /// <returns>This builder instance.</returns>
    public CustomRoleBuilder WithStamina(float? startingStamina, float? maxStamina = null)
    {
        Data.StartingStamina = startingStamina;
        Data.MaxStamina = maxStamina;
        
        return this;
    }

    /// <summary>
    /// Sets the role's toggles.
    /// </summary>
    /// <param name="toggleConfiguration">The delegate used to configure the role's toggles.</param>
    /// <returns>This builder instance.</returns>
    public CustomRoleBuilder WithToggles(Action<SwitchContainer> toggleConfiguration)
    {
        if (toggleConfiguration is null)
            throw new ArgumentNullException(nameof(toggleConfiguration));
        
        var newToggles = new SwitchContainer();
        
        newToggles.Copy(SwitchContainer.DefaultPlayerSwitches);
        
        toggleConfiguration(newToggles);
        
        Data.Toggles = newToggles;
        return this;
    }
    
    /// <summary>
    /// Adds a voice profile for this role.
    /// </summary>
    /// <typeparam name="T">The profile type.</typeparam>
    /// <returns>This builder instance.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public CustomRoleBuilder WithVoiceProfile<T>() where T : VoiceProfile, new()
    {
        if (!Data.VoiceProfiles.Contains(typeof(T)))
            Data.VoiceProfiles.Add(typeof(T));
        
        return this;
    }

    /// <summary>
    /// Adds a voice profile for this role.
    /// </summary>
    /// <param name="profileType">The profile type.</param>
    /// <returns>This builder instance.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public CustomRoleBuilder WithVoiceProfile(Type profileType)
    {
        if (profileType is null)
            throw new ArgumentNullException(nameof(profileType));
        
        if (!Data.VoiceProfiles.Contains(profileType))
            Data.VoiceProfiles.Add(profileType);
        
        return this;
    }

    /// <summary>
    /// Adds multiple voice profiles for this role.
    /// </summary>
    /// <param name="voiceProfiles">The profiles to add.</param>
    /// <returns>This builder instance.</returns>
    public CustomRoleBuilder WithVoiceProfiles(IEnumerable<Type> voiceProfiles)
    {
        if (voiceProfiles is null)
            throw new ArgumentNullException(nameof(voiceProfiles));
        
        voiceProfiles.ForEach(p => Data.VoiceProfiles.AddIfNotContains(p));
        return this;
    }

    /// <summary>
    /// Adds an item to the spawn inventory.
    /// </summary>
    /// <param name="type">The item to add.</param>
    /// <returns>This builder instance.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public CustomRoleBuilder WithItem(ItemType type)
    {
        if (type is ItemType.None)
            throw new ArgumentException($"Invalid item type: {type}", nameof(type));

        if (type.IsAmmo())
            throw new ArgumentException($"Ammo cannot be added", nameof(type));
        
        Data.SpawnInventory.Add(type);
        return this;
    }

    /// <summary>
    /// Adds multiple items to the spawn inventory.
    /// </summary>
    /// <param name="items">The items to add.</param>
    /// <returns>This builder instance.</returns>
    public CustomRoleBuilder WithItems(IEnumerable<ItemType> items)
    {
        if (items is null)
            throw new ArgumentNullException(nameof(items));
        
        items.ForEach(p =>
        {
            if (p.IsAmmo() || p is ItemType.None)
                return;
            
            Data.SpawnInventory.Add(p);
        });

        return this;
    }

    /// <summary>
    /// Adds ammo to the spawn inventory.
    /// </summary>
    /// <param name="ammoType">Ammo type.</param>
    /// <param name="amount">Ammo amount.</param>
    /// <returns>This builder instance.</returns>
    public CustomRoleBuilder WithAmmo(ItemType ammoType, ushort amount)
    {
        if (!ammoType.IsAmmo())
            throw new ArgumentException($"Invalid ammo type: {ammoType}", nameof(ammoType));
        
        if (amount < 1)
            throw new ArgumentException($"Invalid ammo amount: {amount}", nameof(amount));
        
        Data.SpawnAmmo[ammoType] = amount;
        return this;
    }

    /// <summary>
    /// Adds a status effect.
    /// </summary>
    /// <param name="effectType">The effect type.</param>
    /// <param name="effectIntensity">The effect intensity.</param>
    /// <param name="effectDuration">The effect duration.</param>
    /// <returns>This builder instance.</returns>
    public CustomRoleBuilder WithEffect(Type effectType, byte effectIntensity, float effectDuration = 0f)
    {
        if (effectType is null)
            throw new ArgumentNullException(nameof(effectType));

        Data.SpawnEffects[effectType] = new(effectIntensity, effectDuration);
        return this;
    }

    /// <summary>
    /// Adds a callback method used when a player spawns.
    /// </summary>
    /// <param name="spawnDelegate">The callback delegate.</param>
    /// <returns>This builder instance.</returns>
    public CustomRoleBuilder WithSpawnDelegate(Action<ExPlayer, CustomRoleInstance> spawnDelegate)
    {
        if (spawnDelegate is null)
            throw new ArgumentNullException(nameof(spawnDelegate));

        Data.SpawnHandlers.AddIfNotContains(spawnDelegate);
        return this;
    }
}