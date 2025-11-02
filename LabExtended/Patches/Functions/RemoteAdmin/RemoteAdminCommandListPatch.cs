using CommandSystem;

using HarmonyLib;

using LabExtended.Extensions;

using NorthwoodLib.Pools;

using RemoteAdmin;

namespace LabExtended.Patches.Functions.RemoteAdmin;

using Commands;

/// <summary>
/// Used to insert commands from <see cref="CommandManager.Commands"/> into the Remote Admin panel.
/// </summary>
public static class RemoteAdminCommandListPatch
{
    [HarmonyPatch(typeof(QueryProcessor), nameof(QueryProcessor.ParseCommandsToStruct))]
    private static bool Prefix(List<ICommand> list, ref QueryProcessor.CommandData[] __result)
    {
        var commands = ListPool<QueryProcessor.CommandData>.Shared.Rent();

        foreach (var command in list)
        {
            var description = command.Description;

            if (string.IsNullOrWhiteSpace(description))
                description = null;
            else if (description.Length > QueryProcessor.CommandDescriptionSyncMaxLength)
                description = description.Substring(0, QueryProcessor.CommandDescriptionSyncMaxLength) + "...";

            var data = new QueryProcessor.CommandData();

            data.Command = command.Command;
            data.Usage = command is IUsageProvider usageProvider ? usageProvider.Usage : null;
            data.Description = description;
            data.AliasOf = null;
            data.Hidden = command is IHiddenCommand;
            
            commands.Add(data);

            if (command.Aliases?.Length > 0)
            {
                for (var i = 0; i < command.Aliases.Length; i++)
                {
                    var alias = command.Aliases[i];
                    var aliasData = new QueryProcessor.CommandData();

                    aliasData.Command = alias;
                    aliasData.Usage = data.Usage;
                    aliasData.Description = data.Description;
                    aliasData.AliasOf = command.Command;
                    aliasData.Hidden = data.Hidden;
                    
                    commands.Add(aliasData);
                }
            }
        }
        
        foreach (var command in CommandManager.Commands)
        {
            Parse(commands, command, command.Path[0]);

            if (command.Aliases.Count > 0)
            {
                foreach (var alias in command.Aliases)
                {
                    Parse(commands, command, alias);
                }
            }
        }

        __result = ListPool<QueryProcessor.CommandData>.Shared.ToArrayReturn(commands);
        return false;
    }

    private static void Parse(List<QueryProcessor.CommandData> commands, CommandData command, string commandRoot)
    {
        if (command.Path.Count > 1)
        {
            for (var i = 1; i < command.Path.Count; i++)
            {
                commandRoot += $"_{command.Path[i]}";
            }
        }
        
        if (command.DefaultOverload != null)
            commands.Add(Parse(command, command.DefaultOverload, commandRoot));
        
        foreach (var overload in command.Overloads)
            commands.Add(Parse(command, overload, commandRoot));
    }

    private static QueryProcessor.CommandData Parse(CommandData command, CommandOverload overload, string commandRoot)
    {
        var data = new QueryProcessor.CommandData();

        data.Hidden = command.IsHidden;
        
        if (command.DefaultOverload != null && overload == command.DefaultOverload)
        {
            data.Command = commandRoot;
            data.Description = command.Description;
        }
        else
        {
            data.Command = string.Concat(commandRoot, "_", overload.Name.Replace(' ', '_'));
            data.Description = string.IsNullOrWhiteSpace(overload.Description)
                ? command.Description
                : overload.Description;
        }

        var usage = new string[overload.ParameterCount];

        for (var i = 0; i < overload.ParameterCount; i++)
        {
            var parameter = overload.Parameters[i];

            var prefix = "[";
            var postfix = "]";
            var name = parameter.Name;

            if (parameter.HasDefault)
            {
                prefix = "(";
                postfix = ")";
            }

            if (parameter.UsageAlias?.Length > 0)
            {
                name = parameter.UsageAlias;

                prefix = string.Empty;
                postfix = string.Empty;
            }
            else if (parameter.FriendlyAlias?.Length > 0)
            {
                name = parameter.FriendlyAlias;
            }

            usage[i] = string.Concat(prefix, name, postfix);
        }

        data.Usage = usage;
        return data;
    }
}