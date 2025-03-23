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
        
        QueryProcessor.CommandData ToData(CommandInstance command, string name)
        {
            var data = new QueryProcessor.CommandData();

            data.Command = name;
                
            data.Hidden = command.IsHidden;
            data.Description = command.Description;

            data.Usage = command.Overload.Parameters.CompileParameters();
            return data;
        }
        
        CommandManager.Commands.ForEach(cmd =>
        {
            commands.Add(ToData(cmd, cmd.Name));

            if (cmd.Aliases.Count > 0)
            {
                var aliasParts = ListPool<string>.Shared.Rent(cmd.NameParts);

                cmd.Aliases.ForEach(alias =>
                {
                    aliasParts[aliasParts.Count - 1] = alias;
                    commands.Add(ToData(cmd, string.Join(" ", aliasParts)));
                });

                ListPool<string>.Shared.Return(aliasParts);
            }
        });
    }
}