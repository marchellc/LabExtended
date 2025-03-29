using LabExtended.API;

using LabExtended.Commands.Attributes;
using LabExtended.Commands.Tokens;

using LabExtended.Core;
using LabExtended.Utilities;
using LabExtended.Attributes;
using LabExtended.Extensions;

#pragma warning disable CS8602 // Dereference of a possibly null reference.

namespace LabExtended.Commands.Utilities;

/// <summary>
/// Utility methods targeting <see cref="PropertyToken"/>
/// </summary>
public static class CommandPropertyUtils
{
    /// <summary>
    /// Used to convert objects.
    /// </summary>
    /// <typeparam name="T">The type to convert to.</typeparam>
    public delegate bool TryConvertDelegate<T>(object source, out T? result);
    
    /// <summary>
    /// Prefix used for player getters.
    /// </summary>
    public const string GetPlayerPrefix = "getPlayer(";

    /// <summary>
    /// Prefix used for sender player replacement.
    /// </summary>
    public const string SenderPlayerPrefix = "me.";
    
    /// <summary>
    /// Gets all registered properties.
    /// </summary>
    public static Dictionary<string, Func<string?, ExPlayer?, CommandContext, object>> Properties { get; } = new();

    /// <summary>
    /// Registers a new property getter.
    /// </summary>
    /// <param name="propertyKey">The property key.</param>
    /// <param name="getter">The property value getter.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void Register(string propertyKey, Func<string?, ExPlayer?, CommandContext, object> getter)
    {
        if (string.IsNullOrWhiteSpace(propertyKey))
            throw new ArgumentNullException(nameof(propertyKey));
        
        if (getter is null)
            throw new ArgumentNullException(nameof(getter));
        
        if (!Properties.ContainsKey(propertyKey))
            Properties.Add(propertyKey, getter);
    }

    /// <summary>
    /// Registers a list of getters.
    /// </summary>
    /// <param name="propertyKey"></param>
    /// <param name="getters"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void RegisterMany(string propertyKey,
        params KeyValuePair<string, Func<ExPlayer?, CommandContext, object>>[] getters)
    {
        if (string.IsNullOrWhiteSpace(propertyKey))
            throw new ArgumentNullException(nameof(propertyKey));

        var dict = new Dictionary<string, Func<ExPlayer?, CommandContext, object>>();
        
        dict.AddRange(getters);

        for (var i = 0; i < getters.Length; i++)
        {
            Properties.Add(propertyKey, (key, ply, ctx) =>
            {
                if (!dict.TryGetValue(key, out var getter))
                    return null;

                return getter(ply, ctx);
            });
        }
    }

    /// <summary>
    /// Attempts to retrieve a property.
    /// </summary>
    /// <param name="propertyToken">The property token.</param>
    /// <param name="context">The current command context.</param>
    /// <param name="convertor">Delegate used to convert result.</param>
    /// <param name="result">The token result.</param>
    /// <typeparam name="T">The type of expected value.</typeparam>
    /// <returns>true if the value was retrieved</returns>
    public static bool TryGet<T>(this PropertyToken propertyToken, CommandContext context, TryConvertDelegate<T>? convertor, out T? result)
    {
        if (propertyToken is null)
            throw new ArgumentNullException(nameof(propertyToken));

        if (context is null)
            throw new ArgumentNullException(nameof(context));

        result = default;

        convertor ??= (object obj, out T? val) =>
        {
            val = default;
            
            if (obj is not T castVal)
                return false;

            val = castVal;
            return true;
        };

        if (propertyToken.Value != null)
            return convertor(propertyToken.Value, out result);

        if (propertyToken.Name?.Length < 1)
            return false;

        var tokenValue = propertyToken.Name;
        
        if (TryGetPlayer(ref tokenValue, context, out var player))
        {
            propertyToken.Name = tokenValue;
            
            if (propertyToken.Name.TrySplit('.', true, 2, out var parts))
            {
                if (!Properties.TryGetValue(parts[0], out var propertyGetter))
                    return false;
                
                var propertyValue = propertyGetter(parts[1], player, context);

                if (!convertor(propertyValue, out result))
                    return false;

                propertyToken.Value = result;
                return true;
            }
            else
            {
                if (!Properties.TryGetValue(propertyToken.Name, out var propertyGetter))
                    return false;
                
                var propertyValue = propertyGetter(null, player, context);

                if (!convertor(propertyValue, out result))
                    return false;

                propertyToken.Value = result;
                return true;
            }
        }
        else
        {
            propertyToken.Name = tokenValue;
            
            if (propertyToken.Name.TrySplit('.', true, 2, out var parts))
            {        
                if (!Properties.TryGetValue(parts[0], out var propertyGetter))
                    return false;
                
                var propertyValue = propertyGetter(parts[1], null, context);

                if (!convertor(propertyValue, out result))
                    return false;

                
                propertyToken.Value = result;
                return true;
            }
            else
            {
                if (!Properties.TryGetValue(propertyToken.Name, out var propertyGetter))
                    return false;
                
                var propertyValue = propertyGetter(parts[1], null, context);

                if (!convertor(propertyValue, out result))
                    return false;
                
                propertyToken.Value = result;
                return true;
            }
        }
    }

    /// <summary>
    /// Attempts to retrieve a player instance from a property token.
    /// </summary>
    /// <param name="propertyTokenValue">The target property token value.</param>
    /// <param name="context">The command's context.</param>
    /// <param name="player">The resulting player.</param>
    /// <returns>true if the player was found</returns>
    public static bool TryGetPlayer(ref string propertyTokenValue, CommandContext context, out ExPlayer? player)
    {
        if (propertyTokenValue is null)
            throw new ArgumentNullException(nameof(propertyTokenValue));
        
        if (context is null)
            throw new ArgumentNullException(nameof(context));
        
        player = null;

        if (propertyTokenValue.StartsWith(SenderPlayerPrefix))
        {
            propertyTokenValue = propertyTokenValue.Substring(SenderPlayerPrefix.Length, 
                propertyTokenValue.Length - SenderPlayerPrefix.Length);
            propertyTokenValue = string.Concat("player.", propertyTokenValue);

            player = context.Sender;
            return true;
        }

        if (propertyTokenValue.StartsWith(GetPlayerPrefix))
        {
            var openIndex = propertyTokenValue.IndexOf('(');
            var closeIndex = propertyTokenValue.IndexOf(')');

            if (openIndex != -1 && closeIndex != -1)
            {
                var inBrackets = propertyTokenValue.Substring(openIndex + 1, closeIndex - openIndex - 1);
                
                if (!ExPlayer.TryGet(inBrackets, out player))
                    return false;

                propertyTokenValue = propertyTokenValue.Substring(closeIndex, 
                    propertyTokenValue.Length - closeIndex);

                if (propertyTokenValue.StartsWith(")"))
                    propertyTokenValue = propertyTokenValue.Substring(1);

                if (propertyTokenValue.StartsWith("."))
                    propertyTokenValue = propertyTokenValue.Substring(1);

                propertyTokenValue = string.Concat("player.", propertyTokenValue);
                return true;
            }
        }

        return false;
    }

    [LoaderInitialize(1)]
    private static void OnInit()
    {
        var count = 0;
        
        foreach (var type in ApiLoader.Assembly.GetTypes())
        {
            if (!type.IsPublic)
                continue;
            
            if (!type.HasAttribute<CommandPropertyAliasAttribute>(out var aliasAttribute))
                continue;
            
            var properties = new Dictionary<string, Func<object, object[], object>>();

            foreach (var property in type.GetAllProperties())
            {
                if (!property.HasAttribute<CommandPropertyAliasAttribute>(out var propertyAliasAttribute))
                    continue;
                
                if (property.GetMethod is null || (type != typeof(ExPlayer) && !property.GetMethod.IsStatic))
                    continue;
                
                properties.Add(propertyAliasAttribute.Alias, FastReflection.ForMethod(property.GetMethod));
            }

            if (properties.Count > 0)
            {
                count += properties.Count;
                
                if (type == typeof(ExPlayer))
                {
                    Properties.Add(aliasAttribute.Alias, (key, ply, _) =>
                    {
                        if (!properties.TryGetValue(key, out var propertyGetter))
                            return null;

                        return propertyGetter(ply, Array.Empty<object>());
                    });
                }
                else
                {
                    Properties.Add(aliasAttribute.Alias, (key, _, _) =>
                    {
                        if (!properties.TryGetValue(key, out var propertyGetter))
                            return null;

                        return propertyGetter(null, Array.Empty<object>());
                    });
                }
            }
        }
    }
}