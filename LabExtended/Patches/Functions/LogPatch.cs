using System.Reflection;
using System.Reflection.Emit;

using HarmonyLib;

using LabExtended.Core;
using LabExtended.Events;
using LabExtended.Attributes;
using LabExtended.Extensions;

namespace LabExtended.Patches.Functions
{
    [HarmonyPatch(typeof(ServerConsole), nameof(ServerConsole.AddLog))]
    public static class LogPatch
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(ILGenerator generator, IEnumerable<CodeInstruction> instructions,
            MethodBase originalMethod)
        {
            return generator.RunTranspiler(instructions, originalMethod, ctx =>
            {
                ctx.FindIndex(OpCodes.Call,
                    AccessTools.Method(typeof(ServerConsole), nameof(ServerConsole.PrintOnOutputs)));
                
                ctx.LoadZeroArgument();
                ctx.Call(typeof(ExServerEvents), nameof(ExServerEvents.OnLogging), false);
            });
        }

        [LoaderInitialize(-1)]
        private static void OnInit() => ApiPatcher.Harmony.CreateClassProcessor(typeof(LogPatch)).Patch();
    }
}