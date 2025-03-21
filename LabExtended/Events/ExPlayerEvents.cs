using LabExtended.API;
using LabExtended.Extensions;
using LabExtended.Events.Player;

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
    /// Gets called when a player finishes authentification.
    /// </summary>
    public static event Action<ExPlayer>? Verified; 

    /// <summary>
    /// Gets called after the player's object is destroyed.
    /// </summary>
    public static event Action<ExPlayer>? Left; 
    
    /// <inheritdoc cref="PlayerLeavingEventArgs"/>
    public static event Action<PlayerLeavingEventArgs>? Leaving;
    #endregion

    #region Remote Admin
    /// <inheritdoc cref="PlayerOpenedRemoteAdminEventArgs"/>
    public static event Action<PlayerOpenedRemoteAdminEventArgs>? OpenedRemoteAdmin; 
    
    /// <inheritdoc cref="PlayerClosedRemoteAdminEventArgs"/>
    public static event Action<PlayerClosedRemoteAdminEventArgs>? ClosedRemoteAdmin; 
    
    /// <inheritdoc cref="PlayerReceivingRemoteAdminRequest"/>
    public static event Action<PlayerReceivingRemoteAdminRequest>? ReceivingRemoteAdminRequest; 
    
    /// <inheritdoc cref="PlayerSendingStaffChatMessageEventArgs"/>
    public static event Action<PlayerSendingStaffChatMessageEventArgs>? SendingStaffChatMessage; 
    
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
    #endregion
    
    #region Items
    /// <inheritdoc cref="PlayerDroppingCandyEventArgs"/>
    public static event Action<PlayerDroppingCandyEventArgs>? DroppingCandy;
    
    /// <inheritdoc cref="PlayerReceivingCandyEventArgs"/>
    public static event Action<PlayerReceivingCandyEventArgs>? ReceivingCandy; 
    
    /// <inheritdoc cref="PlayerDroppingItemEventArgs"/>
    public static event Action<PlayerDroppingItemEventArgs>? DroppingItem;
    
    /// <inheritdoc cref="PlayerThrowingItemEventArgs"/>
    public static event Action<PlayerThrowingItemEventArgs>? ThrowingItem; 
    
    /// <inheritdoc cref="PlayerSelectingItemEventArgs"/>
    public static event Action<PlayerSelectingItemEventArgs>? SelectingItem; 
    
    /// <inheritdoc cref="PlayerSelectedItemEventArgs"/>
    public static event Action<PlayerSelectedItemEventArgs>? SelectedItem; 
    #endregion

    #region Handlers - Join & Leave
    /// <summary>
    /// Invokes the <see cref="Joined"/> event.
    /// </summary>
    /// <param name="player">Player who joined.</param>
    public static void OnJoined(ExPlayer player)
        => Joined.InvokeSafe(player);
    
    /// <summary>
    /// Invokes the <see cref="Verified"/> event.
    /// </summary>
    /// <param name="player">The player who just verified.</param>
    public static void OnVerified(ExPlayer player)
        => Verified.InvokeSafe(player);
    
    /// <summary>
    /// Invokes the <see cref="Left"/> event.
    /// </summary>
    /// <param name="player">Player who left.</param>
    public static void OnLeft(ExPlayer player)
        => Left.InvokeSafe(player);

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
    /// Invokes the <see cref="ReceivingRemoteAdminRequest"/> event.
    /// </summary>
    /// <param name="args">The event's arguments.</param>
    /// <returns>The event's <see cref="BooleanEventArgs.IsAllowed"/> property.</returns>
    public static bool OnReceivingRemoteAdminRequest(PlayerReceivingRemoteAdminRequest args)
        => ReceivingRemoteAdminRequest.InvokeBooleanEvent(args);
    
    /// <summary>
    /// Invokes the <see cref="SendingStaffChatMessage"/> event.
    /// </summary>
    /// <param name="args">The event's arguments.</param>
    /// <returns>The event's <see cref="BooleanEventArgs.IsAllowed"/> property.</returns>
    public static bool OnSendingStaffChatMessage(PlayerSendingStaffChatMessageEventArgs args)
        => SendingStaffChatMessage.InvokeBooleanEvent(args);
    
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
    /// Invokes the <see cref="DroppingItem"/> event.
    /// </summary>
    /// <param name="args">The event's arguments.</param>
    /// <returns>The event's <see cref="BooleanEventArgs.IsAllowed"/> property.</returns>
    public static bool OnDroppingItem(PlayerDroppingItemEventArgs args)
        => DroppingItem.InvokeBooleanEvent(args);
    
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
    #endregion
}