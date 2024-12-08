using LabExtended.API.Collections.Locked;

using LabExtended.API.Settings.Entries;
using LabExtended.API.Settings.Entries.Buttons;
using LabExtended.API.Settings.Entries.Dropdown;

namespace LabExtended.API.Settings.Menus
{
    public abstract class SettingsMenu
    {
        public abstract string CustomId { get; }
        public abstract string MenuLabel { get; }

        public ExPlayer Player { get; internal set; }

        public LockedList<SettingsEntry> Settings { get; } = new LockedList<SettingsEntry>();

        public abstract void BuildMenu(List<SettingsEntry> settings);

        public virtual void OnButtonTriggered(SettingsButton button) {}
        public virtual void OnButtonSwitched(SettingsTwoButtons button) {}
        
        public virtual void OnDropdownSelected(SettingsDropdown dropdown, SettingsDropdownOption option) {}
        
        public virtual void OnKeyBindPressed(SettingsKeyBind keyBind) {}
        
        public virtual void OnPlainTextUpdated(SettingsPlainText plainText) {}
        public virtual void OnSliderMoved(SettingsSlider slider) {}
        public virtual void OnTextInput(SettingsTextArea textArea) {}
    }
}