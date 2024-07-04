using LabExtended.Core.Hooking.Interfaces;
using LabExtended.Hints.Elements.SelectMenu;

namespace LabExtended.Events.SelectMenu
{
    public class SelectMenuSelectedOptionArgs : IHookEvent
    {
        public SelectMenuElement Element { get; }
        public SelectMenuOption Option { get; }

        internal SelectMenuSelectedOptionArgs(SelectMenuElement element, SelectMenuOption option)
        {
            Element = element;
            Option = option;
        }
    }
}