using LabExtended.Core.Hooking.Interfaces;
using LabExtended.Hints.Elements.SelectMenu;

namespace LabExtended.Events.SelectMenu
{
    public class SelectMenuRemovedOptionArgs : IHookEvent
    {
        public SelectMenuElement Element { get; }
        public SelectMenuOption Option { get; }

        internal SelectMenuRemovedOptionArgs(SelectMenuElement element, SelectMenuOption option)
        {
            Element = element;
            Option = option;
        }
    }
}