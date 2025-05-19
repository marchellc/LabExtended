using System.Reflection;

using CommandSystem;

using HarmonyLib;

using LabApi.Features.Console;
using LabApi.Loader;

using LabExtended.Attributes;
using LabExtended.Extensions;
using LabExtended.Core;

using static LabApi.Loader.CommandLoader;

namespace LabExtended.Patches.Fixes;

/// <summary>
/// Fixes an issue in LabAPI's CommandManager which prevents the registration of multiple parent commands.
/// </summary>
public static class LabApiParentCommandFix
{
    private static bool Prefix(Type commandType, Type commandHandlerType, out ICommand? command, string logName, ref bool __result)
    {
        command = default;
        
        if (!CommandHandlers.TryGetValue(commandHandlerType, out var commandHandler))
        {
            Logger.Error($"{LoggerPrefix} Unable to register command '{commandType.Name}' from '{logName}'. CommandHandler '{commandHandlerType}' not found.");
            return __result = false;
        }

        try
        {
            if (Activator.CreateInstance(commandType) is not ICommand cmd)
            {
                Logger.Error($"{LoggerPrefix} Unable to register command '{commandType.Name}' from '{logName}'. Couldn't create an instance of the command.");
                return __result = false;
            }
            
            command = cmd;
        }
        catch (Exception e)
        {
            Logger.Error($"{LoggerPrefix} Unable to register command '{commandType.Name}' from '{logName}'. Couldn't create an instance of the command.");
            Logger.Error(e);

            return __result = false;
        }
        
        __result = TryRegisterCommand(command, commandHandler, logName);
        return false;
    }

    [LoaderInitialize(-1)]
    private static void OnInit()
    {
        MethodInfo method = null;

        foreach (var other in typeof(CommandLoader).GetAllMethods())
        {
            if (!other.IsStatic) continue;
            if (other.Name != "TryRegisterCommand") continue;

            var parameters = other.GetAllParameters();
            
            if (parameters.Length < 2) continue;
            if (parameters[0].ParameterType != typeof(Type) || parameters[1].ParameterType != typeof(Type)) continue;
            
            method = other;
            break;
        }

        if (method is null)
        {
            ApiLog.Error("LabApiParentCommandFix", $"Failed to find target method");
            return;
        }

        ApiPatcher.Harmony.Patch(method, new HarmonyMethod(typeof(LabApiParentCommandFix).Method("Prefix")));
    }
}