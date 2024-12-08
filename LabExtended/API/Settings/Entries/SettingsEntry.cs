using LabExtended.API.Settings.Menus;
using LabExtended.API.Wrappers;

using UserSettings.ServerSpecific;

namespace LabExtended.API.Settings.Entries
{
    public class SettingsEntry : Wrapper<ServerSpecificSettingBase>
    {
        public SettingsEntry(ServerSpecificSettingBase baseValue, string customId) : base(baseValue)
            => CustomId = customId;

        public ExPlayer Player { get; internal set; }
        public SettingsMenu Menu { get; internal set; }

        public string CustomId { get; }

        public int AssignedId
        {
            get => Base.SettingId;
            set => Base.SettingId = value;
        }

        public string Label
        {
            get => Base.Label;
            set => Base.Label = value;
        }

        internal virtual void InternalOnUpdated() { }
    }
}