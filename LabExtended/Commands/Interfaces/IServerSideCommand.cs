namespace LabExtended.Commands.Interfaces;

/// <summary>
/// Allows the command to be executed in the Remote Admin panel and the server console.
/// </summary>
public interface IServerSideCommand : IRemoteAdminCommand, IServerCommand { }