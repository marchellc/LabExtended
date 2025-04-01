using System.Reflection;
using System.Reflection.Emit;

using HarmonyLib;

using LabExtended.Commands.Utilities;
using LabExtended.Extensions;

using RemoteAdmin;

namespace LabExtended.Patches.Functions.RemoteAdmin;

using Commands;

/// <summary>
/// Used to insert commands from <see cref="CommandManager.Commands"/> into the Remote Admin panel.
/// </summary>
public static class RemoteAdminCommandListPatch
{
    [HarmonyPatch(typeof(QueryProcessor), nameof(QueryProcessor.ParseCommandsToStruct))]
    public static IEnumerable<CodeInstruction> Transpiler(ILGenerator generator, 
        IEnumerable<CodeInstruction> instructions, MethodBase original)
    {
        return generator.RunTranspiler(instructions, original, ctx =>
        {
            ctx.FindIndex(OpCodes.Newobj, AccessTools.Constructor(typeof(List<QueryProcessor.CommandData>)));
            
            ctx.LoadFromLocal(0);
            ctx.Call(typeof(CommandListSynchronization), nameof(CommandListSynchronization.AddCommands));
        });
    }
}