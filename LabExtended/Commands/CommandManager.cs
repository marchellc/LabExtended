using LabApi.Events.Arguments.ServerEvents;
using LabApi.Features.Enums;
using LabExtended.API;
using LabExtended.Core;

#pragma warning disable CS8602 // Dereference of a possibly null reference.

namespace LabExtended.Commands;

/// <summary>
/// Manages in-game commands.
/// </summary>
public static class CommandManager
{
    /// <summary>
    /// Gets a list of all registered commands.
    /// </summary>
    public static List<CommandInstance> Commands { get; } = new(byte.MaxValue);

    // Handles custom command execution.
    private static void OnCommand(CommandExecutingEventArgs args)
    {
        if (args.CommandFound && !ApiLoader.ApiConfig.CommandSection.AllowOverride)
            return;
    }
}