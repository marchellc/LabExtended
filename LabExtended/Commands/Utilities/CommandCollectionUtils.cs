using LabExtended.Commands.Tokens;

using NorthwoodLib.Pools;

namespace LabExtended.Commands.Utilities;

/// <summary>
/// Utilities targeting collection tokens.
/// </summary>
public static class CommandCollectionUtils
{
    /// <summary>
    /// Converts a string token to a <b>POOLED</b> list.
    /// </summary>
    /// <param name="stringToken"></param>
    /// <returns>The split list.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static List<string> ToListNonAlloc(this StringToken stringToken)
    {
        if (stringToken is null)
            throw new ArgumentNullException(nameof(stringToken));

        var list = ListPool<string>.Shared.Rent();
        var parts = stringToken.Value.Split(CommandManager.spaceSeparator, StringSplitOptions.RemoveEmptyEntries);

        for (var i = 0; i < parts.Length; i++)
            list.Add(parts[i].Trim(CommandManager.spaceSeparator));
        
        return list;
    }
}