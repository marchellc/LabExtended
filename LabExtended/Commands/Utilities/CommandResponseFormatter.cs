using NorthwoodLib.Pools;

using LabApi.Features.Enums;
using LabExtended.Commands.Parameters;

namespace LabExtended.Commands.Utilities;

using Tokens.Parsing;

using Contexts;
using Extensions;

/// <summary>
/// Used to format command responses.
/// </summary>
internal static class CommandResponseFormatter
{
    internal static bool WriteResponse(this CommandContext ctx, out ContinuableCommandBase continuableCommand)
    {
        if (ctx.Response.HasValue)
        {
            if (ctx.Type is CommandType.Console or CommandType.RemoteAdmin)
            {
                ctx.Sender.SendRemoteAdminMessage(ctx.FormatCommandResponse(), ctx.Response is { IsSuccess: true }, true, ctx.Command.Name);
            }
            else
            {
                ctx.Sender.SendConsoleMessage(ctx.FormatCommandResponse(), ctx.Response is { IsSuccess: true } ? "green" : "red");
            }

            if (ctx.Response.Value.IsContinuted)
            {
                continuableCommand = ctx.Instance as ContinuableCommandBase;
                return continuableCommand != null;
            }
        }

        continuableCommand = null;
        return false;
    }
    
    internal static string FormatCommandResponse(this CommandContext ctx)
    {
        return StringBuilderPool.Shared.BuildString(x =>
        {
            if (ctx.Type is CommandType.Client)
            {
                x.Append($"[");
                x.Append(ctx.Command.Name);
                x.Append("] ");
            }

            x.AppendLine(ctx.Response.Value.Content);
        });
    }

    internal static string FormatExceptionResponse(this CommandContext ctx, Exception ex)
    {
        return StringBuilderPool.Shared.BuildString(x =>
        {
            if (ctx.Type is CommandType.Client)
            {
                x.Append($"[");
                x.Append(ctx.Command.Name);
                x.Append("] ");
            }

            x.AppendLine("Failed while invoking command: ");
            x.AppendLine(ex.Message);
        });
    }

    internal static string FormatNoOverloadsFailure(this CommandContext ctx)
    {
        return StringBuilderPool.Shared.BuildString(x =>
        {
            if (ctx.Type is CommandType.Client)
            {
                x.Append($"[");
                x.Append(ctx.Command.Name);
                x.Append("] ");
            }

            x.AppendLine("Failed while searching for compatible command overloads.");
            x.AppendLine($"Use \"help {ctx.Command.Name}\" to get a list of command overloads.");
        });
    }

    internal static string FormatMissingPermissionsFailure(string requiredPermission, string commandName, CommandType type)
    {
        return StringBuilderPool.Shared.BuildString(x =>
        {
            if (type is CommandType.Console)
            {
                x.Append($"[");
                x.Append(commandName);
                x.Append("] ");
            }

            if (type is CommandType.RemoteAdmin)
            {
                x.Append("You are missing the required <color=red>");
                x.Append(requiredPermission);
                x.AppendLine("</color> permission to execute this command.");
            }
            else
            {
                x.Append("You are missing the required ");
                x.Append(requiredPermission);
                x.AppendLine(" permission to execute this command.");
            }
        });
    }

    internal static string FormatMissingArgumentsFailure(this CommandContext ctx)
    {
        return StringBuilderPool.Shared.BuildString(x =>
        {
            if (ctx.Type is CommandType.Client)
            {
                x.Append($"[");
                x.Append(ctx.Command.Name);
                x.Append("] ");
            }

            x.Append("Missing required command arguments!");

            for (var i = 0; i < ctx.Overload.ParameterCount; i++)
            {
                var parameter = ctx.Overload.Parameters[i];

                x.AppendLine();

                if (ctx.Type is CommandType.RemoteAdmin)
                {
                    x.Append("<color=yellow>");
                    x.Append("[");
                    x.Append(i);
                    x.Append("] ");
                    x.Append("</color><color=red>");
                    x.Append(parameter.Name);
                    x.Append("</color>");
                    x.Append(" <i>(");
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
                        x.Append(parameter.Type.Type.Name);
                        x.Append(")");
                    }

                    x.Append("</i>");
                }
                else
                {
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
                        x.Append(parameter.Type.Type.Name);
                        x.Append(")");
                    }
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

                if (ctx.Type is CommandType.RemoteAdmin)
                {
                    x.Append("<color=red>");
                    x.Append("[");
                    x.Append(i);
                    x.Append("]</color> <color=yellow>");
                    x.Append(result.Parameter?.Name ?? "null");
                    x.Append("</color>:");

                    if (result.Success)
                    {
                        x.Append(" <color=green>OK</color>");
                    }
                    else
                    {
                        x.Append(" <color=red>");
                        x.Append(result.Error);
                        x.Append("</color>");
                        
                        if (result.Parameter?.FriendlyAlias?.Length > 0)
                        {
                            x.Append(" <i>(");
                            x.Append(result.Parameter.FriendlyAlias);
                            x.Append(")</i>");
                        }
                        else
                        {
                            x.Append(" <i>(");
                            x.Append(result.Parameter?.Type?.Type?.Name ?? "null");
                            x.Append(")</i>");
                        }
                    }
                }
                else
                {
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
                            x.Append(result.Parameter?.Type?.Type?.Name ?? "null");
                            x.Append(")");
                        }
                    }
                }
            }
        });
    }

    internal static string FormatTokenParserFailure(this CommandContext ctx, CommandTokenParserResult result)
    {
        return StringBuilderPool.Shared.BuildString(x =>
        {
            if (ctx.Type is CommandType.Client)
            {
                x.Append($"[");
                x.Append(ctx.Command.Name);
                x.Append("] ");
            }

            x.Append("Failed while parsing command arguments at position ");
            x.Append(result.Position.Value);
            x.Append(", faulty character: ");
            x.Append(result.Character.Value);
                
            x.AppendLine();
            x.AppendLine();
            
            if (ctx.Type is CommandType.RemoteAdmin)
            {
                x.Append("<color=green>");

                for (var i = 0; i < result.Position.Value; i++)
                    x.Append(result.Input[i]);
                
                x.Append("</color>");
                
                x.Append("<color=red>");
                x.Append(result.Character.Value);
                x.Append("</color>");

                x.Append("<color=green>");
                
                for (var i = result.Position.Value; i < result.Input.Length; i++)
                    x.Append(result.Input[i]);
                
                x.AppendLine("</color>");
                
                x.Append("<color=red>");

                for (var i = 0; i < result.Position.Value; i++)
                    x.Append(" ");

                x.Append("^^</color>");
            }
            else
            {
                x.AppendLine(result.Input);
                
                for (var i = 0; i < result.Position.Value; i++)
                    x.Append(" ");
                
                x.Append("^^");
            }
            
        });
    }
}