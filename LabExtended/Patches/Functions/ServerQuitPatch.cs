using System.Reflection;
using System.Reflection.Emit;

using HarmonyLib;

using LabExtended.Events;
using LabExtended.Extensions;

namespace LabExtended.Patches.Functions;

/// <summary>
/// Patches the <see cref="ExServerEvents.Quitting"/> event.
/// </summary>
public static class ServerQuitPatch
{
    [HarmonyPatch(typeof(ServerConsole), nameof(ServerConsole.OnApplicationQuit))]
    public static IEnumerable<CodeInstruction> Transpiler(ILGenerator generator, 
        IEnumerable<CodeInstruction> instructions,
        MethodBase originalMethod)
    {
        return generator.RunTranspiler(instructions, originalMethod, ctx =>
        {
            ctx.Index = 0;
            ctx.Call(typeof(ExServerEvents), nameof(ExServerEvents.OnQuitting));
        });
    }
}