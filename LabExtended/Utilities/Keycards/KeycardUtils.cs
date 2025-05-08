using InventorySystem.Items;
using InventorySystem.Items.Keycards;
using LabExtended.API;
using LabExtended.Core.Pooling.Pools;
using LabExtended.Extensions;

using Mirror;
using UnityEngine;

#pragma warning disable CS8602 // Dereference of a possibly null reference.

namespace LabExtended.Utilities.Keycards;

/// <summary>
/// Utilities and extensions targeting keycard items.
/// </summary>
public static class KeycardUtils
{
    /// <summary>
    /// Adds a keycard with custom properties to a player's inventory.
    /// </summary>
    /// <param name="target">The target player.</param>
    /// <param name="keycardType">The keycard's item type.</param>
    /// <param name="serial">The keycard's serial number.</param>
    /// <param name="dropIfFull">Whether or not to drop the item if the target player's inventory is full.</param>
    /// <param name="builder">The delegate used to build the item.</param>
    /// <returns>null if the player's inventory is full, otherwise the created item instance</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="Exception"></exception>
    public static KeycardItem? Give(ExPlayer target, ItemType keycardType, ushort? serial, bool dropIfFull,
        Action<KeycardBuilder> builder)
    {
        if (target is null)
            throw new ArgumentNullException(nameof(target));
        
        if (builder is null)
            throw new ArgumentNullException(nameof(builder));

        if (target.Inventory.ItemCount > 7)
        {
            if (dropIfFull)
                SpawnPickup(keycardType, target.Position, Vector3.one, target.Rotation, serial, builder);

            return null;
        }

        var item = CreateItem(keycardType, serial, builder);
        
        item.TransferItem(target.ReferenceHub);
        return item;
    }
    
    /// <summary>
    /// Creates a new keycard pickup with configured custom parameters.
    /// </summary>
    /// <param name="keycardType">The type of the keycard.</param>
    /// <param name="position">The position to spawn the pickup at.</param>
    /// <param name="scale">The scale of the pickup.</param>
    /// <param name="rotation">The rotation of the pickup.</param>
    /// <param name="builder">The delegate used to change the keycard's properties.</param>
    /// <param name="serial">The optional custom item serial.</param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="Exception"></exception>
    public static KeycardPickup SpawnPickup(ItemType keycardType, Vector3? position, Vector3? scale, Quaternion? rotation, 
        ushort? serial, Action<KeycardBuilder> builder)
    {
        if (builder is null)
            throw new ArgumentNullException(nameof(builder));

        var instance = keycardType.GetPickupInstance<KeycardPickup>(position, scale, rotation, serial, true);
        
        SetCustomProperties(instance, builder);
        return instance;
    }
    
    /// <summary>
    /// Creates a new keycard with configured custom parameters.
    /// </summary>
    /// <param name="keycardType">The type of the keycard.</param>
    /// <param name="builder">The delegate used to change the keycard's properties.</param>
    /// <param name="serial">The optional custom item serial.</param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="Exception"></exception>
    public static KeycardItem CreateItem(ItemType keycardType, ushort? serial, Action<KeycardBuilder> builder)
    {
        if (builder is null)
            throw new ArgumentNullException(nameof(builder));
        
        if (!keycardType.TryGetItemPrefab(out var itemPrefab))
            throw new Exception($"Could not retrieve item prefab for {keycardType}");
        
        if (itemPrefab is not KeycardItem keycardItem)
            throw new Exception($"Item {keycardType} is not a KeycardItem");
        
        var instance = UnityEngine.Object.Instantiate(keycardItem);

        instance.ItemSerial = serial ?? ItemSerialGenerator.GenerateNext();
        
        SetCustomProperties(instance, builder);
        return instance;
    }
    
    /// <summary>
    /// Creates a new keycard with configured custom parameters.
    /// </summary>
    /// <param name="keycardType">The type of the keycard.</param>
    /// <param name="builder">The builder for the keycard's custom properties.</param>
    /// <param name="serial">The optional custom item serial.</param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="Exception"></exception>
    public static KeycardItem CreateItem(ItemType keycardType, KeycardBuilder builder, ushort? serial = null)
    {
        if (builder is null)
            throw new ArgumentNullException(nameof(builder));
        
        if (!keycardType.TryGetItemPrefab(out var itemPrefab))
            throw new Exception($"Could not retrieve item prefab for {keycardType}");
        
        if (itemPrefab is not KeycardItem keycardItem)
            throw new Exception($"Item {keycardType} is not a KeycardItem");
        
        var instance = UnityEngine.Object.Instantiate(keycardItem);

        instance.ItemSerial = serial ?? ItemSerialGenerator.GenerateNext();
        
        ApplyCustomProperties(instance, builder);
        return instance;
    }
    
    /// <summary>
    /// Sets the custom properties of a keycard.
    /// </summary>
    /// <param name="keycard">The keycard to change.</param>
    /// <param name="builder">The delegate used to change the keycard's properties.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void SetCustomProperties(this KeycardItem keycard, Action<KeycardBuilder> builder)
    {
        if (keycard is null)
            throw new ArgumentNullException(nameof(keycard));
        
        if (builder is null)
            throw new ArgumentNullException(nameof(builder));

        var instance = KeycardBuilder.Pooled;
        
        builder(instance);
        
        ApplyCustomProperties(keycard, instance);
        
        ObjectPool<KeycardBuilder>.Shared.Return(instance);
    }

    /// <summary>
    /// Sets the custom properties of a keycard.
    /// </summary>
    /// <param name="keycard">The keycard to change.</param>
    /// <param name="builder">The delegate used to change the keycard's properties.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void SetCustomProperties(this KeycardPickup keycard, Action<KeycardBuilder> builder)
    {
        if (keycard is null)
            throw new ArgumentNullException(nameof(keycard));
        
        if (builder is null)
            throw new ArgumentNullException(nameof(builder));

        var instance = KeycardBuilder.Pooled;
        
        builder(instance);
        
        ApplyCustomProperties(keycard, instance);
        
        ObjectPool<KeycardBuilder>.Shared.Return(instance);
    }
    
    /// <summary>
    /// Applies the custom properties of a keycard.
    /// </summary>
    /// <param name="keycard">The keycard to change.</param>
    /// <param name="builder">The builder used to change the keycard's properties.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void ApplyCustomProperties(this KeycardPickup keycard, KeycardBuilder builder)
    {
        if (keycard is null)
            throw new ArgumentNullException(nameof(keycard));
        
        if (builder is null)
            throw new ArgumentNullException(nameof(builder));

        var prefab = keycard.ItemId.TypeId.GetItemPrefab<KeycardItem>();
        var data = builder.ToSegment(prefab);
        
        builder.Apply(prefab, keycard.Info.Serial);
        
        KeycardDetailSynchronizer.Database[keycard.Info.Serial] = data;
        
        NetworkServer.SendToAll(new KeycardDetailSynchronizer.DetailsSyncMsg
        {
            Serial = keycard.Info.Serial,
            Payload = data
        });
    }
    
    /// <summary>
    /// Applies the custom properties of a keycard.
    /// </summary>
    /// <param name="keycard">The keycard to change.</param>
    /// <param name="builder">The builder used to change the keycard's properties.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void ApplyCustomProperties(this KeycardItem keycard, KeycardBuilder builder)
    {
        if (keycard is null)
            throw new ArgumentNullException(nameof(keycard));
        
        if (builder is null)
            throw new ArgumentNullException(nameof(builder));
        
        var data = builder.ToSegment(keycard);
        
        builder.Apply(keycard, keycard.ItemSerial);
        
        KeycardDetailSynchronizer.Database[keycard.ItemSerial] = data;
        
        NetworkServer.SendToAll(new KeycardDetailSynchronizer.DetailsSyncMsg
        {
            Serial = keycard.ItemSerial,
            Payload = data
        });
    }
    
    /// <summary>
    /// Attempts to get a specific detail component of a keycard.
    /// </summary>
    /// <param name="keycard">The target keycard item.</param>
    /// <param name="detail">The found component.</param>
    /// <typeparam name="T">The type of component to find.</typeparam>
    /// <returns>true if the component was found</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static bool TryGetDetail<T>(this KeycardItem keycard, out T detail) where T : DetailBase
    {
        if (keycard is null)
            throw new ArgumentNullException(nameof(keycard));

        for (var i = 0; i < keycard.Details.Length; i++)
        {
            if (keycard.Details[i] is not T targetDetail)
                continue;
            
            detail = targetDetail;
            return true;
        }
        
        detail = null;
        return false;
    }
}