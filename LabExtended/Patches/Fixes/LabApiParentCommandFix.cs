using CommandSystem;

using HarmonyLib;

using LabApi.Features.Console;
using LabApi.Loader;

using LabExtended.Attributes;
using LabExtended.Core;

using static LabApi.Loader.CommandLoader;

namespace LabExtended.Patches.Fixes;

[HarmonyPatch(typeof(CommandLoader), nameof(TryRegisterCommand), typeof(Type), typeof(Type), typeof(ICommand), typeof(string))]
public static class LabApiParentCommandFix
{
    public static bool Prefix(Type commandType, Type commandHandlerType, out ICommand? command, string logName, ref bool __result)
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
    private static void OnInit() => ApiPatcher.Harmony.CreateClassProcessor(typeof(LabApiParentCommandFix)).Patch();
}