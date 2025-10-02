using LabExtended.API.Custom.Items;

using System.ComponentModel;

namespace LabExtended.API.Custom.Firearms
{
    /// <summary>
    /// Base class for custom firearms.
    /// </summary>
    public abstract class CustomFirearm : CustomItem
    {
        /// <summary>
        /// Gets or sets the type of ammo used by this firearm. Default ammo type will be used if set to <see cref="ItemType.None"/>.
        /// </summary>
        [Description("Sets the type of the ammo used by this firearm. Default ammo type will be used if set to \"None\".")]
        public virtual ItemType AmmoType { get; set; } = ItemType.None;
    }
}