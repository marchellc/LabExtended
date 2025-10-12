using GameCore;

using LabExtended.Commands.Attributes;

namespace LabExtended.Commands.Custom.Reload;

public partial class ReloadCommand
{
    [CommandOverload("gameconfig", "Reloads the game's gameplay and Remote Admin configuration.", null)]
    public void GameConfigOverload()
    {
        ConfigFile.ReloadGameConfigs(false);
        
        ServerStatic.RolesConfig = new YamlConfig(ServerStatic.RolesConfigPath ?? (FileManager.GetAppFolder(true, true, "") + "config_remoteadmin.txt"));
        
        ServerStatic.SharedGroupsConfig = ((ConfigSharing.Paths[4] == null) ? null : new YamlConfig(ConfigSharing.Paths[4] + "shared_groups.txt"));
        ServerStatic.SharedGroupsMembersConfig = ((ConfigSharing.Paths[5] == null) ? null : new YamlConfig(ConfigSharing.Paths[5] + "shared_groups_members.txt"));
        
        ServerStatic.PermissionsHandler = new PermissionsHandler(ref ServerStatic.RolesConfig, ref ServerStatic.SharedGroupsConfig, ref ServerStatic.SharedGroupsMembersConfig);
        
        Ok($"Reloaded game configuration.");
    }
}