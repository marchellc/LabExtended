using LabExtended.Commands.Parameters;

using NorthwoodLib.Pools;

using RemoteAdmin;

namespace LabExtended.Commands.Utilities;

/// <summary>
/// Used to synchronize commands with the client.
/// </summary>
public static class CommandListSynchronization
{
    /// <summary>
    /// Adds all custom commands into the specified command data list.
    /// </summary>
    /// <param name="commands">The data list.</param>
    public static void AddCommands(List<QueryProcessor.CommandData> commands)
    {
        if (commands is null)
            throw new ArgumentNullException(nameof(commands));

        if (CommandManager.Commands.Count < 1)
            return;
        
        QueryProcessor.CommandData ToData(CommandData command, CommandOverload? ov, string name, string? aliasOf = null)
        {
            var data = new QueryProcessor.CommandData();

            data.Command = name;
                
            data.Hidden = command.IsHidden;
            data.Description = command.Description;

            data.Usage = ov?.Parameters?.CompileParameters() ?? Array.Empty<string>();
            
            if (aliasOf != null)
                data.AliasOf = aliasOf;
            
            return data;
        }
        
        CommandManager.Commands.ForEach(cmd =>
        {
            commands.Add(ToData(cmd, cmd.DefaultOverload, cmd.Name));

            foreach (var overload in cmd.Overloads)
                commands.Add(ToData(cmd, overload.Value, $"{cmd.Name} {overload.Key}"));

            if (cmd.Aliases.Count > 0)
            {
                var aliasSwapBuffer = ListPool<string>.Shared.Rent(cmd.Path);

                foreach (var alias in cmd.Aliases)
                {
                    commands.Add(ToData(cmd, cmd.DefaultOverload, alias, cmd.Name));

                    aliasSwapBuffer[aliasSwapBuffer.Count - 1] = alias;

                    foreach (var overload in cmd.Overloads)
                        commands.Add(ToData(cmd, overload.Value, $"{string.Join(" ", aliasSwapBuffer)} {overload.Key}", $"{cmd.Name} {overload.Key}"));
                }
                
                ListPool<string>.Shared.Return(aliasSwapBuffer);
            }
        });
    }
}