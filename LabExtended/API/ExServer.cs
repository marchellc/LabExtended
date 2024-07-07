﻿using GameCore;
using LabExtended.Core;

using MEC;

using Mirror;

using PlayerRoles.RoleAssign;
using PlayerStatsSystem;

using RoundRestarting;

using UnityEngine;

namespace LabExtended.API
{
    public static class ExServer
    {
        /// <summary>
        /// Gets the server's version.
        /// </summary>
        public static System.Version Version => ExLoader.GameVersion;

        /// <summary>
        /// Gets the time of the server starting.
        /// </summary>
        public static DateTime StartedAt => DateTime.Now - TimeSpan.FromSeconds(Time.realtimeSinceStartupAsDouble);

        /// <summary>
        /// Gets the amount of time that passed since the server started.
        /// </summary>
        public static TimeSpan TimeSinceStarted => TimeSpan.FromSeconds(Time.realtimeSinceStartupAsDouble);

        /// <summary>
        /// Gets the server's build type.
        /// </summary>
        public static GameCore.Version.VersionType BuildType => GameCore.Version.BuildType;

        /// <summary>
        /// Gets a value indicating whether or not this version can be streamed.
        /// </summary>
        public static bool IsStreamingAllowed => GameCore.Version.StreamingAllowed;

        /// <summary>
        /// Gets a value indicating whether or not this version is a beta build.
        /// </summary>
        public static bool IsBetaBuild => GameCore.Version.PublicBeta || GameCore.Version.PrivateBeta;

        /// <summary>
        /// Gets a value indicating whether or not this is a dedicated server.
        /// </summary>
        public static bool IsDedicated => ServerStatic.IsDedicated;

        /// <summary>
        /// Gets a value indicating whether or not Late Join is enabled.
        /// </summary>
        public static bool IsLateJoinEnabled => LateJoinTime > 0f;

        /// <summary>
        /// Gets a value indicating whether or not the server is verified.
        /// </summary>
        public static bool IsVerified => CustomNetworkManager.IsVerified;

        /// <summary>
        /// Gets the server's connection port.
        /// </summary>
        public static ushort Port => ServerStatic.ServerPort;

        /// <summary>
        /// Gets the servers actual tick rate.
        /// </summary>
        public static double Tps => Math.Round(1f / Time.smoothDeltaTime);

        /// <summary>
        /// Gets the amount of time required for each frame.
        /// </summary>
        public static double FrameTime => Math.Round(1f / Time.deltaTime);

        /// <summary>
        /// Gets the amount of active players.
        /// </summary>
        public static int PlayerCount => ExPlayer.Count;

        /// <summary>
        /// Gets the server's version string.
        /// </summary>
        public static string VersionString => GameCore.Version.VersionString;

        /// <summary>
        /// Gets or sets the server's name.
        /// </summary>
        public static string Name
        {
            get => ServerConsole._serverName;
            set
            {
                ServerConsole._serverName = value;
                ServerConsole.singleton?.RefreshServerName();
            }
        }

        /// <summary>
        /// Gets or sets the server's connection IP.
        /// </summary>
        public static string Ip
        {
            get => ServerConsole.Ip;
            set => ServerConsole.Ip = value;
        }

        /// <summary>
        /// Enables or disables friendly fire on the server.
        /// </summary>
        public static bool FriendlyFire
        {
            get => ServerConsole.FriendlyFire;
            set
            {
                ServerConsole.FriendlyFire = value;
                ServerConfigSynchronizer.Singleton?.RefreshMainBools();
                AttackerDamageHandler.RefreshConfigs();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether or not to enforce the same IP for each token.
        /// </summary>
        public static bool EnforceSameIp
        {
            get => ServerConsole.EnforceSameIp;
            set => ServerConsole.EnforceSameIp = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether or not to display the Heavily Modded flag in the server list.
        /// </summary>
        public static bool IsHeavilyModded
        {
            get => ServerConsole.TransparentlyModdedServerConfig;
            set => ServerConsole.TransparentlyModdedServerConfig = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether or not the whitelist is active.
        /// </summary>
        public static bool IsWhitelistEnabled
        {
            get => ServerConsole.WhiteListEnabled;
            set => ServerConsole.WhiteListEnabled = true;
        }

        /// <summary>
        /// Gets or sets a value indicating whether or not the Idle Mode is enabled.
        /// </summary>
        public static bool IsIdleModeEnabled
        {
            get => IdleMode.IdleModeEnabled;
            set => IdleMode.IdleModeEnabled = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether or not the Idle Mode is active.
        /// </summary>
        public static bool IsIdleModeActive
        {
            get => IdleMode.IdleModeActive;
            set => IdleMode.IdleModeActive = value;
        }

        /// <summary>
        /// Gets or sets the maximum amount of time that can pass since the round start for a player to be spawned via Late Join.
        /// </summary>
        public static float LateJoinTime
        {
            get => ConfigFile.ServerConfig.GetFloat(RoleAssigner.LateJoinKey, 0f);
            set
            {
                ConfigFile.ServerConfig.SetString(RoleAssigner.LateJoinKey, value.ToString());
                ConfigFile.ServerConfig.Reload();
            }
        }

        /// <summary>
        /// Gets or sets the server's maximum amount of players.
        /// </summary>
        public static int MaxSlots
        {
            get => CustomNetworkManager.slots;
            set => CustomNetworkManager.slots = value;
        }

        /// <summary>
        /// Gets or sets the amount
        /// </summary>
        public static int ReservedSlots
        {
            get => CustomNetworkManager.reservedSlots;
            set => CustomNetworkManager.reservedSlots = value;
        }

        /// <summary>
        /// Gets or sets the server's target frame rate.
        /// </summary>
        public static int TargetFrameRate
        {
            get => Application.targetFrameRate;
            set => Application.targetFrameRate = value;
        }

        /// <summary>
        /// Restarts the server.
        /// </summary>
        public static void Restart()
            => ExRound.Restart(false, ServerStatic.NextRoundAction.Restart);

        /// <summary>
        /// Restarts the server and redirect's all players to another server.
        /// </summary>
        /// <param name="redirectPort">The port to redirect all players to.</param>
        public static void RestartRedirect(ushort redirectPort)
        {
            NetworkServer.SendToAll(new RoundRestartMessage(RoundRestartType.RedirectRestart, 0f, redirectPort, true, false));
            Timing.CallDelayed(0.5f, Restart);
        }

        /// <summary>
        /// Shuts the server down.
        /// </summary>
        public static void Shutdown()
            => ExRound.Restart(false, ServerStatic.NextRoundAction.Shutdown);

        /// <summary>
        /// Shuts the server down and redirects all players to another server.
        /// </summary>
        /// <param name="redirectPort">The port to redirect all players to.</param>
        public static void ShutdownRedirect(ushort redirectPort)
        {
            NetworkServer.SendToAll(new RoundRestartMessage(RoundRestartType.RedirectRestart, 0f, redirectPort, true, false));
            Timing.CallDelayed(0.5f, Shutdown);
        }

        /// <summary>
        /// Executes a command on the server.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        /// <param name="sender">The command sender to use.</param>
        /// <returns>The command's output.</returns>
        public static string ExecuteCommand(string command, CommandSender sender = null)
            => GameCore.Console.singleton.TypeCommand(command, sender);

        /// <summary>
        /// Shows the server on the list.
        /// </summary>
        public static void MakePublic()
            => ExecuteCommand("!public");

        /// <summary>
        /// Hides the server from the server list.
        /// </summary>
        public static void MakePrivate()
            => ExecuteCommand("!private");
    }
}