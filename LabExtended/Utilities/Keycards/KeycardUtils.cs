using InventorySystem.Items;
using InventorySystem.Items.Keycards;

using LabExtended.Core.Pooling.Pools;
using LabExtended.Extensions;

using Mirror;

namespace LabExtended.Utilities.Keycards;

/// <summary>
/// Utilities and extensions targeting keycard items.
/// </summary>
public static class KeycardUtils
{
    /// <summary>
    /// Creates a new keycard with configured custom parameters.
    /// </summary>
    /// <param name="keycardType">The type of the keycard.</param>
    /// <param name="builder">The delegate used to change the keycard's properties.</param>
    /// <param name="serial">The optional custom item serial.</param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="Exception"></exception>
    public static KeycardItem Create(ItemType keycardType, ushort? serial, Action<KeycardBuilder> builder)
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
    public static KeycardItem Create(ItemType keycardType, KeycardBuilder builder, ushort? serial = null)
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
        
        builder.Apply(keycard);
        
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