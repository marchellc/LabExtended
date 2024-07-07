using LabExtended.API.Hints.Elements.SelectMenu;
using LabExtended.Core.Hooking.Interfaces;

namespace LabExtended.Events.SelectMenu
{
    /// <summary>
    /// Gets called when a new option is added to the select menu.
    /// </summary>
    public class SelectMenuAddedOptionArgs : IHookEvent
    {
        /// <summary>
        /// The select menu instance.
        /// </summary>
        public SelectMenuElement Element { get; }

        /// <summary>
        /// The option that was added.
        /// </summary>
        public SelectMenuOption Option { get; }

        internal SelectMenuAddedOptionArgs(SelectMenuElement element, SelectMenuOption option)
        {
            Element = element;
            Option = option;
        }
    }
}