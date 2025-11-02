using LabApi.Features.Enums;
using LabApi.Events.Arguments.ServerEvents;

using NorthwoodLib.Pools;

using LabExtended.Commands.Parameters;

using RemoteAdmin;

namespace LabExtended.Commands.Utilities;

using Extensions;

/// <summary>
/// Used to format command responses.
/// </summary>
internal static class CommandResponseFormatter
{
    internal static void WriteError(this CommandExecutingEventArgs args, string response, string? color = null)
    {
        if (args.CommandType is CommandType.Client)
        {
            if (args.Sender is PlayerCommandSender sender)
            {
                sender.ReferenceHub.gameConsoleTransmission.SendToClient(response, color ?? "magenta");
            }
        }
        else
        {
            args.Sender.Respond(response, false);
        }
    }
    
    internal static bool WriteResponse(this CommandContext ctx, out ContinuableCommandBase continuableCommand)
    {
        if (ctx.Response != null)
        {
            if (ctx.Type is CommandType.Console or CommandType.RemoteAdmin)
            {
                ctx.Sender.SendRemoteAdminMessage(ctx.FormatCommandResponse(), ctx.Response is { IsSuccess: true }, true, ctx.Command.Name);
            }
            else
            {
                ctx.Sender.SendConsoleMessage(ctx.FormatCommandResponse(), ctx.Response is { IsSuccess: true } ? "green" : "red");
            }

            if (ctx.Response.IsContinued)
            {
                continuableCommand = ctx.Instance as ContinuableCommandBase;
                return continuableCommand != null;
            }
        }

        continuableCommand = null;
        return false;
    }

    internal static string FormatLikelyCommands(List<CommandData> likelyCommands, string query)
    {
        return StringBuilderPool.Shared.BuildString(x =>
        {
            x.AppendLine($"Unable to find a command matching your query (\"{query.Trim()}\"), did you perhaps mean one of these?");
            x.AppendLine();
            
            for (var i = 0; i < likelyCommands.Count; i++)
                x.Append(likelyCommands[i].GetString(false));
        });
    }
    
    internal static string FormatCommandResponse(this CommandContext ctx)
    {
        return StringBuilderPool.Shared.BuildString(x =>
        {
            if (ctx.Type is CommandType.Client)
            {
                x.Append("[");
                x.Append(ctx.Command.Name);
                x.Append("] ");
            }

            x.AppendLine(ctx.Response.Content);
        });
    }

    internal static string FormatExceptionResponse(this CommandContext ctx, Exception ex)
    {
        return StringBuilderPool.Shared.BuildString(x =>
        {
            if (ctx.Type is CommandType.Client)
            {
                x.Append("[");
                x.Append(ctx.Command.Name);
                x.Append("] ");
            }

            x.AppendLine("Failed while invoking command: ");
            x.AppendLine(ex.Message);
        });
    }

    internal static string FormatMissingPermissionsFailure(string requiredPermission, string commandName, CommandType type)
    {
        return StringBuilderPool.Shared.BuildString(x =>
        {
            if (type is CommandType.Console)
            {
                x.Append("[");
                x.Append(commandName);
                x.Append("] ");
            }

            x.Append("You are missing the required \"");
            x.Append(requiredPermission);
            x.AppendLine("\" permission to execute this command.");
        });
    }

    internal static string FormatUnknownOverloadFailure(CommandData commandData)
    {
        return StringBuilderPool.Shared.BuildString(x =>
        {
            x.AppendLine("Unknown overload, try using one of these:");
            x.AppendLine(commandData.GetString(false));
        });
    }

    internal static string FormatMissingArgumentsFailure(this CommandContext ctx)
    {
        return StringBuilderPool.Shared.BuildString(x =>
        {
            if (ctx.Type is CommandType.Client)
            {
                x.Append("[");
                x.Append(ctx.Command.Name);
                x.Append("] ");
            }

            x.Append("Missing required command arguments!");

            for (var i = 0; i < ctx.Overload.ParameterCount; i++)
            {
                var parameter = ctx.Overload.Parameters[i];

                x.AppendLine();

                x.Append("[");
                x.Append(i);
                x.Append("] ");
                x.Append(parameter.Name);
                x.Append(" (");
                x.Append(parameter.Description);
                x.Append(")");

                if (parameter.FriendlyAlias?.Length > 0)
                {
                    x.Append(" (");
                    x.Append(parameter.FriendlyAlias);
                    x.Append(")");
                }
                else
                {
                    x.Append(" (");
                    x.Append((parameter.Type.NullableType ?? parameter.Type.Type).Name);
                    x.Append(")");
                }
            }
        });
    }

    internal static string FormatInvalidArgumentsFailure(this CommandContext ctx, List<CommandParameterParserResult> results)
    {
        return StringBuilderPool.Shared.BuildString(x =>
        {
            if (ctx.Type is CommandType.Client)
            {
                x.Append("[");
                x.Append(ctx.Command.Name);
                x.Append("] ");
            }

            x.Append("Failed while parsing command arguments!");

            for (var i = 0; i < results.Count; i++)
            {
                var result = results[i];

                x.AppendLine();

                x.Append("[");
                x.Append(i);
                x.Append("] ");
                x.Append(result.Parameter?.Name ?? "null");
                x.Append(":");

                if (result.Success)
                {
                    x.Append(" OK");
                }
                else
                {
                    x.Append(" ");
                    x.Append(result.Error);

                    if (result.Parameter?.FriendlyAlias?.Length > 0)
                    {
                        x.Append(" (");
                        x.Append(result.Parameter.FriendlyAlias);
                        x.Append(")");
                    }
                    else
                    {
                        x.Append(" (");
                        x.Append((result.Parameter.Type.NullableType ?? result.Parameter.Type.Type)?.Name ?? "null");
                        x.Append(")");
                    }
                }
            }
        });
    }

    internal static string FormatTokenParserFailure(this CommandContext ctx)
    {
        return StringBuilderPool.Shared.BuildString(x =>
        {
            if (ctx.Type is CommandType.Client)
            {
                x.Append("[");
                x.Append(ctx.Command.Name);
                x.Append("] ");
            }

            x.Append("Failed while parsing command line tokens!");
        });
    }
}