namespace LabExtended.Commands.Interfaces;

/// <summary>
/// Allows a command to be executed in all consoles (Remote Admin, server & player).
/// </summary>
public interface IAllCommand : IPlayerCommand, IRemoteAdminCommand, IServerCommand { }