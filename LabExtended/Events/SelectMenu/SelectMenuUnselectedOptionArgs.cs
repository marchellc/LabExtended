using LabExtended.Core.Hooking.Interfaces;
using LabExtended.Hints.Elements.SelectMenu;

namespace LabExtended.Events.SelectMenu
{
    public class SelectMenuUnselectedOptionArgs : IHookEvent
    {
        public SelectMenuElement Element { get; }
        public SelectMenuOption Option { get; }

        internal SelectMenuUnselectedOptionArgs(SelectMenuElement element, SelectMenuOption option)
        {
            Element = element;
            Option = option;
        }
    }
}