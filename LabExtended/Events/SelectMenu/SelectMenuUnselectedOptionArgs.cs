using LabExtended.API.Hints.Elements.SelectMenu;
using LabExtended.Core.Hooking.Interfaces;

namespace LabExtended.Events.SelectMenu
{
    /// <summary>
    /// Gets called when an option is unselected by a player.
    /// </summary>
    public class SelectMenuUnselectedOptionArgs : IHookEvent
    {
        /// <summary>
        /// The select menu instance.
        /// </summary>
        public SelectMenuElement Element { get; }

        /// <summary>
        /// The option that was unselected.
        /// </summary>
        public SelectMenuOption Option { get; }

        internal SelectMenuUnselectedOptionArgs(SelectMenuElement element, SelectMenuOption option)
        {
            Element = element;
            Option = option;
        }
    }
}