using CommandSystem;

using LabExtended.API.Settings;
using LabExtended.API.Settings.Entries;
using LabExtended.API.Settings.Entries.Buttons;
using LabExtended.API.Settings.Entries.Dropdown;
using LabExtended.API.Settings.Menus;

using LabExtended.Commands;
using LabExtended.Commands.Arguments;
using LabExtended.Commands.Interfaces;

using UserSettings.ServerSpecific;

using UnityEngine;

namespace LabExtended.API.CustomCommands.Debug.Settings
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    public class SettingsTestCommand : CustomCommand
    {
        public override string Command => "settingstest";
        public override string Description => "Toggles the Settings API test.";

        public override void OnCommand(ExPlayer sender, ICommandContext ctx, ArgumentCollection args)
        {
            base.OnCommand(sender, ctx, args);

            if (SettingsManager.HasBuilder("ApiTestBuilder"))
            {
                SettingsManager.RemoveBuilder("ApiTestBuilder");

                ctx.RespondOk("Disabled the testing builder.");
                return;
            }

            SettingsManager.AddBuilder(new SettingsBuilder("ApiTestBuilder")
            {
                MenuBuilders = ApplyTestMenu
            });

            ctx.RespondOk("Enabled the testing menu builder.");
        }

        private static void ApplyTestMenu(List<SettingsMenu> menus)
            => menus.Add(new TestMenu());
    }

    public class TestMenu : SettingsMenu
    {
        public override string CustomId { get; } = "testMenu";
        public override string MenuLabel { get; } = "Test Menu";

        public override void BuildMenu(List<SettingsEntry> settings)
        {
            settings.Add(SettingsKeyBind.Create("testKeyBind", "Test Key Bind", KeyCode.AltGr));
            settings.Add(SettingsPlainText.Create("testPlainText", "Test Plain Text", "Hello"));
            settings.Add(SettingsSlider.Create("testSlider", "Test Slider", 0f, 1f, 0f));
            settings.Add(SettingsTextArea.Create("testTextArea", "Test Text Area", "Test Text Area - Collapsed"));
            settings.Add(SettingsDropdown.Create("testDropdown", "Test Dropdown", 0, SSDropdownSetting.DropdownEntryType.Regular, dp => dp.AddOption(0, "Option One").AddOption(1, "Option Two")));
            settings.Add(SettingsButton.Create("testButton", "Test Button", "Test Button Text"));
            settings.Add(SettingsTwoButtons.Create("testTwoButtons", "Test Two Buttons", "Test Two Buttons A", "Test Two Buttons B"));
        }
    }
}