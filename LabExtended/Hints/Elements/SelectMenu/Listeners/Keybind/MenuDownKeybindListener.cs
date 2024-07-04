using LabExtended.API.Input;
using LabExtended.API.Input.Inputs;

namespace LabExtended.Hints.Elements.SelectMenu.Listeners.Keybind
{
    public class MenuDownKeybindListener : InputListener<KeybindInputInfo>
    {
        public override void OnTriggered(KeybindInputInfo info)
        {
            base.OnTriggered(info);

            if (!info.Player.Hints.TryGetElement<SelectMenuElement>(out var selectMenuElement))
                return;

            if (!selectMenuElement.IsActive)
                return;

            if (selectMenuElement.AddedOptions.Count < 2)
                return;

            if (selectMenuElement.CurrentOption is null)
            {
                selectMenuElement.CurrentOption = selectMenuElement.AddedOptions.First();
                return;
            }

            var nextPos = selectMenuElement.CurrentOption.Position + 1;

            if (nextPos >= selectMenuElement.AddedOptions.Count)
                nextPos = selectMenuElement.AddedOptions.First().Position;

            if (!selectMenuElement.TryGetOption(nextPos.ToString(), out var nextOption))
                return;

            selectMenuElement.CurrentOption = nextOption;
        }
    }
}