﻿using LabExtended.API.Settings.Entries;
using LabExtended.API.Settings.Entries.Buttons;
using LabExtended.API.Settings.Entries.Dropdown;

namespace LabExtended.API.Settings.Menus
{
    public abstract class SettingsMenu
    {
        public abstract string CustomId { get; }
        public abstract string Header { get; }
        
        public virtual string HeaderHint { get; }
        
        public virtual bool HeaderReducedPadding { get; }
        
        public ExPlayer Player { get; internal set; }
        
        public bool IsHidden { get; internal set; }

        public SettingsEntry[] Entries { get; internal set; }

        public abstract void BuildMenu(List<SettingsEntry> settings);

        public virtual void OnButtonTriggered(SettingsButton button) { }
        public virtual void OnButtonSwitched(SettingsTwoButtons button) { }
        
        public virtual void OnDropdownSelected(SettingsDropdown dropdown, SettingsDropdownOption option) { }
        
        public virtual void OnKeyBindPressed(SettingsKeyBind keyBind) { }
        
        public virtual void OnPlainTextUpdated(SettingsPlainText plainText) { }
        public virtual void OnSliderMoved(SettingsSlider slider) { }
        public virtual void OnTextInput(SettingsTextArea textArea) { }

        public void HideMenu()
        {
            if (IsHidden) return;
            if (!Player) return;

            IsHidden = true;
            
            SettingsManager.SyncEntries(Player);
        }
        
        public void ShowMenu()
        {
            if (!IsHidden) return;
            if (!Player) return;

            IsHidden = false;
            
            SettingsManager.SyncEntries(Player);
        }
    }
}