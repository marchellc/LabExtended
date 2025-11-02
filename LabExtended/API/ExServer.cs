using GameCore;

using LabExtended.Core;
using LabExtended.Events;

using MEC;

using Mirror;

using PlayerRoles.RoleAssign;
using PlayerStatsSystem;

using RoundRestarting;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace LabExtended.API;

/// <summary>
/// Contains methods and properties related to server configuration and management.
/// </summary>
public static class ExServer
{
    private static volatile bool running = true;

    /// <summary>
    /// Gets the server's version.
    /// </summary>
    public static System.Version Version => ApiVersion.Game;

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
    /// Whether or not the server's process is still running.
    /// </summary>
    public static bool IsRunning => running;

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
    /// Gets the servers actual tick rate (rounded).
    /// </summary>
    public static float Tps => Mathf.Round(1f / Time.smoothDeltaTime);

    /// <summary>
    /// Gets the amount of time required for last frame.
    /// </summary>
    public static float FrameTime => Time.deltaTime;

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
        get => ServerConsole.ServerName;
        set
        {
            ServerConsole.ServerName = value;
            ServerConsole.Singleton?.RefreshServerName();
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
        set => ServerConsole.WhiteListEnabled = value;
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
        get => IsIdleModeActive ? Application.targetFrameRate : ServerStatic.ServerTickrate;
        set
        {
            if (IsIdleModeActive)
                Application.targetFrameRate = value;
            else
                ServerStatic.ServerTickrate = (short)value;
        }
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
        NetworkServer.SendToAll(
            new RoundRestartMessage(RoundRestartType.RedirectRestart, 0f, redirectPort, true, false));
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
        NetworkServer.SendToAll(
            new RoundRestartMessage(RoundRestartType.RedirectRestart, 0f, redirectPort, true, false));
        Timing.CallDelayed(0.5f, Shutdown);
    }

    /// <summary>
    /// Executes a command on the server.
    /// </summary>
    /// <param name="command">The command to execute.</param>
    /// <param name="sender">The command sender to use.</param>
    /// <returns>The command's output.</returns>
    public static string ExecuteCommand(string command, CommandSender? sender = null)
        => GameCore.Console.Singleton.TypeCommand(command, sender ?? ServerConsole.Scs);

    /// <summary>
    /// Shows the server on the list (only works on verified servers).
    /// </summary>
    public static void MakePublic()
        => ExecuteCommand("!public");

    /// <summary>
    /// Hides the server from the server list (only works on verified servers).
    /// </summary>
    public static void MakePrivate()
        => ExecuteCommand("!private");

    private static void OnQuitting()
        => running = false;

    // For some odd reason the tick rate keeps getting reset to 60 once the Facility scene is loaded
    // I suspect it's due to Headless but I ain't dealing with that
    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (ServerStatic.ServerTickrate != Application.targetFrameRate && !IsIdleModeActive)
        {
            Application.targetFrameRate = ServerStatic.ServerTickrate;
        }
    }

    internal static void Internal_Init()
    {
        ExServerEvents.Quitting += OnQuitting;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
}