using LabExtended.API.Settings.Entries;
using LabExtended.API.Settings.Entries.Buttons;
using LabExtended.API.Settings.Entries.Dropdown;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace LabExtended.API.Settings.Menus;

/// <summary>
/// A base class for setting menus.
/// </summary>
public abstract class SettingsMenu
{
    /// <summary>
    /// The custom ID of the menu.
    /// </summary>
    public abstract string CustomId { get; }
    
    /// <summary>
    /// Header of the menu.
    /// </summary>
    public abstract string Header { get; }

    /// <summary>
    /// Header hint displayed when hovering over the header.
    /// </summary>
    public virtual string HeaderHint { get; } = string.Empty;

    /// <summary>
    /// Whether or not the header should have reduced padding.
    /// </summary>
    public virtual bool HeaderReducedPadding { get; } = false;

    /// <summary>
    /// Gets the player assigned to this menu.
    /// </summary>
    public ExPlayer Player { get; internal set; }

    /// <summary>
    /// Whether or not the menu is currently hidden.
    /// </summary>
    public bool IsHidden { get; internal set; }

    /// <summary>
    /// Array of generated setting entries.
    /// </summary>
    public SettingsEntry[] Entries { get; internal set; }

    /// <summary>
    /// Called when the menu is constructed.
    /// </summary>
    /// <param name="settings">List of settings to add custom entries to.</param>
    public abstract void BuildMenu(List<SettingsEntry> settings);

    /// <summary>
    /// Gets called when a button is pressed.
    /// </summary>
    /// <param name="button">The pressed button.</param>
    public virtual void OnButtonTriggered(SettingsButton button)
    {
    }

    /// <summary>
    /// Gets called when a two-way button is toggled.
    /// </summary>
    /// <param name="button">The button.</param>
    public virtual void OnButtonSwitched(SettingsTwoButtons button)
    {
    }

    /// <summary>
    /// Gets called when a dropdown option is selected.
    /// </summary>
    /// <param name="dropdown">The dropdown.</param>
    /// <param name="option">The selected option.</param>
    public virtual void OnDropdownSelected(SettingsDropdown dropdown, SettingsDropdownOption option)
    {
    }

    /// <summary>
    /// Gets called when a key bind is pressed.
    /// </summary>
    /// <param name="keyBind">The key bind.</param>
    public virtual void OnKeyBindPressed(SettingsKeyBind keyBind)
    {
    }

    /// <summary>
    /// Gets called when a plain text field gets updated.
    /// </summary>
    /// <param name="plainText">The plain text field.</param>
    public virtual void OnPlainTextUpdated(SettingsPlainText plainText)
    {
    }

    /// <summary>
    /// Gets called when a slider is moved.
    /// </summary>
    /// <param name="slider">The slider.</param>
    public virtual void OnSliderMoved(SettingsSlider slider)
    {
    }

    /// <summary>
    /// Gets called when a text input changes.
    /// </summary>
    /// <param name="textArea">The text input area.</param>
    public virtual void OnTextInput(SettingsTextArea textArea)
    {
    }

    /// <summary>
    /// Hides this menu (if not hidden).
    /// </summary>
    public void HideMenu()
    {
        if (IsHidden) 
            return;
        
        if (!Player) 
            return;

        IsHidden = true;

        SettingsManager.SyncEntries(Player);
    }

    /// <summary>
    /// Shows this menu (if hidden).
    /// </summary>
    public void ShowMenu()
    {
        if (!IsHidden)
            return;
        
        if (!Player)
            return;

        IsHidden = false;

        SettingsManager.SyncEntries(Player);
    }
}