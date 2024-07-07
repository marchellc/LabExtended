using LabExtended.API.Hints.Elements.SelectMenu;
using LabExtended.Core.Hooking.Interfaces;

namespace LabExtended.Events.SelectMenu
{
    /// <summary>
    /// Gets called when a player changes their selected option.
    /// </summary>
    public class SelectMenuSelectedOptionArgs : IHookEvent
    {
        /// <summary>
        /// The select menu instance.
        /// </summary>
        public SelectMenuElement Element { get; }

        /// <summary>
        /// The selected option.
        /// </summary>
        public SelectMenuOption Option { get; }

        internal SelectMenuSelectedOptionArgs(SelectMenuElement element, SelectMenuOption option)
        {
            Element = element;
            Option = option;
        }
    }
}