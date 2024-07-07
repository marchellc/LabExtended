using LabExtended.API.Hints.Elements.SelectMenu;
using LabExtended.Core.Hooking.Interfaces;

namespace LabExtended.Events.SelectMenu
{
    /// <summary>
    /// Gets called when an option gets removed from a select menu.
    /// </summary>
    public class SelectMenuRemovedOptionArgs : IHookEvent
    {
        /// <summary>
        /// The select menu instance.
        /// </summary>
        public SelectMenuElement Element { get; }

        /// <summary>
        /// The option that was removed.
        /// </summary>
        public SelectMenuOption Option { get; }

        internal SelectMenuRemovedOptionArgs(SelectMenuElement element, SelectMenuOption option)
        {
            Element = element;
            Option = option;
        }
    }
}