using LabExtended.API;

using LabExtended.Extensions;

using LabExtended.Events.Player;
using LabExtended.Events.Player.Snake;
using LabExtended.Events.Player.Settings;

using System.Text;

#pragma warning disable CS8604 // Possible null reference argument.

namespace LabExtended.Events;

/// <summary>
/// Player-related events.
/// </summary>
public static class ExPlayerEvents
{
    #region Join & Leave
    /// <summary>
    /// Gets called when a new player joins the server.
    /// </summary>
    public static event Action<ExPlayer>? Joined;

    /// <summary>
    /// Gets called when a new NPC joins the server.
    /// </summary>
    public static event Action<ExPlayer>? NpcJoined;

    /// <summary>
    /// Gets called when the local server player joins the server.
    /// </summary>
    public static event Action<ExPlayer>? HostJoined;

    /// <summary>
    /// Gets called when a player finishes authentification.
    /// </summary>
    public static event Action<ExPlayer>? Verified;

    /// <summary>
    /// Gets called when an NPC player finishes authentification.
    /// </summary>
    public static event Action<ExPlayer>? NpcVerified;

    /// <summary>
    /// Gets called when the local server player finishes authentification.
    /// </summary>
    public static event Action<ExPlayer>? HostVerified;

    /// <summary>
    /// Gets called after the player's object is destroyed.
    /// </summary>
    public static event Action<ExPlayer>? Left;

    /// <summary>
    /// Gets called after an NPC player's object is destroyed.
    /// </summary>
    public static event Action<ExPlayer>? NpcLeft;

    /// <summary>
    /// Gets called after the local server player's object is destroyed.
    /// </summary>
    public static event Action<ExPlayer>? HostLeft;

    /// <inheritdoc cref="PlayerLeavingEventArgs"/>
    public static event Action<PlayerLeavingEventArgs>? Leaving;
    #endregion

    #region Remote Admin
    /// <inheritdoc cref="PlayerOpenedRemoteAdminEventArgs"/>
    public static event Action<PlayerOpenedRemoteAdminEventArgs>? OpenedRemoteAdmin; 
    
    /// <inheritdoc cref="PlayerClosedRemoteAdminEventArgs"/>
    public static event Action<PlayerClosedRemoteAdminEventArgs>? ClosedRemoteAdmin; 
    
    /// <inheritdoc cref="PlayerTogglingLobbyLockEventArgs"/>
    public static event Action<PlayerTogglingLobbyLockEventArgs>? TogglingLobbyLock; 
    
    /// <inheritdoc cref="PlayerTogglingRoundLockEventArgs"/>
    public static event Action<PlayerTogglingRoundLockEventArgs>? TogglingRoundLock; 
    #endregion
    
    #region User Settings
    /// <inheritdoc cref="PlayerSettingsTabOpenedEventArgs"/>
    public static event Action<PlayerSettingsTabOpenedEventArgs>? SettingsTabOpened;
    
    /// <inheritdoc cref="PlayerSettingsTabClosedEventArgs"/>
    public static event Action<PlayerSettingsTabClosedEventArgs>? SettingsTabClosed;
    
    /// <inheritdoc cref="PlayerSettingsEntryCreatedEventArgs"/>
    public static event Action<PlayerSettingsEntryCreatedEventArgs>? SettingsEntryCreated; 
    
    /// <inheritdoc cref="PlayerSettingsEntryUpdatedEventArgs"/>
    public static event Action<PlayerSettingsEntryUpdatedEventArgs>? SettingsEntryUpdated;
    
    /// <inheritdoc cref="PlayerSettingsStatusReportReceivedEventArgs"/>
    public static event Action<PlayerSettingsStatusReportReceivedEventArgs>? SettingsStatusReportReceived; 
    #endregion
    
    #region Miscellaneous
    /// <inheritdoc cref="PlayerOverridingPositionEventArgs"/>
    public static event Action<PlayerOverridingPositionEventArgs>? OverridingPosition; 
    
    /// <inheritdoc cref="PlayerObservingScp173EventArgs"/>
    public static event Action<PlayerObservingScp173EventArgs>? ObservingScp173; 
    
    /// <inheritdoc cref="PlayerTriggeringTeslaGateEventArgs"/>
    public static event Action<PlayerTriggeringTeslaGateEventArgs>? TriggeringTesla;

    /// <inheritdoc cref="PlayerRefreshingModifiersEventArgs"/>
    public static event Action<PlayerRefreshingModifiersEventArgs>? RefreshingModifiers;

    /// <summary>
    /// Gets called every frame if the player has the custom info flag enabled.
    /// </summary>
    public static event Action<ExPlayer, StringBuilder>? RefreshingCustomInfo;
    #endregion

    #region Items
    /// <inheritdoc cref="PlayerDroppingCandyEventArgs"/>
    public static event Action<PlayerDroppingCandyEventArgs>? DroppingCandy;
    
    /// <inheritdoc cref="PlayerReceivingCandyEventArgs"/>
    public static event Action<PlayerReceivingCandyEventArgs>? ReceivingCandy; 
    
    /// <inheritdoc cref="PlayerThrowingItemEventArgs"/>
    public static event Action<PlayerThrowingItemEventArgs>? ThrowingItem; 
    
    /// <inheritdoc cref="PlayerSelectingItemEventArgs"/>
    public static event Action<PlayerSelectingItemEventArgs>? SelectingItem; 
    
    /// <inheritdoc cref="PlayerSelectedItemEventArgs"/>
    public static event Action<PlayerSelectedItemEventArgs>? SelectedItem;

    /// <inheritdoc cref="PlayerShootingFirearmEventArgs"/>
    public static event Action<PlayerShootingFirearmEventArgs>? ShootingFirearm;

    /// <inheritdoc cref="PlayerShotFirearmEventArgs"/>
    public static event Action<PlayerShotFirearmEventArgs>? ShotFirearm;

    /// <inheritdoc cref="PlayerChangingFirearmAttachmentsEventArgs"/>
    public static event Action<PlayerChangingFirearmAttachmentsEventArgs>? ChangingAttachments;

    /// <inheritdoc cref="PlayerChangedFirearmAttachmentsEventArgs"/>
    public static event Action<PlayerChangedFirearmAttachmentsEventArgs>? ChangedAttachments;

    /// <inheritdoc cref="PlayerSynchronizingRoleEventArgs"/>
    public static event Action<PlayerSynchronizingRoleEventArgs>? SynchronizingRole;

    /// <inheritdoc cref="PlayerSynchronizedRoleEventArgs"/>
    public static event Action<PlayerSynchronizedRoleEventArgs>? SynchronizedRole; 

    /// <inheritdoc cref="PlayerSnakeChangedDirectionEventArgs"/>
    public static event Action<PlayerSnakeChangedDirectionEventArgs>? SnakeChangedDirection;

    /// <inheritdoc cref="PlayerSnakeGameOverEventArgs"/>
    public static event Action<PlayerSnakeGameOverEventArgs>? SnakeGameOver;

    /// <inheritdoc cref="PlayerSnakeStartedEventArgs"/>
    public static event Action<PlayerSnakeStartedEventArgs>? SnakeStarted;
    
    /// <inheritdoc cref="PlayerSnakeStoppedEventArgs"/>
    public static event Action<PlayerSnakeStoppedEventArgs>? SnakeStopped; 

    /// <inheritdoc cref="PlayerSnakeEatenEventArgs"/>
    public static event Action<PlayerSnakeEatenEventArgs>? SnakeEaten;

    /// <inheritdoc cref="PlayerSnakeMovedEventArgs"/>
    public static event Action<PlayerSnakeMovedEventArgs>? SnakeMoved; 
    #endregion

    #region Handlers - Join & Leave
    /// <summary>
    /// Invokes the <see cref="Joined"/> event.
    /// </summary>
    /// <param name="player">Player who joined.</param>
    public static void OnJoined(ExPlayer player)
        => Joined.InvokeSafe(player);

    /// <summary>
    /// Invokes the <see cref="NpcJoined"/> event.
    /// </summary>
    /// <param name="player">Player who joined.</param>
    public static void OnNpcJoined(ExPlayer player)
        => NpcJoined.InvokeSafe(player);

    /// <summary>
    /// Invokes the <see cref="HostJoined"/> event.
    /// </summary>
    /// <param name="player">Player who joined.</param>
    public static void OnHostJoined(ExPlayer player)
        => HostJoined.InvokeSafe(player);

    /// <summary>
    /// Invokes the <see cref="Verified"/> event.
    /// </summary>
    /// <param name="player">The player who just verified.</param>
    public static void OnVerified(ExPlayer player)
        => Verified.InvokeSafe(player);

    /// <summary>
    /// Invokes the <see cref="NpcVerified"/> event.
    /// </summary>
    /// <param name="player">Player who joined.</param>
    public static void OnNpcVerified(ExPlayer player)
        => NpcJoined.InvokeSafe(player);

    /// <summary>
    /// Invokes the <see cref="HostVerified"/> event.
    /// </summary>
    /// <param name="player">Player who joined.</param>
    public static void OnHostVerified(ExPlayer player)
        => HostJoined.InvokeSafe(player);

    /// <summary>
    /// Invokes the <see cref="Left"/> event.
    /// </summary>
    /// <param name="player">Player who left.</param>
    public static void OnLeft(ExPlayer player)
        => Left.InvokeSafe(player);

    /// <summary>
    /// Invokes the <see cref="NpcLeft"/> event.
    /// </summary>
    /// <param name="player">Player who left.</param>
    public static void OnNpcLeft(ExPlayer player)
        => NpcLeft.InvokeSafe(player);

    /// <summary>
    /// Invokes the <see cref="HostLeft"/> event.
    /// </summary>
    /// <param name="player">Player who left.</param>
    public static void OnHostLeft(ExPlayer player)
        => HostLeft.InvokeSafe(player);

    /// <summary>
    /// Invokes the <see cref="Leaving"/> event.
    /// </summary>
    /// <param name="args">The event's arguments.</param>
    public static void OnLeaving(PlayerLeavingEventArgs args)
        => Leaving.InvokeEvent(args);
    #endregion 
    
    #region Handlers - Remote Admin
    /// <summary>
    /// Invokes the <see cref="OpenedRemoteAdmin"/> event.
    /// </summary>
    /// <param name="args">The event's arguments.</param>
    public static void OnOpenedRemoteAdmin(PlayerOpenedRemoteAdminEventArgs args)
        => OpenedRemoteAdmin.InvokeEvent(args);
    
    /// <summary>
    /// Invokes the <see cref="ClosedRemoteAdmin"/> event.
    /// </summary>
    /// <param name="args">The event's arguments.</param>
    public static void OnClosedRemoteAdmin(PlayerClosedRemoteAdminEventArgs args)
        => ClosedRemoteAdmin.InvokeEvent(args);
    
    /// <summary>
    /// Invokes the <see cref="TogglingLobbyLock"/> event.
    /// </summary>
    /// <param name="args">The event's arguments.</param>
    /// <returns>The event's <see cref="BooleanEventArgs.IsAllowed"/> property.</returns>
    public static bool OnTogglingLobbyLock(PlayerTogglingLobbyLockEventArgs args)
        => TogglingLobbyLock.InvokeBooleanEvent(args);
    
    /// <summary>
    /// Invokes the <see cref="TogglingRoundLock"/> event.
    /// </summary>
    /// <param name="args">The event's arguments.</param>
    /// <returns>The event's <see cref="BooleanEventArgs.IsAllowed"/> property.</returns>
    public static bool OnTogglingRoundLock(PlayerTogglingRoundLockEventArgs args)
        => TogglingRoundLock.InvokeBooleanEvent(args);
    #endregion
    
    #region Handlers - User Settings
    /// <summary>
    /// Invokes the <see cref="SettingsTabOpened"/> event.
    /// </summary>
    /// <param name="args">The event's arguments.</param>
    public static void OnSettingsTabOpened(PlayerSettingsTabOpenedEventArgs args)
        => SettingsTabOpened.InvokeEvent(args);
    
    /// <summary>
    /// Invokes the <see cref="SettingsTabClosed"/> event.
    /// </summary>
    /// <param name="args">The event's arguments.</param>
    public static void OnSettingsTabClosed(PlayerSettingsTabClosedEventArgs args)
        => SettingsTabClosed.InvokeEvent(args);
    
    /// <summary>
    /// Invokes the <see cref="SettingsEntryCreated"/> event.
    /// </summary>
    /// <param name="args">The event's arguments.</param>
    public static void OnSettingsEntryCreated(PlayerSettingsEntryCreatedEventArgs args)
        => SettingsEntryCreated.InvokeEvent(args);
    
    /// <summary>
    /// Invokes the <see cref="SettingsEntryUpdated"/> event.
    /// </summary>
    /// <param name="args">The event's arguments.</param>
    public static void OnSettingsEntryUpdated(PlayerSettingsEntryUpdatedEventArgs args)
        => SettingsEntryUpdated.InvokeEvent(args);
    
    /// <summary>
    /// Invokes the <see cref="SettingsStatusReportReceived"/> event.
    /// </summary>
    /// <param name="args">The event's arguments.</param>
    public static void OnSettingsStatusReportReceived(PlayerSettingsStatusReportReceivedEventArgs args)
        => SettingsStatusReportReceived.InvokeEvent(args);
    #endregion
    
    #region Handlers - Miscellaneous
    /// <summary>
    /// Invokes the <see cref="OverridingPosition"/> event.
    /// </summary>
    /// <param name="args">The event's arguments.</param>
    /// <returns>The event's <see cref="BooleanEventArgs.IsAllowed"/> property.</returns>
    public static bool OnOverridingPosition(PlayerOverridingPositionEventArgs args)
        => OverridingPosition.InvokeBooleanEvent(args);
    
    /// <summary>
    /// Invokes the <see cref="ObservingScp173"/> event.
    /// </summary>
    /// <param name="args">The event's arguments.</param>
    /// <returns>The event's <see cref="BooleanEventArgs.IsAllowed"/> property.</returns>
    public static bool OnObservingScp173(PlayerObservingScp173EventArgs args)
        => ObservingScp173.InvokeBooleanEvent(args);
    
    /// <summary>
    /// Invokes the <see cref="TriggeringTesla"/> event.
    /// </summary>
    /// <param name="args">The event's arguments.</param>
    /// <returns>The event's <see cref="BooleanEventArgs.IsAllowed"/> property.</returns>
    public static bool OnTriggeringTeslaGate(PlayerTriggeringTeslaGateEventArgs args)
        => TriggeringTesla.InvokeBooleanEvent(args);
    
    /// <summary>
    /// Invokes the <see cref="SynchronizingRole"/> event.
    /// </summary>
    /// <param name="args">The event's arguments.</param>
    /// <returns>The event's <see cref="BooleanEventArgs.IsAllowed"/> property.</returns>
    public static bool OnSynchronizingRole(PlayerSynchronizingRoleEventArgs args)
        => SynchronizingRole.InvokeBooleanEvent(args);
    
    /// <summary>
    /// Invokes the <see cref="SynchronizedRole"/> event.
    /// </summary>
    /// <param name="args">The event's arguments.</param>
    public static void OnSynchronizedRole(PlayerSynchronizedRoleEventArgs args)
        => SynchronizedRole.InvokeEvent(args);

    /// <summary>
    /// Invokes the <see cref="RefreshingModifiers"/> event.
    /// </summary>
    /// <param name="args">The event's arguments.</param>
    public static void OnRefreshingModifiers(PlayerRefreshingModifiersEventArgs args)
        => RefreshingModifiers.InvokeEvent(args);

    /// <summary>
    /// Invokes the <see cref="RefreshingCustomInfo"/> event.
    /// </summary>
    public static void OnRefreshingCustomInfo(ExPlayer player, StringBuilder builder)
        => RefreshingCustomInfo.InvokeSafe(player, builder);
    #endregion

    #region Handlers - Items
    /// <summary>
    /// Invokes the <see cref="DroppingCandy"/> event.
    /// </summary>
    /// <param name="args">The event's arguments.</param>
    /// <returns>The event's <see cref="BooleanEventArgs.IsAllowed"/> property.</returns>
    public static bool OnDroppingCandy(PlayerDroppingCandyEventArgs args)
        => DroppingCandy.InvokeBooleanEvent(args);
    
    /// <summary>
    /// Invokes the <see cref="ReceivingCandy"/> event.
    /// </summary>
    /// <param name="args">The event's arguments.</param>
    /// <returns>The event's <see cref="BooleanEventArgs.IsAllowed"/> property.</returns>
    public static bool OnReceivingCandy(PlayerReceivingCandyEventArgs args)
        => ReceivingCandy.InvokeBooleanEvent(args);
    
    /// <summary>
    /// Invokes the <see cref="ThrowingItem"/> event.
    /// </summary>
    /// <param name="args">The event's arguments.</param>
    /// <returns>The event's <see cref="BooleanEventArgs.IsAllowed"/> property.</returns>
    public static bool OnThrowingItem(PlayerThrowingItemEventArgs args)
        => ThrowingItem.InvokeBooleanEvent(args);
    
    /// <summary>
    /// Invokes the <see cref="SelectingItem"/> event.
    /// </summary>
    /// <param name="args">The event's arguments.</param>
    /// <returns>The event's <see cref="BooleanEventArgs.IsAllowed"/> property.</returns>
    public static bool OnSelectingItem(PlayerSelectingItemEventArgs args)
        => SelectingItem.InvokeBooleanEvent(args);

    /// <summary>
    /// Invokes the <see cref="SelectedItem"/> event.
    /// </summary>
    /// <param name="args">The event's arguments.</param>
    public static void OnSelectedItem(PlayerSelectedItemEventArgs args)
        => SelectedItem.InvokeEvent(args);

    /// <summary>
    /// Invokes the <see cref="ShootingFirearm"/> event.
    /// </summary>
    /// <param name="args">The event's arguments.</param>
    /// <returns>The event's <see cref="BooleanEventArgs.IsAllowed"/> property.</returns>
    public static bool OnShootingFirearm(PlayerShootingFirearmEventArgs args)
        => ShootingFirearm.InvokeBooleanEvent(args);

    /// <summary>
    /// Invokes the <see cref="ShotFirearm"/> event.
    /// </summary>
    /// <param name="args">The event's arguments.</param>
    public static void OnShotFirearm(PlayerShotFirearmEventArgs args)
        => ShotFirearm.InvokeEvent(args);

    /// <summary>
    /// Invokes the <see cref="ChangingAttachments"/> event.
    /// </summary>
    /// <param name="args">The event's arguments.</param>
    /// <returns>The event's <see cref="BooleanEventArgs.IsAllowed"/> property.</returns>
    public static bool OnChangingAttachments(PlayerChangingFirearmAttachmentsEventArgs args)
        => ChangingAttachments.InvokeBooleanEvent(args);

    /// <summary>
    /// Invokes the <see cref="ChangedAttachments"/> event.
    /// </summary>
    /// <param name="args">The event's arguments.</param>
    public static void OnChangedAttachments(PlayerChangedFirearmAttachmentsEventArgs args)
        => ChangedAttachments.InvokeEvent(args);
    
    /// <summary>
    /// Invokes the <see cref="SnakeChangedDirection"/> event.
    /// </summary>
    /// <param name="args">The event's arguments.</param>
    public static void OnSnakeChangedDirection(PlayerSnakeChangedDirectionEventArgs args)
        => SnakeChangedDirection.InvokeEvent(args);
    
    /// <summary>
    /// Invokes the <see cref="SnakeGameOver"/> event.
    /// </summary>
    /// <param name="args">The event's arguments.</param>
    public static void OnSnakeGameOver(PlayerSnakeGameOverEventArgs args)
        => SnakeGameOver.InvokeEvent(args);
    
    /// <summary>
    /// Invokes the <see cref="SnakeStarted"/> event.
    /// </summary>
    /// <param name="args">The event's arguments.</param>
    public static void OnSnakeStarted(PlayerSnakeStartedEventArgs args)
        => SnakeStarted.InvokeEvent(args);
    
    /// <summary>
    /// Invokes the <see cref="SnakeStopped"/> event.
    /// </summary>
    /// <param name="args">The event's arguments.</param>
    public static void OnSnakeStopped(PlayerSnakeStoppedEventArgs args)
        => SnakeStopped.InvokeEvent(args);
    
    /// <summary>
    /// Invokes the <see cref="SnakeEaten"/> event.
    /// </summary>
    /// <param name="args">The event's arguments.</param>
    public static void OnSnakeEaten(PlayerSnakeEatenEventArgs args)
        => SnakeEaten.InvokeEvent(args);
    
    /// <summary>
    /// Invokes the <see cref="SnakeMoved"/> event.
    /// </summary>
    /// <param name="args">The event's arguments.</param>
    public static void OnSnakeMoved(PlayerSnakeMovedEventArgs args)
        => SnakeMoved.InvokeEvent(args);
    #endregion
}