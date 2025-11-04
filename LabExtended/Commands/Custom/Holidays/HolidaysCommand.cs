using LabExtended.API;

using LabExtended.Commands.Attributes;
using LabExtended.Commands.Interfaces;

namespace LabExtended.Commands.Custom.Holidays
{
    /// <summary>
    /// Provides server-side commands related to the active in-game holiday update, including operations for managing
    /// holiday-specific features and entities.
    /// </summary>
    /// <remarks>This command class includes functionality for activating holiday-themed features, such as the
    /// Hubert skybox and Hubert Moon entity, during special in-game events. All commands are intended for server-side
    /// use and may affect the current game state for all players.</remarks>
    [Command("holidays", "Functions related to the active in-game holiday update.")]
    public class HolidaysCommand : CommandBase, IServerSideCommand
    {
        #region Halloween 2025 - 14.1.2
        /// <summary>
        /// Sets the active status of the Hubert skybox.
        /// </summary>
        /// <param name="status">true to activate the Hubert skybox; otherwise, false.</param>
        [CommandOverload("hubert skybox", "Sets the Hubert skybox status.", null)]
        public void HubertSkybox(
            [CommandParameter("Status", "The status of the skybox to set.")] bool status)
        {
            ExMap.IsHubertSkyboxActive = status;

            Ok($"Hubert Skybox set to: {ExMap.IsHubertSkyboxActive}");
        }

        /// <summary>
        /// Spawns a new Hubert Moon instance in the current context.
        /// </summary>
        /// <remarks>Use this method to create and register a Hubert Moon entity. The method outputs a
        /// confirmation message including the unique network identifier of the spawned instance.</remarks>
        [CommandOverload("hubert moon", "Spawns a Hubert Moon instance.", null)]
        public void HubertMoon()
        {
            var instance = ExMap.SpawnHubertMoon();

            Ok($"Spawned a Hubert Moon instance (ID: {instance.netId})");
        }
        #endregion
    }
}