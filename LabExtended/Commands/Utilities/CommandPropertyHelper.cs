using LabExtended.Commands.Interfaces;
using LabExtended.Commands.Contexts;
using LabExtended.Commands.Tokens;

namespace LabExtended.Commands.Utilities;

/// <summary>
/// Helps with property tokens.
/// </summary>
public static class CommandPropertyHelper
{
    /// <summary>
    /// Processes a property token.
    /// </summary>
    /// <param name="token">The token to process.</param>
    /// <param name="context">The command's context.</param>
    /// <param name="result">The resulting string.</param>
    /// <returns>true if the PropertyToken was converted</returns>
    public static bool TryProcessProperty(this ICommandToken token, CommandContext context, out object result)
    {
        if (token is null)
            throw new ArgumentNullException(nameof(token));

        result = null;

        if (token is not PropertyToken propertyToken)
            return false;
        
        var propertyName = string.Concat(propertyToken.Key, ".", propertyToken.Name);

        if (!PropertyToken.Properties.TryGetValue(propertyName, out var property))
            return false;

        var propertyValue = property.Value(context);

        if (propertyValue is null)
            return false;

        result = propertyValue;
        return true;
    }
}