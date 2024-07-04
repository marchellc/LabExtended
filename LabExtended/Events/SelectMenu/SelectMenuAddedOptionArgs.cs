using LabExtended.Core.Hooking.Interfaces;
using LabExtended.Hints.Elements.SelectMenu;

namespace LabExtended.Events.SelectMenu
{
    public class SelectMenuAddedOptionArgs : IHookEvent
    {
        public SelectMenuElement Element { get; }
        public SelectMenuOption Option { get; }

        internal SelectMenuAddedOptionArgs(SelectMenuElement element, SelectMenuOption option)
        {
            Element = element;
            Option = option;
        }
    }
}