namespace LabExtended.Commands.Interfaces;

/// <summary>
/// Enables a command to be executed via the server console (LocalAdmin, MultiAdmin, etc.)
/// <remarks>Not to be confused with <see cref="IServerSideCommand"/> which allows execution in the server console AND 
/// Remote Admin panel!</remarks>
/// </summary>
public interface IServerConsoleCommand { }