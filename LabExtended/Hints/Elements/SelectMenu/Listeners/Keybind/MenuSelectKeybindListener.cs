using LabExtended.API.Input;
using LabExtended.API.Input.Inputs;

namespace LabExtended.Hints.Elements.SelectMenu.Listeners.Keybind
{
    public class MenuSelectKeybindListener : InputListener<KeybindInputInfo>
    {
        public override void OnTriggered(KeybindInputInfo info)
        {
            base.OnTriggered(info);

            if (!info.Player.Hints.TryGetElement<SelectMenuElement>(out var selectMenuElement))
                return;

            if (!selectMenuElement.IsActive)
                return;

            selectMenuElement.CurrentOption ??= selectMenuElement.AddedOptions.First();

            if (selectMenuElement.IsSelected(selectMenuElement.CurrentOption))
                selectMenuElement.UnselectOptions(selectMenuElement.CurrentOption.Id);
            else
                selectMenuElement.SelectOptions(selectMenuElement.CurrentOption.Id);
        }
    }
}