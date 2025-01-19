using LabExtended.API;
using LabExtended.Commands.Interfaces;
using LabExtended.Extensions;

namespace LabExtended.Commands;

public static class CommandPropertyParser
{
    public static bool TryParse(ExPlayer sender, ICommandParser parser, string arg, out object value)
    {
        value = null;

        if (parser?.PlayerProperties is null || parser.PlayerProperties.Count == 0)
            return false;

        ExPlayer target = null;

        if (arg.StartsWith("$me."))
        {
            target = sender;
        }
        else if (arg.StartsWith("$ply."))
        {
            var text = arg.GetAfter(':');

            if (!ExPlayer.TryGet(text, out target))
                return false;

            arg = text.GetBefore(':');
        }
        else
        {
            return false;
        }

        if (target is null)
            return false;

        arg = arg.Replace("$ply.", "")
                 .Replace("$me.", "");
        
        if (!parser.PlayerProperties.TryGetValue(arg.ToLower(), out var getter))
            return false;

        value = getter(target);
        return true;
    }
}