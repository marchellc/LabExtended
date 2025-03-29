namespace LabExtended.Commands.Interfaces;

/// <summary>
/// Allows the command to be executed in the Remote Admin panel and the server console.
/// <remarks>Not to be confused with <see cref="IServerConsoleCommand"/> which allows command usage only in the
/// server console!</remarks>
/// </summary>
public interface IServerSideCommand : IRemoteAdminCommand, IServerConsoleCommand { }