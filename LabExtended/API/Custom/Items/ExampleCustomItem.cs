/*
namespace LabExtended.API.Custom.Items
{
    /// <summary>
    /// Serves as an example of a configuration file for custom items.
    /// </summary>
    public class ExampleConfigFile
    {
        /// <summary>
        /// Gets or sets the config file instance of the example item.
        /// </summary>
        [Description("Modifies properties of the Example Custom Item.")]
        public ExampleCustomItem ExampleCustomItem { get;set; } = new ExampleCustomItem();
    }

    /// <summary>
    /// Serves as an example of a plugin class that registers a custom item using it's config.
    /// </summary>
    public class ExamplePlugin // : Plugin<ExampleConfigFile>
    {
        // Gets the name of the plugin.
        // public override string Name => "Example Plugin";

        /// Gets the author of the plugin.
        // public override string Author => "YourName";

        // Gets the version of the plugin.
        // public override Version Version { get; } new Version(1, 0, 0);

        public override void Enable()
        {
            // Register the custom item when the plugin is enabled
            Config.ExampleCustomItem.Register();
        }

        public override void Disable()
        {
            // Unregister the custom item when the plugin is disabled
            Config.ExampleCustomItem.Unregister();
        }

    } 

    /// <summary>
    /// This class serves as an example on how to use the CustomItem API.
    /// </summary>
    public class ExampleCustomItem : CustomItem
    {
        /// <summary>
        /// Gets the unique identifier for this custom item.
        /// </summary>
        public override string Id { get; } = "labextended.example";

        /// <summary>
        /// Gets the name of this custom item.
        /// </summary>
        public override string Name { get; } = "Example Custom Item";

        /// <summary>
        /// If set to true, all custom items in a player's inventory will be dropped when they leave the server 
        /// using the <see cref="CustomItem.DropItem(InventorySystem.Items.ItemBase, bool)"/> method. 
        /// If false, all custom items in a player's inventory will be destroyed when they leave the server 
        /// using the <see cref="CustomItem.DestroyItem(InventorySystem.Items.ItemBase)"/> method."/>
        /// </summary>
        public override bool DropOnOwnerLeave { get; set; } = false;

        /// <summary>
        /// If set to true, all custom item pickups dropped by player leaving the server will be destroyed.
        /// If false, all custom item pickups dropped by player leaving the server will remain in the game.
        /// </summary>
        public override bool DestroyOnOwnerLeave { get; set; } = true;

        /// <summary>
        /// Sets the type of the pickup that will be spawned when the item is dropped or spawned.
        /// </summary>
        public override ItemType PickupType { get; set; } = ItemType.Radio;

        /// <summary>
        /// Sets the type of the item that will be added to a player's inventory when the item is given or picked up.
        /// </summary>
        public override ItemType InventoryType { get; set; } = ItemType.Medkit;
    }
}
*/