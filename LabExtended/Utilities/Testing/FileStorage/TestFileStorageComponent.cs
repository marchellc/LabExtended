// #define ENABLE_TEST_STORAGE_COMPONENT

using LabExtended.API.FileStorage;

using LabExtended.Core;
using LabExtended.Utilities.Generation;

using MEC;

namespace LabExtended.Utilities.Testing.FileStorage;

/// <summary>
/// Component used for testing purposes.
/// </summary>
public class TestFileStorageComponent : FileStorageComponent
{
    /// <summary>
    /// Whether or not the testing component is enabled in this build.
    /// </summary>
#if ENABLE_TEST_STORAGE_COMPONENT
    public static bool IsEnabled = true;
#else
    public static bool IsEnabled = false;
#endif
    
    /// <inheritdoc cref="FileStorageComponent.Name"/>
    public override string Name { get; } = "Test";
    
    /// <summary>
    /// A property that contains a randomly generated number.
    /// </summary>
    public FileStorageProperty<int> RandomProperty { get; private set; }
    
    /// <inheritdoc cref="FileStorageComponent.InitProperties"/>
    public override void InitProperties()
    {
        RandomProperty = AddProperty("random", 0);
        
        RandomProperty.Changed += OnChanged;
        RandomProperty.Modified += OnModified;
        
        ApiLog.Debug("TestFileStorageComponent", $"[InitProperties - {Storage.UserId}]");   
    }

    /// <inheritdoc cref="FileStorageComponent.OnLoaded"/>
    public override void OnLoaded()
    {
        RandomProperty.Value = RandomGen.Instance.GetInt32(1, 1000);
        
        ApiLog.Debug("TestFileStorageComponent", $"[OnLoaded - {Storage.UserId}] Random: {RandomProperty.Value} ({RandomProperty.IsDirty})");

        Timing.CallDelayed(10f, () =>
        {
            RandomProperty.Value = RandomGen.Instance.GetInt32(1, 1000);

            ApiLog.Debug("TestFileStorageComponent",
                $"[OnLoaded-Delayed - {Storage.UserId}] Random: {RandomProperty.Value} ({RandomProperty.IsDirty})");
        });
    }

    /// <inheritdoc cref="FileStorageComponent.OnUnloaded"/>
    public override void OnUnloaded()
    {
        ApiLog.Debug("TestFileStorageComponent", $"[OnUnloaded - {Storage.UserId}]");   
    }

    /// <inheritdoc cref="FileStorageComponent.OnLeft"/>
    public override void OnLeft()
    {
        ApiLog.Debug("TestFileStorageComponent", $"[OnLeft - {Storage.UserId}]");   
    }

    /// <inheritdoc cref="FileStorageComponent.OnJoined"/>
    public override void OnJoined()
    {
        ApiLog.Debug("TestFileStorageComponent", $"[OnJoined] {Player.Nickname} ({Player.UserId})");   
    }

    /// <inheritdoc cref="FileStorageComponent.OnReloaded"/>
    public override void OnReloaded()
    {
        ApiLog.Debug("TestFileStorageComponent", $"[OnReloaded - {Storage.UserId}]");   
    }

    private void OnChanged(int prev, int now)
    {
        if (prev == now)
            return;
        
        ApiLog.Debug("TestFileStorageComponent", $"[OnChanged - {Storage.UserId}] {prev} -> {now}");
    }

    private void OnModified(int prev, int now)
    {
        if (prev == now)
            return;
        
        ApiLog.Debug("TestFileStorageComponent", $"[OnModified - {Storage.UserId}] {prev} -> {now}");
    }
}